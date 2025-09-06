using TimeClockSystem.Core.Entities;
using TimeClockSystem.Core.Enums;
using TimeClockSystem.Core.Interfaces;

namespace TimeClockSystem.Infrastructure.Repositories
{
    public class TimeClockRepository : ITimeClockRepository
    {
        public Task AddAsync(TimeClockRecord record)
        {
            throw new NotImplementedException();
        }

        public Task<TimeClockRecord?> GetLastRecordForEmployeeAsync(string employeeId)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<TimeClockRecord>> GetPendingSyncRecordsAsync()
        {
            throw new NotImplementedException();
        }

        public Task UpdateStatusAsync(Guid recordId, SyncStatus newStatus)
        {
            throw new NotImplementedException();
        }
    }
}
