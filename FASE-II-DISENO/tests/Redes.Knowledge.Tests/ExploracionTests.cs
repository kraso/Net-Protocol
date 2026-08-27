using Redes.Knowledge.Domain;
using Redes.Knowledge.Infrastructure;
using Redes.Knowledge.Infrastructure.Iana;
using Redes.Knowledge.Visualization;

namespace Redes.Knowledge.Tests;

/// <summary>D5: grafo de relaciones (F4), comparador (F5/F6/IANA) y fichas detalladas (F2/F5).</summary>
public class ExploracionTests
{
    private static string Raiz()
    {
        var d = new DirectoryInfo(AppContext.BaseDirectory);
        while (d is not null && !File.Exists(Path.Combine(d.FullName, "PLANREDES.md")))
            d = d.Parent;
        return d?.FullName ?? throw new InvalidOperationException("No se encontró la raíz del repositorio.");
    }

    private static string R(string carpeta, string archivo) => Path.Combine(Raiz(), carpeta, archivo);
    private const string F4 = @"FASE-04-PROFUNDIZACION\F4-Matriz-Encapsulacion.json";
    private const string F5 = @"FASE-05-MENSAJERIA\F5-Campos-PDU.json";
    private const string F6 = @"FASE-06-SEGURIDAD\F6-Seguridad-Protocolos.json";
    private const string F2D = @"FASE-02-DISPOSITIVOS\F2-Catalogo-Dispositivos.json";
    private const string F2R = @"FASE-02-DISPOSITIVOS\F2-Catalogo-Redes.json";
    private const string F3 = @"FASE-03-INVENTARIO\F3-Protocolos.json";

    [Fact]
    public void Grafo_Carga_Relaciones_Reales()
    {
        var rels = CatalogoExploracion.CargarRelacionesF4(R(F4.Split('\\')[0], string.Join('\\', F4.Split('\\').Skip(1))));
        Assert.True(rels.Count >= 20);
        // Las URN del grafo usan nombres normalizados (urn:entidad:http3)
        Assert.Contains(rels, r =>
            GrafoRelaciones.EntidadDe(r.Origen.Value) == "http3" &&
            GrafoRelaciones.EntidadDe(r.Destino.Value) == "quic" &&
            r.Tipo == RelacionTipo.CorreSobre);
    }

    [Fact]
    public void Grafo_Vecinos_1Salto_HTTP3()
    {
        var rels = CatalogoExploracion.CargarRelacionesF4(R(F4.Split('\\')[0], string.Join('\\', F4.Split('\\').Skip(1))));
        var vecinos = GrafoRelaciones.Vecinos1Salto("HTTP/3", rels);
        Assert.Contains(vecinos, v => v.Nombre == "quic" && v.Tipo == RelacionTipo.CorreSobre);
    }

    [Fact]
    public void Grafo_Alias_F4_Resuelve_ETH()
    {
        // La matriz F4 nombra la entidad "Ethernet (802.3)"; el catálogo F3 la cataloga como ETH.
        var rels = CatalogoExploracion.CargarRelacionesF4(R(F4.Split('\\')[0], string.Join('\\', F4.Split('\\').Skip(1))));
        Assert.Contains(rels, r => GrafoRelaciones.EntidadDe(r.Origen.Value) == "eth"
                                   || GrafoRelaciones.EntidadDe(r.Destino.Value) == "eth");
        var vecinos = GrafoRelaciones.Vecinos1Salto("ETH", rels);
        Assert.Contains(vecinos, v => v.Nombre == "ipv4" || v.Nombre == "ipv6" || v.Nombre == "arp");
    }

    [Fact]
    public void Notas_Y_Fuentes_F3_Disponibles()
    {
        var notas = CatalogJson.CargarNotasFuenteF3(R(F3.Split('\\')[0], string.Join('\\', F3.Split('\\').Skip(1))));
        Assert.True(notas.Count >= 100);
        Assert.True(notas.TryGetValue("ETH", out var eth) && eth.Nota.Length > 0); // "Familia de tramas con EtherTypes"
        Assert.Equal("pendiente", notas["ETH"].Fuente); // el pipeline R1-R11 aún no asigna fuentes
    }

    [Fact]
    public void Fichas_Prioritarias_F4_Cargan_18_Campos()
    {
        var fichas = CatalogoExploracion.CargarFichasF4(
            R(F4.Split('\\')[0], "F4-Fichas-Prioritarias.json"));
        Assert.True(fichas.Count == 113, $"Fichas: {fichas.Count}");
        Assert.True(fichas.TryGetValue("TCP", out var tcp));
        Assert.Equal("F-01", tcp.Id);
        Assert.NotNull(tcp.Campo(3)); // finalidad
        Assert.NotNull(tcp.Campo(18)); // fuentes
        Assert.Contains("RFC 9293", tcp.Campo(18)!);
        Assert.NotNull(fichas["ETH"].Campo(3)); // Ethernet (IEEE 802.3)
        Assert.NotNull(fichas["WIFI"].Campo(3)); // ampliación lote 4
        Assert.NotNull(fichas["LTE"].Campo(3)); // ampliación lote 5
        Assert.NotNull(fichas["EIGRP"].Campo(3)); // ampliación lote 5 (última)
        Assert.NotNull(fichas["X.25"].Campo(3)); // históricos
        // Todas las fichas tienen los 18 campos.
        foreach (var f in fichas.Values)
            Assert.True(f.Campo(18) is not null, $"Ficha {f.Acronimo} sin campo 18");
    }

    [Fact]
    public void Grafo_F4_Permite_Pila_De_Encapsulacion()
    {
        // La cadena de F4 (corre_sobre) resuelve desde el protocolo hacia el medio:
        // TCP -> IPv4 -> Ethernet(ETH) -> Cobre/Fibra.
        var rels = CatalogoExploracion.CargarRelacionesF4(R(F4.Split('\\')[0], string.Join('\\', F4.Split('\\').Skip(1))));

        var cadena = new List<string> { "TCP" };
        var actual = "TCP";
        for (var i = 0; i < 8; i++)
        {
            var rel = rels.FirstOrDefault(r =>
                GrafoRelaciones.EntidadDe(r.Origen.Value) == actual.ToLowerInvariant() &&
                r.Tipo == RelacionTipo.CorreSobre);
            if (rel is null) break;
            var sig = GrafoRelaciones.EntidadDe(rel.Destino.Value);
            cadena.Add(sig);
            actual = sig;
        }
        Assert.Contains("ipv4", cadena);
        Assert.Contains("eth", cadena); // alias Ethernet (802.3) -> ETH
        Assert.Contains("cobrefibra", cadena);

        var pila = Layouts.Pila("Pila", cadena);
        var svg = SvgRenderer.Render(pila);
        Assert.Contains("TCP", svg);
        Assert.Contains("cobrefibra", svg);
    }

    [Fact]
    public void Fichas_Dispositivos_Y_Redes_Reales()
    {
        var dispositivos = CatalogoExploracion.CargarDispositivosF2(R(F2D.Split('\\')[0], string.Join('\\', F2D.Split('\\').Skip(1))));
        var redes = CatalogoExploracion.CargarRedesF2(R(F2R.Split('\\')[0], string.Join('\\', F2R.Split('\\').Skip(1))));
        Assert.Equal(22, dispositivos.Count);
        Assert.Equal(16, redes.Count);
        Assert.Contains(dispositivos, d => d.Clase == "Router");
        Assert.Contains(redes, n => n.Nombre == "WAN");
    }

    [Fact]
    public void Ficha_PDU_Y_Seguridad_Reales()
    {
        var r5 = R(F5.Split('\\')[0], string.Join('\\', F5.Split('\\').Skip(1)));
        var r6 = R(F6.Split('\\')[0], string.Join('\\', F6.Split('\\').Skip(1)));

        Assert.Equal("segmento", CatalogoExploracion.ObtenerPduF5(r5, "TCP"));
        Assert.Equal("datagrama", CatalogoExploracion.ObtenerPduF5(r5, "UDP"));

        var cifrado = CatalogoExploracion.CargarSeguridadF6(r6, "cifrado");
        Assert.Contains("TLS", cifrado.Keys);
        Assert.Contains("AEAD", cifrado["TLS"]);
    }

    [Fact]
    public void Comparador_Con_Datos_Reales()
    {
        var catalogos = CatalogJson.CargarProtocolosF3(R(F3.Split('\\')[0], string.Join('\\', F3.Split('\\').Skip(1))));
        var tcp = catalogos.First(p => p.Acronimo == "TCP");
        var udp = catalogos.First(p => p.Acronimo == "UDP");
        var bgp = catalogos.First(p => p.Acronimo == "BGP");

        var r5 = R(F5.Split('\\')[0], string.Join('\\', F5.Split('\\').Skip(1)));
        var pdu = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["TCP"] = CatalogoExploracion.ObtenerPduF5(r5, "TCP") ?? "—",
            ["UDP"] = CatalogoExploracion.ObtenerPduF5(r5, "UDP") ?? "—"
        };

        IReadOnlyList<IanaServiceEntry> puertos(string nombre, int lim)
            => nombre == "bgp" ? new[] { new IanaServiceEntry("bgp", 179, "TCP", "", "", "") } : Array.Empty<IanaServiceEntry>();

        var filas = ProtocoloComparador.Comparar(new[] { tcp, udp, bgp }, puertos, pdu);
        var filaBg = filas.First(f => f.Protocolo == "BGP");
        Assert.Equal("179/TCP", filaBg.Puertos);
        Assert.Equal("—", filaBg.Pdu);
        Assert.Equal("TRAN", filas.First(f => f.Protocolo == "TCP").Familia);
        Assert.Equal("segmento", filas.First(f => f.Protocolo == "TCP").Pdu);
    }

    [Fact]
    public void Comparador_Enriquecido_Con_Fichas_Y_Grafo_F4()
    {
        // El comparador debe enriquecerse con capas/finalidad/encapsulación reales
        // (fichas F4 + grafo de relaciones), no solo con "—".
        var r4 = R(F4.Split('\\')[0], string.Join('\\', F4.Split('\\').Skip(1)));
        var fichas = CatalogoExploracion.CargarFichasF4(
            R(F4.Split('\\')[0], "F4-Fichas-Prioritarias.json"));
        var rels = CatalogoExploracion.CargarRelacionesF4(r4);
        var catalogos = CatalogJson.CargarProtocolosF3(R(F3.Split('\\')[0], string.Join('\\', F3.Split('\\').Skip(1))));
        var tcp = catalogos.First(p => p.Acronimo == "TCP");
        var bgp = catalogos.First(p => p.Acronimo == "BGP");
        var smtp = catalogos.First(p => p.Acronimo == "SMTP");

        IReadOnlyList<IanaServiceEntry> puertos(string nombre, int lim)
            => Array.Empty<IanaServiceEntry>();

        var filas = ProtocoloComparador.Comparar(new[] { tcp, bgp, smtp }, puertos,
            null, null, fichas, rels);
        var fTcp = filas.First(f => f.Protocolo == "TCP");
        var fBgp = filas.First(f => f.Protocolo == "BGP");
        var fSmtp = filas.First(f => f.Protocolo == "SMTP");

        // TCP: ficha F4 con finalidad y capa OSI (campo 5).
        Assert.Contains("Transporte fiable", fTcp.Finalidad);
        Assert.Contains("Transporte", fTcp.Capas);
        // BGP: con ficha F4 (F-10), finalidad real.
        Assert.Contains("Intercambio de rutas", fBgp.Finalidad);
        // SMTP: nueva ficha F4 (ampliación) con finalidad real y encapsulación sobre TCP.
        Assert.Contains("correo", fSmtp.Finalidad);
        Assert.Contains("TCP", fSmtp.Encapsulacion);
        Assert.Contains("Aplicación", fSmtp.Capas);
    }

    [Fact]
    public void Grafo_Layout_Determinista()
    {
        var semilla = "HTTP/3";
        var nodos = new[] { ("HTTP/3", "HTTP/3"), ("QUIC", "QUIC"), ("UDP", "UDP"), ("TCP", "TCP") };
        var aristas = new[] { ("HTTP/3", "QUIC", "corre_sobre"), ("QUIC", "UDP", "corre_sobre") };
        var doc = Layouts.Grafo("Vecinos de HTTP/3", semilla, nodos, aristas);
        var svg = SvgRenderer.Render(doc);
        Assert.Equal(svg, SvgRenderer.Render(doc));
        Assert.Contains("HTTP/3", svg);
        Assert.Contains("QUIC", svg);
    }

    [Fact]
    public void GrafoConNodos_Determinista_Y_Semilla_Centrada()
    {
        var semilla = "HTTP/3";
        var nodos = new[] { ("HTTP/3", "HTTP/3"), ("QUIC", "QUIC"), ("UDP", "UDP"), ("TCP", "TCP") };
        var aristas = new[] { ("HTTP/3", "QUIC", "corre_sobre"), ("QUIC", "UDP", "corre_sobre") };

        var (doc, nodosA) = Layouts.GrafoConNodos("Vecinos de HTTP/3", semilla, nodos, aristas);
        var (_, nodosB) = Layouts.GrafoConNodos("Vecinos de HTTP/3", semilla, nodos, aristas);

        // D5-1: la geometría de navegación es determinista (mismo input -> mismos rectángulos)
        Assert.Equal(nodosA, nodosB);
        Assert.Equal(nodos.Length, nodosA.Count);

        // La semilla es el único nodo centrado y marcado como tal (rect 150x30 en (225,170)).
        var semillaNodo = Assert.Single(nodosA, n => n.EsSemilla);
        Assert.Equal("HTTP/3", semillaNodo.Clave);
        Assert.Equal(150, semillaNodo.W);
        Assert.Equal(30, semillaNodo.H);
        Assert.Equal(225, semillaNodo.X);
        Assert.Equal(170, semillaNodo.Y);

        // Un vecino cae sobre el círculo de radio 155 alrededor de (300,185).
        var quic = Assert.Single(nodosA, n => n.Clave == "QUIC");
        var centroX = quic.X + quic.W / 2;
        var centroY = quic.Y + quic.H / 2;
        var dx = centroX - 300;
        var dy = centroY - 185;
        Assert.InRange(Math.Sqrt(dx * dx + dy * dy), 154, 156);

        // El documento sigue siendo idéntico al del grafo "compat" (misma salida visual).
        var docCompat = Layouts.Grafo("Vecinos de HTTP/3", semilla, nodos, aristas);
        Assert.Equal(docCompat.Items, doc.Items);
    }
}