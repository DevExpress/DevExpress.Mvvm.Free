using DevExpress.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.ComponentModel;
#if NETFX_CORE
using Windows.Foundation;
using System.Threading.Tasks;
#endif

namespace DevExpress.Mvvm {
    public interface IMessageBoxService {
#if SILVERLIGHT
        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        MessageResult Show(string messageBoxText, string caption, MessageButton button, MessageResult defaultResult);
#elif NETFX_CORE
        Task<UICommand> ShowAsync(string messageBoxText, string caption, IList<UICommand> commands);
#else
        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        MessageResult Show(string messageBoxText, string caption, MessageButton button, MessageIcon icon, MessageResult defaultResult);
#endif
    }
}