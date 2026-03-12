using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using PokemonEssentialsEditorEvs.ViewModels;
using System;

// 1. El namespace debe coincidir con el x:Class de tu archivo .axaml
namespace PokemonEssentialsEditorEvs.Views;

// 2. Cambiamos el nombre de la clase y heredamos de UserControl
public partial class ProjectView : UserControl
{
    // 3. Añadimos el constructor obligatorio
    public ProjectView()
    {
        InitializeComponent();
    }

    private async void SelectProjectFolder_Click(object? sender, RoutedEventArgs e)
    {
        // 4. MAGIA SENIOR: Obtenemos la ventana principal que contiene a este UserControl
        var topLevel = TopLevel.GetTopLevel(this);
        if (topLevel == null) return;

        // Ahora usamos el StorageProvider del topLevel
        var result = await topLevel.StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
        {
            Title = "Selecciona la carpeta raíz de tu proyecto Pokémon Essentials",
            AllowMultiple = false
        });

        if (result.Count > 0)
        {
            string folderPath = result[0].Path.LocalPath;
            
            // Le pasamos la ruta al ViewModel
            if (DataContext is MainWindowViewModel vm)
            {
                vm.LoadProject(folderPath);
            }
        }
    }
}