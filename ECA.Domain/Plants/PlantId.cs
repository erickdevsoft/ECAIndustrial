namespace ECA.Domain.Plants;

/// <summary>
/// Representa la identidad de una planta industrial.
///
/// Utilizamos un tipo específico para evitar trabajar directamente
/// con Guid por todo el dominio y reducir el riesgo de mezclar
/// identificadores de conceptos diferentes.
/// </summary>
public readonly record struct PlantId(Guid Value)
{
    /// <summary>
    /// Genera una nueva identidad para una planta.
    /// </summary>
    public static PlantId New()
        => new(Guid.NewGuid());
}
