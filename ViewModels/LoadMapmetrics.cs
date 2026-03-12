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
}