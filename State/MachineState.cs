namespace PokemonEssentialsEditorEvs.State;

public enum EditorState
{
    NoProject,      // Estado inicial
    Loading,        // Mientras se procesan archivos
    MapOverview,    // Viendo el mapa y lista de eventos
    EditingEvent,   // Dentro de la "escena" de edición de comandos
    Error           // Estado de fallo
}