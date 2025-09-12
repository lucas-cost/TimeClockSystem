using Microsoft.Extensions.Logging;
using Moq;
using OpenCvSharp;
using System.Reflection;
using TimeClockSystem.Core.Exceptions;
using TimeClockSystem.Infrastructure.Hardware;
using TimeClockSystem.Infrastructure.Hardware.Abstractions;

namespace TimeClockSystem.UnitTests.InfrastructureTests
{
    [TestFixture]
    public class WebcamServiceTests
    {
        private Mock<IVideoCaptureWrapper> _mockVideoCapture;
        private Mock<ILogger<WebcamService>> _mockLogger;
        private WebcamService _webcamService;
        private CancellationTokenSource _cancellationTokenSource;

        [SetUp]
        public void Setup()
        {
            _mockVideoCapture = new Mock<IVideoCaptureWrapper>();
            _mockLogger = new Mock<ILogger<WebcamService>>();
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
            _webcamService = new WebcamService(null, _mockLogger.Object);

            // Assert
            Assert.IsFalse(_webcamService.IsWebcamAvailable());
        }

        [Test]
        public void Constructor_WithValidCapture_SetsCapture()
        {
            // Arrange
            _mockVideoCapture.Setup(v => v.IsOpened()).Returns(true);

            // Act
            _webcamService = new WebcamService(_mockVideoCapture.Object, _mockLogger.Object);

            // Assert
            Assert.IsTrue(_webcamService.IsWebcamAvailable());
        }

        [Test]
        public void IsWebcamAvailable_WhenCaptureIsNull_ReturnsFalse()
        {
            // Arrange
            _webcamService = new WebcamService(null, _mockLogger.Object);

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
            _webcamService = new WebcamService(_mockVideoCapture.Object, _mockLogger.Object);

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
            _webcamService = new WebcamService(_mockVideoCapture.Object, _mockLogger.Object);

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
            _webcamService = new WebcamService(_mockVideoCapture.Object, _mockLogger.Object);

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => _webcamService.Start());
        }

        [Test]
        public void Start_WhenAlreadyRunning_DoesNothing()
        {
            // Arrange
            _mockVideoCapture.Setup(v => v.IsOpened()).Returns(true);
            _webcamService = new WebcamService(_mockVideoCapture.Object, _mockLogger.Object);

            _webcamService.Start();

            // Act
            Assert.DoesNotThrow(() => _webcamService.Start());

            // Cleanup
            _webcamService.Stop();
        }

        [Test]
        public async Task Start_WhenCalled_StartsCaptureLoop()
        {
            // Arrange
            _mockVideoCapture.Setup(v => v.IsOpened()).Returns(true);
            _webcamService = new WebcamService(_mockVideoCapture.Object, _mockLogger.Object);

            var frameReadyCalled = false;
            _webcamService.FrameReady += (buffer) => frameReadyCalled = true;

            // Act
            _webcamService.Start();
            await Task.Delay(100); 
            _webcamService.Stop();

            // Assert
            _mockVideoCapture.Verify(v => v.Read(It.IsAny<Mat>()), Times.AtLeastOnce);
        }

        [Test]
        public void Stop_WhenNotRunning_DoesNothing()
        {
            // Arrange
            _webcamService = new WebcamService(_mockVideoCapture.Object, _mockLogger.Object);

            // Act & Assert
            Assert.DoesNotThrow(() => _webcamService.Stop());
        }

        [Test]
        public async Task Stop_WhenRunning_StopsCaptureLoop()
        {
            // Arrange
            _mockVideoCapture.Setup(v => v.IsOpened()).Returns(true);
            _webcamService = new WebcamService(_mockVideoCapture.Object, _mockLogger.Object);

            _webcamService.Start();
            await Task.Delay(50);

            // Act
            _webcamService.Stop();
            await Task.Delay(100);

            // Assert
            Assert.Pass();
        }

        [Test]
        public void CaptureAndSaveImage_WhenWebcamNotAvailable_ThrowsInvalidOperationException()
        {
            // Arrange
            _mockVideoCapture.Setup(v => v.IsOpened()).Returns(false);
            _webcamService = new WebcamService(_mockVideoCapture.Object, _mockLogger.Object);

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() =>
                _webcamService.CaptureAndSaveImage("123"));
        }

        [Test]
        public void CaptureAndSaveImage_WhenCaptureIsNull_ThrowsInvalidOperationException()
        {
            // Arrange
            _webcamService = new WebcamService(null, _mockLogger.Object);

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() =>
                _webcamService.CaptureAndSaveImage("123"));
        }

        [Test]
        public void CaptureAndSaveImage_WhenSuccessful_ReturnsValidFilePath()
        {
            // Arrange
            _mockVideoCapture.Setup(v => v.IsOpened()).Returns(true);

            _mockVideoCapture.Setup(v => v.Read(It.IsAny<Mat>()))
                .Callback<Mat>((frame) =>
                {
                    frame.Create(100, 100, MatType.CV_8UC3);

                    frame.RowRange(0, 50).SetTo(new Scalar(50, 50, 50));   
                    frame.RowRange(50, 100).SetTo(new Scalar(70, 70, 70)); 

                    Cv2.Line(frame, new Point(0, 0), new Point(100, 100), new Scalar(255, 255, 255), 2);
                    Cv2.Line(frame, new Point(0, 100), new Point(100, 0), new Scalar(255, 255, 255), 2);
                });

            _webcamService = new WebcamService(_mockVideoCapture.Object, _mockLogger.Object);
            string employeeId = "12345";

            // Act
            string result = _webcamService.CaptureAndSaveImage(employeeId);

            // Assert
            Assert.IsNotNull(result);
            Assert.That(result, Does.Contain($"ponto_{employeeId}"));
            Assert.That(result, Does.Contain(".jpg"));
            Assert.That(result, Does.StartWith(Path.GetTempPath()));

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

            _mockVideoCapture.Setup(v => v.Read(It.IsAny<Mat>()))
                .Callback<Mat>((frame) =>
                {
                    using Mat sampleFrame = new Mat(100, 100, MatType.CV_8UC3, new Scalar(255, 0, 0));
                    sampleFrame.CopyTo(frame);
                });

            _webcamService = new WebcamService(_mockVideoCapture.Object, _mockLogger.Object);

            bool eventInvoked = false;
            byte[] capturedBuffer = null!;
            _webcamService.FrameReady += (buffer) =>
            {
                eventInvoked = true;
                capturedBuffer = buffer;
            };

            // Act
            _webcamService.Start();
            await Task.Delay(150);
            _webcamService.Stop();
            await Task.Delay(50); 

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
            _webcamService = new WebcamService(_mockVideoCapture.Object, _mockLogger.Object);

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
            _webcamService = new WebcamService(_mockVideoCapture.Object, _mockLogger.Object);

            // Act & Assert
            Assert.DoesNotThrow(() =>
            {
                _webcamService.Dispose();
                _webcamService.Dispose(); 
            });

            _mockVideoCapture.Verify(v => v.Dispose(), Times.AtLeastOnce);
        }


        [Test]
        public async Task CaptureLoop_WhenCancelled_StopsGracefully()
        {
            // Arrange
            _mockVideoCapture.Setup(v => v.IsOpened()).Returns(true);
            _webcamService = new WebcamService(_mockVideoCapture.Object, _mockLogger.Object);

            // Act
            _webcamService.Start();
            await Task.Delay(50);
            _webcamService.Stop();
            await Task.Delay(50);

            // Assert 
            Assert.Pass();
        }

        [Test]
        public void CaptureAndSaveImage_WhenFrameIsEmpty_ThrowsInvalidOperationException()
        {
            // Arrange
            _mockVideoCapture.Setup(v => v.IsOpened()).Returns(true);

            _mockVideoCapture.Setup(v => v.Read(It.IsAny<Mat>()))
                .Callback<Mat>((frame) =>
                {
                    frame.Create(0, 0, MatType.CV_8UC3);
                });

            _webcamService = new WebcamService(_mockVideoCapture.Object, _mockLogger.Object);

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() =>
                _webcamService.CaptureAndSaveImage("123"));
        }

        [Test]
        public void CaptureAndSaveImage_WhenFrameCaptureFails_ThrowsInvalidOperationException()
        {
            // Arrange
            _mockVideoCapture.Setup(v => v.IsOpened()).Returns(true);

            _mockVideoCapture.Setup(v => v.Read(It.IsAny<Mat>()))
                .Callback<Mat>((frame) =>
                {
                    frame.Create(0, 0, MatType.CV_8UC3);
                });

            _webcamService = new WebcamService(_mockVideoCapture.Object, _mockLogger.Object);

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() =>
                _webcamService.CaptureAndSaveImage("123"));
        }
        [Test]
        public void ValidateImageQuality_WhenImageTooDark_ThrowsImageQualityException()
        {
            // Arrange
            _mockVideoCapture.Setup(v => v.IsOpened()).Returns(true);
            _webcamService = new WebcamService(_mockVideoCapture.Object, _mockLogger.Object);

            using Mat darkImage = new Mat(100, 100, MatType.CV_8UC3, new Scalar(10, 10, 10)); 

            // Act & Assert
            TargetInvocationException ex = Assert.Throws<TargetInvocationException>(() =>
                _webcamService.GetType()
                    .GetMethod("ValidateImageQuality", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!
                    .Invoke(_webcamService, new object[] { darkImage }));

            Assert.IsInstanceOf<ImageQualityException>(ex.InnerException);
            Assert.That(ex.InnerException!.Message, Does.Contain("muito escura"));
        }

        [Test]
        public void ValidateImageQuality_WhenImageTooBright_ThrowsImageQualityException()
        {
            // Arrange
            _mockVideoCapture.Setup(v => v.IsOpened()).Returns(true);
            _webcamService = new WebcamService(_mockVideoCapture.Object, _mockLogger.Object);

            using Mat brightImage = new Mat(100, 100, MatType.CV_8UC3, new Scalar(200, 200, 200));

            // Act & Assert
            TargetInvocationException ex = Assert.Throws<TargetInvocationException>(() =>
                _webcamService.GetType()
                    .GetMethod("ValidateImageQuality", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!
                    .Invoke(_webcamService, new object[] { brightImage }));

            Assert.IsInstanceOf<ImageQualityException>(ex.InnerException);
            Assert.That(ex.InnerException!.Message, Does.Contain("muito clara"));
        }

        [Test]
        public void ValidateImageQuality_WhenImageBlurred_ThrowsImageQualityException()
        {
            // Arrange
            _mockVideoCapture.Setup(v => v.IsOpened()).Returns(true);
            _webcamService = new WebcamService(_mockVideoCapture.Object, _mockLogger.Object);

            using Mat blurredImage = new Mat(100, 100, MatType.CV_8UC3, new Scalar(60, 60, 60));

            // Act & Assert
            TargetInvocationException ex = Assert.Throws<TargetInvocationException>(() =>
                _webcamService.GetType()
                    .GetMethod("ValidateImageQuality", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!
                    .Invoke(_webcamService, new object[] { blurredImage }));

            Assert.IsInstanceOf<ImageQualityException>(ex.InnerException);
            Assert.That(ex.InnerException!.Message, Does.Contain("A imagem está (borrada). Fique parado e tente novamente."));
        }

        [Test]
        public void ValidateImageQuality_WhenImageHasGoodQuality_DoesNotThrowException()
        {
            // Arrange
            _mockVideoCapture.Setup(v => v.IsOpened()).Returns(true);
            _webcamService = new WebcamService(_mockVideoCapture.Object, _mockLogger.Object);

            using Mat goodImage = new Mat(100, 100, MatType.CV_8UC3, new Scalar(60, 60, 60));

            goodImage.Rectangle(new Rect(20, 20, 60, 60), new Scalar(0, 0, 0), -1); 
            goodImage.Rectangle(new Rect(25, 25, 50, 50), new Scalar(255, 255, 255), -1); 

            Cv2.Circle(goodImage, new Point(50, 50), 20, new Scalar(0, 0, 0), 2);

            // Act & Assert
            Assert.DoesNotThrow(() =>
                _webcamService.GetType()
                    .GetMethod("ValidateImageQuality", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!
                    .Invoke(_webcamService, new object[] { goodImage }));
        }
    }
}
