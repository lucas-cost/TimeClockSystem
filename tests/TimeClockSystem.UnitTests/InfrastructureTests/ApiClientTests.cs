using AutoMapper;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Protected;
using Newtonsoft.Json;
using System.Net;
using System.Text;
using TimeClockSystem.Application.DTOs;
using TimeClockSystem.Core.Entities;
using TimeClockSystem.Core.Settings;
using TimeClockSystem.Infrastructure.Api;
using TimeClockSystem.Infrastructure.Api.DTOs;

namespace TimeClockSystem.UnitTests.InfrastructureTests
{
    [TestFixture]
    public class ApiClientTests
    {
        private Mock<IOptions<ApiSettings>> _mockApiSettings;
        private Mock<IMapper> _mockMapper;
        private Mock<ILogger<ApiClient>> _mockLogger;
        private Mock<HttpMessageHandler> _mockHttpMessageHandler;
        private HttpClient _httpClient;
        private ApiClient _apiClient;
        private ApiSettings _apiSettings;

        [SetUp]
        public void Setup()
        {
            _mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            _httpClient = new HttpClient(_mockHttpMessageHandler.Object);
            _mockLogger = new Mock<ILogger<ApiClient>>();

            _apiSettings = new ApiSettings
            {
                BaseUrl = "https://api.example.com/",
                AuthToken = "test-token"
            };

            _mockApiSettings = new Mock<IOptions<ApiSettings>>();
            _mockApiSettings.Setup(x => x.Value).Returns(_apiSettings);

            _mockMapper = new Mock<IMapper>();

            _apiClient = new ApiClient(_httpClient, _mockApiSettings.Object, _mockMapper.Object, _mockLogger.Object);
        }

        [TearDown]
        public void TearDown()
        {
            _httpClient.Dispose();
        }

        [Test]
        public async Task RegisterPointAsync_WhenRequestIsSuccessful_ReturnsTrue()
        {
            // Arrange
            TimeClockRecord record = new() { Id = new Guid("2d9c32ea-fbf6-40fe-856c-c609aa17c9f3"), EmployeeId = "123", Timestamp = DateTime.Now };
            ApiTimeClockRecordDto apiDto = new();
            RegisterPointResponseDto responseDto = new() { Message = "Point registered successfully" };

            _mockMapper.Setup(m => m.Map<ApiTimeClockRecordDto>(record)).Returns(apiDto);

            HttpResponseMessage responseMessage = new(HttpStatusCode.OK)
            {
                Content = new StringContent(JsonConvert.SerializeObject(responseDto), Encoding.UTF8, "application/json")
            };

            _mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req =>
                        req.Method == HttpMethod.Post &&
                        req.RequestUri.ToString().Contains("api/timesheet/register") &&
                        req.Headers.Authorization!.Scheme == "Bearer" &&
                        req.Headers.Authorization.Parameter == "test-token"),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(responseMessage);

            // Act
            bool result = await _apiClient.RegisterPointAsync(record);

            // Assert
            Assert.IsTrue(result);
            _mockMapper.Verify(m => m.Map<ApiTimeClockRecordDto>(record), Times.Once);
        }

        [Test]
        public async Task RegisterPointAsync_WhenRequestFails_ReturnsFalse()
        {
            // Arrange
            TimeClockRecord record = new() { Id = new Guid("2d9c32ea-fbf6-40fe-856c-c609aa17c9f3"), EmployeeId = "123", Timestamp = DateTime.Now };
            ApiTimeClockRecordDto apiDto = new();

            _mockMapper.Setup(m => m.Map<ApiTimeClockRecordDto>(record)).Returns(apiDto);

            HttpResponseMessage responseMessage = new(HttpStatusCode.BadRequest);

            _mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(responseMessage);

            // Act
            bool result = await _apiClient.RegisterPointAsync(record);

            // Assert
            Assert.IsFalse(result);
        }

        [Test]
        public async Task RegisterPointAsync_WhenExceptionOccurs_ReturnsFalse()
        {
            // Arrange
            TimeClockRecord record = new() { Id = new Guid("2d9c32ea-fbf6-40fe-856c-c609aa17c9f3"), EmployeeId = "123", Timestamp = DateTime.Now };
            ApiTimeClockRecordDto apiDto = new();

            _mockMapper.Setup(m => m.Map<ApiTimeClockRecordDto>(record)).Returns(apiDto);

            _mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ThrowsAsync(new HttpRequestException("Network error"));

            // Act
            bool result = await _apiClient.RegisterPointAsync(record);

            // Assert
            Assert.IsFalse(result);
        }
    }
}