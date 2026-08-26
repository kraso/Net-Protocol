using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Threading;

namespace Redes.Knowledge.App;

/// <summary>
/// Carrusel (marquee) para los ítems de un desplegable que el usuario recorre con el ratón.
/// Es un Grid de ancho fijo con ClipToBounds que contiene un TextBlock SIEMPRE visible;
/// el texto se desplaza cambiando su Margin (el relayout del contenedor fuerza el
/// repintado, sin depender del renderer de transforms).
/// - Dentro del popup: se desplaza SOLO cuando el puntero está sobre ese ítem.
/// - Fuera del popup (recuadro del selector tras elegir): estático con elipsis.
/// </summary>
public sealed class MarqueeTextBlock : Grid
{
    private readonly TextBlock _texto = new()
    {
        VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
        HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left
    };

    private DispatcherTimer? _timer;
    private double _anchoTexto;
    private int _ticksInicio;
    private bool _enPopup;    // true si este control está dentro del popup del desplegable
    private bool _punteroSobre;
    private bool _animando;

    private const double VelocidadPx = 1.5;   // px por tick (~90 px/s a 60 FPS)
    private const double HuecoPx = 24;        // respiro entre fin y reinicio
    private const int InicioEsperaTicks = 12; // pausa breve (~0,2 s) al entrar con el ratón

    public MarqueeTextBlock()
    {
        ClipToBounds = true;
        Background = Brushes.Transparent; // imprescindible para recibir el puntero
        Children.Add(_texto);

        // Reenvío de propiedades al TextBlock interno (mismas propiedades de TextBlock,
        // sin AddOwner: sin problemas de nulabilidad y binding directo).
        _texto.Bind(TextBlock.TextProperty, this.GetObservable(TextBlock.TextProperty));
        _texto.Bind(TextBlock.FontFamilyProperty, this.GetObservable(TextBlock.FontFamilyProperty));
        _texto.Bind(TextBlock.FontSizeProperty, this.GetObservable(TextBlock.FontSizeProperty));
        _texto.Bind(TextBlock.FontStyleProperty, this.GetObservable(TextBlock.FontStyleProperty));
        _texto.Bind(TextBlock.FontWeightProperty, this.GetObservable(TextBlock.FontWeightProperty));

        PointerEntered += (_, _) => { _punteroSobre = true; Replantear(); };
        PointerExited += (_, _) => { _punteroSobre = false; Replantear(); };
    }

    // Propiedades públicas que delegan directamente en las de TextBlock.
    public string? Text
    {
        get => GetValue(TextBlock.TextProperty);
        set => SetValue(TextBlock.TextProperty, value);
    }

    public FontFamily? FontFamily
    {
        get => GetValue(TextBlock.FontFamilyProperty);
        set => SetValue(TextBlock.FontFamilyProperty, value!);
    }

    public double FontSize
    {
        get => GetValue(TextBlock.FontSizeProperty);
        set => SetValue(TextBlock.FontSizeProperty, value);
    }

    public FontStyle FontStyle
    {
        get => GetValue(TextBlock.FontStyleProperty);
        set => SetValue(TextBlock.FontStyleProperty, value);
    }

    public FontWeight FontWeight
    {
        get => GetValue(TextBlock.FontWeightProperty);
        set => SetValue(TextBlock.FontWeightProperty, value);
    }

    public IBrush? Foreground
    {
        get => GetValue(TextBlock.ForegroundProperty);
        set => SetValue(TextBlock.ForegroundProperty, value);
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        // Dentro del popup del ComboBox, el root visual es un PopupRoot (no Window);
        // el recuadro del selector pertenece al Window.
        _enPopup = VisualRoot is not Window;
        _texto.TextTrimming = _enPopup ? TextTrimming.None : TextTrimming.CharacterEllipsis;
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
        if (change.Property == BoundsProperty || change.Property == TextBlock.TextProperty)
            Replantear();
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        var size = base.MeasureOverride(availableSize);
        var texto = _texto.Text ?? "";
        if (texto.Length == 0)
        {
            _anchoTexto = 0;
            return size;
        }

        var typeface = new Typeface(_texto.FontFamily, _texto.FontStyle, _texto.FontWeight);
        var ft = new FormattedText(texto,
            CultureInfo.CurrentCulture,
            FlowDirection.LeftToRight,
            typeface,
            _texto.FontSize,
            _texto.Foreground ?? Brushes.Black);
        _anchoTexto = ft.Width;
        return size;
    }

    private void Replantear()
    {
        var ancho = Bounds.Width > 0 ? Bounds.Width : DesiredSize.Width;
        if (ancho <= 0) { PararAnimacion(); return; }

        // Fuera del popup: siempre estático con elipsis.
        if (!_enPopup)
        {
            PararAnimacion();
            _texto.Margin = new Thickness(0);
            _texto.TextTrimming = TextTrimming.CharacterEllipsis;
            return;
        }

        // Dentro del popup: carrusel solo bajo el puntero del ratón.
        if (!_punteroSobre || _anchoTexto <= ancho + 1)
        {
            PararAnimacion();
            _texto.Margin = new Thickness(0);
            _texto.TextTrimming = TextTrimming.CharacterEllipsis;
            return;
        }

        // Por encima y desborda: carrusel.
        _texto.TextTrimming = TextTrimming.None;
        if (_timer is null)
        {
            _timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(16) };
            _timer.Tick += (_, _) => Tick();
        }
        if (!_animando)
        {
            _animando = true;
            _ticksInicio = 0;
            // Entra desde la derecha: margin izquierdo igual al ancho del contenedor.
            _texto.Margin = new Thickness(ancho + HuecoPx, 0, 0, 0);
        }
        _timer.Start();
    }

    private void Tick()
    {
        var ancho = Bounds.Width > 0 ? Bounds.Width : DesiredSize.Width;
        if (ancho <= 0) return;

        if (!_punteroSobre || _anchoTexto <= ancho + 1)
        {
            Replantear(); // salió el puntero o ya cabe → detener
            return;
        }

        if (_ticksInicio < InicioEsperaTicks)
        {
            _ticksInicio++;
            return; // pausa breve antes de moverse
        }

        var x = _texto.Margin.Left - VelocidadPx;
        if (x + _anchoTexto < 0)
            x = ancho + HuecoPx; // salió por la izquierda → vuelve a entrar
        // Cambiar el Margin recoloca el texto (layout) → repintado garantizado.
        _texto.Margin = new Thickness(x, 0, 0, 0);
    }

    private void PararAnimacion()
    {
        if (_timer is not null) _timer.Stop();
        _animando = false;
    }
}