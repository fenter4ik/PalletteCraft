using System;
using System.Drawing;
using System.Windows.Forms;

namespace PaletteCraft
{
    public static class UIManager
    {
        // Цветовая палитра тёмной темы
        public static readonly Color BackgroundColor = ColorTranslator.FromHtml("#1E1E1E");
        public static readonly Color PanelColor = ColorTranslator.FromHtml("#2D2D30");
        public static readonly Color ButtonColor = ColorTranslator.FromHtml("#3C3C3C");
        public static readonly Color ButtonHoverColor = ColorTranslator.FromHtml("#505050");
        public static readonly Color AccentColor = ColorTranslator.FromHtml("#007ACC");
        public static readonly Color TextColor = Color.White;
        public static readonly Font DefaultFont = new Font("Segoe UI", 9f);
        public static readonly Font ButtonFont = new Font("Segoe UI", 10, FontStyle.Bold);

        public static void StyleForm(Form form)
        {
            form.BackColor = BackgroundColor;
            form.Font = DefaultFont;
        }

        public static void StylePanel(Panel panel)
        {
            panel.BackColor = PanelColor;
            panel.ForeColor = TextColor;
        }

        public static void StyleTextBox(TextBox textBox)
        {
            textBox.BackColor = ButtonColor;
            textBox.ForeColor = TextColor;
            textBox.BorderStyle = BorderStyle.FixedSingle;
            textBox.Font = new Font("Consolas", 12);
        }

        /// <summary>
        /// Плавно анимирует изменение цвета свойства BackColor у указанного контрола.
        /// </summary>
        public static void AnimateBackColor(Control control, Color targetColor, int duration)
        {
            Color startColor = control.BackColor;
            int steps = duration / 15; // шаг примерно каждые 15 мс
            int currentStep = 0;
            System.Windows.Forms.Timer timer = new System.Windows.Forms.Timer { Interval = 15 };

            timer.Tick += (s, e) =>
            {
                currentStep++;
                float ratio = (float)currentStep / steps;
                int r = (int)(startColor.R + (targetColor.R - startColor.R) * ratio);
                int g = (int)(startColor.G + (targetColor.G - startColor.G) * ratio);
                int b = (int)(startColor.B + (targetColor.B - startColor.B) * ratio);
                control.BackColor = Color.FromArgb(r, g, b);

                if (currentStep >= steps)
                {
                    timer.Stop();
                    timer.Dispose();
                }
            };

            timer.Start();
        }

        public static Button CreateStyledButton(string text, string emoji)
        {
            var btn = new Button
            {
                Text = $"{emoji} {text}",
                Height = 40,
                Dock = DockStyle.Top,
                FlatStyle = FlatStyle.Flat,
                BackColor = ButtonColor,
                ForeColor = TextColor,
                Font = ButtonFont,
                Margin = new Padding(0, 0, 0, 10),
                Cursor = Cursors.Hand
            };
            btn.FlatAppearance.BorderSize = 0;

            // При наведении запускаем анимацию к ButtonHoverColor, при уходе – обратно к ButtonColor.
            btn.MouseEnter += (s, e) => AnimateBackColor(btn, ButtonHoverColor, 200);
            btn.MouseLeave += (s, e) => AnimateBackColor(btn, ButtonColor, 200);

            return btn;
        }
        public static Button CreateStyledButtonBottom(string text, string emoji)
        {
            var btn = new Button
            {
                Text = $"{emoji} {text}",
                Height = 40,
                Dock = DockStyle.Bottom,
                FlatStyle = FlatStyle.Flat,
                BackColor = ButtonColor,
                ForeColor = TextColor,
                Font = ButtonFont,
                Margin = new Padding(0, 0, 0, 10),
                Cursor = Cursors.Hand
            };
            btn.FlatAppearance.BorderSize = 0;

            // При наведении запускаем анимацию к ButtonHoverColor, при уходе – обратно к ButtonColor.
            btn.MouseEnter += (s, e) => AnimateBackColor(btn, ButtonHoverColor, 200);
            btn.MouseLeave += (s, e) => AnimateBackColor(btn, ButtonColor, 200);

            return btn;
        }
    }
}
