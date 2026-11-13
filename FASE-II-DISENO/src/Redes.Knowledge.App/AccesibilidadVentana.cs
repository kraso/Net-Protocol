using Avalonia.Automation;
using Avalonia.Controls;
using Redes.Knowledge.Visualization;

namespace Redes.Knowledge.App;

/// <summary>
/// Nombres y ayudas que la ventana expone a un lector de pantalla. Vive fuera del code-behind
/// para que el cableado de la ventana no mezcle accesibilidad con textos visibles. Todo sale
/// del catálogo <see cref="Textos"/>, así que sigue al idioma activo.
/// <para>
/// Sin estos nombres, un lector anuncia botones vacíos (los glifos — □ ✕ de la barra propia) o
/// campos sin propósito.
/// </para>
/// </summary>
internal static class AccesibilidadVentana
{
    /// <summary>Vuelca nombres accesibles, ayudas largas y tooltips de la barra de título.</summary>
    public static void Aplicar(MainWindow ventana)
    {
        AutomationProperties.SetName(ventana.MinimizeButton, Textos.T("acc.ventana.minimizar"));
        AutomationProperties.SetName(ventana.MaximizeButton, Textos.T("acc.ventana.maximizar"));
        AutomationProperties.SetName(ventana.CloseButton, Textos.T("acc.ventana.cerrar"));
        ToolTip.SetTip(ventana.MinimizeButton, Textos.T("acc.ventana.minimizar"));
        ToolTip.SetTip(ventana.MaximizeButton, Textos.T("acc.ventana.maximizar"));
        ToolTip.SetTip(ventana.CloseButton, Textos.T("acc.ventana.cerrar"));

        AutomationProperties.SetName(ventana.SearchBox, Textos.T("acc.busqueda"));
        AutomationProperties.SetName(ventana.SearchButton, Textos.T("buscar.boton"));
        AutomationProperties.SetName(ventana.ProtocolSelector, Textos.T("acc.protocolo"));
        AutomationProperties.SetName(ventana.CompareTarget, Textos.T("acc.comparar.con"));
        AutomationProperties.SetName(ventana.CompareButton, Textos.T("boton.comparar"));
        AutomationProperties.SetHelpText(ventana.CompareButton, Textos.T("help.comparar"));
        AutomationProperties.SetName(ventana.VolverFichaButton, Textos.T("boton.volver.ficha"));
        AutomationProperties.SetName(ventana.FilterFamilia, Textos.T("acc.familia"));
        AutomationProperties.SetName(ventana.FilterEstado, Textos.T("acc.estado"));
        AutomationProperties.SetName(ventana.ExportFormat, Textos.T("acc.formato.exportacion"));
        AutomationProperties.SetName(ventana.ExportButton, Textos.T("boton.exportar"));
        AutomationProperties.SetHelpText(ventana.ExportButton, Textos.T("help.exportar"));
        AutomationProperties.SetName(ventana.AbrirCapturaButton, Textos.T("boton.abrir.captura"));
        AutomationProperties.SetHelpText(ventana.AbrirCapturaButton, Textos.T("help.abrir.captura"));
        AutomationProperties.SetName(ventana.MuestraButton, Textos.T("boton.muestra"));
        AutomationProperties.SetHelpText(ventana.MuestraButton, Textos.T("help.muestra"));
        AutomationProperties.SetName(ventana.LanguageButton, Textos.T("acc.idioma"));
        AutomationProperties.SetName(ventana.NavFilter, Textos.T("acc.filtro.familias"));
        AutomationProperties.SetName(ventana.ThemeButton, Textos.T("sidebar.tema"));
        AutomationProperties.SetName(ventana.LegendButton, Textos.T("sidebar.leyenda"));
        AutomationProperties.SetName(ventana.AboutButton, Textos.T("sidebar.acerca"));
        AutomationProperties.SetName(ventana.DetailText, Textos.T("acc.ficha"));
        AutomationProperties.SetName(ventana.DiagramTitle, Textos.T("acc.diagramas"));
        AutomationProperties.SetName(ventana.StatusText, Textos.T("acc.estado.barra"));
        AutomationProperties.SetName(ventana.ResumenCaptura, Textos.T("acc.captura.resumen"));
        AutomationProperties.SetName(ventana.ListaPaquetes, Textos.T("acc.captura.paquetes"));
        AutomationProperties.SetName(ventana.DetalleCaptura, Textos.T("acc.captura.detalle"));
        AutomationProperties.SetName(ventana.CerrarCapturaButton, Textos.T("boton.cerrar.captura"));
    }
}
