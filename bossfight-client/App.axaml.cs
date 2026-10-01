using System;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core;
using Avalonia.Data.Core.Plugins;
using System.Linq;
using Avalonia.Markup.Xaml;
using bossfight_client.Services;
using bossfight_client.ViewModels;
using bossfight_client.Views;
using StoreModel = bossfight_client.Store.Store;

namespace bossfight_client;

public partial class App : Application
{
    private MainWindow? _mainWindow;

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            // Avoid duplicate validations from both Avalonia and the CommunityToolkit.
            // More info: https://docs.avaloniaui.net/docs/guides/development-guides/data-validation#manage-validationplugins
            DisableAvaloniaDataAnnotationValidation();

            StoreModel store;
            try
            {
                store = StoreModel.Load();
            }
            catch (Exception ex)
            {
                // A missing or broken store should not stop the app from starting: the
                // whole point of the settings form is to let the user fix it.
                store = new StoreModel();
                Desktop.PostStartupError(ex);
            }

            var viewModel = new MainViewModel(store, new SamplePlayerIdProvider());

            _mainWindow = new MainWindow
            {
                DataContext = viewModel
            };

            desktop.MainWindow = _mainWindow;

            // Keep the process alive after the window is hidden to the tray.
            desktop.ShutdownMode = ShutdownMode.OnExplicitShutdown;

            _ = InitializeAsync(viewModel);

            TrayIconController.Attach(this, _mainWindow);

            desktop.Exit += (_, _) => TrayIconController.Detach();
        }
        else if (ApplicationLifetime is ISingleViewApplicationLifetime singleViewPlatform)
        {
            singleViewPlatform.MainView = new MainView
            {
                DataContext = new MainViewModel(new StoreModel(), new SamplePlayerIdProvider())
            };
        }

        base.OnFrameworkInitializationCompleted();
    }

    private static async Task InitializeAsync(MainViewModel viewModel)
    {
        try
        {
            await viewModel.InitializeAsync();
        }
        catch (Exception ex)
        {
            Desktop.PostStartupError(ex);
        }
    }

    private void DisableAvaloniaDataAnnotationValidation()
    {
        // Get an array of plugins to remove
        var dataValidationPluginsToRemove =
            BindingPlugins.DataValidators.OfType<DataAnnotationsValidationPlugin>().ToArray();

        // remove each entry found
        foreach (var plugin in dataValidationPluginsToRemove)
        {
            BindingPlugins.DataValidators.Remove(plugin);
        }
    }

    private static class Desktop
    {
        public static void PostStartupError(Exception ex)
        {
            System.Diagnostics.Trace.TraceError($"bossfight-client startup: {ex}");
        }
    }
}