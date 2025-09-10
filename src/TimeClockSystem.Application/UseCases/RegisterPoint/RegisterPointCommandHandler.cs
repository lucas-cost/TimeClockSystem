using MediatR;
using TimeClockSystem.Application.Interfaces;
using TimeClockSystem.Core.Entities;
using TimeClockSystem.Core.Enums;
using TimeClockSystem.Core.Exceptions;
using TimeClockSystem.Core.Interfaces;
using TimeClockSystem.Core.Services;

namespace TimeClockSystem.Application.UseCases.RegisterPoint
{
    public class RegisterPointCommandHandler : IRequestHandler<RegisterPointCommand, RegisterPointResult>
    {
        private readonly ITimeClockRepository _repository;
        private readonly IApiClient _apiClient;
        private readonly IWebcamService _webcamService;
        private readonly ITimeClockService _timeClockService;

        public RegisterPointCommandHandler(
            ITimeClockRepository repository,
            IApiClient apiClient,
            IWebcamService webcamService,
            ITimeClockService timeClockService)
        {
            _repository = repository;
            _apiClient = apiClient;
            _webcamService = webcamService;
            _timeClockService = timeClockService; 
        }

        public async Task<RegisterPointResult> Handle(RegisterPointCommand request, CancellationToken cancellationToken)
        {
            try
            {
                RecordType recordType = await _timeClockService.GetNextRecordTypeAsync(request.PointData.EmployeeId);

                string photoPath = _webcamService.CaptureAndSaveImage(request.PointData.EmployeeId);

                TimeClockRecord record = new TimeClockRecord
                {
                    Id = Guid.NewGuid(),
                    EmployeeId = request.PointData.EmployeeId,
                    Timestamp = DateTime.Now,
                    Type = recordType,
                    Location = "Escritório Principal (Simulado)",
                    PhotoPath = photoPath,
                    Status = SyncStatus.Pending
                };

                await _repository.AddAsync(record);

                bool synced = await _apiClient.RegisterPointAsync(record);

                if (synced)
                    await _repository.UpdateStatusAsync(record.Id, SyncStatus.Synced);
                
                return new RegisterPointResult { Success = true, CreatedRecordType = record.Type };
            }
            catch (BusinessRuleException ex)
            {
                Console.WriteLine($"Falha na regra de negócio: {ex.Message}");
                return new RegisterPointResult { Success = false, ErrorMessage = ex.Message };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Falha grave ao registrar ponto: {ex.Message}");
                return new RegisterPointResult { Success = false, ErrorMessage = "Ocorreu uma falha inesperada ao registrar o ponto." };
            }
        }
    }
}