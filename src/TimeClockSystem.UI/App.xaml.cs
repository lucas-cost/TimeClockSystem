using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Polly;
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
using TimeClockSystem.Infrastructure.Repositories;
using TimeClockSystem.UI.ViewModels;
using WpfApplication = System.Windows.Application;

namespace TimeClockSystem.UI
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : WpfApplication
    {
        private IHost _host;

        public App()
        {
            _host = Host.CreateDefaultBuilder()
                .ConfigureServices((context, services) =>
                {
                    ConfigureServices(services);
                })
                .Build();
        }

        private void ConfigureServices(IServiceCollection services)
        {
            var configuration = new ConfigurationBuilder()
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
                // 1. Pega a factory que acabamos de registrar
                var factory = provider.GetRequiredService<IWebcamFactory>();
                // 2. A factory tenta criar o dispositivo de captura
                IVideoCaptureWrapper? captureDevice = factory.CreateVideoCapture();
                // 3. Cria a WebcamService, passando o dispositivo (que pode ser nulo)
                return new WebcamService(captureDevice);
            });
            services.AddHttpClient<IApiClient, ApiClient>().AddPolicyHandler(GetRetryPolicy());

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
            // Política de Retry com backoff exponencial: 3 tentativas com 2, 4, 8 segundos de espera.
            return Policy
                .Handle<HttpRequestException>()
                .OrResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
                .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));
        }

        protected override async void OnStartup(StartupEventArgs e)
        {
            try
            {
                await _host.StartAsync(); //inicio da sincronização

                // Aplica as migrations do EF Core ao iniciar, se necessário
                using (var scope = _host.Services.CreateScope()) 
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

                this.Shutdown();
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
