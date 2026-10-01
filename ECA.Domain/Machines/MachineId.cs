namespace ECA.Domain.Machines
{
    public readonly record struct MachineId(Guid Value)
    {
        public static MachineId New()
            => new(Guid.NewGuid());
    }
}
