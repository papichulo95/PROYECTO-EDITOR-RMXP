using System.IO;
using System.Text.Json;
using Avalonia.Media.Imaging;
using PokemonEssentialsEditorEvs.Models;
using System.Diagnostics;

namespace PokemonEssentialsEditorEvs.Services
{
    public class ProjectLoaderService : IProjectLoaderService
    {
        public SystemExportData? LoadSystemData(string projectPath)
        {
            string systemJsonPath = Path.Combine(projectPath, "Data", "ExportMaps", "System_export.json");
            if (File.Exists(systemJsonPath))
            {
                string json = File.ReadAllText(systemJsonPath);
                return JsonSerializer.Deserialize<SystemExportData>(json);
            }
            return null;
        }

        public MapExportData? LoadMapEvents(string projectPath, string mapFolderName)
        {
            string eventsJsonPath = Path.Combine(projectPath, "Data", "ExportMaps", mapFolderName, $"{mapFolderName}_Events_export.json");
            if (File.Exists(eventsJsonPath))
            {
                string jsonString = File.ReadAllText(eventsJsonPath);
                var wrapper = JsonSerializer.Deserialize<MapEventsWrapper>(jsonString);
                return wrapper?.Data;
            }
            return null;
        }

        public MapMetricsData? LoadMapMetrics(string projectPath, string mapFolderName)
        {
            string metricsJsonPath = Path.Combine(projectPath, "Data", "ExportMaps", mapFolderName, $"{mapFolderName}_mapmetrics_export.json");
            if (File.Exists(metricsJsonPath))
            {
                string jsonString = File.ReadAllText(metricsJsonPath);
                return JsonSerializer.Deserialize<MapMetricsData>(jsonString);
            }
            return null;
        }

        public Bitmap? LoadTilesetImage(string projectPath, string imageName)
        {
            if (string.IsNullOrEmpty(imageName)) return null;

            string imagePath = Path.Combine(projectPath, "Graphics", "Tilesets", imageName + ".png");
            
            if (File.Exists(imagePath))
            {
                // Carga limpia en memoria para no bloquear el archivo
                using (var stream = new FileStream(imagePath, FileMode.Open, FileAccess.Read))
                {
                    using (var memoryStream = new MemoryStream())
                    {
                        stream.CopyTo(memoryStream);
                        memoryStream.Position = 0;
                        Debug.WriteLine($"Imagen del tileset cargada: {imagePath}");
                        return new Bitmap(memoryStream);
                    }
                }
            }
            
            Debug.WriteLine($"No se encontró la imagen del tileset: {imagePath}");
            return null;
        }
    }
}