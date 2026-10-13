using Moq;
using ECA.Application.Machines;
using ECA.Domain.Machines;
using ECA.Domain.Machines.Alarms;

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
}
