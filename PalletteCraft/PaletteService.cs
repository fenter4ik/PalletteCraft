using PalletteCraft;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace PaletteCraft.Services
{
    public static class PaletteService
    {
        public static BindingList<PaletteColor> Colors { get; private set; } = new BindingList<PaletteColor>();
        private static Stack<BindingList<PaletteColor>> _undoStack = new Stack<BindingList<PaletteColor>>();
        private static Stack<BindingList<PaletteColor>> _redoStack = new Stack<BindingList<PaletteColor>>();

        // Создание снимка текущего состояния палитры для Undo/Redo
        public static void Snapshot()
        {
            var clone = new BindingList<PaletteColor>(
                Colors.Select(c => new PaletteColor { Name = c.Name, Color = c.Color }).ToList()
            );
            _undoStack.Push(clone);
            _redoStack.Clear(); // Сброс Redo при новом действии
        }

        // Добавление нового цвета
        public static void AddColor(Color color)
        {
            Snapshot();
            var name = $"Color {Colors.Count + 1}";
            Colors.Add(new PaletteColor { Color = color, Name = name });
        }

        // Удаление цвета
        public static void DeleteColor(PaletteColor color)
        {
            Snapshot();
            if (Colors.Contains(color))
                Colors.Remove(color);
        }

        // Отмена последнего действия
        public static void Undo()
        {
            if (_undoStack.Count < 2) return; // Первый снимок — начальное состояние
            _redoStack.Push(_undoStack.Pop());
            Colors = _undoStack.Peek();
        }

        // Повтор отмененного действия
        public static void Redo()
        {
            if (_redoStack.Count == 0) return;
            _undoStack.Push(_redoStack.Pop());
            Colors = _undoStack.Peek();
        }

        // Сохранение палитры в файл формата GIMP
        public static async Task SavePaletteAsync(string path, string paletteName)
        {
            Snapshot();
            using var writer = new StreamWriter(path);
            await writer.WriteLineAsync("GIMP Palette");
            await writer.WriteLineAsync($"Name: {paletteName}");
            await writer.WriteLineAsync($"# Generated {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            await writer.WriteLineAsync("#");

            foreach (var color in Colors)
            {
                await writer.WriteLineAsync($"{color.Color.R,-3} {color.Color.G,-3} {color.Color.B,-3}\t{color.Name}");
            }
        }
        public static void ClearColors()
        {
            Snapshot(); // Создаем снимок перед очисткой
            Colors.Clear();
        }
        // Загрузка палитры из файла
        public static async Task LoadPaletteAsync(string path)
        {
            Snapshot();
            var loadedColors = new List<PaletteColor>();
            using var reader = new StreamReader(path);
            await reader.ReadLineAsync(); // Пропуск заголовка

            while (!reader.EndOfStream)
            {
                var line = (await reader.ReadLineAsync())?.Trim();
                if (string.IsNullOrEmpty(line) || line.StartsWith("#")) continue;

                var parts = line.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length < 3) continue;

                if (int.TryParse(parts[0], out int r) &&
                    int.TryParse(parts[1], out int g) &&
                    int.TryParse(parts[2], out int b))
                {
                    loadedColors.Add(new PaletteColor
                    {
                        Color = Color.FromArgb(r, g, b),
                        Name = parts.Length > 3 ? string.Join(" ", parts.Skip(3)) : "Unnamed"
                    });
                }
            }

            Colors = new BindingList<PaletteColor>(loadedColors);
        }
    }
}