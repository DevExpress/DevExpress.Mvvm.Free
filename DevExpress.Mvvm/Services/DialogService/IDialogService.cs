using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
#if NETFX_CORE
using Windows.UI.Popups;
using DevExpress.Mvvm.Native;
#endif

namespace DevExpress.Mvvm {
    public interface IDialogService {
#if SILVERLIGHT
        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        Task<UICommand> ShowDialog(IEnumerable<UICommand> dialogCommands, string title, string documentType, object viewModel, object parameter, object parentViewModel);
#elif NETFX_CORE
        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        Task<UICommand> ShowDialogAsync(IEnumerable<UICommand> dialogCommands, string title, string documentType, object viewModel, object parameter, object parentViewModel);
#else
        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        UICommand ShowDialog(IEnumerable<UICommand> dialogCommands, string title, string documentType, object viewModel, object parameter, object parentViewModel);
#endif
    }
}