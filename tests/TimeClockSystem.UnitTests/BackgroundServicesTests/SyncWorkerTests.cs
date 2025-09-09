using Microsoft.Extensions.DependencyInjection;
using Moq;
using TimeClockSystem.BackgroundServices;
using TimeClockSystem.Core.Entities;
using TimeClockSystem.Core.Enums;
using TimeClockSystem.Core.Interfaces;

namespace TimeClockSystem.UnitTests.BackgroundServicesTests
{
    [TestFixture]
    public class SyncWorkerTests
    {
        private CancellationTokenSource _cancellationTokenSource;
        private Mock<IServiceProvider> _mockServiceProvider;
        private Mock<IServiceScope> _mockServiceScope;
        private Mock<IServiceScopeFactory> _mockServiceScopeFactory;
        private Mock<ITimeClockRepository> _mockRepository;
        private Mock<IApiClient> _mockApiClient;
        private SyncWorker _syncWorker;

        private readonly Guid _recordId1 = Guid.NewGuid();
        private readonly Guid _recordId2 = Guid.NewGuid();
        private readonly Guid _recordId3 = Guid.NewGuid();

        [SetUp]
        public void Setup()
        {
            _mockServiceProvider = new Mock<IServiceProvider>();
            _mockServiceScope = new Mock<IServiceScope>();
            _mockServiceScopeFactory = new Mock<IServiceScopeFactory>();
            _mockRepository = new Mock<ITimeClockRepository>();
            _mockApiClient = new Mock<IApiClient>();
            _cancellationTokenSource = new CancellationTokenSource();

            _mockServiceScopeFactory.Setup(x => x.CreateScope())
                .Returns(_mockServiceScope.Object);

            _mockServiceProvider.Setup(x => x.GetService(typeof(IServiceScopeFactory)))
                .Returns(_mockServiceScopeFactory.Object);

            _mockServiceScope.Setup(x => x.ServiceProvider)
                .Returns(_mockServiceProvider.Object);

            _mockServiceProvider.Setup(x => x.GetService(typeof(ITimeClockRepository)))
                .Returns(_mockRepository.Object);

            _mockServiceProvider.Setup(x => x.GetService(typeof(IApiClient)))
                .Returns(_mockApiClient.Object);

            _syncWorker = new SyncWorker(_mockServiceProvider.Object);
        }

        [TearDown]
        public void TearDown()
        {
            _syncWorker?.Dispose();
            _cancellationTokenSource.Dispose();
        }

        [Test]
        public async Task ExecuteAsync_WhenNoPendingRecords_DoesNotCallApi()
        {
            // Arrange
            List<TimeClockRecord> emptyRecords = [];
            _mockRepository.Setup(r => r.GetPendingSyncRecordsAsync())
                .ReturnsAsync(emptyRecords);

            // Act - Simula uma única execução do loop
            await _syncWorker.StartAsync(_cancellationTokenSource.Token);
            await Task.Delay(100); // Aguarda um pouco para a execução
            _cancellationTokenSource.Cancel();

            // Assert
            _mockApiClient.Verify(a => a.RegisterPointAsync(It.IsAny<TimeClockRecord>()), Times.Never);
        }

        [Test]
        public async Task ExecuteAsync_WithPendingRecords_SyncsSuccessfully()
        {
            // Arrange
            List<TimeClockRecord> pendingRecords = new List<TimeClockRecord>
            {
                new TimeClockRecord { Id = _recordId1, EmployeeId = "123", Type = RecordType.Entry }
            };

            _mockRepository.Setup(r => r.GetPendingSyncRecordsAsync())
                .ReturnsAsync(pendingRecords);

            _mockApiClient.Setup(a => a.RegisterPointAsync(It.IsAny<TimeClockRecord>()))
                .ReturnsAsync(true);

            // Act
            await _syncWorker.StartAsync(_cancellationTokenSource.Token);
            await Task.Delay(100);
            _cancellationTokenSource.Cancel();

            // Assert
            _mockRepository.Verify(r => r.UpdateStatusAsync(_recordId1, SyncStatus.Synced), Times.Once);
        }

        [Test]
        public async Task ExecuteAsync_WhenApiCallFails_DoesNotUpdateStatus()
        {
            // Arrange
            List<TimeClockRecord> pendingRecords = new List<TimeClockRecord>
            {
                new TimeClockRecord { Id = _recordId1, EmployeeId = "123", Type = RecordType.Entry }
            };

            _mockRepository.Setup(r => r.GetPendingSyncRecordsAsync())
                .ReturnsAsync(pendingRecords);

            _mockApiClient.Setup(a => a.RegisterPointAsync(It.IsAny<TimeClockRecord>()))
                .ReturnsAsync(false);

            // Act
            await _syncWorker.StartAsync(_cancellationTokenSource.Token);
            await Task.Delay(100);
            _cancellationTokenSource.Cancel();

            // Assert
            _mockRepository.Verify(r => r.UpdateStatusAsync(It.IsAny<Guid>(), It.IsAny<SyncStatus>()), Times.Never);
        }

        [Test]
        public async Task ExecuteAsync_DisposesServiceScope()
        {
            // Arrange
            List<TimeClockRecord> emptyRecords = [];
            _mockRepository.Setup(r => r.GetPendingSyncRecordsAsync())
                .ReturnsAsync(emptyRecords);

            // Act
            await _syncWorker.StartAsync(_cancellationTokenSource.Token);
            await Task.Delay(100);
            _cancellationTokenSource.Cancel();

            // Assert
            _mockServiceScope.Verify(x => x.Dispose(), Times.AtLeastOnce);
        }

        [Test]
        public async Task ExecuteAsync_WithMixedSuccessAndFailure_UpdatesOnlySuccessfulOnes()
        {
            // Arrange
            List<TimeClockRecord> pendingRecords = new List<TimeClockRecord>
            {
                new TimeClockRecord { Id = _recordId1, EmployeeId = "123" },
                new TimeClockRecord { Id = _recordId2, EmployeeId = "456" },
                new TimeClockRecord { Id = _recordId3, EmployeeId = "789" }
            };

            _mockRepository.Setup(r => r.GetPendingSyncRecordsAsync())
                .ReturnsAsync(pendingRecords);

            // Configura sequência de respostas
            var callCount = 0;
            _mockApiClient.Setup(a => a.RegisterPointAsync(It.IsAny<TimeClockRecord>()))
                .ReturnsAsync(() =>
                {
                    callCount++;
                    return callCount switch
                    {
                        1 => true,    
                        2 => false,   
                        3 => true,  
                        _ => false
                    };
                });

            // Act
            await _syncWorker.StartAsync(_cancellationTokenSource.Token);
            await Task.Delay(100);
            _cancellationTokenSource.Cancel();

            // Assert
            _mockRepository.Verify(r => r.UpdateStatusAsync(_recordId1, SyncStatus.Synced), Times.Once);
            _mockRepository.Verify(r => r.UpdateStatusAsync(_recordId3, SyncStatus.Synced), Times.Once);
            _mockRepository.Verify(r => r.UpdateStatusAsync(_recordId2, It.IsAny<SyncStatus>()), Times.Never);
        }
    }
}
