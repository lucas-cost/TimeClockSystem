using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TimeClockSystem.Application.DTOs;
using TimeClockSystem.Application.Interfaces;
using TimeClockSystem.Application.UseCases.RegisterPoint;
using TimeClockSystem.Core.Entities;
using TimeClockSystem.Core.Enums;
using TimeClockSystem.Core.Exceptions;
using TimeClockSystem.Core.Interfaces;

namespace TimeClockSystem.UnitTests.ApplicationTests
{
    [TestFixture]
    public class RegisterPointCommandHandlerTests
    {
        private Mock<ITimeClockRepository> _mockRepository;
        private Mock<IApiClient> _mockApiClient;
        private Mock<IWebcamService> _mockWebcamService;
        private Mock<ITimeClockService> _mockTimeClockService;
        private Mock<ILogger<RegisterPointCommandHandler>> _mockLogger;
        private RegisterPointCommandHandler _handler;
        private RegisterPointRequestDto _requestDto;
        private RegisterPointCommand _command;

        [SetUp]
        public void Setup()
        {
            _mockRepository = new Mock<ITimeClockRepository>();
            _mockApiClient = new Mock<IApiClient>();
            _mockWebcamService = new Mock<IWebcamService>();
            _mockTimeClockService = new Mock<ITimeClockService>();
            _mockLogger = new Mock<ILogger<RegisterPointCommandHandler>>();

            _handler = new RegisterPointCommandHandler(
                _mockRepository.Object,
                _mockApiClient.Object,
                _mockWebcamService.Object,
                _mockTimeClockService.Object,
                _mockLogger.Object);

            _requestDto = new RegisterPointRequestDto { EmployeeId = "123" };
            _command = new RegisterPointCommand(_requestDto);
        }

        [TearDown]
        public void TearDown()
        {
            _mockRepository.Invocations.Clear();
            _mockApiClient.Invocations.Clear();
            _mockWebcamService.Invocations.Clear();
            _mockTimeClockService.Invocations.Clear();
        }

        [Test]
        public async Task Handle_WhenSuccessful_ReturnsSuccessResult()
        {
            // Arrange
            _mockTimeClockService.Setup(s => s.GetNextRecordTypeAsync(_requestDto.EmployeeId))
                .ReturnsAsync(RecordType.Entry);
            _mockWebcamService.Setup(w => w.CaptureAndSaveImage(_requestDto.EmployeeId))
                .Returns(@"C:\temp\photo.jpg");
            _mockApiClient.Setup(a => a.RegisterPointAsync(It.IsAny<TimeClockRecord>()))
                .ReturnsAsync(true);

            // Act
            RegisterPointResult result = await _handler.Handle(_command, CancellationToken.None);

            // Assert
            Assert.IsTrue(result.Success);
        }

        [Test]
        public async Task Handle_WhenSuccessful_ReturnsCorrectRecordType()
        {
            // Arrange
            RecordType expectedRecordType = RecordType.Entry;
            _mockTimeClockService.Setup(s => s.GetNextRecordTypeAsync(_requestDto.EmployeeId))
                .ReturnsAsync(expectedRecordType);
            _mockWebcamService.Setup(w => w.CaptureAndSaveImage(_requestDto.EmployeeId))
                .Returns(@"C:\temp\photo.jpg");
            _mockApiClient.Setup(a => a.RegisterPointAsync(It.IsAny<TimeClockRecord>()))
                .ReturnsAsync(true);

            // Act
            RegisterPointResult result = await _handler.Handle(_command, CancellationToken.None);

            // Assert
            Assert.AreEqual(expectedRecordType, result.CreatedRecordType);
        }

        [Test]
        public async Task Handle_WhenSuccessful_AddsRecordToRepository()
        {
            // Arrange
            _mockTimeClockService.Setup(s => s.GetNextRecordTypeAsync(_requestDto.EmployeeId))
                .ReturnsAsync(RecordType.Entry);
            _mockWebcamService.Setup(w => w.CaptureAndSaveImage(_requestDto.EmployeeId))
                .Returns(@"C:\temp\photo.jpg");
            _mockApiClient.Setup(a => a.RegisterPointAsync(It.IsAny<TimeClockRecord>()))
                .ReturnsAsync(true);

            // Act
            await _handler.Handle(_command, CancellationToken.None);

            // Assert
            _mockRepository.Verify(r => r.AddAsync(It.IsAny<TimeClockRecord>()), Times.Once);
        }

        [Test]
        public async Task Handle_WhenApiSucceeds_UpdatesStatusToSynced()
        {
            // Arrange
            _mockTimeClockService.Setup(s => s.GetNextRecordTypeAsync(_requestDto.EmployeeId))
                .ReturnsAsync(RecordType.Entry);
            _mockWebcamService.Setup(w => w.CaptureAndSaveImage(_requestDto.EmployeeId))
                .Returns(@"C:\temp\photo.jpg");
            _mockApiClient.Setup(a => a.RegisterPointAsync(It.IsAny<TimeClockRecord>()))
                .ReturnsAsync(true);

            // Act
            await _handler.Handle(_command, CancellationToken.None);

            // Assert
            _mockRepository.Verify(r => r.UpdateStatusAsync(It.IsAny<Guid>(), SyncStatus.Synced), Times.Once);
        }

        [Test]
        public async Task Handle_WhenApiFails_DoesNotUpdateStatus()
        {
            // Arrange
            _mockTimeClockService.Setup(s => s.GetNextRecordTypeAsync(_requestDto.EmployeeId))
                .ReturnsAsync(RecordType.Entry);
            _mockWebcamService.Setup(w => w.CaptureAndSaveImage(_requestDto.EmployeeId))
                .Returns(@"C:\temp\photo.jpg");
            _mockApiClient.Setup(a => a.RegisterPointAsync(It.IsAny<TimeClockRecord>()))
                .ReturnsAsync(false);

            // Act
            await _handler.Handle(_command, CancellationToken.None);

            // Assert
            _mockRepository.Verify(r => r.UpdateStatusAsync(It.IsAny<Guid>(), It.IsAny<SyncStatus>()), Times.Never);
        }

        [Test]
        public async Task Handle_WhenBusinessRuleException_ReturnsFailureWithErrorMessage()
        {
            // Arrange
            string errorMessage = "Jornada mínima não cumprida";
            _mockTimeClockService.Setup(s => s.GetNextRecordTypeAsync(_requestDto.EmployeeId))
                .ThrowsAsync(new BusinessRuleException(errorMessage));

            // Act
            RegisterPointResult result = await _handler.Handle(_command, CancellationToken.None);

            // Assert
            Assert.IsFalse(result.Success);
            Assert.AreEqual(errorMessage, result.ErrorMessage);
        }

        [Test]
        public async Task Handle_WhenBusinessRuleException_DoesNotAddRecord()
        {
            // Arrange
            _mockTimeClockService.Setup(s => s.GetNextRecordTypeAsync(_requestDto.EmployeeId))
                .ThrowsAsync(new BusinessRuleException("Error"));

            // Act
            await _handler.Handle(_command, CancellationToken.None);

            // Assert
            _mockRepository.Verify(r => r.AddAsync(It.IsAny<TimeClockRecord>()), Times.Never);
        }

        [Test]
        public async Task Handle_WhenGeneralException_ReturnsGenericErrorMessage()
        {
            // Arrange
            _mockTimeClockService.Setup(s => s.GetNextRecordTypeAsync(_requestDto.EmployeeId))
                .ThrowsAsync(new Exception("Database error"));

            // Act
            RegisterPointResult result = await _handler.Handle(_command, CancellationToken.None);

            // Assert
            Assert.AreEqual("Ocorreu uma falha inesperada ao registrar o ponto.", result.ErrorMessage);
        }

        [Test]
        public async Task Handle_WhenWebcamFails_ReturnsFailureResult()
        {
            // Arrange
            _mockTimeClockService.Setup(s => s.GetNextRecordTypeAsync(_requestDto.EmployeeId))
                .ReturnsAsync(RecordType.Entry);
            _mockWebcamService.Setup(w => w.CaptureAndSaveImage(_requestDto.EmployeeId))
                .Throws(new InvalidOperationException("Webcam error"));

            // Act
            RegisterPointResult result = await _handler.Handle(_command, CancellationToken.None);

            // Assert
            Assert.IsFalse(result.Success);
        }

        [Test]
        public async Task Handle_CreatesRecordWithCorrectEmployeeId()
        {
            // Arrange
            TimeClockRecord capturedRecord = null!;
            _mockTimeClockService.Setup(s => s.GetNextRecordTypeAsync(_requestDto.EmployeeId))
                .ReturnsAsync(RecordType.Entry);
            _mockWebcamService.Setup(w => w.CaptureAndSaveImage(_requestDto.EmployeeId))
                .Returns(@"C:\temp\photo.jpg");
            _mockApiClient.Setup(a => a.RegisterPointAsync(It.IsAny<TimeClockRecord>()))
                .ReturnsAsync(true);
            _mockRepository.Setup(r => r.AddAsync(It.IsAny<TimeClockRecord>()))
                .Callback<TimeClockRecord>(r => capturedRecord = r)
                .Returns(Task.CompletedTask);

            // Act
            await _handler.Handle(_command, CancellationToken.None);

            // Assert
            Assert.AreEqual(_requestDto.EmployeeId, capturedRecord.EmployeeId);
        }

        [Test]
        public async Task Handle_CreatesRecordWithCorrectLocation()
        {
            // Arrange
            TimeClockRecord capturedRecord = null;
            _mockTimeClockService.Setup(s => s.GetNextRecordTypeAsync(_requestDto.EmployeeId))
                .ReturnsAsync(RecordType.Entry);
            _mockWebcamService.Setup(w => w.CaptureAndSaveImage(_requestDto.EmployeeId))
                .Returns(@"C:\temp\photo.jpg");
            _mockApiClient.Setup(a => a.RegisterPointAsync(It.IsAny<TimeClockRecord>()))
                .ReturnsAsync(true);
            _mockRepository.Setup(r => r.AddAsync(It.IsAny<TimeClockRecord>()))
                .Callback<TimeClockRecord>(r => capturedRecord = r)
                .Returns(Task.CompletedTask);

            // Act
            await _handler.Handle(_command, CancellationToken.None);

            // Assert
            Assert.AreEqual("Escritório Principal (Simulado)", capturedRecord.Location);
        }
    }
}
