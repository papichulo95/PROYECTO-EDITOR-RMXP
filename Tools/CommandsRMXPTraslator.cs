using System.IO;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using PokemonEssentialsEditorEvs.Models;

namespace PokemonEssentialsEditorEvs.Tools
{
    // ── Modelo del schema ──────────────────────────────────────────────────────
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

    // ── Diccionario: carga y accede al schema ──────────────────────────────────
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
            => _schemas.TryGetValue(code.ToString(), out var s) ? s : null;
    }

    // ── Diccionario de comandos de MoveRoute (códigos RMXP estándar) ───────────
    internal static class MoveCommandNames
    {
        // Códigos estándar RPG Maker XP / PBMoveRoute
        private static readonly Dictionary<int, string> _names = new()
        {
            { 0,  "Fin de Ruta" },
            { 1,  "Bajar" },
            { 2,  "Izquierda" },
            { 3,  "Derecha" },
            { 4,  "Subir" },
            { 5,  "Abajo-Izq." },
            { 6,  "Abajo-Der." },
            { 7,  "Arriba-Izq." },
            { 8,  "Arriba-Der." },
            { 9,  "Aleatorio" },
            { 10, "Hacia Jugador" },
            { 11, "Alejarse" },
            { 12, "Un Paso Adelante" },
            { 13, "Un Paso Atrás" },
            { 14, "Saltar" },
            { 15, "Esperar" },
            { 16, "Girar Abajo" },
            { 17, "Girar Izq." },
            { 18, "Girar Der." },
            { 19, "Girar Arriba" },
            { 20, "Girar 90° Der." },
            { 21, "Girar 90° Izq." },
            { 22, "Girar 180°" },
            { 23, "Girar Aleatorio" },
            { 24, "Girar Hacia Jugador" },
            { 25, "Girar Alejarse" },
            { 26, "Velocidad+" },
            { 27, "Velocidad-" },
            { 28, "Frecuencia+" },
            { 29, "Frecuencia-" },
            { 30, "Anim. Caminar ON" },
            { 31, "Anim. Caminar OFF" },
            { 32, "Anim. Parado ON" },
            { 33, "Anim. Parado OFF" },
            { 34, "Dir. Fija ON" },
            { 35, "Dir. Fija OFF" },
            { 36, "Atravesar ON" },
            { 37, "Atravesar OFF" },
            { 38, "Siempre Visible ON" },
            { 39, "Siempre Visible OFF" },
            { 40, "Cambiar Gráfico" },
            { 41, "Opacidad" },
            { 42, "Mezcla" },
            { 43, "Reproducir SE" },
            { 44, "Script" },
        };

        public static string Get(int code)
            => _names.TryGetValue(code, out var name) ? name : $"Mov.#{code}";
    }

    // ── Traductor principal ────────────────────────────────────────────────────
    public class TranslatorCommands
    {
        private readonly CommandsDictionary _dictionary;

        public TranslatorCommands(CommandsDictionary dictionary)
        {
            _dictionary = dictionary;
        }

        public CommandSchema? GetSchema(int code) => _dictionary.GetSchema(code);

        public string TranslateCommandToUI(int code, JsonElement parameters)
        {
            var schema = _dictionary.GetSchema(code);

            // ── Comando 0: línea vacía / separador estructural ─────────────
            if (code == 0)
                return "";   // Se renderiza como espacio en blanco

            if (schema == null)
                return $"Comando desconocido ({code})";

            bool hasParams = parameters.ValueKind == JsonValueKind.Array
                          && parameters.GetArrayLength() > 0;

            // Extractor de parámetro como string legible
            string P(int i)
            {
                if (!hasParams || i >= parameters.GetArrayLength()) return "?";
                var el = parameters[i];
                return el.ValueKind switch
                {
                    JsonValueKind.String => el.GetString() ?? "",
                    JsonValueKind.Number => el.GetRawText(),
                    JsonValueKind.True   => "ON",
                    JsonValueKind.False  => "OFF",
                    JsonValueKind.Null   => "null",
                    _                   => el.GetRawText()
                };
            }

            // Extractor de parámetro como entero
            int PI(int i, int fallback = 0)
            {
                if (!hasParams || i >= parameters.GetArrayLength()) return fallback;
                var el = parameters[i];
                return el.ValueKind == JsonValueKind.Number ? el.GetInt32() : fallback;
            }

            return code switch
            {
                // Mensajes
                101 or 401 => $">> \"{Truncate(P(0), 50)}\"",
                102        => "Mostrar opciones",
                402        => $"  Cuando: opcion {PI(0) + 1}",
                403        => "  Cuando: [Cancelar]",
                404        => "Fin de Opciones",
                103        => $"Ingresar numero -> Var[{PI(0)}] ({PI(1)} digitos)",
                104        => $"Opciones de texto: pos={PI(0)}, frame={PI(1)}",
                105        => $"Capturar boton -> Var[{PI(0)}]",

                // ── Notas / Comentarios ───────────────────────────────────────
                108        => $"# {P(0)}",
                408        => $"  {P(0)}",

                // ── Control de flujo ──────────────────────────────────────────
                106        => $"Esperar {PI(0)} frames",
                111        => FormatConditional(parameters, PI),
                411        => "  Si No",
                412        => "Fin de Rama Condicional",
                112        => "Inicio de Bucle",
                413        => "  Repetir",
                113        => "Salir del Bucle",
                115        => "Salir del Evento",
                116        => "Borrar Evento",
                117        => $"Evento Comun [{PI(0)}]",
                118        => $"Etiqueta: \"{P(0)}\"",
                119        => $"Saltar a: \"{P(0)}\"",

                // Variables e Interruptores
                121        => FormatSwitch(parameters, PI),
                122        => FormatVariable(parameters, PI),
                123        => $"Inter. Local [{P(0)}] -> {(PI(1) == 0 ? "ON" : "OFF")}",
                124        => $"Temporizador: {(PI(0) == 0 ? $"Iniciar ({PI(1)}s)" : "Detener")}",

                // Objeto y Dinero
                125        => $"Dinero: {(PI(0) == 0 ? "+" : "-")}{P(2)}",
                126        => $"Objeto [{PI(0)}]: {(PI(1) == 0 ? "+" : "-")}{PI(2)}",
                127        => $"Arma [{PI(0)}]: {(PI(1) == 0 ? "+" : "-")}{PI(2)}",
                128        => $"Armadura [{PI(0)}]: {(PI(1) == 0 ? "+" : "-")}{PI(2)}",
                129        => $"Grupo: {(PI(1) == 0 ? "Anadir" : "Quitar")} Actor[{PI(0)}]",

                // Sistema
                131        => $"Skin ventana: \"{P(0)}\"",
                132        => $"BGM batalla: \"{P(0)}\"",
                133        => $"ME fin batalla: \"{P(0)}\"",
                134        => $"Guardar: {(PI(0) == 0 ? "Permitir" : "Prohibir")}",
                135        => $"Menu: {(PI(0) == 0 ? "Permitir" : "Prohibir")}",
                136        => $"Encuentros: {(PI(0) == 0 ? "Permitir" : "Prohibir")}",
                351        => "Abrir Menu",
                352        => "Pantalla de Guardado",
                353        => "Game Over",
                354        => "Volver a Titulo",
                302        => "Abrir Tienda",
                303        => $"Ingresar Nombre: Actor[{PI(0)}]",

                // Movimiento
                201        => PI(0) == 0
                                ? $"Transferir -> Mapa[{PI(1)}] ({PI(2)},{PI(3)})"
                                : $"Transferir -> Mapa[Var[{PI(1)}]]",
                202        => $"Mover Evento[{PI(0)}] -> ({PI(2)},{PI(3)})",
                203        => $"Desplazar mapa: dir={PI(0)}, dist={PI(1)}",
                204        => PI(0) switch { 0=>"Cambiar panorama", 1=>"Cambiar niebla", 2=>"Cambiar battleback", _=>"Cambiar ajustes" },
                205        => $"Tono niebla (dur={PI(1)})",
                206        => $"Opacidad niebla: {PI(0)} (dur={PI(1)})",
                207        => $"Animacion[{PI(1)}] en Evento[{PI(0)}]",
                208        => $"Transparencia jugador: {(PI(0) == 0 ? "SI" : "NO")}",
                209        => FormatSetMoveRoute(parameters, PI),
                509        => FormatMoveRouteStep(parameters),
                210        => "Esperar fin de movimiento",

                // Pantalla
                221        => "Congelar pantalla",
                222        => $"Transicion: \"{P(0)}\"",
                223        => $"Tono de pantalla (dur={PI(1)})",
                224        => $"Destello (dur={PI(1)})",
                225        => $"Sacudir pantalla (dur={PI(2)})",
                231        => $"Mostrar imagen [{PI(0)}]: \"{P(1)}\"",
                232        => $"Mover imagen [{PI(0)}] (dur={PI(1)})",
                233        => $"Rotar imagen [{PI(0)}]: {PI(1)}",
                234        => $"Tono imagen [{PI(0)}]",
                235        => $"Borrar imagen [{PI(0)}]",
                236        => $"Clima: tipo={PI(0)}, poder={PI(1)}",

                // Audio
                241        => $"BGM: \"{P(0)}\"",
                242        => $"Desvanecer BGM ({PI(0)}s)",
                245        => $"BGS: \"{P(0)}\"",
                246        => $"Desvanecer BGS ({PI(0)}s)",
                247        => "Memorizar BGM/BGS",
                248        => "Restaurar BGM/BGS",
                249        => $"ME: \"{P(0)}\"",
                250        => $"SE: \"{P(0)}\"",
                251        => "Detener SE",

                // Batalla
                301        => $"Batalla: Tropa[{PI(0)}]",
                601        => "  Si gana",
                602        => "  Si escapa",
                603        => "  Si pierde",
                331        => $"HP Enemigo[{PI(0)}]",
                332        => $"SP Enemigo[{PI(0)}]",
                333        => $"Estado Enemigo[{PI(0)}]",
                334        => $"Recuperar Enemigo[{PI(0)}]",
                335        => $"Aparicion Enemigo[{PI(0)}]",
                336        => $"Transformar Enemigo[{PI(0)}] -> [{PI(1)}]",
                337        => $"Animacion batalla en Enemigo[{PI(0)}]",
                338        => $"Infligir dano",
                339        => $"Forzar accion: Enemigo[{PI(0)}]",
                340        => "Abortar batalla",

                // Actores
                311        => $"HP Actor[{PI(0)}]: {(PI(1) == 0 ? "+" : "-")}{PI(2)}",
                312        => $"SP Actor[{PI(0)}]: {(PI(1) == 0 ? "+" : "-")}{PI(2)}",
                313        => $"Estado Actor[{PI(0)}]: {(PI(2) == 0 ? "Anadir" : "Quitar")} [{PI(1)}]",
                314        => $"Recuperar: {(PI(0) == 0 ? "Grupo" : $"Actor[{PI(0)}]")}",
                315        => $"EXP Actor[{PI(0)}]: {(PI(1) == 0 ? "+" : "-")}{PI(2)}",
                316        => $"Nivel Actor[{PI(0)}]: {(PI(1) == 0 ? "+" : "-")}{PI(2)}",
                317        => $"Parametros Actor[{PI(0)}]",
                318        => $"Habilidades Actor[{PI(0)}]: {(PI(2) == 0 ? "Aprender" : "Olvidar")} [{PI(1)}]",
                319        => $"Equipo Actor[{PI(0)}]: slot={PI(1)}",
                320        => $"Nombre Actor[{PI(0)}]: \"{P(1)}\"",
                321        => $"Clase Actor[{PI(0)}] -> [{PI(1)}]",
                322        => $"Grafico Actor[{PI(0)}]: \"{P(1)}\"",

                // Script
                355        => $"Script: {Truncate(P(0), 45)}",
                655        => $"  -> {Truncate(P(0), 45)}",

                _          => schema.Name
            };
        }

        // ── Formateadores específicos ──────────────────────────────────────────

        private static string FormatConditional(JsonElement p, System.Func<int, int, int> PI)
        {
            return PI(0, -1) switch
            {
                0  => $"Si Interruptor[{PI(1, 0)}] == {(PI(2, 0) == 0 ? "ON" : "OFF")}",
                1  => $"Si Variable[{PI(1, 0)}] {Op(PI(4, 0))} {PI(3, 0)}",
                2  => $"Si Inter. Local es {(PI(2, 0) == 0 ? "ON" : "OFF")}",
                3  => $"Si Temporizador {(PI(2, 0) == 0 ? ">=" : "<=")} {PI(1, 0)}s",
                6  => $"Si direccion personaje == {Dir(PI(2, 0))}",
                7  => $"Si Dinero {(PI(2, 0) == 0 ? ">=" : "<=")} {PI(1, 0)}",
                11 => $"Si boton [{PI(1, 0)}] presionado",
                12 => "Si Script...",
                _  => "Rama Condicional"
            };
        }

        private static string FormatSwitch(JsonElement p, System.Func<int, int, int> PI)
        {
            int from = PI(0, 0), to = PI(1, 0);
            string state = PI(2, 0) == 0 ? "ON" : "OFF";
            return from == to
                ? $"Interruptor[{from}] -> {state}"
                : $"Interruptores[{from}~{to}] -> {state}";
        }

        private static string FormatVariable(JsonElement p, System.Func<int, int, int> PI)
        {
            int from = PI(0, 0), to = PI(1, 0);
            string op  = PI(2, 0) switch { 0=>"=", 1=>"+=", 2=>"-=", 3=>"*=", 4=>"/=", 5=>"%=", _=>"=" };
            string src = PI(3, 0) switch
            {
                0 => PI(4, 0).ToString(),
                1 => $"Var[{PI(4, 0)}]",
                2 => $"Random({PI(4, 0)}~{PI(5, 0)})",
                _ => "?"
            };
            string varStr = from == to ? $"Variable[{from}]" : $"Variables[{from}~{to}]";
            return $"{varStr} {op} {src}";
        }

        /// <summary>
        /// Formatea el comando 209 (Set Move Route).
        /// parameters[0] = character id, parameters[1] = RPG::MoveRoute object
        /// </summary>
        private static string FormatSetMoveRoute(JsonElement p, System.Func<int, int, int> PI)
        {
            int charId = PI(0, 0);
            string target = charId switch
            {
                -1 => "Jugador",
                0  => "Este Evento",
                _  => $"Evento[{charId}]"
            };

            // Intentamos contar cuántos pasos tiene la ruta
            int stepCount = 0;
            if (p.ValueKind == JsonValueKind.Array && p.GetArrayLength() > 1)
            {
                var routeEl = p[1];
                if (routeEl.ValueKind == JsonValueKind.Object &&
                    routeEl.TryGetProperty("list", out var listEl) &&
                    listEl.ValueKind == JsonValueKind.Array)
                {
                    // No contamos el comando 0 final de la ruta
                    stepCount = System.Math.Max(0, listEl.GetArrayLength() - 1);
                }
            }

            return stepCount > 0
                ? $"Ruta de movimiento -> {target} ({stepCount} pasos)"
                : $"Ruta de movimiento -> {target}";
        }

        /// <summary>
        /// Formatea el comando 509 (paso individual de una ruta de movimiento).
        /// parameters[0] = RPG::MoveCommand del paso anterior en la ruta.
        /// Se muestra indentado bajo el 209 que lo contiene.
        /// </summary>
        private static string FormatMoveRouteStep(JsonElement p)
        {
            if (p.ValueKind != JsonValueKind.Array || p.GetArrayLength() == 0)
                return "  : Paso de ruta";

            var moveCmd = p[0];
            if (moveCmd.ValueKind != JsonValueKind.Object)
                return "  : Paso de ruta";

            // El MoveCommand tiene { "code": N, "parameters": [...] }
            if (!moveCmd.TryGetProperty("code", out var codeEl))
                return "  : Paso de ruta";

            int moveCode = codeEl.GetInt32();
            string moveName = MoveCommandNames.Get(moveCode);

            // Parámetros extra del paso (ej. frames de espera, SE a reproducir)
            string extra = "";
            if (moveCmd.TryGetProperty("parameters", out var moveParams) &&
                moveParams.ValueKind == JsonValueKind.Array &&
                moveParams.GetArrayLength() > 0)
            {
                var first = moveParams[0];
                extra = first.ValueKind switch
                {
                    JsonValueKind.Number => $" ({first.GetRawText()})",
                    JsonValueKind.String => $" \"{first.GetString()}\"",
                    JsonValueKind.Object => FormatAudioParam(first),
                    _                   => ""
                };
            }

            return $"  : {moveName}{extra}";
        }

        /// <summary>
        /// Extrae el nombre de un RPG::AudioFile serializado en JSON.
        /// </summary>
        private static string FormatAudioParam(JsonElement audioEl)
        {
            if (audioEl.TryGetProperty("name", out var nameEl))
                return $" \"{nameEl.GetString()}\"";
            return "";
        }

        private static string Op(int op) => op switch
        {
            0=>"==", 1=>">=", 2=>"<=", 3=>">", 4=>"<", 5=>"!=", _=>"?"
        };

        private static string Dir(int dir) => dir switch
        {
            2=>"Abajo", 4=>"Izquierda", 6=>"Derecha", 8=>"Arriba", _=>dir.ToString()
        };

        private static string Truncate(string value, int max)
            => value.Length <= max ? value : value[..max] + "...";
    }

    // ── Comando de UI ──────────────────────────────────────────────────────────
    public class UICommand
    {
        // Código RMXP original — necesario para distinguir tipos en la UI
        public int    Code        { get; set; }
        public string DisplayText { get; set; } = "";
        public string Color       { get; set; } = "#3E3E42";
        public int    Indent      { get; set; }

        // Separador visual: comando 0 = línea vacía
        public bool IsEmpty => Code == 0;

        // Las notas (108/408) se muestran en gris oscuro con texto más claro
        public bool IsComment => Code == 108 || Code == 408;

        // Los pasos de ruta (509) usan fuente monospace más pequeña
        public bool IsMoveStep => Code == 509;

        // Color del texto: notas en gris, resto en blanco
        public string TextColor => IsComment ? "#AAAAAA" : "White";

        // Estilo: notas en cursiva
        public Avalonia.Media.FontStyle TextStyle =>
            IsComment ? Avalonia.Media.FontStyle.Italic
                      : Avalonia.Media.FontStyle.Normal;

        public Avalonia.Thickness Margin => new Avalonia.Thickness(Indent * 20, 1, 0, 1);
    }
}