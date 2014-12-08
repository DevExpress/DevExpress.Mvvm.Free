using DevExpress.Mvvm;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;

namespace DevExpress.Mvvm {
    public static class MessageBoxServiceExtensions {
#if SILVERLIGHT
        public static MessageResult ShowMessage(this IMessageBoxService service, string messageBoxText, string caption, MessageButton button, MessageResult defaultResult) {
            VerifyService(service);
            return service.Show(messageBoxText, caption, button, defaultResult);
        }
#else
        public static MessageResult ShowMessage(this IMessageBoxService service, string messageBoxText, string caption, MessageButton button, MessageIcon icon, MessageResult defaultResult) {
            VerifyService(service);
            return service.Show(messageBoxText, caption, button, icon, defaultResult);
        }
#endif
        public static bool? ShowMessage(this IMessageBoxService service, string messageBoxText) {
            return service.ShowMessage(messageBoxText, string.Empty);
        }
        public static bool? ShowMessage(this IMessageBoxService service, string messageBoxText, string caption) {
            return service.ShowMessage(messageBoxText, caption, MessageButton.OK).ToBool();
        }
        public static MessageResult ShowMessage(this IMessageBoxService service, string messageBoxText, string caption, MessageButton button) {
#if !SILVERLIGHT
            return service.ShowMessage(messageBoxText, caption, button, MessageIcon.None);
#else
            VerifyService(service);
            return service.Show(messageBoxText, caption, button, MessageResult.None);
#endif
        }
#if !SILVERLIGHT
        public static MessageResult ShowMessage(this IMessageBoxService service, string messageBoxText, string caption, MessageButton button, MessageIcon icon) {
            VerifyService(service);
            return service.Show(messageBoxText, caption, button, icon, MessageResult.None);
        }
#endif
        static void VerifyService(IMessageBoxService service) {
            if(service == null)
                throw new ArgumentNullException("service");
        }
    }
}