using ECA.Domain.Machines;

namespace ECA.Application.Machines;

/// <summary>
/// Define las operaciones de persistencia que los casos de uso
/// necesitan para recuperar y guardar máquinas.
/// </summary>
public interface IMachineRepository
{
    /// <summary>
    /// Busca una máquina por su identidad.
    ///
    /// Devuelve null cuando no existe ninguna máquina con ese identificador.
    /// </summary>
    Task<Machine?> GetByIdAsync(
        MachineId id,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Guarda los cambios realizados en una máquina.
    /// </summary>
    Task SaveAsync(
        Machine machine,
        CancellationToken cancellationToken = default);
}
