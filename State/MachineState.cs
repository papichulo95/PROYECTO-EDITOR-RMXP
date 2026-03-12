using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace PokemonEssentialsEditorEvs.State;

/// <summary>
/// Los estados posibles del editor. Cada uno representa una "escena" distinta.
/// </summary>
public enum EditorState
{
    NoProject,      // Pantalla de bienvenida, esperando que se abra un proyecto
    Loading,        // Cargando archivos del disco (futuro: mostrar spinner)
    MapOverview,    // Mapa visible con lista de eventos
    EditingEvent,   // Pantalla completa de edición de comandos de un evento
    Error           // Algo salió mal
}

/// <summary>
/// Máquina de estados del editor. Es la única fuente de verdad sobre
/// "en qué pantalla estamos". Los ViewModels reaccionan a ella, nunca la ignoran.
/// </summary>
public class EditorStateMachine : INotifyPropertyChanged
{
    // ── Singleton accesible desde cualquier parte ──────────────────────────
    public static EditorStateMachine Instance { get; } = new();

    // ── Estado actual ──────────────────────────────────────────────────────
    private EditorState _current = EditorState.NoProject;
    public EditorState Current
    {
        get => _current;
        private set
        {
            if (_current == value) return;
            var previous = _current;
            _current = value;
            OnPropertyChanged();
            StateChanged?.Invoke(this, new StateChangedEventArgs(previous, value));

            // Notificamos las propiedades derivadas de una sola vez
            OnPropertyChanged(nameof(IsNoProject));
            OnPropertyChanged(nameof(IsLoading));
            OnPropertyChanged(nameof(IsMapOverview));
            OnPropertyChanged(nameof(IsEditingEvent));
            OnPropertyChanged(nameof(IsMapEditorVisible));
        }
    }

    // ── Propiedades derivadas (para bindings en la UI) ─────────────────────
    public bool IsNoProject     => Current == EditorState.NoProject;
    public bool IsLoading       => Current == EditorState.Loading;
    public bool IsMapOverview   => Current == EditorState.MapOverview;
    public bool IsEditingEvent  => Current == EditorState.EditingEvent;

    /// <summary>
    /// El mapa y sus controles solo son visibles cuando hay proyecto cargado
    /// y NO estamos en la pantalla de edición de comandos.
    /// </summary>
    public bool IsMapEditorVisible => Current == EditorState.MapOverview;

    // ── Evento para quien quiera suscribirse a los cambios ─────────────────
    public event EventHandler<StateChangedEventArgs>? StateChanged;

    // ── Transiciones explícitas (las únicas formas válidas de cambiar estado)
    public void StartLoading()   => Transition(EditorState.Loading,      EditorState.NoProject, EditorState.MapOverview);
    public void ProjectLoaded()  => Transition(EditorState.MapOverview,  EditorState.Loading);
    public void OpenEditor()     => Transition(EditorState.EditingEvent, EditorState.MapOverview);
    public void CloseEditor()    => Transition(EditorState.MapOverview,  EditorState.EditingEvent);
    public void UnloadProject()  => Transition(EditorState.NoProject,    EditorState.MapOverview, EditorState.Error);
    public void SetError()       => Current = EditorState.Error;

    // ── Lógica de transición con validación ───────────────────────────────
    private void Transition(EditorState next, params EditorState[] validFrom)
    {
        foreach (var valid in validFrom)
        {
            if (Current == valid)
            {
                Current = next;
                return;
            }
        }
        // Transición inválida: la ignoramos en Release, lanzamos en Debug
#if DEBUG
        throw new InvalidOperationException(
            $"Transición inválida: {Current} → {next}. " +
            $"Permitido solo desde: [{string.Join(", ", validFrom)}]");
#endif
    }

    // ── INotifyPropertyChanged ─────────────────────────────────────────────
    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}

/// <summary>
/// Datos del evento de cambio de estado.
/// </summary>
public class StateChangedEventArgs : EventArgs
{
    public EditorState Previous { get; }
    public EditorState Next { get; }

    public StateChangedEventArgs(EditorState previous, EditorState next)
    {
        Previous = previous;
        Next = next;
    }
}