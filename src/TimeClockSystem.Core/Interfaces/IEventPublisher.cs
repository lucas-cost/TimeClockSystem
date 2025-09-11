namespace TimeClockSystem.Core.Interfaces
{
    public interface IEventPublisher
    {
        // A assinatura do método permanece a mesma, agora será implementada pelo MassTransit.
        Task PublishEventAsync(object payload, string eventType, CancellationToken cancellationToken = default);
    }
}
