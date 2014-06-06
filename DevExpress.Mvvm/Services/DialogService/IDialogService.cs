using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;

namespace DevExpress.Mvvm {
    public interface IDialogService {
#if !SILVERLIGHT
        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        UICommand ShowDialog(IEnumerable<UICommand> dialogCommands, string title, string documentType, object viewModel, object parameter, object parentViewModel);
#else
        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        Task<UICommand> ShowDialog(IEnumerable<UICommand> dialogCommands, string title, string documentType, object viewModel, object parameter, object parentViewModel);
#endif
    }
}