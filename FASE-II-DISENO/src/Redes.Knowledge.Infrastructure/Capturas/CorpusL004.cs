using Redes.Knowledge.Infrastructure.Iana;

namespace Redes.Knowledge.Infrastructure.Capturas;

public sealed record FicheroL004(string Nombre, int Paquetes, int CapasReconocidas, int ProtocolosDistintos);

public sealed record EstadisticaL004(string Protocolo, int Paquetes, int CamposTotal, int CamposEnLimites, bool AlgunOkCompleto);

public sealed record ResultadoL004(
    IReadOnlyList<FicheroL004> Ficheros,
    IReadOnlyList<EstadisticaL004> PorProtocolo,
    IReadOnlyList<string> ProtocolosF5,
    IReadOnlyList<string> SinLayoutEnCorpus,
    string Informe);

/// <summary>
/// Validación cruzada L-004: paquetes REALES del corpus ↔ layouts F5-Campos-PDU.json.
/// Por cada paquete se recorren las capas detectadas (<see cref="PcapDissector.DisectarCapas"/>)
/// y se validan sus campos (base bits según la capa; ETH con base 64 por el preámbulo físico).
/// Genera un informe Markdown como evidencia. No modifica datos: solo lee y reporta.
/// </summary>
public static class CorpusL004
{
    public static ResultadoL004 Validar(string carpeta, string jsonF5, string? informePath = null)
    {
        var layouts = CargarLayoutsF5(jsonF5);
        var ficheros = new List<FicheroL004>();
        var acum = new Dictionary<string, (int Paquetes, int Campos, int EnLimites, bool OkCompleto)>(StringComparer.Ordinal);
        var vistos = new HashSet<string>(StringComparer.Ordinal);

        foreach (var ruta in Directory.EnumerateFiles(carpeta, "*", SearchOption.TopDirectoryOnly)
                     .Where(f => f.EndsWith(".pcap", StringComparison.OrdinalIgnoreCase) ||
                                 f.EndsWith(".pcapng", StringComparison.OrdinalIgnoreCase))
                     .OrderBy(f => f, StringComparer.Ordinal))
        {
            PcapCapture captura;
            try { captura = PcapCaptureReader.Abrir(ruta); }
            catch (Exception ex)
            {
                ficheros.Add(new FicheroL004(Path.GetFileName(ruta), 0, 0, 0));
                continue; // fichero no parseable: se anota sin paquetes
            }

            var capasReconocidas = 0;
            var protocolosEnElFichero = new HashSet<string>(StringComparer.Ordinal);
            foreach (var paquete in captura.Paquetes)
            {
                foreach (var capa in PcapDissector.DisectarCapas(paquete.Data))
                {
                    if (!layouts.TryGetValue(capa.AcronimoF5, out var campos)) continue;
                    capasReconocidas++;
                    protocolosEnElFichero.Add(capa.AcronimoF5);

                    var buffer = paquete.Data.AsSpan(capa.InicioBytes, capa.LongitudBytes).ToArray();
                    var validados = PcapDissector.Validar(buffer, campos, capa.BaseBits)
                        .Where(v => !(capa.AcronimoF5 == "ETH" && v.Nombre is ("Preamble" or "SFD")))
                        .ToList(); // los campos físicos de ETH no viajan en la trama capturada
                    var resumen = PcapDissector.Resumen(capa.AcronimoF5, validados);

                    if (!acum.TryGetValue(capa.AcronimoF5, out var a))
                        a = (0, 0, 0, true);
                    acum[capa.AcronimoF5] = (a.Paquetes + 1, a.Campos + resumen.TotalCampos,
                        a.EnLimites + resumen.CamposEnLimites, a.OkCompleto && resumen.Ok);
                }
                foreach (var capa in PcapDissector.DisectarCapas(paquete.Data)) vistos.Add(capa.AcronimoF5);
            }
            ficheros.Add(new FicheroL004(Path.GetFileName(ruta), captura.Paquetes.Count, capasReconocidas, protocolosEnElFichero.Count));
        }

        var porProtocolo = acum
            .OrderBy(kv => kv.Key, StringComparer.OrdinalIgnoreCase)
            .Select(kv => new EstadisticaL004(kv.Key, kv.Value.Paquetes, kv.Value.Campos,
                kv.Value.EnLimites, kv.Value.OkCompleto))
            .ToList();
        var sinLayout = layouts.Keys.Where(k => !vistos.Contains(k)).OrderBy(k => k, StringComparer.OrdinalIgnoreCase).ToList();

        var informe = Redactar(ficheros, porProtocolo, layouts.Keys.OrderBy(k => k, StringComparer.OrdinalIgnoreCase).ToList(), sinLayout);
        if (informePath is not null)
        {
            var dir = Path.GetDirectoryName(informePath);
            if (!string.IsNullOrEmpty(dir)) Directory.CreateDirectory(dir);
            File.WriteAllText(informePath, informe);
        }
        return new ResultadoL004(ficheros, porProtocolo, layouts.Keys.ToList(), sinLayout, informe);
    }

    private static Dictionary<string, IReadOnlyList<CampoDefinido>> CargarLayoutsF5(string jsonF5)
    {
        var resultado = new Dictionary<string, IReadOnlyList<CampoDefinido>>(StringComparer.OrdinalIgnoreCase);
        foreach (var p in CatalogJson.CargarCatalogosF5(jsonF5))
        {
            var campos = p.Campos
                .Where(c => c.OffsetBits.HasValue)
                .Select(c => new CampoDefinido(c.OffsetBits!.Value, c.LongitudBits, c.Nombre))
                .ToList();
            resultado[p.Acronimo] = campos;
        }
        return resultado;
    }

    private static string Redactar(IReadOnlyList<FicheroL004> ficheros, IReadOnlyList<EstadisticaL004> porProtocolo,
        IReadOnlyList<string> todosF5, IReadOnlyList<string> sinLayout)
    {
        var sb = new System.Text.StringBuilder();
        sb.AppendLine("# L-004 — Validación de layouts F5 contra corpus real");
        sb.AppendLine();
        sb.AppendLine($"Fecha: {DateTime.UtcNow:yyyy-MM-dd}");
        sb.AppendLine($"Corpus: {ficheros.Count} captura(s) PCAP/PCAPNG reales (repositorio de Wireshark).");
        sb.AppendLine($"Layouts F5 considerados: {todosF5.Count}.");
        sb.AppendLine();
        sb.AppendLine("## Por fichero");
        sb.AppendLine();
        sb.AppendLine("| Captura | Paquetes | Capas reconocidas | Protocolos distintos |");
        sb.AppendLine("|---|---|---|---|");
        foreach (var f in ficheros) sb.AppendLine($"| {f.Nombre} | {f.Paquetes} | {f.CapasReconocidas} | {f.ProtocolosDistintos} |");
        sb.AppendLine();
        sb.AppendLine("## Por protocolo (acumulado)");
        sb.AppendLine();
        sb.AppendLine("| Protocolo | Paquetes | Campos | En límites | % | Algún paquete 100% OK |");
        sb.AppendLine("|---|---|---|---|---|---|");
        foreach (var e in porProtocolo)
        {
            var pct = e.CamposTotal == 0 ? 0 : (double)e.CamposEnLimites * 100 / e.CamposTotal;
            sb.AppendLine($"| {e.Protocolo} | {e.Paquetes} | {e.CamposTotal} | {e.CamposEnLimites} | {pct:0.0} % | {(e.AlgunOkCompleto ? "sí" : "no")} |");
        }
        sb.AppendLine();
        if (sinLayout.Count > 0)
        {
            sb.AppendLine($"## Sin paquetes en el corpus ({sinLayout.Count}/{todosF5.Count})");
            sb.AppendLine();
            sb.AppendLine(string.Join(", ", sinLayout));
            sb.AppendLine();
            sb.AppendLine("Nota honesta: los campos fuera de límites suelen deberse a capturas truncadas, " +
                          "cabeceras con opciones o variantes que el paquete no alcanza a transmitir; " +
                          "el indicador clave es % de campos en límites y la existencia de paquetes 100 % OK.");
        }
        return sb.ToString();
    }
}