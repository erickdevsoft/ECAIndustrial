namespace ECA.Domain.ProductionLines;

/// <summary>
/// Identidad de una línea de producción.
///
/// Se utiliza un tipo específico en lugar de trabajar directamente
/// con Guid para evitar mezclar accidentalmente identificadores
/// de distintos conceptos del dominio.
/// </summary>
public readonly record struct ProductionLineId(Guid Value)
{
    /// <summary>
    /// Genera una nueva identidad para una línea de producción.
    /// </summary>
    public static ProductionLineId New()
        => new(Guid.NewGuid());
}
