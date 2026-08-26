namespace Redes.Knowledge.Domain;

public sealed record ValidationResult
{
    public bool IsValid => Errors.Count == 0;
    public IReadOnlyList<string> Errors { get; init; } = Array.Empty<string>();

    public static ValidationResult Ok() => new();
    public static ValidationResult Fail(params string[] errores) => new() { Errors = errores };
}

/// <summary>Validaciones de dominio (sin dependencias externas). Reglas de la plantilla de ficha F4.</summary>
public static class ProtocolValidator
{
    public static ValidationResult Validate(Protocol p)
    {
        var errores = new List<string>();
        if (string.IsNullOrWhiteSpace(p.Nombre)) errores.Add("El protocolo requiere Nombre.");
        if (string.IsNullOrWhiteSpace(p.Acronimo)) errores.Add("El protocolo requiere Acronimo.");
        if (!Enum.IsDefined(p.Familia)) errores.Add($"Familia no válida: {p.Familia}.");
        if (!Enum.IsDefined(p.Estado)) errores.Add($"Estado de ciclo de vida no válido: {p.Estado}.");
        if (p.ValidFrom is { } desde && p.ValidTo is { } hasta && desde > hasta)
            errores.Add("valid_from no puede ser posterior a valid_to.");
        return errores.Count == 0 ? ValidationResult.Ok() : ValidationResult.Fail(errores.ToArray());
    }
}

public static class SourceValidator
{
    public static ValidationResult Validate(Source s)
    {
        var errores = new List<string>();
        if (string.IsNullOrWhiteSpace(s.Titulo)) errores.Add("La fuente requiere Titulo.");
        if (string.IsNullOrWhiteSpace(s.Url)) errores.Add("La fuente requiere Url (o identificador).");
        if (string.IsNullOrWhiteSpace(s.Version)) errores.Add("La fuente requiere Version (número concreto).");
        if (s.FechaConsulta is null) errores.Add("La fuente requiere FechaConsulta (política F0).");
        if (!Enum.IsDefined(s.Nivel)) errores.Add($"Nivel de autoridad no válido: {s.Nivel}.");
        return errores.Count == 0 ? ValidationResult.Ok() : ValidationResult.Fail(errores.ToArray());
    }
}