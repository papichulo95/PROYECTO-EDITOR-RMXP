using System.Collections.Generic;
using System.Text.Json.Serialization;
using System.Text.Json;
using System.ComponentModel;
using System.Runtime.CompilerServices;



namespace PokemonEssentialsEditorEvs.Models
{
    public class EventPageData : INotifyPropertyChanged
    {
            public event PropertyChangedEventHandler? PropertyChanged;
            protected void OnPropertyChanged([CallerMemberName] string? propertyName = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));[JsonPropertyName("condition")]
            public EventConditionData? Condition { get; set; }

            private int _moveType; [JsonPropertyName("move_type")]
            public int MoveType { get => _moveType; set { _moveType = value; OnPropertyChanged(); } }

            private int _moveSpeed;[JsonPropertyName("move_speed")]
            public int MoveSpeed { get => _moveSpeed; set { _moveSpeed = value; OnPropertyChanged(); } }

            private int _moveFrequency; [JsonPropertyName("move_frequency")]
            public int MoveFrequency { get => _moveFrequency; set { _moveFrequency = value; OnPropertyChanged(); } }

            private bool _walkAnime;[JsonPropertyName("walk_anime")]
            public bool WalkAnime { get => _walkAnime; set { _walkAnime = value; OnPropertyChanged(); } }

            private bool _stepAnime;[JsonPropertyName("step_anime")]
            public bool StepAnime { get => _stepAnime; set { _stepAnime = value; OnPropertyChanged(); } }

            private bool _directionFix; [JsonPropertyName("direction_fix")]
            public bool DirectionFix { get => _directionFix; set { _directionFix = value; OnPropertyChanged(); } }

            private bool _through;[JsonPropertyName("through")]
            public bool Through { get => _through; set { _through = value; OnPropertyChanged(); } }

            private bool _alwaysOnTop; [JsonPropertyName("always_on_top")]
            public bool AlwaysOnTop { get => _alwaysOnTop; set { _alwaysOnTop = value; OnPropertyChanged(); } }

            private int _trigger; [JsonPropertyName("trigger")]
            public int Trigger { get => _trigger; set { _trigger = value; OnPropertyChanged(); } }

            private List<EventCommandData>? _list;
            [JsonPropertyName("list")]
            public List<EventCommandData>? List { get => _list; set { _list = value; OnPropertyChanged(); } }

            [JsonIgnore]
            public string Title { get; set; } = "Página";
        }

        // NUEVA CLASE: Representa las condiciones de aparición
    public class EventConditionData : INotifyPropertyChanged
        {
            public event PropertyChangedEventHandler? PropertyChanged;
            protected void OnPropertyChanged([CallerMemberName] string? propertyName = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

            private bool _selfSwitchValid; [JsonPropertyName("self_switch_valid")]
            public bool SelfSwitchValid { get => _selfSwitchValid; set { _selfSwitchValid = value; OnPropertyChanged(); } }

            private string? _selfSwitchCh; [JsonPropertyName("self_switch_ch")]
            public string? SelfSwitchCh { get => _selfSwitchCh; set { _selfSwitchCh = value; OnPropertyChanged(); } }
        
    }

    public class EventCommandData
    {[JsonPropertyName("code")]
        public int Code { get; set; }

        [JsonPropertyName("indent")]
        public int Indent { get; set; }


        [JsonPropertyName("parameters")]
        public JsonElement Parameters { get; set; } 
    }
}