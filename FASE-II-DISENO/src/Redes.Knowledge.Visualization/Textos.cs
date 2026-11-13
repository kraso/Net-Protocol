using System.Globalization;

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
