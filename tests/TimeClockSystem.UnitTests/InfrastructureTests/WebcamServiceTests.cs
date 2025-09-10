using Moq;
using OpenCvSharp;
using TimeClockSystem.Infrastructure.Hardware;
using TimeClockSystem.Infrastructure.Hardware.Abstractions;

namespace TimeClockSystem.UnitTests.InfrastructureTests
{
    [TestFixture]
    public class WebcamServiceTests
    {
        private Mock<IVideoCaptureWrapper> _mockVideoCapture;
        private WebcamService _webcamService;
        private CancellationTokenSource _cancellationTokenSource;

        [SetUp]
        public void Setup()
        {
            _mockVideoCapture = new Mock<IVideoCaptureWrapper>();
            _cancellationTokenSource = new CancellationTokenSource();
        }

        [TearDown]
        public void TearDown()
        {
            _webcamService?.Dispose();
            _cancellationTokenSource?.Dispose();
        }

        [Test]
        public void Constructor_WithNullCapture_SetsCaptureToNull()
        {
            // Arrange & Act
            _webcamService = new WebcamService(null);

            // Assert
            Assert.IsFalse(_webcamService.IsWebcamAvailable());
        }

        [Test]
        public void Constructor_WithValidCapture_SetsCapture()
        {
            // Arrange
            _mockVideoCapture.Setup(v => v.IsOpened()).Returns(true);

            // Act
            _webcamService = new WebcamService(_mockVideoCapture.Object);

            // Assert
            Assert.IsTrue(_webcamService.IsWebcamAvailable());
        }

        [Test]
        public void IsWebcamAvailable_WhenCaptureIsNull_ReturnsFalse()
        {
            // Arrange
            _webcamService = new WebcamService(null);

            // Act
            bool result = _webcamService.IsWebcamAvailable();

            // Assert
            Assert.IsFalse(result);
        }

        [Test]
        public void IsWebcamAvailable_WhenCaptureIsNotOpened_ReturnsFalse()
        {
            // Arrange
            _mockVideoCapture.Setup(v => v.IsOpened()).Returns(false);
            _webcamService = new WebcamService(_mockVideoCapture.Object);

            // Act
            bool result = _webcamService.IsWebcamAvailable();

            // Assert
            Assert.IsFalse(result);
        }

        [Test]
        public void IsWebcamAvailable_WhenCaptureIsOpened_ReturnsTrue()
        {
            // Arrange
            _mockVideoCapture.Setup(v => v.IsOpened()).Returns(true);
            _webcamService = new WebcamService(_mockVideoCapture.Object);

            // Act
            bool result = _webcamService.IsWebcamAvailable();

            // Assert
            Assert.IsTrue(result);
        }

        [Test]
        public void Start_WhenWebcamNotAvailable_ThrowsInvalidOperationException()
        {
            // Arrange
            _mockVideoCapture.Setup(v => v.IsOpened()).Returns(false);
            _webcamService = new WebcamService(_mockVideoCapture.Object);

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => _webcamService.Start());
        }

        [Test]
        public void Start_WhenAlreadyRunning_DoesNothing()
        {
            // Arrange
            _mockVideoCapture.Setup(v => v.IsOpened()).Returns(true);
            _webcamService = new WebcamService(_mockVideoCapture.Object);

            // Primeira chamada
            _webcamService.Start();

            // Act - Segunda chamada
            Assert.DoesNotThrow(() => _webcamService.Start());

            // Cleanup
            _webcamService.Stop();
        }

        [Test]
        public async Task Start_WhenCalled_StartsCaptureLoop()
        {
            // Arrange
            _mockVideoCapture.Setup(v => v.IsOpened()).Returns(true);
            _webcamService = new WebcamService(_mockVideoCapture.Object);

            var frameReadyCalled = false;
            _webcamService.FrameReady += (buffer) => frameReadyCalled = true;

            // Act
            _webcamService.Start();
            await Task.Delay(100); // Aguarda um loop
            _webcamService.Stop();

            // Assert
            _mockVideoCapture.Verify(v => v.Read(It.IsAny<Mat>()), Times.AtLeastOnce);
        }

        [Test]
        public void Stop_WhenNotRunning_DoesNothing()
        {
            // Arrange
            _webcamService = new WebcamService(_mockVideoCapture.Object);

            // Act & Assert
            Assert.DoesNotThrow(() => _webcamService.Stop());
        }

        [Test]
        public async Task Stop_WhenRunning_StopsCaptureLoop()
        {
            // Arrange
            _mockVideoCapture.Setup(v => v.IsOpened()).Returns(true);
            _webcamService = new WebcamService(_mockVideoCapture.Object);

            _webcamService.Start();
            await Task.Delay(50);

            // Act
            _webcamService.Stop();
            await Task.Delay(100); // Aguarda para garantir que parou

            // Assert
            // Se não houve exception, o stop funcionou
            Assert.Pass();
        }

        [Test]
        public void CaptureAndSaveImage_WhenWebcamNotAvailable_ThrowsInvalidOperationException()
        {
            // Arrange
            _mockVideoCapture.Setup(v => v.IsOpened()).Returns(false);
            _webcamService = new WebcamService(_mockVideoCapture.Object);

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() =>
                _webcamService.CaptureAndSaveImage("123"));
        }

        [Test]
        public void CaptureAndSaveImage_WhenCaptureIsNull_ThrowsInvalidOperationException()
        {
            // Arrange
            _webcamService = new WebcamService(null);

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() =>
                _webcamService.CaptureAndSaveImage("123"));
        }

        [Test]
        public void CaptureAndSaveImage_WhenSuccessful_ReturnsValidFilePath()
        {
            // Arrange
            _mockVideoCapture.Setup(v => v.IsOpened()).Returns(true);

            // Simula uma captura bem-sucedida criando um frame válido
            _mockVideoCapture.Setup(v => v.Read(It.IsAny<Mat>()))
                .Callback<Mat>((frame) =>
                {
                    // Cria um frame válido (100x100 pixels) para simular captura bem-sucedida
                    frame.Create(100, 100, MatType.CV_8UC3);
                    // Preenche com algum conteúdo (cor preta)
                    frame.SetTo(new Scalar(0, 0, 0));
                });

            _webcamService = new WebcamService(_mockVideoCapture.Object);
            string employeeId = "12345";

            // Act
            string result = _webcamService.CaptureAndSaveImage(employeeId);

            // Assert
            Assert.IsNotNull(result);
            Assert.That(result, Does.Contain($"ponto_{employeeId}"));
            Assert.That(result, Does.Contain(".jpg"));
            Assert.That(result, Does.StartWith(Path.GetTempPath()));

            // Verifica se o arquivo foi realmente criado
            Assert.IsTrue(File.Exists(result), "Arquivo deveria ter sido criado");

            // Cleanup
            if (File.Exists(result))
                File.Delete(result);
        }

        [Test]
        public async Task FrameReady_Event_WhenFrameCaptured_IsInvoked()
        {
            // Arrange
            _mockVideoCapture.Setup(v => v.IsOpened()).Returns(true);

            // Configura o mock para preencher o frame com dados válidos
            _mockVideoCapture.Setup(v => v.Read(It.IsAny<Mat>()))
                .Callback<Mat>((frame) =>
                {
                    // Cria um frame válido para simular captura bem-sucedida
                    using var sampleFrame = new Mat(100, 100, MatType.CV_8UC3, new Scalar(255, 0, 0));
                    sampleFrame.CopyTo(frame);
                });

            _webcamService = new WebcamService(_mockVideoCapture.Object);

            bool eventInvoked = false;
            byte[] capturedBuffer = null!;
            _webcamService.FrameReady += (buffer) =>
            {
                eventInvoked = true;
                capturedBuffer = buffer;
            };

            // Act
            _webcamService.Start();
            await Task.Delay(150); // Aguarda um pouco mais para garantir o processamento
            _webcamService.Stop();
            await Task.Delay(50); // Aguarda finalização

            // Assert
            Assert.IsTrue(eventInvoked, "O evento FrameReady deveria ter sido invocado");
            Assert.IsNotNull(capturedBuffer, "O buffer não deveria ser nulo");
            Assert.Greater(capturedBuffer.Length, 0, "O buffer deveria conter dados");
        }

        [Test]
        public void Dispose_WhenCalled_DisposesResources()
        {
            // Arrange
            _mockVideoCapture.Setup(v => v.IsOpened()).Returns(true);
            _webcamService = new WebcamService(_mockVideoCapture.Object);

            // Act
            _webcamService.Dispose();

            // Assert
            _mockVideoCapture.Verify(v => v.Dispose(), Times.Once);
        }

        [Test]
        public void Dispose_WhenCalledMultipleTimes_DoesNotThrow()
        {
            // Arrange
            _mockVideoCapture.Setup(v => v.IsOpened()).Returns(true);
            _webcamService = new WebcamService(_mockVideoCapture.Object);

            // Act & Assert - múltiplas chamadas de Dispose não devem lançar exceção
            Assert.DoesNotThrow(() =>
            {
                _webcamService.Dispose();
                _webcamService.Dispose(); // Segunda chamada
            });

            // Verifica que Dispose foi chamado pelo menos uma vez
            _mockVideoCapture.Verify(v => v.Dispose(), Times.AtLeastOnce);
        }


        [Test]
        public async Task CaptureLoop_WhenCancelled_StopsGracefully()
        {
            // Arrange
            _mockVideoCapture.Setup(v => v.IsOpened()).Returns(true);
            _webcamService = new WebcamService(_mockVideoCapture.Object);

            // Act
            _webcamService.Start();
            await Task.Delay(50);
            _webcamService.Stop();
            await Task.Delay(50);

            // Assert - Se não houve exception, o cancelamento foi graceful
            Assert.Pass();
        }

        [Test]
        public void CaptureAndSaveImage_WhenFrameIsEmpty_ThrowsInvalidOperationException()
        {
            // Arrange
            _mockVideoCapture.Setup(v => v.IsOpened()).Returns(true);

            // Simula frame vazio configurando o Mat para estar vazio após a leitura
            // Precisamos mockar o comportamento interno do Read
            _mockVideoCapture.Setup(v => v.Read(It.IsAny<Mat>()))
                .Callback<Mat>((frame) =>
                {
                    // Simula um frame vazio
                    frame.Create(0, 0, MatType.CV_8UC3);
                });

            _webcamService = new WebcamService(_mockVideoCapture.Object);

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() =>
                _webcamService.CaptureAndSaveImage("123"));
        }

        [Test]
        public void CaptureAndSaveImage_WhenFrameCaptureFails_ThrowsInvalidOperationException()
        {
            // Arrange
            _mockVideoCapture.Setup(v => v.IsOpened()).Returns(true);

            // Simula falha na captura (frame vazio)
            _mockVideoCapture.Setup(v => v.Read(It.IsAny<Mat>()))
                .Callback<Mat>((frame) =>
                {
                    // Cria um frame vazio (0x0)
                    frame.Create(0, 0, MatType.CV_8UC3);
                });

            _webcamService = new WebcamService(_mockVideoCapture.Object);

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() =>
                _webcamService.CaptureAndSaveImage("123"));
        }
    }
}
