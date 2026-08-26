using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Threading;

namespace Redes.Knowledge.App;

/// <summary>
/// TextBlock con efecto carrusel (marquee) para los ítems de un desplegable que el
/// usuario recorre con el ratón:
/// - Dentro del popup (lista abierta): el texto se desplaza SOLO cuando el puntero
///   está sobre ese ítem; sin puntero, queda estático con elipsis si no cabe.
/// - Fuera del popup (recuadro del selector tras elegir): siempre estático con elipsis
///   (la animación allí es superflua).
/// La ventanita de la lista conserva un ancho fijo (el Border del template fija el ancho).
/// </summary>
public sealed class MarqueeTextBlock : TextBlock
{
    private DispatcherTimer? _timer;
    private readonly TranslateTransform _transform = new();
    private double _anchoTexto;
    private int _ticksInicio;
    private bool _enPopup;   // true si este control está dentro del popup del desplegable
    private bool _animando;

    private const double VelocidadPx = 1.5;   // px por tick (~90 px/s a 60 FPS)
    private const double HuecoPx = 24;        // respiro entre fin y reinicio
    private const int InicioEsperaTicks = 12; // pausa breve (~0,2 s) al entrar con el ratón

    public MarqueeTextBlock()
    {
        RenderTransform = _transform;
        TextTrimming = TextTrimming.CharacterEllipsis;
        VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center;
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
        Replantear();
        return size;
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        // Dentro del popup del ComboBox, el root visual es un PopupRoot (no Window);
        // el recuadro del selector pertenece al Window.
        _enPopup = VisualRoot is not Window;
        Replantear();
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        PararAnimacion();
        base.OnDetachedFromVisualTree(e);
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        // Reacciona al puntero para arrancar/parar el carrusel en el ítem bajo el ratón.
        if (change.Property == IsPointerOverProperty)
            Replantear();
    }

    private void Replantear()
    {
        var ancho = Bounds.Width > 0 ? Bounds.Width : DesiredSize.Width;
        if (ancho <= 0) { PararAnimacion(); return; }

        // Fuera del popup: siempre estático con elipsis.
        if (!_enPopup)
        {
            PararAnimacion();
            TextTrimming = TextTrimming.CharacterEllipsis;
            _transform.X = 0;
            return;
        }

        // Dentro del popup: carrusel solo bajo el puntero del ratón.
        if (!IsPointerOver || _anchoTexto <= ancho + 1)
        {
            PararAnimacion();
            TextTrimming = TextTrimming.CharacterEllipsis;
            _transform.X = 0;
            return;
        }

        // Por encima y desborda: carrusel.
        TextTrimming = TextTrimming.None;
        if (_timer is null)
        {
            _timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(16) };
            _timer.Tick += (_, _) => Tick();
        }
        if (!_animando)
        {
            _animando = true;
            _ticksInicio = 0;
            _transform.X = ancho + HuecoPx; // entra desde la derecha
        }
        _timer.Start();
    }

    private void Tick()
    {
        var ancho = Bounds.Width > 0 ? Bounds.Width : DesiredSize.Width;
        if (ancho <= 0) return;

        if (!IsPointerOver || _anchoTexto <= ancho + 1)
        {
            Replantear(); // salió el puntero o ya cabe → detener
            return;
        }

        if (_ticksInicio < InicioEsperaTicks)
        {
            _ticksInicio++;
            return; // pausa breve antes de moverse
        }

        _transform.X -= VelocidadPx;
        if (_transform.X + _anchoTexto < 0)
            _transform.X = ancho + HuecoPx; // salió por la izquierda → vuelve a entrar
    }

    private void PararAnimacion()
    {
        if (_timer is not null) _timer.Stop();
        _animando = false;
    }
}