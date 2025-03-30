using PalletteCraft;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace PaletteCraft.Controls
{
    public class ColorBoxControl : UserControl
    {
        public PaletteColor PaletteColor { get; private set; }
        private Label lblHex;

        public event EventHandler<PaletteColor> ColorSelected;

        public ColorBoxControl(PaletteColor paletteColor)
        {
            PaletteColor = paletteColor;
            InitializeControl();
        }

        private void InitializeControl()
        {
            this.Size = new Size(80, 100);
            this.BackColor = PaletteColor.Color;
            this.BorderStyle = BorderStyle.FixedSingle;
            this.Margin = new Padding(5);
            this.Cursor = Cursors.Hand;

            lblHex = new Label
            {
                Text = ColorToHex(PaletteColor.Color),
                Dock = DockStyle.Bottom,
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = Color.White,
                AutoSize = false,
                Height = 20
            };

            this.Controls.Add(lblHex);
            this.Click += (s, e) => OnColorSelected();
            lblHex.Click += (s, e) => OnColorSelected();
        }

        public void UpdateColor(Color newColor)
        {
            PaletteColor.Color = newColor;
            this.BackColor = newColor;
            lblHex.Text = ColorToHex(newColor);
        }

        private string ColorToHex(Color color) =>
            $"#{color.R:X2}{color.G:X2}{color.B:X2}";

        private void OnColorSelected()
        {
            ColorSelected?.Invoke(this, PaletteColor);
        }
    }
}
