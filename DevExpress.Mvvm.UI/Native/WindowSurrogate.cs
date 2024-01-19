using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using DevExpress.Mvvm.Native;
using WindowBase = System.Windows.Window;
using System.Windows;

namespace DevExpress.Mvvm.UI.Native {
    public interface IWindowSurrogate {
        WindowBase RealWindow { get; }
        void Show();
        event CancelEventHandler Closing;
        event EventHandler Closed;
        event EventHandler Activated;
        event EventHandler Deactivated;
        bool? ShowDialog();
        void Close();
        bool Activate();
        void Hide();
    }
    public class WindowProxy : IWindowSurrogate {
        [IgnoreDependencyPropertiesConsistencyChecker]
        static readonly DependencyProperty ActualWindowSurrogateProperty =
            DependencyProperty.RegisterAttached("ActualWindowSurrogate", typeof(IWindowSurrogate), typeof(WindowProxy), new PropertyMetadata(null));
        static IWindowSurrogate GetActualWindowSurrogate(DependencyObject obj) {
            return (IWindowSurrogate)obj.GetValue(ActualWindowSurrogateProperty);
        }
        static void SetActualWindowSurrogate(DependencyObject obj, IWindowSurrogate value) {
            obj.SetValue(ActualWindowSurrogateProperty, value);
        }

        public static IWindowSurrogate GetWindowSurrogate(object window) {
            IWindowSurrogate res = (window as DependencyObject).With(x => GetActualWindowSurrogate(x));
            if(res != null)
                return res;
            res = window as IWindowSurrogate ?? new WindowProxy((WindowBase)window);
            (window as DependencyObject).Do(x => SetActualWindowSurrogate(x, res));
            return res;
        }
        public WindowProxy(WindowBase window) {
            if(window == null) throw new ArgumentNullException("window");
            RealWindow = window;
        }
        public WindowBase RealWindow { get; private set; }
        public void Show() {
            RealWindow.Show();
        }
        public void Close() {
            RealWindow.Close();
        }
        public bool? ShowDialog() {
            return RealWindow.ShowDialog();
        }
        public event CancelEventHandler Closing {
            add { RealWindow.Closing += value; }
            remove { RealWindow.Closing -= value; }
        }
        public event EventHandler Closed {
            add { RealWindow.Closed += value; }
            remove { RealWindow.Closed -= value; }
        }
        public event EventHandler Activated {
            add { RealWindow.Activated += value; }
            remove { RealWindow.Activated -= value; }
        }
        public event EventHandler Deactivated {
            add { RealWindow.Deactivated += value; }
            remove { RealWindow.Deactivated -= value; }
        }
        public bool Activate() {
            return RealWindow.Activate();
        }
        public void Hide() {
            RealWindow.Hide();
        }
    }
}