namespace Redes.Knowledge.Infrastructure.Iana;

/// <summary>
/// Importador del registro oficial de IANA (Service Name and Transport Protocol Port Number Registry).
/// Reglas de la Fase 3: el registro se consume como FUENTE DE DATOS; se registra la fecha de consulta;
/// puerto registrado ≠ protocolo (no se crean protocolos a partir de puertos). Deduplicación por
/// (nombre, puerto, transporte).
/// </summary>
public static class IanaServiceImporter
{
    private const string CabeceraEsperada = "Service Name";

    public static IanaImportResult Importar(string csvPath, DateTime? fechaConsulta = null)
    {
        var fecha = fechaConsulta ?? DateTime.UtcNow.Date;
        var lineas = File.ReadAllLines(csvPath);
        if (lineas.Length == 0 || !lineas[0].Contains(CabeceraEsperada, StringComparison.OrdinalIgnoreCase))
            throw new InvalidDataException("El archivo no tiene la cabecera esperada del registro IANA.");

        var entradas = new List<IanaServiceEntry>();
        var vistos = new HashSet<(string, int?, string)>();
        var sinNombre = 0;
        var sinPuerto = 0;

        for (var i = 1; i < lineas.Length; i++)
        {
            var raw = lineas[i];
            if (string.IsNullOrWhiteSpace(raw)) continue;

            var f = CsvReader.ParseLine(raw);
            var nombre = f.Count > 0 ? f[0].Trim().ToLowerInvariant() : string.Empty;
            if (string.IsNullOrEmpty(nombre)) { sinNombre++; continue; }

            int? puerto = null;
            if (f.Count > 1 && int.TryParse(f[1].Trim(), out var p)) puerto = p;
            else sinPuerto++;

            var transporte = f.Count > 2 ? f[2].Trim().ToUpperInvariant() : string.Empty;
            var descripcion = f.Count > 3 ? f[3].Trim() : string.Empty;
            var fechaRegistro = f.Count > 6 ? f[6].Trim() : string.Empty;
            var referencia = f.Count > 8 ? f[8].Trim() : string.Empty;

            if (!vistos.Add((nombre, puerto, transporte))) continue; // deduplicación

            entradas.Add(new IanaServiceEntry(nombre, puerto, transporte, descripcion, referencia, fechaRegistro));
        }

        return new IanaImportResult(lineas.Length, sinNombre, sinPuerto, entradas.Count, fecha, entradas);
    }
}