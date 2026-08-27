using Redes.Knowledge.Infrastructure.Iana;

namespace Redes.Knowledge.Infrastructure;

/// <summary>
/// Deduplicación fina y entity-linking del registro IANA (D2-2).
/// - Dedup: agrupa sinónimos (normalización laxa: minúsculas + solo alfanuméricos, p. ej.
///   "http"/"www-http"/"HTTP") y une puertos repetidos dentro del grupo.
/// - Entity-linking: vínculo determinista servicio IANA → protocolo del catálogo F3,
///   con un mapa CURADO y documentado de alias bien conocidos (nunca inventado en runtime).
/// </summary>
public static class ServiciosDedup
{
    /// <summary>Normalización laxa de un nombre de servicio (minúsculas, sin puntuación).</summary>
    public static string Normalizar(string s)
        => string.Concat(s.Where(char.IsLetterOrDigit)).ToLowerInvariant();

    /// <summary>Deduplicación determinista sobre las entradas IANA.</summary>
    public static ResultadoDedup Agrupar(IEnumerable<IanaServiceEntry> entradas)
    {
        var grupos = new SortedDictionary<string, List<IanaServiceEntry>>(StringComparer.Ordinal);
        foreach (var e in entradas)
        {
            var clave = Normalizar(e.ServiceName);
            if (!grupos.TryGetValue(clave, out var lista)) grupos[clave] = lista = new List<IanaServiceEntry>();
            lista.Add(e);
        }

        var canonicoPorNombre = new Dictionary<string, string>(StringComparer.Ordinal);
        var servicios = new List<ServicioCanonico>();
        var sinonimos = 0;
        var puertosDuplicados = 0;

        foreach (var (clave, lista) in grupos)
        {
            // Nombre canónico: el más corto; en empate, minúsculas primero y luego orden estable.
            var canonico = lista
                .OrderBy(e => e.ServiceName.Length)
                .ThenBy(e => e.ServiceName.Any(char.IsUpper)) // "http" antes que "HTTP"
                .ThenBy(e => e.ServiceName, StringComparer.Ordinal)
                .First().ServiceName;
            canonicoPorNombre[clave] = canonico;

            var puertos = new SortedSet<(int Puerto, string Transporte)>();
            foreach (var e in lista)
            {
                if (e.Port is not { } puerto || string.IsNullOrWhiteSpace(e.Transport)) continue;
                if (!puertos.Add((puerto, e.Transport.ToUpperInvariant()))) puertosDuplicados++;
            }
            servicios.Add(new ServicioCanonico(canonico, puertos.ToList()));
            sinonimos += lista.Count - 1;
        }

        return new ResultadoDedup(canonicoPorNombre, servicios, sinonimos, puertosDuplicados);
    }
}

/// <summary>Resultado de la deduplicación: nombre canónico por clave normalizada y servicios agrupados.</summary>
public sealed record ResultadoDedup(
    IReadOnlyDictionary<string, string> CanonicoPorNombre,
    IReadOnlyList<ServicioCanonico> Servicios,
    int SinonimosAgrupados,
    int PuertosDuplicados);

/// <summary>Servicio agrupado: nombre canónico + sus puertos distintos (puerto, transporte).</summary>
public sealed record ServicioCanonico(string Nombre, IReadOnlyList<(int Puerto, string Transporte)> Puertos);

/// <summary>
/// Vínculo determinista servicio IANA → acrónimo del catálogo F3. El mapa curado documenta
/// los alias oficiales del registro IANA para los protocolos más conocidos (no se generan
/// enlaces en runtime para no inventar correspondencias).
/// </summary>
public static class VinculoServicios
{
    /// <summary>Nombre de servicio IANA (minúsculas) → acrónimo F3 del catálogo.</summary>
    public static IReadOnlyDictionary<string, string> AliasF3 { get; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
    {
        // Web / transporte
        ["www"] = "HTTP", ["www-http"] = "HTTP", ["http-alt"] = "HTTP",
        ["https"] = "HTTPS",
        // Ficheros
        ["ftp-data"] = "FTP", ["ftps"] = "FTP", ["ftps-data"] = "FTP",
        // Correo
        ["smtp"] = "SMTP", ["submission"] = "SMTP",
        ["pop3"] = "POP3", ["pop3s"] = "POP3",
        ["imap"] = "IMAP", ["imaps"] = "IMAP",
        // Resolución de nombres y configuración
        ["domain"] = "DNS", ["domain-s"] = "DNS",
        ["bootps"] = "DHCP", ["bootpc"] = "DHCP",
        ["dhcp"] = "DHCP", ["dhcpv6-client"] = "DHCPv6", ["dhcpv6-server"] = "DHCPv6",
        ["ntp"] = "NTP",
        // Operación
        ["ssh"] = "SSH",
        ["telnet"] = "Telnet",
        ["snmp"] = "SNMP",
        ["syslog"] = "Syslog",
        ["bgp"] = "BGP",
        ["rip"] = "RIP",
        ["sip"] = "SIP",
        ["rtp"] = "RTP",
        ["rtsp"] = "RTSP",
        ["tftp"] = "TFTP",
        ["ldap"] = "LDAP",
        ["mqtt"] = "MQTT",
        ["coap"] = "CoAP",
        ["quic"] = "QUIC"
    };

    /// <summary>Acrónimo F3 vinculado a un nombre de servicio (por alias curado), o null.</summary>
    public static string? AcronimoDe(string nombreServicio)
        => AliasF3.TryGetValue(nombreServicio, out var acronimo) ? acronimo : null;
}