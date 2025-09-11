using MassTransit;
using Microsoft.Extensions.Logging;
using TimeClockSystem.Core.Interfaces;

namespace TimeClockSystem.Infrastructure.Menssaging
{
    public class MassTransitEventPublisher : IEventPublisher
    {
        private readonly IBus _bus;
        private readonly ILogger<MassTransitEventPublisher> _logger;

        public MassTransitEventPublisher(IBus bus, ILogger<MassTransitEventPublisher> logger)
        {
            _bus = bus;
            _logger = logger;
        }

        public async Task PublishEventAsync(object payload, string eventType, CancellationToken cancellationToken = default)
        {
            try
            {
                await _bus.Publish(payload, cancellationToken);
                _logger.LogInformation("Evento do tipo {EventType} publicado com sucesso via MassTransit.", payload.GetType().Name);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao publicar evento do tipo {EventType} via MassTransit.", payload.GetType().Name);
            }
        }
    }
}
