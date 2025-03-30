using PaletteCraft;
using PaletteCraft.Controls;
using PaletteCraft.Services;
using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace PalletteCraft
{
    public partial class MainForm : Form
    {
        private PaletteColor selectedColor;
        // Используем акцент из UIManager или можно задать другой
        private readonly Color accentColor = UIManager.AccentColor;

        // Элементы управления
        private FlowLayoutPanel colorsPanel;
        private Panel selectedColorPanel;
        private TextBox txtHex;
        private Button btnAddColor, btnDeleteColor, btnClearAll, btnAddHexColor;
        private Button btnGenerateGradient, btnSavePalette, btnLoadPalette;

        public MainForm()
        {
            InitializeComponent(); // Если используется дизайнер, иначе можно удалить
            SetupUI();
            SetupEventHandlers();
            DoubleBuffered = true;
            MinimumSize = new Size(1000, 650);
            RefreshColorBoxes();
        }

        private void SetupUI()
        {
            Text = "🎨 Palette Craft";
            UIManager.StyleForm(this);
            Padding = new Padding(20);

            // Главный контейнер
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

            // Панель для отображения цветов
            colorsPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                WrapContents = true,
                Padding = new Padding(15),
                BackColor = UIManager.PanelColor
            };

            // Панель управления
            var controlPanel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(20)
            };
            UIManager.StylePanel(controlPanel);

            var controlLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                RowCount = 10,
                RowStyles =
                {
                    new RowStyle(SizeType.Absolute, 180), // Selected color
                    new RowStyle(SizeType.Absolute, 20),  // Spacer
                    new RowStyle(SizeType.Absolute, 40),  // HEX поле
                    new RowStyle(SizeType.Absolute, 15),  // Spacer
                    new RowStyle(SizeType.Absolute, 40),
                    new RowStyle(SizeType.Absolute, 40),// Кнопки
                    new RowStyle(SizeType.Absolute, 40),
                    new RowStyle(SizeType.Absolute, 40),
                    new RowStyle(SizeType.Absolute, 40),
                    new RowStyle(SizeType.Percent, 100)
                }
            };

            selectedColorPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = UIManager.ButtonColor,
                BorderStyle = BorderStyle.FixedSingle,
                Padding = new Padding(5)
            };

            txtHex = new TextBox
            {
                Dock = DockStyle.Fill,
                TextAlign = HorizontalAlignment.Center
            };
            UIManager.StyleTextBox(txtHex);

            btnAddColor = UIManager.CreateStyledButton("Add Color", "➕");
            btnAddHexColor = UIManager.CreateStyledButton("Add HEX Color", "#️");
            btnDeleteColor = UIManager.CreateStyledButton("Delete Color", "🗑");
            btnClearAll = UIManager.CreateStyledButton("Clear All", "❌");
            btnGenerateGradient = UIManager.CreateStyledButton("Generate Gradient", "🌓");
            btnSavePalette = UIManager.CreateStyledButton("Save Palette", "💾");
            btnLoadPalette = UIManager.CreateStyledButton("Load Palette", "📂");

            // Добавляем элементы в панель управления
            controlLayout.Controls.Add(selectedColorPanel, 0, 0);
            controlLayout.Controls.Add(txtHex, 0, 2);
            controlLayout.Controls.Add(btnAddColor, 0, 4);
            controlLayout.Controls.Add(btnAddHexColor, 0, 5);
            controlLayout.Controls.Add(btnDeleteColor, 0, 6);
            controlLayout.Controls.Add(btnClearAll, 0, 7);
            controlLayout.Controls.Add(btnGenerateGradient, 0, 8);
            controlLayout.Controls.Add(btnSavePalette, 0, 9);
            controlLayout.Controls.Add(btnLoadPalette, 0, 10);

            controlPanel.Controls.Add(controlLayout);
            mainLayout.Controls.Add(colorsPanel, 0, 0);
            mainLayout.Controls.Add(controlPanel, 1, 0);
            Controls.Add(mainLayout);
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
                    selectedColorPanel.BackColor = UIManager.ButtonColor;
                    txtHex.Text = "";
                    RefreshColorBoxes();
                }
            };

            btnClearAll.Click += (s, e) =>
            {
                PaletteService.ClearColors();
                selectedColor = null;
                selectedColorPanel.BackColor = UIManager.ButtonColor;
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

            btnAddHexColor.Click += (s, e) =>
            {
                try
                {
                    var color = HexToColor(txtHex.Text);
                    PaletteService.AddColor(color);
                    RefreshColorBoxes();
                }
                catch
                {
                    MessageBox.Show("Invalid HEX format. Use #RRGGBB or RRGGBB", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
