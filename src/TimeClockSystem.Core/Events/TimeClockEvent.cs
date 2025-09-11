namespace TimeClockSystem.Core.Events
{
    // Interface que pode ser usada por todos os eventos de ponto
    public interface TimeClockEvent
    {
        public DateTime EventTimestamp { get; }
    }

    // Mensagem para um registro bem-sucedido
    public record PointRegisteredSuccessfullyEvent : TimeClockEvent
    {
        public Guid RecordId { get; init; }
        public string EmployeeId { get; init; }
        public DateTime EventTimestamp { get; init; } = DateTime.UtcNow;
    }

    // Mensagem para uma falha no registro
    public record PointRegistrationFailedEvent : TimeClockEvent
    {
        public string EmployeeId { get; init; }
        public string ErrorMessage { get; init; }
        public DateTime EventTimestamp { get; init; } = DateTime.UtcNow;
    }
}
