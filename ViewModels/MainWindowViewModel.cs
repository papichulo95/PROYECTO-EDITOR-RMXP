using System;
using System.IO;
using System.Text.Json;
using System.Diagnostics;
using System.Collections.ObjectModel;
using System.Linq;
using System.Collections.Generic;
using PokemonEssentialsEditorEvs.Models;
using PokemonEssentialsEditorEvs.Tools;
using Avalonia.Media.Imaging;
using PokemonEssentialsEditorEvs.Services;

namespace PokemonEssentialsEditorEvs.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{


    //
    private readonly ICommandTranslatorService _commandTranslator;
    private readonly IProjectLoaderService _projectLoader;
     // --- ESTADO DE LA APLICACIÓN ---
    private bool _isProjectLoaded;
    public bool IsProjectLoaded 
    { 
        get => _isProjectLoaded; 
        set 
        { 
            _isProjectLoaded = value; 
            OnPropertyChanged(nameof(IsProjectLoaded)); 
            OnPropertyChanged(nameof(IsMapEditorVisible));
        } 
    }




    private string _projectPath = "";
    public string ProjectPath 
    { 
        get => _projectPath; 
        set { _projectPath = value; OnPropertyChanged(nameof(ProjectPath)); } 
    }

    

    public ObservableCollection<UICommand> CurrentPageCommands { get; set; } = new();




    public MainWindowViewModel()

    {   

        _commandTranslator = new CommandTranslatorService();
        _projectLoader = new ProjectLoaderService();
        
        string schemaPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "CommandSchema.json");
        _commandTranslator.LoadSchema(schemaPath); 
            
        
        


        IsProjectLoaded = false;
    }


    public void LoadProject(string folderPath)
    {
        ProjectPath = folderPath;
        string exportFolder = Path.Combine(folderPath, "Data", "ExportMaps");

        if (Directory.Exists(exportFolder))
        {   
            SystemData = _projectLoader.LoadSystemData(folderPath);

            var mapFolders = Directory.GetDirectories(exportFolder);
            if (mapFolders.Length > 0)
            {
                string mapFolderName = new DirectoryInfo(mapFolders[0]).Name; // ej. "Map002"

                // Cargar Eventos
                CurrentMap = _projectLoader.LoadMapEvents(folderPath, mapFolderName);
                if (CurrentMap?.Events != null)
                {
                    MapEvents.Clear();
                    foreach (var eventData in CurrentMap.Events.Values)
                    {
                        MapEvents.Add(eventData);
                    }
                }

                // Cargar Métricas
                CurrentMapMetrics = _projectLoader.LoadMapMetrics(folderPath, mapFolderName);

                // Cargar Imagen del Tileset
                if (CurrentMapMetrics != null && SystemData?.Tilesets != null)
                {
                    string tilesetIdStr = CurrentMapMetrics.TilesetId.ToString();
                    if (SystemData.Tilesets.TryGetValue(tilesetIdStr, out var tilesetData))
                    {
                        TilesetImage = _projectLoader.LoadTilesetImage(folderPath, tilesetData.TilesetName ?? "");
                    }
                }

                IsProjectLoaded = true;
            }
        }
    }
    private void UpdateCommandsList()
    {
        CurrentPageCommands.Clear();
        
        var translated = _commandTranslator.TranslatePage(SelectedPage);
            
        foreach (var cmd in translated)
        {
            CurrentPageCommands.Add(cmd);
        }

    }
        // * Propiedades de metrics de los mapas


    
    

    


    private MapEventData? _selectedEvent;


    public MapEventData? SelectedEvent
    {
        get => _selectedEvent;
        set
        {
            if (_selectedEvent != value)
            {
                _selectedEvent = value;
                // Dependiendo de la plantilla que usaste, esto notifica a la UI del cambio.
                // Si OnPropertyChanged te da error, cámbialo por: SetProperty(ref _selectedEvent, value);
                OnPropertyChanged(nameof(SelectedEvent)); 

                SelectedPage = _selectedEvent?.Pages?.FirstOrDefault();

                UpdateCommandsList();
            }
        }
    }

    private EventPageData? _selectedPage;
        public EventPageData? SelectedPage
        {
            get => _selectedPage;
            set
            {
                if (_selectedPage != value)
                {
                    _selectedPage = value;
                    OnPropertyChanged(nameof(SelectedPage));
                    UpdateCommandsList(); // Actualiza los comandos al cambiar de pestaña
                }
            }
        }



    // Método para crear un nuevo evento
    public void CreateNewEvent()
    {
        if (CurrentMap == null || CurrentMap.Events == null) return;

        // Calculamos el siguiente ID disponible
        int newId = 1;
        if (CurrentMap.Events.Count > 0)
        {
            newId = CurrentMap.Events.Values.Max(e => e.Id) + 1;
        }

        // Creamos la estructura base del evento (como lo hace RMXP por defecto)
        var newEvent = new MapEventData
        {
            Id = newId,
            Name = $"EV{newId:D3}",
            X = 0,
            Y = 0,
            Pages = new List<EventPageData> 
            { 
                new EventPageData 
                { 
                    Condition = new EventConditionData(),
                    MoveSpeed = 3,
                    MoveFrequency = 3,
                    WalkAnime = true,
                    Trigger = 0
                } 
            }
        };

        // Lo añadimos al diccionario interno y a la lista visual
        CurrentMap.Events.Add(newId.ToString(), newEvent);
        MapEvents.Add(newEvent);
        
        // Lo seleccionamos automáticamente para editarlo
        SelectedEvent = newEvent; 
    }

    // Método para guardar los cambios en el JSON
    public void SaveMap()
    {
        if (CurrentMap != null)
        {
            // Opciones para que el JSON quede bonito y legible
            var options = new JsonSerializerOptions { WriteIndented = true };
            string jsonString = JsonSerializer.Serialize(CurrentMap, options);
            
            
            File.WriteAllText("Map002_export.json", jsonString);
        }
    }
}
