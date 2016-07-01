using DevExpress.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.ComponentModel;

namespace DevExpress.Mvvm {
    public interface IMessageBoxService {
        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        MessageResult Show(string messageBoxText, string caption, MessageButton button, MessageIcon icon, MessageResult defaultResult);
    }
}