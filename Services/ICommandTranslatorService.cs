using System.Collections.ObjectModel;
using PokemonEssentialsEditorEvs.Models;
using PokemonEssentialsEditorEvs.Tools;

namespace PokemonEssentialsEditorEvs.Services
{

    public interface ICommandTranslatorService
    {
        void LoadSchema(string jsonFilePath);
            ObservableCollection<UICommand> TranslatePage(EventPageData? page);
    }
}
