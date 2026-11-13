using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Media;
using Avalonia.Threading;

namespace Redes.Knowledge.App;

/// <summary>
/// Popups de información "cristalinos" de los botones de acción y su supervisión global.
/// Sustituyen a los tooltips nativos (que se cierran al salir del botón y no permiten
/// enlaces). Extraído del code-behind sin cambiar ninguna constante ni el comportamiento:
/// los comentarios de abajo documentan por qué está hecho así.
/// </summary>
public sealed class SupervisorPopups
{
    /// <summary>Deadline de permanencia: el popup se cierra pasado este tiempo desde que se abrió.</summary>
    public static readonly TimeSpan Deadline = TimeSpan.FromSeconds(5);

    /// <summary>Gracia anti-reabrir tras cerrar por deadline (re-enters espurios de X11).</summary>
    public static readonly TimeSpan Gracia = TimeSpan.FromMilliseconds(1200);

    /// <summary>Regla pura del supervisor: un popup abierto más de <see cref="Deadline"/> se cierra.</summary>
    public static bool DebeCerrarse(DateTime abiertoDesde, DateTime ahora) =>
        ahora - abiertoDesde > Deadline;

    /// <summary>Regla pura: dentro de la gracia no se vuelve a abrir el popup.</summary>
    public static bool EnGracia(DateTime noReabrirHasta, DateTime ahora) => ahora < noReabrirHasta;

    private readonly Window _ventana;

    /// <summary>Estado de un popup de información vigilado por el supervisor global.</summary>
    private sealed class Vigilado
    {
        public required Popup Popup { get; init; }
        public DateTime AbiertoDesde { get; set; }
        public DateTime NoReabrirHasta { get; set; } = DateTime.MinValue;
    }

    // Última posición del puntero (coordenadas de la ventana) para el cierre por geometría
    // de los popups de información (sin parpadeo).
    private Point _ultimaPosPuntero = new(-1, -1);

    // Supervisión GLOBAL: cierre determinista por deadline (5 s desde que el popup se ABRE).
    // Los re-enters espurios de X11 ya no pueden reiniciar un temporizador por-popup (eso
    // dejaba los tooltips abiertos mucho tiempo y de forma errática); la deadline solo se
    // fija en la transición cerrado→abierto.
    private readonly DispatcherTimer _supervisor;
    private readonly List<Vigilado> _vigilados = new();

    public SupervisorPopups(Window ventana)
    {
        _ventana = ventana;
        _supervisor = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(500) };
        _supervisor.Tick += (_, _) =>
        {
            foreach (var v in _vigilados)
            {
                if (!v.Popup.IsOpen) continue;
                if (DebeCerrarse(v.AbiertoDesde, DateTime.UtcNow))
                {
                    v.Popup.IsOpen = false;
                    // Gracia anti-reabrir: en X11 el desmapeo del overlay re-emite enters
                    // espurios; ignorarlos 1,2 s para no volver a abrir el popup al cerrarlo.
                    v.NoReabrirHasta = DateTime.UtcNow + Gracia;
                }
            }
        };
    }

    /// <summary>Arranca la supervisión global de deadlines.</summary>
    public void Iniciar() => _supervisor.Start();

    /// <summary>Posición del puntero en coordenadas de la ventana, para el cierre por geometría.</summary>
    public void RegistrarPuntero(Point posicion) => _ultimaPosPuntero = posicion;

    /// <summary>
    /// Crea el popup de información "cristalino". Estilo profesional: acrílico oscuro
    /// translúcido (blur real de Avalonia), borde claro, esquinas redondeadas y ancho limitado
    /// al de la ventana actual (el texto largo se envuelve en multilínea y nunca se escapa por
    /// el borde).
    /// Permanencia SIN PARPADEO: el cierre se decide por GEOMETRÍA (posición del puntero contra
    /// el rect del botón), no por PointerEntered/Exited del overlay (que al abrir disparan
    /// eventos espurios). La posición la mantiene el PointerMoved global. Además hay un
    /// AUTO-CIERRE por tiempo INCONDICIONAL (igual que el ShowDuration del ToolTip nativo): en
    /// Linux/X11 los eventos del overlay no llegan de forma fiable, así que a los 5 s el popup
    /// se cierra siempre (con una breve gracia anti-reabrir para no dejarlo abierto de nuevo por
    /// los re-enter espurios al desmapear).
    /// Si se pasa <paramref name="alPulsar"/>, se engancha al contenido y cierra el popup.
    /// </summary>
    public void Registrar(Control anfitrion, Control contenido, Action? alPulsar = null)
    {
        // Cristal acrílico: el ExperimentalAcrylicBorder aporta el fondo translúcido con blur;
        // un Border exterior añade borde, esquinas, padding y el límite de ancho.
        var borde = new Border
        {
            // Transparente para recibir punteros en toda su área (el acrílico lo pinta).
            Background = Brushes.Transparent,
            BorderBrush = RecursoPopupBrush("tBordePopup", "#55FFFFFF"),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(8),
            Padding = new Thickness(1),
            MaxWidth = Math.Max(320, _ventana.Bounds.Width - 48),
            Child = new ExperimentalAcrylicBorder
            {
                Material = new ExperimentalAcrylicMaterial
                {
                    BackgroundSource = AcrylicBackgroundSource.Digger,
                    TintColor = RecursoPopupColor("tFondoPopup", "#22252A"),
                    TintOpacity = 0.85,
                    MaterialOpacity = 0.75
                },
                CornerRadius = new CornerRadius(7),
                Padding = new Thickness(10, 7),
                Child = contenido
            }
        };

        var popup = new Popup
        {
            PlacementTarget = anfitrion,
            Placement = PlacementMode.Bottom,
            VerticalOffset = 4,
            // SIN light-dismiss: con él, el primer clic sobre el botón lo consume el propio
            // popup y el botón no recibe la pulsación. El cierre lo gestionamos nosotros
            // (geometría del botón + flags de entrada/salida del popup).
            IsLightDismissEnabled = false,
            Child = borde
        };
        // El Popup debe estar en el árbol visual de la ventana para poder abrirse.
        if (_ventana.Content is Panel raiz && !raiz.Children.Contains(popup))
            raiz.Children.Add(popup);

        // CIERRE determinista por DEADLINE (supervisor global): el popup se cierra 5 s después
        // de ABRIRSE, sin depender de flags ni eventos del overlay. En Linux/X11 los
        // PointerEntered autobiográficos del botón/popup se re-emiten sin orden (reiniciaban un
        // temporizador por-popup → cierres a los 30-60 s y erráticos); aquí la deadline se fija
        // solo en la transición cerrado→abierto, así que ningún re-enter la puede prolongar. El
        // cierre por geometría (salir del botón) sigue como atajo cuando los eventos SÍ llegan.
        var vig = new Vigilado { Popup = popup };
        _vigilados.Add(vig);

        void OcultarPopup() => popup.IsOpen = false;

        void AbrirSiProcede()
        {
            if (EnGracia(vig.NoReabrirHasta, DateTime.UtcNow)) return;
            var yaAbierto = popup.IsOpen;
            popup.IsOpen = true;
            if (!yaAbierto) vig.AbiertoDesde = DateTime.UtcNow;
        }

        bool PunteroDentro()
        {
            var vacio = new Rect(0, 0, 0, 0);
            var botonRect = anfitrion.TranslatePoint(new Point(0, 0), _ventana) is { } pb
                ? new Rect(pb, anfitrion.Bounds.Size)
                : vacio;
            return botonRect.Contains(_ultimaPosPuntero);
        }

        void ProgramarCierre()
        {
            var timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(400) };
            timer.Tick += (_, _) =>
            {
                timer.Stop();
                if (!PunteroDentro()) OcultarPopup();
            };
            timer.Start();
        }

        anfitrion.PointerEntered += (_, _) => AbrirSiProcede();
        anfitrion.PointerExited += (_, _) => ProgramarCierre();
        // Al pulsar el botón se cierra el popup sin consumir el clic: el botón sigue
        // ejecutando su acción normal (Comparar, Exportar, Abrir captura, Muestra…).
        anfitrion.PointerPressed += (_, _) => OcultarPopup();
        borde.PointerEntered += (_, _) => AbrirSiProcede();
        borde.PointerExited += (_, _) => ProgramarCierre();
        if (alPulsar is not null)
            contenido.PointerPressed += (_, _) =>
            {
                OcultarPopup();
                alPulsar();
            };
    }

    // Colores del popup cristalino (legibles en tema claro y oscuro). Viven como TOKENS
    // permanentes en App.axaml; aquí se resuelven con respaldo al valor actual del token
    // (si la resolución de recursos fallara, el popup sigue como estaba).
    public static IBrush TextoPopup => RecursoPopupBrush("tTextoPopup", "#F2F2F2");

    public static IBrush EnlacePopup => RecursoPopupBrush("tEnlacePopup", "#6CBAFF");

    private static IBrush RecursoPopupBrush(string clave, string respaldoHex)
    {
        if (Application.Current?.TryGetResource(clave, null, out var v) is true && v is IBrush br) return br;
        return new SolidColorBrush(Color.Parse(respaldoHex));
    }

    private static Color RecursoPopupColor(string clave, string respaldoHex)
    {
        if (Application.Current?.TryGetResource(clave, null, out var v) is true && v is SolidColorBrush sc) return sc.Color;
        return Color.Parse(respaldoHex);
    }
}
