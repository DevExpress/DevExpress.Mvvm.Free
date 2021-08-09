using System;
using System.ComponentModel;
using System.Windows;

namespace DevExpress.Mvvm {
    public enum DXWindowState { Normal = 0, Minimized = 1, Maximized = 2 }
    public interface ICurrentWindowService {
        void Close();
        DXWindowState WindowState { get; set; }
        void Activate();
        void Hide();
        void Show();
    }
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class CurrentWindowServicePlatformExtensions {
        [EditorBrowsable(EditorBrowsableState.Never)]
        public static void SetWindowState(this ICurrentWindowService service, WindowState state) {
            VerifyService(service);
            service.WindowState = DXWindowStateConverter.ToDXWindowState(state);
        }
        static void VerifyService(ICurrentWindowService service) {
            if(service == null)
                throw new ArgumentNullException("service");
        }
    }
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class DXWindowStateConverter {
        public static WindowState ToWindowState(this DXWindowState x) {
            switch(x) {
                case DXWindowState.Normal: return WindowState.Normal;
                case DXWindowState.Minimized: return WindowState.Minimized;
                case DXWindowState.Maximized: return WindowState.Maximized;
                default: throw new NotImplementedException();
            }
        }
        public static DXWindowState ToDXWindowState(this WindowState x) {
            switch(x) {
                case WindowState.Normal: return DXWindowState.Normal;
                case WindowState.Minimized: return DXWindowState.Minimized;
                case WindowState.Maximized: return DXWindowState.Maximized;
                default: throw new NotImplementedException();
            }
        }
    }
}
