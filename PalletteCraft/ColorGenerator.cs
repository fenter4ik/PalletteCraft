using System;
using System.Collections.Generic;
using System.Drawing;

namespace PalletteCraft
{
    public static class ColorGenerator
    {
        public static List<Color> CreateSmoothGradient(Color start, Color end, int steps)
        {
            var gradient = new List<Color>();
            if (steps < 2) return gradient;

            for (int i = 0; i < steps; i++)
            {
                float ratio = (float)i / (steps - 1);
                gradient.Add(InterpolateColor(start, end, ratio));
            }

            return gradient;
        }

        private static Color InterpolateColor(Color start, Color end, float ratio)
        {
            // Используем HSL для плавных переходов
            var hslStart = new HSLColor(start);
            var hslEnd = new HSLColor(end);

            float h = Interpolate(hslStart.H, hslEnd.H, ratio);
            float s = Interpolate(hslStart.S, hslEnd.S, ratio);
            float l = Interpolate(hslStart.L, hslEnd.L, ratio);

            return new HSLColor(h, s, l).ToRGB();
        }

        private static float Interpolate(float start, float end, float ratio)
        {
            // Учитываем циклическую природу Hue в HSL
            if (start > end && Math.Abs(start - end) > 0.5f)
            {
                end += 1f;
                if (ratio > 0.5f) ratio -= 1f;
            }

            return start + (end - start) * ratio;
        }
    }

    // Вспомогательный класс для работы с HSL
    public class HSLColor
    {
        public float H { get; set; } // 0-1
        public float S { get; set; } // 0-1
        public float L { get; set; } // 0-1

        public HSLColor(Color color)
        {
            float r = color.R / 255f;
            float g = color.G / 255f;
            float b = color.B / 255f;

            float max = Math.Max(r, Math.Max(g, b));
            float min = Math.Min(r, Math.Min(g, b));

            // Hue
            if (max == min)
            {
                H = 0;
            }
            else if (max == r)
            {
                H = ((g - b) / (max - min)) / 6f;
                if (H < 0) H += 1f;
            }
            else if (max == g)
            {
                H = (2f + (b - r) / (max - min)) / 6f;
            }
            else
            {
                H = (4f + (r - g) / (max - min)) / 6f;
            }

            // Lightness
            L = (max + min) / 2f;

            // Saturation
            if (max == min)
            {
                S = 0;
            }
            else if (L <= 0.5f)
            {
                S = (max - min) / (2f * L);
            }
            else
            {
                S = (max - min) / (2f - 2f * L);
            }
        }

        public HSLColor(float h, float s, float l)
        {
            H = h;
            S = s;
            L = l;
        }

        public Color ToRGB()
        {
            float r, g, b;

            if (S == 0)
            {
                r = g = b = L;
            }
            else
            {
                float q = L < 0.5f ? L * (1 + S) : L + S - L * S;
                float p = 2 * L - q;

                r = HueToRGB(p, q, H + 1f / 3f);
                g = HueToRGB(p, q, H);
                b = HueToRGB(p, q, H - 1f / 3f);
            }

            return Color.FromArgb(
                (int)(r * 255),
                (int)(g * 255),
                (int)(b * 255));
        }

        private float HueToRGB(float p, float q, float t)
        {
            if (t < 0) t += 1f;
            if (t > 1) t -= 1f;
            if (t < 1f / 6f) return p + (q - p) * 6f * t;
            if (t < 1f / 2f) return q;
            if (t < 2f / 3f) return p + (q - p) * (2f / 3f - t) * 6f;
            return p;
        }
    }
}