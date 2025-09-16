using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace ColorpickPRO
{
    public partial class ColorModelsControl : UserControl, IOnColorPickedListener
    {
        private IOnColorPickedListener? _onColorPickedListener;

        private interface IColorModel
        {
            Color GetColor(String color);
            String SetColor(Color color);
        }

        private class RGB : IColorModel
        {
            public Color GetColor(String color)
            {
                if (String.IsNullOrEmpty(color))
                    throw new ArgumentException("Color String cannot be null or empty.");

                String[] parts = color.Split(',');

                if (parts.Length != 3)
                    throw new ArgumentException("Color String is in an incorrect format.");

                Byte r, g, b;

                if (!byte.TryParse(parts[0].Trim(), out r) ||
                    !byte.TryParse(parts[1].Trim(), out g) ||
                    !byte.TryParse(parts[2].Trim(), out b))
                    throw new ArgumentException("Invalid Byte value in color String.");

                return Color.FromRgb(r, g, b);
            }

            public String SetColor(Color color)
            {
                return $"{color.R},{color.G},{color.B}";
            }
        }

        private class CMYK : IColorModel
        {
            public Color GetColor(String color)
            {
                if (String.IsNullOrEmpty(color))
                    throw new ArgumentException("Color String cannot be null or empty.");

                String[] parts = color.Split(',');

                if (parts.Length != 4)
                    throw new ArgumentException("Color String is in an incorrect format.");

                Double c, m, y, k;

                if (!Double.TryParse(parts[0].Trim().TrimEnd('%'), out c) ||
                    !Double.TryParse(parts[1].Trim().TrimEnd('%'), out m) ||
                    !Double.TryParse(parts[2].Trim().TrimEnd('%'), out y) ||
                    !Double.TryParse(parts[3].Trim().TrimEnd('%'), out k))
                    throw new ArgumentException("Invalid value in color String.");

                // Convert percentage values to the range [0, 1]
                c /= 100.0;
                m /= 100.0;
                y /= 100.0;
                k /= 100.0;

                Byte r = (byte)(255 * (1 - c) * (1 - k));
                Byte g = (byte)(255 * (1 - m) * (1 - k));
                Byte b = (byte)(255 * (1 - y) * (1 - k));

                return Color.FromRgb(r, g, b);
            }

            public String SetColor(Color color)
            {
                Double r = color.R / 255.0;
                Double g = color.G / 255.0;
                Double b = color.B / 255.0;

                Double k = 1 - Math.Max(r, Math.Max(g, b));

                if (k < 1)
                {
                    Double c = (1 - r - k) / (1 - k) * 100.0;  // Multiplied by 100 to convert to percentage
                    Double m = (1 - g - k) / (1 - k) * 100.0;
                    Double y = (1 - b - k) / (1 - k) * 100.0;

                    return $"{Math.Round(c)}%,{Math.Round(m)}%,{Math.Round(y)}%,{Math.Round(k * 100)}%";
                }
                else
                {
                    return $"0%,0%,0%,100%"; // Special case when RGB is (0,0,0)
                }
            }
        }

        private class HSV : IColorModel
        {
            public Color GetColor(String color)
            {
                if (String.IsNullOrEmpty(color))
                    throw new ArgumentException("Color String cannot be null or empty.");

                String[] parts = color.Split(',');

                if (parts.Length != 3)
                    throw new ArgumentException("Color String is in an incorrect format.");

                Double h, s, v;

                if (!Double.TryParse(parts[0].Trim().TrimEnd('°'), out h) ||
                    !Double.TryParse(parts[1].Trim().TrimEnd('%'), out s) ||
                    !Double.TryParse(parts[2].Trim().TrimEnd('%'), out v))
                    throw new ArgumentException("Invalid value in color String.");

                s /= 100.0; // Convert to range [0, 1]
                v /= 100.0;

                Int32 hi = Convert.ToInt32(Math.Floor(h / 60.0)) % 6;
                Double f = h / 60.0 - Math.Floor(h / 60.0);

                Double p = v * (1.0 - s);
                Double q = v * (1.0 - f * s);
                Double t = v * (1.0 - (1.0 - f) * s);

                switch (hi)
                {
                    case 0:
                        return Color.FromRgb((byte)(v * 255), (byte)(t * 255), (byte)(p * 255));
                    case 1:
                        return Color.FromRgb((byte)(q * 255), (byte)(v * 255), (byte)(p * 255));
                    case 2:
                        return Color.FromRgb((byte)(p * 255), (byte)(v * 255), (byte)(t * 255));
                    case 3:
                        return Color.FromRgb((byte)(p * 255), (byte)(q * 255), (byte)(v * 255));
                    case 4:
                        return Color.FromRgb((byte)(t * 255), (byte)(p * 255), (byte)(v * 255));
                    default:
                        return Color.FromRgb((byte)(v * 255), (byte)(p * 255), (byte)(q * 255));
                }
            }

            public String SetColor(Color color)
            {
                Double r = color.R / 255.0;
                Double g = color.G / 255.0;
                Double b = color.B / 255.0;

                Double max = Math.Max(r, Math.Max(g, b));
                Double min = Math.Min(r, Math.Min(g, b));
                Double diff = max - min;
                Double h = 0;
                Double s = (max == 0 ? 0 : diff / max);
                Double v = max;

                if (max == min)
                {
                    h = 0;
                }
                else
                {
                    if (max == r)
                    {
                        h = (60 * ((g - b) / diff) + 360) % 360;
                    }
                    else if (max == g)
                    {
                        h = (60 * ((b - r) / diff) + 120) % 360;
                    }
                    else if (max == b)
                    {
                        h = (60 * ((r - g) / diff) + 240) % 360;
                    }
                }

                return $"{Math.Round(h)}°,{Math.Round(s * 100)}%,{Math.Round(v * 100)}%";
            }
        }

        private class HSL : IColorModel
        {
            public Color GetColor(String color)
            {
                if (String.IsNullOrEmpty(color))
                    throw new ArgumentException("Color String cannot be null or empty.");

                String[] parts = color.Split(',');

                if (parts.Length != 3)
                    throw new ArgumentException("Color String is in an incorrect format.");

                Double h, s, l;

                if (!Double.TryParse(parts[0].Trim().TrimEnd('°'), out h) ||
                    !Double.TryParse(parts[1].Trim().TrimEnd('%'), out s) ||
                    !Double.TryParse(parts[2].Trim().TrimEnd('%'), out l))
                    throw new ArgumentException("Invalid value in color String.");

                s /= 100.0; // Convert to range [0, 1]
                l /= 100.0;
                h /= 360.0; // Normalize hue to be between 0 and 1

                Double q = l < 0.5 ? l * (1 + s) : l + s - l * s;
                Double p = 2 * l - q;

                Double r = hue2rgb(p, q, h + 1.0 / 3.0);
                Double g = hue2rgb(p, q, h);
                Double b = hue2rgb(p, q, h - 1.0 / 3.0);

                return Color.FromRgb((byte)(r * 255), (byte)(g * 255), (byte)(b * 255));
            }

            private Double hue2rgb(Double p, Double q, Double t)
            {
                if (t < 0) t += 1;
                if (t > 1) t -= 1;
                if (t < 1.0 / 6.0) return p + (q - p) * 6.0 * t;
                if (t < 1.0 / 2.0) return q;
                if (t < 2.0 / 3.0) return p + (q - p) * (2.0 / 3.0 - t) * 6.0;
                return p;
            }

            public String SetColor(Color color)
            {
                Double r = color.R / 255.0;
                Double g = color.G / 255.0;
                Double b = color.B / 255.0;

                Double max = Math.Max(r, Math.Max(g, b));
                Double min = Math.Min(r, Math.Min(g, b));
                Double diff = max - min;

                Double h = 0;
                Double s = 0;
                Double l = (max + min) / 2.0;

                if (max != min)
                {
                    s = l > 0.5 ? diff / (2.0 - max - min) : diff / (max + min);
                    if (max == r)
                    {
                        h = (g - b) / diff + (g < b ? 6 : 0);
                    }
                    else if (max == g)
                    {
                        h = (b - r) / diff + 2;
                    }
                    else if (max == b)
                    {
                        h = (r - g) / diff + 4;
                    }
                    h = (h / 6.0) % 1.0; // Ensure h is between 0 and 1
                }

                return $"{Math.Round(h * 360)}°,{Math.Round(s * 100)}%,{Math.Round(l * 100)}%";
            }
        }



        public ColorModelsControl()
        {
            InitializeComponent();
        }

        public void SetListener(IOnColorPickedListener listener)
        {
            _onColorPickedListener = listener;
        }

        private void RGB_KeyDown(Object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                Color color = new RGB().GetColor(rgbTextBox.Text);
                _onColorPickedListener?.OnColorPicked(System.Drawing.Color.FromArgb(color.A, color.R, color.G, color.B));
            }
        }

        private void CMYK_KeyDown(Object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                Color color = new CMYK().GetColor(cmykTextBox.Text);
                _onColorPickedListener?.OnColorPicked(System.Drawing.Color.FromArgb(color.A, color.R, color.G, color.B));
            }
        }

        private void HSL_KeyDown(Object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                Color color = new HSL().GetColor(hslTextBox.Text);
                _onColorPickedListener?.OnColorPicked(System.Drawing.Color.FromArgb(color.A, color.R, color.G, color.B));
            }
        }

        private void HSV_KeyDown(Object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                Color color = new HSV().GetColor(hsvTextBox.Text);
                _onColorPickedListener?.OnColorPicked(System.Drawing.Color.FromArgb(color.A, color.R, color.G, color.B));
            }
        }

        public void OnColorPicked(System.Drawing.Color color)
        {
            rgbTextBox.Text = new RGB().SetColor(Color.FromArgb(color.A, color.R, color.G, color.B));
            cmykTextBox.Text = new CMYK().SetColor(Color.FromArgb(color.A, color.R, color.G, color.B));
            hsvTextBox.Text = new HSV().SetColor(Color.FromArgb(color.A, color.R, color.G, color.B));
            hslTextBox.Text = new HSL().SetColor(Color.FromArgb(color.A, color.R, color.G, color.B));
        }
    }
}
