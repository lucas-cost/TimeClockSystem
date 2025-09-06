using TimeClockSystem.Core.Enums;
using TimeClockSystem.Core.Interfaces;

namespace TimeClockSystem.Core.Services
{
    public class TimeClockService
    {
        private readonly ITimeClockRepository _repository;

        public TimeClockService(ITimeClockRepository repository)
        {
            _repository = repository;
        }

        public async Task<RecordType> GetNextRecordTypeAsync(string employeeId)
        {
            var lastRecord = await _repository.GetLastRecordForEmployeeAsync(employeeId);

            if (lastRecord == null || lastRecord.Type == RecordType.Exit)
            {
                return RecordType.Entry;
            }

            return RecordType.Exit;
        }
    }
}
