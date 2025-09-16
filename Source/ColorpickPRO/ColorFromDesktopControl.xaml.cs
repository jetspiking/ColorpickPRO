using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace ColorpickPRO
{
    public partial class ColorFromDesktopControl : UserControl, IOnColorPickedListener
    {
        private IOnColorPickedListener? _onColorPicked;

        private const Int32 WH_MOUSE_LL = 14;
        private IntPtr _hookID = IntPtr.Zero;
        private LowLevelMouseProc _proc;
        private Boolean _suppressNextMouseUp = false;
        private System.Windows.Media.Color? _selectedColorValue;


        public ColorFromDesktopControl()
        {
            InitializeComponent();
            _proc = HookCallback;
        }

        public void SetListener(IOnColorPickedListener onColorPicked)
        {
            this._onColorPicked = onColorPicked;
        }

        private void OnPickColorClick(Object sender, RoutedEventArgs e)
        {
            Mouse.OverrideCursor = Cursors.Cross; // Change the cursor to a crosshair
            _hookID = SetHook(_proc);
        }

        private IntPtr SetHook(LowLevelMouseProc proc)
        {
            using (System.Diagnostics.Process curProcess = System.Diagnostics.Process.GetCurrentProcess())
            using (System.Diagnostics.ProcessModule curModule = curProcess.MainModule)
            {
                return SetWindowsHookEx(WH_MOUSE_LL, proc, GetModuleHandle(curModule.ModuleName), 0);
            }
        }

        private delegate IntPtr LowLevelMouseProc(Int32 nCode, IntPtr wParam, IntPtr lParam);

        private IntPtr HookCallback(Int32 nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0)
            {
                if (MouseMessages.WM_LBUTTONDOWN == (MouseMessages)wParam)
                {
                    Mouse.OverrideCursor = null;
                    UnhookWindowsHookEx(_hookID);

                    System.Drawing.Color pickedColor = GetColorUnderMouse();
                    _onColorPicked?.OnColorPicked(pickedColor);

                    _suppressNextMouseUp = true; // Set the flag to suppress the next mouse up event
                    return (IntPtr)1; // Prevent this event from being passed to other applications
                }
                else if (_suppressNextMouseUp && MouseMessages.WM_LBUTTONUP == (MouseMessages)wParam)
                {
                    _suppressNextMouseUp = false; // Reset the flag for future use
                    return (IntPtr)1; // Prevent this event from being passed to other applications
                }
            }

            return CallNextHookEx(_hookID, nCode, wParam, lParam);
        }



        private System.Drawing.Color GetColorUnderMouse()
        {
            POINT mousePoint;
            GetCursorPos(out mousePoint);

            using (var screenBitmap = new Bitmap(1, 1, System.Drawing.Imaging.PixelFormat.Format32bppArgb))
            {
                using (var g = System.Drawing.Graphics.FromImage(screenBitmap))
                {
                    g.CopyFromScreen(mousePoint.X, mousePoint.Y, 0, 0, new System.Drawing.Size(1, 1));
                }
                var dotNetColor = screenBitmap.GetPixel(0, 0);
                return System.Drawing.Color.FromArgb(dotNetColor.A, dotNetColor.R, dotNetColor.G, dotNetColor.B);
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct POINT
        {
            public Int32 X;
            public Int32 Y;

            public POINT(Int32 x, Int32 y)
            {
                this.X = x;
                this.Y = y;
            }
        }

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern Boolean GetCursorPos(out POINT lpPoint);

        private enum MouseMessages
        {
            WM_LBUTTONDOWN = 0x0201,
            WM_LBUTTONUP = 0x0202
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(Int32 idHook, LowLevelMouseProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern Boolean UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, Int32 nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(String lpModuleName);

        private void OnCopyClick(Object sender, RoutedEventArgs e)
        {
            Clipboard.SetText("#"+_selectedColorValue.ToString().Substring(3));
        }

        public void OnColorPicked(System.Drawing.Color color)
        {
            _selectedColorValue = System.Windows.Media.Color.FromArgb(color.A, color.R, color.G, color.B);
        }
    }
}
