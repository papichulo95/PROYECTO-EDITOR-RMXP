using System.IO;
using System.Text.Json;
using System.Diagnostics;
using System.Collections.ObjectModel;
using System.Linq;
using System.Collections.Generic;
using PokemonEssentialsEditorEvs.Models;
using PokemonEssentialsEditorEvs.Tools;
using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;

namespace PokemonEssentialsEditorEvs.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
// ── Propiedades de Estado y Rutas ───────────────────────────────────────

    [ObservableProperty]
    private string _projectPath = "";


    // ── Datos del Mapa ──────────────────────────────────────────────────────
    public SystemExportData? SystemData { get; set; }
    public MapExportData? CurrentMap { get; set; }
    
    private MapMetricsData? _currentMapMetrics;
    public MapMetricsData? CurrentMapMetrics 
    { 
        get => _currentMapMetrics; 
        set 
        { 
            _currentMapMetrics = value; 
            OnPropertyChanged(nameof(CurrentMapMetrics)); 
            OnPropertyChanged(nameof(MapWidth)); 
            OnPropertyChanged(nameof(MapHeight)); 
        } 
    }

    private Bitmap? _tilesetImage;
    public Bitmap? TilesetImage 
    { 
        get => _tilesetImage; 
        set { _tilesetImage = value; OnPropertyChanged(nameof(TilesetImage)); } 
    }

    public int MapWidth => (CurrentMapMetrics?.Width ?? 0) * 32;
    public int MapHeight => (CurrentMapMetrics?.Height ?? 0) * 32;

    // ── Colecciones Visuales ────────────────────────────────────────────────
    public ObservableCollection<MapEventData> MapEvents { get; set; } = new();
    public ObservableCollection<UICommand> CurrentPageCommands { get; set; } = new();

    // ── Selección ───────────────────────────────────────────────────────────
    private MapEventData? _selectedEvent;
    public MapEventData? SelectedEvent
    {
        get => _selectedEvent;
        set
        {
            if (_selectedEvent != value)
            {
                _selectedEvent = value;
                OnPropertyChanged(nameof(SelectedEvent)); 
                
                // Al cambiar de evento, seleccionamos su primera página por defecto
                SelectedPage = _selectedEvent?.Pages?.FirstOrDefault();
                UpdateCommandsList();
            }
        }
    }

    private List<Bitmap?> _autotiles = new();
    public List<Bitmap?> Autotiles 
    { 
        get => _autotiles; 
        set { _autotiles = value; OnPropertyChanged(nameof(Autotiles)); } 
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
                
                // Al cambiar de página, actualizamos la lista de comandos visuales
                UpdateCommandsList(); 
            }
        }
    }
}