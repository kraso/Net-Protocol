namespace Redes.Knowledge.Domain;

/// <summary>
/// Clave estable tipo URN, separada del nombre mostrado (regla N2 del glosario F0).
/// Ejemplo: urn:proto:ietf:rfc9114
/// </summary>
public readonly record struct Urn
{
    public string Value { get; }

    public Urn(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("La URN no puede estar vacía.", nameof(value));
        Value = value;
    }

    public static Urn Parse(string s) => new(s);

    public static Urn Protocol(string familia, string acronimo)
        => new($"urn:proto:{familia.ToLowerInvariant()}:{Normalizar(acronimo)}");

    private static string Normalizar(string texto)
        => string.Concat(texto.Where(char.IsLetterOrDigit)).ToLowerInvariant();

    public override string ToString() => Value;
}