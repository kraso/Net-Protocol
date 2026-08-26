using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Threading;

namespace Redes.Knowledge.App;

/// <summary>
/// TextBlock con efecto carrusel (marquee): si el texto mide más que el ancho
/// disponible, se desplaza horizontalmente en bucle (desde el borde derecho hasta
/// que termina, y vuelve a entrar); si cabe, se queda estático. El recorte visual
/// lo hace el contenedor (Border con ClipToBounds) que lo envuelva.
/// </summary>
public sealed class MarqueeTextBlock : TextBlock
{
    private DispatcherTimer? _timer;
    private readonly TranslateTransform _transform = new();
    private double _anchoTexto;
    private int _ticksInicio;

    private const double VelocidadPx = 1.5;                 // px por tick (~90 px/s a 60 FPS)
    private const double HuecoPx = 24;                      // respiro entre fin y reinicio
    private const int InicioEsperaTicks = 50;               // pausa (~0,8 s) antes de empezar a mover

    public MarqueeTextBlock()
    {
        RenderTransform = _transform;
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        var size = base.MeasureOverride(availableSize);
        var texto = Text ?? "";
        if (texto.Length == 0)
        {
            _anchoTexto = 0;
            return size;
        }

        var typeface = new Typeface(FontFamily, FontStyle, FontWeight);
        var ft = new FormattedText(texto,
            CultureInfo.CurrentCulture,
            FlowDirection.LeftToRight,
            typeface,
            FontSize,
            Foreground ?? Brushes.Black);
        _anchoTexto = ft.Width;
        Replantear(true);
        return size;
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        Stop();
        base.OnDetachedFromVisualTree(e);
    }

    private void Replantear(bool forzarInicio)
    {
        // Ancho útil real (ya colocado); si aún no hay bounds, usamos el ancho solicitado.
        var ancho = Bounds.Width > 0 ? Bounds.Width : DesiredSize.Width;
        if (ancho <= 0) { Stop(); return; }

        if (_anchoTexto <= ancho + 1)
        {
            // Cabe: sin movimiento.
            Stop();
            _transform.X = 0;
            return;
        }

        if (_timer is not null && forzarInicio) return;

        if (_timer is null)
        {
            _timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(16) };
            _timer.Tick += (_, _) => Tick();
        }

        if (forzarInicio)
        {
            _ticksInicio = 0;
            _transform.X = ancho + HuecoPx; // entra desde la derecha
        }
        _timer.Start();
    }

    private void Tick()
    {
        var ancho = Bounds.Width > 0 ? Bounds.Width : DesiredSize.Width;
        if (ancho <= 0) return;

        if (_anchoTexto <= ancho + 1)
        {
            Stop();
            _transform.X = 0;
            return;
        }

        if (_ticksInicio < InicioEsperaTicks)
        {
            _ticksInicio++;
            return; // pausa inicial para poder leer el principio
        }

        _transform.X -= VelocidadPx;
        if (_transform.X + _anchoTexto < 0)
            _transform.X = ancho + HuecoPx; // salió por la izquierda → vuelve a entrar
    }

    private void Stop()
    {
        _timer?.Stop();
    }
}