using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

namespace DevExpress.Mvvm.UI {
    public abstract class EventToCommandBase : DevExpress.Mvvm.UI.Interactivity.EventTrigger {
        public static readonly DependencyProperty CommandProperty =
           DependencyProperty.Register("Command", typeof(ICommand), typeof(EventToCommandBase),
           new PropertyMetadata(null, (d, e) => ((EventToCommandBase)d).OnCommandChanged()));
        public static readonly DependencyProperty CommandParameterProperty =
           DependencyProperty.Register("CommandParameter", typeof(object), typeof(EventToCommandBase),
           new PropertyMetadata(null, (d, e) => ((EventToCommandBase)d).OnCommandParameterChanged()));
        public static readonly DependencyProperty ProcessEventsFromDisabledEventOwnerProperty =
            DependencyProperty.Register("ProcessEventsFromDisabledEventOwner", typeof(bool), typeof(EventToCommandBase),
            new PropertyMetadata(true));
        public static readonly DependencyProperty MarkRoutedEventsAsHandledProperty =
            DependencyProperty.Register("MarkRoutedEventsAsHandled", typeof(bool), typeof(EventToCommandBase),
            new PropertyMetadata(false));

        public static readonly DependencyProperty UseDispatcherProperty =
            DependencyProperty.Register("UseDispatcher", typeof(bool?), typeof(EventToCommandBase),
            new PropertyMetadata(null));
#if !SILVERLIGHT
        public static readonly DependencyProperty DispatcherPriorityProperty =
            DependencyProperty.Register("DispatcherPriority", typeof(DispatcherPriority?), typeof(EventToCommandBase),
            new PropertyMetadata(null));
#endif

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
#if !SILVERLIGHT
                    return DispatcherPriority != null;
#else
                    return false;
#endif
                }
                return UseDispatcher.Value;
            }
        }
#if !SILVERLIGHT
        public DispatcherPriority? DispatcherPriority {
            get { return (DispatcherPriority?)GetValue(DispatcherPriorityProperty); }
            set { SetValue(DispatcherPriorityProperty, value); }
        }
        protected internal DispatcherPriority ActualDispatcherPriority {
            get { return DispatcherPriority ?? System.Windows.Threading.DispatcherPriority.Normal; }
        }
#endif

        protected override void OnEvent(object sender, object eventArgs) {
            base.OnEvent(sender, eventArgs);
            if(Command != null) {
                OnEventCore(sender, eventArgs);
                return;
            }
#if !SILVERLIGHT
            bool commandIsBound = System.Windows.Data.BindingOperations.GetBindingExpression(this, CommandProperty) != null;
            if(Command == null && commandIsBound) {
                Dispatcher.BeginInvoke(new Action(() => {
                    OnEventCore(sender, eventArgs);
                }));
            }
#endif
        }
        void OnEventCore(object sender, object eventArgs) {
            if(Command == null) return;
            if(!CanInvoke(sender, eventArgs)) return;
            if(!ActualUseDispatcher)
                Invoke(sender, eventArgs);
            else {
#if !SILVERLIGHT
                Dispatcher.BeginInvoke(new Action<object, object>(Invoke), ActualDispatcherPriority, new object[] { sender, eventArgs });
#else
                Dispatcher.BeginInvoke(new Action<object, object>(Invoke), new object[] { sender, eventArgs });
#endif
            }
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
        protected abstract void Invoke(object sender, object eventArgs);
        protected virtual bool CanInvoke(object sender, object eventArgs) {
#if !SILVERLIGHT
            FrameworkElement associatedFrameworkObject = Source as FrameworkElement;
#else
            Control associatedFrameworkObject = Source as Control;
#endif
            return ProcessEventsFromDisabledEventOwner || associatedFrameworkObject == null || associatedFrameworkObject.IsEnabled;
        }
        protected virtual void OnCommandChanged() { }
        protected virtual void OnCommandParameterChanged() { }
    }
}