using ECA.Application.Machines;
using ECA.Domain.Common;
using ECA.Domain.Machines;
using ECA.Domain.Machines.Alarms;
using Moq;

namespace ECA.UnitTests.Machines;

public class ClearMachineAlarmUseCaseTests
{
    [Fact]
    public async Task ExecuteAsync_Should_Return_False_When_Machine_Does_Not_Exist()
    {
        // Arrange
        // Creamos un repositorio simulado que no encuentra la máquina.
        var mockRepository = new Mock<IMachineRepository>();

        mockRepository
            .Setup(repo => repo.GetByIdAsync(
                It.IsAny<MachineId>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Machine?)null);

        var useCase = new ClearMachineAlarmUseCase(
            mockRepository.Object);

        var machineId = MachineId.New();
        var alarmId = new MachineAlarmId(Guid.NewGuid());

        // Act
        var result = await useCase.ExecuteAsync(
            machineId,
            alarmId);

        // Assert
        // El caso de uso debe indicar que no se encontró la máquina.
        Assert.False(result);

        // No debe intentar guardar una máquina que no existe.
        mockRepository.Verify(
            repo => repo.SaveAsync(
                It.IsAny<Machine>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }


    [Fact]
    public async Task ExecuteAsync_Should_Return_True_When_Machine_Exists_And_Alarm_Is_Cleared()
    {
        // Arrange
        var machine = Machine.Create(
            "M001",
            "Máquina 1",
            MachineType.Cnc);

        var alarm = machine.RaiseAlarm(
            "A001",
            "Alarma de prueba",
            AlarmSeverity.Warning);

        var mockRepository = new Mock<IMachineRepository>();

        // El repositorio devuelve la máquina solicitada.
        mockRepository.Setup(repo => repo.GetByIdAsync(
                machine.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(machine);

        var useCase = new ClearMachineAlarmUseCase(
            mockRepository.Object);

        // Act
        var result = await useCase.ExecuteAsync(
            machine.Id,
            alarm.Id);

        // Assert
        Assert.True(result);

        // La alarma debe haber pasado de activa a inactiva.
        Assert.False(alarm.IsActive);

        // El repositorio debe guardar la máquina exactamente una vez.
        mockRepository.Verify(
            repo => repo.SaveAsync(
                machine,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }


    [Fact]
    public async Task ExecuteAsync_Should_Propagate_DomainException_When_Alarm_Does_Not_Belong_To_Machine()
    {
        // Arrange
        var machine1 = Machine.Create(
            "M001",
            "Máquina 1",
            MachineType.Cnc);

        var machine2 = Machine.Create(
            "M002",
            "Máquina 2",
            MachineType.Robot);

        // La alarma pertenece a machine2, no a machine1.
        var alarm = machine2.RaiseAlarm(
            "A002",
            "Alarma de prueba",
            AlarmSeverity.Warning);

        var mockRepository = new Mock<IMachineRepository>();

        // El repositorio devuelve la primera máquina.
        mockRepository
            .Setup(repo => repo.GetByIdAsync(
                machine1.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(machine1);

        var useCase = new ClearMachineAlarmUseCase(
            mockRepository.Object);

        // Act
        // El caso de uso debe propagar la excepción lanzada por el dominio.
        var exception = await Assert.ThrowsAsync<DomainException>(
            () => useCase.ExecuteAsync(
                machine1.Id,
                alarm.Id));

        // Assert
        Assert.Equal(
            "La alarma no pertenece a esta máquina.",
            exception.Message);

        // Si la operación falla, no debemos guardar la máquina.
        mockRepository.Verify(
            repo => repo.SaveAsync(
                It.IsAny<Machine>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }
}
