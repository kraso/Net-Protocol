namespace Redes.Knowledge.Visualization;

public enum PrimitiveKind { Rect, Line, Text }

/// <summary>
/// Primitiva de dibujo independiente del renderer (ADR-003): el modelo de diagrama
/// NO conoce SVG ni PDF; los renderers lo convierten.
/// </summary>
public sealed record Primitive(
    PrimitiveKind Kind,
    double X,
    double Y,
    double W,
    double H,
    string Label = "",
    string? Fill = null,
    string? Stroke = null);

/// <summary>Documento de diagrama: primitivas + tamaño + metadatos.</summary>
public sealed record DiagramDocument(
    string Titulo,
    string Tipo,
    int Width,
    int Height,
    IReadOnlyList<Primitive> Items);

/// <summary>Input del layout de wire format (bit/byte).</summary>
public sealed record WireField(string Nombre, int OffsetBits, int? LongitudBits);

/// <summary>Input del layout de secuencia temporal.</summary>
public sealed record MensajeSecuencia(string De, string Para, string Etiqueta);

/// <summary>Input del layout de máquina de estados.</summary>
public sealed record Transicion(string De, string Evento, string A);