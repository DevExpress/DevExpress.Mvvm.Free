using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
#if !SILVERLIGHT
using WindowBase = System.Windows.Window;
#else
using WindowBase = DevExpress.Xpf.Core.DXWindowBase;
#endif

namespace DevExpress.Mvvm.UI.Native {
    public interface IWindowSurrogate {
        WindowBase RealWindow { get; }
        void Show();
#if SILVERLIGHT
        void ShowDialog();
#else
        bool? ShowDialog();
#endif
    }
    public class WindowProxy : IWindowSurrogate {
        public static IWindowSurrogate GetWindowSurrogate(object window) {
            return window as IWindowSurrogate ?? new WindowProxy((WindowBase)window);
        }
        public WindowProxy(WindowBase window) {
            if(window == null) throw new ArgumentNullException("window");
            RealWindow = window;
        }
        public WindowBase RealWindow { get; private set; }
        public void Show() {
            RealWindow.Show();
        }
#if SILVERLIGHT
            public void ShowDialog() {
                RealWindow.ShowDialog();
            }
#else
        public bool? ShowDialog() {
            return RealWindow.ShowDialog();
        }
#endif
    }
}