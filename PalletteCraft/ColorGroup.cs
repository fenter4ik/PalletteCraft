using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PalletteCraft
{
    // ColorGroup.cs
    public class ColorGroup
    {
        public string Name { get; set; } = "New Group";
        public BindingList<PaletteColor> Colors { get; set; } = new BindingList<PaletteColor>();

        public override string ToString() => $"{Name} ({Colors.Count} colors)";
    }
}
