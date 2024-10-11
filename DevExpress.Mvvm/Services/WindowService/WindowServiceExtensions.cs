using System;
using System.ComponentModel;
using System.Windows;

namespace DevExpress.Mvvm {
    public static class WindowServiceExtensions {
        public static void Show(this IWindowService service, object viewModel) {
            VerifyService(service);
            service.Show(null, viewModel, null, null);
        }
        public static void Show(this IWindowService service, string documentType, object viewModel) {
            VerifyService(service);
            service.Show(documentType, viewModel, null, null);
        }
        public static void Show(this IWindowService service, string documentType, object parameter, object parentViewModel) {
            VerifyService(service);
            service.Show(documentType, null, parameter, parentViewModel);
        }
        internal static void VerifyService(IWindowService service) {
            if(service == null)
                throw new ArgumentNullException(nameof(service));
        }
    }
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class WindowServicePlatformExtensions {
        [EditorBrowsable(EditorBrowsableState.Never)]
        public static void SetWindowState(this IWindowService service, WindowState state) {
            WindowServiceExtensions.VerifyService(service);
            service.WindowState = DXWindowStateConverter.ToDXWindowState(state);
        }
    }
}