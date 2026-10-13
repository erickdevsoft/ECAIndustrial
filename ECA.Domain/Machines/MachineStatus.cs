namespace ECA.Domain.Machines;

/// <summary>
/// Representa los posibles estados operativos de una máquina industrial.
/// </summary>
public enum MachineStatus
{
    /// <summary>La máquina está desconectada.</summary>
    Offline = 1,

    /// <summary>La máquina está disponible, pero no produce.</summary>
    Idle = 2,

    /// <summary>La máquina está ejecutando producción.</summary>
    Running = 3,

    /// <summary>La máquina se encuentra detenida.</summary>
    Stopped = 4,

    /// <summary>La máquina está en mantenimiento.</summary>
    Maintenance = 5,

    /// <summary>La máquina ha registrado un fallo.</summary>
    Faulted = 6
}
