namespace ECA.Domain.Machines.Alarms;

/// <summary>
/// Representa una alarma generada por una máquina.
///
/// MachineAlarm es una Entity perteneciente al Aggregate Machine.
/// No se manipula directamente desde fuera del agregado.
/// </summary>
public sealed class MachineAlarm
{
    private MachineAlarm(
        MachineAlarmId id,
        string code,
        string message,
        AlarmSeverity severity)
    {
        Id = id;
        Code = code;
        Message = message;
        Severity = severity;
        RaisedAt = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// Identidad única de la alarma.
    /// </summary>
    public MachineAlarmId Id { get; }

    /// <summary>
    /// Código técnico de la alarma.
    ///
    /// Ejemplo: TEMP-001
    /// </summary>
    public string Code { get; }

    /// <summary>
    /// Descripción de la condición que provocó la alarma.
    /// </summary>
    public string Message { get; }

    /// <summary>
    /// Gravedad de la alarma.
    /// </summary>
    public AlarmSeverity Severity { get; }

    /// <summary>
    /// Momento en el que se generó la alarma.
    /// </summary>
    public DateTimeOffset RaisedAt { get; }

    /// <summary>
    /// Indica si la alarma sigue activa.
    ///
    /// Una alarma recién generada está activa.
    /// </summary>
    public bool IsActive { get; private set; } = true;

    /// <summary>
    /// Crea una nueva alarma.
    ///
    /// La validación del contenido propio de la alarma
    /// pertenece a esta Entity.
    /// </summary>
    public static MachineAlarm Create(
        string code,
        string message,
        AlarmSeverity severity)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            throw new ArgumentException(
                "El código de la alarma es obligatorio.",
                nameof(code));
        }

        if (string.IsNullOrWhiteSpace(message))
        {
            throw new ArgumentException(
                "El mensaje de la alarma es obligatorio.",
                nameof(message));
        }

        // Comprobamos que la gravedad corresponde a un valor
        // declarado en AlarmSeverity.
        // Un enum puede contener valores numéricos no definidos.
        if (!Enum.IsDefined(severity))
        {
            throw new ArgumentOutOfRangeException(
                nameof(severity),
                severity,
                "La gravedad de la alarma no es válida.");
        }

        return new MachineAlarm(
            MachineAlarmId.New(),
            code.Trim(),
            message.Trim(),
            severity);
    }

    /// <summary>
    /// Desactiva la alarma.
    ///
    /// Este método solo puede utilizarse desde el ensamblado del dominio.
    /// La modificación pública debe realizarse a través de Machine.
    /// </summary>
    internal void Clear()
    {
        IsActive = false;
    }
}
