using System.Globalization;
using Redes.Knowledge.Domain;

namespace Redes.Knowledge.Visualization;

/// <summary>
/// Catálogo de textos de la interfaz en español e inglés. Sin dependencia de Avalonia:
/// la ventana pide las cadenas y las aplica, de modo que el catálogo se prueba en frío.
/// El idioma activo es estado estático del proceso; <see cref="Detectar"/> lo resuelve
/// desde NETPROTOCOL_IDIOMA y, si no está, desde LANG/LC_ALL.
/// </summary>
public static class Textos
{
    public const string Es = "es";
    public const string En = "en";

    private static readonly Dictionary<string, (string Es, string En)> Catalogo =
        new(StringComparer.Ordinal)
        {
            // Barra de título y barra superior
            ["app.titulo"] = ("Net Protocol", "Net Protocol"),
            ["buscar.placeholder"] = ("Buscar protocolo, RFC, puerto, campo...", "Search protocol, RFC, port, field..."),
            ["buscar.boton"] = ("Buscar", "Search"),
            ["etiqueta.protocolo"] = ("Protocolo:", "Protocol:"),
            ["etiqueta.comparar"] = ("Comparar con:", "Compare with:"),
            ["boton.comparar"] = ("Comparar", "Compare"),
            ["boton.volver.ficha"] = ("✕ Volver a la ficha", "✕ Back to sheet"),
            ["etiqueta.familia"] = ("Familia:", "Family:"),
            ["etiqueta.estado"] = ("Estado:", "Status:"),
            ["etiqueta.exportar"] = ("Exportar:", "Export:"),
            ["boton.exportar"] = ("Exportar", "Export"),
            ["boton.abrir.captura"] = ("Abrir captura", "Open capture"),
            ["boton.muestra"] = ("Muestra de prueba", "Sample data"),
            ["boton.idioma"] = ("EN", "ES"),

            // Accesibilidad: nombre que expone cada control a lectores de pantalla.
            ["acc.ventana.minimizar"] = ("Minimizar", "Minimize"),
            ["acc.ventana.maximizar"] = ("Maximizar", "Maximize"),
            ["acc.ventana.cerrar"] = ("Cerrar", "Close"),
            ["acc.busqueda"] = ("Búsqueda de protocolos", "Protocol search"),
            ["acc.protocolo"] = ("Selector de protocolo", "Protocol selector"),
            ["acc.comparar.con"] = ("Protocolo de comparación", "Comparison protocol"),
            ["acc.familia"] = ("Filtro por familia", "Filter by family"),
            ["acc.estado"] = ("Filtro por estado", "Filter by status"),
            ["acc.formato.exportacion"] = ("Formato de exportación", "Export format"),
            ["acc.idioma"] = ("Cambiar idioma", "Change language"),
            ["acc.filtro.familias"] = ("Filtro de familias", "Family filter"),
            ["acc.ficha"] = ("Ficha del protocolo", "Protocol sheet"),
            ["acc.diagramas"] = ("Diagramas de arquitectura", "Architecture diagrams"),
            ["acc.estado.barra"] = ("Barra de estado", "Status bar"),
            ["acc.captura.resumen"] = ("Resumen de la captura", "Capture summary"),
            ["acc.captura.paquetes"] = ("Paquetes de la captura", "Capture packets"),
            ["acc.captura.detalle"] = ("Detalle del paquete", "Packet detail"),
            ["acc.lista.protocolos"] = ("Lista de protocolos", "Protocol list"),

            // Ayuda larga de los botones de acción (se expone a lectores de pantalla; el
            // popup visual conserva su propio texto mientras la i18n siga siendo parcial).
            ["help.comparar"] = (
                "Compara el protocolo actual con otro del catálogo. Pulsa «✕ Volver a la ficha» para salir.",
                "Compares the current protocol with another one in the catalog. Press «✕ Back to sheet» to leave."),
            ["help.exportar"] = (
                "Exporta los diagramas de la ficha actual (pila, grafo, cabecera) al formato elegido: SVG, PNG o PDF.",
                "Exports the current sheet diagrams (stack, graph, header) to the chosen format: SVG, PNG or PDF."),
            ["help.abrir.captura"] = (
                "Abre el explorador para elegir un archivo de captura PCAP o PCAPNG y muestra sus paquetes.",
                "Opens the file manager to choose a PCAP or PCAPNG capture file and shows its packets."),
            ["help.muestra"] = (
                "Genera una captura sintética determinista con los 28 protocolos con layout F5.",
                "Generates a deterministic synthetic capture with the 28 protocols that have an F5 layout."),

            // Ficha del protocolo (formateador FichaTexto, en Visualization)
            ["ficha.identidad"] = ("Identidad", "Identity"),
            ["ficha.estado"] = ("Estado", "Status"),
            ["ficha.finalidad"] = ("Finalidad", "Purpose"),
            ["ficha.encapsulacion"] = ("Encapsulación", "Encapsulation"),
            ["ficha.capas"] = ("Capas", "Layers"),
            ["ficha.transporte"] = ("Transp./dir", "Transp./addr"),
            ["ficha.pdu"] = ("PDU", "PDU"),
            ["ficha.mensajes"] = ("Mensajes", "Messages"),
            ["ficha.campos"] = ("Campos", "Fields"),
            ["ficha.secuencia"] = ("Secuencia", "Sequence"),
            ["ficha.addressing"] = ("Addressing", "Addressing"),
            ["ficha.routing"] = ("Routing", "Routing"),
            ["ficha.seguridad"] = ("Seguridad", "Security"),
            ["ficha.qos"] = ("QoS/rendim.", "QoS/perf."),
            ["ficha.observabilidad"] = ("Observabilidad", "Observability"),
            ["ficha.interoperabilidad"] = ("Interoperabilidad", "Interoperability"),
            ["ficha.implementaciones"] = ("Implementaciones", "Implementations"),
            ["ficha.fuentes"] = ("Fuentes", "Sources"),
            ["ficha.familia"] = ("familia", "family"),
            ["ficha.estado.f3"] = (
                "{0} (catálogo F3, 2026-08-26)",
                "{0} (F3 catalog, 2026-08-26)"),
            ["ficha.cifrado"] = ("cifrado: {0} [F6]", "encrypted: {0} [F6]"),
            ["ficha.nota"] = ("Nota/F3", "Note/F3"),
            ["ficha.vecinos"] = ("--- Vecinos a 1 salto (grafo F4) ---", "--- 1-hop neighbors (F4 graph) ---"),
            ["ficha.campos.titulo"] = ("--- Campos catalogados (F5) ---", "--- Catalogued fields (F5) ---"),

            // Comparador de dos protocolos
            ["cmp.titulo"] = ("=== Comparador: {0} vs {1} ===", "=== Comparison: {0} vs {1} ==="),
            ["cmp.aspecto"] = ("Aspecto", "Aspect"),
            ["cmp.igual"] = ("  (= igual)", "  (= equal)"),
            ["cmp.familia"] = ("Familia", "Family"),
            ["cmp.estado"] = ("Estado", "Status"),
            ["cmp.capas"] = ("Capas", "Layers"),
            ["cmp.pdu"] = ("PDU", "PDU"),
            ["cmp.puertos"] = ("Puertos (IANA)", "Ports (IANA)"),
            ["cmp.cifrado"] = ("Cifrado (F6)", "Encryption (F6)"),
            ["cmp.finalidad"] = ("Finalidad", "Purpose"),
            ["cmp.encapsulacion"] = ("Encapsulación", "Encapsulation"),
            ["cmp.fuentes"] = (
                "Fuentes: F3 (familia/estado/capas/puertos IANA), F4 (finalidad/encapsulación por ficha y grafo), F5 (PDU), F6 (cifrado).",
                "Sources: F3 (family/status/layers/IANA ports), F4 (purpose/encapsulation per sheet and graph), F5 (PDU), F6 (encryption)."),

            // Vista de captura y servicio IANA
            ["cap.resumen"] = (
                "{0} · linktype {1} ({2}) · {3} paquetes",
                "{0} · linktype {1} ({2}) · {3} packets"),
            ["cap.otro"] = ("otro", "other"),
            ["cap.paquete"] = ("=== Paquete de {0} bytes ===", "=== Packet of {0} bytes ==="),
            ["cap.ipv6.f5"] = ("IPv6: capas y campos en el detalle F5", "IPv6: layers and fields in the F5 detail"),
            ["cap.sin.layout"] = ("{0}: sin layout F5 en el catálogo", "{0}: no F5 layout in the catalog"),
            ["iana.titulo"] = ("=== Servicio IANA: {0} ===", "=== IANA service: {0} ==="),
            ["iana.puertos"] = ("Puertos registrados: {0}", "Registered ports: {0}"),
            ["iana.vinculo"] = (
                "Protocolo vinculado del catálogo: {0} ({1}) [vínculo curado IANA→F3]",
                "Catalog protocol linked: {0} ({1}) [curated IANA→F3 link]"),

            // Navegación
            ["nav.resultados"] = ("Resultados ({0})", "Results ({0})"),
            ["filtro.todas.familias"] = ("Todas las familias", "All families"),
            ["filtro.todos.estados"] = ("Todos los estados", "All states"),

            // Sidebar y vistas textuales
            ["nav.filtro"] = ("Filtrar familias - Ver leyenda", "Filter families - View legend"),
            ["sidebar.acciones"] = ("Acciones", "Actions"),
            ["sidebar.tema"] = ("Tema claro/oscuro", "Light/dark theme"),
            ["sidebar.leyenda"] = ("Leyenda", "Legend"),
            ["sidebar.acerca"] = ("Acerca de", "About"),
            ["diagramas.titulo"] = ("Diagramas de arquitectura:", "Architecture diagrams:"),
            ["boton.cerrar.captura"] = ("✕ Cerrar captura", "✕ Close capture"),

            // Barra de estado
            ["estado.dataset"] = (
                "Dataset: {0} protocolos · {1} servicios IANA · {2} relaciones F4 · importados: {3} · zoom {4} (Ctrl+Scroll)",
                "Dataset: {0} protocols · {1} IANA services · {2} F4 relations · imported: {3} · zoom {4} (Ctrl+Scroll)"),
            ["estado.zoom"] = (
                "Zoom del contenido: {0} — Ctrl+Scroll ajusta el tamaño de letra (reflow, sin desbordes)",
                "Content zoom: {0} — Ctrl+Scroll adjusts the font size (reflow, no overflow)"),
            ["estado.dataset.zoom"] = (
                "Dataset: {0} protocolos · {1} servicios IANA · zoom del contenido {2} (Ctrl+Scroll)",
                "Dataset: {0} protocols · {1} IANA services · content zoom {2} (Ctrl+Scroll)"),
            ["estado.visible"] = (
                "Visible: {0} de {1} protocolos · {2} servicios IANA · zoom {3}",
                "Visible: {0} of {1} protocols · {2} IANA services · zoom {3}"),
            ["estado.sin.resultados"] = ("Sin resultados para la búsqueda.", "No results for the search."),
            ["estado.busqueda.cero"] = ("Búsqueda \"{0}\": 0 resultados.", "Search \"{0}\": 0 results."),
            ["estado.busqueda"] = ("Búsqueda \"{0}\": {1} resultados · zoom {2}", "Search \"{0}\": {1} results · zoom {2}"),
            ["estado.seleccione"] = ("Seleccione un protocolo.", "Select a protocol."),
            ["estado.grafo"] = ("Grafo: {0} · {1} relaciones F4 · zoom {2}", "Graph: {0} · {1} F4 relations · zoom {2}"),
            ["estado.nada.exportar"] = (
                "Nada que exportar: abre la ficha de un protocolo con diagramas.",
                "Nothing to export: open a protocol sheet with diagrams."),
            ["estado.export.png.invalida"] = ("Exportación PNG inválida para {0}.", "Invalid PNG export for {0}."),
            ["estado.export.error"] = ("Error exportando {0}: {1}", "Error exporting {0}: {1}"),
            ["estado.export.ok"] = (
                "Exportados {0} diagrama(s) de {1} como {2} en {3}",
                "Exported {0} diagram(s) of {1} as {2} in {3}"),
            ["estado.captura.cerrada"] = ("Captura cerrada · {0} protocolos · zoom {1}", "Capture closed · {0} protocols · zoom {1}"),
            ["estado.iana"] = ("Servicio IANA \"{0}\" → {1} · {2} puerto(s)", "IANA service \"{0}\" → {1} · {2} port(s)"),
            ["estado.leyenda"] = ("Leyenda de familias · zoom {0}", "Family legend · zoom {0}"),
            ["estado.acerca"] = ("Acerca de · zoom {0}", "About · zoom {0}"),
            ["estado.actualizacion"] = ("Versión {0} disponible · {1}", "Version {0} available · {1}"),
            ["boton.descargar"] = ("Descargar instalador", "Download installer"),
            ["estado.descarga.ok"] = (
                "Instalador descargado · su SHA256 coincide con el publicado en {0}",
                "Installer downloaded · its SHA256 matches the published one at {0}"),
            ["estado.descarga.progreso"] = (
                "Descargando {0}…",
                "Downloading {0}…"),
            ["estado.descarga.hash"] = (
                "El instalador no coincide con el SHA256 publicado: se descartó",
                "The installer does not match the published SHA256: discarded"),
            ["estado.descarga.sin.sumas"] = (
                "La release no publica SHA256SUMS para este instalador",
                "The release does not publish SHA256SUMS for this installer"),
            ["estado.descarga.sin.asset"] = (
                "No hay instalador para este sistema en la release",
                "No installer for this system in the release"),
            ["estado.descarga.error"] = ("No se pudo descargar el instalador: {0}", "Could not download the installer: {0}"),
            ["estado.explorador.error"] = ("No se pudo abrir el explorador: {0}", "Could not open the file manager: {0}"),
            ["estado.captura.error"] = ("No se pudo abrir la captura: {0}", "Could not open the capture: {0}"),
            ["estado.muestra.ok"] = (
                "Muestra sintética (ETH/IPv4/IPv6/TCP/UDP/DNS/ICMP) volcada en {0} · click en un paquete para su detalle F5",
                "Synthetic sample (ETH/IPv4/IPv6/TCP/UDP/DNS/ICMP) written to {0} · click a packet for its F5 detail"),
            ["estado.muestra.error"] = ("No se pudo volcar la muestra: {0}", "Could not write the sample: {0}"),

            // Leyenda de familias
            ["leyenda.titulo"] = ("=== Leyenda de familias de protocolos ===", "=== Protocol family legend ==="),
            ["leyenda.intro"] = (
                "Cada familia agrupa protocolos con un propósito/ámbito común. Acrónimo — descripción (ejemplos del catálogo):",
                "Each family groups protocols with a common purpose/scope. Acronym — description (catalog examples):"),
            ["leyenda.ejemplos"] = (" — ej.: {0}", " — e.g.: {0}"),
            ["leyenda.fuente"] = (
                "Fuente: catálogo F3 (campo 'familias') · {0} protocolos en {1} familias.",
                "Source: F3 catalog ('familias' field) · {0} protocols in {1} families."),

            // Acerca de
            ["acerca.titulo"] = ("=== Acerca de ===", "=== About ==="),
            ["acerca.nombre"] = ("Nombre de la aplicación: Net Protocol", "Application name: Net Protocol"),
            ["acerca.version"] = ("Versión: {0}", "Version: {0}"),
            ["acerca.autor"] = ("Autor: Marcos Calabrés Ibáñez", "Author: Marcos Calabrés Ibáñez"),
            ["acerca.email"] = ("Email: marcoscalabresibaniez@gmail.com", "Email: marcoscalabresibaniez@gmail.com"),
            ["acerca.derechos"] = ("© Todos los derechos reservados", "© All rights reserved"),
        };

    /// <summary>Idioma activo del proceso. Español por defecto.</summary>
    public static string Idioma { get; private set; } = Es;

    /// <summary>
    /// Nombre visible de cada estado del ciclo de vida. El valor del enum no cambia: solo
    /// se traduce lo que se muestra, para no romper filtros ni comparaciones.
    /// </summary>
    private static readonly Dictionary<LifecycleState, (string Es, string En)> Estados =
        new()
        {
            [LifecycleState.Vigente] = ("Vigente", "Current"),
            [LifecycleState.Actualizado] = ("Actualizado", "Updated"),
            [LifecycleState.Obsoleto] = ("Obsoleto", "Obsolete"),
            [LifecycleState.Sustituido] = ("Sustituido", "Superseded"),
            [LifecycleState.Experimental] = ("Experimental", "Experimental"),
            [LifecycleState.Propietario] = ("Propietario", "Proprietary"),
            [LifecycleState.Restringido] = ("Restringido", "Restricted"),
            [LifecycleState.Historico] = ("Histórico", "Historical"),
            [LifecycleState.Desconocido] = ("Desconocido", "Unknown")
        };

    /// <summary>Nombre visible del estado en el idioma activo; si falta, el valor del enum.</summary>
    public static string Estado(LifecycleState estado) =>
        Estados.TryGetValue(estado, out var par) ? (Idioma == En ? par.En : par.Es) : estado.ToString();

    /// <summary>
    /// Igual que <see cref="Estado(LifecycleState)"/> pero partiendo del nombre del enum, que
    /// es como lo entrega el comparador de protocolos. Un valor no reconocido se devuelve tal cual.
    /// </summary>
    public static string Estado(string valor) =>
        Enum.TryParse<LifecycleState>(valor, out var estado) ? Estado(estado) : valor;

    /// <summary>Fija el idioma activo. Un valor desconocido o nulo deja español.</summary>
    public static void Fijar(string? idioma)
        => Idioma = string.Equals(idioma, En, StringComparison.OrdinalIgnoreCase) ? En : Es;

    /// <summary>
    /// Resuelve el idioma inicial: NETPROTOCOL_IDIOMA manda; si no está, LANG/LC_ALL
    /// con prefijo "en" eligen inglés. Cualquier otra cosa deja español.
    /// </summary>
    public static string Detectar()
    {
        var explicito = Environment.GetEnvironmentVariable("NETPROTOCOL_IDIOMA");
        if (!string.IsNullOrWhiteSpace(explicito))
        {
            Fijar(explicito.Trim());
            return Idioma;
        }

        foreach (var variable in new[] { "LANG", "LC_ALL", "LC_MESSAGES" })
        {
            var valor = Environment.GetEnvironmentVariable(variable);
            if (!string.IsNullOrWhiteSpace(valor) &&
                valor.TrimStart().StartsWith("en", StringComparison.OrdinalIgnoreCase))
            {
                Fijar(En);
                return Idioma;
            }
        }

        Fijar(Es);
        return Idioma;
    }

    /// <summary>Texto de la clave en el idioma activo; si falta, español; si tampoco, la clave.</summary>
    public static string T(string clave)
    {
        if (!Catalogo.TryGetValue(clave, out var par)) return clave;
        return Idioma == En ? par.En : par.Es;
    }

    /// <summary>Texto con formato: los argumentos se insertan con cultura invariante.</summary>
    public static string F(string clave, params object?[] args)
        => string.Format(CultureInfo.InvariantCulture, T(clave), args);

    /// <summary>Alterna es/en y devuelve el idioma resultante.</summary>
    public static string Alternar()
    {
        Fijar(Idioma == Es ? En : Es);
        return Idioma;
    }

    /// <summary>Claves publicadas, para pruebas de cobertura del catálogo.</summary>
    public static IReadOnlyCollection<string> Claves => Catalogo.Keys;
}
