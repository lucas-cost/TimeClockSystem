using TimeClockSystem.Core.Enums;

namespace TimeClockSystem.Application.UseCases.RegisterPoint
{
    public class RegisterPointResult
    {
        public bool Success { get; set; }
        public RecordType? CreatedRecordType { get; set; }
        public string ErrorMessage { get; set; } = string.Empty;
    }
}
