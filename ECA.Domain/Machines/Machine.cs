using ECA.Domain.Common;
using ECA.Domain.Common.Events;
using ECA.Domain.Machines.Events;

namespace ECA.Domain.Machines;

/// <summary>
/// Representa una máquina industrial dentro del dominio.
///
/// La entidad es responsable de mantener su propio estado y de garantizar
/// que las transiciones respeten las reglas de negocio.
///
/// Machine actúa actualmente como Aggregate Root de su agregado.
/// </summary>
public sealed class Machine
{
    /// <summary>
    /// Lista interna de eventos producidos por el agregado.
    ///
    /// Es privada para impedir que otras partes de la aplicación
    /// puedan añadir eventos arbitrariamente.
    /// </summary>
    private readonly List<IDomainEvent> _domainEvents = [];

    /// <summary>
    /// Constructor privado para controlar cómo se crea una máquina.
    ///
    /// La creación pública se realiza mediante Create(), que aplica
    /// las validaciones necesarias antes de construir la entidad.
    /// </summary>
    private Machine(
        MachineId id,
        MachineCode code,
        string name,
        MachineType type)
    {
        Id = id;
        Code = code;
        Name = name;
        Type = type;

        // Una máquina recién creada todavía no está conectada.
        Status = MachineStatus.Offline;

        // Guardamos el instante inicial utilizando UTC.
        LastStatusChangedAt = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// Identidad única de la máquina.
    /// </summary>
    public MachineId Id { get; }

    /// <summary>
    /// Código de la máquina.
    ///
    /// MachineCode es un Value Object que encapsula las reglas
    /// específicas de este concepto.
    /// </summary>
    public MachineCode Code { get; private set; }

    /// <summary>
    /// Nombre descriptivo de la máquina.
    ///
    /// Actualmente un string es suficiente porque todavía no
    /// tenemos reglas de negocio que justifiquen otro Value Object.
    /// </summary>
    public string Name { get; private set; }

    /// <summary>
    /// Tipo de máquina.
    /// </summary>
    public MachineType Type { get; private set; }

    /// <summary>
    /// Estado actual de la máquina.
    ///
    /// El setter es privado para obligar a que cualquier transición
    /// pase por los métodos de comportamiento de la entidad.
    /// </summary>
    public MachineStatus Status { get; private set; }

    /// <summary>
    /// Instante del último cambio de estado.
    /// </summary>
    public DateTimeOffset LastStatusChangedAt { get; private set; }

    /// <summary>
    /// Crea una máquina válida.
    ///
    /// La creación se controla mediante este método para evitar
    /// que puedan existir entidades inválidas.
    /// </summary>
    public static Machine Create(
        string code,
        string name,
        MachineType type)
    {
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

        // MachineCode se encarga de validar y normalizar el código.
        var machineCode = MachineCode.Create(code);

        return new Machine(
            MachineId.New(),
            machineCode,
            name.Trim(),
            type);
    }

    /// <summary>
    /// Pone la máquina online.
    ///
    /// Offline -> Idle
    /// </summary>
    public void BringOnline()
    {
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
    /// Idle -> Running
    ///
    /// Antes de realizar la transición comprobamos las condiciones
    /// que deben cumplirse para poder comenzar a trabajar.
    /// </summary>
    public void Start()
    {
        if (Status is MachineStatus.Running)
        {
            throw new DomainException(
                "La máquina ya está en funcionamiento.");
        }

        if (Status is MachineStatus.Offline)
        {
            throw new DomainException(
                "Una máquina desconectada no puede iniciarse.");
        }

        if (Status is MachineStatus.Maintenance)
        {
            throw new DomainException(
                "Una máquina en mantenimiento no puede iniciarse.");
        }

        if (Status is MachineStatus.Faulted)
        {
            throw new DomainException(
                "Una máquina con un fallo debe reiniciarse antes de iniciarse.");
        }

        ChangeStatus(MachineStatus.Running);
    }

    /// <summary>
    /// Detiene una máquina que está en funcionamiento.
    ///
    /// La única transición válida es:
    ///
    /// Running -> Stopped
    ///
    /// El método representa una parada controlada de la máquina.
    /// </summary>
    public void Stop()
    {
        if (Status is not MachineStatus.Running)
        {
            throw new DomainException(
                "Solo una máquina en funcionamiento puede detenerse.");
        }

        ChangeStatus(MachineStatus.Stopped);
    }

    /// <summary>
    /// Envía la máquina a mantenimiento.
    ///
    /// Las transiciones válidas son:
    ///
    /// Idle -> Maintenance
    /// Stopped -> Maintenance
    ///
    /// Una máquina en funcionamiento debe detenerse antes de entrar
    /// en mantenimiento.
    /// </summary>
    public void SetMaintenance()
    {
        if (Status is MachineStatus.Running)
        {
            throw new DomainException(
                "Una máquina en funcionamiento no puede entrar en mantenimiento.");
        }

        if (Status is MachineStatus.Offline)
        {
            throw new DomainException(
                "Una máquina desconectada no puede entrar en mantenimiento.");
        }

        if (Status is MachineStatus.Maintenance)
        {
            throw new DomainException(
                "La máquina ya está en mantenimiento.");
        }

        if (Status is MachineStatus.Faulted)
        {
            throw new DomainException(
                "Una máquina con un fallo debe reiniciarse antes de entrar en mantenimiento.");
        }

        ChangeStatus(MachineStatus.Maintenance);
    }

    /// <summary>
    /// Recupera una máquina que se encuentra en estado Faulted.
    ///
    /// Faulted -> Idle
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
    /// Una máquina disponible, detenida o en mantenimiento puede
    /// pasar a Faulted cuando se detecta una condición de error.
    /// </summary>
    public void ReportFault()
    {
        if (Status is MachineStatus.Offline)
        {
            throw new DomainException(
                "Una máquina desconectada no puede reportar un fallo.");
        }

        if (Status is MachineStatus.Faulted)
        {
            throw new DomainException(
                "La máquina ya se encuentra en estado de fallo.");
        }

        ChangeStatus(MachineStatus.Faulted);
    }

    /// <summary>
    /// Devuelve los eventos pendientes y limpia la colección interna.
    ///
    /// El agregado registra los eventos, pero no es responsable de
    /// procesarlos ni de decidir qué sistemas externos reaccionarán a ellos.
    /// </summary>
    public IReadOnlyCollection<IDomainEvent> GetDomainEvents()
    {
        var domainEvents = _domainEvents.ToArray();

        _domainEvents.Clear();

        return domainEvents;
    }

    /// <summary>
    /// Cambia el estado y registra el Domain Event correspondiente.
    ///
    /// Centralizar esta operación garantiza que cada cambio de estado
    /// actualice también LastStatusChangedAt y genere su evento.
    /// </summary>
    private void ChangeStatus(MachineStatus newStatus)
    {
        var previousStatus = Status;

        Status = newStatus;

        var occurredOn = DateTimeOffset.UtcNow;

        LastStatusChangedAt = occurredOn;

        _domainEvents.Add(
            new MachineStatusChangedDomainEvent(
                Id,
                previousStatus,
                newStatus,
                occurredOn));
    }


    /// <summary>
    /// Finaliza el mantenimiento de la máquina.
    ///
    /// La transición permitida es:
    ///
    /// Maintenance -> Idle
    ///
    /// La máquina vuelve a estar disponible, pero todavía no está
    /// ejecutando producción. Será necesario llamar posteriormente
    /// a Start() para pasar a Running.
    /// </summary>
    public void CompleteMaintenance()
    {
        if (Status is not MachineStatus.Maintenance)
        {
            throw new DomainException(
                "Solo una máquina en mantenimiento puede finalizar el mantenimiento.");
        }

        ChangeStatus(MachineStatus.Idle);
    }
}
