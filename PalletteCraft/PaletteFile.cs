using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PalletteCraft
{
    public static class PaletteFile
    {
        public static List<PaletteColor> Load(string path)
        {
            var colors = new List<PaletteColor>();
            using var reader = new StreamReader(path);

            reader.ReadLine(); // Skip header
            while (!reader.EndOfStream)
            {
                var line = reader.ReadLine()?.Trim();
                if (string.IsNullOrEmpty(line) || line.StartsWith("#")) continue;

                var parts = line.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length < 3) continue;

                if (int.TryParse(parts[0], out int r) &&
                    int.TryParse(parts[1], out int g) &&
                    int.TryParse(parts[2], out int b))
                {
                    colors.Add(new PaletteColor
                    {
                        Color = Color.FromArgb(r, g, b),
                        Name = parts.Length > 3 ? string.Join(" ", parts[3..]) : "Unnamed"
                    });
                }
            }
            return colors;
        }

        public static void Save(string path, IEnumerable<PaletteColor> colors, string name = "Custom Palette")
        {
            using var writer = new StreamWriter(path);
            writer.WriteLine("GIMP Palette");
            writer.WriteLine($"Name: {name}");
            writer.WriteLine($"# Generated {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            writer.WriteLine("#");

            foreach (var color in colors)
            {
                writer.WriteLine($"{color.Color.R,-3} {color.Color.G,-3} {color.Color.B,-3}\t{color.Name}");
            }
        }
    }
}
