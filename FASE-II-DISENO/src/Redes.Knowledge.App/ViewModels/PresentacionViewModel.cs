using Redes.Knowledge.Domain;
using Redes.Knowledge.Infrastructure.Capturas;

namespace Redes.Knowledge.App.ViewModels;

/// <summary>
/// Estado de presentación de la ventana (zoom por reflow, protocolo seleccionado y
/// captura abierta), sin dependencia de Avalonia.
/// <para>
/// MainWindow conserva solo el reflejo visual de este estado: asigna fuentes, alterna
/// paneles y re-renderiza diagramas. Todo lo que se puede ejercitar sin UI (límites del
/// zoom, ciclo de vida de la captura, índice de paquete válido) vive aquí y se prueba en
/// <c>Redes.Knowledge.App.Tests</c>.
/// </para>
/// </summary>
public sealed class PresentacionViewModel
{
    /// <summary>Zoom mínimo del contenido (70 %).</summary>
    public const double ZoomMin = 0.7;

    /// <summary>Zoom máximo del contenido (300 %).</summary>
    public const double ZoomMax = 3.0;

    /// <summary>Incremento por muesca de rueda con Ctrl pulsado.</summary>
    public const double ZoomPaso = 0.1;

    /// <summary>Fuente de reserva cuando el tema aún no ha aplicado tamaño al contenido.</summary>
    public const double FuenteReserva = 13;

    /// <summary>Factor de zoom activo (1.0 = 100 %).</summary>
    public double Zoom { get; private set; } = 1.0;

    /// <summary>Fuente base del contenido; valor provisional hasta que <see cref="FijarBases"/> lo sobrescriba.</summary>
    public double FuenteBase { get; private set; } = FuenteReserva;

    /// <summary>Fuente base del título de los diagramas; nunca menor que <see cref="FuenteBase"/>.</summary>
    public double TituloBase { get; private set; } = FuenteReserva + 1;

    /// <summary>Protocolo cuya ficha está en pantalla; null = ninguna selección.</summary>
    public Protocol? Seleccionado { get; private set; }

    /// <summary>Paquetes de la captura en pantalla; vacío si no hay captura.</summary>
    public IReadOnlyList<PcapPacket> Paquetes { get; private set; } = Array.Empty<PcapPacket>();

    /// <summary>Ruta del fichero de captura en pantalla; cadena vacía si no hay captura.</summary>
    public string RutaCaptura { get; private set; } = "";

    /// <summary>True mientras la vista de captura ocupa el panel central.</summary>
    public bool CapturaAbierta { get; private set; }

    /// <summary>Índice del paquete seleccionado en la lista; -1 si no hay ninguno válido.</summary>
    public int IndicePaquete { get; private set; } = -1;

    /// <summary>True cuando la ventana ya capturó la fuente base y el zoom puede aplicarse.</summary>
    public bool BasesFijadas => FuenteBase > 0;

    /// <summary>Número de paquetes de la captura en pantalla.</summary>
    public int TotalPaquetes => Paquetes.Count;

    /// <summary>Zoom formateado como porcentaje entero, tal y como aparece en la barra de estado.</summary>
    public string ZoomTexto => $"{Zoom * 100:0}%";

    /// <summary>Fuente efectiva del contenido a aplicar en la ficha y los paneles de captura.</summary>
    public double FuenteContenido => FuenteBase * Zoom;

    /// <summary>Fuente efectiva del título de los diagramas.</summary>
    public double FuenteTitulo => TituloBase * Zoom;

    /// <summary>Paquete seleccionado, o null si el índice no apunta a ninguno válido.</summary>
    public PcapPacket? PaqueteActual =>
        IndicePaquete >= 0 && IndicePaquete < Paquetes.Count ? Paquetes[IndicePaquete] : null;

    /// <summary>
    /// Aplica una muesca de zoom y devuelve el factor resultante. El valor queda acotado a
    /// [<see cref="ZoomMin"/>, <see cref="ZoomMax"/>], de modo que las muescas de más no
    /// acumulan desbordamiento.
    /// </summary>
    public double AplicarZoom(double delta)
    {
        Zoom = Math.Clamp(Zoom + delta, ZoomMin, ZoomMax);
        return Zoom;
    }

    /// <summary>
    /// Devuelve el zoom a 100 % (atajo Ctrl+0) ignorando el valor previo. Es independiente
    /// de las muescas: siempre deja el factor en 1.0, que está dentro de los límites.
    /// </summary>
    public double RestablecerZoom()
    {
        Zoom = 1.0;
        return Zoom;
    }

    /// <summary>
    /// Fija las fuentes base al abrir la ventana. Un valor no positivo o NaN se sustituye
    /// por <see cref="FuenteReserva"/>; el título nunca queda por debajo del contenido.
    /// </summary>
    public void FijarBases(double fuente, double titulo)
    {
        FuenteBase = double.IsNaN(fuente) || fuente <= 0 ? FuenteReserva : fuente;
        TituloBase = double.IsNaN(titulo) || titulo <= 0 ? FuenteBase : Math.Max(FuenteBase, titulo);
    }

    /// <summary>
    /// Selecciona el protocolo en pantalla. Toda navegación a una ficha cierra la captura,
    /// que es el comportamiento observado en la ventana.
    /// </summary>
    public void Seleccionar(Protocol? protocolo)
    {
        Seleccionado = protocolo;
        CerrarCaptura();
    }

    /// <summary>
    /// Abre la vista de captura con sus paquetes y deja seleccionado el primero. Una
    /// captura sin paquetes deja el índice en -1 en lugar de apuntar fuera de rango.
    /// </summary>
    public void AbrirCaptura(IReadOnlyList<PcapPacket> paquetes, string ruta)
    {
        Paquetes = paquetes ?? Array.Empty<PcapPacket>();
        RutaCaptura = ruta ?? "";
        CapturaAbierta = true;
        IndicePaquete = Paquetes.Count > 0 ? 0 : -1;
    }

    /// <summary>
    /// Cierra la captura y limpia su estado. Devuelve true si había una abierta, para que
    /// la ventana sepa si debe restaurar la visibilidad de los paneles.
    /// </summary>
    public bool CerrarCaptura()
    {
        if (!CapturaAbierta) return false;
        CapturaAbierta = false;
        Paquetes = Array.Empty<PcapPacket>();
        RutaCaptura = "";
        IndicePaquete = -1;
        return true;
    }

    /// <summary>
    /// Marca el paquete seleccionado. Los índices fuera de rango se ignoran en lugar de
    /// dejar el estado apuntando a un elemento inexistente.
    /// </summary>
    public bool SeleccionarPaquete(int indice)
    {
        if (indice < 0 || indice >= Paquetes.Count) return false;
        IndicePaquete = indice;
        return true;
    }
}
