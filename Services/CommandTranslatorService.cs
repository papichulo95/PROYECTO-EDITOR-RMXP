using System;
using System.IO;
using System.Collections.ObjectModel;
using System.Diagnostics;
using PokemonEssentialsEditorEvs.Models;
using PokemonEssentialsEditorEvs.Tools;

namespace PokemonEssentialsEditorEvs.Services
{
    // Esta clase "implementa" (firma) el contrato ICommandTranslatorService
    public class CommandTranslatorService : ICommandTranslatorService
    {
        private readonly CommandsDictionary _dictionary;
        private readonly TranslatorCommands _translator;

        public CommandTranslatorService()
        {
            _dictionary = new CommandsDictionary();
            _translator = new TranslatorCommands(_dictionary);

            // Carga automática: el .csproj ya copia CommandSchema.json al lado del ejecutable
            string schemaPath = Path.Combine(AppContext.BaseDirectory, "CommandSchema.json");
            Debug.WriteLine($"[Schema] Buscando en: {schemaPath}");
            Debug.WriteLine($"[Schema] Existe: {File.Exists(schemaPath)}");

            if (File.Exists(schemaPath))
            {
                _dictionary.LoadDictionary(schemaPath);
                Debug.WriteLine("[Schema] Cargado OK");
            }
            else
            {
                Debug.WriteLine("[Schema] ARCHIVO NO ENCONTRADO — los comandos no se traducirán");
            }
        }

        public void LoadSchema(string jsonFilePath)
        {
            _dictionary.LoadDictionary(jsonFilePath);
        }

        // Aquí metemos toda la lógica pesada que antes ensuciaba el ViewModel
        public ObservableCollection<UICommand> TranslatePage(EventPageData? page)
        {
            var result = new ObservableCollection<UICommand>();

            if (page?.List == null)
            {
                Debug.WriteLine("[Translator] TranslatePage: page o page.List es null");
                return result;
            }

            Debug.WriteLine($"[Translator] Traduciendo {page.List.Count} comandos");

            foreach (var cmd in page.List)
            {
                var schema = _translator.GetSchema(cmd.Code);
                string text  = _translator.TranslateCommandToUI(cmd.Code, cmd.Parameters);
                string color = schema?.Color ?? "#3E3E42";

                result.Add(new UICommand
                {
                    Code        = cmd.Code,
                    DisplayText = text,
                    Color       = color,
                    Indent      = cmd.Indent
                });
            }

            Debug.WriteLine($"[Translator] Resultado: {result.Count} UICommands generados");
            return result;
        }
    }
}