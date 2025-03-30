using PaletteCraft.Controls;
using PaletteCraft.Services;
using System;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PalletteCraft
{
    public partial class MainForm : Form
    {
        private PaletteColor selectedColor;
        private readonly Color accentColor = Color.FromArgb(0, 122, 204);

        // Элементы управления
        private FlowLayoutPanel colorsPanel;
        private Panel selectedColorPanel;
        private TextBox txtHex;
        private Button btnAddColor, btnDeleteColor, btnClearAll;
        private Button btnGenerateGradient, btnSavePalette, btnLoadPalette;

        public MainForm()
        {
            InitializeComponent();
            SetupUI();
            SetupEventHandlers();
            DoubleBuffered = true;
            MinimumSize = new Size(1000, 650);
            RefreshColorBoxes();
        }

        private void SetupUI()
        {
            Text = "🎨 Palette Craft";
            BackColor = Color.White;
            Font = new Font("Segoe UI", 9f);
            Padding = new Padding(20);

            var mainLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                ColumnStyles =
                {
                    new ColumnStyle(SizeType.Percent, 70),
                    new ColumnStyle(SizeType.Percent, 30)
                }
            };

            colorsPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                WrapContents = true,
                Padding = new Padding(15),
                BackColor = ColorTranslator.FromHtml("#F8F9FA")
            };

            var controlPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                Padding = new Padding(20)
            };

            var controlLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                RowCount = 9,
                RowStyles =
                {
                    new RowStyle(SizeType.Absolute, 180), // Selected color
                    new RowStyle(SizeType.Absolute, 20),  // Spacer
                    new RowStyle(SizeType.Absolute, 40),  // Hex
                    new RowStyle(SizeType.Absolute, 15),  // Spacer
                    new RowStyle(SizeType.Absolute, 40),  // Buttons
                    new RowStyle(SizeType.Absolute, 40),
                    new RowStyle(SizeType.Absolute, 40),
                    new RowStyle(SizeType.Absolute, 40),
                    new RowStyle(SizeType.Percent, 100)
                }
            };

            selectedColorPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                Padding = new Padding(5)
            };

            txtHex = new TextBox
            {
                Dock = DockStyle.Fill,
                Font = new Font("Consolas", 12),
                TextAlign = HorizontalAlignment.Center,
                BorderStyle = BorderStyle.FixedSingle
            };

            btnAddColor = CreateButton("Add Color", "➕");
            btnDeleteColor = CreateButton("Delete Color", "🗑");
            btnClearAll = CreateButton("Clear All", "❌");
            btnGenerateGradient = CreateButton("Generate Gradient", "🌓");
            btnSavePalette = CreateButton("Save Palette", "💾");
            btnLoadPalette = CreateButton("Load Palette", "📂");

            // Расположение элементов в панели управления
            controlLayout.Controls.Add(selectedColorPanel, 0, 0);
            controlLayout.Controls.Add(txtHex, 0, 2);
            controlLayout.Controls.Add(btnAddColor, 0, 4);
            controlLayout.Controls.Add(btnDeleteColor, 0, 5);
            controlLayout.Controls.Add(btnClearAll, 0, 6);
            controlLayout.Controls.Add(btnGenerateGradient, 0, 7);
            controlLayout.Controls.Add(btnSavePalette, 0, 8);
            controlLayout.Controls.Add(btnLoadPalette, 0, 9);

            controlPanel.Controls.Add(controlLayout);
            mainLayout.Controls.Add(colorsPanel, 0, 0);
            mainLayout.Controls.Add(controlPanel, 1, 0);
            Controls.Add(mainLayout);
        }

        private Button CreateButton(string text, string emoji)
        {
            var btn = new Button
            {
                Text = $"{emoji} {text}",
                Height = 40,
                Dock = DockStyle.Top,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.White,
                ForeColor = accentColor,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Margin = new Padding(0, 0, 0, 10),
                Cursor = Cursors.Hand
            };
            btn.FlatAppearance.BorderColor = Color.Gainsboro;
            btn.FlatAppearance.BorderSize = 1;
            btn.MouseEnter += (s, e) => btn.BackColor = ColorTranslator.FromHtml("#F8F9FA");
            btn.MouseLeave += (s, e) => btn.BackColor = Color.White;
            return btn;
        }

        private void SetupEventHandlers()
        {
            btnAddColor.Click += (s, e) =>
            {
                using var dlg = new ColorDialog();
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    PaletteService.AddColor(dlg.Color);
                    RefreshColorBoxes();
                }
            };

            btnDeleteColor.Click += (s, e) =>
            {
                if (selectedColor != null)
                {
                    PaletteService.DeleteColor(selectedColor);
                    selectedColor = null;
                    selectedColorPanel.BackColor = Color.White;
                    txtHex.Text = "";
                    RefreshColorBoxes();
                }
            };

            btnClearAll.Click += (s, e) =>
            {
                PaletteService.ClearColors();
                selectedColor = null;
                selectedColorPanel.BackColor = Color.White;
                txtHex.Text = "";
                RefreshColorBoxes();
            };

            btnGenerateGradient.Click += (s, e) =>
            {
                using var dlg = new GradientDialog();
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    foreach (var clr in dlg.GeneratedColors)
                    {
                        PaletteService.AddColor(clr);
                    }
                    RefreshColorBoxes();
                }
            };

            btnSavePalette.Click += async (s, e) =>
            {
                using var dialog = new SaveFileDialog { Filter = "GIMP Palette|*.gpl" };
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    await PaletteService.SavePaletteAsync(dialog.FileName, Path.GetFileNameWithoutExtension(dialog.FileName));
                }
            };

            btnLoadPalette.Click += async (s, e) =>
            {
                using var dialog = new OpenFileDialog { Filter = "GIMP Palette|*.gpl" };
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    await PaletteService.LoadPaletteAsync(dialog.FileName);
                    RefreshColorBoxes();
                }
            };

            txtHex.KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Enter)
                {
                    UpdateSelectedColorFromHex();
                }
            };
        }

        private void RefreshColorBoxes()
        {
            colorsPanel.Controls.Clear();
            foreach (var color in PaletteService.Colors)
            {
                var box = new ColorBoxControl(color);
                box.ColorSelected += (s, palColor) => SelectColor(palColor);
                colorsPanel.Controls.Add(box);
            }
        }

        private void SelectColor(PaletteColor palColor)
        {
            selectedColor = palColor;
            selectedColorPanel.BackColor = palColor.Color;
            txtHex.Text = ColorToHex(palColor.Color);
        }

        private void UpdateSelectedColorFromHex()
        {
            if (selectedColor == null) return;

            try
            {
                var newColor = HexToColor(txtHex.Text);
                selectedColor.Color = newColor;
                selectedColorPanel.BackColor = newColor;
                // Обновляем отображение в соответствующем ColorBoxControl
                foreach (ColorBoxControl box in colorsPanel.Controls)
                {
                    if (box.PaletteColor == selectedColor)
                    {
                        box.UpdateColor(newColor);
                        break;
                    }
                }
            }
            catch
            {
                MessageBox.Show("Invalid HEX format. Use #RRGGBB or RRGGBB", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private string ColorToHex(Color color) =>
            $"#{color.R:X2}{color.G:X2}{color.B:X2}";

        private Color HexToColor(string hex)
        {
            hex = hex.Replace("#", "").Trim();

            if (hex.Length != 6)
                throw new ArgumentException("Invalid HEX format. Use #RRGGBB or RRGGBB");

            return Color.FromArgb(
                Convert.ToInt32(hex.Substring(0, 2), 16),
                Convert.ToInt32(hex.Substring(2, 2), 16),
                Convert.ToInt32(hex.Substring(4, 2), 16));
        }

    }
}
