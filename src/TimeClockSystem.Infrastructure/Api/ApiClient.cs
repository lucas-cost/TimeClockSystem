using AutoMapper;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Text;
using TimeClockSystem.Application.DTOs;
using TimeClockSystem.Core.Entities;
using TimeClockSystem.Core.Interfaces;
using TimeClockSystem.Core.Settings;
using TimeClockSystem.Infrastructure.Api.DTOs;

namespace TimeClockSystem.Infrastructure.Api
{
    public class ApiClient : IApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly ApiSettings _apiSettings;
        private readonly IMapper _mapper;
        private readonly ILogger<ApiClient> _logger;

        public ApiClient(HttpClient httpClient, IOptions<ApiSettings> apiSettings, IMapper mapper, ILogger<ApiClient> logger)
        {
            _httpClient = httpClient;
            _mapper = mapper;
            _apiSettings = apiSettings.Value;
            _logger = logger;

            _httpClient.BaseAddress = new Uri(_apiSettings.BaseUrl);
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _apiSettings.AuthToken);
        }

        public async Task<bool> RegisterPointAsync(TimeClockRecord record)
        {
            try
            {
                ApiTimeClockRecordDto payloadDto = _mapper.Map<ApiTimeClockRecordDto>(record);

                string jsonPayload = JsonConvert.SerializeObject(payloadDto);
                StringContent content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

                HttpResponseMessage response = await _httpClient.PostAsync("api/timesheet/register", content);

                if (response.IsSuccessStatusCode)
                {
                    string jsonResponse = await response.Content.ReadAsStringAsync();
                    RegisterPointResponseDto responseDto = JsonConvert.DeserializeObject<RegisterPointResponseDto>(jsonResponse)!;

                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao sincronizar ponto para o funcionário {EmployeeId}", record.EmployeeId);
                return false;
            }
        }
    }
}
