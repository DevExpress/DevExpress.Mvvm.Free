using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

namespace DevExpress.Mvvm.UI {
    public class EventToCommand : DevExpress.Mvvm.UI.Interactivity.EventTrigger {
        public static readonly DependencyProperty CommandParameterProperty = DependencyProperty.Register("CommandParameter", typeof(object), typeof(EventToCommand), new PropertyMetadata(null, (d, e) => ((EventToCommand)d).UpdateIsEnabled()));
        public static readonly DependencyProperty CommandProperty = DependencyProperty.Register("Command", typeof(ICommand), typeof(EventToCommand), new PropertyMetadata(null, (d, e) => ((EventToCommand)d).UpdateIsEnabled()));
        public static readonly DependencyProperty EventArgsConverterProperty = DependencyProperty.Register("EventArgsConverter", typeof(IEventArgsConverter), typeof(EventToCommand), new PropertyMetadata(null));
        public static readonly DependencyProperty PassEventArgsToCommandProperty = DependencyProperty.Register("PassEventArgsToCommand", typeof(bool), typeof(EventToCommand), new PropertyMetadata(false));
        public static readonly DependencyProperty AllowChangingEventOwnerIsEnabledProperty = DependencyProperty.Register("AllowChangingEventOwnerIsEnabled", typeof(bool), typeof(EventToCommand), new PropertyMetadata(false, (d, e) => ((EventToCommand)d).UpdateIsEnabled()));
        public static readonly DependencyProperty ProcessEventsFromDisabledEventOwnerProperty = DependencyProperty.Register("ProcessEventsFromDisabledEventOwner", typeof(bool), typeof(EventToCommand), new PropertyMetadata(true));
        public static readonly DependencyProperty MarkRoutedEventsAsHandledProperty = DependencyProperty.Register("MarkRoutedEventsAsHandled", typeof(bool), typeof(EventToCommand), new PropertyMetadata(false));
        public static readonly DependencyProperty UseDispatcherProperty = DependencyProperty.Register("UseDispatcher", typeof(bool), typeof(EventToCommand), new PropertyMetadata(false));
#if !SILVERLIGHT
        public static readonly DependencyProperty DispatcherPriorityProperty = DependencyProperty.Register("DispatcherPriority", typeof(DispatcherPriority), typeof(EventToCommand), new PropertyMetadata(DispatcherPriority.Normal));
#endif

        public ICommand Command {
            get { return (ICommand)GetValue(CommandProperty); }
            set { SetValue(CommandProperty, value); }
        }
        public object CommandParameter {
            get { return GetValue(CommandParameterProperty); }
            set { SetValue(CommandParameterProperty, value); }
        }
        public IEventArgsConverter EventArgsConverter {
            get { return (IEventArgsConverter)GetValue(EventArgsConverterProperty); }
            set { SetValue(EventArgsConverterProperty, value); }
        }
        public bool PassEventArgsToCommand {
            get { return (bool)GetValue(PassEventArgsToCommandProperty); }
            set { SetValue(PassEventArgsToCommandProperty, value); }
        }
        public bool AllowChangingEventOwnerIsEnabled {
            get { return (bool)GetValue(AllowChangingEventOwnerIsEnabledProperty); }
            set { SetValue(AllowChangingEventOwnerIsEnabledProperty, value); }
        }
        public bool ProcessEventsFromDisabledEventOwner {
            get { return (bool)GetValue(ProcessEventsFromDisabledEventOwnerProperty); }
            set { SetValue(ProcessEventsFromDisabledEventOwnerProperty, value); }
        }
        public bool MarkRoutedEventsAsHandled {
            get { return (bool)GetValue(MarkRoutedEventsAsHandledProperty); }
            set { SetValue(MarkRoutedEventsAsHandledProperty, value); }
        }
        public bool UseDispatcher {
            get { return (bool)GetValue(UseDispatcherProperty); }
            set { SetValue(UseDispatcherProperty, value); }
        }
#if !SILVERLIGHT
        public DispatcherPriority DispatcherPriority {
            get { return (DispatcherPriority)GetValue(DispatcherPriorityProperty); }
            set { SetValue(DispatcherPriorityProperty, value); }
        }
#endif

#if !SILVERLIGHT
        FrameworkElement AssociatedFrameworkObject { get { return Source as FrameworkElement; } }
#else
        Control AssociatedFrameworkObject { get { return Source as Control; } }
#endif

        protected override void OnEvent(object eventArgs) {
            base.OnEvent(eventArgs);
            if(UseDispatcher) {
#if !SILVERLIGHT
                Dispatcher.BeginInvoke(new Action<object>(Invoke), DispatcherPriority, eventArgs);
#else
                Dispatcher.BeginInvoke(new Action<object>(Invoke), eventArgs);
#endif
                return;
            }
            Invoke(eventArgs);
            if(MarkRoutedEventsAsHandled) {
#if !SILVERLIGHT
                if(eventArgs is RoutedEventArgs)
                    ((RoutedEventArgs)eventArgs).Handled = true;
#else
                if(eventArgs is MouseButtonEventArgs)
                    ((MouseButtonEventArgs)eventArgs).Handled = true;
                if(eventArgs is KeyEventArgs)
                    ((KeyEventArgs)eventArgs).Handled = true;
#endif
            }
        }
        protected override void OnAttached() {
            base.OnAttached();
            UpdateIsEnabled();
        }
        protected override void OnSourceChanged(object oldSource, object newSource) {
            base.OnSourceChanged(oldSource, newSource);
            UpdateIsEnabled();
        }
        void Invoke(object parameter) {
            if(!CanInvokeCommand())
                return;
            object commandParameter = CommandParameter;
            if(commandParameter == null && PassEventArgsToCommand) {
                if(EventArgsConverter != null)
                    parameter = EventArgsConverter.Convert(parameter);
                commandParameter = parameter;
            }
            if(Command != null && Command.CanExecute(commandParameter))
                Command.Execute(commandParameter);
        }
        bool CanInvokeCommand() {
            return ProcessEventsFromDisabledEventOwner || AssociatedFrameworkObject == null || AssociatedFrameworkObject.IsEnabled;
        }
        void UpdateIsEnabled() {
            if(AllowChangingEventOwnerIsEnabled && Command != null && AssociatedFrameworkObject != null) {
                AssociatedFrameworkObject.IsEnabled = Command.CanExecute(CommandParameter);
            }
        }
    }
    public interface IEventArgsConverter {
        object Convert(object args);
    }
    public abstract class EventArgsConverterBase<TArgs> : IEventArgsConverter {
        object IEventArgsConverter.Convert(object args) {
            return (args is TArgs) ? Convert((TArgs)args) : null;
        }
        protected abstract object Convert(TArgs args);
    }
}