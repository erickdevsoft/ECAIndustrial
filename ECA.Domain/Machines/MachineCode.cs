namespace ECA.Domain.Machines;

/// <summary>
/// Representa el código identificativo de una máquina.
///
/// Es un Value Object porque su significado depende de su valor
/// y no de una identidad propia.
///
/// Además, centraliza las reglas que debe cumplir un código válido.
/// </summary>
public sealed record MachineCode
{
    /// <summary>
    /// Constructor privado para impedir que se creen códigos
    /// sin pasar primero por las validaciones de Create().
    /// </summary>
    private MachineCode(string value)
    {
        Value = value;
    }

    /// <summary>
    /// Valor textual del código.
    /// </summary>
    public string Value { get; }

    /// <summary>
    /// Crea un MachineCode válido aplicando las reglas del dominio.
    /// </summary>
    public static MachineCode Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException(
                "El código de la máquina es obligatorio.",
                nameof(value));
        }

        var normalizedValue = value.Trim();

        if (normalizedValue.Length > 50)
        {
            throw new ArgumentException(
                "El código de la máquina no puede superar los 50 caracteres.",
                nameof(value));
        }

        return new MachineCode(normalizedValue);
    }

    /// <summary>
    /// Permite utilizar el Value Object de forma sencilla cuando
    /// necesitamos su representación textual.
    /// </summary>
    public override string ToString()
        => Value;
}
