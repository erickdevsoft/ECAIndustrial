using ECA.Domain.Common;
using ECA.Domain.Machines;

namespace ECA.Domain.ProductionLines;

/// <summary>
/// Representa una línea de producción.
///
/// ProductionLine es un Aggregate Root independiente de Machine.
/// Su responsabilidad es mantener la relación entre la línea
/// y las máquinas que forman parte de ella.
///
/// La línea conoce las identidades de las máquinas, pero no controla
/// el estado interno de dichas máquinas.
/// </summary>
public sealed class ProductionLine
{
    /// <summary>
    /// Colección interna de identificadores de máquinas pertenecientes
    /// a esta línea.
    ///
    /// La colección es privada para impedir que código externo pueda
    /// modificar directamente las máquinas asociadas a la línea.
    /// </summary>
    private readonly List<MachineId> _machineIds = [];

    /// <summary>
    /// Constructor privado para garantizar que las líneas se creen
    /// mediante Create(), donde se aplican las validaciones del dominio.
    /// </summary>
    private ProductionLine(
        ProductionLineId id,
        string code,
        string name)
    {
        Id = id;
        Code = code;
        Name = name;
    }

    /// <summary>
    /// Identidad de la línea de producción.
    /// </summary>
    public ProductionLineId Id { get; }

    /// <summary>
    /// Código utilizado para identificar la línea dentro de la planta.
    ///
    /// Ejemplos:
    /// LINE-01
    /// LINE-MACHINING-01
    /// </summary>
    public string Code { get; private set; }

    /// <summary>
    /// Nombre descriptivo de la línea.
    /// </summary>
    public string Name { get; private set; }

    /// <summary>
    /// Identificadores de las máquinas asociadas a la línea.
    ///
    /// Exponemos una colección de solo lectura para que otras capas
    /// puedan consultar la relación sin poder modificarla directamente.
    /// </summary>
    public IReadOnlyCollection<MachineId> MachineIds
        => _machineIds.AsReadOnly();

    /// <summary>
    /// Crea una nueva línea de producción aplicando las invariantes
    /// necesarias antes de permitir su creación.
    /// </summary>
    public static ProductionLine Create(
        string code,
        string name)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            throw new DomainException(
                "El código de la línea de producción es obligatorio.");
        }

        if (code.Length > 50)
        {
            throw new DomainException(
                "El código de la línea de producción no puede superar los 50 caracteres.");
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new DomainException(
                "El nombre de la línea de producción es obligatorio.");
        }

        if (name.Length > 100)
        {
            throw new DomainException(
                "El nombre de la línea de producción no puede superar los 100 caracteres.");
        }

        return new ProductionLine(
            ProductionLineId.New(),
            code.Trim(),
            name.Trim());
    }

    /// <summary>
    /// Asocia una máquina existente a la línea.
    ///
    /// ProductionLine solamente almacena el identificador de la máquina.
    /// No recibe ni almacena la entidad Machine completa porque Machine
    /// pertenece a otro Aggregate.
    /// </summary>
    public void AddMachine(MachineId machineId)
    {
        if (machineId.Value == Guid.Empty)
        {
            throw new DomainException(
                "El identificador de la máquina no puede estar vacío.");
        }

        if (_machineIds.Contains(machineId))
        {
            throw new DomainException(
                "La máquina ya está asociada a la línea de producción.");
        }

        _machineIds.Add(machineId);
    }

    /// <summary>
    /// Elimina una máquina de la línea.
    ///
    /// La operación se realiza mediante el Aggregate Root para evitar
    /// que código externo pueda modificar directamente la colección.
    /// </summary>
    public void RemoveMachine(MachineId machineId)
    {
        if (!_machineIds.Remove(machineId))
        {
            throw new DomainException(
                "La máquina no está asociada a la línea de producción.");
        }
    }
}
