namespace TimeClockSystem.Core.Interfaces
{
    public interface IApiHealthCheckService
    {
        event Action<bool> ConnectionStatusChanged;
        Task<bool> IsApiOnlineAsync();
        void StartMonitoring();
        void StopMonitoring();
    }
}
