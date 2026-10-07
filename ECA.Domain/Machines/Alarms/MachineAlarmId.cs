namespace ECA.Domain.Machines.Alarms;

/// <summary>
/// Identidad de una alarma de máquina.
///
/// La identidad permite distinguir una alarma de otra aunque
/// tengan el mismo código o mensaje.
/// </summary>
public readonly record struct MachineAlarmId(Guid Value)
{
    /// <summary>
    /// Genera una nueva identidad para una alarma.
    /// </summary>
    public static MachineAlarmId New()
        => new(Guid.NewGuid());
}
