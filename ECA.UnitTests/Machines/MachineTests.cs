using ECA.Domain.Common;
using ECA.Domain.Machines;
using ECA.Domain.Machines.Alarms;
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
        Assert.Equal("CNC-01", machine.Code.Value);
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
    /// Comprueba que Start() genera un Domain Event con
    /// la información correcta sobre el cambio de estado.
    ///
    /// La operación Start() no solamente modifica el estado
    /// interno de Machine; también registra el hecho de que
    /// ese cambio ha ocurrido.
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

        // Una máquina nueva empieza Offline.
        // Necesitamos llevarla a Idle antes de poder iniciarla.
        machine.BringOnline();

        // BringOnline() también genera un evento.
        //
        // Para este test queremos analizar únicamente el evento
        // generado por Start(), por lo que consumimos primero
        // los eventos pendientes.
        machine.GetDomainEvents();

        // ============================================================
        // ACT
        // ============================================================
        machine.Start();

        // ============================================================
        // ASSERT
        // ============================================================
        // Start() debe haber generado exactamente un evento.
        var domainEvents = machine.GetDomainEvents();

        Assert.Single(domainEvents);

        // Comprobamos que el evento sea del tipo esperado.
        var domainEvent = Assert.IsType<MachineStatusChangedDomainEvent>(
            domainEvents.Single());

        // El evento debe identificar correctamente la máquina.
        Assert.Equal(machine.Id, domainEvent.MachineId);

        // Start() se ejecutó cuando la máquina estaba Idle.
        Assert.Equal(
            MachineStatus.Idle,
            domainEvent.PreviousStatus);

        // Después de Start(), la máquina está Running.
        Assert.Equal(
            MachineStatus.Running,
            domainEvent.NewStatus);

        // El evento debe tener registrada la fecha en la que ocurrió.
        Assert.NotEqual(
            default,
            domainEvent.OccurredOn);
    }


    /// <summary>
    /// Comprueba que GetDomainEvents() devuelve los eventos pendientes
    /// y posteriormente los elimina del agregado.
    ///
    /// De esta forma cada evento puede ser procesado una única vez
    /// por el mecanismo que utilicemos posteriormente.
    /// </summary>
    [Fact]
    public void GetDomainEvents_Should_Return_And_Clear_Pending_Events()
    {
        // ARRANGE
        var machine = Machine.Create(
            "CNC-01",
            "Main CNC",
            MachineType.Cnc);

        machine.BringOnline();

        // ACT
        var domainEvents = machine.GetDomainEvents();

        // ASSERT
        // BringOnline() genera un evento.
        Assert.Single(domainEvents);

        // Al haberlos obtenido, ya no deberían quedar eventos pendientes.
        var remainingEvents = machine.GetDomainEvents();

        Assert.Empty(remainingEvents);
    }


    /// <summary>
    /// Comprueba que una máquina detenida puede volver a iniciarse.
    ///
    /// La transición permitida es:
    ///
    /// Stopped -> Running
    ///
    /// Este comportamiento permite detener una máquina y posteriormente
    /// volver a ponerla en producción sin tener que pasar nuevamente
    /// por el estado Offline.
    /// </summary>
    [Fact]
    public void Start_Should_Change_Stopped_To_Running()
    {
        // ARRANGE
        var machine = Machine.Create(
            "CNC-01",
            "Main CNC",
            MachineType.Cnc);

        machine.BringOnline();
        machine.Start();
        machine.Stop();

        // ACT
        machine.Start();

        // ASSERT
        Assert.Equal(
            MachineStatus.Running,
            machine.Status);
    }


    /// <summary>
    /// Comprueba que una máquina Offline no puede registrar
    /// un fallo operativo.
    ///
    /// Una máquina desconectada no está disponible para reportar
    /// un estado operativo dentro de nuestro modelo.
    /// </summary>
    [Fact]
    public void ReportFault_Should_Throw_When_Machine_Is_Offline()
    {
        // ARRANGE
        var machine = Machine.Create(
            "CNC-01",
            "Main CNC",
            MachineType.Cnc);

        // ACT
        var exception = Assert.Throws<DomainException>(
            () => machine.ReportFault());

        // ASSERT
        Assert.Equal(
            "Una máquina desconectada no puede reportar un fallo.",
            exception.Message);
    }

    /// <summary>
    /// Comprueba que Reset() solamente puede utilizarse cuando
    /// la máquina se encuentra en estado Faulted.
    ///
    /// Todos los demás estados deben rechazar la operación.
    /// </summary>
    [Theory]
    [InlineData(MachineStatus.Offline)]
    [InlineData(MachineStatus.Idle)]
    [InlineData(MachineStatus.Running)]
    [InlineData(MachineStatus.Stopped)]
    [InlineData(MachineStatus.Maintenance)]
    public void Reset_Should_Throw_When_Machine_Is_Not_Faulted(
        MachineStatus status)
    {
        // ARRANGE
        var machine = Machine.Create(
            "CNC-01",
            "Main CNC",
            MachineType.Cnc);

        // Llevamos la máquina al estado que queremos comprobar.
        SetMachineStatus(machine, status);

        // ACT
        var exception = Assert.Throws<DomainException>(
            () => machine.Reset());

        // ASSERT
        Assert.Equal(
            "Solo una máquina con un fallo puede reiniciarse.",
            exception.Message);
    }


    /// <summary>
    /// Lleva una máquina a un estado concreto para preparar
    /// diferentes escenarios de prueba.
    ///
    /// Este método solamente existe para facilitar la preparación
    /// de los tests y no forma parte de la lógica de producción.
    /// </summary>
    private static void SetMachineStatus(
        Machine machine,
        MachineStatus status)
    {
        switch (status)
        {
            case MachineStatus.Offline:
                break;

            case MachineStatus.Idle:
                machine.BringOnline();
                break;

            case MachineStatus.Running:
                machine.BringOnline();
                machine.Start();
                break;

            case MachineStatus.Stopped:
                machine.BringOnline();
                machine.Start();
                machine.Stop();
                break;

            case MachineStatus.Maintenance:
                machine.BringOnline();
                machine.SetMaintenance();
                break;

            case MachineStatus.Faulted:
                machine.BringOnline();
                machine.Start();
                machine.ReportFault();
                break;

            default:
                throw new ArgumentOutOfRangeException(
                    nameof(status),
                    status,
                    "Estado de máquina no contemplado.");
        }
    }


    /// <summary>
    /// Comprueba que una máquina en mantenimiento puede finalizar
    /// correctamente el mantenimiento.
    ///
    /// La transición esperada es:
    ///
    /// Maintenance -> Idle
    ///
    /// Después de completar el mantenimiento, la máquina queda
    /// disponible pero todavía no está en funcionamiento.
    /// </summary>
    [Fact]
    public void CompleteMaintenance_Should_Change_Maintenance_To_Idle()
    {
        // ============================================================
        // ARRANGE
        // ============================================================
        var machine = Machine.Create(
            "CNC-01",
            "Main CNC",
            MachineType.Cnc);

        machine.BringOnline();
        machine.SetMaintenance();

        // Consumimos el evento generado por SetMaintenance().
        machine.GetDomainEvents();

        // ============================================================
        // ACT
        // ============================================================
        machine.CompleteMaintenance();

        // ============================================================
        // ASSERT
        // ============================================================
        Assert.Equal(
            MachineStatus.Idle,
            machine.Status);
    }


    /// <summary>
    /// Comprueba que Stop() solamente puede ejecutarse
    /// cuando la máquina está en Running.
    ///
    /// El mismo comportamiento inválido se comprueba para
    /// los diferentes estados en los que la parada no tiene sentido.
    /// </summary>
    [Theory]
    [InlineData(MachineStatus.Offline)]
    [InlineData(MachineStatus.Idle)]
    [InlineData(MachineStatus.Stopped)]
    [InlineData(MachineStatus.Maintenance)]
    [InlineData(MachineStatus.Faulted)]
    public void Stop_Should_Throw_When_Machine_Is_Not_Running(
        MachineStatus status)
    {
        // ARRANGE
        var machine = Machine.Create(
            "CNC-01",
            "Main CNC",
            MachineType.Cnc);

        SetMachineStatus(machine, status);

        // ACT
        var exception = Assert.Throws<DomainException>(
            () => machine.Stop());

        // ASSERT
        Assert.Equal(
            "Solo una máquina en funcionamiento puede detenerse.",
            exception.Message);
    }


    /// <summary>
    /// Comprueba que una máquina con un fallo no puede entrar
    /// directamente en mantenimiento.
    ///
    /// Primero debe recuperarse mediante Reset().
    /// </summary>
    [Fact]
    public void SetMaintenance_Should_Throw_When_Machine_Is_Faulted()
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
            () => machine.SetMaintenance());

        // ASSERT
        Assert.Equal(
            "Una máquina con un fallo debe reiniciarse antes de entrar en mantenimiento.",
            exception.Message);
    }


    /// <summary>
    /// Comprueba que CompleteMaintenance() solamente puede ejecutarse
    /// cuando la máquina está realmente en mantenimiento.
    /// </summary>
    [Theory]
    [InlineData(MachineStatus.Offline)]
    [InlineData(MachineStatus.Idle)]
    [InlineData(MachineStatus.Running)]
    [InlineData(MachineStatus.Stopped)]
    [InlineData(MachineStatus.Faulted)]
    public void CompleteMaintenance_Should_Throw_When_Machine_Is_Not_In_Maintenance(
        MachineStatus status)
    {
        // ARRANGE
        var machine = Machine.Create(
            "CNC-01",
            "Main CNC",
            MachineType.Cnc);

        SetMachineStatus(machine, status);

        // ACT
        var exception = Assert.Throws<DomainException>(
            () => machine.CompleteMaintenance());

        // ASSERT
        Assert.Equal(
            "Solo una máquina en mantenimiento puede finalizar el mantenimiento.",
            exception.Message);
    }


    /// <summary>
    /// Comprueba que al registrar una alarma, esta queda asociada
    /// a la máquina con los datos proporcionados y permanece activa.
    /// </summary>
    [Fact]
    public void RaiseAlarm_Should_Add_Alarm_To_Machine()
    {
        // Arrange
        var machine = Machine.Create(
            "CNC-01",
            "Main CNC",
            MachineType.Cnc);

        const string code = "TEMP-001";
        const string message = "Temperatura del husillo demasiado alta";
        const AlarmSeverity severity = AlarmSeverity.Critical;

        // Act
        var alarm = machine.RaiseAlarm(code, message, severity);

        // Assert
        Assert.Contains(alarm, machine.Alarms);
        Assert.NotEqual(default, alarm.Id);
        Assert.Equal(code, alarm.Code);
        Assert.Equal(message, alarm.Message);
        Assert.Equal(severity, alarm.Severity);
        Assert.True(alarm.IsActive);
    }


    /// <summary>
    /// Comprueba que una máquina no puede limpiar una alarma
    /// que pertenece a otra máquina.
    /// </summary>
    [Fact]
    public void ClearAlarm_Should_Throw_When_Alarm_Belongs_To_Another_Machine()
    {
        // Arrange
        // Creamos dos máquinas independientes.
        var machine1 = Machine.Create(
            "CNC-01",
            "Main CNC",
            MachineType.Cnc);

        var machine2 = Machine.Create(
            "CNC-02",
            "Secondary CNC",
            MachineType.Cnc);

        // La alarma pertenece exclusivamente a machine1.
        var alarm = machine1.RaiseAlarm(
            "TEMP-001",
            "Temperatura del husillo demasiado alta",
            AlarmSeverity.Critical);

        // Act
        // Intentamos limpiar desde machine2 una alarma que no le pertenece.
        var exception = Assert.Throws<DomainException>(
            () => machine2.ClearAlarm(alarm.Id));

        // Assert
        // Machine debe rechazar la operación porque la alarma no pertenece
        // al agregado de la máquina que está intentando modificarla.
        Assert.Equal(
            "La alarma no pertenece a esta máquina.",
            exception.Message);
    }


    /// <summary>
    /// Comprueba que una máquina puede limpiar una de sus propias
    /// alarmas y que esta pasa de activa a inactiva.
    /// </summary>
    [Fact]
    public void ClearAlarm_Should_Deactivate_Alarm_When_It_Belongs_To_Machine()
    {
        // Arrange
        var machine = Machine.Create(
            "CNC-01",
            "Main CNC",
            MachineType.Cnc);

        var alarm = machine.RaiseAlarm(
            "TEMP-001",
            "Temperatura del husillo demasiado alta",
            AlarmSeverity.Critical);

        // Comprobamos la condición inicial antes de ejecutar la operación.
        Assert.True(alarm.IsActive);

        // Act
        // Solicitamos a Machine que limpie una alarma de su propiedad.
        machine.ClearAlarm(alarm.Id);

        // Assert
        // La alarma debe haber pasado de activa a inactiva.
        Assert.False(alarm.IsActive);
    }

    /// <summary>
    /// Comprueba que generar una alarma registra un Domain Event
    /// con la identidad de la máquina, los datos de la alarma
    /// y el instante en que se produjo.
    /// </summary>
    [Fact]
    public void RaiseAlarm_Should_Create_Alarm_Raised_Domain_Event()
    {
        // Arrange
        var machine = Machine.Create(
            "CNC-01",
            "Main CNC",
            MachineType.Cnc);

        const string code = "TEMP-001";
        const string message = "Temperatura del husillo demasiado alta";
        const AlarmSeverity severity = AlarmSeverity.Critical;

        // Act
        var alarm = machine.RaiseAlarm(code, message, severity);

        // Assert
        // Comprobamos que se ha registrado exactamente un evento.
        var pendingEvents = machine.GetDomainEvents();

        var domainEvent = Assert.IsType<MachineAlarmRaisedDomainEvent>(
            Assert.Single(pendingEvents));

        // Verificamos que el evento identifica correctamente la máquina
        // y la alarma que se acaba de generar.
        Assert.Equal(machine.Id, domainEvent.MachineId);
        Assert.Equal(alarm.Id, domainEvent.AlarmId);

        // Los datos del evento deben coincidir con los de la alarma.
        Assert.Equal(code, domainEvent.Code);
        Assert.Equal(message, domainEvent.Message);
        Assert.Equal(severity, domainEvent.Severity);

        // El evento y la alarma representan el mismo instante de creación.
        Assert.Equal(alarm.RaisedAt, domainEvent.OccurredOn);
    }


    /// <summary>
    /// Comprueba que al desactivar una alarma se registra un evento
    /// con los datos correctos y la fecha de desactivación.
    /// </summary>
    [Fact]
    public void ClearAlarm_Should_Create_Alarm_Cleared_Domain_Event()
    {
        // Arrange
        var machine = Machine.Create(
            "CNC-01",
            "Main CNC",
            MachineType.Cnc);

        const string code = "TEMP-001";
        const string message = "Temperatura del husillo demasiado alta";
        const AlarmSeverity severity = AlarmSeverity.Critical;

        var alarm = machine.RaiseAlarm(code, message, severity);

        // Consumimos el evento generado al crear la alarma.
        // Queremos comprobar únicamente el evento de desactivación.
        machine.GetDomainEvents();

        // Act
        // Guardamos los límites temporales para comprobar que la fecha
        // del evento corresponde a esta operación.
        var beforeClear = DateTimeOffset.UtcNow;

        machine.ClearAlarm(alarm.Id);

        var afterClear = DateTimeOffset.UtcNow;

        // Assert
        var pendingEvents = machine.GetDomainEvents();

        var domainEvent = Assert.IsType<MachineAlarmClearedDomainEvent>(
            Assert.Single(pendingEvents));

        Assert.Equal(machine.Id, domainEvent.MachineId);
        Assert.Equal(alarm.Id, domainEvent.AlarmId);
        Assert.Equal(code, domainEvent.Code);
        Assert.Equal(message, domainEvent.Message);
        Assert.Equal(severity, domainEvent.Severity);

        // El evento debe haberse producido durante la desactivación.
        Assert.InRange(
            domainEvent.OccurredOn,
            beforeClear,
            afterClear);
    }


    [Fact]
    public void ClearAlarm_Should_Not_Create_Duplicate_Event_When_Already_Cleared()
    {
        // Arrange
        var machine = Machine.Create(
            "CNC-01",
            "Main CNC",
            MachineType.Cnc);

        var alarm = machine.RaiseAlarm(
            "TEMP-001",
            "Temperatura del husillo demasiado alta",
            AlarmSeverity.Critical);

        // Consumimos el evento de creación para comprobar
        // exclusivamente los eventos de desactivación.
        machine.GetDomainEvents();

        // Act
        // Primera limpieza: debe generar un evento.
        machine.ClearAlarm(alarm.Id);

        var firstClearEvents = machine.GetDomainEvents();

        // Segunda limpieza: no debe generar otro evento ni lanzar
        // una excepción porque la alarma ya está desactivada.
        machine.ClearAlarm(alarm.Id);

        var pendingEvents = machine.GetDomainEvents();

        // Assert
        Assert.Single(firstClearEvents);
        Assert.IsType<MachineAlarmClearedDomainEvent>(
            firstClearEvents.Single());

        // La segunda llamada no debe dejar eventos pendientes.
        Assert.Empty(pendingEvents);

        // La alarma permanece desactivada.
        Assert.False(alarm.IsActive);
    }
}
