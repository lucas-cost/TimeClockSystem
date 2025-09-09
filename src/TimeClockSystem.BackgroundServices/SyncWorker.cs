using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TimeClockSystem.Core.Enums;
using TimeClockSystem.Core.Interfaces;

namespace TimeClockSystem.BackgroundServices
{
    public class SyncWorker : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;

        public SyncWorker(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    // Cria um escopo para resolver serviços com tempo de vida 'scoped'
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var repository = scope.ServiceProvider.GetRequiredService<ITimeClockRepository>();
                        var apiClient = scope.ServiceProvider.GetRequiredService<IApiClient>();

                        var pendingRecords = await repository.GetPendingSyncRecordsAsync();
                        foreach (var record in pendingRecords)
                        {
                            bool success = await apiClient.RegisterPointAsync(record);
                            if (success)
                            {
                                await repository.UpdateStatusAsync(record.Id, SyncStatus.Synced);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Erro no worker de sincronização: {ex.Message}");
                }

                // Aguarda 1 minuto antes da próxima tentativa
                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }
        }
    }
}
