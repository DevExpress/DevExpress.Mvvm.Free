using DevExpress.Mvvm.Native;
using DevExpress.Mvvm.UI.Interactivity;
using System;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Input;

namespace DevExpress.Mvvm.UI {
    public class ConfirmationBehavior : Behavior<DependencyObject> {
        #region Dependency Properties
        static MessageBoxButton DefaultMessageBoxButton = MessageBoxButton.YesNo;
        public static readonly DependencyProperty EnableConfirmationMessageProperty =
            DependencyProperty.Register("EnableConfirmationMessage", typeof(bool), typeof(ConfirmationBehavior),
            new PropertyMetadata(true));
        public static readonly DependencyProperty CommandProperty =
            DependencyProperty.Register("Command", typeof(ICommand), typeof(ConfirmationBehavior),
            new PropertyMetadata(null, (d, e) => ((ConfirmationBehavior)d).OnCommandChanged((ICommand)e.OldValue, (ICommand)e.NewValue)));
        public static readonly DependencyProperty CommandParameterProperty =
            DependencyProperty.Register("CommandParameter", typeof(object), typeof(ConfirmationBehavior),
            new PropertyMetadata(null, (d, e) => ((ConfirmationBehavior)d).OnCommandParameterChanged()));
        public static readonly DependencyProperty CommandPropertyNameProperty =
            DependencyProperty.Register("CommandPropertyName", typeof(string), typeof(ConfirmationBehavior),
            new PropertyMetadata("Command"));
        public static readonly DependencyProperty MessageBoxServiceProperty =
            DependencyProperty.Register("MessageBoxService", typeof(IMessageBoxService), typeof(ConfirmationBehavior),
            new PropertyMetadata(null));
        public static readonly DependencyProperty MessageTitleProperty =
            DependencyProperty.Register("MessageTitle", typeof(string), typeof(ConfirmationBehavior),
            new PropertyMetadata("Confirmation"));
        public static readonly DependencyProperty MessageTextProperty =
            DependencyProperty.Register("MessageText", typeof(string), typeof(ConfirmationBehavior),
            new PropertyMetadata("Do you want to perform this action?"));
        public static readonly DependencyProperty MessageButtonProperty =
            DependencyProperty.Register("MessageButton", typeof(MessageBoxButton), typeof(ConfirmationBehavior),
            new PropertyMetadata(DefaultMessageBoxButton));
        public static readonly DependencyProperty MessageDefaultResultProperty =
            DependencyProperty.Register("MessageDefaultResult", typeof(MessageBoxResult), typeof(ConfirmationBehavior),
            new PropertyMetadata(null));
        public static readonly DependencyProperty MessageIconProperty =
            DependencyProperty.Register("MessageIcon", typeof(MessageBoxImage), typeof(ConfirmationBehavior),
            new PropertyMetadata(MessageBoxImage.None));

        public bool EnableConfirmationMessage {
            get { return (bool)GetValue(EnableConfirmationMessageProperty); }
            set { SetValue(EnableConfirmationMessageProperty, value); }
        }
        public ICommand Command {
            get { return (ICommand)GetValue(CommandProperty); }
            set { SetValue(CommandProperty, value); }
        }
        public object CommandParameter {
            get { return (object)GetValue(CommandParameterProperty); }
            set { SetValue(CommandParameterProperty, value); }
        }
        public string CommandPropertyName {
            get { return (string)GetValue(CommandPropertyNameProperty); }
            set { SetValue(CommandPropertyNameProperty, value); }
        }
        public IMessageBoxService MessageBoxService {
            get { return (IMessageBoxService)GetValue(MessageBoxServiceProperty); }
            set { SetValue(MessageBoxServiceProperty, value); }
        }
        public string MessageTitle {
            get { return (string)GetValue(MessageTitleProperty); }
            set { SetValue(MessageTitleProperty, value); }
        }
        public string MessageText {
            get { return (string)GetValue(MessageTextProperty); }
            set { SetValue(MessageTextProperty, value); }
        }
        public MessageBoxButton MessageButton {
            get { return (MessageBoxButton)GetValue(MessageButtonProperty); }
            set { SetValue(MessageButtonProperty, value); }
        }
        public MessageBoxResult MessageDefaultResult {
            get { return (MessageBoxResult)GetValue(MessageDefaultResultProperty); }
            set { SetValue(MessageDefaultResultProperty, value); }
        }
        public MessageBoxImage MessageIcon {
            get { return (MessageBoxImage)GetValue(MessageIconProperty); }
            set { SetValue(MessageIconProperty, value); }
        }
        #endregion

        internal DelegateCommand<object> ConfirmationCommand;
        public ConfirmationBehavior() {
            ConfirmationCommand = new DelegateCommand<object>(ConfirmationCommandExecute, ConfirmationCommandCanExecute, false);
        }
        protected override void OnAttached() {
            base.OnAttached();
            SetAssociatedObjectCommandProperty(ConfirmationCommand);
            if(Command != null) {
                Command.CanExecuteChanged -= OnCommandCanExecuteChanged;
                Command.CanExecuteChanged += OnCommandCanExecuteChanged;
            }
        }
        protected override void OnDetaching() {
            if(Command != null)
                Command.CanExecuteChanged -= OnCommandCanExecuteChanged;
            base.OnDetaching();
        }

        void ConfirmationCommandExecute(object parameter) {
            var c = Command;
            var p = CommandParameter ?? parameter;
            if(c == null) return;
            if(ShowConfirmation())
                c.Execute(CommandParameter ?? p);
        }
        bool ConfirmationCommandCanExecute(object parameter) {
            if(Command == null) return true;
            return Command.CanExecute(CommandParameter ?? parameter);
        }
        void OnCommandCanExecuteChanged(object sender, EventArgs e) {
            ConfirmationCommand.RaiseCanExecuteChanged();
        }
        void OnCommandChanged(ICommand oldValue, ICommand newValue) {
            if(oldValue != null)
                oldValue.CanExecuteChanged -= OnCommandCanExecuteChanged;
            if(newValue != null)
                newValue.CanExecuteChanged += OnCommandCanExecuteChanged;
            ConfirmationCommand.RaiseCanExecuteChanged();
        }
        void OnCommandParameterChanged() {
            ConfirmationCommand.RaiseCanExecuteChanged();
        }

        PropertyInfo GetCommandProperty() {
            Type associatedObjectType = AssociatedObject.GetType();
            PropertyInfo commandPropertyInfo = associatedObjectType.GetProperty(CommandPropertyName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.SetProperty);
            return commandPropertyInfo;
        }
        internal bool SetAssociatedObjectCommandProperty(object command) {
            var pi = GetCommandProperty();
            if(pi != null) {
                pi.SetValue(AssociatedObject, command, null);
                return true;
            }
            return false;
        }
        internal IMessageBoxService GetActualService() {
            IMessageBoxService res = MessageBoxService;
            if(res != null) return res;
            ISupportServices viewModel = AssociatedObject.With(x => x as FrameworkElement).
                Return(x => x.DataContext, null) as ISupportServices;
            if(viewModel != null)
                res = viewModel.ServiceContainer.GetService<IMessageBoxService>();
            if(res != null) return res;
            res = new MessageBoxService();
            return res;
        }
        bool ShowConfirmation() {
            if (!EnableConfirmationMessage) return true;
            IMessageBoxService service = GetActualService();
            MessageBoxResult res = service.Show(MessageText, MessageTitle, MessageButton, MessageIcon, MessageDefaultResult);
            return res == MessageBoxResult.OK || res == MessageBoxResult.Yes;
        }
    }
}