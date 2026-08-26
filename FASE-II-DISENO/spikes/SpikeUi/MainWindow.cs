using System;
using System.Collections.Generic;
using System.Diagnostics;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Styling;

namespace SpikeUi;

/// <summary>
/// Spike D0-2 — UI rica con grandes volúmenes:
/// - DockPanel con barra superior, panel lateral y área central;
/// - ListBox virtualizado (VirtualizingStackPanel por defecto) con 10.000 filas;
/// - tema claro/oscuro en tiempo de ejecución (Application.RequestedThemeVariant).
/// </summary>
public class MainWindow : Window
{
    private readonly TextBlock _status;

    public MainWindow()
    {
        Title = "Spike D0-2 · UI rica (Avalonia 12)";
        Width = 1200;
        Height = 720;

        var dock = new DockPanel();

        // Barra superior
        var btn = new Button { Content = "Tema claro / oscuro", Margin = new Thickness(8) };
        btn.Click += (_, _) => ToggleTheme();
        DockPanel.SetDock(btn, Dock.Top);
        dock.Children.Add(btn);

        // Panel lateral (exploración por ejes F0)
        var side = new Border
        {
            Width = 260,
            Child = new TextBlock
            {
                Text = "Panel lateral (DockPanel)\nExploración por ejes F0:\nfamilias · capas · planos · dominios",
                Margin = new Thickness(8)
            }
        };
        DockPanel.SetDock(side, Dock.Left);
        dock.Children.Add(side);

        // Área central: lista virtualizada con 10.000 filas
        var rows = new List<string>();
        var sw = Stopwatch.StartNew();
        for (var i = 0; i < 10_000; i++)
        {
            rows.Add($"Fila-{i:D5} | TCP | puerto 443 | trama 64 B | segmento");
        }
        sw.Stop();

        // Área central: ListBox virtualizado (VirtualizingStackPanel por defecto).
        // Avalonia 12: Dock enum = {Left, Bottom, Right, Top} (no existe Fill) →
        // el último hijo sin Dock explícito rellena el espacio restante (LastChildFill).
        var list = new ListBox { ItemsSource = rows };
        dock.Children.Add(list);

        // Barra de estado
        _status = new TextBlock { Margin = new Thickness(8) };
        DockPanel.SetDock(_status, Dock.Bottom);
        dock.Children.Add(_status);

        Content = dock;
        _status.Text = $"Filas: {rows.Count} · generación de datos: {sw.ElapsedMilliseconds} ms · ListBox virtualizado · tema: {Application.Current?.RequestedThemeVariant?.Key ?? "? "}";
    }

    private static void ToggleTheme()
    {
        var app = Application.Current;
        if (app is null) return;
        app.RequestedThemeVariant = app.RequestedThemeVariant == ThemeVariant.Dark
            ? ThemeVariant.Light
            : ThemeVariant.Dark;
    }
}