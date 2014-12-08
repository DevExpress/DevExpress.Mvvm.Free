using DevExpress.Core;
using DevExpress.Mvvm.Native;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.Foundation;
using Windows.System;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Input;

namespace DevExpress.Mvvm.UI {
    public class DialogButtonViewModel {
        public object Content { get; private set; }
        public ICommand Command { get; private set; }
        public UICommand UICommand { get; private set; }
        public DialogButtonViewModel(UICommand customUICommand, ICommand command) {
            this.UICommand = customUICommand;
            this.Content = customUICommand.Caption;
            this.Command = command;
        }
    }
    public class DialogButton : Button {
        public DialogButton() {
            DefaultStyleKey = typeof(DialogButton);
        }
    }
    public class DialogContentControl : ContentControl {
        public IEnumerable<DialogButtonViewModel> Buttons {
            get { return (IEnumerable<DialogButtonViewModel>)GetValue(ButtonsProperty); }
            set { SetValue(ButtonsProperty, value); }
        }
        public static readonly DependencyProperty ButtonsProperty =
            DependencyProperty.Register("Buttons", typeof(IEnumerable<DialogButtonViewModel>), typeof(DialogContentControl), new PropertyMetadata(null));

        public object Title {
            get { return (object)GetValue(TitleProperty); }
            set { SetValue(TitleProperty, value); }
        }
        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register("Title", typeof(object), typeof(DialogContentControl), new PropertyMetadata(null));

        public IEnumerable<UICommand> CommandsSource {
            get { return (IEnumerable<UICommand>)GetValue(CommandsSourceProperty); }
            set { SetValue(CommandsSourceProperty, value); }
        }
        public static readonly DependencyProperty CommandsSourceProperty =
            DependencyProperty.Register("CommandsSource", typeof(IEnumerable<UICommand>), typeof(DialogContentControl), new PropertyMetadata(null));

        public DialogContentControl() {
            DefaultStyleKey = typeof(DialogContentControl);
            CommandsSource = new List<UICommand>();
        }
        protected ICommand DialogResultCommand { get; set; }
        protected Popup Popup { get; set; }
        protected CancellationToken CancellationToken { get; set; }
        protected IAsyncAction AsyncAction { get; set; }
        protected UICommand Result { get; set; }

        public async Task<UICommand> ShowAsync() {
            Popup = new Popup();
            Popup.Width = Window.Current.CoreWindow.Bounds.Width;
            Popup.Height = Window.Current.CoreWindow.Bounds.Height;
            Window.Current.CoreWindow.SizeChanged += CoreWindow_SizeChanged;
            Result = null;
            DialogResultCommand = new DevExpress.Mvvm.DelegateCommand<object>(OnDialogResult);
            List<DialogButtonViewModel> buttons = new List<DialogButtonViewModel>();
            foreach(UICommand command in CommandsSource) {
                buttons.Add(new DialogButtonViewModel(command, DialogResultCommand));
            }
            Buttons = buttons;
            Popup.Child = this;
            Width = Popup.Width;
            Height = Popup.Height;
            KeyDown += DialogContentControl_KeyDown;
            Popup.VerticalOffset = 0;
            Popup.HorizontalOffset = 0;
            Popup.IsOpen = true;
            Focus(FocusState.Programmatic);
            await WaitPopupClosing();
            return Result;
        }
        void OnDialogResult(object parameter) {
            if(!Close())
                return;
            if(parameter is UICommand) {
                UICommand command = (UICommand)parameter;
                command.Command.If(x => x.CanExecute(command)).Do(x => x.Execute(command));
                Result = command;
            }
        }
        async Task WaitPopupClosing() {
            CancellationToken = new CancellationToken();
            Popup popup = Popup;
            AsyncAction = EventAwaiter.WaitEventAsync<object>(handler => popup.Closed += handler, handler => popup.Closed -= handler, () => { }).AsAsyncAction();
            await AsyncAction;
            popup = null;
        }
#if DEBUG
        public event EventHandler<CancelEventArgs> Closing_Test;
        protected void OnClosing(CancelEventArgs e) {
            if(Closing_Test != null)
                Closing_Test(this, e);
        }
#endif
        void OnPopup_Closing(CancelEventArgs e) {
#if DEBUG
            OnClosing(e);
#endif
            DocumentViewModelHelper.OnClose(ViewHelper.GetViewModelFromView(Content), e);
        }
        void CoreWindow_SizeChanged(CoreWindow sender, WindowSizeChangedEventArgs args) {
            this.Width = Window.Current.CoreWindow.Bounds.Width;
            this.Height = Window.Current.CoreWindow.Bounds.Height;
        }
        public void Close(IDocumentContent documentContent, bool force = true) {
            Close(force);
        }
        public bool Close(bool force = false) {
            if(!force) {
                var eventArgs = new CancelEventArgs();
                OnPopup_Closing(eventArgs);
                if(eventArgs.Cancel)
                    return false;
            }
            if(AsyncAction != null)
                AsyncAction.Cancel();
            Window.Current.CoreWindow.SizeChanged -= CoreWindow_SizeChanged;
            if(Popup != null) {
                DocumentViewModelHelper.OnDestroy(ViewHelper.GetViewModelFromView(Content));
                Popup.IsOpen = false;
                Popup = null;
            }
            return true;
        }
#if DEBUG
        public void SilentClose() {
            if(AsyncAction != null)
                AsyncAction.Cancel();
            Window.Current.CoreWindow.SizeChanged -= CoreWindow_SizeChanged;
            if(Popup != null) {
                Popup.IsOpen = false;
                Popup = null;
            }
        }
#endif
        void DialogContentControl_KeyDown(object sender, KeyRoutedEventArgs e) {
            if(e.Key == VirtualKey.Enter) {
                CommandsSource.FirstOrDefault(x => x.IsDefault).Do(x => { e.Handled = true; DialogResultCommand.Execute(x); });
            } else if(e.Key == VirtualKey.Escape) {
                CommandsSource.FirstOrDefault(x => x.IsCancel).Do(x => { e.Handled = true; DialogResultCommand.Execute(x); });
            }
        }
    }
    public class DialogTitleControl : ContentControl {
        public DialogTitleControl() {
            DefaultStyleKey = typeof(DialogTitleControl);
        }
    }
}