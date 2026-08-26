using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace SpikeDiagramas;

internal static class Program
{
    // Ruta por defecto relativa al directorio de trabajo del proyecto (raíz del repositorio).
    private const string F5Path = @"FASE-05-MENSAJERIA\F5-Campos-PDU.json";

    private static async Task<int> Main(string[] args)
    {
        var path = args.Length > 0 ? args[0] : Path.GetFullPath(F5Path);
        Console.WriteLine($"Leyendo catálogo de campos: {path}");
        var json = await File.ReadAllTextAsync(path);
        using var doc = JsonDocument.Parse(json);

        var campos = new List<FieldDef>();
        foreach (var p in doc.RootElement.GetProperty("protocolos").EnumerateArray())
        {
            if (p.GetProperty("acronimo").GetString() != "TCP") continue;
            foreach (var c in p.GetProperty("campos").EnumerateArray())
            {
                if (!c.TryGetProperty("offset_bits", out var off) || off.ValueKind == JsonValueKind.Null) continue;
                var lenEl = c.GetProperty("longitud_bits");
                int? len = lenEl.ValueKind == JsonValueKind.Null ? null : lenEl.GetInt32();
                campos.Add(new FieldDef(c.GetProperty("nombre").GetString()!, off.GetInt32(), len, c.GetProperty("tipo").GetString()!));
            }
        }
        var fijos = campos.Where(c => c.LongitudBits.HasValue).ToList();
        Console.WriteLine($"Campos TCP catalogados en F5: {campos.Count} (con longitud: {fijos.Count})");

        // Determinismo: dos generaciones del mismo input deben producir contenido idéntico.
        var a = DeterministicSvg.Render("TCP Header (RFC 9293) - layout determinista", fijos);
        var b = DeterministicSvg.Render("TCP Header (RFC 9293) - layout determinista", fijos);
        var ha = Sha256(a);
        var hb = Sha256(b);

        Console.WriteLine($"Run1 SHA256: {ha}");
        Console.WriteLine($"Run2 SHA256: {hb}");
        Console.WriteLine($"DETERMINISMO: {(ha == hb ? "OK (contenidos idénticos)" : "FALLO (difiere entre ejecuciones)")}");

        var outDir = Path.Combine(Directory.GetCurrentDirectory(), "FASE-II-DISENO", "spikes", "out");
        Directory.CreateDirectory(outDir);
        var outPath = Path.Combine(outDir, "tcp-header.svg");
        await File.WriteAllTextAsync(outPath, a);
        Console.WriteLine($"SVG exportado: {Path.GetFullPath(outPath)} ({a.Length} bytes)");
        Console.WriteLine($"Longitud del SVG: {a.Length} caracteres");

        return ha == hb ? 0 : 1;
    }

    private static string Sha256(string s)
    {
        var bytes = System.Security.Cryptography.SHA256.HashData(Encoding.UTF8.GetBytes(s));
        return Convert.ToHexString(bytes);
    }
}