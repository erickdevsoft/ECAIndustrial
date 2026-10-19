using ECA.Application.Machines;
using ECA.Domain.Machines;
using ECA.Domain.Machines.Alarms;
using Moq;

namespace ECA.UnitTests.Machines;

public class RaiseMachineAlarmUseCaseTests
{
    /// <summary>
    /// Comprueba que, cuando la máquina existe, el caso de uso crea una alarma activa,
    /// guarda la máquina modificada y devuelve el identificador de la alarma.
    /// </summary>
    [Fact]
    public async Task ExecuteAsync_Should_Raise_Alarm_And_Save_Machine_When_Machine_Exists()
    {
        // Arrange: preparamos una máquina sin alarmas y un repositorio simulado
        // que devuelve esa máquina.
        var machine = Machine.Create("M001", "Test Machine", MachineType.Cnc);
        var mockRepository = new Mock<IMachineRepository>();

        mockRepository
            .Setup(repo => repo.GetByIdAsync(
                machine.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(machine);

        var useCase = new RaiseMachineAlarmUseCase(mockRepository.Object);

        // Act: pedimos al caso de uso que registre una alarma en la máquina.
        var result = await useCase.ExecuteAsync(
            machine.Id,
            "ALM001",
            "Test Alarm",
            AlarmSeverity.Critical,
            CancellationToken.None);

        // Assert: la alarma se creó, está activa y su ID es el devuelto por el caso de uso.
        var raisedAlarm = Assert.Single(machine.Alarms);
        Assert.Equal(raisedAlarm.Id, result);
        Assert.True(raisedAlarm.IsActive);

        // La máquina con la alarma nueva debe guardarse exactamente una vez.
        mockRepository.Verify(
            repo => repo.SaveAsync(
                machine,
                CancellationToken.None),
            Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_Should_Return_Null_When_Machine_Does_Not_Exist()
    {
        // Arrange: preparamos un repositorio simulado que devuelve null para cualquier ID de máquina.

        var machineId = MachineId.New();
        var mockRepository = new Mock<IMachineRepository>();
        mockRepository
            .Setup(repo => repo.GetByIdAsync(
                machineId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Machine?)null);
        var useCase = new RaiseMachineAlarmUseCase(mockRepository.Object);
        // Act: pedimos al caso de uso que registre una alarma en una máquina inexistente.
        var result = await useCase.ExecuteAsync(
            machineId,
            "ALM001",
            "Test Alarm",
            AlarmSeverity.Critical,
            CancellationToken.None);
        // Assert: el resultado debe ser null y no se debe intentar guardar ninguna máquina.
        Assert.Null(result);
        mockRepository.Verify(
            repo => repo.GetByIdAsync(machineId, CancellationToken.None),
            Times.Once);

        mockRepository.Verify(
            repo => repo.SaveAsync(
                It.IsAny<Machine>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }
}
