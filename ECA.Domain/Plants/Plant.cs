using ECA.Domain.Common;
using ECA.Domain.ProductionLines;

namespace ECA.Domain.Plants;

/// <summary>
/// Representa una planta industrial dentro del dominio.
///
/// Plant es un Aggregate Root independiente de ProductionLine.
///
/// La planta mantiene qué líneas forman parte de ella, pero no contiene
/// las entidades ProductionLine completas. Esto permite mantener límites
/// claros entre los diferentes Aggregates.
/// </summary>
public sealed class Plant
{
    /// <summary>
    /// Colección privada con los identificadores de las líneas
    /// pertenecientes a esta planta.
    ///
    /// La colección no se expone directamente para evitar que código
    /// externo pueda modificar la relación sin pasar por las reglas
    /// de negocio de Plant.
    /// </summary>
    private readonly List<ProductionLineId> _productionLineIds = [];

    /// <summary>
    /// Constructor privado para garantizar que la creación de Plant
    /// pase por el método Create(), donde se aplican las validaciones.
    /// </summary>
    private Plant(
        PlantId id,
        string code,
        string name)
    {
        Id = id;
        Code = code;
        Name = name;
    }

    /// <summary>
    /// Identidad de la planta.
    /// </summary>
    public PlantId Id { get; }

    /// <summary>
    /// Código utilizado para identificar la planta.
    ///
    /// Ejemplos:
    /// PLANT-01
    /// BCN-FACTORY
    /// </summary>
    public string Code { get; private set; }

    /// <summary>
    /// Nombre descriptivo de la planta.
    /// </summary>
    public string Name { get; private set; }

    /// <summary>
    /// Identificadores de las líneas de producción asociadas
    /// a esta planta.
    ///
    /// Exponemos solamente una colección de lectura para impedir
    /// modificaciones directas desde fuera del Aggregate Root.
    /// </summary>
    public IReadOnlyCollection<ProductionLineId> ProductionLineIds
        => _productionLineIds.AsReadOnly();

    /// <summary>
    /// Crea una nueva planta aplicando las reglas necesarias
    /// para garantizar que no nazca en un estado inválido.
    /// </summary>
    public static Plant Create(
        string code,
        string name)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            throw new DomainException(
                "El código de la planta es obligatorio.");
        }

        if (code.Length > 50)
        {
            throw new DomainException(
                "El código de la planta no puede superar los 50 caracteres.");
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new DomainException(
                "El nombre de la planta es obligatorio.");
        }

        if (name.Length > 100)
        {
            throw new DomainException(
                "El nombre de la planta no puede superar los 100 caracteres.");
        }

        return new Plant(
            PlantId.New(),
            code.Trim(),
            name.Trim());
    }

    /// <summary>
    /// Asocia una línea de producción con la planta.
    ///
    /// Plant solamente guarda el identificador de ProductionLine.
    /// No mantiene una referencia al objeto completo.
    /// </summary>
    public void AddProductionLine(ProductionLineId productionLineId)
    {
        if (productionLineId.Value == Guid.Empty)
        {
            throw new DomainException(
                "El identificador de la línea de producción no puede estar vacío.");
        }

        if (_productionLineIds.Contains(productionLineId))
        {
            throw new DomainException(
                "La línea de producción ya está asociada a la planta.");
        }

        _productionLineIds.Add(productionLineId);
    }

    /// <summary>
    /// Elimina una línea de producción de la planta.
    ///
    /// La modificación pasa por Plant para mantener su encapsulación.
    /// </summary>
    public void RemoveProductionLine(
        ProductionLineId productionLineId)
    {
        if (!_productionLineIds.Remove(productionLineId))
        {
            throw new DomainException(
                "La línea de producción no está asociada a la planta.");
        }
    }
}
