namespace ECA.Domain.Machines.Alarms;

/// <summary>
/// Define los niveles de gravedad de las alarmas industriales.
/// </summary>
public enum AlarmSeverity
{
    /// <summary>
    /// Incidencia informativa que no requiere necesariamente intervención.
    /// </summary>
    Info = 1,

    /// <summary>
    /// Condición que requiere atención, aunque no implique una parada inmediata.
    /// </summary>
    Warning = 2,

    /// <summary>
    /// Condición grave que puede requerir intervención inmediata.
    /// </summary>
    Critical = 3
}
