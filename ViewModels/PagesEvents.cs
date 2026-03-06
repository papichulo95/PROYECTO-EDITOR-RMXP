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
    private bool _isEditingEvent;
        public bool IsEditingEvent 
        { 
            get => _isEditingEvent; 
            set 
            { 
                _isEditingEvent = value; 
                OnPropertyChanged(nameof(IsEditingEvent)); 
                OnPropertyChanged(nameof(IsMapEditorVisible)); // Avisa que la visibilidad del mapa cambió
            } 
        }

        // El mapa solo se ve si el proyecto cargó Y NO estamos editando un evento
        public bool IsMapEditorVisible => IsProjectLoaded && !IsEditingEvent;

        // Método para entrar a la escena del evento
        public void OpenEventEditor()
        {
            if (SelectedEvent != null)
            {
                IsEditingEvent = true;
            }
        }

        // Método para volver al mapa
        public void CloseEventEditor()
        {
            IsEditingEvent = false;
        }
}