using System.Collections.Generic;
using System.Text.Json.Serialization;
using System.ComponentModel;
using System.Runtime.CompilerServices;
namespace PokemonEssentialsEditorEvs.Models
{   
    public class MapEventsWrapper
    {
        [JsonPropertyName("data")]
        public MapExportData? Data { get; set; }[JsonPropertyName("checksum")]
        public string? Checksum { get; set; }
    }
    public class MapExportData
    {
        [JsonPropertyName("map_id")]
        public int MapId { get; set; }
        
        [JsonPropertyName("width")]
        public int Width { get; set; }

        [JsonPropertyName("height")]
        public int Height { get; set; }

        [JsonPropertyName("tileset_id")]
        public int TilesetId { get; set; }

        [JsonPropertyName("bgm")]
        public string? Bgm { get; set; }

        [JsonPropertyName("bgs")]
        public string? Bgs { get; set; }

        [JsonPropertyName("events")]
        public Dictionary<string, MapEventData>? Events { get; set; }
    }

    // Cargar Tilesets data

    public class SystemExportData
    {
        [JsonPropertyName("tilesets")]
        public Dictionary<string, TilesetData>? Tilesets { get; set; }
    }

    public class TilesetData
    {[JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("tileset_name")]
        public string? TilesetName { get; set; }

        [JsonPropertyName("autotile_name")]


        // CARGAR AUTOTILES
        public List<string>? AutotileNames { get; set; }
    };

    



    // CARGAR METRICS
    public class MapMetricsData
    {[JsonPropertyName("map_id")]
        public int MapId { get; set; }

        [JsonPropertyName("width")]
        public int Width { get; set; }

        [JsonPropertyName("height")]
        public int Height { get; set; }

        [JsonPropertyName("tileset_id")]
        public int TilesetId { get; set; }

        [JsonPropertyName("tile_data")]
        public int[][][]? TileData { get; set; }
    }



    public class MapEventData : INotifyPropertyChanged
    {   public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }



        [JsonPropertyName("id")]
        public int Id { get; set; }

        private string? _name;[JsonPropertyName("name")]
        public string? Name
        {
            get => _name;
            set { if (_name != value) { _name = value; OnPropertyChanged(); } }
        }
        

        private int _x;[JsonPropertyName("x")]
        public int X
        {
            get => _x;
            set 
            { 
                if (_x != value) 
                { 
                    _x = value; 
                    OnPropertyChanged(); // Avisa que cambió X
                    OnPropertyChanged(nameof(PixelX)); // Avisa que también cambió PixelX
                } 
            }
        }

        private int _y;[JsonPropertyName("y")]
        public int Y
        {
            get => _y;
            set 
            { 
                if (_y != value) 
                { 
                    _y = value; 
                    OnPropertyChanged(); // Avisa que cambió Y
                    OnPropertyChanged(nameof(PixelY)); // Avisa que también cambió PixelY
                } 
            }
        }

        [JsonIgnore]
        public int PixelX => X * 32; // Asumiendo que cada tile es de 32x32 píxeles
        [JsonIgnore]
        public int PixelY => Y * 32;


    // PAGINAS DE EVENTOS
        private List<EventPageData>? _pages;
        [JsonPropertyName("pages")]
        public List<EventPageData>? Pages
        {
            get => _pages;
            set { if (_pages != value) { _pages = value; OnPropertyChanged(); } }
        }
    }

    
    









}
