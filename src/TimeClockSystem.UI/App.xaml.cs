using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Polly;
using Serilog;
using Serilog.Events;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Windows;
using TimeClockSystem.Application.Interfaces;
using TimeClockSystem.Application.UseCases.RegisterPoint;
using TimeClockSystem.BackgroundServices;
using TimeClockSystem.Core.Interfaces;
using TimeClockSystem.Core.Services;
using TimeClockSystem.Core.Settings;
using TimeClockSystem.Infrastructure.Api;
using TimeClockSystem.Infrastructure.Data.Context;
using TimeClockSystem.Infrastructure.Hardware;
using TimeClockSystem.Infrastructure.Hardware.Abstractions;
using TimeClockSystem.Infrastructure.Hardware.Factories;
using TimeClockSystem.Infrastructure.Menssaging;
using TimeClockSystem.Infrastructure.Repositories;
using TimeClockSystem.UI.ViewModels;
using WpfApplication = System.Windows.Application;

namespace TimeClockSystem.UI
{
    public partial class App : WpfApplication
    {
        private readonly IHost _host;

        public App()
        {
            // 1. Configura o Serilog antes de construir o Host
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug() 
                .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                .Enrich.FromLogContext()
                .WriteTo.Debug()
                .WriteTo.File(
                    path: Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "TimeClockSystem", "Logs", "log-.txt"),
                    rollingInterval: RollingInterval.Day,
                    outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
                .CreateLogger();


            _host = Host.CreateDefaultBuilder()
                .UseSerilog()
                .ConfigureServices((context, services) =>
                {
                    ConfigureServices(services);
                })
                .Build();
        }

        private void ConfigureServices(IServiceCollection services)
        {
            IConfigurationRoot configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            services.Configure<ApiSettings>(configuration.GetSection("ApiSettings"));

            // Database
            services.AddDbContext<TimeClockDbContext>(options =>
            {
                string dbPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "TimeClockSystem", "timeclock.db");
                Directory.CreateDirectory(Path.GetDirectoryName(dbPath)!);
                options.UseSqlite($"Data Source={dbPath}");
            });

            // Core
            services.AddScoped<ITimeClockService, TimeClockService>();

            // Infrastructure
            services.AddAutoMapper(typeof(MappingProfile).Assembly);
            services.AddScoped<ITimeClockRepository, TimeClockRepository>();
            services.AddSingleton<IWebcamFactory, WebcamFactory>();
            services.AddSingleton<IWebcamService>(provider =>
            {
                IWebcamFactory factory = provider.GetRequiredService<IWebcamFactory>();
                ILogger<WebcamService> logger = provider.GetRequiredService<ILogger<WebcamService>>();
                IVideoCaptureWrapper? captureDevice = factory.CreateVideoCapture();

                return new WebcamService(captureDevice, logger);
            });
            services.AddHttpClient<IApiHealthCheckService, ApiHealthCheckService>((serviceProvider, client) =>
            {
                var apiSettings = serviceProvider.GetRequiredService<IOptions<ApiSettings>>().Value;
                client.BaseAddress = new Uri(apiSettings.BaseUrl);
            });

            // Adiciona o MassTransit
            services.AddMassTransit(busConfigurator =>
            {
                busConfigurator.UsingRabbitMq((context, cfg) =>
                {
                    // Conecta ao RabbitMQ rodando no Docker em localhost
                    cfg.Host("localhost", "/", h =>
                    {
                        h.Username("guest");
                        h.Password("guest");
                    });

                    // Opcional: Configurações adicionais podem ir aqui
                });
            });
            services.AddSingleton<IEventPublisher, MassTransitEventPublisher>();

            IAsyncPolicy<HttpResponseMessage> retryPolicy = GetRetryPolicy();
            IAsyncPolicy<HttpResponseMessage> circuitBreakerPolicy = GetCircuitBreakerPolicy();

            // Combina as políticas na ordem correta, com o Circuit Breaker envolvendo o Retry.
            IAsyncPolicy<HttpResponseMessage> combinedPolicy = Policy.WrapAsync(circuitBreakerPolicy, retryPolicy);

            services.AddHttpClient<IApiClient, ApiClient>()
                .AddPolicyHandler(combinedPolicy);

            // Application
            services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(RegisterPointCommand).Assembly));

            // UI
            services.AddSingleton<MainViewModel>();
            services.AddSingleton<MainWindow>();

            // Background Services
            services.AddHostedService<SyncWorker>();
        }

        private static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
        {
            return Policy
                .Handle<HttpRequestException>()
                .OrResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
                .WaitAndRetryAsync(3, retryAttempt =>
                {
                    TimeSpan delay = TimeSpan.FromSeconds(Math.Pow(2, retryAttempt));
                    Debug.WriteLine($"RETRY: Tentativa {retryAttempt}. Aguardando {delay.TotalSeconds}s...");
                    return delay;
                });
        }

        private static IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy()
        {
            return Policy
                .Handle<HttpRequestException>()
                .OrResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
                .CircuitBreakerAsync(
                    5,
                    TimeSpan.FromSeconds(30),
                    OnBreak,
                    OnReset,
                    OnHalfOpen
                );
        }

        private static void OnBreak(DelegateResult<HttpResponseMessage> result, TimeSpan timeSpan)
        {
            Log.Warning($"CIRCUIT BREAKER: Circuito aberto por 30 segundos. Motivo: {result.Exception?.Message ?? result.Result?.ReasonPhrase}");
        }

        private static void OnReset()
        {
            Log.Information("CIRCUIT BREAKER: Circuito fechado. As chamadas voltam ao normal.");
        }

        private static void OnHalfOpen()
        {
            Log.Information("CIRCUIT BREAKER: Circuito meio-aberto. A próxima chamada será um teste.");
        }

        protected override async void OnStartup(StartupEventArgs e)
        {
            try
            {
                await _host.StartAsync();

                using (IServiceScope scope = _host.Services.CreateScope())
                {
                    TimeClockDbContext dbContext = scope.ServiceProvider.GetRequiredService<TimeClockDbContext>();
                    await dbContext.Database.MigrateAsync();
                }

                MainWindow mainWindow = _host.Services.GetRequiredService<MainWindow>();
                mainWindow.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ocorreu um erro fatal ao iniciar a aplicação. A aplicação será encerrada.\n\nDetalhes do Erro:\n{ex.ToString()}",
                                "Erro de Inicialização",
                                MessageBoxButton.OK,
                                MessageBoxImage.Error);
                Shutdown();
            }
        }

        protected override async void OnExit(ExitEventArgs e)
        {
            await _host.StopAsync();
            _host.Dispose();
            base.OnExit(e);
        }
    }
}