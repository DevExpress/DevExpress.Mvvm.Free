using System;
using System.Collections;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;

namespace DevExpress {
    public class TestWindowBase : Window {
        public delegate TestWindow TestWindowCreate();
        public static T GetContainer<T>(TestWindowCreate create) where T : TestWindow {
            return create() as T;
        }

        public static TestWindow GetContainer() {
            return GetContainer<TestWindow>(delegate() { return new TestWindow(); });
        }
    }

    public class TestWindow : ContentControl {
        public static bool IsAllowStaticContainer { get { return true; } }
        [ThreadStatic]
        static Hashtable staticContainers;
        static Hashtable StaticContainers {
            get {
                if(staticContainers == null) staticContainers = new Hashtable();
                return staticContainers;
            }
        }
        static readonly Size sizeSmall = new Size(800, 600), sizeLarge = new Size(850, 600);
        WindowState windowState;
        Size lastSize = sizeSmall;
        public virtual void Show() {
            if(StaticContainers[GetType()] == this) {
                WpfTestWindow.CheckToSkip(WpfTestWindow.GetMethodsToSkip(this));
                SetSize(GetNewSize());
                return;
            }
            CreateHandle();
            SetSize(sizeSmall);
        }
        Size GetNewSize() {
            if(lastSize.Width == sizeLarge.Width)
                lastSize = sizeSmall;
            else
                lastSize = sizeLarge;
            return lastSize;
        }
        Size GetNewSizeMaximized() {
            lastSize.Width += 300;
            lastSize.Height += 300;
            return lastSize;
        }

        public delegate TestWindow TestWindowCreate();
        public static T GetContainer<T>(TestWindowCreate create) where T : TestWindow {
            if(!IsAllowStaticContainer) return create() as T;
            T res = StaticContainers[typeof(T)] as T;
            if(res == null) {
                res = create() as T;
                ClearThemeName(res);
                res.Show();
                StaticContainers[typeof(T)] = res;
            } else {
                res.Content = null;
                ClearThemeName(res);
                res.WindowState = WindowState.Normal;
                if(!double.Equals(double.NaN, res.Width)) {
                    res.ClearValue(FrameworkElement.WidthProperty);
                }
                if(!double.Equals(double.NaN, res.Height)) {
                    res.ClearValue(FrameworkElement.HeightProperty);
                }
            }
            return res;
        }
        static void ClearThemeName(TestWindow window) {
        }
        public static TestWindow GetContainer() {
            return GetContainer<TestWindow>(delegate() { return new TestWindow(); });
        }
        public WindowState WindowState {
            get { return windowState; }
            set {
                if(WindowState == value) return;
                windowState = value;
                if(WindowState == WindowState.Maximized) SetSize(GetNewSizeMaximized());
                if(WindowState == WindowState.Normal) SetSize(GetNewSize());
            }
        }

        public WindowStartupLocation WindowStartupLocation { get; set; }
        public virtual void Close() {
            Content = null;
            if(StaticContainers[GetType()] == this) return;
            if(hwnd != null) {
                hwnd.Dispose();
                this.hwnd = null;
            }
        }
        [System.Runtime.InteropServices.DllImport("user32.dll", CharSet = System.Runtime.InteropServices.CharSet.Auto)]
        static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int x, int y, int Width, int Height, SetWindowPosFlags flags);
        enum SetWindowPosFlags {
            SWP_NOSIZE = 0x0001, SWP_NOMOVE = 0x0002, SWP_NOZORDER = 0x0004, SWP_NOREDRAW = 0x0008, SWP_NOACTIVATE = 0x0010,
            SWP_FRAMECHANGED = 0x0020, SWP_SHOWWINDOW = 0x0040, SWP_HIDEWINDOW = 0x0080, SWP_NOCOPYBITS = 0x0100, SWP_NOOWNERZORDER = 0x0200, SWP_NOSENDCHANGING = 0x0400,
            SWP_DRAWFRAME = 0x0020, SWP_NOREPOSITION = 0x0200, SWP_DEFERERASE = 0x2000, SWP_ASYNCWINDOWPOS = 0x4000
        }

        [System.Security.SecuritySafeCritical]
        void SetSize(Size size) {
            if(hwnd == null) return;
            System.Drawing.Size b = System.Windows.Forms.SystemInformation.FrameBorderSize;
            int c = System.Windows.Forms.SystemInformation.CaptionHeight;
            SetWindowPos(hwnd.Handle, IntPtr.Zero, 0, 0, (int)size.Width - 16 + b.Width * 2, (int)size.Height - 38 + c + b.Height * 2, SetWindowPosFlags.SWP_FRAMECHANGED);
        }
        HwndSource hwnd = null;
        void CreateHandle() {
            if(this.hwnd == null) {
                HwndSourceParameters parameters = new HwndSourceParameters("Hidden Window");
                parameters.SetSize(-2147483648, -2147483648);
                parameters.SetPosition(-2147483648, -2147483648);
                parameters.WindowStyle = 0xcf0000;
                this.hwnd = new HwndSource(parameters);
                this.hwnd.RootVisual = this;
            }

        }

        public void ShowDialog() {
            Show();
        }
        public string Title { get; set; }

        public void Activate() {
        }
    }
}