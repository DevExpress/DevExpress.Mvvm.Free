using DevExpress.Mvvm;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;

namespace DevExpress.Mvvm {
    public static class MessageBoxServiceExtensions {
        public static MessageResult ShowMessage(this IMessageBoxService service, string messageBoxText, string caption, MessageButton button, MessageIcon icon, MessageResult defaultResult) {
            VerifyService(service);
            return service.Show(messageBoxText, caption, button, icon, defaultResult);
        }
        public static bool? ShowMessage(this IMessageBoxService service, string messageBoxText) {
            return service.ShowMessage(messageBoxText, string.Empty);
        }
        public static bool? ShowMessage(this IMessageBoxService service, string messageBoxText, string caption) {
            return service.ShowMessage(messageBoxText, caption, MessageButton.OK).ToBool();
        }
        public static MessageResult ShowMessage(this IMessageBoxService service, string messageBoxText, string caption, MessageButton button) {
            return service.ShowMessage(messageBoxText, caption, button, MessageIcon.None);
        }
        public static MessageResult ShowMessage(this IMessageBoxService service, string messageBoxText, string caption, MessageButton button, MessageIcon icon) {
            VerifyService(service);
            return service.Show(messageBoxText, caption, button, icon, MessageResult.None);
        }
        static void VerifyService(IMessageBoxService service) {
            if(service == null)
                throw new ArgumentNullException("service");
        }
    }
}