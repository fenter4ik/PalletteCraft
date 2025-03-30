using PaletteCraft;
using PaletteCraft.Services;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace PalletteCraft
{
    public partial class GradientDialog : Form
    {
        public List<Color> GeneratedColors { get; private set; } = new List<Color>();

        private Panel startColorPanel;
        private Panel endColorPanel;
        private Panel gradientPreview;
        private NumericUpDown stepsCount;
        private Label lblSteps;
        private Button btnAccept;
        private Button btnCancel;
        private TextBox txtHexStart;
        private TextBox txtHexEnd;

        public GradientDialog()
        {
            InitializeComponent();
            SetupUI();
            SetupEventHandlers();
            GenerateGradientPreview();
        }

        private void SetupUI()
        {
            Text = "Gradient Generator";
            Size = new Size(600, 400);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            StartPosition = FormStartPosition.CenterParent;
            BackColor = UIManager.BackgroundColor;
            Font = UIManager.DefaultFont;

            // Labels
            Label lblStart = new Label
            {
                Text = "Start Color:",
                Location = new Point(30, 20),
                AutoSize = true,
                ForeColor = UIManager.TextColor
            };

            Label lblEnd = new Label
            {
                Text = "End Color:",
                Location = new Point(220, 20),
                AutoSize = true,
                ForeColor = UIManager.TextColor
            };

            Label lblHexStart = new Label
            {
                Text = "",
                Location = new Point(30, 90),
                AutoSize = true,
                ForeColor = UIManager.TextColor
            };

            Label lblHexEnd = new Label
            {
                Text = "",
                Location = new Point(220, 90),
                AutoSize = true,
                ForeColor = UIManager.TextColor
            };

            // Color panels
            startColorPanel = new Panel
            {
                Size = new Size(80, 60),
                BackColor = Color.RoyalBlue,
                BorderStyle = BorderStyle.FixedSingle,
                Location = new Point(30, 40),
                Cursor = Cursors.Hand
            };

            endColorPanel = new Panel
            {
                Size = new Size(80, 60),
                BackColor = Color.LightSkyBlue,
                BorderStyle = BorderStyle.FixedSingle,
                Location = new Point(220, 40),
                Cursor = Cursors.Hand
            };

            // TextBoxes for HEX
            txtHexStart = new TextBox
            {
                Location = new Point(30, 110),
                Width = 80,
                Text = ColorToHex(startColorPanel.BackColor),
                TextAlign = HorizontalAlignment.Center,
                BackColor = UIManager.ButtonColor,
                ForeColor = UIManager.TextColor
            };

            txtHexEnd = new TextBox
            {
                Location = new Point(220, 110),
                Width = 80,
                Text = ColorToHex(endColorPanel.BackColor),
                TextAlign = HorizontalAlignment.Center,
                BackColor = UIManager.ButtonColor,
                ForeColor = UIManager.TextColor
            };

            // Steps
            lblSteps = new Label
            {
                Text = "Steps:",
                Location = new Point(330, 50),
                AutoSize = true,
                ForeColor = UIManager.TextColor
            };

            stepsCount = new NumericUpDown
            {
                Minimum = 2,
                Maximum = 50,
                Value = 5,
                Width = 60,
                Location = new Point(390, 45),
                BackColor = UIManager.ButtonColor,
                ForeColor = UIManager.TextColor
            };

            // Preview panel
            gradientPreview = new Panel
            {
                Size = new Size(540, 100),
                Location = new Point(30, 150),
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = UIManager.PanelColor
            };

            // Buttons
            btnAccept = UIManager.CreateStyledButtonBottom("Generate", "");
            btnAccept.Size = new Size(120, 35);
            btnAccept.Location = new Point(150, 280);

            btnCancel = UIManager.CreateStyledButtonBottom("Cancel", "");
            btnCancel.Size = new Size(120, 35);
            btnCancel.Location = new Point(310, 280);

            // Controls
            Controls.AddRange(new Control[]
            {
        lblStart, startColorPanel,
        lblEnd, endColorPanel,
        lblHexStart, txtHexStart,
        lblHexEnd, txtHexEnd,
        lblSteps, stepsCount,
        gradientPreview,
        btnAccept, btnCancel
            });
        }


        private void SetupEventHandlers()
        {

            startColorPanel.Click += (s, e) => PickColor(startColorPanel);
            endColorPanel.Click += (s, e) => PickColor(endColorPanel);
            txtHexStart.KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Enter)
                {
                    try
                    {
                        startColorPanel.BackColor = HexToColor(txtHexStart.Text);
                        GenerateGradientPreview();
                    }
                    catch
                    {
                        MessageBox.Show("Invalid HEX format", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            };

            txtHexEnd.KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Enter)
                {
                    try
                    {
                        endColorPanel.BackColor = HexToColor(txtHexEnd.Text);
                        GenerateGradientPreview();
                    }
                    catch
                    {
                        MessageBox.Show("Invalid HEX format", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            };

            btnAccept.Click += (s, e) =>
            {
                GenerateGradientPreview();
                DialogResult = DialogResult.OK;
            };
            btnCancel.Click += (s, e) => DialogResult = DialogResult.Cancel;
            stepsCount.ValueChanged += (s, e) => GenerateGradientPreview();
            gradientPreview.Paint += (s, e) => DrawGradientPreview(e.Graphics);
        }

        private void PickColor(Panel panel)
        {
            using var dlg = new ColorDialog();
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                panel.BackColor = dlg.Color;
                if (panel == startColorPanel) txtHexStart.Text = ColorToHex(dlg.Color);
                if (panel == endColorPanel) txtHexEnd.Text = ColorToHex(dlg.Color);
                GenerateGradientPreview();
            }
        }


        private void GenerateGradientPreview()
        {
            GeneratedColors = ColorGenerator.CreateSmoothGradient(
                startColorPanel.BackColor,
                endColorPanel.BackColor,
                (int)stepsCount.Value);
            gradientPreview.Invalidate();
        }
        private string ColorToHex(Color color) =>
    $"#{color.R:X2}{color.G:X2}{color.B:X2}";

        private Color HexToColor(string hex)
        {
            hex = hex.Replace("#", "").Trim();
            if (hex.Length != 6)
                throw new ArgumentException("Invalid HEX format");

            return Color.FromArgb(
                Convert.ToInt32(hex.Substring(0, 2), 16),
                Convert.ToInt32(hex.Substring(2, 2), 16),
                Convert.ToInt32(hex.Substring(4, 2), 16));
        }

        private void DrawGradientPreview(Graphics graphics)
        {
            if (GeneratedColors == null || GeneratedColors.Count == 0) return;
            float stepWidth = gradientPreview.Width / (float)GeneratedColors.Count;
            for (int i = 0; i < GeneratedColors.Count; i++)
            {
                using var brush = new SolidBrush(GeneratedColors[i]);
                graphics.FillRectangle(brush, i * stepWidth, 0, stepWidth, gradientPreview.Height);
            }
        }
    }
}
