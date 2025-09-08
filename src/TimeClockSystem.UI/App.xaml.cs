using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Polly;
using System.IO;
using System.Net.Http;
using TimeClockSystem.Core.Interfaces;
using TimeClockSystem.Core.Settings;
using TimeClockSystem.Infrastructure.Api;
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

            // Infrastructure
            services.AddAutoMapper(typeof(MappingProfile).Assembly);
            services.AddHttpClient<IApiClient, ApiClient>().AddPolicyHandler(GetRetryPolicy());
            // UI
            services.AddSingleton<MainWindow>();
        }

        private static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
        {
            // Política de Retry com backoff exponencial: 3 tentativas com 2, 4, 8 segundos de espera.
            return Policy
                .Handle<HttpRequestException>()
                .OrResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
                .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));
        }
        protected override async void OnExit(ExitEventArgs e)
        {
            await _host.StopAsync();
            _host.Dispose();
            base.OnExit(e);
    }
}
}
