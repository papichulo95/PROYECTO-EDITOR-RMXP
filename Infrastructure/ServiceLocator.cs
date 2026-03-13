using System;
using Microsoft.Extensions.DependencyInjection;

namespace PokemonEssentialsEditorEvs.Infrastructure;

/// <summary>
/// Punto de acceso al contenedor de DI. Solo App.axaml.cs construye el contenedor.
/// El resto del código solo consume — nunca crea con "new".
/// </summary>
public static class ServiceLocator
{
    private static IServiceProvider? _provider;

    /// <summary>
    /// Inicializa el locator. Llamar una sola vez desde App.axaml.cs.
    /// </summary>
    public static void Initialize(IServiceProvider provider)
    {
        if (_provider is not null)
            throw new InvalidOperationException("ServiceLocator ya fue inicializado.");
        _provider = provider;
    }

    /// <summary>
    /// Resuelve un servicio registrado. Lanza si no fue inicializado.
    /// </summary>
    public static T Get<T>() where T : notnull
    {
        if (_provider is null)
            throw new InvalidOperationException(
                "ServiceLocator no fue inicializado. Llama Initialize() desde App.axaml.cs.");
        return _provider.GetRequiredService<T>();
    }

    /// <summary>
    /// Versión nullable — para casos donde el servicio puede no existir.
    /// </summary>
    public static T? TryGet<T>() where T : class
        => _provider?.GetService<T>();
}
