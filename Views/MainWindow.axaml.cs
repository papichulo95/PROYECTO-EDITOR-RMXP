using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using PokemonEssentialsEditorEvs.ViewModels;
using System;

namespace PokemonEssentialsEditorEvs.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    private async void SelectProjectFolder_Click(object? sender, RoutedEventArgs e)
    {
        // Abre el diálogo nativo del sistema operativo
        var result = await StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
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