using TimeClockSystem.Core.Interfaces;

namespace TimeClockSystem.Infrastructure.Api
{
    public class ApiHealthCheckService : IApiHealthCheckService
    {
        private readonly HttpClient _httpClient;
        private Timer _timer;

        public event Action<bool> ConnectionStatusChanged;

        public ApiHealthCheckService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public void StartMonitoring()
        {
            _timer = new Timer(async _ => await CheckStatus(), null, TimeSpan.Zero, TimeSpan.FromMinutes(1));
        }

        private async Task CheckStatus()
        {
            bool isOnline = await IsApiOnlineAsync();
            ConnectionStatusChanged?.Invoke(isOnline);
        }

        public async Task<bool> IsApiOnlineAsync()
        {
            try
            {
                using (CancellationTokenSource cts = new CancellationTokenSource(TimeSpan.FromSeconds(5)))
                {
                    HttpResponseMessage response = await _httpClient.GetAsync("", cts.Token);

                    return response.IsSuccessStatusCode;
                }
            }
            catch
            {
                return false;
            }
        }

        public void StopMonitoring()
        {
            _timer?.Dispose();
        }
    }
}
