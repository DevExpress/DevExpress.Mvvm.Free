using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
#if NETFX_CORE
using Windows.UI.Popups;
#endif

namespace DevExpress.Mvvm {
    public interface IDialogService {
#if NETFX_CORE
        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        Task<UICommand> ShowDialogAsync(IEnumerable<UICommand> dialogCommands, string title, string documentType, object viewModel, object parameter, object parentViewModel);
#else
        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        UICommand ShowDialog(IEnumerable<UICommand> dialogCommands, string title, string documentType, object viewModel, object parameter, object parentViewModel);
#endif
    }
}
