using DevExpress.Mvvm.Native;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Foundation.Metadata;
using Windows.Phone.UI.Input;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Style = Windows.UI.Xaml.Style;

namespace DevExpress.Mvvm.UI {
    public class ButtonContent {
        public ButtonContent() { }
        public ButtonContent(object id, object content, IconElement icon) {
            Id = id;
            Content = content;
            Icon = icon;
        }
        public object Id { get; set; }
        public object Content { get; set; }
        public IconElement Icon { get; set; }
    }
    public class DialogService : ViewServiceBase, IDialogService, IMessageButtonLocalizer, IDocumentOwner {
        internal const string ShowDialogException = "Cannot use dialogButtons and dialogCommands parameters simultaneously.";
        public static readonly DependencyProperty DialogStyleProperty =
            DependencyProperty.Register("DialogStyle", typeof(Style), typeof(DialogService), new PropertyMetadata(null));
        public Style DialogStyle {
            get { return (Style)GetValue(DialogStyleProperty); }
            set { SetValue(DialogStyleProperty, value); }
        }
        public List<ButtonContent> ButtonsContents {
            get { return (List<ButtonContent>)GetValue(ButtonsContentsProperty); }
            set { SetValue(ButtonsContentsProperty, value); }
        }
        public static readonly DependencyProperty ButtonsContentsProperty =
            DependencyProperty.Register("ButtonsContents", typeof(List<ButtonContent>), typeof(DialogService), new PropertyMetadata(null));
        public DialogService() {
            ButtonsContents = new List<ButtonContent>();
        }
        protected DialogContentControl CurrentDialogContentControl { get; set; }
        string IMessageButtonLocalizer.Localize(MessageResult button) {
            return new DefaultMessageButtonLocalizer().Localize(button);
        }
        public async Task<UICommand> ShowDialogAsync(IEnumerable<UICommand> dialogCommands, string title, string documentType, object viewModel, object parameter, object parentViewModel) {
            object view = CreateAndInitializeView(documentType, viewModel, parameter, parentViewModel, this);
            CurrentDialogContentControl = new DialogContentControl() { Content = view, Style = DialogStyle, ButtonsContents = ButtonsContents };
            dialogCommands.Do(c => CurrentDialogContentControl.CommandsSource = c);
            if(title != null)
                CurrentDialogContentControl.Title = title;
            else
                DocumentUIServiceBase.SetTitleBinding(view, DialogContentControl.TitleProperty, CurrentDialogContentControl, true);
            var resultCommand = await CurrentDialogContentControl.ShowAsync();
            return resultCommand;
        }
        public void Close(IDocumentContent documentContent, bool force = true) {
            CurrentDialogContentControl.Close(documentContent, force);
            CurrentDialogContentControl = null;
        }
        T GetDataContext<T>() where T : class {
            FrameworkElement page = CurrentDialogContentControl.Content as FrameworkElement;
            return page != null ? (page.DataContext as T) : null;
        }
    }
}