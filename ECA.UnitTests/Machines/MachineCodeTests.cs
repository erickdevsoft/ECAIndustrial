using ECA.Domain.Machines;

namespace ECA.UnitTests.Machines;

/// <summary>
/// Tests del Value Object MachineCode.
///
/// Estas pruebas verifican exclusivamente las reglas propias
/// del concepto MachineCode, independientemente de Machine.
/// </summary>
public class MachineCodeTests
{
    /// <summary>
    /// Comprueba que se puede crear un código válido.
    /// </summary>
    [Fact]
    public void Create_Should_Create_Valid_Machine_Code()
    {
        // Arrange
        const string value = "CNC-01";

        // Act
        var machineCode = MachineCode.Create(value);

        // Assert
        Assert.Equal(
            value,
            machineCode.Value);
    }

    /// <summary>
    /// Comprueba que los espacios al principio y al final
    /// se eliminan durante la normalización.
    ///
    /// Esto evita que:
    ///
    /// " CNC-01 "
    ///
    /// y:
    ///
    /// "CNC-01"
    ///
    /// sean tratados como códigos diferentes.
    /// </summary>
    [Fact]
    public void Create_Should_Trim_WhiteSpace()
    {
        // Act
        var machineCode = MachineCode.Create(
            "  CNC-01  ");

        // Assert
        Assert.Equal(
            "CNC-01",
            machineCode.Value);
    }

    /// <summary>
    /// Comprueba que un código vacío no puede existir.
    /// </summary>
    [Fact]
    public void Create_Should_Throw_When_Code_Is_Empty()
    {
        // Act
        var exception = Assert.Throws<ArgumentException>(
            () => MachineCode.Create(""));

        // Assert
        Assert.Equal(
            "El código de la máquina es obligatorio. (Parameter 'value')",
            exception.Message);
    }

    /// <summary>
    /// Comprueba que un código formado únicamente por espacios
    /// tampoco es válido.
    /// </summary>
    [Fact]
    public void Create_Should_Throw_When_Code_Is_WhiteSpace()
    {
        // Act
        var exception = Assert.Throws<ArgumentException>(
            () => MachineCode.Create("   "));

        // Assert
        Assert.Equal(
            "El código de la máquina es obligatorio. (Parameter 'value')",
            exception.Message);
    }

    /// <summary>
    /// Comprueba que el código no puede superar los 50 caracteres.
    /// </summary>
    [Fact]
    public void Create_Should_Throw_When_Code_Exceeds_Maximum_Length()
    {
        // Arrange
        var value = new string(
            'A',
            51);

        // Act
        var exception = Assert.Throws<ArgumentException>(
            () => MachineCode.Create(value));

        // Assert
        Assert.Equal(
            "El código de la máquina no puede superar los 50 caracteres. (Parameter 'value')",
            exception.Message);
    }

    /// <summary>
    /// Comprueba que dos MachineCode con el mismo valor
    /// tienen igualdad basada en valor.
    ///
    /// Esta es una de las características fundamentales
    /// de un Value Object.
    /// </summary>
    [Fact]
    public void Two_Machine_Codes_With_Same_Value_Should_Be_Equal()
    {
        // Arrange
        var code1 = MachineCode.Create("CNC-01");
        var code2 = MachineCode.Create("CNC-01");

        // Assert
        Assert.Equal(
            code1,
            code2);
    }
}
