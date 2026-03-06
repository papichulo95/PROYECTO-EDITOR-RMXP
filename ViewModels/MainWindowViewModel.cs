using System.IO;
using System.Text.Json;
using System.Diagnostics;
using System.Collections.ObjectModel;
using System.Linq;
using System.Collections.Generic;
using PokemonEssentialsEditorEvs.Models;
using PokemonEssentialsEditorEvs.Tools;
using Avalonia.Media.Imaging;

namespace PokemonEssentialsEditorEvs.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    private  TranslatorCommands _translator;

    private bool _isProjectLoaded;
        public bool IsProjectLoaded 
        { 
            get => _isProjectLoaded; 


            set { _isProjectLoaded = value; 
            OnPropertyChanged(nameof(IsProjectLoaded)); 
            OnPropertyChanged(nameof(IsMapEditorVisible));} 
        }

        private string _projectPath = "";
        public string ProjectPath 
        { 
            get => _projectPath; 
            set { _projectPath = value; OnPropertyChanged(nameof(ProjectPath)); } 
        }
    public ObservableCollection<UICommand> CurrentPageCommands { get; set; } = new();
    public MapExportData? CurrentMap { get; set; }


    public ObservableCollection<MapEventData> MapEvents { get; set; } = new();


    // Propiedades para acceder a la anchura y altura del mapa
    public int MapWidth
    {
        get
        {
            if (CurrentMapMetrics != null)
            {
                return CurrentMapMetrics.Width * 32; // Convertir de tiles a píxeles (asumiendo que cada tile es de 32 píxeles)
            }
            else
            {
                return 0; // Valor predeterminado si no hay mapa cargado
            }
        }
    }
    public int MapHeight
    {
        get
        {
            if (CurrentMapMetrics != null)
            {
                return CurrentMapMetrics.Height * 32; // Convertir de tiles a píxeles (asumiendo que cada tile es de 32 píxeles)
            }
            else
            {
                return 0; // Valor predeterminado si no hay mapa cargado
            }
        }
    }
    public MainWindowViewModel()

    {   var dict = new CommandsDictionary();
        dict.LoadDictionary(@"C:\Users\DELL\Documents\GitHub\PROYECTO-EDITOR-EVENTOS-RMXP\PokemonEssentialsEditorEvs\CommandSchema.json");
        _translator = new TranslatorCommands(dict);

        IsProjectLoaded = false;
    }
        private MapMetricsData? _currentMapMetrics;
        public MapMetricsData? CurrentMapMetrics 
        { 
            get => _currentMapMetrics; 
            set { _currentMapMetrics = value; OnPropertyChanged(nameof(CurrentMapMetrics)); OnPropertyChanged(nameof(MapWidth)); OnPropertyChanged(nameof(MapHeight)); } 
        }
        private Bitmap? _tilesetImage;
        public Bitmap? TilesetImage 
        { 
            get => _tilesetImage; 
            set { _tilesetImage = value; OnPropertyChanged(nameof(TilesetImage)); } 
        }
        public SystemExportData? SystemData { get; set; }
    public void LoadProject(string folderPath)
        {
            ProjectPath = folderPath;
            string exportFolder = Path.Combine(folderPath, "Data", "ExportMaps");

            if (Directory.Exists(exportFolder))
            {   
                
                // 1. Cargar System_export.json
                string systemJsonPath = Path.Combine(exportFolder, "System_export.json");
                if (File.Exists(systemJsonPath))
                {
                    string sysJson = File.ReadAllText(systemJsonPath);
                    SystemData = JsonSerializer.Deserialize<SystemExportData>(sysJson);
                }

                // Buscamos la primera carpeta de mapa disponible (ej. Map002)
                var mapFolders = Directory.GetDirectories(exportFolder);
                if (mapFolders.Length > 0)
                {
                    string firstMapFolder = mapFolders[0];
                    string mapName = new DirectoryInfo(firstMapFolder).Name; // "Map002"
                    string eventsJsonPath = Path.Combine(firstMapFolder, $"{mapName}_Events_export.json");

                    LoadMapJson(eventsJsonPath);

                    string metricsJsonPath = Path.Combine(firstMapFolder, $"{mapName}_mapmetrics_export.json");
                    LoadMapMetricsJson(metricsJsonPath);
                    IsProjectLoaded = true;
                }
            }
        }

    private void LoadMapJson(string filePath)
    {
        if (File.Exists(filePath))
        {
            string jsonString = File.ReadAllText(filePath);
            // Deserializamos usando el Wrapper
            var wrapper = JsonSerializer.Deserialize<MapEventsWrapper>(jsonString);
            
            CurrentMap = wrapper?.Data;
            
            if (CurrentMap != null && CurrentMap.Events != null)
            {
                MapEvents.Clear();
                foreach (var eventData in CurrentMap.Events.Values)
                {
                    MapEvents.Add(eventData);
                }
                OnPropertyChanged(nameof(MapWidth));
                OnPropertyChanged(nameof(MapHeight));
            }
            //int p = 0;
            //foreach (var page in  eventData.Pages) { page.Title = $"Página {p++}"; }
        }
    }

    private void LoadMapMetricsJson(string filePath)
    {
        if (File.Exists(filePath))
            {
                string jsonString = File.ReadAllText(filePath);
                CurrentMapMetrics = JsonSerializer.Deserialize<MapMetricsData>(jsonString);

                OnPropertyChanged(nameof(MapWidth));
                OnPropertyChanged(nameof(MapHeight));

                // Si cargamos las métricas y tenemos el SystemData, intentamos cargar la imagen del tileset
                if (CurrentMapMetrics != null && SystemData?.Tilesets != null)
                {
                    string tilesetIdStr = CurrentMapMetrics.TilesetId.ToString();
                    if (SystemData.Tilesets.TryGetValue(tilesetIdStr, out var tilesetData))
                    {
                        string imageName = tilesetData.TilesetName ?? "";
                        // Construimos la ruta a la imagen: Proyecto/Graphics/Tilesets/NombreImagen.png
                        // Nota: RMXP a veces no guarda la extensión en el nombre, así que asumimos .png
                        string imagePath = Path.Combine(ProjectPath, "Graphics", "Tilesets", imageName + ".png");
                        
                        if (File.Exists(imagePath))
                        {
                            TilesetImage = new Bitmap(imagePath);
                            Debug.WriteLine($"Imagen del tileset cargada: {imagePath}");
                        }
                        else
                        {
                            Debug.WriteLine($"No se encontró la imagen del tileset: {imagePath}");
                        }
                    }
                }
            }
    }


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
    private void UpdateCommandsList()
    {
        CurrentPageCommands.Clear();
        
        if (SelectedPage?.List != null)
        {
            var page = SelectedPage.List;

            if (page != null)
            {
                foreach (var cmd in page)
                {
                    var schema = _translator.GetSchema(cmd.Code);
                    string text = _translator.TranslateCommandToUI(cmd.Code, cmd.Parameters);
                    string color = schema?.Color ?? "#3E3E42"; // Gris si es desconocido

                    CurrentPageCommands.Add(new UICommand 
                    { 
                        DisplayText = text, 
                        Color = color, 
                        Indent = cmd.Indent 
                    });
                }
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
