using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
using System.Linq;
using Avalonia.Markup.Xaml;
using PokemonEssentialsEditorEvs.Infrastructure;
using PokemonEssentialsEditorEvs.ViewModels;
using PokemonEssentialsEditorEvs.Views;

namespace PokemonEssentialsEditorEvs;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        // ── 1. Construir el contenedor de DI ──────────────────────────────
        var provider = AppServices.Build();
        ServiceLocator.Initialize(provider);

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            DisableAvaloniaDataAnnotationValidation();

            // ── 2. Resolver el ViewModel desde el contenedor ──────────────
            // Cero "new". El contenedor sabe cómo construirlo con sus dependencias.
            desktop.MainWindow = new MainWindow
            {
                DataContext = ServiceLocator.Get<MainWindowViewModel>()
            };
        }

        base.OnFrameworkInitializationCompleted();
    }

    private static void DisableAvaloniaDataAnnotationValidation()
    {
        var toRemove = BindingPlugins.DataValidators
            .OfType<DataAnnotationsValidationPlugin>()
            .ToArray();
        foreach (var plugin in toRemove)
            BindingPlugins.DataValidators.Remove(plugin);
    }
}