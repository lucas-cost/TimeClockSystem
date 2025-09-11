using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TimeClockSystem.Core.Entities;
using TimeClockSystem.Core.Enums;
using TimeClockSystem.Core.Interfaces;

namespace TimeClockSystem.BackgroundServices
{
    public class SyncWorker : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<SyncWorker> _logger;

        public SyncWorker(IServiceProvider serviceProvider, ILogger<SyncWorker> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("SyncWorker iniciado.");

            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Iniciando ciclo de sincronização...");

                try
                {
                    using (IServiceScope scope = _serviceProvider.CreateScope())
                    {
                        ITimeClockRepository repository = scope.ServiceProvider.GetRequiredService<ITimeClockRepository>();
                        IApiClient apiClient = scope.ServiceProvider.GetRequiredService<IApiClient>();

                        IEnumerable<TimeClockRecord> pendingRecords = await repository.GetPendingSyncRecordsAsync();
                        int pendingCount = pendingRecords.Count();

                        if (pendingCount > 0)
                        {
                            _logger.LogInformation("Encontrados {PendingCount} registros pendentes para sincronização.", pendingCount);

                            foreach (TimeClockRecord record in pendingRecords)
                            {
                                bool success = await apiClient.RegisterPointAsync(record);
                                if (success)
                                {
                                    await repository.UpdateStatusAsync(record.Id, SyncStatus.Synced);
                                    _logger.LogInformation("Registro {RecordId} do funcionário {EmployeeId} sincronizado com sucesso.", record.Id, record.EmployeeId);
                                }
                                else
                                {
                                    // Log de aviso se a sincronização de um registro específico falhar
                                    _logger.LogWarning("Falha ao sincronizar o registro {RecordId} do funcionário {EmployeeId}.", record.Id, record.EmployeeId);
                                }
                            }
                        }
                        else
                        {
                            _logger.LogInformation("Nenhum registro pendente encontrado.");
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Ocorreu um erro inesperado durante o ciclo de sincronização.");
                }

                _logger.LogInformation("Ciclo de sincronização concluído. Aguardando próximo ciclo.");
                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }

            _logger.LogInformation("SyncWorker finalizado.");
        }
    }
}