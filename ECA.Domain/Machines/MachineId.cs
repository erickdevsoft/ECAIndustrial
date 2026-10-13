namespace ECA.Domain.Machines;

/// <summary>
/// Representa la identidad única de una máquina industrial.
/// Utiliza un tipo específico para evitar confundir este identificador
/// con los de otros conceptos del dominio.
/// </summary>
/// <param name="Value">
/// Identificador GUID asociado a la máquina.
/// </param>
public readonly record struct MachineId(Guid Value)
{
    /// <summary>
    /// Genera una nueva identidad para una máquina.
    /// </summary>
    /// <returns>Un nuevo identificador de máquina.</returns>
    public static MachineId New()
        => new(Guid.NewGuid());
}
