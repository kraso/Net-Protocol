namespace Redes.Knowledge.Visualization;

/// <summary>
/// Productores de layouts (plantillas del plan §11). Todos usan aritmética determinista:
/// mismo input → mismas coordenadas → mismo diagrama (regla del proyecto).
/// </summary>
public static class Layouts
{
    private const int RowBits = 32, CellPx = 20, RowH = 34, LabelH = 18;
    private const string Fill = "#eef2ff", Stroke = "#334155", TextColor = "#0f172a";

    // 1) Mensaje / wire format (bit/byte), estilo RFC
    public static DiagramDocument WireFormat(string titulo, IReadOnlyList<WireField> campos)
    {
        var fijos = campos.Where(c => c.LongitudBits.HasValue).OrderBy(c => c.OffsetBits).ToList();
        var maxEnd = fijos.Count == 0 ? 0 : fijos.Max(c => c.OffsetBits + c.LongitudBits!.Value);
        var filas = (maxEnd + RowBits - 1) / RowBits;
        var w = RowBits * CellPx + 48;
        var h = 26 + LabelH + filas * RowH + 16;

        var items = new List<Primitive> { new(PrimitiveKind.Text, 8, 10, 0, 0, titulo, TextColor) };
        for (var r = 0; r < filas; r++)
        {
            var y = 26 + LabelH + r * RowH;
            // Número de fila centrado respecto a la fila completa (top-left del texto).
            items.Add(new Primitive(PrimitiveKind.Text, 6, y + (RowH - 13) / 2, 0, 0, $"{r * RowBits:D2}", TextColor));
            foreach (var c in fijos)
            {
                var s = c.OffsetBits - r * RowBits;
                if (s >= RowBits || s + c.LongitudBits!.Value <= 0) continue;
                var start = Math.Max(s, 0);
                var len = Math.Min(c.LongitudBits.Value, RowBits - start);
                var x = 44 + start * CellPx;
                var ancho = len * CellPx;
                items.Add(new Primitive(PrimitiveKind.Rect, x, y, ancho, RowH - 4, "", Fill, Stroke));
                // Etiqueta centrada verticalmente dentro de la casilla (Y = top-left del texto).
                var etiqueta = EtiquetaCampo(c.Nombre, c.OffsetBits, c.LongitudBits.Value, ancho - 6);
                items.Add(new Primitive(PrimitiveKind.Text, x + 3, y + (RowH - 4 - 13) / 2,
                    Math.Max(0, ancho - 6), 0, etiqueta, TextColor));
            }
        }
        return new DiagramDocument(titulo, "wire-format", w, h, items);
    }

    /// <summary>Etiqueta que cabe dentro de la casilla del wire format (estilo RFC).
    /// Estima el ancho del texto a tamaño 13px (~6,5 px/carácter) para decidir.</summary>
    private static string EtiquetaCampo(string nombre, int offset, int longitud, double anchoDisponible)
    {
        const double PxPorCaracter = 6.5;
        var completo = $"{nombre} ({offset}-{offset + longitud})";
        if (completo.Length * PxPorCaracter <= anchoDisponible) return completo;
        if (nombre.Length * PxPorCaracter <= anchoDisponible) return nombre;
        var max = Math.Max(1, (int)(anchoDisponible / PxPorCaracter) - 1);
        return nombre[..Math.Min(max, nombre.Length)] + "…";
    }

    // 2) Pila y encapsulación (top → bottom)
    public static DiagramDocument Pila(string titulo, IReadOnlyList<string> capas,
        bool mostrarTitulo = true, bool mostrarEtiquetasEnlace = true)
    {
        const double bw = 380, bh = 44, gap = 30;
        var w = 480;
        var h = (int)(40 + capas.Count * (bh + gap) + 30);
        var items = new List<Primitive>();
        if (mostrarTitulo) items.Add(new Primitive(PrimitiveKind.Text, 10, 14, 0, 0, titulo, TextColor));
        double prevBottom = 0;
        for (var i = 0; i < capas.Count; i++)
        {
            var y = 40 + i * (bh + gap);
            var x = (w - bw) / 2;
            items.Add(new Primitive(PrimitiveKind.Rect, x, y, bw, bh, "", Fill, Stroke));
            // Texto centrado verticalmente en la caja (Y = borde superior de la caja de texto)
            items.Add(new Primitive(PrimitiveKind.Text, x + 14, y + (bh - 13) / 2, 0, 0, capas[i], TextColor));
            if (i > 0)
            {
                items.Add(new Primitive(PrimitiveKind.Line, x + bw / 2, prevBottom, 0, gap, "", null, Stroke));
                if (mostrarEtiquetasEnlace)
                    items.Add(new Primitive(PrimitiveKind.Text,
                        x + bw / 2 - 40, prevBottom + gap / 2 - 8, 0, 0, "encapsulación", TextColor));
            }
            prevBottom = y + bh;
        }
        return new DiagramDocument(titulo, "pila", w, h, items);
    }

    // 3) Secuencia temporal entre participantes
    public static DiagramDocument Secuencia(string titulo, IReadOnlyList<MensajeSecuencia> mensajes)
    {
        var actores = mensajes.SelectMany(m => new[] { m.De, m.Para }).Distinct().ToList();
        const double bx = 130, by = 34;
        var w = 40 + actores.Count * 220 + 40;
        var h = 70 + mensajes.Count * 48 + 30;

        var x = actores.ToDictionary(a => a, a => 60 + actores.IndexOf(a) * 220.0);
        var items = new List<Primitive> { new(PrimitiveKind.Text, 10, 14, 0, 0, titulo, TextColor) };
        foreach (var a in actores)
        {
            items.Add(new Primitive(PrimitiveKind.Rect, x[a] - bx / 2, by, bx, 26, "", Fill, Stroke));
            items.Add(new Primitive(PrimitiveKind.Text, x[a] - bx / 2 + 8, by + 8, 0, 0, a, TextColor));
        }
        for (var i = 0; i < mensajes.Count; i++)
        {
            var m = mensajes[i];
            var y = 70 + i * 48;
            items.Add(new Primitive(PrimitiveKind.Line, x[m.De], y, x[m.Para] - x[m.De], 0, "", null, Stroke));
            var mid = (x[m.De] + x[m.Para]) / 2;
            items.Add(new Primitive(PrimitiveKind.Text, mid - 4, y - 16, 0, 0, m.Etiqueta, TextColor));
        }
        return new DiagramDocument(titulo, "secuencia", w, h, items);
    }

    // 4) Máquina de estados (cuadrícula determinista)
    public static DiagramDocument MaquinaEstados(string titulo, IReadOnlyList<string> estados, IReadOnlyList<Transicion> transiciones)
    {
        const double bw = 150, bh = 40, colW = 170;
        var n = estados.Count;
        var fila0 = (n + 1) / 2;
        var w = (int)(40 + Math.Max(fila0, n - fila0) * colW + 40);
        var h = 210;

        var pos = new Dictionary<string, (double X, double Y)>();
        var items = new List<Primitive> { new(PrimitiveKind.Text, 10, 14, 0, 0, titulo, TextColor) };
        for (var i = 0; i < n; i++)
        {
            var c = i < fila0 ? i : i - fila0;
            var y = i < fila0 ? 60 : 140;
            var px = 40 + c * colW;
            pos[estados[i]] = (px, y);
            items.Add(new Primitive(PrimitiveKind.Rect, px, y, bw, bh, "", Fill, Stroke));
            items.Add(new Primitive(PrimitiveKind.Text, px + 10, y + bh / 2 + 2, 0, 0, estados[i], TextColor));
        }
        foreach (var t in transiciones)
        {
            if (!pos.TryGetValue(t.De, out var a) || !pos.TryGetValue(t.A, out var b)) continue;
            items.Add(new Primitive(PrimitiveKind.Line, a.X + bw, a.Y + bh / 2, b.X - (a.X + bw), b.Y - a.Y, "", null, Stroke));
            items.Add(new Primitive(PrimitiveKind.Text, (a.X + b.X) / 2 + 4, Math.Min(a.Y, b.Y) + bh / 2 + 6, 0, 0, t.Evento, TextColor));
        }
        return new DiagramDocument(titulo, "estados", w, h, items);
    }

    // 5) Ruta extremo a extremo con PDU por enlace
    public static DiagramDocument RutaE2E(string titulo, IReadOnlyList<string> nodos, IReadOnlyList<string> pduPorEnlace)
    {
        const double bw = 140, bh = 44;
        var w = 40 + nodos.Count * 180 + 40;
        var h = 210;
        var items = new List<Primitive> { new(PrimitiveKind.Text, 10, 14, 0, 0, titulo, TextColor) };
        for (var i = 0; i < nodos.Count; i++)
        {
            var x = 40 + i * 180;
            var y = 100;
            items.Add(new Primitive(PrimitiveKind.Rect, x, y, bw, bh, "", Fill, Stroke));
            items.Add(new Primitive(PrimitiveKind.Text, x + 10, y + bh / 2 + 2, 0, 0, nodos[i], TextColor));
            if (i < pduPorEnlace.Count)
            {
                items.Add(new Primitive(PrimitiveKind.Line, x + bw, y + bh / 2, 40, 0, "", null, Stroke));
                var label = string.IsNullOrWhiteSpace(pduPorEnlace[i]) ? "PDU" : pduPorEnlace[i];
                items.Add(new Primitive(PrimitiveKind.Text, x + bw + 8, y + bh / 2 - 10, 0, 0, label, TextColor));
            }
        }
        return new DiagramDocument(titulo, "ruta-e2e", w, h, items);
    }

    // 6) Grafo simple (estrella determinista desde una semilla)
    public static DiagramDocument Grafo(string titulo, string semilla,
        IReadOnlyList<(string Nodo, string Etiqueta)> nodos,
        IReadOnlyList<(string A, string B, string Etiqueta)> aristas,
        bool mostrarEtiquetasAristas = true)
    {
        const double cx = 300, cy = 185, radio = 155;
        const double bw = 120, bh = 30;
        var pos = new Dictionary<string, (double X, double Y)>();
        var items = new List<Primitive> { new(PrimitiveKind.Text, 10, 14, 0, 0, titulo, TextColor) };

        var vecinos = nodos.Where(n => !string.Equals(n.Nodo, semilla, StringComparison.Ordinal)).ToList();
        for (var i = 0; i < vecinos.Count; i++)
        {
            var angulo = 2 * Math.PI * i / Math.Max(1, vecinos.Count);
            var x = cx + radio * Math.Cos(angulo);
            var y = cy + radio * Math.Sin(angulo);
            pos[vecinos[i].Nodo] = (x - bw / 2, y - bh / 2);
            items.Add(new Primitive(PrimitiveKind.Rect, x - bw / 2, y - bh / 2, bw, bh, "", Fill, Stroke));
            items.Add(new Primitive(PrimitiveKind.Text, x - bw / 2 + 8, y - bh / 2 + 12, 0, 0, vecinos[i].Etiqueta, TextColor));
        }

        pos[semilla] = (cx - 75, cy - bh / 2);
        items.Add(new Primitive(PrimitiveKind.Rect, cx - 75, cy - bh / 2, 150, bh, "", "#fde68a", Stroke));
        items.Add(new Primitive(PrimitiveKind.Text, cx - 65, cy - bh / 2 + 12, 0, 0, semilla, TextColor));

        foreach (var ar in aristas)
        {
            if (!pos.TryGetValue(ar.A, out var pa) || !pos.TryGetValue(ar.B, out var pb)) continue;
            var x1 = pa.X + bw / 2;
            var y1 = pa.Y + bh / 2;
            var x2 = pb.X + bw / 2;
            var y2 = pb.Y + bh / 2;
            // La línea conserva la etiqueta (el renderer la usa para colorear por tipo);
            // el texto visible solo se dibuja cuando se solicita.
            items.Add(new Primitive(PrimitiveKind.Line, x1, y1, x2 - x1, y2 - y1, ar.Etiqueta, null, Stroke));
            if (mostrarEtiquetasAristas && !string.IsNullOrWhiteSpace(ar.Etiqueta))
                items.Add(new Primitive(PrimitiveKind.Text, (x1 + x2) / 2 + 6, (y1 + y2) / 2, 0, 0, ar.Etiqueta, TextColor));
        }

        return new DiagramDocument(titulo, "grafo", 600, 400, items);
    }
}