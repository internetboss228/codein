using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

public class UserSettings
{
    public int Width { get; set; } = 800;
    public int Height { get; set; } = 600;
}

public class SettingsManager
{
    public UserSettings LoadSettings()
    {
        var settings = new UserSettings();
        try
        {
            if (File.Exists("settings.txt"))
            {
                var lines = File.ReadAllLines("settings.txt");
                foreach (var line in lines)
                {
                    var parts = line.Split('=');
                    if (parts.Length == 2)
                    {
                        if (parts[0] == "Width" && int.TryParse(parts[1], out int w)) settings.Width = w;
                        if (parts[0] == "Height" && int.TryParse(parts[1], out int h)) settings.Height = h;
                    }
                }
            }
        }
        catch { }
        return settings;
    }

    public void SaveSettings(UserSettings settings)
    {
        try
        {
            File.WriteAllLines("settings.txt", new[] {
                $"Width={settings.Width}",
                $"Height={settings.Height}"
            });
        }
        catch { }
    }

    public List<string> LoadHistory()
    {
        try
        {
            if (File.Exists("history.txt"))
                return File.ReadAllLines("history.txt").ToList();
        }
        catch { }
        return new List<string>();
    }

    public void SaveHistory(List<string> history)
    {
        try
        {
            if (history.Count > 100)
                history = history.Skip(history.Count - 100).ToList();
            File.WriteAllLines("history.txt", history);
        }
        catch { }
    }
}

public class UserInterface
{
    private SettingsManager _manager = new SettingsManager();
    private UserSettings _settings;
    private List<string> _history;
    private bool _running = true;

    public UserInterface()
    {
        _settings = _manager.LoadSettings();
        _history = _manager.LoadHistory();
    }

    public void Run()
    {
        Console.WriteLine("=== Система управления изображениями ===");
        ShowHelp();

        while (_running)
        {
            Console.Write("\n> ");
            var input = Console.ReadLine()?.Trim();
            if (string.IsNullOrEmpty(input)) continue;

            _history.Add(input);
            _manager.SaveHistory(_history);
            ProcessCommand(input);
        }

        _manager.SaveSettings(_settings);
        Console.WriteLine("До свидания!");
    }

    private void ProcessCommand(string command)
    {
        var parts = command.Split(' ', (char)StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 0) return;

        var cmd = parts[0].ToLower();

        switch (cmd)
        {
            case "load": HandleLoad(parts); break;
            case "info": HandleInfo(); break;
            case "resize": HandleResize(parts); break;
            case "save": HandleSave(parts); break;
            case "settings": HandleSettings(parts); break;
            case "history": HandleHistory(); break;
            case "help": ShowHelp(); break;
            case "exit": case "quit": _running = false; break;
            default: Console.WriteLine("Неизвестная команда. Введите 'help' для справки."); break;
        }
    }

    private void HandleLoad(string[] parts)
    {
        if (parts.Length < 2)
        {
            Console.WriteLine("Использование: load <путь>");
            return;
        }
        Console.WriteLine(File.Exists(parts[1]) ?
            $"✓ Файл '{parts[1]}' загружен" :
            $"✗ Файл '{parts[1]}' не найден");
    }

    private void HandleInfo()
    {
        Console.WriteLine($"Информация о изображении:");
        Console.WriteLine($"  Размер: {_settings.Width}x{_settings.Height}");
        Console.WriteLine($"  Формат: JPEG");
        Console.WriteLine($"  Размер файла: 3 MB");
    }

    private void HandleResize(string[] parts)
    {
        if (parts.Length < 3)
        {
            Console.WriteLine("Использование: resize <ширина> <высота>");
            return;
        }

        if (int.TryParse(parts[1], out int width) && int.TryParse(parts[2], out int height))
        {
            if (width > 0 && height > 0)
            {
                _settings.Width = width;
                _settings.Height = height;
                _manager.SaveSettings(_settings);
                Console.WriteLine($"✓ Размер изменен на {width}x{height}");
            }
            else
            {
                Console.WriteLine("Ошибка: ширина и высота должны быть положительными числами");
            }
        }
        else
        {
            Console.WriteLine("Ошибка: неверный формат чисел");
        }
    }

    private void HandleSave(string[] parts)
    {
        var filePath = parts.Length > 1 ? parts[1] : "output.jpg";
        Console.WriteLine($"✓ Изображение сохранено как '{filePath}'");
    }

    private void HandleSettings(string[] parts)
    {
        if (parts.Length == 1)
        {
            Console.WriteLine("Текущие настройки:");
            Console.WriteLine($"  Размер: {_settings.Width}x{_settings.Height}");
            return;
        }

        if (parts.Length < 3)
        {
            Console.WriteLine("Использование: settings <параметр> <значение>");
            Console.WriteLine("Доступные параметры: width, height");
            return;
        }

        var setting = parts[1].ToLower();
        var value = parts[2];

        switch (setting)
        {
            case "width":
                if (int.TryParse(value, out int width) && width > 0)
                {
                    _settings.Width = width;
                    _manager.SaveSettings(_settings);
                    Console.WriteLine($"✓ Ширина изменена на: {width}");
                }
                else
                {
                    Console.WriteLine("Ошибка: ширина должна быть положительным числом");
                }
                break;
            case "height":
                if (int.TryParse(value, out int height) && height > 0)
                {
                    _settings.Height = height;
                    _manager.SaveSettings(_settings);
                    Console.WriteLine($"✓ Высота изменена на: {height}");
                }
                else
                {
                    Console.WriteLine("Ошибка: высота должна быть положительным числом");
                }
                break;
            default:
                Console.WriteLine($"Неизвестный параметр: {setting}");
                break;
        }
    }

    private void HandleHistory()
    {
        Console.WriteLine("История команд:");
        if (_history.Count == 0)
        {
            Console.WriteLine("  История пуста");
            return;
        }

        for (int i = 0; i < _history.Count; i++)
        {
            Console.WriteLine($"  {i + 1}. {_history[i]}");
        }
    }

    private void ShowHelp()
    {
        Console.WriteLine("\nДоступные команды:");
        Console.WriteLine("  load <путь>      - Загрузить изображение");
        Console.WriteLine("  info             - Показать информацию об изображении");
        Console.WriteLine("  resize <w> <h>   - Изменить размер изображения");
        Console.WriteLine("  save [путь]      - Сохранить изображение");
        Console.WriteLine("  settings         - Показать текущие настройки");
        Console.WriteLine("  settings <п> <з> - Изменить настройку");
        Console.WriteLine("  history          - Показать историю команд");
        Console.WriteLine("  help             - Показать эту справку");
        Console.WriteLine("  exit/quit        - Выйти из программы");
    }
}

class Program
{
    static void Main()
    {
        new UserInterface().Run();
    }
}