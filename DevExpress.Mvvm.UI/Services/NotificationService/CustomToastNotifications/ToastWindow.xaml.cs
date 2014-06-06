using System;
using System.Runtime.InteropServices;
using System.Security;
using System.Windows;
using System.Windows.Interop;

namespace DevExpress.Mvvm.UI.Native {
    public partial class ToastWindow : Window {
        public ToastWindow() {
            Loaded += ToastWindow_Loaded;
            InitializeComponent();
        }
        [SecuritySafeCritical]
        void ToastWindow_Loaded(object sender, RoutedEventArgs e) {
            var wndHelper = new WindowInteropHelper(this);
            int exStyle = GetWindowLong(wndHelper.Handle, -20);
            exStyle |= WS_EX_TOOLWINDOW;
            SetWindowLong(wndHelper.Handle, -20, exStyle);
        }
        [DllImport("user32.dll", SetLastError = true)]
        static extern int GetWindowLong(IntPtr hWnd, int nIndex);
        [DllImport("user32.dll")]
        static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);
        const int WS_EX_TOOLWINDOW = 0x00000080;
    }
}