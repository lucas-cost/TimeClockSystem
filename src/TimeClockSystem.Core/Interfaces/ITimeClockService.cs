using TimeClockSystem.Core.Enums;

namespace TimeClockSystem.Core.Interfaces
{
    public interface ITimeClockService
    {
        Task<RecordType> GetNextRecordTypeAsync(string employeeId);
    }
}
