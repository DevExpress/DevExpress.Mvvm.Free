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
    public class DialogButtonViewModel : BindableBase{
        public object Content { get;  set; }
        public ICommand Command { get; private set; }
        public UICommand UICommand { get; private set; }
        public object AppBarButtonContent { get; set; }
        public object AppBarButtonLabel { get; set; }
        public IconElement AppBarButtonIcon { get; set; }
        public bool CanExecute {
            get { return GetProperty(() => CanExecute); }
            set { SetProperty(() => CanExecute, value); }
        }
        public DialogButtonViewModel(UICommand customUICommand, ICommand command) {
            this.UICommand = customUICommand;
            this.Content = customUICommand.Caption;
            this.Command = command;
            CanExecute = true;
            if(UICommand.Command != null) {
                CanExecute = UICommand.Command.CanExecute(null);
                UICommand.Command.CanExecuteChanged += CanExecuteChanged;
            }
        }
        void CanExecuteChanged(object sender, EventArgs e) {
            this.CanExecute = UICommand.Command.CanExecute(sender);
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
            ButtonsContents = new List<ButtonContent>();
            defaultButtonContent = new Dictionary<string, List<ButtonContent>> {
                { "OK",         new List<ButtonContent>() {
                                        new ButtonContent() { Id = "OK", Icon = new SymbolIcon(Symbol.Accept)}}
                                },
                { "OKCancel",   new List<ButtonContent>() {
                                        new ButtonContent() { Id = "OK", Icon = new SymbolIcon(Symbol.Accept) },
                                        new ButtonContent() { Id = "Cancel", Icon = new SymbolIcon(Symbol.Cancel)}}
                                },
                { "YesNo",      new List<ButtonContent>() {
                                        new ButtonContent() { Id = "Yes", Icon = new SymbolIcon(Symbol.Accept) },
                                        new ButtonContent() { Id = "No", Icon = new SymbolIcon(Symbol.Cancel) }}
                                },
                { "RetryCancel",      new List<ButtonContent>() {
                                        new ButtonContent() { Id = "Retry", Icon = new SymbolIcon(Symbol.RepeatAll) },
                                        new ButtonContent() { Id = "Cancel", Icon = new SymbolIcon(Symbol.Cancel) }}
                                },
                { "Close",         new List<ButtonContent>() {
                                        new ButtonContent() { Id = "Close", Icon = new SymbolIcon(Symbol.Cancel)}}
                                },
            };
        }
        protected ICommand DialogResultCommand { get; set; }
        protected Popup Popup { get; set; }
        protected UICommand Result { get; set; }
        protected CancellationToken CancellationToken { get; set; }
        protected IAsyncAction AsyncAction { get; set; }
        public List<ButtonContent> ButtonsContents { get; set; }
        Dictionary<string, List<ButtonContent>> defaultButtonContent;
        public void GoBack() {
            if(!CanGoBack) return;
            Close();
        }
        public bool CanGoBack{ get { return Popup == null ? false : Popup.IsOpen; } }
            List<DialogButtonViewModel> CreateButtons(List<ButtonContent> appBarButtonsContents, IEnumerable<UICommand> commandsSource) {
            List<DialogButtonViewModel> newButtons = new List<DialogButtonViewModel>();
            appBarButtonsContents = appBarButtonsContents.Where(x => x.Id != null).ToList();
            string messageButtonsType = "";
            foreach(UICommand command in commandsSource) {
                messageButtonsType += command.Caption;
            }
            foreach(UICommand command in commandsSource) {
                var buttonContent = appBarButtonsContents.FirstOrDefault(x => x.Id.Equals(command.Id.ToString()));
                if(buttonContent?.Icon == null && buttonContent?.Content == null) {
                    if(defaultButtonContent.ContainsKey(messageButtonsType)){
                        newButtons.Add(new DialogButtonViewModel(command, DialogResultCommand) { AppBarButtonLabel = command.Caption, AppBarButtonIcon = defaultButtonContent[messageButtonsType].FirstOrDefault(x => x.Id.Equals(command.Id.ToString())).Icon });
                    } else {
                        newButtons.Add(new DialogButtonViewModel(command, DialogResultCommand) { AppBarButtonContent = command.Caption});
                    }
                } else {
                    newButtons.Add(new DialogButtonViewModel(command, DialogResultCommand) { AppBarButtonLabel = command.Caption, AppBarButtonContent = buttonContent?.Content, AppBarButtonIcon = buttonContent?.Icon });
                }
            }
            return newButtons;
        }
        public async Task<UICommand> ShowAsync() {
            Popup = new Popup();
            Popup.Width = Window.Current.CoreWindow.Bounds.Width;
            Popup.Height = Window.Current.CoreWindow.Bounds.Height;
            Window.Current.CoreWindow.SizeChanged += CoreWindow_SizeChanged;
            Result = null;
            DialogResultCommand = new DevExpress.Mvvm.DelegateCommand<object>(OnDialogResult);
            Buttons = CreateButtons(ButtonsContents, CommandsSource);
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