using System.Collections.ObjectModel;
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
        }

        public void LoadSchema(string jsonFilePath)
        {
            _dictionary.LoadDictionary(jsonFilePath);
        }

        // Aquí metemos toda la lógica pesada que antes ensuciaba el ViewModel
        public ObservableCollection<UICommand> TranslatePage(EventPageData? page)
        {
            var translatedCommands = new ObservableCollection<UICommand>();

            if (page?.List != null)
            {
                foreach (var cmd in page.List)
                {
                    var schema = _translator.GetSchema(cmd.Code);
                    string text = _translator.TranslateCommandToUI(cmd.Code, cmd.Parameters);
                    string color = schema?.Color ?? "#3E3E42";

                    translatedCommands.Add(new UICommand 
                    { 
                        DisplayText = text, 
                        Color = color, 
                        Indent = cmd.Indent 
                    });
                }
            }

            return translatedCommands;
        }
    }
}