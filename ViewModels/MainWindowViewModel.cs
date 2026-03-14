using System;
using System.IO;
using System.Text.Json;
using System.Linq;
using System.Collections.Generic;
using PokemonEssentialsEditorEvs.Models;
using PokemonEssentialsEditorEvs.Services;
using PokemonEssentialsEditorEvs.State;
using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.Input;

namespace PokemonEssentialsEditorEvs.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    // ── Dependencias inyectadas (nunca construidas con "new" aquí) ──────────
    private readonly ICommandTranslatorService _commandTranslator;
    private readonly IProjectLoaderService _projectLoader;

    /// <summary>
    /// La máquina de estados es la única fuente de verdad sobre
    /// qué pantalla está activa. El ViewModel la expone para los bindings.
    /// </summary>
    public EditorStateMachine State { get; }

    // ── Constructor con inyección de dependencias ───────────────────────────
    public MainWindowViewModel(
        ICommandTranslatorService commandTranslator,
        IProjectLoaderService projectLoader,
        EditorStateMachine state)
    {
        _commandTranslator = commandTranslator;
        _projectLoader     = projectLoader;
        State              = state;
    }

    // ── Comandos (CommunityToolkit genera ICommand automáticamente) ─────────
    [RelayCommand]
    private void OpenEventEditor()
    {
        if (SelectedEvent is not null)
            State.OpenEditor();
    }

    [RelayCommand]
    private void CloseEventEditor() => State.CloseEditor();

    [RelayCommand]
    private void CreateNewEvent()
    {
        if (CurrentMap?.Events is null) return;

        int newId = CurrentMap.Events.Count > 0
            ? CurrentMap.Events.Values.Max(e => e.Id) + 1
            : 1;

        var newEvent = new MapEventData
        {
            Id   = newId,
            Name = $"EV{newId:D3}",
            X    = 0,
            Y    = 0,
            Pages = new List<EventPageData>
            {
                new()
                {
                    Condition      = new EventConditionData(),
                    MoveSpeed      = 3,
                    MoveFrequency  = 3,
                    WalkAnime      = true,
                    Trigger        = 0
                }
            }
        };

        CurrentMap.Events.Add(newId.ToString(), newEvent);
        MapEvents.Add(newEvent);
        SelectedEvent = newEvent;
    }

    [RelayCommand]
    private void SaveMap()
    {
        if (CurrentMap is null || string.IsNullOrEmpty(ProjectPath)) return;

        // Guardamos en la ruta real del proyecto, no en el directorio de ejecución
        string exportFolder = Path.Combine(
            ProjectPath, "Data", "ExportMaps",
            $"Map{CurrentMap.MapId:D3}");

        string outputPath = Path.Combine(
            exportFolder,
            $"Map{CurrentMap.MapId:D3}_Events_export.json");

        var wrapper = new MapEventsWrapper { Data = CurrentMap };
        var options = new JsonSerializerOptions { WriteIndented = true };
        File.WriteAllText(outputPath, JsonSerializer.Serialize(wrapper, options));
    }

    // ── Método de carga del proyecto ────────────────────────────────────────
    public void LoadProject(string folderPath)
    {
        State.StartLoading();

        ProjectPath   = folderPath;
        string exportFolder = Path.Combine(folderPath, "Data", "ExportMaps");

        if (!Directory.Exists(exportFolder))
        {
            State.SetError();
            return;
        }

        var mapFolders = Directory.GetDirectories(exportFolder);
        if (mapFolders.Length == 0)
        {
            State.SetError();
            return;
        }

        SystemData = _projectLoader.LoadSystemData(folderPath);

        string mapFolderName = new DirectoryInfo(mapFolders[0]).Name;

        CurrentMap = _projectLoader.LoadMapEvents(folderPath, mapFolderName);
        if (CurrentMap?.Events is not null)
        {
            MapEvents.Clear();

            var characterCache = new Dictionary<string, Bitmap>();

            foreach (var ev in CurrentMap.Events.Values)
            {
                // 1. Verificamos si el evento tiene páginas y gráficos
                if (ev.Pages != null && ev.Pages.Count > 0)
                {
                    var graphicData = ev.Pages[0].Graphic; // Leemos el gráfico de la página 1
                    
                    if (graphicData != null && !string.IsNullOrEmpty(graphicData.CharacterName))
                    {
                        string charName = graphicData.CharacterName;
                        
                        // 2. Cargamos la imagen (usando caché para ser súper eficientes)
                        if (!characterCache.ContainsKey(charName))
                        {
                            var loadedBmp = _projectLoader.LoadCharacterImage(folderPath, charName);
                            if (loadedBmp != null) characterCache[charName] = loadedBmp;
                        }

                        if (characterCache.TryGetValue(charName, out Bitmap? fullBitmap))
                        {
                            // 3. MATEMÁTICA DE RMXP: Recortar el frame correcto
                            // Una hoja de personaje tiene 4 columnas (patrones) y 4 filas (direcciones)
                            int frameWidth = fullBitmap.PixelSize.Width / 4;
                            int frameHeight = fullBitmap.PixelSize.Height / 4;

                            // RMXP Direcciones: 2=Abajo(Fila 0), 4=Izq(Fila 1), 6=Der(Fila 2), 8=Arriba(Fila 3)
                            int row = graphicData.DirectionEvent switch {
                                2 => 0, 4 => 1, 6 => 2, 8 => 3, _ => 0
                            };
                            
                            int col = graphicData.PatternEvent; // Usualmente 0, 1, 2 o 3

                            // 4. Creamos el recorte y se lo asignamos al evento
                            var sourceRect = new Avalonia.PixelRect(col * frameWidth, row * frameHeight, frameWidth, frameHeight);
                            ev.DisplayGraphic = new CroppedBitmap(fullBitmap, sourceRect);
                        }
                    }
                }
                
                MapEvents.Add(ev);
            }
        }

        CurrentMapMetrics = _projectLoader.LoadMapMetrics(folderPath, mapFolderName);

        if (CurrentMapMetrics is not null && SystemData?.Tilesets is not null)
        {
            string idStr = CurrentMapMetrics.TilesetId.ToString();
            if (SystemData.Tilesets.TryGetValue(idStr, out var tilesetData))
            {
                TilesetImage = _projectLoader.LoadTilesetImage(
                    folderPath, tilesetData.TilesetName ?? "");
                Autotiles = _projectLoader.LoadAutotiles(folderPath, tilesetData?.AutotileNames ?? new List<string> ());
            }
                
        }

        State.ProjectLoaded();
    }

    // ── Actualización de la lista de comandos ───────────────────────────────
    private void UpdateCommandsList()
    {
        CurrentPageCommands.Clear();
        foreach (var cmd in _commandTranslator.TranslatePage(SelectedPage))
            CurrentPageCommands.Add(cmd);
    }
}