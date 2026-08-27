using System.Globalization;
using System.Text;
using Redes.Knowledge.Infrastructure;
using Redes.Knowledge.Visualization;

namespace Redes.Knowledge.Tests;

/// <summary>D4: plantillas deterministas (5), render SVG y exportación PDF (mínimo, sin dependencias).</summary>
public class VisualizationTests
{
    private static string RaizDelRepositorio()
    {
        var d = new DirectoryInfo(AppContext.BaseDirectory);
        while (d is not null && !File.Exists(Path.Combine(d.FullName, "PLANREDES.md")))
            d = d.Parent;
        return d?.FullName ?? throw new InvalidOperationException("No se encontró la raíz del repositorio.");
    }

    private static IReadOnlyList<WireField> CamposTcpReales()
    {
        var ruta = Path.Combine(RaizDelRepositorio(), "FASE-05-MENSAJERIA", "F5-Campos-PDU.json");
        return CatalogJson.CargarCamposF5(ruta, "TCP")
            .Select(c => new WireField(c.Nombre, c.OffsetBits ?? 0, c.LongitudBits))
            .ToList();
    }

    // 1) Wire format bit/byte (sobre datos canónicos reales de F5)
    [Fact]
    public void WireFormat_DesdeF5_Es_Determinista()
    {
        var doc = Layouts.WireFormat("TCP Header (RFC 9293)", CamposTcpReales());
        var svg1 = SvgRenderer.Render(doc);
        var svg2 = SvgRenderer.Render(doc);
        Assert.Equal(svg1, svg2);
        Assert.Contains("<svg", svg1);
        Assert.Contains("Source Port", svg1);
        Assert.Contains("Destination Port", svg1);
    }

    // 2) Pila y encapsulación
    [Fact]
    public void Pila_Contiene_Capas_Determinista()
    {
        var capas = new[] { "HTTP/3", "QUIC", "UDP", "IPv4", "Ethernet", "Fibra óptica" };
        var doc = Layouts.Pila("Pila de encapsulación (HTTP/3)", capas);
        var svg1 = SvgRenderer.Render(doc);
        Assert.Equal(svg1, SvgRenderer.Render(doc));
        Assert.Contains("HTTP/3", svg1);
        Assert.Contains("Ethernet", svg1);
        Assert.Contains("encapsulación", svg1);
    }

    // 3) Secuencia temporal (DHCP DORA)
    [Fact]
    public void Secuencia_DHCPDORA_Determinista()
    {
        var mensajes = new[]
        {
            new MensajeSecuencia("Cliente", "Servidor", "DHCP Discover"),
            new MensajeSecuencia("Servidor", "Cliente", "DHCP Offer"),
            new MensajeSecuencia("Cliente", "Servidor", "DHCP Request"),
            new MensajeSecuencia("Servidor", "Cliente", "DHCP Ack")
        };
        var doc = Layouts.Secuencia("DHCP — DORA", mensajes);
        var svg = SvgRenderer.Render(doc);
        Assert.Equal(svg, SvgRenderer.Render(doc));
        Assert.Contains("DHCP Discover", svg);
        Assert.Contains("DHCP Ack", svg);
    }

    // 4) Máquina de estados (TCP, subconjunto)
    [Fact]
    public void MaquinaEstados_TCP_Determinista()
    {
        var estados = new[] { "LISTEN", "SYN-SENT", "SYN-RECEIVED", "ESTABLISHED" };
        var trans = new[]
        {
            new Transicion("LISTEN", "SYN recibido", "SYN-RECEIVED"),
            new Transicion("SYN-SENT", "SYN+ACK recibido", "ESTABLISHED"),
            new Transicion("SYN-RECEIVED", "ACK recibido", "ESTABLISHED")
        };
        var doc = Layouts.MaquinaEstados("TCP — establecimiento", estados, trans);
        var svg = SvgRenderer.Render(doc);
        Assert.Equal(svg, SvgRenderer.Render(doc));
        Assert.Contains("ESTABLISHED", svg);
        Assert.Contains("SYN+ACK recibido", svg);
    }

    // 5) Ruta extremo a extremo con PDU por enlace
    [Fact]
    public void RutaE2E_Determinista()
    {
        var nodos = new[] { "Host", "Switch L2", "Router", "Switch L2", "Servidor" };
        var pdu = new[] { "trama", "paquete", "paquete", "trama" };
        var doc = Layouts.RutaE2E("Ruta extremo a extremo", nodos, pdu);
        var svg = SvgRenderer.Render(doc);
        Assert.Equal(svg, SvgRenderer.Render(doc));
        Assert.Contains("Host", svg);
        Assert.Contains("Servidor", svg);
        Assert.Contains("trama", svg);
    }

    // Exportación PDF mínimo válido y determinista
    [Fact]
    public void Pdf_Minimo_Valido_Determinista()
    {
        var doc = Layouts.Pila("PDF test", new[] { "A", "B" });
        var pdf1 = PdfExporter.Export(doc);
        var pdf2 = PdfExporter.Export(doc);
        Assert.Equal(pdf1, pdf2);
        Assert.StartsWith("%PDF-1.4", Encoding.ASCII.GetString(pdf1, 0, 8));
        var texto = Encoding.ASCII.GetString(pdf1);
        Assert.Contains("startxref", texto);
        Assert.Contains("%%EOF", texto);
        Assert.True(pdf1.Length > 500, $"PDF demasiado corto: {pdf1.Length}");
    }

    // Exportación SVG a archivo
    [Fact]
    public void Svg_SePuede_Exportar_A_Archivo()
    {
        var doc = Layouts.WireFormat("TCP", CamposTcpReales());
        var svg = SvgRenderer.Render(doc);
        var tmp = Path.Combine(Path.GetTempPath(), $"rk_diag_{Guid.NewGuid():N}.svg");
        try
        {
            File.WriteAllText(tmp, svg);
            Assert.True(File.Exists(tmp));
            Assert.Equal(svg, File.ReadAllText(tmp));
            Assert.True(new FileInfo(tmp).Length > 800);
        }
        finally
        {
            if (File.Exists(tmp)) File.Delete(tmp);
        }
    }

    // Regresión (D4-3): los colores del PDF usan '.' aunque la cultura use coma decimal.
    [Fact]
    public void Pdf_Colores_Independientes_De_Cultura()
    {
        var doc = Layouts.Pila("Cultura", new[] { "CapaA", "CapaB" });
        var culturaOriginal = CultureInfo.CurrentCulture;
        try
        {
            CultureInfo.CurrentCulture = new CultureInfo("es-ES");
            var texto = Encoding.ASCII.GetString(PdfExporter.Export(doc));
            // Sin comas: '0,933 0,949 1 rg' invalidaría el operador de color en PDF.
            Assert.DoesNotContain("0,9", texto);
            Assert.Contains("0.933 0.949 1 rg", texto);
        }
        finally
        {
            CultureInfo.CurrentCulture = culturaOriginal;
        }
    }

    // Regresión (D4-3): el texto SVG queda en la misma posición que el renderer de
    // pantalla (13 px; SVG deposita en la línea base -> y + 13).
    [Fact]
    public void Svg_Texto_En_La_Misma_Posicion_Que_La_Pantalla()
    {
        var doc = Layouts.Pila("Título", new[] { "CapaA" });
        var svg = SvgRenderer.Render(doc);
        Assert.Contains("font-size=\"13\"", svg);
        var titulo = doc.Items.First(p => p.Kind == PrimitiveKind.Text && p.X == 10 && p.Y == 14);
        Assert.Contains($"y=\"{titulo.Y + 13}\"", svg);
    }
}