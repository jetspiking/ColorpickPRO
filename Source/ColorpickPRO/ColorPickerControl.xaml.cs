using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;

namespace ColorpickPRO
{
    public partial class ColorPickerControl : UserControl, IOnColorPickedListener
    {
        private Boolean _isDragging  = false;
        private Double  _selectedHue = 0;
        private List<IOnColorPickedListener> _colorStringListener = new List<IOnColorPickedListener>();

        public ColorPickerControl()
        {
            InitializeComponent();
            UpdateColorFromHSV(0); // default to red
            selectedColorHex.Text = ColorConverter.ColorToHex(selectedColorBrush.Color); // default hex color value
        }

        public void SetListener(IOnColorPickedListener colorStringListener)
        {
            this._colorStringListener.Add(colorStringListener);
        }

        public void SetColor(Color color)
        {
            // Convert RGB to HSV
            var (hue, saturation, value) = ColorConverter.RGBToHSV(color);

            // Update Hue
            _selectedHue = hue;
            UpdateColorFromHSV(hue);

            // Update Saturation and Value
            Point position = new Point(saturation * colorBox.Width, (1 - value) * colorBox.Height);
            Color selectedColor = ColorConverter.ColorFromHSV(hue, saturation, value);
            selectedColorBrush.Color = selectedColor;
            selectedColorHex.Text = ColorConverter.ColorToHex(selectedColor); // Update the hex value when selected
            hoverColorHex.Text = ColorConverter.ColorToHex(selectedColor);

            // Show and position the ellipse
            PositionEllipseAtPoint(position);

            // Update the thumb position on the hue slider
            UpdateHueThumbPosition(hue);
            UpdateEllipsePosition(saturation, value);

            foreach(IOnColorPickedListener listener in _colorStringListener)
                listener.OnColorPicked(System.Drawing.Color.FromArgb(color.A,color.R,color.G,color.B));
        }

        private void UpdateColorFromHSV(Double hue)
        {
            Color newColor = ColorConverter.ColorFromHSV(hue, 1, 1);
            saturationEnd.Color = newColor; // Update the end color for saturation
            hoverColorHex.Text = ColorConverter.ColorToHex(saturationEnd.Color); // Update hex color value on hover
        }

        private void ColorBox_MouseLeftButtonDown(Object sender, MouseButtonEventArgs e)
        {
            UpdateBox(sender, e);
        }

        private void UpdateBox(Object sender, MouseButtonEventArgs e)
        {
            Border border = (Border)sender;
            Point position = e.GetPosition(border);

            Double saturation = position.X / border.Width;
            Double lightness = 1 - (position.Y / border.Height);
            Double hue = this._selectedHue;

            Color selectedColor = ColorConverter.ColorFromHSV(hue, saturation, lightness);
            selectedColorBrush.Color = selectedColor;
            selectedColorHex.Text = ColorConverter.ColorToHex(selectedColor); // Update the hex value when selected
            hoverColorHex.Text = ColorConverter.ColorToHex(selectedColor);

            // Show and position the ellipse
            PositionEllipseAtPoint(position);

            foreach (IOnColorPickedListener listener in _colorStringListener)
                listener.OnColorPicked(System.Drawing.Color.FromArgb(selectedColor.A, selectedColor.R, selectedColor.G, selectedColor.B));
        }

        private void ColorBox_MouseMove(Object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                Point position = e.GetPosition(colorBox);
                Color color = GetColorFromPoint(position);
                hoverColorHex.Text = ColorConverter.ColorToHex(color);

                // Position the ellipse
                PositionEllipseAtPoint(position);
            }
        }

        private void ColorBox_MouseLeftButtonUp(Object sender, MouseButtonEventArgs e)
        {
            UpdateBox(sender, e);
        }

        private Color GetColorFromPoint(Point point)
        {
            Int32 x = (int)point.X;
            Int32 y = (int)point.Y;

            Double saturation = x / 256.0;
            Double value = 1 - (y / 256.0);

            return ColorConverter.ColorFromHSV(this._selectedHue, saturation, value);
        }

        private void HueRectangle_MouseDown(Object sender, MouseButtonEventArgs e)
        {
            UpdateHueRectangleWithEventArgs(sender, e);

            _isDragging = true;
        }

        private void hueRectangle_MouseMove(Object sender, MouseEventArgs e)
        {
            if (_isDragging)
            {
                UpdateHueRectangleWithEventArgs(sender, e);
            }
        }

        private void hueRectangle_MouseUp(Object sender, MouseButtonEventArgs e)
        {
            UpdateHueRectangleWithEventArgs(sender, e);

            _isDragging = false;
        }

        private void PositionEllipseAtPoint(Point position)
        {
            colorPositionEllipse.Visibility = Visibility.Visible;

            // Calculate the top-left position for the ellipse to center it on the clicked position
            Double left = position.X - (colorPositionEllipse.Width / 2);
            Double top = position.Y - (colorPositionEllipse.Height / 2);

            Canvas.SetLeft(colorPositionEllipse, left);
            Canvas.SetTop(colorPositionEllipse, top);
        }

        private void UpdateEllipsePosition(Double saturation, Double lightness)
        {
            Double xPosition = saturation * colorBox.Width;
            Double yPosition = (1 - lightness) * colorBox.Height;

            PositionEllipseAtPoint(new Point(xPosition, yPosition));
        }


        private void UpdateHueRectangleWithEventArgs(Object sender, MouseEventArgs e)
        {
            // Update the thumb's position when the user clicks inside the hue rectangle
            Point mousePosition = e.GetPosition(hueRectangle);

            Double newY = mousePosition.Y - (hueThumbControl.ActualHeight / 2);
            newY = Math.Max(0, Math.Min(hueRectangle.ActualHeight - hueThumbControl.ActualHeight, newY));
            Canvas.SetTop(hueThumbControl, newY);

            Double calculatedPosition = mousePosition.Y - (hueThumbControl.Height / 2);
            if (calculatedPosition < 0)
                hueThumbControl.Margin = new Thickness(0, 0, 0, 0);
            else if (mousePosition.Y > this.hueRectangle.Height - (hueThumbControl.Height/2))
                hueThumbControl.Margin = new Thickness(0, this.hueRectangle.Height - hueThumbControl.Height - 1, 0, 0);
            else
                hueThumbControl.Margin = new Thickness(0, mousePosition.Y - (hueThumbControl.Height / 2), 0, 0);

            // Calculate the selected hue based on the thumb's position
            _selectedHue = (newY / hueRectangle.ActualHeight) * 360;

            UpdateColorFromHSV(_selectedHue);
        }

        private void UpdateHueThumbPosition(Double hue)
        {
            Double huePosition = (hue / 360) * hueRectangle.ActualHeight;
            Double newY = huePosition - (hueThumbControl.Height / 2);
            newY = Math.Max(0, Math.Min(hueRectangle.ActualHeight - hueThumbControl.ActualHeight, newY));

            hueThumbControl.Margin = new Thickness(0, newY, 0, 0);
        }

        private void hueRectangle_MouseLeave(Object sender, MouseEventArgs e)
        {
            this._isDragging = false;
        }

        public void OnColorPicked(System.Drawing.Color color)
        {
            SetColor(Color.FromArgb(color.A,color.R,color.G,color.B));
        }
    }
}
