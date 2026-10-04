using System;
using System.Collections.Generic;
using System.IO;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform;
using bossfight_client.Views;

namespace bossfight_client;

/// <summary>
/// Owns the system-tray icon and the "closing the window hides to tray" behaviour.
/// </summary>
internal static class TrayIconController
{
    private const string IconUri = "avares://bossfight-client/Assets/avalonia-logo.ico";

    private static TrayIcon? _trayIcon;
    private static bool _allowClose;

    public static void Attach(App app, MainWindow window)
    {
        var menu = new NativeMenu();

        var showItem = new NativeMenuItem("Show bossfight-client");
        showItem.Click += (_, _) => Show(window);

        var quitItem = new NativeMenuItem("Quit");
        quitItem.Click += (_, _) => Quit(window);

        menu.Add(showItem);
        menu.Add(quitItem);

        _trayIcon = new TrayIcon
        {
            Icon = LoadIcon(),
            ToolTipText = "bossfight-client",
            Menu = menu
        };

        // TrayIcons is an attached property on Application, so the icon lives for the
        // whole process lifetime regardless of window visibility.
        TrayIcons icons = app.GetValue(TrayIcon.IconsProperty) as TrayIcons ?? new TrayIcons();
        app.SetValue(TrayIcon.IconsProperty, icons);
        icons.Add(_trayIcon);

        window.Closing += OnWindowClosing;
    }

    public static void Detach()
    {
        _trayIcon?.Dispose();
        _trayIcon = null;
    }

    /// <summary>
    /// Lets the window really close (used by the tray's Quit item).
    /// </summary>
    public static void AllowClose(MainWindow window)
    {
        _allowClose = true;
        window.Close();
    }

    private static void OnWindowClosing(object? sender, WindowClosingEventArgs e)
    {
        // Only intercept a user-initiated close. Programmatic closes (OS shutdown,
        // lifetime shutdown) must be allowed through so the app can exit.
        if (_allowClose || e.IsProgrammatic)
        {
            return;
        }

        e.Cancel = true;

        if (sender is Window window)
        {
            window.Hide();
        }
    }

    private static void Show(MainWindow window)
    {
        window.Show();

        if (window.WindowState == WindowState.Minimized)
        {
            window.WindowState = WindowState.Normal;
        }

        window.Activate();
    }

    private static void Quit(MainWindow window)
    {
        _allowClose = true;

        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.Shutdown();
            return;
        }

        window.Close();
    }

    private static WindowIcon? LoadIcon()
    {
        try
        {
            using Stream stream = AssetLoader.Open(new Uri(IconUri));
            return new WindowIcon(stream);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Trace.TraceWarning($"Tray icon could not be loaded: {ex.Message}");
            return null;
        }
    }
}