using ECA.Domain.Common;
using ECA.Domain.Machines;

namespace ECA.UnitTests.Machines;

public class MachineTests
{
    [Fact]
    public void Create_Should_Create_Offline_Machine()
    {
        // Arrange
        var machine = Machine.Create(
            "CNC-01",
            "Main CNC",
            MachineType.Cnc);

        // Assert
        Assert.NotEqual(default, machine.Id);
        Assert.Equal("CNC-01", machine.Code);
        Assert.Equal("Main CNC", machine.Name);
        Assert.Equal(MachineType.Cnc, machine.Type);
        Assert.Equal(MachineStatus.Offline, machine.Status);
    }

    [Fact]
    public void BringOnline_Should_Change_Status_To_Idle()
    {
        // Arrange
        var machine = Machine.Create(
            "CNC-01",
            "Main CNC",
            MachineType.Cnc);

        // Act
        machine.BringOnline();

        // Assert
        Assert.Equal(MachineStatus.Idle, machine.Status);
    }

    [Fact]
    public void Start_Should_Change_Idle_To_Running()
    {
        // Arrange
        var machine = Machine.Create(
            "CNC-01",
            "Main CNC",
            MachineType.Cnc);

        machine.BringOnline();

        // Act
        machine.Start();

        // Assert
        Assert.Equal(MachineStatus.Running, machine.Status);
    }

    [Fact]
    public void Start_Should_Throw_When_Machine_Is_Offline()
    {
        // Arrange
        var machine = Machine.Create(
            "CNC-01",
            "Main CNC",
            MachineType.Cnc);

        // Act
        var exception = Assert.Throws<DomainException>(
            () => machine.Start());

        // Assert
        Assert.Equal(
            "No se puede iniciar una máquina que está fuera de línea.",
            exception.Message);
    }

    [Fact]
    public void ReportFault_Should_Change_Status_To_Faulted()
    {
        // Arrange
        var machine = Machine.Create(
            "CNC-01",
            "Main CNC",
            MachineType.Cnc);

        machine.BringOnline();
        machine.Start();

        // Act
        machine.ReportFault();

        // Assert
        Assert.Equal(MachineStatus.Faulted, machine.Status);
    }

    [Fact]
    public void Reset_Should_Change_Faulted_To_Idle()
    {
        // Arrange
        var machine = Machine.Create(
            "CNC-01",
            "Main CNC",
            MachineType.Cnc);

        machine.BringOnline();
        machine.Start();
        machine.ReportFault();

        // Act
        machine.Reset();

        // Assert
        Assert.Equal(MachineStatus.Idle, machine.Status);
    }

    [Fact]
    public void Start_Should_Throw_When_Machine_Is_Faulted()
    {
        // Arrange
        var machine = Machine.Create(
            "CNC-01",
            "Main CNC",
            MachineType.Cnc);

        machine.BringOnline();
        machine.Start();
        machine.ReportFault();

        // Act
        var exception = Assert.Throws<DomainException>(
            () => machine.Start());

        // Assert
        Assert.Equal(
            "Una máquina en fallo debe ser reiniciada antes de iniciarla.",
            exception.Message);
    }
}
