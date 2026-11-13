using Redes.Knowledge.Domain;
using Redes.Knowledge.Infrastructure;
using Redes.Knowledge.Infrastructure.Iana;

namespace Redes.Knowledge.App.ViewModels;

/// <summary>
/// Estado del catálogo (F3–F6) y consultas puras sobre él, sin dependencia de Avalonia.
/// <para>
/// MainWindow conserva solo la composición visual y el estado de presentación (zoom,
/// selección, captura abierta); todo lo que se puede ejercitar sin UI vive aquí y se
/// prueba en <c>Redes.Knowledge.Tests</c>.
/// </para>
/// </summary>
public sealed class CatalogoViewModel
{
    private readonly Dictionary<string, Protocol> _protocolos = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, Protocol> _normalizados = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, IReadOnlyList<Field>> _campos = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, string> _pdu = new(StringComparer.OrdinalIgnoreCase);
    private Dictionary<string, string> _cifrado = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, CatalogJson.NotaFuente> _notas = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, FichaPrioritaria> _fichas = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, string> _familias = new(StringComparer.OrdinalIgnoreCase);
    private readonly List<Relationship> _relaciones = new();

    /// <summary>Protocolos indexados por su identificador de catálogo.</summary>
    public IReadOnlyDictionary<string, Protocol> Protocolos => _protocolos;

    /// <summary>Protocolos indexados por acrónimo y por nombre, ambos normalizados.</summary>
    public IReadOnlyDictionary<string, Protocol> Normalizados => _normalizados;

    /// <summary>Campos PDU (F5) por acrónimo; solo los protocolos que los tienen catalogados.</summary>
    public IReadOnlyDictionary<string, IReadOnlyList<Field>> Campos => _campos;

    /// <summary>PDU resumida (F5) por acrónimo.</summary>
    public IReadOnlyDictionary<string, string> Pdu => _pdu;

    /// <summary>Cifrado (F6) por acrónimo.</summary>
    public IReadOnlyDictionary<string, string> Cifrado => _cifrado;

    /// <summary>Notas de fuente (F3) por identificador de protocolo.</summary>
    public IReadOnlyDictionary<string, CatalogJson.NotaFuente> Notas => _notas;

    /// <summary>Fichas prioritarias (F4) por acrónimo.</summary>
    public IReadOnlyDictionary<string, FichaPrioritaria> Fichas => _fichas;

    /// <summary>Descripción de familia (F3) por acrónimo de familia.</summary>
    public IReadOnlyDictionary<string, string> Familias => _familias;

    /// <summary>Relaciones de encapsulación y dependencia (F4).</summary>
    public IReadOnlyList<Relationship> Relaciones => _relaciones;

    /// <summary>Deduplicación fina IANA (D2-2): sinónimos agrupados y vínculos al catálogo.</summary>
    public ResultadoDedup Dedup { get; private set; } =
        new(new Dictionary<string, string>(), Array.Empty<ServicioCanonico>(), 0, 0);

    /// <summary>Número de protocolos cargados.</summary>
    public int Total => _protocolos.Count;

    /// <summary>
    /// Carga los catálogos de la Fase I desde <paramref name="raiz"/> y construye los
    /// índices de consulta. Los protocolos y servicios vienen ya resueltos por el
    /// repositorio SQLite, de modo que este tipo no toca la base de datos.
    /// </summary>
    public static CatalogoViewModel Cargar(
        string raiz,
        IEnumerable<Protocol> protocolos,
        IEnumerable<IanaServiceEntry> servicios)
    {
        var vm = new CatalogoViewModel();

        foreach (var p in protocolos) vm._protocolos[p.Id.Value] = p;
        foreach (var p in vm._protocolos.Values)
        {
            vm._normalizados[Normalizar(p.Acronimo)] = p;
            vm._normalizados[Normalizar(p.Nombre)] = p;
        }

        var f3 = Path.Combine(raiz, "FASE-03-INVENTARIO", "F3-Protocolos.json");
        foreach (var kv in CatalogJson.CargarNotasFuenteF3(f3)) vm._notas[kv.Key] = kv.Value;
        foreach (var kv in CatalogJson.CargarFamiliasF3(f3)) vm._familias[kv.Key] = kv.Value;

        // Campos PDU (F5) para cualquier protocolo del catálogo que los tenga catalogados,
        // no solo los seis iniciales.
        var f5 = Path.Combine(raiz, "FASE-05-MENSAJERIA", "F5-Campos-PDU.json");
        foreach (var p in vm._protocolos.Values)
        {
            var campos = CatalogJson.CargarCamposF5(f5, p.Acronimo);
            if (campos.Count > 0) vm._campos[p.Acronimo] = campos;
            var pdu = CatalogoExploracion.ObtenerPduF5(f5, p.Acronimo);
            if (pdu is not null) vm._pdu[p.Acronimo] = pdu;
        }

        vm._cifrado = CatalogoExploracion.CargarSeguridadF6(
                Path.Combine(raiz, "FASE-06-SEGURIDAD", "F6-Seguridad-Protocolos.json"), "cifrado")
            .ToDictionary(e => e.Key, e => e.Value, StringComparer.OrdinalIgnoreCase);

        vm._relaciones.AddRange(CatalogoExploracion.CargarRelacionesF4(
            Path.Combine(raiz, "FASE-04-PROFUNDIZACION", "F4-Matriz-Encapsulacion.json")));

        foreach (var kv in CatalogoExploracion.CargarFichasF4(
                     Path.Combine(raiz, "FASE-04-PROFUNDIZACION", "F4-Fichas-Prioritarias.json")))
            vm._fichas[kv.Key] = kv.Value;

        vm.Dedup = ServiciosDedup.Agrupar(servicios);
        return vm;
    }

    /// <summary>Protocolos que pasan los filtros de familia y estado (nulos = sin filtro).</summary>
    public IEnumerable<Protocol> Activos(FamiliaProtocolo? familia, LifecycleState? estado)
    {
        IEnumerable<Protocol> lista = _protocolos.Values;
        if (familia is { } f) lista = lista.Where(p => p.Familia == f);
        if (estado is { } e) lista = lista.Where(p => p.Estado == e);
        return lista;
    }

    /// <summary>Resuelve un acrónimo o nombre de protocolo contra el catálogo.</summary>
    public Protocol? Resolver(string nombre)
        => _normalizados.TryGetValue(Normalizar(nombre), out var p) ? p : null;

    /// <summary>Resuelve un protocolo por el identificador usado en los resultados de búsqueda.</summary>
    public Protocol? PorUrn(string urn)
        => _protocolos.TryGetValue(urn, out var p) ? p : null;

    /// <summary>Campos PDU (F5) del protocolo, o <c>null</c> si no están catalogados.</summary>
    public IReadOnlyList<Field>? CamposF5De(string acronimo)
        => _campos.TryGetValue(acronimo, out var c) ? c : null;

    /// <summary>PDU resumida (F5) del protocolo, o <c>null</c>.</summary>
    public string? PduDe(string acronimo)
        => _pdu.TryGetValue(acronimo, out var v) ? v : null;

    /// <summary>Cifrado (F6) del protocolo, o <c>null</c>.</summary>
    public string? CifradoDe(string acronimo)
        => _cifrado.TryGetValue(acronimo, out var v) ? v : null;

    /// <summary>Ficha prioritaria (F4) del protocolo, o <c>null</c>.</summary>
    public FichaPrioritaria? FichaDe(string acronimo)
        => _fichas.TryGetValue(acronimo, out var v) ? v : null;

    /// <summary>Nota de fuente (F3) del protocolo, o <c>null</c>.</summary>
    public CatalogJson.NotaFuente? NotaDe(string id)
        => _notas.TryGetValue(id, out var v) ? v : null;

    /// <summary>Descripción de la familia, o <c>null</c>.</summary>
    public string? FamiliaDe(string acronimo)
        => _familias.TryGetValue(acronimo, out var v) ? v : null;

    /// <summary>Vecinos a un salto en el grafo F4.</summary>
    public IReadOnlyList<Vecino> VecinosDe(string acronimo)
        => GrafoRelaciones.Vecinos1Salto(acronimo, _relaciones);

    /// <summary>
    /// Puertos IANA deduplicados y enlazados al protocolo (D2-2): unión de (a) servicios
    /// cuyo nombre normalizado coincide con el acrónimo y (b) servicios cuyo vínculo
    /// curado IANA→F3 apunta al protocolo (p. ej. HTTP recoge http, www-http, http-alt).
    /// </summary>
    public string PuertosDe(string acronimo)
    {
        var vistos = new SortedSet<(int Puerto, string Transporte)>();
        var acrNorm = ServiciosDedup.Normalizar(acronimo);
        foreach (var s in Dedup.Servicios)
        {
            var coincideExacto = ServiciosDedup.Normalizar(s.Nombre) == acrNorm;
            var vinculado = VinculoServicios.AcronimoDe(s.Nombre);
            var coincideVinculo = vinculado is not null &&
                string.Equals(vinculado, acronimo, StringComparison.OrdinalIgnoreCase);
            if (coincideExacto || coincideVinculo)
                foreach (var p in s.Puertos) vistos.Add(p);
        }
        return vistos.Count == 0
            ? "—"
            : string.Join(", ", vistos.Select(v => $"{v.Puerto}/{v.Transporte}"));
    }

    /// <summary>Normaliza acrónimos y nombres para compararlos sin signos ni mayúsculas.</summary>
    public static string Normalizar(string s)
        => string.Concat(s.Where(char.IsLetterOrDigit)).ToLowerInvariant();
}
