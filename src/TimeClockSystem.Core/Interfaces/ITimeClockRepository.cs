using TimeClockSystem.Core.Entities;
using TimeClockSystem.Core.Enums;

namespace TimeClockSystem.Core.Interfaces
{
    public interface ITimeClockRepository
    {
        Task AddAsync(TimeClockRecord record);
        Task<IEnumerable<TimeClockRecord>> GetPendingSyncRecordsAsync();
        Task UpdateStatusAsync(Guid recordId, SyncStatus newStatus);
        Task<List<TimeClockRecord>> GetTodaysRecordsForEmployeeAsync(string employeeId);
    }
}
