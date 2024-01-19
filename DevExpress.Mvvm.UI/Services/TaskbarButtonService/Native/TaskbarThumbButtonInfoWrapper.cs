using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Shell;
using DevExpress.Mvvm.Native;

namespace DevExpress.Mvvm.UI.Native {
    public class TaskbarThumbButtonInfoWrapper {
        class ThumbButtonInfoCommand : ICommand {
            public ThumbButtonInfoCommand(TaskbarThumbButtonInfo taskbarThumbButtonInfo) {
                ITaskbarThumbButtonInfo taskbarThumbButtonInfoInternal = taskbarThumbButtonInfo;
                Click = taskbarThumbButtonInfoInternal.Click;
                Action = taskbarThumbButtonInfo.Action;
                InternalCommand = taskbarThumbButtonInfo.Command;
            }
            public Action Action { get; private set; }
            public ICommand InternalCommand { get; private set; }
            public EventHandler Click { get; private set; }
            public event EventHandler CanExecuteChanged {
                add {
                    if(InternalCommand != null)
                        InternalCommand.CanExecuteChanged += value;
                }
                remove {
                    if(InternalCommand != null)
                        InternalCommand.CanExecuteChanged -= value;
                }
            }
            public bool CanExecute(object parameter) {
                return InternalCommand == null || InternalCommand.CanExecute(parameter);
            }
            public void Execute(object parameter) {
                if(Action != null)
                    Action();
                if(Click != null)
                    Click(this, EventArgs.Empty);
                if(InternalCommand != null && InternalCommand.CanExecute(parameter))
                    InternalCommand.Execute(parameter);
            }
        }
        #region Dependency Properties
        public static readonly DependencyProperty TaskbarThumbButtonInfoProperty =
            DependencyProperty.RegisterAttached("TaskbarThumbButtonInfo", typeof(Container<TaskbarThumbButtonInfo>), typeof(TaskbarThumbButtonInfoWrapper), new PropertyMetadata(new Container<TaskbarThumbButtonInfo>(),
                OnTaskbarThumbButtonInfoChanged));
        public static readonly DependencyProperty ThumbButtonInfoProperty =
            DependencyProperty.RegisterAttached("ThumbButtonInfo", typeof(Container<ThumbButtonInfo>), typeof(TaskbarThumbButtonInfoWrapper), new PropertyMetadata(new Container<ThumbButtonInfo>(),
                OnThumbButtonInfoChanged));
        public static readonly DependencyProperty DoNotProcessPropertyChangedProperty =
            DependencyProperty.RegisterAttached("DoNotProcessPropertyChanged", typeof(bool), typeof(TaskbarThumbButtonInfoWrapper), new PropertyMetadata(false));
        #endregion
        #region ThumbButtonInfo Dependency Properties
        [IgnoreDependencyPropertiesConsistencyChecker]
        static readonly DependencyProperty ThumbButtonInfoCommandParameterProperty =
            DependencyProperty.RegisterAttached("ThumbButtonInfoCommandParameter", typeof(object), typeof(TaskbarThumbButtonInfoWrapper), new PropertyMetadata(new object(),
                (d, e) => OnThumbButtonInfoPropertyChanged(d, e, (x, y) => CopyIfNeeded(y.CommandParameter, x.CommandParameter, v => y.CommandParameter = v))));
        [IgnoreDependencyPropertiesConsistencyChecker]
        static readonly DependencyProperty ThumbButtonInfoCommandProperty =
            DependencyProperty.RegisterAttached("ThumbButtonInfoCommand", typeof(object), typeof(TaskbarThumbButtonInfoWrapper), new PropertyMetadata(new object(),
                (d, e) => OnThumbButtonInfoPropertyChanged(d, e, (x, y) => CopyCommandIsNeeded(x, y))));
        [IgnoreDependencyPropertiesConsistencyChecker]
        static readonly DependencyProperty ThumbButtonInfoDescriptionProperty =
            DependencyProperty.RegisterAttached("ThumbButtonInfoDescription", typeof(object), typeof(TaskbarThumbButtonInfoWrapper), new PropertyMetadata(new object(),
                (d, e) => OnThumbButtonInfoPropertyChanged(d, e, (x, y) => CopyIfNeeded(y.Description, x.Description, v => y.Description = v))));
        [IgnoreDependencyPropertiesConsistencyChecker]
        static readonly DependencyProperty ThumbButtonInfoDismissWhenClickedProperty =
            DependencyProperty.RegisterAttached("ThumbButtonInfoDismissWhenClicked", typeof(object), typeof(TaskbarThumbButtonInfoWrapper), new PropertyMetadata(new object(),
                (d, e) => OnThumbButtonInfoPropertyChanged(d, e, (x, y) => CopyIfNeeded(y.DismissWhenClicked, x.DismissWhenClicked, v => y.DismissWhenClicked = v))));
        [IgnoreDependencyPropertiesConsistencyChecker]
        static readonly DependencyProperty ThumbButtonInfoImageSourceProperty =
            DependencyProperty.RegisterAttached("ThumbButtonInfoImageSource", typeof(object), typeof(TaskbarThumbButtonInfoWrapper), new PropertyMetadata(new object(),
                (d, e) => OnThumbButtonInfoPropertyChanged(d, e, (x, y) => CopyIfNeeded(y.ImageSource, x.ImageSource, v => y.ImageSource = v))));
        [IgnoreDependencyPropertiesConsistencyChecker]
        static readonly DependencyProperty ThumbButtonInfoIsBackgroundVisibleProperty =
            DependencyProperty.RegisterAttached("ThumbButtonInfoIsBackgroundVisible", typeof(object), typeof(TaskbarThumbButtonInfoWrapper), new PropertyMetadata(new object(),
                (d, e) => OnThumbButtonInfoPropertyChanged(d, e, (x, y) => CopyIfNeeded(y.IsBackgroundVisible, x.IsBackgroundVisible, v => y.IsBackgroundVisible = v))));
        [IgnoreDependencyPropertiesConsistencyChecker]
        static readonly DependencyProperty ThumbButtonInfoIsEnabledProperty =
            DependencyProperty.RegisterAttached("ThumbButtonInfoIsEnabled", typeof(object), typeof(TaskbarThumbButtonInfoWrapper), new PropertyMetadata(new object(),
                (d, e) => OnThumbButtonInfoPropertyChanged(d, e, (x, y) => CopyIfNeeded(y.IsEnabled, x.IsEnabled, v => y.IsEnabled = v))));
        [IgnoreDependencyPropertiesConsistencyChecker]
        static readonly DependencyProperty ThumbButtonInfoIsInteractiveProperty =
            DependencyProperty.RegisterAttached("ThumbButtonInfoIsInteractive", typeof(object), typeof(TaskbarThumbButtonInfoWrapper), new PropertyMetadata(new object(),
                (d, e) => OnThumbButtonInfoPropertyChanged(d, e, (x, y) => CopyIfNeeded(y.IsInteractive, x.IsInteractive, v => y.IsInteractive = v))));
        [IgnoreDependencyPropertiesConsistencyChecker]
        static readonly DependencyProperty ThumbButtonInfoVisibilityProperty =
            DependencyProperty.RegisterAttached("ThumbButtonInfoVisibility", typeof(object), typeof(TaskbarThumbButtonInfoWrapper), new PropertyMetadata(new object(),
                (d, e) => OnThumbButtonInfoPropertyChanged(d, e, (x, y) => CopyIfNeeded(y.Visibility, x.Visibility, v => y.Visibility = v))));
        #endregion
        #region TaskbarThumbButtonInfo Dependency Properties
        [IgnoreDependencyPropertiesConsistencyChecker]
        static readonly DependencyProperty TaskbarThumbButtonInfoCommandParameterProperty =
            DependencyProperty.RegisterAttached("TaskbarThumbButtonInfoCommandParameter", typeof(object), typeof(TaskbarThumbButtonInfoWrapper), new PropertyMetadata(new object(),
                (d, e) => OnTaskbarThumbButtonInfoPropertyChanged(d, e, (x, y) => y.CommandParameter = x.CommandParameter)));
        [IgnoreDependencyPropertiesConsistencyChecker]
        static readonly DependencyProperty TaskbarThumbButtonInfoCommandProperty =
            DependencyProperty.RegisterAttached("TaskbarThumbButtonInfoCommand", typeof(object), typeof(TaskbarThumbButtonInfoWrapper), new PropertyMetadata(new object(),
                (d, e) => OnTaskbarThumbButtonInfoPropertyChanged(d, e, (x, y) => y.Command = new ThumbButtonInfoCommand(x))));
        [IgnoreDependencyPropertiesConsistencyChecker]
        static readonly DependencyProperty TaskbarThumbButtonInfoDescriptionProperty =
            DependencyProperty.RegisterAttached("TaskbarThumbButtonInfoDescription", typeof(object), typeof(TaskbarThumbButtonInfoWrapper), new PropertyMetadata(new object(),
                (d, e) => OnTaskbarThumbButtonInfoPropertyChanged(d, e, (x, y) => y.Description = x.Description)));
        [IgnoreDependencyPropertiesConsistencyChecker]
        static readonly DependencyProperty TaskbarThumbButtonInfoDismissWhenClickedProperty =
            DependencyProperty.RegisterAttached("TaskbarThumbButtonInfoDismissWhenClicked", typeof(object), typeof(TaskbarThumbButtonInfoWrapper), new PropertyMetadata(new object(),
                (d, e) => OnTaskbarThumbButtonInfoPropertyChanged(d, e, (x, y) => y.DismissWhenClicked = x.DismissWhenClicked)));
        [IgnoreDependencyPropertiesConsistencyChecker]
        static readonly DependencyProperty TaskbarThumbButtonInfoImageSourceProperty =
            DependencyProperty.RegisterAttached("TaskbarThumbButtonInfoImageSource", typeof(object), typeof(TaskbarThumbButtonInfoWrapper), new PropertyMetadata(new object(),
                (d, e) => OnTaskbarThumbButtonInfoPropertyChanged(d, e, (x, y) => y.ImageSource = x.ImageSource)));
        [IgnoreDependencyPropertiesConsistencyChecker]
        static readonly DependencyProperty TaskbarThumbButtonInfoIsBackgroundVisibleProperty =
            DependencyProperty.RegisterAttached("TaskbarThumbButtonInfoIsBackgroundVisible", typeof(object), typeof(TaskbarThumbButtonInfoWrapper), new PropertyMetadata(new object(),
                (d, e) => OnTaskbarThumbButtonInfoPropertyChanged(d, e, (x, y) => y.IsBackgroundVisible = x.IsBackgroundVisible)));
        [IgnoreDependencyPropertiesConsistencyChecker]
        static readonly DependencyProperty TaskbarThumbButtonInfoIsEnabledProperty =
            DependencyProperty.RegisterAttached("TaskbarThumbButtonInfoIsEnabled", typeof(object), typeof(TaskbarThumbButtonInfoWrapper), new PropertyMetadata(new object(),
                (d, e) => OnTaskbarThumbButtonInfoPropertyChanged(d, e, (x, y) => y.IsEnabled = x.IsEnabled)));
        [IgnoreDependencyPropertiesConsistencyChecker]
        static readonly DependencyProperty TaskbarThumbButtonInfoIsInteractiveProperty =
            DependencyProperty.RegisterAttached("TaskbarThumbButtonInfoIsInteractive", typeof(object), typeof(TaskbarThumbButtonInfoWrapper), new PropertyMetadata(new object(),
                (d, e) => OnTaskbarThumbButtonInfoPropertyChanged(d, e, (x, y) => y.IsInteractive = x.IsInteractive)));
        [IgnoreDependencyPropertiesConsistencyChecker]
        static readonly DependencyProperty TaskbarThumbButtonInfoVisibilityProperty =
            DependencyProperty.RegisterAttached("TaskbarThumbButtonInfoVisibility", typeof(object), typeof(TaskbarThumbButtonInfoWrapper), new PropertyMetadata(new object(),
                (d, e) => OnTaskbarThumbButtonInfoPropertyChanged(d, e, (x, y) => y.Visibility = x.Visibility)));
        #endregion
        public static Container<TaskbarThumbButtonInfo> GetTaskbarThumbButtonInfo(ThumbButtonInfo d) { return (Container<TaskbarThumbButtonInfo>)d.GetValue(TaskbarThumbButtonInfoProperty); }
        public static void SetTaskbarThumbButtonInfo(ThumbButtonInfo d, Container<TaskbarThumbButtonInfo> value) { d.SetValue(TaskbarThumbButtonInfoProperty, value); }
        public static Container<ThumbButtonInfo> GetThumbButtonInfo(TaskbarThumbButtonInfo d) { return (Container<ThumbButtonInfo>)d.GetValue(ThumbButtonInfoProperty); }
        public static void SetThumbButtonInfo(TaskbarThumbButtonInfo d, Container<ThumbButtonInfo> value) { d.SetValue(ThumbButtonInfoProperty, value); }
        public static bool GetDoNotProcessPropertyChanged(DependencyObject d) { return (bool)d.GetValue(DoNotProcessPropertyChangedProperty); }
        public static void SetDoNotProcessPropertyChanged(DependencyObject d, bool value) { d.SetValue(DoNotProcessPropertyChangedProperty, value); }

        public static ThumbButtonInfo Wrap(TaskbarThumbButtonInfo taskbarThumbButtonInfo) {
            ThumbButtonInfo thumbButtonInfo = GetThumbButtonInfo(taskbarThumbButtonInfo).Content;
            if(thumbButtonInfo == null) {
                thumbButtonInfo = new ThumbButtonInfo();
                ButtonInfoSetProperty(taskbarThumbButtonInfo, thumbButtonInfo, ThumbButtonInfoSetPropertiesCore);
                SetTaskbarThumbButtonInfo(thumbButtonInfo, new Container<TaskbarThumbButtonInfo>(taskbarThumbButtonInfo));
            }
            return thumbButtonInfo;
        }
        public static TaskbarThumbButtonInfo UnWrap(ThumbButtonInfo thumbButtonInfo) {
            TaskbarThumbButtonInfo taskbarThumbButtonInfo = GetTaskbarThumbButtonInfo(thumbButtonInfo).Content;
            if(taskbarThumbButtonInfo == null) {
                taskbarThumbButtonInfo = new TaskbarThumbButtonInfo();
                ButtonInfoSetProperty(thumbButtonInfo, taskbarThumbButtonInfo, TaskbarThumbButtonInfoSetPropertiesCore);
                SetThumbButtonInfo(taskbarThumbButtonInfo, new Container<ThumbButtonInfo>(thumbButtonInfo));
            }
            return taskbarThumbButtonInfo;
        }
        static void OnTaskbarThumbButtonInfoChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            ThumbButtonInfo thumbButtonInfo = (ThumbButtonInfo)d;
            Container<TaskbarThumbButtonInfo> newValue = (Container<TaskbarThumbButtonInfo>)e.NewValue;
            Container<TaskbarThumbButtonInfo> oldValue = (Container<TaskbarThumbButtonInfo>)e.OldValue;
            if(oldValue.Content != null) {
                UnsubscribeFromTaskbarThumbButtonInfoPropertiesChanged(oldValue.Content);
                if(GetThumbButtonInfo(oldValue.Content).Content == thumbButtonInfo)
                    SetThumbButtonInfo(oldValue.Content, new Container<ThumbButtonInfo>(null));
            }
            if(newValue.Content != null) {
                SetThumbButtonInfo(newValue.Content, new Container<ThumbButtonInfo>(thumbButtonInfo));
                SubscribeToTaskbarThumbButtonInfoPropertiesChanged(newValue.Content);
            }
        }

        static void OnThumbButtonInfoChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            TaskbarThumbButtonInfo taskbarThumbButtonInfo = (TaskbarThumbButtonInfo)d;
            Container<ThumbButtonInfo> newValue = (Container<ThumbButtonInfo>)e.NewValue;
            Container<ThumbButtonInfo> oldValue = (Container<ThumbButtonInfo>)e.OldValue;
            if(oldValue.Content != null) {
                UnsubscribeFromThumbButtonInfoPropertiesChanged(oldValue.Content);
                if(GetTaskbarThumbButtonInfo(oldValue.Content).Content == taskbarThumbButtonInfo)
                    SetTaskbarThumbButtonInfo(oldValue.Content, new Container<TaskbarThumbButtonInfo>(null));
            }
            if(newValue.Content != null) {
                SetTaskbarThumbButtonInfo(newValue.Content, new Container<TaskbarThumbButtonInfo>(taskbarThumbButtonInfo));
                SubscribeToThumbButtonInfoPropertiesChanged(newValue.Content);
            }
        }
        static void SubscribeToThumbButtonInfoPropertiesChanged(ThumbButtonInfo thumbButtonInfo) {
            BindingOperations.SetBinding(thumbButtonInfo, ThumbButtonInfoCommandParameterProperty, new Binding("CommandParameter") { Source = thumbButtonInfo, Mode = BindingMode.OneWay });
            BindingOperations.SetBinding(thumbButtonInfo, ThumbButtonInfoCommandProperty, new Binding("Command") { Source = thumbButtonInfo, Mode = BindingMode.OneWay });
            BindingOperations.SetBinding(thumbButtonInfo, ThumbButtonInfoDescriptionProperty, new Binding("Description") { Source = thumbButtonInfo, Mode = BindingMode.OneWay });
            BindingOperations.SetBinding(thumbButtonInfo, ThumbButtonInfoDismissWhenClickedProperty, new Binding("DismissWhenClicked") { Source = thumbButtonInfo, Mode = BindingMode.OneWay });
            BindingOperations.SetBinding(thumbButtonInfo, ThumbButtonInfoImageSourceProperty, new Binding("ImageSource") { Source = thumbButtonInfo, Mode = BindingMode.OneWay });
            BindingOperations.SetBinding(thumbButtonInfo, ThumbButtonInfoIsBackgroundVisibleProperty, new Binding("IsBackgroundVisible") { Source = thumbButtonInfo, Mode = BindingMode.OneWay });
            BindingOperations.SetBinding(thumbButtonInfo, ThumbButtonInfoIsEnabledProperty, new Binding("IsEnabled") { Source = thumbButtonInfo, Mode = BindingMode.OneWay });
            BindingOperations.SetBinding(thumbButtonInfo, ThumbButtonInfoIsInteractiveProperty, new Binding("IsInteractive") { Source = thumbButtonInfo, Mode = BindingMode.OneWay });
            BindingOperations.SetBinding(thumbButtonInfo, ThumbButtonInfoVisibilityProperty, new Binding("Visibility") { Source = thumbButtonInfo, Mode = BindingMode.OneWay });
        }
        static void UnsubscribeFromThumbButtonInfoPropertiesChanged(ThumbButtonInfo thumbButtonInfo) {
            ClearListenPropertyBinding(thumbButtonInfo, ThumbButtonInfoCommandParameterProperty);
            ClearListenPropertyBinding(thumbButtonInfo, ThumbButtonInfoCommandProperty);
            ClearListenPropertyBinding(thumbButtonInfo, ThumbButtonInfoDescriptionProperty);
            ClearListenPropertyBinding(thumbButtonInfo, ThumbButtonInfoDismissWhenClickedProperty);
            ClearListenPropertyBinding(thumbButtonInfo, ThumbButtonInfoImageSourceProperty);
            ClearListenPropertyBinding(thumbButtonInfo, ThumbButtonInfoIsBackgroundVisibleProperty);
            ClearListenPropertyBinding(thumbButtonInfo, ThumbButtonInfoIsEnabledProperty);
            ClearListenPropertyBinding(thumbButtonInfo, ThumbButtonInfoIsInteractiveProperty);
            ClearListenPropertyBinding(thumbButtonInfo, ThumbButtonInfoVisibilityProperty);
        }
        static void SubscribeToTaskbarThumbButtonInfoPropertiesChanged(TaskbarThumbButtonInfo taskbarThumbButtonInfo) {
            BindingOperations.SetBinding(taskbarThumbButtonInfo, TaskbarThumbButtonInfoCommandParameterProperty, new Binding("CommandParameter") { Source = taskbarThumbButtonInfo, Mode = BindingMode.OneWay });
            BindingOperations.SetBinding(taskbarThumbButtonInfo, TaskbarThumbButtonInfoCommandProperty, new Binding("Command") { Source = taskbarThumbButtonInfo, Mode = BindingMode.OneWay });
            BindingOperations.SetBinding(taskbarThumbButtonInfo, TaskbarThumbButtonInfoDescriptionProperty, new Binding("Description") { Source = taskbarThumbButtonInfo, Mode = BindingMode.OneWay });
            BindingOperations.SetBinding(taskbarThumbButtonInfo, TaskbarThumbButtonInfoDismissWhenClickedProperty, new Binding("DismissWhenClicked") { Source = taskbarThumbButtonInfo, Mode = BindingMode.OneWay });
            BindingOperations.SetBinding(taskbarThumbButtonInfo, TaskbarThumbButtonInfoImageSourceProperty, new Binding("ImageSource") { Source = taskbarThumbButtonInfo, Mode = BindingMode.OneWay });
            BindingOperations.SetBinding(taskbarThumbButtonInfo, TaskbarThumbButtonInfoIsBackgroundVisibleProperty, new Binding("IsBackgroundVisible") { Source = taskbarThumbButtonInfo, Mode = BindingMode.OneWay });
            BindingOperations.SetBinding(taskbarThumbButtonInfo, TaskbarThumbButtonInfoIsEnabledProperty, new Binding("IsEnabled") { Source = taskbarThumbButtonInfo, Mode = BindingMode.OneWay });
            BindingOperations.SetBinding(taskbarThumbButtonInfo, TaskbarThumbButtonInfoIsInteractiveProperty, new Binding("IsInteractive") { Source = taskbarThumbButtonInfo, Mode = BindingMode.OneWay });
            BindingOperations.SetBinding(taskbarThumbButtonInfo, TaskbarThumbButtonInfoVisibilityProperty, new Binding("Visibility") { Source = taskbarThumbButtonInfo, Mode = BindingMode.OneWay });
        }
        static void UnsubscribeFromTaskbarThumbButtonInfoPropertiesChanged(TaskbarThumbButtonInfo taskbarThumbButtonInfo) {
            ClearListenPropertyBinding(taskbarThumbButtonInfo, TaskbarThumbButtonInfoCommandParameterProperty);
            ClearListenPropertyBinding(taskbarThumbButtonInfo, TaskbarThumbButtonInfoCommandProperty);
            ClearListenPropertyBinding(taskbarThumbButtonInfo, TaskbarThumbButtonInfoDescriptionProperty);
            ClearListenPropertyBinding(taskbarThumbButtonInfo, TaskbarThumbButtonInfoDismissWhenClickedProperty);
            ClearListenPropertyBinding(taskbarThumbButtonInfo, TaskbarThumbButtonInfoImageSourceProperty);
            ClearListenPropertyBinding(taskbarThumbButtonInfo, TaskbarThumbButtonInfoIsBackgroundVisibleProperty);
            ClearListenPropertyBinding(taskbarThumbButtonInfo, TaskbarThumbButtonInfoIsEnabledProperty);
            ClearListenPropertyBinding(taskbarThumbButtonInfo, TaskbarThumbButtonInfoIsInteractiveProperty);
            ClearListenPropertyBinding(taskbarThumbButtonInfo, TaskbarThumbButtonInfoVisibilityProperty);
        }
        static void ClearListenPropertyBinding(DependencyObject source, DependencyProperty property) {
            source.SetValue(property, source.GetValue(property));
        }
        static void OnThumbButtonInfoPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e, Action<ThumbButtonInfo, TaskbarThumbButtonInfo> setPropertyAction) {
            ThumbButtonInfo thumbButtonInfo = (ThumbButtonInfo)d;
            if(GetDoNotProcessPropertyChanged(thumbButtonInfo)) return;
            TaskbarThumbButtonInfo taskbarThumbButtonInfo = GetTaskbarThumbButtonInfo(thumbButtonInfo).Content;
            ButtonInfoSetProperty(thumbButtonInfo, taskbarThumbButtonInfo, setPropertyAction);
        }
        static void OnTaskbarThumbButtonInfoPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e, Action<TaskbarThumbButtonInfo, ThumbButtonInfo> setPropertyAction) {
            TaskbarThumbButtonInfo taskbarThumbButtonInfo = (TaskbarThumbButtonInfo)d;
            if(GetDoNotProcessPropertyChanged(taskbarThumbButtonInfo)) return;
            ThumbButtonInfo thumbButtonInfo = GetThumbButtonInfo(taskbarThumbButtonInfo).Content;
            ButtonInfoSetProperty(taskbarThumbButtonInfo, thumbButtonInfo, setPropertyAction);
        }
        static bool BeginSetProperties(DependencyObject d) {
            bool ret = GetDoNotProcessPropertyChanged(d);
            SetDoNotProcessPropertyChanged(d, true);
            return ret;
        }
        static void EndSetProperties(DependencyObject d, bool oldValue) {
            SetDoNotProcessPropertyChanged(d, oldValue);
        }
        static void ButtonInfoSetProperty<TSource, TDest>(TSource sourceInfo, TDest destInfo, Action<TSource, TDest> setPropertyAction) where TSource : DependencyObject where TDest : DependencyObject {
            bool oldValue = BeginSetProperties(destInfo);
            try {
                setPropertyAction(sourceInfo, destInfo);
            } finally {
                EndSetProperties(destInfo, oldValue);
            }
        }
        static void ThumbButtonInfoSetPropertiesCore(TaskbarThumbButtonInfo taskbarThumbButtonInfo, ThumbButtonInfo thumbButtonInfo) {
            thumbButtonInfo.CommandParameter = taskbarThumbButtonInfo.CommandParameter;
            thumbButtonInfo.Command = new ThumbButtonInfoCommand(taskbarThumbButtonInfo);
            thumbButtonInfo.Description = taskbarThumbButtonInfo.Description;
            thumbButtonInfo.DismissWhenClicked = taskbarThumbButtonInfo.DismissWhenClicked;
            thumbButtonInfo.ImageSource = taskbarThumbButtonInfo.ImageSource;
            thumbButtonInfo.IsBackgroundVisible = taskbarThumbButtonInfo.IsBackgroundVisible;
            thumbButtonInfo.IsEnabled = taskbarThumbButtonInfo.IsEnabled;
            thumbButtonInfo.IsInteractive = taskbarThumbButtonInfo.IsInteractive;
            thumbButtonInfo.Visibility = taskbarThumbButtonInfo.Visibility;
        }
        static void TaskbarThumbButtonInfoSetPropertiesCore(ThumbButtonInfo thumbButtonInfo, TaskbarThumbButtonInfo taskbarThumbButtonInfo) {
            CopyIfNeeded(taskbarThumbButtonInfo.CommandParameter, thumbButtonInfo.CommandParameter, x => taskbarThumbButtonInfo.CommandParameter = x);
            CopyCommandIsNeeded(thumbButtonInfo, taskbarThumbButtonInfo);
            CopyIfNeeded(taskbarThumbButtonInfo.Description, thumbButtonInfo.Description, x => taskbarThumbButtonInfo.Description = x);
            CopyIfNeeded(taskbarThumbButtonInfo.DismissWhenClicked, thumbButtonInfo.DismissWhenClicked, x => taskbarThumbButtonInfo.DismissWhenClicked = x);
            CopyIfNeeded(taskbarThumbButtonInfo.ImageSource, thumbButtonInfo.ImageSource, x => taskbarThumbButtonInfo.ImageSource = x);
            CopyIfNeeded(taskbarThumbButtonInfo.IsBackgroundVisible, thumbButtonInfo.IsBackgroundVisible, x => taskbarThumbButtonInfo.IsBackgroundVisible = x);
            CopyIfNeeded(taskbarThumbButtonInfo.IsEnabled, thumbButtonInfo.IsEnabled, x => taskbarThumbButtonInfo.IsEnabled = x);
            CopyIfNeeded(taskbarThumbButtonInfo.IsInteractive, thumbButtonInfo.IsInteractive, x => taskbarThumbButtonInfo.IsInteractive = x);
            CopyIfNeeded(taskbarThumbButtonInfo.Visibility, thumbButtonInfo.Visibility, x => taskbarThumbButtonInfo.Visibility = x);
        }
        static void CopyCommandIsNeeded(ThumbButtonInfo thumbButtonInfo, TaskbarThumbButtonInfo taskbarThumbButtonInfo) {
            ThumbButtonInfoCommand command = thumbButtonInfo.Command as ThumbButtonInfoCommand;
            if(command != null) {
                CopyIfNeeded(taskbarThumbButtonInfo.Command, command.InternalCommand, x => taskbarThumbButtonInfo.Command = x);
                CopyIfNeeded(taskbarThumbButtonInfo.Action, command.Action, x => taskbarThumbButtonInfo.Action = x);
                ITaskbarThumbButtonInfo taskbarThumbButtonInfoInternal = taskbarThumbButtonInfo;
                CopyIfNeeded(taskbarThumbButtonInfoInternal.Click, command.Click, x => taskbarThumbButtonInfoInternal.Click = x);
            } else {
                taskbarThumbButtonInfo.Command = command;
            }
        }
        static void CopyIfNeeded<T>(T oldValue, T newValue, Action<T> setPropertyAction) {
            if(!object.Equals(oldValue, newValue))
                setPropertyAction(newValue);
        }
    }
}