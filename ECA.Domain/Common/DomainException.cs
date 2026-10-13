namespace ECA.Domain.Common;

/// <summary>
/// Representa una excepción provocada por la infracción
/// de una regla de negocio del dominio.
/// </summary>
public sealed class DomainException : Exception
{
    /// <summary>
    /// Inicializa una nueva excepción de dominio.
    /// </summary>
    /// <param name="message">
    /// Mensaje que describe la regla de negocio incumplida.
    /// </param>
    public DomainException(string message)
        : base(message)
    {
    }
}
