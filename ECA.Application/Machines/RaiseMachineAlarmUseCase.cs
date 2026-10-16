using ECA.Domain.Machines;
using ECA.Domain.Machines.Alarms;

namespace ECA.Application.Machines;

public sealed class RaiseMachineAlarmUseCase
{
    private readonly IMachineRepository machineRepository;

    public RaiseMachineAlarmUseCase(IMachineRepository machineRepository)
    {
        this.machineRepository = machineRepository;
    }

    public async Task<MachineAlarmId?> ExecuteAsync(
        MachineId machineId,
        string code,
        string message,
        AlarmSeverity severity,
        CancellationToken cancellationToken = default)
    {
        var machine = await machineRepository.GetByIdAsync(machineId, cancellationToken);

        if (machine is null)
        {
            return null;
        }

        var alarm = machine.RaiseAlarm(code, message, severity);

        await machineRepository.SaveAsync(machine, cancellationToken);

        return alarm.Id;
    }
}
