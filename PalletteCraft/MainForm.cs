using PaletteCraft;
using PaletteCraft.Controls;
using PaletteCraft.Services;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace PalletteCraft
{
    public partial class MainForm : Form
    {
        // Существующие элементы управления
        private FlowLayoutPanel colorsPanel;
        private Panel selectedColorPanel;
        private TextBox txtHex;
        private Button btnAddColor, btnDeleteColor, btnClearAll, btnAddHexColor, btnUndo, btnRedo;
        private Button btnGenerateGradient, btnSavePalette, btnLoadPalette;

        // Поле для выбранного цвета
        private PaletteColor selectedColor;

        // Новая коллекция групп
        private BindingList<ColorGroup> groups = new BindingList<ColorGroup>();

        // Константы: ширина цветового блока и максимальное количество колонок (9)
        private const int ColorBoxWidth = 90;
        private const int MaxColumns = 9;

        public MainForm()
        {
            InitializeComponent();
            SetupUI();
            SetupEventHandlers();
            DoubleBuffered = true;
            MinimumSize = new Size(1400, 800);
            // При старте обновляем отображение палитры с группами
            RefreshPaletteDisplay();
            // При изменении размера окна обновляем отображение, чтобы группы корректно занимали строку
            this.Resize += (s, e) => RefreshPaletteDisplay();
        }

        private void SetupUI()
        {
            Text = "🎨 Palette Craft";
            UIManager.StyleForm(this);
            Padding = new Padding(20);

            // Главный контейнер делим на 2 части:
            //  - левая часть: отображение цветов (с группами и неотнесёнными)
            //  - правая часть: панель управления
            var mainLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                ColumnStyles =
                {
                    new ColumnStyle(SizeType.Percent, 80),
                    new ColumnStyle(SizeType.Percent, 20)
                }
            };

            // Панель для отображения палитры (группы и неотнесённые цвета)
            colorsPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                WrapContents = true,
                Padding = new Padding(15),
                BackColor = UIManager.PanelColor
            };

            // Панель управления (правая часть)
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
                    new RowStyle(SizeType.Absolute, 180), // Панель выбранного цвета
                    new RowStyle(SizeType.Absolute, 40),  // Поле HEX
                    new RowStyle(SizeType.Absolute, 40),  // Add Color
                    new RowStyle(SizeType.Absolute, 40),  // Add HEX Color
                    new RowStyle(SizeType.Absolute, 40),  // Delete Color
                    new RowStyle(SizeType.Absolute, 40),  // Clear All
                    new RowStyle(SizeType.Absolute, 40),  // Generate Gradient
                    new RowStyle(SizeType.Absolute, 40),  // Save Palette
                    new RowStyle(SizeType.Absolute, 40),  // Undo/Redo
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
            btnUndo = UIManager.CreateStyledButton("Undo", "↩");
            btnRedo = UIManager.CreateStyledButton("Redo", "↪");

            controlLayout.Controls.Add(selectedColorPanel, 0, 0);
            controlLayout.Controls.Add(txtHex, 0, 1);
            controlLayout.Controls.Add(btnAddColor, 0, 2);
            controlLayout.Controls.Add(btnAddHexColor, 0, 3);
            controlLayout.Controls.Add(btnDeleteColor, 0, 4);
            controlLayout.Controls.Add(btnClearAll, 0, 5);
            controlLayout.Controls.Add(btnGenerateGradient, 0, 6);
            controlLayout.Controls.Add(btnSavePalette, 0, 7);

            // Панель для Undo/Redo
            var undoRedoPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                ColumnStyles ={new ColumnStyle(SizeType.Percent, 50), new ColumnStyle(SizeType.Percent, 50) }
            };
            undoRedoPanel.Controls.Add(btnUndo);
            undoRedoPanel.Controls.Add(btnRedo);
            controlLayout.Controls.Add(undoRedoPanel, 0, 8);
            controlLayout.Controls.Add(btnLoadPalette, 0, 9);
            btnUndo.Dock = DockStyle.Fill;
            btnRedo.Dock = DockStyle.Fill;
            controlPanel.Controls.Add(controlLayout);
            mainLayout.Controls.Add(colorsPanel, 0, 0);
            mainLayout.Controls.Add(controlPanel, 1, 0);
            Controls.Add(mainLayout);
        }

        private void SetupEventHandlers()
        {
            // Обработчики для основных кнопок
            btnAddColor.Click += (s, e) =>
            {
                using var dlg = new ColorDialog();
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    PaletteService.AddColor(dlg.Color);
                    RefreshPaletteDisplay();
                }
            };

            btnUndo.Click += (s, e) => { PaletteService.Undo(); RefreshPaletteDisplay(); };
            btnRedo.Click += (s, e) => { PaletteService.Redo(); RefreshPaletteDisplay(); };

            btnDeleteColor.Click += (s, e) =>
            {
                if (selectedColor != null)
                {
                    PaletteService.DeleteColor(selectedColor);
                    // Удаляем цвет из групп, если он там есть
                    foreach (var grp in groups)
                    {
                        var item = grp.Colors.FirstOrDefault(c => c.ToString() == selectedColor.ToString());
                        if (item != null)
                            grp.Colors.Remove(item);
                    }
                    selectedColor = null;
                    selectedColorPanel.BackColor = UIManager.ButtonColor;
                    txtHex.Text = "";
                    RefreshPaletteDisplay();
                }
            };

            btnClearAll.Click += (s, e) =>
            {
                PaletteService.ClearColors();
                foreach (var grp in groups)
                    grp.Colors.Clear();
                selectedColor = null;
                selectedColorPanel.BackColor = UIManager.ButtonColor;
                txtHex.Text = "";
                RefreshPaletteDisplay();
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
                    RefreshPaletteDisplay();
                }
            };

            btnAddHexColor.Click += (s, e) =>
            {
                try
                {
                    var color = HexToColor(txtHex.Text);
                    PaletteService.AddColor(color);
                    RefreshPaletteDisplay();
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
                    await PaletteService.SavePaletteAsync(dialog.FileName, System.IO.Path.GetFileNameWithoutExtension(dialog.FileName));
                }
            };

            btnLoadPalette.Click += async (s, e) =>
            {
                using var dialog = new OpenFileDialog { Filter = "GIMP Palette|*.gpl" };
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    await PaletteService.LoadPaletteAsync(dialog.FileName);
                    RefreshPaletteDisplay();
                }
            };

            txtHex.KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Enter)
                {
                    UpdateSelectedColorFromHex();
                }
            };

            // Горячая клавиша для создания новой группы (Ctrl+G)
            this.KeyPreview = true;
            this.KeyDown += (s, e) =>
            {
                if (e.Control && e.KeyCode == Keys.G)
                {
                    groups.Add(new ColorGroup { Name = $"Group {groups.Count + 1}" });
                    RefreshPaletteDisplay();
                }
            };
        }

        /// <summary>
        /// Обновляет отображение палитры с группами.
        /// Сначала выводятся панели для каждой группы (с редактируемым заголовком, кнопкой удаления и цветами),
        /// затем — панель для неотнесённых цветов.
        /// Каждая группа занимает всю ширину контейнера, а внутри группы цвета располагаются в строку с переносом,
        /// где в каждой строке максимум 9 блоков.
        /// </summary>
        private void RefreshPaletteDisplay()
        {
            colorsPanel.Controls.Clear();

            // Вывод групп
            foreach (var group in groups.ToList()) // ToList для безопасной итерации
            {
                // Панель группы с рамкой и отступами
                var groupPanel = new Panel
                {
                    AutoSize = true,
                    BorderStyle = BorderStyle.FixedSingle,
                    Margin = new Padding(5),
                    Padding = new Padding(5)
                };
                // Ограничиваем максимальную ширину, чтобы группа занимала одну строку
                groupPanel.MaximumSize = new Size(colorsPanel.ClientSize.Width, 0);

                // Заголовок группы – панель с редактируемым названием и кнопкой удаления
                var headerPanel = new FlowLayoutPanel
                {
                    Dock = DockStyle.Top,
                    AutoSize = true,
                    FlowDirection = FlowDirection.LeftToRight
                };

                var headerLabel = new Label
                {
                    Text = group.ToString(), // например, "Group 1 (3 colors)"
                    AutoSize = true,
                    BackColor = UIManager.PanelColor,
                    ForeColor = UIManager.TextColor,
                    Padding = new Padding(3)
                };

                var txtGroupName = new TextBox
                {
                    Text = group.Name,
                    Visible = false,
                    AutoSize = true
                };

                // Кнопка удаления группы – при удалении группы её цвета становятся неотнесёнными
                var btnDeleteGroup = new Button
                {
                    Text = "X",
                    AutoSize = true,
                    BackColor = Color.IndianRed,
                    ForeColor = Color.White,
                    Margin = new Padding(5, 0, 0, 0)
                };
                btnDeleteGroup.Click += (s, e) =>
                {
                    if (MessageBox.Show($"Удалить группу \"{group.Name}\"? Все её цвета будут перенесены в неотнесённые.",
                        "Подтверждение", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                    {
                        groups.Remove(group);
                        RefreshPaletteDisplay();
                    }
                };

                // Редактирование имени по двойному клику
                headerLabel.DoubleClick += (s, e) =>
                {
                    headerLabel.Visible = false;
                    txtGroupName.Visible = true;
                    txtGroupName.Focus();
                };

                txtGroupName.KeyDown += (s, e) =>
                {
                    if (e.KeyCode == Keys.Enter)
                        SaveGroupName();
                };
                txtGroupName.Leave += (s, e) => SaveGroupName();

                void SaveGroupName()
                {
                    string newName = txtGroupName.Text.Trim();
                    if (!string.IsNullOrEmpty(newName))
                    {
                        group.Name = newName;
                        headerLabel.Text = group.ToString();
                    }
                    txtGroupName.Visible = false;
                    headerLabel.Visible = true;
                }

                headerPanel.Controls.Add(headerLabel);
                headerPanel.Controls.Add(txtGroupName);
                headerPanel.Controls.Add(btnDeleteGroup);
                groupPanel.Controls.Add(headerPanel);

                // Панель для цветов внутри группы – цвета располагаются в строку с переносом
                var groupColorsPanel = new FlowLayoutPanel
                {
                    Dock = DockStyle.Fill,
                    AutoSize = true,
                    FlowDirection = FlowDirection.LeftToRight,
                    WrapContents = true,
                    BackColor = UIManager.ButtonColor,
                    Padding = new Padding(5),
                    // Ограничиваем ширину панели так, чтобы в одной строке было максимум 9 блоков
                    MaximumSize = new Size(MaxColumns * ColorBoxWidth, 0)
                };

                // Если группа пуста, устанавливаем минимальный размер для удобства перетаскивания
                if (!group.Colors.Any())
                    groupColorsPanel.MinimumSize = new Size(ColorBoxWidth, ColorBoxWidth);

                // Разрешаем drag & drop в группу
                groupColorsPanel.AllowDrop = true;
                groupColorsPanel.DragEnter += (s, e) =>
                {
                    if (e.Data.GetDataPresent(typeof(PaletteColor)))
                        e.Effect = DragDropEffects.Copy;
                };
                groupColorsPanel.DragDrop += (s, e) =>
                {
                    if (e.Data.GetData(typeof(PaletteColor)) is PaletteColor color)
                    {
                        if (!group.Colors.Any(c => c.ToString() == color.ToString()))
                        {
                            group.Colors.Add(color);
                            headerLabel.Text = group.ToString();
                            RefreshPaletteDisplay();
                        }
                    }
                };

                // Заполняем панель группы цветами
                foreach (var color in group.Colors)
                {
                    var box = new ColorBoxControl(color);
                    box.ColorSelected += (s, palColor) => SelectColor(palColor);
                    box.MouseDown += (s, e) =>
                    {
                        if (e.Button == MouseButtons.Left)
                            box.DoDragDrop(color, DragDropEffects.Copy);
                    };
                    groupColorsPanel.Controls.Add(box);
                }
                groupPanel.Controls.Add(groupColorsPanel);
                colorsPanel.Controls.Add(groupPanel);
            }

            // Вывод неотнесённых цветов – ограничиваем ширину панели так, чтобы в строке было не более 9 блоков
            var ungroupedColors = PaletteService.Colors.Except(groups.SelectMany(g => g.Colors)).ToList();
            if (ungroupedColors.Any())
            {
                var ungroupedPanel = new Panel
                {
                    AutoSize = true,
                    BorderStyle = BorderStyle.FixedSingle,
                    Margin = new Padding(5),
                    Padding = new Padding(5)
                };
                ungroupedPanel.MaximumSize = new Size(colorsPanel.ClientSize.Width - 30, 0);

                var header = new Label
                {
                    Text = "Ungrouped Colors",
                    Dock = DockStyle.Top,
                    BackColor = UIManager.PanelColor,
                    ForeColor = UIManager.TextColor,
                    Padding = new Padding(3),
                    AutoSize = true
                };
                ungroupedPanel.Controls.Add(header);

                var ungroupedColorsPanel = new FlowLayoutPanel
                {
                    Dock = DockStyle.Fill,
                    AutoSize = true,
                    FlowDirection = FlowDirection.LeftToRight,
                    WrapContents = true,
                    BackColor = UIManager.ButtonColor,
                    Padding = new Padding(5),
                    MaximumSize = new Size(MaxColumns * ColorBoxWidth, 0)
                };

                // Добавляем drop-обработку для возврата цвета из групп
                ungroupedColorsPanel.AllowDrop = true;
                ungroupedColorsPanel.DragEnter += (s, e) =>
                {
                    if (e.Data.GetDataPresent(typeof(PaletteColor)))
                        e.Effect = DragDropEffects.Copy;
                };
                ungroupedColorsPanel.DragDrop += (s, e) =>
                {
                    if (e.Data.GetData(typeof(PaletteColor)) is PaletteColor color)
                    {
                        // Если цвет находится в какой-либо группе, удаляем его оттуда
                        foreach (var grp in groups)
                        {
                            var item = grp.Colors.FirstOrDefault(c => c.ToString() == color.ToString());
                            if (item != null)
                                grp.Colors.Remove(item);
                        }
                        RefreshPaletteDisplay();
                    }
                };

                foreach (var color in ungroupedColors)
                {
                    var box = new ColorBoxControl(color);
                    box.ColorSelected += (s, palColor) => SelectColor(palColor);
                    box.MouseDown += (s, e) =>
                    {
                        if (e.Button == MouseButtons.Left)
                            box.DoDragDrop(color, DragDropEffects.Copy);
                    };
                    ungroupedColorsPanel.Controls.Add(box);
                }
                ungroupedPanel.Controls.Add(ungroupedColorsPanel);
                colorsPanel.Controls.Add(ungroupedPanel);
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
                RefreshPaletteDisplay();
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
