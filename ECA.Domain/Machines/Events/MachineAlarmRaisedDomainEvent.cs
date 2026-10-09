using ECA.Domain.Common.Events;
using ECA.Domain.Machines.Alarms;

namespace ECA.Domain.Machines.Events;

/// <summary>
/// Representa el hecho de que una máquina ha generado una alarma.
///
/// El evento contiene una copia de los datos relevantes del momento
/// en que ocurrió. No necesita conservar una referencia a la alarma.
/// </summary>
public sealed record MachineAlarmRaisedDomainEvent(
    MachineId MachineId,
    MachineAlarmId AlarmId,
    string Code,
    string Message,
    AlarmSeverity Severity,
    DateTimeOffset OccurredOn) : IDomainEvent;
