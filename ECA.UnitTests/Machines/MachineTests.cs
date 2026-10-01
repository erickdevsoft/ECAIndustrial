using ECA.Domain.Common;
using ECA.Domain.Machines;
using ECA.Domain.Machines.Events;

namespace ECA.UnitTests.Machines;

/// <summary>
/// Tests unitarios de la entidad Machine.
///
/// Estos tests verifican exclusivamente reglas del dominio.
/// No utilizamos base de datos, API, WPF, Redis, RabbitMQ ni ningún
/// otro sistema externo.
///
/// La finalidad es comprobar que Machine mantiene correctamente
/// sus reglas de negocio y sus transiciones de estado.
/// </summary>
public class MachineTests
{
    /// <summary>
    /// Comprueba que una máquina recién creada:
    ///
    /// - Tiene un identificador.
    /// - Conserva su código.
    /// - Conserva su nombre.
    /// - Tiene el tipo indicado.
    /// - Comienza en estado Offline.
    ///
    /// Esta prueba representa el comportamiento esperado
    /// de la creación de una Machine.
    /// </summary>
    [Fact]
    public void Create_Should_Create_Offline_Machine()
    {
        // ============================================================
        // ARRANGE
        // ============================================================
        // Preparamos los datos necesarios para ejecutar
        // el comportamiento que queremos comprobar.
        var machine = Machine.Create(
            "CNC-01",
            "Main CNC",
            MachineType.Cnc);

        // ============================================================
        // ASSERT
        // ============================================================
        // Comprobamos que la identidad se haya generado.
        Assert.NotEqual(default, machine.Id);

        // Verificamos que los datos proporcionados durante
        // la creación se hayan almacenado correctamente.
        Assert.Equal("CNC-01", machine.Code);
        Assert.Equal("Main CNC", machine.Name);
        Assert.Equal(MachineType.Cnc, machine.Type);

        // Una máquina nueva comienza Offline.
        // Esta es una regla del dominio que queremos proteger
        // mediante este test.
        Assert.Equal(MachineStatus.Offline, machine.Status);
    }

    /// <summary>
    /// Comprueba la transición:
    ///
    /// Offline -> Idle
    ///
    /// BringOnline representa que la máquina ha pasado a estar
    /// disponible para operar.
    ///
    /// Idle significa que la máquina está online pero todavía
    /// no está ejecutando producción.
    /// </summary>
    [Fact]
    public void BringOnline_Should_Change_Status_To_Idle()
    {
        // ARRANGE
        // Machine.Create() crea la máquina inicialmente Offline.
        var machine = Machine.Create(
            "CNC-01",
            "Main CNC",
            MachineType.Cnc);

        // ACT
        // Ejecutamos la operación que queremos comprobar.
        machine.BringOnline();

        // ASSERT
        // Después de ponerse online, la máquina debe pasar a Idle.
        Assert.Equal(MachineStatus.Idle, machine.Status);
    }

    /// <summary>
    /// Comprueba la transición:
    ///
    /// Idle -> Running
    ///
    /// Una máquina debe estar online y disponible antes
    /// de poder comenzar a producir.
    /// </summary>
    [Fact]
    public void Start_Should_Change_Idle_To_Running()
    {
        // ARRANGE
        var machine = Machine.Create(
            "CNC-01",
            "Main CNC",
            MachineType.Cnc);

        // Primero llevamos la máquina a un estado válido para iniciar.
        machine.BringOnline();

        // ACT
        machine.Start();

        // ASSERT
        Assert.Equal(MachineStatus.Running, machine.Status);
    }

    /// <summary>
    /// Comprueba una regla de negocio:
    ///
    /// Offline -> Start()
    ///
    /// Esta operación no está permitida.
    ///
    /// El dominio debe impedir que una máquina desconectada
    /// pueda comenzar a trabajar.
    /// </summary>
    [Fact]
    public void Start_Should_Throw_When_Machine_Is_Offline()
    {
        // ARRANGE
        var machine = Machine.Create(
            "CNC-01",
            "Main CNC",
            MachineType.Cnc);

        // ACT
        // Assert.Throws<T>() ejecuta la operación y comprueba
        // que se produzca una excepción del tipo indicado.
        //
        // Esperamos DomainException porque estamos violando
        // una regla de negocio del dominio.
        var exception = Assert.Throws<DomainException>(
            () => machine.Start());

        // ASSERT
        // Además del tipo de excepción verificamos el mensaje
        // para asegurarnos de que se está produciendo
        // exactamente la regla que esperamos.
        Assert.Equal(
            "Una máquina desconectada no puede iniciarse.",
            exception.Message);
    }

    /// <summary>
    /// Comprueba la transición:
    ///
    /// Running -> Faulted
    ///
    /// ReportFault permite registrar que la máquina ha sufrido
    /// una condición de fallo.
    /// </summary>
    [Fact]
    public void ReportFault_Should_Change_Status_To_Faulted()
    {
        // ARRANGE
        var machine = Machine.Create(
            "CNC-01",
            "Main CNC",
            MachineType.Cnc);

        machine.BringOnline();
        machine.Start();

        // ACT
        machine.ReportFault();

        // ASSERT
        Assert.Equal(MachineStatus.Faulted, machine.Status);
    }

    /// <summary>
    /// Comprueba la recuperación de una máquina con fallo.
    ///
    /// Faulted -> Idle
    ///
    /// Reset no inicia directamente la máquina.
    /// Primero la devuelve a Idle para que posteriormente
    /// pueda ejecutar Start().
    /// </summary>
    [Fact]
    public void Reset_Should_Change_Faulted_To_Idle()
    {
        // ARRANGE
        var machine = Machine.Create(
            "CNC-01",
            "Main CNC",
            MachineType.Cnc);

        machine.BringOnline();
        machine.Start();
        machine.ReportFault();

        // ACT
        machine.Reset();

        // ASSERT
        // Después del reset, la máquina está disponible,
        // pero todavía no está produciendo.
        Assert.Equal(MachineStatus.Idle, machine.Status);
    }

    /// <summary>
    /// Comprueba que una máquina Faulted no puede volver
    /// directamente a Running.
    ///
    /// La secuencia correcta es:
    ///
    /// Faulted
    ///     ↓
    /// Reset()
    ///     ↓
    /// Idle
    ///     ↓
    /// Start()
    ///     ↓
    /// Running
    ///
    /// Este test protege esa regla de negocio.
    /// </summary>
    [Fact]
    public void Start_Should_Throw_When_Machine_Is_Faulted()
    {
        // ARRANGE
        var machine = Machine.Create(
            "CNC-01",
            "Main CNC",
            MachineType.Cnc);

        machine.BringOnline();
        machine.Start();
        machine.ReportFault();

        // ACT
        var exception = Assert.Throws<DomainException>(
            () => machine.Start());

        // ASSERT
        Assert.Equal(
            "Una máquina con un fallo debe reiniciarse antes de iniciarse.",
            exception.Message);
    }

    /// <summary>
    /// Comprueba la transición:
    ///
    /// Running -> Stopped
    ///
    /// Stop representa una parada controlada de la máquina.
    /// </summary>
    [Fact]
    public void Stop_Should_Change_Running_To_Stopped()
    {
        // ARRANGE
        var machine = Machine.Create(
            "CNC-01",
            "Main CNC",
            MachineType.Cnc);

        machine.BringOnline();
        machine.Start();

        // ACT
        machine.Stop();

        // ASSERT
        Assert.Equal(MachineStatus.Stopped, machine.Status);
    }

    /// <summary>
    /// Comprueba la transición:
    ///
    /// Idle -> Maintenance
    ///
    /// Una máquina disponible puede entrar en mantenimiento.
    /// </summary>
    [Fact]
    public void SetMaintenance_Should_Change_Idle_To_Maintenance()
    {
        // ARRANGE
        var machine = Machine.Create(
            "CNC-01",
            "Main CNC",
            MachineType.Cnc);

        machine.BringOnline();

        // ACT
        machine.SetMaintenance();

        // ASSERT
        Assert.Equal(MachineStatus.Maintenance, machine.Status);
    }

    /// <summary>
    /// Comprueba que una máquina en mantenimiento
    /// no puede comenzar a trabajar.
    ///
    /// Maintenance -> Start()
    ///
    /// Esta operación debe ser rechazada por el dominio.
    /// </summary>
    [Fact]
    public void Start_Should_Throw_When_Machine_Is_In_Maintenance()
    {
        // ARRANGE
        var machine = Machine.Create(
            "CNC-01",
            "Main CNC",
            MachineType.Cnc);

        machine.BringOnline();
        machine.SetMaintenance();

        // ACT
        var exception = Assert.Throws<DomainException>(
            () => machine.Start());

        // ASSERT
        Assert.Equal(
            "Una máquina en mantenimiento no puede iniciarse.",
            exception.Message);
    }

    /// <summary>
    /// Comprueba que BringOnline solamente puede ejecutarse
    /// cuando la máquina está Offline.
    ///
    /// Una vez que la máquina está online no podemos repetir
    /// la misma transición.
    /// </summary>
    [Fact]
    public void BringOnline_Should_Throw_When_Machine_Is_Not_Offline()
    {
        // ARRANGE
        var machine = Machine.Create(
            "CNC-01",
            "Main CNC",
            MachineType.Cnc);

        machine.BringOnline();

        // ACT
        var exception = Assert.Throws<DomainException>(
            () => machine.BringOnline());

        // ASSERT
        Assert.Equal(
            "Solo una máquina desconectada puede ponerse online.",
            exception.Message);
    }

    /// <summary>
    /// Comprueba que cuando una máquina cambia de estado,
    /// genera un Domain Event describiendo dicho cambio.
    ///
    /// Esto permite que otras partes de la aplicación sepan
    /// qué ocurrió sin acoplar la entidad Machine a servicios externos.
    /// </summary>
    [Fact]
    public void Start_Should_Create_Status_Changed_Domain_Event()
    {
        // ============================================================
        // ARRANGE
        // ============================================================
        var machine = Machine.Create(
            "CNC-01",
            "Main CNC",
            MachineType.Cnc);

        machine.BringOnline();

        // Limpiamos los eventos generados durante BringOnline().
        //
        // En una aplicación real, el consumidor de eventos será
        // responsable de procesarlos y posteriormente marcarlos
        // como procesados.
        //
        // Para este test queremos comprobar únicamente el evento
        // generado por Start().
        machine.ClearDomainEvents();

        // ============================================================
        // ACT
        // ============================================================
        machine.Start();

        // ============================================================
        // ASSERT
        // ============================================================
        // Después de Start() debe existir exactamente un evento.
        Assert.Single(machine.DomainEvents);

        // Recuperamos el evento generado.
        var domainEvent = Assert.IsType<MachineStatusChangedDomainEvent>(
            machine.DomainEvents.Single());

        // Comprobamos que el evento identifica correctamente
        // la máquina que ha cambiado.
        Assert.Equal(machine.Id, domainEvent.MachineId);

        // El estado anterior debe ser Idle.
        Assert.Equal(
            MachineStatus.Idle,
            domainEvent.PreviousStatus);

        // El nuevo estado debe ser Running.
        Assert.Equal(
            MachineStatus.Running,
            domainEvent.NewStatus);

        // El evento debe tener una fecha válida.
        Assert.NotEqual(
            default,
            domainEvent.OccurredOn);
    }
}
