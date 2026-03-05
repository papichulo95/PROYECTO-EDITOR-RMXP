using System.IO;
using System.Text.Json;
using System.Diagnostics;
using System.Collections.ObjectModel;
using System.Linq;
using System.Collections.Generic;
using PokemonEssentialsEditorEvs.Models;

namespace PokemonEssentialsEditorEvs.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    
    public MapExportData? CurrentMap { get; set; }


    public ObservableCollection<MapEventData> MapEvents { get; set; } = new();


    // Propiedades para acceder a la anchura y altura del mapa
    public int MapWidth
    {
        get
        {
            if (CurrentMap != null)
            {
                return CurrentMap.Width * 32; // Convertir de tiles a píxeles (asumiendo que cada tile es de 32 píxeles)
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
            if (CurrentMap != null)
            {
                return CurrentMap.Height * 32; // Convertir de tiles a píxeles (asumiendo que cada tile es de 32 píxeles)
            }
            else
            {
                return 0; // Valor predeterminado si no hay mapa cargado
            }
        }
    }
    public MainWindowViewModel()
    {
        LoadMapJson("Map002_export.json"); // Cambia esto por tu ruta real
    }

    private void LoadMapJson(string filePath)
    {
        if (File.Exists(filePath))
        {
            string jsonString = File.ReadAllText(filePath);
            CurrentMap = JsonSerializer.Deserialize<MapExportData>(jsonString);
            
            if (CurrentMap != null && CurrentMap.Events != null)
            {
                // Cargar los eventos del mapa en la colección ObservableCollection
                MapEvents.Clear();
                foreach (var eventData in CurrentMap.Events.Values)
                {
                    MapEvents.Add(eventData);
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
