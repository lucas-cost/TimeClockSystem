using Moq;
using Moq.Protected;
using System.Net;
using TimeClockSystem.Infrastructure.Api;

namespace TimeClockSystem.UnitTests.InfrastructureTests
{
    [TestFixture]
    public class ApiHealthCheckServiceTests
    {
        private Mock<HttpMessageHandler> _mockHttpMessageHandler;
        private HttpClient _httpClient;
        private ApiHealthCheckService _apiHealthCheckService;

        [SetUp]
        public void Setup()
        {
            _mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            _httpClient = new HttpClient(_mockHttpMessageHandler.Object)
            {
                BaseAddress = new Uri("https://api.example.com/")
            };
            _apiHealthCheckService = new ApiHealthCheckService(_httpClient);
        }

        [TearDown]
        public void TearDown()
        {
            _apiHealthCheckService.StopMonitoring();
            _httpClient.Dispose();
        }

        [Test]
        public void Constructor_WithHttpClient_SetsHttpClient()
        {
            // Arrange & Act
            ApiHealthCheckService service = new(_httpClient);

            // Assert
            Assert.IsNotNull(service);
        }

        [Test]
        public async Task IsApiOnlineAsync_WhenApiReturnsSuccess_ReturnsTrue()
        {
            // Arrange
            _mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent("")
                });

            // Act
            bool result = await _apiHealthCheckService.IsApiOnlineAsync();

            // Assert
            Assert.IsTrue(result);
        }

        [Test]
        public async Task IsApiOnlineAsync_WhenApiReturnsError_ReturnsFalse()
        {
            // Arrange
            _mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.InternalServerError,
                    Content = new StringContent("")
                });

            // Act
            bool result = await _apiHealthCheckService.IsApiOnlineAsync();

            // Assert
            Assert.IsFalse(result);
        }

        [Test]
        public async Task IsApiOnlineAsync_WhenApiTimesOut_ReturnsFalse()
        {
            // Arrange
            _mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ThrowsAsync(new TaskCanceledException());

            // Act
            bool result = await _apiHealthCheckService.IsApiOnlineAsync();

            // Assert
            Assert.IsFalse(result);
        }

        [Test]
        public async Task IsApiOnlineAsync_WhenHttpExceptionOccurs_ReturnsFalse()
        {
            // Arrange
            _mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ThrowsAsync(new HttpRequestException());

            // Act
            bool result = await _apiHealthCheckService.IsApiOnlineAsync();

            // Assert
            Assert.IsFalse(result);
        }

        [Test]
        public void StartMonitoring_WhenCalled_StartsTimer()
        {
            // Arrange
            bool eventRaised = false;
            _apiHealthCheckService.ConnectionStatusChanged += (status) => eventRaised = true;

            // Mock para retornar sucesso
            _mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent("")
                });

            // Act
            _apiHealthCheckService.StartMonitoring();

            // Assert - Aguarda um pouco para o timer executar
            Thread.Sleep(100); // Pequeno delay para permitir a execução do timer
            Assert.IsTrue(eventRaised, "O evento ConnectionStatusChanged deveria ter sido invocado");
        }

        [Test]
        public void StopMonitoring_WhenCalled_StopsTimer()
        {
            // Arrange
            int eventCount = 0;
            _apiHealthCheckService.ConnectionStatusChanged += (status) => eventCount++;

            _mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent("")
                });

            _apiHealthCheckService.StartMonitoring();

            // Act
            _apiHealthCheckService.StopMonitoring();
            int countAfterStop = eventCount;

            // Aguarda um pouco para ver se o timer continua executando
            Thread.Sleep(150);

            // Assert
            Assert.That(eventCount, Is.EqualTo(countAfterStop), "O timer deveria ter parado de executar");
        }

        [Test]
        public void ConnectionStatusChanged_Event_WhenStatusChanges_IsInvoked()
        {
            // Arrange
            bool? lastStatus = null;
            _apiHealthCheckService.ConnectionStatusChanged += (status) => lastStatus = status;

            // Configura o mock para retornar sucesso
            _mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent("")
                });

            // Act
            _apiHealthCheckService.StartMonitoring();
            Thread.Sleep(100); // Aguarda a execução do timer

            // Assert
            Assert.IsTrue(lastStatus.HasValue, "O evento deveria ter sido invocado");
            Assert.IsTrue(lastStatus.Value, "O status deveria ser true (online)");
        }

        [Test]
        public void Dispose_WhenCalled_DisposesTimer()
        {
            // Arrange
            _apiHealthCheckService.StartMonitoring();

            // Act
            _apiHealthCheckService.StopMonitoring(); // Equivalente a Dispose para o timer

            // Assert - Se não houve exceção, o dispose funcionou
            Assert.Pass();
        }
    }
}
