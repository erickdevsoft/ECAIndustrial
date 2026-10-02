using ECA.Domain.Common;
using ECA.Domain.Plants;
using ECA.Domain.ProductionLines;

namespace ECA.UnitTests.Plants;

/// <summary>
/// Tests unitarios del Aggregate Root Plant.
///
/// Estos tests verifican las reglas de negocio relacionadas con
/// la asociación de líneas de producción a una planta.
/// </summary>
public class PlantTests
{
    /// <summary>
    /// Comprueba que una planta nueva se crea correctamente
    /// y comienza sin líneas de producción asociadas.
    /// </summary>
    [Fact]
    public void Create_Should_Create_Plant_Without_Production_Lines()
    {
        // ============================================================
        // ARRANGE
        // ============================================================
        var plant = Plant.Create(
            "PLANT-01",
            "Planta principal");

        // ============================================================
        // ASSERT
        // ============================================================

        // Una planta nueva debe recibir una identidad válida.
        Assert.NotEqual(
            default,
            plant.Id);

        // Comprobamos que los datos de creación se almacenan
        // correctamente.
        Assert.Equal(
            "PLANT-01",
            plant.Code);

        Assert.Equal(
            "Planta principal",
            plant.Name);

        // Al crearse todavía no existen líneas asociadas.
        Assert.Empty(
            plant.ProductionLineIds);
    }

    /// <summary>
    /// Comprueba que una línea puede asociarse correctamente
    /// a una planta.
    /// </summary>
    [Fact]
    public void AddProductionLine_Should_Add_Line_To_Plant()
    {
        // ARRANGE
        var plant = Plant.Create(
            "PLANT-01",
            "Planta principal");

        var productionLine = ProductionLine.Create(
            "LINE-01",
            "Línea de mecanizado");

        // ACT
        plant.AddProductionLine(
            productionLine.Id);

        // ASSERT
        Assert.Single(
            plant.ProductionLineIds);

        Assert.Contains(
            productionLine.Id,
            plant.ProductionLineIds);
    }

    /// <summary>
    /// Comprueba que una misma línea no puede asociarse dos veces
    /// a la misma planta.
    ///
    /// Esto protege la consistencia del Aggregate.
    /// </summary>
    [Fact]
    public void AddProductionLine_Should_Throw_When_Line_Is_Already_Associated()
    {
        // ARRANGE
        var plant = Plant.Create(
            "PLANT-01",
            "Planta principal");

        var productionLine = ProductionLine.Create(
            "LINE-01",
            "Línea de mecanizado");

        plant.AddProductionLine(
            productionLine.Id);

        // ACT
        var exception = Assert.Throws<DomainException>(
            () => plant.AddProductionLine(
                productionLine.Id));

        // ASSERT
        Assert.Equal(
            "La línea de producción ya está asociada a la planta.",
            exception.Message);
    }

    /// <summary>
    /// Comprueba que no se puede asociar una línea cuyo identificador
    /// sea Guid.Empty.
    ///
    /// Un identificador vacío no representa una línea válida.
    /// </summary>
    [Fact]
    public void AddProductionLine_Should_Throw_When_Line_Id_Is_Empty()
    {
        // ARRANGE
        var plant = Plant.Create(
            "PLANT-01",
            "Planta principal");

        var emptyProductionLineId =
            default(ProductionLineId);

        // ACT
        var exception = Assert.Throws<DomainException>(
            () => plant.AddProductionLine(
                emptyProductionLineId));

        // ASSERT
        Assert.Equal(
            "El identificador de la línea de producción no puede estar vacío.",
            exception.Message);
    }

    /// <summary>
    /// Comprueba que una línea asociada puede eliminarse
    /// correctamente de la planta.
    /// </summary>
    [Fact]
    public void RemoveProductionLine_Should_Remove_Line_From_Plant()
    {
        // ARRANGE
        var plant = Plant.Create(
            "PLANT-01",
            "Planta principal");

        var productionLine = ProductionLine.Create(
            "LINE-01",
            "Línea de mecanizado");

        plant.AddProductionLine(
            productionLine.Id);

        // ACT
        plant.RemoveProductionLine(
            productionLine.Id);

        // ASSERT
        Assert.Empty(
            plant.ProductionLineIds);
    }

    /// <summary>
    /// Comprueba que no se puede eliminar una línea que no
    /// está asociada actualmente a la planta.
    /// </summary>
    [Fact]
    public void RemoveProductionLine_Should_Throw_When_Line_Is_Not_Associated()
    {
        // ARRANGE
        var plant = Plant.Create(
            "PLANT-01",
            "Planta principal");

        var productionLine = ProductionLine.Create(
            "LINE-01",
            "Línea de mecanizado");

        // ACT
        var exception = Assert.Throws<DomainException>(
            () => plant.RemoveProductionLine(
                productionLine.Id));

        // ASSERT
        Assert.Equal(
            "La línea de producción no está asociada a la planta.",
            exception.Message);
    }
}
