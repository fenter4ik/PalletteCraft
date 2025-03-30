using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PalletteCraft
{
    public class PaletteColor
    {
        public string Name { get; set; } = "New Color";
        public Color Color { get; set; } = Color.White;

        public override string ToString() =>
            $"{Name} (#{Color.R:X2}{Color.G:X2}{Color.B:X2})";
    }
}
