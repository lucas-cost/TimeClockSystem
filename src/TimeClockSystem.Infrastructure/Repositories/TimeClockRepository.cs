using Microsoft.EntityFrameworkCore;
using TimeClockSystem.Core.Entities;
using TimeClockSystem.Core.Enums;
using TimeClockSystem.Core.Interfaces;
using TimeClockSystem.Infrastructure.Data.Context;

namespace TimeClockSystem.Infrastructure.Repositories
{
    public class TimeClockRepository : ITimeClockRepository
    {
        private readonly TimeClockDbContext _context;

        public TimeClockRepository(TimeClockDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(TimeClockRecord record)
        {
            await _context.TimeClockRecords.AddAsync(record);
            await _context.SaveChangesAsync();
        }

        public async Task<TimeClockRecord?> GetLastRecordForEmployeeAsync(string employeeId)
        {
            return await _context.TimeClockRecords
                .Where(r => r.EmployeeId == employeeId)
                .OrderByDescending(r => r.Timestamp)
                .FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<TimeClockRecord>> GetPendingSyncRecordsAsync()
        {
            return await _context.TimeClockRecords
                .Where(r => r.Status == SyncStatus.Pending)
                .OrderBy(r => r.Timestamp)
                .ToListAsync();
        }

        public async Task UpdateStatusAsync(Guid recordId, SyncStatus newStatus)
        {
            var record = await _context.TimeClockRecords.FindAsync(recordId);
            if (record != null)
            {
                record.Status = newStatus;
                await _context.SaveChangesAsync();
            }
        }
    }
}
