namespace ECA.Domain.Machines
{
    public enum MachineStatus
    {
        Offline = 1,
        Idle = 2,
        Running = 3,
        Stopped = 4,
        Maintenance = 5,
        Faulted = 6
    }
}
