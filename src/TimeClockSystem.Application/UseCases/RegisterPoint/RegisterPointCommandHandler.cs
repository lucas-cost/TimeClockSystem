using MediatR;
using Microsoft.Extensions.Logging;
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
        private readonly ILogger<RegisterPointCommandHandler> _logger;

        public RegisterPointCommandHandler(
            ITimeClockRepository repository,
            IApiClient apiClient,
            IWebcamService webcamService,
            ITimeClockService timeClockService,
            ILogger<RegisterPointCommandHandler> logger)
        {
            _repository = repository;
            _apiClient = apiClient;
            _webcamService = webcamService;
            _timeClockService = timeClockService; 
            _logger = logger;
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
            catch (ImageQualityException ex)
            {
                _logger.LogWarning("Falha na qualidade da imagem para {EmployeeId}: {Message}", request.PointData.EmployeeId, ex.Message);
                return new RegisterPointResult { Success = false, ErrorMessage = ex.Message };
            }
            catch (BusinessRuleException ex)
            {
                _logger.LogWarning("Falha na regra de negócio ao registrar ponto para {EmployeeId}: {Message}", request.PointData.EmployeeId, ex.Message);
                return new RegisterPointResult { Success = false, ErrorMessage = ex.Message };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Falha grave ao registrar ponto para o funcionário {EmployeeId}", request.PointData.EmployeeId);
                return new RegisterPointResult { Success = false, ErrorMessage = "Ocorreu uma falha inesperada ao registrar o ponto." };
            }
        }
    }
}