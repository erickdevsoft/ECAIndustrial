using ECA.Domain.Common.Events;

namespace ECA.Domain.Machines.Events;

/// <summary>
/// Evento de dominio generado cuando una máquina cambia de estado.
///
/// Este evento no ejecuta ninguna acción por sí mismo.
/// Solamente contiene la información necesaria para describir
/// lo que ocurrió.
/// </summary>
public sealed record MachineStatusChangedDomainEvent(
    MachineId MachineId,
    MachineStatus PreviousStatus,
    MachineStatus NewStatus,
    DateTimeOffset OccurredOn) : IDomainEvent;
