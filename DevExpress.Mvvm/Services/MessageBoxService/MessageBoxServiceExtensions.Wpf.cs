using DevExpress.Mvvm;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;

namespace DevExpress.Mvvm {
    [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
    public static class MessageBoxServicePlatformExtensions {
        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public static MessageBoxResult Show(this IMessageBoxService service, string messageBoxText) {
            return service.Show(messageBoxText, string.Empty);
        }
        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public static MessageBoxResult Show(this IMessageBoxService service, string messageBoxText, string caption) {
            return service.ShowMessage(messageBoxText, caption, MessageButton.OK).ToMessageBoxResult();
        }
        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public static MessageBoxResult Show(this IMessageBoxService service, string messageBoxText, string caption, MessageBoxButton button) {
#if !SILVERLIGHT
            return service.Show(messageBoxText, caption, button, MessageBoxImage.None);
#else
            return service.Show(messageBoxText, caption, button, MessageBoxResult.None);
#endif
        }
#if !SILVERLIGHT
        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public static MessageBoxResult Show(this IMessageBoxService service, string messageBoxText, string caption, MessageBoxButton button, MessageBoxImage icon) {
            return service.Show(messageBoxText, caption, button, icon, MessageBoxResult.None);
        }
#endif
#if SILVERLIGHT
        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public static MessageBoxResult Show(this IMessageBoxService service, string messageBoxText, string caption, MessageBoxButton button, MessageBoxResult defaultResult) {
            VerifyService(service);
            return service.Show(messageBoxText, caption, button.ToMessageButton(), defaultResult.ToMessageResult()).ToMessageBoxResult();
        }
#else
        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public static MessageBoxResult Show(this IMessageBoxService service, string messageBoxText, string caption, MessageBoxButton button, MessageBoxImage icon, MessageBoxResult defaultResult) {
            VerifyService(service);
            return service.Show(messageBoxText, caption, button.ToMessageButton(), icon.ToMessageIcon(), defaultResult.ToMessageResult()).ToMessageBoxResult();
        }
#endif
        static void VerifyService(IMessageBoxService service) {
            if(service == null)
                throw new ArgumentNullException("service");
        }
    }
}