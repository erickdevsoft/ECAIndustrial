using ECA.Domain.Common.Events;
using ECA.Domain.Machines.Alarms;

namespace ECA.Domain.Machines.Events;

/// <summary>
/// Representa el hecho de que una alarma de máquina
/// ha sido desactivada.
/// </summary>
public sealed record MachineAlarmClearedDomainEvent(
    MachineId MachineId,
    MachineAlarmId AlarmId,
    string Code,
    string Message,
    AlarmSeverity Severity,
    DateTimeOffset OccurredOn) : IDomainEvent;
