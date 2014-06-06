using DevExpress.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace DevExpress.Mvvm {
    public interface IMessageBoxService {
#if !SILVERLIGHT
        MessageBoxResult Show(string messageBoxText, string caption, MessageBoxButton button, MessageBoxImage icon, MessageBoxResult defaultResult);
#else
        MessageBoxResult Show(string messageBoxText, string caption, MessageBoxButton button, MessageBoxResult defaultResult);
#endif
    }
}