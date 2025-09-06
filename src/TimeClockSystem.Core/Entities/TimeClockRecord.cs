using TimeClockSystem.Core.Enums;

namespace TimeClockSystem.Core.Entities
{
    public class TimeClockRecord
    {
        public Guid Id { get; set; }
        public string EmployeeId { get; set; }
        public DateTime Timestamp { get; set; }
        public RecordType Type { get; set; }
        public string Location { get; set; } // Simulado
        public string PhotoPath { get; set; } 
        public SyncStatus Status { get; set; }
    }
}
