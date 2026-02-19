using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Utils;
using Microsoft.Extensions.Logging;
using System.Drawing;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using static CounterStrikeSharp.API.Core.Listeners;

namespace CS2SmokeTeamColor;

public class SmokeTeamColorConfig : BasePluginConfig
{
    /// <summary>
    /// Включить/выключить плагин (по умолчанию: 1)
    /// 0 - выключен, 1 - включен
    /// </summary>
    [JsonPropertyName("css_smoketeamcolor_enabled")]
    public int Enabled { get; set; } = 1;

    /// <summary>
    /// Цвет дыма для террористов (по умолчанию: "255 0 0" - красный)
    /// Поддерживаются форматы: RGB ("255 0 0"), HEX ("#FF0000"), название ("red")
    /// </summary>
    [JsonPropertyName("css_smoketeamcolor_color_t")]
    public string ColorT { get; set; } = "255 0 0";

    /// <summary>
    /// Цвет дыма для контр-террористов (по умолчанию: "0 0 255" - синий)
    /// Поддерживаются форматы: RGB ("0 0 255"), HEX ("#0000FF"), название ("blue")
    /// </summary>
    [JsonPropertyName("css_smoketeamcolor_color_ct")]
    public string ColorCT { get; set; } = "0 0 255";

    /// <summary>
    /// Случайный цвет для террористов (по умолчанию: 0)
    /// 0 - использовать фиксированный цвет из ColorT, 1 - случайный цвет при каждом броске
    /// </summary>
    [JsonPropertyName("css_smoketeamcolor_random_t")]
    public int RandomT { get; set; } = 0;

    /// <summary>
    /// Случайный цвет для контр-террористов (по умолчанию: 0)
    /// 0 - использовать фиксированный цвет из ColorCT, 1 - случайный цвет при каждом броске
    /// </summary>
    [JsonPropertyName("css_smoketeamcolor_random_ct")]
    public int RandomCT { get; set; } = 0;

    /// <summary>
    /// Уровень логирования: 0-Trace, 1-Debug, 2-Information, 3-Warning, 4-Error, 5-Critical
    /// </summary>
    [JsonPropertyName("css_smoketeamcolor_loglevel")]
    public int LogLevel { get; set; } = 4;
}

[MinimumApiVersion(362)]
public class CS2_SmokeTeamColor : BasePlugin, IPluginConfig<SmokeTeamColorConfig>
{
    public override string ModuleName => "CS2 Smoke Team Color";
    public override string ModuleVersion => "2.0";
    public override string ModuleAuthor => "Fixed by le1t1337 + AI DeepSeek. Code logic by E!N";

    public required SmokeTeamColorConfig Config { get; set; }

    private readonly Random _random = new();

    public override void Load(bool hotReload)
    {
        // Регистрация команд
        AddCommand("css_smoketeamcolor_help", "Показать справку по плагину Smoke Team Color", OnHelpCommand);
        AddCommand("css_smoketeamcolor_settings", "Показать текущие настройки плагина", OnSettingsCommand);
        AddCommand("css_smoketeamcolor_test", "Тестовая команда для проверки работы плагина", OnTestCommand);
        AddCommand("css_smoketeamcolor_reload", "Перезагрузить конфигурацию плагина", OnReloadCommand);

        // Команды для изменения каждой переменной конфига
        AddCommand("css_smoketeamcolor_setenabled", "Включить/выключить плагин (0/1)", OnSetEnabledCommand);
        AddCommand("css_smoketeamcolor_setcolor_t", "Установить цвет дыма для террористов (RGB / HEX / название)", OnSetColorTCommand);
        AddCommand("css_smoketeamcolor_setcolor_ct", "Установить цвет дыма для контр-террористов (RGB / HEX / название)", OnSetColorCTCommand);
        AddCommand("css_smoketeamcolor_setrandom_t", "Включить случайный цвет для террористов (0/1)", OnSetRandomTCommand);
        AddCommand("css_smoketeamcolor_setrandom_ct", "Включить случайный цвет для контр-террористов (0/1)", OnSetRandomCTCommand);
        AddCommand("css_smoketeamcolor_setloglevel", "Установить уровень логирования (0-5)", OnSetLogLevelCommand);

        // Регистрация слушателя событий
        RegisterListener<OnEntitySpawned>(OnEntitySpawned);
        RegisterEventHandler<EventPlayerDisconnect>(OnPlayerDisconnect);

        if (hotReload)
        {
            Server.NextFrame(() => Log(LogLevel.Information, "Горячая перезагрузка выполнена"));
        }

        PrintInfo();
    }

    private void PrintInfo()
    {
        Log(LogLevel.Information, "===============================================");
        Log(LogLevel.Information, $"Плагин {ModuleName} версии {ModuleVersion} успешно загружен!");
        Log(LogLevel.Information, $"Автор: {ModuleAuthor}");
        Log(LogLevel.Information, "Текущие настройки:");
        Log(LogLevel.Information, $"  css_smoketeamcolor_enabled = {Config.Enabled} (по умолчанию: 1)");
        Log(LogLevel.Information, $"  css_smoketeamcolor_color_t = {Config.ColorT} (по умолчанию: 255 0 0)");
        Log(LogLevel.Information, $"  css_smoketeamcolor_color_ct = {Config.ColorCT} (по умолчанию: 0 0 255)");
        Log(LogLevel.Information, $"  css_smoketeamcolor_random_t = {Config.RandomT} (по умолчанию: 0)");
        Log(LogLevel.Information, $"  css_smoketeamcolor_random_ct = {Config.RandomCT} (по умолчанию: 0)");
        Log(LogLevel.Information, $"  css_smoketeamcolor_loglevel = {Config.LogLevel} (0-Trace, 1-Debug, 2-Information, 3-Warning, 4-Error, 5-Critical)");
        Log(LogLevel.Information, "Команды управления:");
        Log(LogLevel.Information, "  css_smoketeamcolor_help      - показать справку");
        Log(LogLevel.Information, "  css_smoketeamcolor_settings  - показать настройки");
        Log(LogLevel.Information, "  css_smoketeamcolor_test      - тестирование");
        Log(LogLevel.Information, "  css_smoketeamcolor_reload    - перезагрузить конфиг");
        Log(LogLevel.Information, "  css_smoketeamcolor_setenabled <0/1> - вкл/выкл");
        Log(LogLevel.Information, "  css_smoketeamcolor_setcolor_t <цвет>  - цвет T");
        Log(LogLevel.Information, "  css_smoketeamcolor_setcolor_ct <цвет>  - цвет CT");
        Log(LogLevel.Information, "  css_smoketeamcolor_setrandom_t <0/1> - случайный цвет T");
        Log(LogLevel.Information, "  css_smoketeamcolor_setrandom_ct <0/1> - случайный цвет CT");
        Log(LogLevel.Information, "  css_smoketeamcolor_setloglevel <0-5> - уровень логов");
        Log(LogLevel.Information, "===============================================");
    }

    private void Log(LogLevel level, string message)
    {
        if ((int)level >= Config.LogLevel)
            Logger.Log(level, "[SmokeTeamColor] {Message}", message);
    }

    public void OnConfigParsed(SmokeTeamColorConfig config)
    {
        // Валидация числовых параметров
        config.Enabled = Math.Clamp(config.Enabled, 0, 1);
        config.RandomT = Math.Clamp(config.RandomT, 0, 1);
        config.RandomCT = Math.Clamp(config.RandomCT, 0, 1);
        config.LogLevel = Math.Clamp(config.LogLevel, 0, 5);

        // Если строки цветов пустые, устанавливаем значения по умолчанию
        if (string.IsNullOrWhiteSpace(config.ColorT))
            config.ColorT = "255 0 0";
        if (string.IsNullOrWhiteSpace(config.ColorCT))
            config.ColorCT = "0 0 255";

        Config = config;
        Log(LogLevel.Information, $"Конфигурация загружена: Enabled={Config.Enabled}, RandomT={Config.RandomT}, RandomCT={Config.RandomCT}, LogLevel={Config.LogLevel}");
    }

    // Обработчик спавна сущности
    private void OnEntitySpawned(CEntityInstance entity)
    {
        if (Config.Enabled != 1)
            return;

        if (entity.DesignerName != "smokegrenade_projectile")
            return;

        var smokeGrenade = new CSmokeGrenadeProjectile(entity.Handle);
        if (smokeGrenade.Handle == IntPtr.Zero)
            return;

        // Устанавливаем цвет в следующем кадре, чтобы гарантировать доступность всех данных
        Server.NextFrame(() =>
        {
            try
            {
                var thrower = smokeGrenade.Thrower.Value;
                if (thrower == null)
                    return;

                var controller = thrower.Controller.Value;
                if (controller == null)
                    return;

                var player = new CCSPlayerController(controller.Handle);
                if (!player.IsValid)
                    return;

                // Определяем цвет в зависимости от команды и настроек случайности
                Color smokeColor;

                if (player.TeamNum == (int)CsTeam.Terrorist)
                {
                    if (Config.RandomT == 1)
                    {
                        smokeColor = GetRandomColor();
                        Log(LogLevel.Debug, $"Случайный цвет для T от {player.PlayerName}: R={smokeColor.R}, G={smokeColor.G}, B={smokeColor.B}");
                    }
                    else
                    {
                        if (!TryParseColor(Config.ColorT, out smokeColor))
                        {
                            Log(LogLevel.Error, $"Не удалось распарсить цвет '{Config.ColorT}' для игрока {player.PlayerName}. Используется красный.");
                            smokeColor = Color.Red;
                        }
                    }
                }
                else if (player.TeamNum == (int)CsTeam.CounterTerrorist)
                {
                    if (Config.RandomCT == 1)
                    {
                        smokeColor = GetRandomColor();
                        Log(LogLevel.Debug, $"Случайный цвет для CT от {player.PlayerName}: R={smokeColor.R}, G={smokeColor.G}, B={smokeColor.B}");
                    }
                    else
                    {
                        if (!TryParseColor(Config.ColorCT, out smokeColor))
                        {
                            Log(LogLevel.Error, $"Не удалось распарсить цвет '{Config.ColorCT}' для игрока {player.PlayerName}. Используется синий.");
                            smokeColor = Color.Blue;
                        }
                    }
                }
                else
                {
                    return; // Не T и не CT – не меняем цвет
                }

                // Устанавливаем цвет дыма
                smokeGrenade.SmokeColor.X = smokeColor.R;
                smokeGrenade.SmokeColor.Y = smokeColor.G;
                smokeGrenade.SmokeColor.Z = smokeColor.B;

                Log(LogLevel.Debug, $"Цвет дыма установлен для гранаты от {player.PlayerName} (бот: {player.IsBot}): R={smokeColor.R}, G={smokeColor.G}, B={smokeColor.B}");
            }
            catch (Exception ex)
            {
                Log(LogLevel.Error, $"Ошибка в OnEntitySpawned: {ex.Message}");
            }
        });
    }

    private Color GetRandomColor()
    {
        return Color.FromArgb(_random.Next(256), _random.Next(256), _random.Next(256));
    }

    // Парсер цвета, поддерживающий RGB, HEX и названия
    private bool TryParseColor(string input, out Color color)
    {
        color = Color.White;
        if (string.IsNullOrWhiteSpace(input))
            return false;

        input = input.Trim();

        // Попытка распарсить как HEX (#RRGGBB или RRGGBB)
        if (input.StartsWith("#"))
        {
            try
            {
                color = ColorTranslator.FromHtml(input);
                return true;
            }
            catch
            {
                // ignored
            }
        }
        else if (!input.Contains("#") && !input.Contains(",") && input.Split(' ').Length == 1)
        {
            // Попытка распарсить как название цвета
            try
            {
                color = Color.FromName(input);
                if (color.IsKnownColor)
                    return true;
            }
            catch
            {
                // ignored
            }
        }

        // Попытка распарсить как RGB (три числа, разделённые пробелами или запятыми)
        string[] parts = input.Split(new[] { ' ', ',' }, StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 3)
        {
            if (int.TryParse(parts[0], out int r) &&
                int.TryParse(parts[1], out int g) &&
                int.TryParse(parts[2], out int b))
            {
                r = Math.Clamp(r, 0, 255);
                g = Math.Clamp(g, 0, 255);
                b = Math.Clamp(b, 0, 255);
                color = Color.FromArgb(r, g, b);
                return true;
            }
        }

        return false;
    }

    private HookResult OnPlayerDisconnect(EventPlayerDisconnect @event, GameEventInfo info)
    {
        var player = @event.Userid;
        if (player?.IsValid == true)
        {
            Log(LogLevel.Debug, $"Игрок {player.PlayerName} отключился");
        }
        return HookResult.Continue;
    }

    // ---------- Команды ----------

    private void OnHelpCommand(CCSPlayerController? player, CommandInfo command)
    {
        string helpMessage = $"""
            ================================================
            СПРАВКА ПО ПЛАГИНУ {ModuleName} v{ModuleVersion}
            ================================================
            ОПИСАНИЕ:
              Плагин изменяет цвет дымовых гранат в зависимости от команды игрока.
              Можно задать фиксированный цвет для каждой команды или включить случайный цвет.
              Поддерживаются боты.

            КОНФИГУРАЦИОННЫЙ ФАЙЛ:
              addons/counterstrikesharp/configs/plugins/CS2_SmokeTeamColor/CS2_SmokeTeamColor.json

            КОМАНДЫ (значения по умолчанию в скобках):
              css_smoketeamcolor_help               - показать эту справку
              css_smoketeamcolor_settings           - показать текущие настройки
              css_smoketeamcolor_test                - тестовая команда
              css_smoketeamcolor_reload              - перезагрузить конфигурацию

            КОМАНДЫ ДЛЯ ИЗМЕНЕНИЯ НАСТРОЕК:
              css_smoketeamcolor_setenabled <0/1>    - вкл/выкл плагин (1)
              css_smoketeamcolor_setcolor_t <цвет>    - цвет для T (255 0 0)
              css_smoketeamcolor_setcolor_ct <цвет>   - цвет для CT (0 0 255)
              css_smoketeamcolor_setrandom_t <0/1>     - случайный цвет для T (0)
              css_smoketeamcolor_setrandom_ct <0/1>    - случайный цвет для CT (0)
              css_smoketeamcolor_setloglevel <0-5>     - уровень логов (0-Trace,1-Debug,2-Info,3-Warning,4-Error,5-Critical) (4)

            ФОРМАТЫ ЦВЕТА:
              RGB: три числа от 0 до 255 через пробел (например, 255 0 0)
              HEX: #RRGGBB (например, #FF0000)
              Название: red, green, blue, yellow, white, black и др.

            ПРИМЕРЫ:
              css_smoketeamcolor_setcolor_t 255 0 0
              css_smoketeamcolor_setcolor_ct "#0000FF"
              css_smoketeamcolor_setcolor_ct blue
              css_smoketeamcolor_setrandom_t 1
              css_smoketeamcolor_setloglevel 2
            ================================================
            """;
        command.ReplyToCommand(helpMessage);
        if (player != null)
            player.PrintToChat($" {ChatColors.Green}[SmokeTeamColor] {ChatColors.White}Справка отправлена в консоль.");
    }

    private void OnSettingsCommand(CCSPlayerController? player, CommandInfo command)
    {
        string settingsMessage = $"""
            ================================================
            ТЕКУЩИЕ НАСТРОЙКИ {ModuleName} v{ModuleVersion}
            ================================================
            Плагин включен: {Config.Enabled} (по умолчанию: 1)
            Цвет T: {Config.ColorT} (по умолчанию: 255 0 0)
            Цвет CT: {Config.ColorCT} (по умолчанию: 0 0 255)
            Случайный T: {Config.RandomT} (по умолчанию: 0)
            Случайный CT: {Config.RandomCT} (по умолчанию: 0)
            Уровень логирования: {Config.LogLevel} (0-Trace,1-Debug,2-Info,3-Warning,4-Error,5-Critical)
            ================================================
            """;
        command.ReplyToCommand(settingsMessage);
        if (player != null)
            player.PrintToChat($" {ChatColors.Green}[SmokeTeamColor] {ChatColors.White}Настройки отправлены в консоль.");
    }

    private void OnTestCommand(CCSPlayerController? player, CommandInfo command)
    {
        if (player == null)
        {
            command.ReplyToCommand("[SmokeTeamColor] Эта команда доступна только игрокам.");
            return;
        }

        if (Config.Enabled != 1)
        {
            player.PrintToChat($" {ChatColors.Green}[SmokeTeamColor] {ChatColors.White}Плагин выключен. Включите его командой css_smoketeamcolor_setenabled 1.");
            return;
        }

        player.PrintToChat($" {ChatColors.Green}[SmokeTeamColor] {ChatColors.White}=== ТЕСТ ПЛАГИНА ===");
        player.PrintToChat($" Ваша команда: {(CsTeam)player.TeamNum}");

        if (player.TeamNum == (int)CsTeam.Terrorist)
        {
            if (Config.RandomT == 1)
            {
                player.PrintToChat(" Для террористов включен СЛУЧАЙНЫЙ цвет при каждом броске.");
                Color randomColor = GetRandomColor();
                player.PrintToChat($" Пример случайного цвета: R={randomColor.R} G={randomColor.G} B={randomColor.B}");
            }
            else
            {
                player.PrintToChat($" Для террористов установлен ФИКСИРОВАННЫЙ цвет: {Config.ColorT}");
                if (TryParseColor(Config.ColorT, out Color color))
                {
                    player.PrintToChat($" RGB: {color.R} {color.G} {color.B}");
                }
            }
        }
        else if (player.TeamNum == (int)CsTeam.CounterTerrorist)
        {
            if (Config.RandomCT == 1)
            {
                player.PrintToChat(" Для контр-террористов включен СЛУЧАЙНЫЙ цвет при каждом броске.");
                Color randomColor = GetRandomColor();
                player.PrintToChat($" Пример случайного цвета: R={randomColor.R} G={randomColor.G} B={randomColor.B}");
            }
            else
            {
                player.PrintToChat($" Для контр-террористов установлен ФИКСИРОВАННЫЙ цвет: {Config.ColorCT}");
                if (TryParseColor(Config.ColorCT, out Color color))
                {
                    player.PrintToChat($" RGB: {color.R} {color.G} {color.B}");
                }
            }
        }
        else
        {
            player.PrintToChat(" Вы не в команде T или CT. Цвет дыма не будет изменён.");
            return;
        }

        player.PrintToChat(" Бросьте дымовую гранату, чтобы увидеть эффект.");
        player.PrintToChat(" Для изменения настроек используйте css_smoketeamcolor_help");
        player.PrintToChat($" {ChatColors.Green}[SmokeTeamColor] {ChatColors.White}===============================================");

        command.ReplyToCommand("[SmokeTeamColor] Тестовая информация выведена в чат.");
    }

    private void OnReloadCommand(CCSPlayerController? player, CommandInfo command)
    {
        try
        {
            string configPath = Path.Combine(Server.GameDirectory, "counterstrikesharp", "configs", "plugins", "CS2_SmokeTeamColor", "CS2_SmokeTeamColor.json");
            if (File.Exists(configPath))
            {
                string json = File.ReadAllText(configPath);
                var newConfig = JsonSerializer.Deserialize<SmokeTeamColorConfig>(json);
                if (newConfig != null)
                {
                    OnConfigParsed(newConfig);
                    SaveConfig(); // сохраняем обратно, чтобы применить валидацию
                }
            }
            else
            {
                SaveConfig(); // создаст с настройками по умолчанию
            }

            command.ReplyToCommand("[SmokeTeamColor] Конфигурация перезагружена.");
            Log(LogLevel.Information, "Конфигурация перезагружена по команде.");
        }
        catch (Exception ex)
        {
            Log(LogLevel.Error, $"Ошибка при перезагрузке конфига: {ex.Message}");
            command.ReplyToCommand("[SmokeTeamColor] Ошибка при перезагрузке конфига.");
        }
    }

    private void OnSetEnabledCommand(CCSPlayerController? player, CommandInfo command)
    {
        if (command.ArgCount < 2)
        {
            command.ReplyToCommand($"[SmokeTeamColor] Текущее значение Enabled: {Config.Enabled}. Использование: css_smoketeamcolor_setenabled <0/1>");
            return;
        }

        string arg = command.GetArg(1);
        if (int.TryParse(arg, out int value) && (value == 0 || value == 1))
        {
            int oldValue = Config.Enabled;
            Config.Enabled = value;
            SaveConfig();
            command.ReplyToCommand($"[SmokeTeamColor] Enabled изменён с {oldValue} на {value}.");
            Log(LogLevel.Information, $"Enabled изменён на {value} (команда от {(player?.PlayerName ?? "консоли")})");
        }
        else
        {
            command.ReplyToCommand("[SmokeTeamColor] Неверное значение. Используйте 0 или 1.");
        }
    }

    private void OnSetColorTCommand(CCSPlayerController? player, CommandInfo command)
    {
        if (command.ArgCount < 2)
        {
            command.ReplyToCommand($"[SmokeTeamColor] Текущее значение ColorT: {Config.ColorT}. Использование: css_smoketeamcolor_setcolor_t <цвет>");
            return;
        }

        string newColor = command.ArgString.Trim();
        if (TryParseColor(newColor, out _))
        {
            string oldColor = Config.ColorT;
            Config.ColorT = newColor;
            SaveConfig();
            command.ReplyToCommand($"[SmokeTeamColor] Цвет T изменён с '{oldColor}' на '{newColor}'.");
            Log(LogLevel.Information, $"Цвет T изменён на '{newColor}' (команда от {(player?.PlayerName ?? "консоли")})");
        }
        else
        {
            command.ReplyToCommand("[SmokeTeamColor] Не удалось распарсить цвет. Используйте форматы: RGB (255 0 0), HEX (#FF0000) или название (red).");
        }
    }

    private void OnSetColorCTCommand(CCSPlayerController? player, CommandInfo command)
    {
        if (command.ArgCount < 2)
        {
            command.ReplyToCommand($"[SmokeTeamColor] Текущее значение ColorCT: {Config.ColorCT}. Использование: css_smoketeamcolor_setcolor_ct <цвет>");
            return;
        }

        string newColor = command.ArgString.Trim();
        if (TryParseColor(newColor, out _))
        {
            string oldColor = Config.ColorCT;
            Config.ColorCT = newColor;
            SaveConfig();
            command.ReplyToCommand($"[SmokeTeamColor] Цвет CT изменён с '{oldColor}' на '{newColor}'.");
            Log(LogLevel.Information, $"Цвет CT изменён на '{newColor}' (команда от {(player?.PlayerName ?? "консоли")})");
        }
        else
        {
            command.ReplyToCommand("[SmokeTeamColor] Не удалось распарсить цвет. Используйте форматы: RGB (0 0 255), HEX (#0000FF) или название (blue).");
        }
    }

    private void OnSetRandomTCommand(CCSPlayerController? player, CommandInfo command)
    {
        if (command.ArgCount < 2)
        {
            command.ReplyToCommand($"[SmokeTeamColor] Текущее значение RandomT: {Config.RandomT}. Использование: css_smoketeamcolor_setrandom_t <0/1>");
            return;
        }

        string arg = command.GetArg(1);
        if (int.TryParse(arg, out int value) && (value == 0 || value == 1))
        {
            int oldValue = Config.RandomT;
            Config.RandomT = value;
            SaveConfig();
            command.ReplyToCommand($"[SmokeTeamColor] RandomT изменён с {oldValue} на {value}.");
            Log(LogLevel.Information, $"RandomT изменён на {value} (команда от {(player?.PlayerName ?? "консоли")})");
        }
        else
        {
            command.ReplyToCommand("[SmokeTeamColor] Неверное значение. Используйте 0 или 1.");
        }
    }

    private void OnSetRandomCTCommand(CCSPlayerController? player, CommandInfo command)
    {
        if (command.ArgCount < 2)
        {
            command.ReplyToCommand($"[SmokeTeamColor] Текущее значение RandomCT: {Config.RandomCT}. Использование: css_smoketeamcolor_setrandom_ct <0/1>");
            return;
        }

        string arg = command.GetArg(1);
        if (int.TryParse(arg, out int value) && (value == 0 || value == 1))
        {
            int oldValue = Config.RandomCT;
            Config.RandomCT = value;
            SaveConfig();
            command.ReplyToCommand($"[SmokeTeamColor] RandomCT изменён с {oldValue} на {value}.");
            Log(LogLevel.Information, $"RandomCT изменён на {value} (команда от {(player?.PlayerName ?? "консоли")})");
        }
        else
        {
            command.ReplyToCommand("[SmokeTeamColor] Неверное значение. Используйте 0 или 1.");
        }
    }

    private void OnSetLogLevelCommand(CCSPlayerController? player, CommandInfo command)
    {
        if (command.ArgCount < 2)
        {
            command.ReplyToCommand($"[SmokeTeamColor] Текущий уровень логов: {Config.LogLevel}. Использование: css_smoketeamcolor_setloglevel <0-5>");
            return;
        }

        string arg = command.GetArg(1);
        if (int.TryParse(arg, out int value) && value >= 0 && value <= 5)
        {
            int oldValue = Config.LogLevel;
            Config.LogLevel = value;
            SaveConfig();
            command.ReplyToCommand($"[SmokeTeamColor] Уровень логов изменён с {oldValue} на {value}.");
            Log(LogLevel.Information, $"LogLevel изменён на {value} (команда от {(player?.PlayerName ?? "консоли")})");
        }
        else
        {
            command.ReplyToCommand("[SmokeTeamColor] Неверное значение. Используйте число от 0 до 5.");
        }
    }

    private void SaveConfig()
    {
        try
        {
            string configPath = Path.Combine(Server.GameDirectory, "counterstrikesharp", "configs", "plugins", "CS2_SmokeTeamColor", "CS2_SmokeTeamColor.json");
            Directory.CreateDirectory(Path.GetDirectoryName(configPath)!);

            var options = new JsonSerializerOptions { WriteIndented = true, Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping };
            string json = JsonSerializer.Serialize(Config, options);
            File.WriteAllText(configPath, json);

            Log(LogLevel.Debug, $"Конфигурация сохранена в {configPath}");
        }
        catch (Exception ex)
        {
            Log(LogLevel.Error, $"Ошибка сохранения конфигурации: {ex.Message}");
        }
    }

    public override void Unload(bool hotReload)
    {
        Log(LogLevel.Information, "Плагин выгружен.");
    }
}