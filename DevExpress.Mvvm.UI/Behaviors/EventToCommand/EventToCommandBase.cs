using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;
using System.Windows.Threading;

namespace DevExpress.Mvvm.UI {
    public abstract class EventToCommandBase : DevExpress.Mvvm.UI.Interactivity.EventTrigger {
        public static readonly DependencyProperty CommandProperty =
           DependencyProperty.Register("Command", typeof(ICommand), typeof(EventToCommandBase),
           new PropertyMetadata(null, (d, e) => ((EventToCommandBase)d).OnCommandChanged((ICommand)e.OldValue, (ICommand)e.NewValue)));
        public static readonly DependencyProperty CommandParameterProperty =
           DependencyProperty.Register("CommandParameter", typeof(object), typeof(EventToCommandBase),
           new PropertyMetadata(null, (d, e) => ((EventToCommandBase)d).OnCommandParameterChanged(e.OldValue, e.NewValue)));
        public static readonly DependencyProperty ProcessEventsFromDisabledEventOwnerProperty =
            DependencyProperty.Register("ProcessEventsFromDisabledEventOwner", typeof(bool), typeof(EventToCommandBase),
            new PropertyMetadata(true));
        public static readonly DependencyProperty MarkRoutedEventsAsHandledProperty =
            DependencyProperty.Register("MarkRoutedEventsAsHandled", typeof(bool), typeof(EventToCommandBase),
            new PropertyMetadata(false));

        public static readonly DependencyProperty UseDispatcherProperty =
            DependencyProperty.Register("UseDispatcher", typeof(bool?), typeof(EventToCommandBase),
            new PropertyMetadata(null));
        public static readonly DependencyProperty CommandTargetProperty =
            DependencyProperty.Register("CommandTarget", typeof(IInputElement), typeof(EventToCommandBase),
            new PropertyMetadata(null));
        public static readonly DependencyProperty DispatcherPriorityProperty =
            DependencyProperty.Register("DispatcherPriority", typeof(DispatcherPriority?), typeof(EventToCommandBase),
            new PropertyMetadata(null));

        public ICommand Command {
            get { return (ICommand)GetValue(CommandProperty); }
            set { SetValue(CommandProperty, value); }
        }
        public object CommandParameter {
            get { return GetValue(CommandParameterProperty); }
            set { SetValue(CommandParameterProperty, value); }
        }
        public bool ProcessEventsFromDisabledEventOwner {
            get { return (bool)GetValue(ProcessEventsFromDisabledEventOwnerProperty); }
            set { SetValue(ProcessEventsFromDisabledEventOwnerProperty, value); }
        }
        public bool MarkRoutedEventsAsHandled {
            get { return (bool)GetValue(MarkRoutedEventsAsHandledProperty); }
            set { SetValue(MarkRoutedEventsAsHandledProperty, value); }
        }
        public bool? UseDispatcher {
            get { return (bool?)GetValue(UseDispatcherProperty); }
            set { SetValue(UseDispatcherProperty, value); }
        }
        protected internal bool ActualUseDispatcher {
            get {
                if(UseDispatcher == null) {
                    return DispatcherPriority != null;
                }
                return UseDispatcher.Value;
            }
        }
        public IInputElement CommandTarget {
            get { return (IInputElement)GetValue(CommandTargetProperty); }
            set { SetValue(CommandTargetProperty, value); }
        }
        public DispatcherPriority? DispatcherPriority {
            get { return (DispatcherPriority?)GetValue(DispatcherPriorityProperty); }
            set { SetValue(DispatcherPriorityProperty, value); }
        }
        protected internal DispatcherPriority ActualDispatcherPriority {
            get { return DispatcherPriority ?? System.Windows.Threading.DispatcherPriority.Normal; }
        }

        protected override void OnEvent(object sender, object eventArgs) {
            base.OnEvent(sender, eventArgs);
            if(Command != null) {
                OnEventCore(sender, eventArgs);
                return;
            }
            bool commandIsBound = System.Windows.Data.BindingOperations.GetBindingExpression(this, CommandProperty) != null;
            if(Command == null && commandIsBound) {
                Dispatcher.BeginInvoke(new Action(() => {
                    OnEventCore(sender, eventArgs);
                }));
            }
        }
        void OnEventCore(object sender, object eventArgs) {
            if(Command == null) return;
            if(!CanInvoke(sender, eventArgs)) return;
            if(!ActualUseDispatcher)
                Invoke(sender, eventArgs);
            else {
                Dispatcher.BeginInvoke(new Action<object, object>(Invoke), ActualDispatcherPriority, new object[] { sender, eventArgs });
            }
            if(MarkRoutedEventsAsHandled) {
                if(eventArgs is RoutedEventArgs) ((RoutedEventArgs)eventArgs).Handled = true;
            }
        }
        protected abstract void Invoke(object sender, object eventArgs);
        protected virtual bool CanInvoke(object sender, object eventArgs) {
            FrameworkElement associatedFrameworkObject = Source as FrameworkElement;
            return ProcessEventsFromDisabledEventOwner || associatedFrameworkObject == null || associatedFrameworkObject.IsEnabled;
        }
        protected virtual void OnCommandChanged(ICommand oldValue, ICommand newValue) {
            OnCommandChanged();
        }
        protected virtual void OnCommandParameterChanged(object oldValue, object newValue) {
            OnCommandParameterChanged();
        }
        protected virtual void OnCommandChanged() { }
        protected virtual void OnCommandParameterChanged() { }

        protected bool CommandCanExecute(object commandParameter) {
            if(Command is RoutedCommand)
                return ((RoutedCommand)Command).CanExecute(commandParameter, CommandTarget);
            return Command.CanExecute(commandParameter);
        }
        protected void CommandExecute(object commandParameter) {
            if (Command is RoutedCommand) {
                ((RoutedCommand)Command).Execute(commandParameter, CommandTarget);
                return;
            }
            Command.Execute(commandParameter);
        }
    }
}