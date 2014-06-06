using DevExpress.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace DevExpress.Mvvm {
    public static class MessageBoxServiceExtensions {
        public static MessageBoxResult Show(this IMessageBoxService service, string messageBoxText) {
            return service.Show(messageBoxText, string.Empty);
        }
        public static MessageBoxResult Show(this IMessageBoxService service, string messageBoxText, string caption) {
            return service.Show(messageBoxText, caption, MessageBoxButton.OK);
        }
        public static MessageBoxResult Show(this IMessageBoxService service, string messageBoxText, string caption, MessageBoxButton button) {
#if !SILVERLIGHT
            return service.Show(messageBoxText, caption, button, MessageBoxImage.None);
#else
            VerifyService(service);
            return service.Show(messageBoxText, caption, button, MessageBoxResult.None);
#endif
        }
#if !SILVERLIGHT
        public static MessageBoxResult Show(this IMessageBoxService service, string messageBoxText, string caption, MessageBoxButton button, MessageBoxImage icon) {
            VerifyService(service);
            return service.Show(messageBoxText, caption, button, icon, MessageBoxResult.None);
        }
#endif
        static void VerifyService(IMessageBoxService service) {
            if(service == null)
                throw new ArgumentNullException("service");
        }
    }
}