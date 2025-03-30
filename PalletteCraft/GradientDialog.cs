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
            Size = new Size(500, 300);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            StartPosition = FormStartPosition.CenterParent;

            startColorPanel = new Panel
            {
                Size = new Size(60, 60),
                BackColor = Color.RoyalBlue,
                BorderStyle = BorderStyle.FixedSingle,
                Location = new Point(20, 20),
                Cursor = Cursors.Hand
            };

            endColorPanel = new Panel
            {
                Size = new Size(60, 60),
                BackColor = Color.LightSkyBlue,
                BorderStyle = BorderStyle.FixedSingle,
                Location = new Point(100, 20),
                Cursor = Cursors.Hand
            };

            lblSteps = new Label
            {
                Text = "Steps:",
                Location = new Point(180, 30),
                AutoSize = true
            };

            stepsCount = new NumericUpDown
            {
                Minimum = 2,
                Maximum = 50,
                Value = 5,
                Width = 50,
                Location = new Point(220, 28)
            };

            gradientPreview = new Panel
            {
                Size = new Size(440, 60),
                Location = new Point(20, 100),
                BorderStyle = BorderStyle.FixedSingle
            };

            btnAccept = new Button
            {
                Text = "Generate",
                Size = new Size(100, 30),
                Location = new Point(280, 200)
            };

            btnCancel = new Button
            {
                Text = "Cancel",
                Size = new Size(100, 30),
                Location = new Point(390, 200)
            };

            Controls.AddRange(new Control[]
            {
                startColorPanel,
                endColorPanel,
                lblSteps,
                stepsCount,
                gradientPreview,
                btnAccept,
                btnCancel
            });
        }

        private void SetupEventHandlers()
        {
            startColorPanel.Click += (s, e) => PickColor(startColorPanel);
            endColorPanel.Click += (s, e) => PickColor(endColorPanel);
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
            using var colorDialog = new ColorDialog { Color = panel.BackColor };
            if (colorDialog.ShowDialog() == DialogResult.OK)
            {
                panel.BackColor = colorDialog.Color;
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

        private void DrawGradientPreview(Graphics graphics)
        {
            if (GeneratedColors == null || GeneratedColors.Count == 0) return;
            float stepWidth = gradientPreview.Width / (float)GeneratedColors.Count;
            for (int i = 0; i < GeneratedColors.Count; i++)
            {
                using var brush = new SolidBrush(GeneratedColors[i])
                {
                    // Возможна будущая анимация или дополнительные эффекты
                };
                graphics.FillRectangle(brush, i * stepWidth, 0, stepWidth, gradientPreview.Height);
            }
        }
    }
}
