using TimeClockSystem.Core.Entities;

namespace TimeClockSystem.Core.Interfaces
{
    public interface IApiClient
    {
        Task<bool> RegisterPointAsync(TimeClockRecord record);
    }
}
