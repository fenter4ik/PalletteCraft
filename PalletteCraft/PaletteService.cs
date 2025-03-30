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

        public static void AddColor(Color color)
        {
            var name = $"Color {Colors.Count + 1}";
            Colors.Add(new PaletteColor { Color = color, Name = name });
        }

        public static void DeleteColor(PaletteColor color)
        {
            if (Colors.Contains(color))
                Colors.Remove(color);
        }

        public static void ClearColors() =>
            Colors.Clear();

        public static async Task SavePaletteAsync(string path, string paletteName)
        {
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

        public static async Task LoadPaletteAsync(string path)
        {
            var loadedColors = new List<PaletteColor>();
            using var reader = new StreamReader(path);
            await reader.ReadLineAsync(); // header
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
