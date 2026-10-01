
using ECA.Domain.Common;

namespace ECA.Domain.Machines
{
    public sealed class Machine
    {
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
            Status = MachineStatus.Offline;
            LastStatusChangedAt = DateTimeOffset.UtcNow;
        }

        public MachineId Id { get; }
        public string Code { get; private set; }
        public string Name { get; private set; }
        public MachineType Type { get; private set; }
        public MachineStatus Status { get; private set; }
        public DateTimeOffset LastStatusChangedAt { get; private set; }

        public static Machine Create(
            string code,
            string name,
            MachineType type)
        {
            if (string.IsNullOrWhiteSpace(code))
            {
                throw new DomainException("El código de máquina es requerido");
            }

            if (code.Length > 50)
            {
                throw new DomainException("El código de máquina no puede exceder los 50 caracteres");
            }

            if (string.IsNullOrWhiteSpace(name))
            {
                throw new DomainException("El nombre de máquina es requerido");
            }

            if (name.Length > 100)
            {
                throw new DomainException("El nombre de máquina no puede exceder los 100 caracteres");
            }

            return new Machine(MachineId.New(), code.Trim(), name.Trim(), type);
        }

        public void BringOnline()
        {
            if (Status is not MachineStatus.Offline)
            {
                throw new DomainException(
                    "Solo una máquina que esté fuera de línea puede ponerse en línea.");
            }
            ChangeStatus(MachineStatus.Idle);
        }

        public void Start()
        {
            if (Status is MachineStatus.Running)
            {
                throw new DomainException(
                    "La máquina ya está en marcha. ");
            }

            if (Status is MachineStatus.Offline)
            {
                throw new DomainException(
                    "No se puede iniciar una máquina que está fuera de línea.");
            }

            if (Status is MachineStatus.Maintenance)
            {
                throw new DomainException(
                    "Una máquina en mantenimiento no puede ser iniciada.");
            }

            if (Status is MachineStatus.Faulted)
            {
                throw new DomainException(
                    "Una máquina en fallo debe ser reiniciada antes de iniciarla.");
            }

            ChangeStatus(MachineStatus.Running);
        }

        public void Stop()
        {
            if (Status is not MachineStatus.Offline)
            {
                throw new DomainException(
                    "Una máquina desconectada no puede detenerse.");
            }

            ChangeStatus(MachineStatus.Stopped);
        }

        public void SetMaintenance()
        {
            if (Status is MachineStatus.Running)
            {
                throw new DomainException(
                    "Una máquina en funcionamiento no puede entrar en mantenimiento.");
            }

            ChangeStatus(MachineStatus.Maintenance);
        }

        public void Reset()
        {
            if (Status is not MachineStatus.Faulted)
            {
                throw new DomainException(
                    "Solo se puede reiniciar una máquina que presente un fallo.");
            }

            ChangeStatus(MachineStatus.Idle);
        }

        private void ChangeStatus(MachineStatus newStatus)
        {
            Status = newStatus;
            LastStatusChangedAt = DateTimeOffset.UtcNow;
        }

        public void ReportFault()
        {
            if (Status is MachineStatus.Offline)
            {
                throw new DomainException(
                    "Una máquina desconectada no puede reportar un fallo.");
            }

            ChangeStatus(MachineStatus.Faulted);
        }
    }
}
