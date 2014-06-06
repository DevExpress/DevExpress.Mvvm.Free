using DevExpress.Mvvm;
using DevExpress.Mvvm.UI;
using DevExpress.Mvvm.UI.Interactivity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
#if !SILVERLIGHT
namespace DevExpress.Mvvm.UI {
    public class MessageBoxService : ServiceBase, IMessageBoxService {
        MessageBoxResult IMessageBoxService.Show(string messageBoxText, string caption, MessageBoxButton button, MessageBoxImage icon, MessageBoxResult defaultResult) {
            if(AssociatedObject is Window)
                return MessageBox.Show((Window)AssociatedObject, messageBoxText, caption, button, icon, defaultResult);
            else
                return MessageBox.Show(messageBoxText, caption, button, icon, defaultResult);
        }
    }
}
#else
namespace DevExpress.Mvvm.UI {
    public class MessageBoxService : ServiceBase, IMessageBoxService {
        MessageBoxResult IMessageBoxService.Show(string messageBoxText, string caption, MessageBoxButton button, MessageBoxResult defaultResult) {
            return MessageBox.Show(messageBoxText, caption, button);
        }
    }
}
#endif