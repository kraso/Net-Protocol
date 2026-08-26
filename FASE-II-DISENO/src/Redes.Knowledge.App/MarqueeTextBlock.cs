using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Threading;

namespace Redes.Knowledge.App;

/// <summary>
/// Carrusel (marquee) para los ítems de un desplegable que el usuario recorre con el ratón.
/// Es un Canvas de ancho fijo (recorta con ClipToBounds) que reposiciona un TextBlock hijo
/// con Canvas.SetLeft en cada tick: el reposicionamiento invalida el layout → repintado
/// garantizado (no depende de que el renderer repinte un RenderTransform).
/// - Dentro del popup: el texto se desplaza SOLO cuando el puntero está sobre ese ítem.
/// - Fuera del popup (recuadro del selector tras elegir): estático con elipsis.
/// </summary>
public sealed class MarqueeTextBlock : Canvas
{
    private readonly TextBlock _texto = new()
    {
        VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center
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
        // El propio Canvas recorta: así no hay que envolverlo en un Border externo.
        ClipToBounds = true;
        Background = Brushes.Transparent; // imprescindible para recibir el puntero
        Children.Add(_texto);

        // Ruta de propiedades: reenviamos propiedades al TextBlock interno.
        _texto.Bind(TextBlock.TextProperty, this.GetObservable(TextProperty));
        _texto.Bind(TextBlock.FontSizeProperty, this.GetObservable(FontSizeProperty));
        _texto.Bind(TextBlock.FontStyleProperty, this.GetObservable(FontStyleProperty));
        _texto.Bind(TextBlock.FontWeightProperty, this.GetObservable(FontWeightProperty));
        _texto.Bind(TextBlock.ForegroundProperty, this.GetObservable(ForegroundProperty));

        PointerEntered += (_, _) => { _punteroSobre = true; Replantear(); };
        PointerExited += (_, _) => { _punteroSobre = false; Replantear(); };
    }

    // Recubre las propiedades de TextBlock para que el Canvas se use igual en XAML/código.
    public static readonly StyledProperty<string?> TextProperty =
        TextBlock.TextProperty.AddOwner<MarqueeTextBlock>();

    public string? Text
    {
        get => GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    public static readonly StyledProperty<FontFamily> FontFamilyProperty =
        TextBlock.FontFamilyProperty.AddOwner<MarqueeTextBlock>();

    public FontFamily? FontFamily
    {
        get => GetValue(FontFamilyProperty);
        set => SetValue(FontFamilyProperty, value!);
    }

    public static readonly StyledProperty<double> FontSizeProperty =
        TextBlock.FontSizeProperty.AddOwner<MarqueeTextBlock>();

    public double FontSize
    {
        get => GetValue(FontSizeProperty);
        set => SetValue(FontSizeProperty, value);
    }

    public static readonly StyledProperty<FontStyle> FontStyleProperty =
        TextBlock.FontStyleProperty.AddOwner<MarqueeTextBlock>();

    public FontStyle FontStyle
    {
        get => GetValue(FontStyleProperty);
        set => SetValue(FontStyleProperty, value);
    }

    public static readonly StyledProperty<FontWeight> FontWeightProperty =
        TextBlock.FontWeightProperty.AddOwner<MarqueeTextBlock>();

    public FontWeight FontWeight
    {
        get => GetValue(FontWeightProperty);
        set => SetValue(FontWeightProperty, value);
    }

    public static readonly StyledProperty<IBrush?> ForegroundProperty =
        TextBlock.ForegroundProperty.AddOwner<MarqueeTextBlock>();

    public IBrush? Foreground
    {
        get => GetValue(ForegroundProperty);
        set => SetValue(ForegroundProperty, value);
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
        if (change.Property == BoundsProperty)
            Replantear();
        if (change.Property == TextProperty)
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

    protected override Size ArrangeOverride(Size finalSize) => finalSize;

    private void Replantear()
    {
        var ancho = Bounds.Width > 0 ? Bounds.Width : DesiredSize.Width;
        if (ancho <= 0) { PararAnimacion(); return; }

        // Fuera del popup: siempre estático con elipsis.
        if (!_enPopup)
        {
            PararAnimacion();
            _texto.TextTrimming = TextTrimming.CharacterEllipsis;
            Canvas.SetLeft(_texto, 0);
            return;
        }

        // Dentro del popup: carrusel solo bajo el puntero del ratón.
        if (!_punteroSobre || _anchoTexto <= ancho + 1)
        {
            PararAnimacion();
            _texto.TextTrimming = TextTrimming.CharacterEllipsis;
            Canvas.SetLeft(_texto, 0);
            return;
        }

        // Por encima y desborda: carrusel.
        _texto.TextTrimming = TextTrimming.None;
        _texto.Width = double.NaN;       // deja que el texto ocupe su ancho natural
        if (_timer is null)
        {
            _timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(16) };
            _timer.Tick += (_, _) => Tick();
        }
        if (!_animando)
        {
            _animando = true;
            _ticksInicio = 0;
            Canvas.SetLeft(_texto, ancho + HuecoPx); // entra desde la derecha
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

        var x = Canvas.GetLeft(_texto) - VelocidadPx;
        if (x + _anchoTexto < 0)
            x = ancho + HuecoPx; // salió por la izquierda → vuelve a entrar
        Canvas.SetLeft(_texto, x); // reposicionar invalida el layout → repintado garantizado
    }

    private void PararAnimacion()
    {
        if (_timer is not null) _timer.Stop();
        _animando = false;
    }
}