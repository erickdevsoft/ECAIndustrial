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


    /// <summary>
    /// Comprueba que, cuando la máquina no existe, el caso de uso devuelve null.
    /// </summary>
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


    /// <summary>
    /// Comprueba que el caso de uso propague una excepción cuando el código de alarma está vacío.
    /// </summary>
    [Fact]
    public async Task ExecuteAsync_Should_Propagate_ArgumentException_When_Alarm_Code_Is_Empty() 
    {
        // Arrange: preparamos una máquina y un repositorio simulado que devuelve esa máquina.
        var machine = Machine.Create("M001", "Test Machine", MachineType.Cnc);
        var mockRepository = new Mock<IMachineRepository>();
        mockRepository
            .Setup(repo => repo.GetByIdAsync(
                machine.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(machine);
        var useCase = new RaiseMachineAlarmUseCase(mockRepository.Object);
        // Act & Assert: pedimos al caso de uso que registre una alarma con código vacío y esperamos una excepción.
        await Assert.ThrowsAsync<ArgumentException>(() =>
            useCase.ExecuteAsync(
            machine.Id,
            "",
            "Test Alarm",
            AlarmSeverity.Critical,
            CancellationToken.None));

        // Verificamos que no se haya intentado guardar la máquina.
        mockRepository.Verify(
            repo => repo.SaveAsync(
                It.IsAny<Machine>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }


    /// <summary>
    /// Comprueba que el caso de uso propague una excepción cuando la severidad de la alarma es inválida.
    /// </summary>
    [Fact]
    public async Task ExecuteAsync_Should_Propagate_ArgumentOutOfRangeException_When_Alarm_Severity_Is_Invalid()
    {
        // Arrange: preparamos una máquina y un repositorio simulado que devuelve esa máquina.
        var machine = Machine.Create("M001", "Test Machine", MachineType.Cnc);
        var mockRepository = new Mock<IMachineRepository>();
        mockRepository
            .Setup(repo => repo.GetByIdAsync(
                machine.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(machine);
        var useCase = new RaiseMachineAlarmUseCase(mockRepository.Object);
        // Act & Assert: pedimos al caso de uso que registre una alarma con severidad inválida y esperamos una excepción.
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() =>
            useCase.ExecuteAsync(
            machine.Id,
            "ALM001",
            "Test Alarm",
            (AlarmSeverity)999, // Severidad inválida
            CancellationToken.None));
        // Verificamos que no se haya intentado guardar la máquina.
        mockRepository.Verify(
            repo => repo.SaveAsync(
                It.IsAny<Machine>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }



    /// <summary>
    /// Comprueba que el caso de uso propague una excepción cuando el mensaje de la alarma está vacío.
    /// </summary>
    [Fact]
    public async Task ExecuteAsync_Should_Propagate_ArgumentException_When_Alarm_Message_Is_Empty()
    {
        // Arrange: preparamos una máquina y un repositorio simulado que devuelve esa máquina.
        var machine = Machine.Create("M001", "Test Machine", MachineType.Cnc);
        var mockRepository = new Mock<IMachineRepository>();
        mockRepository
            .Setup(repo => repo.GetByIdAsync(
                machine.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(machine);
        var useCase = new RaiseMachineAlarmUseCase(mockRepository.Object);

        // Act & Assert: pedimos al caso de uso que registre una alarma con mensaje vacío y esperamos una excepción.
        await Assert.ThrowsAsync<ArgumentException>(() =>
            useCase.ExecuteAsync(
            machine.Id,
            "ALM001",
            "",
            AlarmSeverity.Critical,
            CancellationToken.None));
        // Verificamos que no se haya intentado guardar la máquina.
        mockRepository.Verify(
            repo => repo.SaveAsync(
                It.IsAny<Machine>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }


    /// Comprueba que el caso de uso propague una excepción cuando el guardado falla.
    /// </summary>
    [Fact]
    public async Task ExecuteAsync_Should_Propagate_Exception_When_Save_Fails()
    {
        // Arrange: preparamos una máquina que el repositorio encuentra.
        var machine = Machine.Create("M001", "Test Machine", MachineType.Cnc);
        var mockRepository = new Mock<IMachineRepository>();

        mockRepository
            .Setup(repo => repo.GetByIdAsync(
                machine.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(machine);

        // Simulamos un fallo al persistir los cambios.
        mockRepository
            .Setup(repo => repo.SaveAsync(
                machine,
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Simulated save failure"));

        var useCase = new RaiseMachineAlarmUseCase(mockRepository.Object);

        // Act & Assert: la excepción del repositorio debe propagarse.
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            useCase.ExecuteAsync(
                machine.Id,
                "ALM001",
                "Test Alarm",
                AlarmSeverity.Critical,
                CancellationToken.None));

        // El caso de uso debe haber intentado guardar la máquina una vez.
        mockRepository.Verify(
            repo => repo.SaveAsync(
                machine,
                CancellationToken.None),
            Times.Once);
    }


    /// <summary>
    /// Comprueba que el caso de uso pase el token de cancelación al repositorio.
    /// </summary>
    [Fact]
    public async Task ExecuteAsync_Should_Pass_CancellationToken_To_Repository()
    {
        // Arrange: preparamos una máquina y un repositorio simulado que devuelve esa máquina.
        var machine = Machine.Create("M001", "Test Machine", MachineType.Cnc);
        var mockRepository = new Mock<IMachineRepository>();
        mockRepository
            .Setup(repo => repo.GetByIdAsync(
                machine.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(machine);
        var useCase = new RaiseMachineAlarmUseCase(mockRepository.Object);
        using var cts = new CancellationTokenSource();
        // Act: pedimos al caso de uso que registre una alarma pasando un token de cancelación.
        await useCase.ExecuteAsync(
            machine.Id,
            "ALM001",
            "Test Alarm",
            AlarmSeverity.Critical,
            cts.Token);
        // Assert: verificamos que el token de cancelación se haya pasado correctamente al repositorio.
        mockRepository.Verify(
            repo => repo.GetByIdAsync(machine.Id, cts.Token),
            Times.Once);
        mockRepository.Verify(
            repo => repo.SaveAsync(machine, cts.Token),
            Times.Once);

    }
}
