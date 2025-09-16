using System;
using System.Windows.Media;

namespace ColorpickPRO
{
    public static class ColorConverter
    {
        public static (Double Hue, Double Saturation, Double Value) RGBToHSV(Color color)
        {
            Double delta, min;
            Double h = 0, s, v;

            min = Math.Min(Math.Min(color.R, color.G), color.B);
            v = Math.Max(Math.Max(color.R, color.G), color.B);
            delta = v - min;

            if (v == 0.0)
                s = 0;
            else
                s = delta / v;

            if (s == 0)
                h = 0.0;
            else
            {
                if (color.R == v)
                    h = (color.G - color.B) / delta;
                else if (color.G == v)
                    h = 2 + (color.B - color.R) / delta;
                else if (color.B == v)
                    h = 4 + (color.R - color.G) / delta;

                h *= 60;
                if (h < 0.0)
                    h = h + 360;
            }

            return (h, s, v / 255.0);
        }

        public static Color ColorFromHSV(Double hue, Double saturation, Double value)
        {
            Int32 hi = Convert.ToInt32(Math.Floor(hue / 60)) % 6;
            Double f = hue / 60 - Math.Floor(hue / 60);

            value = value * 255;
            Int32 v = Convert.ToInt32(value);
            Int32 p = Convert.ToInt32(value * (1 - saturation));
            Int32 q = Convert.ToInt32(value * (1 - f * saturation));
            Int32 t = Convert.ToInt32(value * (1 - (1 - f) * saturation));

            if (hi == 0)
                return Color.FromArgb(255, (byte)v, (byte)t, (byte)p);
            else if (hi == 1)
                return Color.FromArgb(255, (byte)q, (byte)v, (byte)p);
            else if (hi == 2)
                return Color.FromArgb(255, (byte)p, (byte)v, (byte)t);
            else if (hi == 3)
                return Color.FromArgb(255, (byte)p, (byte)q, (byte)v);
            else if (hi == 4)
                return Color.FromArgb(255, (byte)t, (byte)p, (byte)v);
            else
                return Color.FromArgb(255, (byte)v, (byte)p, (byte)q);
        }

        public static String ColorToHex(Color color)
        {
            return $"#{color.R:X2}{color.G:X2}{color.B:X2}";
        }
    }
}
