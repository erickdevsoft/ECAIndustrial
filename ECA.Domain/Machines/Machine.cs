using ECA.Domain.Common;
using ECA.Domain.Common.Events;
using ECA.Domain.Machines.Events;

namespace ECA.Domain.Machines;

/// <summary>
/// Representa una máquina industrial dentro del dominio de ECA Industrial.
///
/// La entidad es responsable de mantener su propio estado y de garantizar
/// que las transiciones entre estados respeten las reglas de negocio.
/// </summary>
public sealed class Machine
{
    /// <summary>
    /// Constructor privado para evitar que otras partes de la aplicación
    /// creen máquinas sin pasar por las validaciones definidas en Create().
    ///
    /// Esto nos permite controlar cómo nace una Machine y garantizar
    /// sus invariantes desde el momento de creación.
    /// </summary>
    private Machine(
        MachineId id,
        string code,
        string name,
        MachineType type)
    {
        Id = id;
        Code = code;
        Name = name;
        Type = type;

        // Toda máquina nueva comienza desconectada de la planta.
        Status = MachineStatus.Offline;

        // Guardamos el momento en el que se establece su estado inicial.
        // Usamos UTC para evitar problemas relacionados con zonas horarias.
        LastStatusChangedAt = DateTimeOffset.UtcNow;
    }

    private readonly List<IDomainEvent> _domainEvents = [];

    public IReadOnlyCollection<IDomainEvent> DomainEvents
    => _domainEvents.AsReadOnly();

    /// <summary>
    /// Identidad de la máquina.
    ///
    /// Aunque el resto de propiedades cambien, esta identidad permite
    /// distinguir esta máquina de cualquier otra dentro del sistema.
    /// </summary>
    public MachineId Id { get; }

    /// <summary>
    /// Código utilizado para identificar la máquina en la planta.
    ///
    /// Ejemplos:
    /// CNC-01
    /// ROBOT-01
    /// INSPECTION-01
    /// </summary>
    public string Code { get; private set; }

    /// <summary>
    /// Nombre descriptivo de la máquina.
    /// </summary>
    public string Name { get; private set; }

    /// <summary>
    /// Tipo de máquina.
    /// </summary>
    public MachineType Type { get; private set; }

    /// <summary>
    /// Estado actual de la máquina.
    ///
    /// El setter es privado para impedir que otra parte de la aplicación
    /// pueda modificar directamente el estado.
    ///
    /// Los cambios deben realizarse mediante métodos del dominio como
    /// Start(), Stop(), ReportFault(), etc., donde se aplican las reglas
    /// de negocio correspondientes.
    /// </summary>
    public MachineStatus Status { get; private set; }

    /// <summary>
    /// Momento en el que la máquina cambió por última vez de estado.
    /// </summary>
    public DateTimeOffset LastStatusChangedAt { get; private set; }

    /// <summary>
    /// Crea una nueva máquina aplicando las validaciones necesarias
    /// antes de permitir su creación.
    ///
    /// Utilizamos un método de creación en lugar de exponer el constructor
    /// para evitar que puedan existir máquinas inválidas dentro del dominio.
    /// </summary>
    public static Machine Create(
        string code,
        string name,
        MachineType type)
    {
        // El código es obligatorio porque identifica la máquina
        // dentro de la planta.
        if (string.IsNullOrWhiteSpace(code))
        {
            throw new DomainException(
                "El código de la máquina es obligatorio.");
        }

        // Limitamos la longitud para mantener una regla de negocio
        // coherente que después también podremos reflejar en persistencia.
        if (code.Length > 50)
        {
            throw new DomainException(
                "El código de la máquina no puede superar los 50 caracteres.");
        }

        // El nombre también es obligatorio.
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new DomainException(
                "El nombre de la máquina es obligatorio.");
        }

        if (name.Length > 100)
        {
            throw new DomainException(
                "El nombre de la máquina no puede superar los 100 caracteres.");
        }

        // Trim() evita almacenar espacios innecesarios al principio o
        // al final de los valores introducidos.
        return new Machine(
            MachineId.New(),
            code.Trim(),
            name.Trim(),
            type);
    }

    /// <summary>
    /// Pone la máquina online.
    ///
    /// La transición permitida es:
    ///
    /// Offline -> Idle
    ///
    /// Idle significa que la máquina está disponible pero todavía
    /// no se encuentra ejecutando producción.
    /// </summary>
    public void BringOnline()
    {
        // Una máquina que ya está online no puede volver a ejecutar
        // la misma transición.
        if (Status is not MachineStatus.Offline)
        {
            throw new DomainException(
                "Solo una máquina desconectada puede ponerse online.");
        }

        ChangeStatus(MachineStatus.Idle);
    }

    /// <summary>
    /// Inicia la máquina.
    ///
    /// La transición normal es:
    ///
    /// Idle -> Running
    ///
    /// Antes de cambiar el estado comprobamos que la máquina
    /// se encuentra en una situación válida para comenzar.
    /// </summary>
    public void Start()
    {
        // Evitamos ejecutar una transición que ya se ha realizado.
        if (Status is MachineStatus.Running)
        {
            throw new DomainException(
                "La máquina ya está en funcionamiento.");
        }

        // Una máquina desconectada no puede comenzar a trabajar.
        if (Status is MachineStatus.Offline)
        {
            throw new DomainException(
                "Una máquina desconectada no puede iniciarse.");
        }

        // El mantenimiento bloquea la ejecución de la máquina.
        if (Status is MachineStatus.Maintenance)
        {
            throw new DomainException(
                "Una máquina en mantenimiento no puede iniciarse.");
        }

        // Un fallo debe resolverse antes de permitir que la máquina
        // vuelva a producción.
        if (Status is MachineStatus.Faulted)
        {
            throw new DomainException(
                "Una máquina con un fallo debe reiniciarse antes de iniciarse.");
        }

        ChangeStatus(MachineStatus.Running);
    }

    /// <summary>
    /// Detiene la máquina.
    ///
    /// La transición principal es:
    ///
    /// Running -> Stopped
    /// </summary>
    public void Stop()
    {
        // Una máquina desconectada no tiene sentido que sea detenida
        // porque no se encuentra operativa.
        if (Status is MachineStatus.Offline)
        {
            throw new DomainException(
                "Una máquina desconectada no puede detenerse.");
        }

        ChangeStatus(MachineStatus.Stopped);
    }

    /// <summary>
    /// Envía la máquina a mantenimiento.
    ///
    /// Una máquina que está Running no puede entrar directamente
    /// en mantenimiento. Primero debe detenerse.
    /// </summary>
    public void SetMaintenance()
    {
        if (Status is MachineStatus.Running)
        {
            throw new DomainException(
                "Una máquina en funcionamiento no puede entrar en mantenimiento.");
        }

        ChangeStatus(MachineStatus.Maintenance);
    }

    /// <summary>
    /// Recupera una máquina que se encuentra en estado Faulted.
    ///
    /// La transición es:
    ///
    /// Faulted -> Idle
    ///
    /// No pasamos directamente a Running porque después del reset
    /// la máquina debe encontrarse preparada antes de iniciar producción.
    /// </summary>
    public void Reset()
    {
        if (Status is not MachineStatus.Faulted)
        {
            throw new DomainException(
                "Solo una máquina con un fallo puede reiniciarse.");
        }

        ChangeStatus(MachineStatus.Idle);
    }

    /// <summary>
    /// Registra un fallo en la máquina.
    ///
    /// Por ejemplo, un problema detectado por un sensor, PLC
    /// o sistema de monitorización.
    /// </summary>
    public void ReportFault()
    {
        // Una máquina desconectada no está disponible para reportar
        // un fallo operativo dentro de este modelo de dominio.
        if (Status is MachineStatus.Offline)
        {
            throw new DomainException(
                "Una máquina desconectada no puede reportar un fallo.");
        }

        ChangeStatus(MachineStatus.Faulted);
    }

    /// <summary>
    /// Centraliza el cambio de estado de la máquina.
    ///
    /// Esto evita repetir en cada método la asignación del estado
    /// y la actualización de LastStatusChangedAt.
    /// </summary>
    private void ChangeStatus(MachineStatus newStatus)
    {
        // Guardamos el estado anterior porque el evento debe describir
        // qué cambio se ha producido.
        var previousStatus = Status;

        Status = newStatus;

        var occurredOn = DateTimeOffset.UtcNow;

        LastStatusChangedAt = occurredOn;

        // Registramos un evento que describe algo que ya ocurrió:
        // la máquina ha cambiado de estado.
        _domainEvents.Add(
            new MachineStatusChangedDomainEvent(
                Id,
                previousStatus,
                newStatus,
                occurredOn));
    }


    /// <summary>
    /// Elimina los Domain Events que ya han sido recogidos
    /// por la capa responsable de procesarlos.
    ///
    /// La entidad no procesa los eventos; únicamente los registra.
    /// </summary>
    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }
}
