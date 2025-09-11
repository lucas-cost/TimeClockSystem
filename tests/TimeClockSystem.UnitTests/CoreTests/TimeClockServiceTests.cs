using Microsoft.Extensions.Logging;
using Moq;
using TimeClockSystem.Core.Entities;
using TimeClockSystem.Core.Enums;
using TimeClockSystem.Core.Exceptions;
using TimeClockSystem.Core.Interfaces;
using TimeClockSystem.Core.Services;

namespace TimeClockSystem.UnitTests.CoreTests
{
    [TestFixture]
    public class TimeClockServiceTests
    {
        private Mock<ITimeClockRepository> _mockRepository;
        private TimeClockService _timeClockService;
        private Mock<ILogger<TimeClockService>> _mockLogger;

        [SetUp]
        public void Setup()
        {
            _mockRepository = new Mock<ITimeClockRepository>();
            _mockLogger = new Mock<ILogger<TimeClockService>>();
            _timeClockService = new TimeClockService(_mockRepository.Object, _mockLogger.Object);
        }

        [Test]
        public async Task GetNextRecordTypeAsync_NoRecordsForToday_ReturnsEntry()
        {
            // Arrange
            string employeeId = "123";
            List<TimeClockRecord> emptyRecords = [];

            _mockRepository.Setup(r => r.GetTodaysRecordsForEmployeeAsync(employeeId))
                .ReturnsAsync(emptyRecords);

            // Act
            RecordType result = await _timeClockService.GetNextRecordTypeAsync(employeeId);

            // Assert
            Assert.That(result, Is.EqualTo(RecordType.Entry));
        }

        [Test]
        public async Task GetNextRecordTypeAsync_LastRecordIsExit_ReturnsEntry()
        {
            // Arrange
            string employeeId = "123";
            List<TimeClockRecord> records =
            [
                new TimeClockRecord { Type = RecordType.Entry, Timestamp = DateTime.Now.AddHours(-9) },
                new TimeClockRecord { Type = RecordType.Exit, Timestamp = DateTime.Now.AddHours(-1) }
            ];

            _mockRepository.Setup(r => r.GetTodaysRecordsForEmployeeAsync(employeeId))
                .ReturnsAsync(records);

            // Act
            RecordType result = await _timeClockService.GetNextRecordTypeAsync(employeeId);

            // Assert
            Assert.That(result, Is.EqualTo(RecordType.Entry));
        }

        [Test]
        public async Task GetNextRecordTypeAsync_LastRecordIsEntry_ReturnsBreakStart()
        {
            // Arrange
            string employeeId = "123";
            List<TimeClockRecord> records =
            [
                new TimeClockRecord { Type = RecordType.Entry, Timestamp = DateTime.Now.AddHours(-2) }
            ];

            _mockRepository.Setup(r => r.GetTodaysRecordsForEmployeeAsync(employeeId))
                .ReturnsAsync(records);

            // Act
            RecordType result = await _timeClockService.GetNextRecordTypeAsync(employeeId);

            // Assert
            Assert.That(result, Is.EqualTo(RecordType.BreakStart));
        }

        [Test]
        public async Task GetNextRecordTypeAsync_LastRecordIsBreakStart_ReturnsBreakEnd()
        {
            // Arrange
            string employeeId = "123";
            List<TimeClockRecord> records =
            [
                new TimeClockRecord { Type = RecordType.Entry, Timestamp = DateTime.Now.AddHours(-3) },
                new TimeClockRecord { Type = RecordType.BreakStart, Timestamp = DateTime.Now.AddHours(-1) }
            ];

            _mockRepository.Setup(r => r.GetTodaysRecordsForEmployeeAsync(employeeId))
                .ReturnsAsync(records);

            // Act
            RecordType result = await _timeClockService.GetNextRecordTypeAsync(employeeId);

            // Assert
            Assert.That(result, Is.EqualTo(RecordType.BreakEnd));
        }

        [Test]
        public async Task GetNextRecordTypeAsync_LastRecordIsBreakEndWithSufficientWorkHours_ReturnsExit()
        {
            // Arrange
            string employeeId = "123";
            List<TimeClockRecord> records =
            [
                new TimeClockRecord { Type = RecordType.Entry, Timestamp = DateTime.Now.AddHours(-9) },
                new TimeClockRecord { Type = RecordType.BreakStart, Timestamp = DateTime.Now.AddHours(-5) },
                new TimeClockRecord { Type = RecordType.BreakEnd, Timestamp = DateTime.Now.AddHours(-4) }
            ];

            _mockRepository.Setup(r => r.GetTodaysRecordsForEmployeeAsync(employeeId))
                .ReturnsAsync(records);

            // Act
            RecordType result = await _timeClockService.GetNextRecordTypeAsync(employeeId);

            // Assert
            Assert.That(result, Is.EqualTo(RecordType.Exit));
        }

        [Test]
        public async Task GetNextRecordTypeAsync_LastRecordIsBreakEndWithInsufficientWorkHours_ThrowsBusinessRuleException()
        {
            // Arrange
            string employeeId = "123";
            List<TimeClockRecord> records =
            [
                new TimeClockRecord { Type = RecordType.Entry, Timestamp = DateTime.Now.AddHours(-7) }, // 7 horas atrás
                new TimeClockRecord { Type = RecordType.BreakStart, Timestamp = DateTime.Now.AddHours(-3) }, // 3 horas atrás
                new TimeClockRecord { Type = RecordType.BreakEnd, Timestamp = DateTime.Now.AddHours(-2) } // 2 horas atrás
            ];

            _mockRepository.Setup(r => r.GetTodaysRecordsForEmployeeAsync(employeeId))
                .ReturnsAsync(records);

            // Act & Assert
            BusinessRuleException ex = Assert.ThrowsAsync<BusinessRuleException>(async () =>
                await _timeClockService.GetNextRecordTypeAsync(employeeId));

            Assert.That(ex.Message, Does.Contain("Jornada mínima é de 8 horas"));
            Assert.That(ex.Message, Does.Contain("Total trabalhado:"));
        }

        [Test]
        public async Task GetNextRecordTypeAsync_InvalidRecordSequence_ThrowsInvalidOperationException()
        {
            // Arrange
            string employeeId = "123";
            List<TimeClockRecord> records =
            [
                new TimeClockRecord { Type = (RecordType)999, Timestamp = DateTime.Now } // Tipo inválido
            ];

            _mockRepository.Setup(r => r.GetTodaysRecordsForEmployeeAsync(employeeId))
                .ReturnsAsync(records);

            // Act & Assert
            Assert.ThrowsAsync<InvalidOperationException>(async () =>
                await _timeClockService.GetNextRecordTypeAsync(employeeId));
        }
    }
}
