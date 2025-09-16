using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ColorpickPRO
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, IOnColorPickedListener
    {
        public MainWindow()
        {
            InitializeComponent();
            Title = "COLORPICK - PRO";
            Topmost = true;
            ResizeMode = ResizeMode.CanMinimize;

            ColorFromDesktopControl.SetListener(this as IOnColorPickedListener);
            ColorPickerControl.SetListener(ColorModelsControl);
            ColorPickerControl.SetListener(ColorFromDesktopControl);
            ColorModelsControl.SetListener(ColorPickerControl);

            ColorPickerControl.SetColor(Colors.White);
        }

        public void OnColorPicked(System.Drawing.Color color)
        {
            ColorPickerControl.SetColor(System.Windows.Media.Color.FromArgb(color.A, color.R, color.G, color.B));
        }
    }
}
