using ECA.Domain.Machines;
using ECA.Domain.Machines.Alarms;

namespace ECA.Application.Machines;

/// <summary>
/// Coordina el caso de uso de desactivación de una alarma.
/// </summary>
public sealed class ClearMachineAlarmUseCase
{
    // Repositorio utilizado para recuperar y guardar máquinas.
    private readonly IMachineRepository machineRepository;

    /// <summary>
    /// Inicializa el caso de uso con el repositorio necesario.
    /// </summary>
    public ClearMachineAlarmUseCase(
        IMachineRepository machineRepository)
    {
        this.machineRepository = machineRepository;
    }

    /// <summary>
    /// Ejecuta la desactivación de una alarma.
    /// </summary>
    public async Task<bool> ExecuteAsync(
        MachineId machineId,
        MachineAlarmId alarmId,
        CancellationToken cancellationToken = default)
    {
        var machine = await machineRepository.GetByIdAsync(machineId, cancellationToken);

        if (machine is null)
        {
            return false;
        }

        machine.ClearAlarm(alarmId);

        

        await machineRepository.SaveAsync(machine, cancellationToken);

        return true;
    }
}
