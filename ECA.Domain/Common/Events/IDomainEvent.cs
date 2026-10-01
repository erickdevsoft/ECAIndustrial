namespace ECA.Domain.Common.Events;

/// <summary>
/// Representa un evento producido dentro del dominio.
///
/// Un Domain Event describe algo que ya ha ocurrido en el negocio.
/// El dominio no decide qué hacer posteriormente con ese evento.
///
/// Esto permite que otras capas reaccionen al evento sin introducir
/// dependencias hacia infraestructura, mensajería, base de datos, etc.
/// </summary>
public interface IDomainEvent
{
    /// <summary>
    /// Momento en el que ocurrió el evento.
    ///
    /// Utilizamos DateTimeOffset para representar correctamente
    /// un instante temporal y evitar ambigüedades de zona horaria.
    /// </summary>
    DateTimeOffset OccurredOn { get; }
}
