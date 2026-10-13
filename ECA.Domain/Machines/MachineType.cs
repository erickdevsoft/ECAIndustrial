namespace ECA.Domain.Machines;

/// <summary>
/// Define los tipos de máquinas industriales admitidos por ECA Industrial.
/// </summary>
public enum MachineType
{
    /// <summary>Máquina de control numérico por ordenador.</summary>
    Cnc = 1,

    /// <summary>Robot industrial.</summary>
    Robot = 2,

    /// <summary>Máquina destinada a tareas de inspección.</summary>
    Inspection = 3,

    /// <summary>Sistema de transporte de materiales.</summary>
    Conveyor = 4
}
