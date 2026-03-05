using System.IO;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using PokemonEssentialsEditorEvs.Models;

namespace PokemonEssentialsEditorEvs.Tools
{
    // 1. Modelo para deserializar el esquema (la base de datos)
    public class CommandSchema
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = "Desconocido";

        [JsonPropertyName("category")]
        public string Category { get; set; } = "General";

        [JsonPropertyName("color")]
        public string Color { get; set; } = "#808080";
        
        [JsonPropertyName("parameters_schema")]
        public List<string> ParametersSchema { get; set; } = new();
    }

    // 2. El Diccionario que carga la base de datos en memoria
    public class CommandsDictionary
    {
        private Dictionary<string, CommandSchema> _schemas = new();

        public void LoadDictionary(string jsonFilePath)
        {
            if (File.Exists(jsonFilePath))
            {
                string json = File.ReadAllText(jsonFilePath);
                _schemas = JsonSerializer.Deserialize<Dictionary<string, CommandSchema>>(json) ?? new();
            }
        }

        public CommandSchema? GetSchema(int code)
        {
            string key = code.ToString();
            return _schemas.ContainsKey(key) ? _schemas[key] : null;
        }
    }

    // 3. El Traductor que usa el diccionario para formatear un comando real del mapa
    public class TranslatorCommands
    {
        private readonly CommandsDictionary _dictionary;

        public TranslatorCommands(CommandsDictionary dictionary)
        {
            _dictionary = dictionary;
        }
        public CommandSchema? GetSchema(int code) => _dictionary.GetSchema(code);
        // Este método tomará el "code" y los "parameters" crudos de tu MapExportData
        // y devolverá un objeto amigable para la UI.
        public string TranslateCommandToUI(int code, JsonElement parameters)
        {
            var schema = _dictionary.GetSchema(code);
            
            if (schema == null)
                return $"Comando Desconocido ({code})";

            // Ejemplo básico de traducción:
            if ((code == 101 || code == 401) && parameters.ValueKind == JsonValueKind.Array && parameters.GetArrayLength() > 0)
            {
                string texto = parameters[0].GetString() ?? "";
                return $"{schema.Name}: \"{texto}\"";
            }

            return schema.Name;
        }
    }

    public class UICommand
    {
        public string DisplayText { get; set; } = "";
        public string Color { get; set; } = "#808080";
        public int Indent { get; set; }
        public Avalonia.Thickness Margin => new Avalonia.Thickness(Indent * 20, 2, 0, 2);
    }
}
