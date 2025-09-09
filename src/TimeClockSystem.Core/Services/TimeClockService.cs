using TimeClockSystem.Core.Entities;
using TimeClockSystem.Core.Enums;
using TimeClockSystem.Core.Exceptions;
using TimeClockSystem.Core.Interfaces;

namespace TimeClockSystem.Core.Services
{
    public class TimeClockService
    {
        private readonly ITimeClockRepository _repository;
        private const double MinWorkHoursForExit = 8.0;

        public TimeClockService(ITimeClockRepository repository)
        {
            _repository = repository;
        }

        public async Task<RecordType> GetNextRecordTypeAsync(string employeeId)
        {
            List<TimeClockRecord> todaysRecords = await _repository.GetTodaysRecordsForEmployeeAsync(employeeId);
            TimeClockRecord? lastRecord = todaysRecords.LastOrDefault();

            if (lastRecord == null || lastRecord.Type == RecordType.Exit)           
                return RecordType.Entry;


            return lastRecord.Type switch
            {
                RecordType.Entry => RecordType.BreakStart,
                RecordType.BreakStart => RecordType.BreakEnd,
                RecordType.BreakEnd => ValidateAndReturnExit(todaysRecords),
                _ => throw new InvalidOperationException("Não foi possível determinar o próximo tipo de registro de ponto.")
            };
        }

        private RecordType ValidateAndReturnExit(List<TimeClockRecord> todaysRecords)
        {
            double workedHours = CalculateWorkedHours(todaysRecords);

            if (workedHours < MinWorkHoursForExit)
                throw new BusinessRuleException(
                    $"Jornada mínima é de {MinWorkHoursForExit} horas não cumprida. Total trabalhado: {workedHours:F2} horas."
                );

            return RecordType.Exit;
        }

        private double CalculateWorkedHours(List<TimeClockRecord> todaysRecords)
        {
            if (todaysRecords == null || todaysRecords.Count == 0)
                return 0.0;

            TimeSpan totalWorkedTime = TimeSpan.Zero;
            DateTime? entryTime = null;

            foreach (var record in todaysRecords)
            {
                switch (record.Type)
                {
                    case RecordType.Entry:
                    case RecordType.BreakEnd:
                        entryTime = record.Timestamp;
                        break;

                    case RecordType.BreakStart:
                    case RecordType.Exit:
                        if (entryTime.HasValue)
                        {
                            totalWorkedTime += record.Timestamp - entryTime.Value;
                            entryTime = null;
                        }
                        break;
                }
            }

            if (entryTime.HasValue)
                totalWorkedTime += DateTime.Now - entryTime.Value;

            return totalWorkedTime.TotalHours;
        }
    }
}
