using DevExpress.Mvvm.Native;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Style = Windows.UI.Xaml.Style;

namespace DevExpress.Mvvm.UI {
    public class DialogService : ViewServiceBase, IDialogService, IMessageButtonLocalizer, IDocumentOwner {
        internal const string ShowDialogException = "Cannot use dialogButtons and dialogCommands parameters simultaneously.";
        public static readonly DependencyProperty DialogStyleProperty =
            DependencyProperty.Register("DialogStyle", typeof(Style), typeof(DialogService), new PropertyMetadata(null));
        public Style DialogStyle {
            get { return (Style)GetValue(DialogStyleProperty); }
            set { SetValue(DialogStyleProperty, value); }
        }
        protected DialogContentControl CurrentDialogContentControl { get; set; }
        string IMessageButtonLocalizer.Localize(MessageResult button) {
            return new DefaultMessageButtonLocalizer().Localize(button);
        }
        public async Task<UICommand> ShowDialogAsync(IEnumerable<UICommand> dialogCommands, string title, string documentType, object viewModel, object parameter, object parentViewModel) {
            object view = CreateAndInitializeView(documentType, viewModel, parameter, parentViewModel, this);
            CurrentDialogContentControl = new DialogContentControl() { Content = view, Style = DialogStyle };
            dialogCommands.Do(c => CurrentDialogContentControl.CommandsSource = c);
            if(title != null)
                CurrentDialogContentControl.Title = title;
            else
                DocumentUIServiceBase.SetTitleBinding(view, DialogContentControl.TitleProperty, CurrentDialogContentControl, true);
            return await CurrentDialogContentControl.ShowAsync();
        }
        public void Close(IDocumentContent documentContent, bool force = true) {
            CurrentDialogContentControl.Close(documentContent, force);
            CurrentDialogContentControl = null;
        }
    }
}