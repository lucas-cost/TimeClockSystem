using Newtonsoft.Json;

namespace TimeClockSystem.Infrastructure.Api.DTOs
{
    public class ApiTimeClockRecordDto
    {
        [JsonProperty("employeeId")]
        public string EmployeeId { get; set; }

        [JsonProperty("timestamp")]
        public DateTime Timestamp { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("location")]
        public string Location { get; set; }

        [JsonProperty("photo")]
        public string Photo { get; set; }

        [JsonProperty("deviceId")]
        public string DeviceId { get; set; }
    }
}
