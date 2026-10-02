using ECA.Domain.Common;
using ECA.Domain.Machines;
using ECA.Domain.ProductionLines;

namespace ECA.UnitTests.ProductionLines;

/// <summary>
/// Tests unitarios del Aggregate Root ProductionLine.
///
/// Estas pruebas verifican que la línea mantiene correctamente
/// sus reglas de negocio relacionadas con las máquinas asociadas.
/// </summary>
public class ProductionLineTests
{
    /// <summary>
    /// Comprueba que una línea se crea correctamente.
    ///
    /// Una línea recién creada no tiene ninguna máquina asociada.
    /// </summary>
    [Fact]
    public void Create_Should_Create_Production_Line_Without_Machines()
    {
        // ============================================================
        // ARRANGE
        // ============================================================
        var line = ProductionLine.Create(
            "LINE-01",
            "Línea de mecanizado principal");

        // ============================================================
        // ASSERT
        // ============================================================
        // La línea debe tener una identidad válida.
        Assert.NotEqual(
            default,
            line.Id);

        // Debe conservar correctamente los datos proporcionados.
        Assert.Equal(
            "LINE-01",
            line.Code);

        Assert.Equal(
            "Línea de mecanizado principal",
            line.Name);

        // Al crearse todavía no tiene máquinas asignadas.
        Assert.Empty(line.MachineIds);
    }

    /// <summary>
    /// Comprueba que una máquina puede asociarse a una línea.
    ///
    /// La línea almacena únicamente el MachineId.
    /// </summary>
    [Fact]
    public void AddMachine_Should_Add_Machine_To_Line()
    {
        // ARRANGE
        var line = ProductionLine.Create(
            "LINE-01",
            "Línea de mecanizado principal");

        var machine = Machine.Create(
            "CNC-01",
            "CNC Principal",
            MachineType.Cnc);

        // ACT
        line.AddMachine(machine.Id);

        // ASSERT
        Assert.Single(line.MachineIds);

        Assert.Contains(
            machine.Id,
            line.MachineIds);
    }

    /// <summary>
    /// Comprueba que una misma máquina no puede asociarse dos veces
    /// a la misma línea.
    ///
    /// Esto protege la consistencia interna de ProductionLine.
    /// </summary>
    [Fact]
    public void AddMachine_Should_Throw_When_Machine_Is_Already_Associated()
    {
        // ARRANGE
        var line = ProductionLine.Create(
            "LINE-01",
            "Línea de mecanizado principal");

        var machine = Machine.Create(
            "CNC-01",
            "CNC Principal",
            MachineType.Cnc);

        line.AddMachine(machine.Id);

        // ACT
        var exception = Assert.Throws<DomainException>(
            () => line.AddMachine(machine.Id));

        // ASSERT
        Assert.Equal(
            "La máquina ya está asociada a la línea de producción.",
            exception.Message);
    }

    /// <summary>
    /// Comprueba que no se puede asociar un MachineId vacío.
    ///
    /// Un identificador vacío no representa una máquina válida.
    /// </summary>
    [Fact]
    public void AddMachine_Should_Throw_When_Machine_Id_Is_Empty()
    {
        // ARRANGE
        var line = ProductionLine.Create(
            "LINE-01",
            "Línea de mecanizado principal");

        var emptyMachineId = default(MachineId);

        // ACT
        var exception = Assert.Throws<DomainException>(
            () => line.AddMachine(emptyMachineId));

        // ASSERT
        Assert.Equal(
            "El identificador de la máquina no puede estar vacío.",
            exception.Message);
    }

    /// <summary>
    /// Comprueba que una máquina asociada puede eliminarse
    /// correctamente de la línea.
    /// </summary>
    [Fact]
    public void RemoveMachine_Should_Remove_Machine_From_Line()
    {
        // ARRANGE
        var line = ProductionLine.Create(
            "LINE-01",
            "Línea de mecanizado principal");

        var machine = Machine.Create(
            "CNC-01",
            "CNC Principal",
            MachineType.Cnc);

        line.AddMachine(machine.Id);

        // ACT
        line.RemoveMachine(machine.Id);

        // ASSERT
        Assert.Empty(line.MachineIds);
    }

    /// <summary>
    /// Comprueba que no se puede eliminar una máquina que no
    /// está asociada a la línea.
    /// </summary>
    [Fact]
    public void RemoveMachine_Should_Throw_When_Machine_Is_Not_Associated()
    {
        // ARRANGE
        var line = ProductionLine.Create(
            "LINE-01",
            "Línea de mecanizado principal");

        var machine = Machine.Create(
            "CNC-01",
            "CNC Principal",
            MachineType.Cnc);

        // ACT
        var exception = Assert.Throws<DomainException>(
            () => line.RemoveMachine(machine.Id));

        // ASSERT
        Assert.Equal(
            "La máquina no está asociada a la línea de producción.",
            exception.Message);
    }
}
