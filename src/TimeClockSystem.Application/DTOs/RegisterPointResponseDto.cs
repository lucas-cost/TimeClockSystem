namespace TimeClockSystem.Application.DTOs
{
    public class RegisterPointResponseDto
    {
        public string Id { get; set; }
        public string Status { get; set; }
        public DateTime ServerTimestamp { get; set; }
        public string Message { get; set; }
    }
}
