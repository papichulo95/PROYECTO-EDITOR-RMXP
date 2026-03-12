using Avalonia.Media.Imaging;
using PokemonEssentialsEditorEvs.Models;

namespace PokemonEssentialsEditorEvs.Services
{
    // El contrato para cargar proyectos y mapas
    public interface IProjectLoaderService
    {
        // Carga el System_export.json
        SystemExportData? LoadSystemData(string projectPath);
        
        // Carga el MapXXX_Events_export.json
        MapExportData? LoadMapEvents(string projectPath, string mapFolderName);
        
        // Carga el MapXXX_mapmetrics_export.json
        MapMetricsData? LoadMapMetrics(string projectPath, string mapFolderName);
        
        // Carga la imagen del tileset de forma segura (sin bloquear el archivo)
        Bitmap? LoadTilesetImage(string projectPath, string imageName);
    }
}