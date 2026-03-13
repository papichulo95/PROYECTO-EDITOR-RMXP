using System;
using System.IO;
using Microsoft.Extensions.DependencyInjection;
using PokemonEssentialsEditorEvs.Services;
using PokemonEssentialsEditorEvs.State;
using PokemonEssentialsEditorEvs.ViewModels;

namespace PokemonEssentialsEditorEvs.Infrastructure;

/// <summary>
/// El único lugar donde se define qué implementación satisface cada contrato.
/// Añadir un servicio nuevo = una línea aquí. Nada más.
/// </summary>
public static class AppServices
{
    public static IServiceProvider Build()
    {
        var services = new ServiceCollection();

        // ── Infraestructura ────────────────────────────────────────────────
        services.AddSingleton(EditorStateMachine.Instance);

        // ── Servicios de dominio ───────────────────────────────────────────
        services.AddSingleton<IProjectLoaderService, ProjectLoaderService>();
        services.AddSingleton<ICommandTranslatorService>(provider =>
        {
            var svc = new CommandTranslatorService();
            string schemaPath = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory, "CommandSchema.json");
            svc.LoadSchema(schemaPath);
            return svc;
        });

        // ── ViewModels ─────────────────────────────────────────────────────
        // Singleton: hay una sola ventana principal, un solo estado.
        services.AddSingleton<MainWindowViewModel>();

        return services.BuildServiceProvider();
    }
}