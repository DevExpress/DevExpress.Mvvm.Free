#pragma warning disable 612,618
using DevExpress.Mvvm.Native;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace DevExpress.Mvvm {
    public abstract class CommandBase {
#if !SILVERLIGHT
        static bool defaultUseCommandManager = true;

        public static bool DefaultUseCommandManager { get { return defaultUseCommandManager; } set { defaultUseCommandManager = value; } }
#endif
    }
    public abstract class CommandBase<T> : CommandBase, ICommand, IDelegateCommand {
        protected Func<T, bool> canExecuteMethod = null;
        protected bool useCommandManager;
        event EventHandler canExecuteChanged;

        public event EventHandler CanExecuteChanged {
            add {
                if(useCommandManager) {
#if !SILVERLIGHT
                    CommandManager.RequerySuggested += value;
#endif
                } else {
                    canExecuteChanged += value;
                }
            }
            remove {
                if(useCommandManager) {
#if !SILVERLIGHT
                    CommandManager.RequerySuggested -= value;
#endif
                } else {
                    canExecuteChanged -= value;
                }
            }
        }

#if !SILVERLIGHT
        public CommandBase(bool? useCommandManager = null) {
            this.useCommandManager = useCommandManager ?? DefaultUseCommandManager;
        }
#else
        public CommandBase() {
            this.useCommandManager = false;
        }
#endif

        public virtual bool CanExecute(T parameter) {
            if(canExecuteMethod == null) return true;
            return canExecuteMethod(parameter);
        }
        public abstract void Execute(T parameter);

        public void RaiseCanExecuteChanged() {
            if(useCommandManager) {
#if !SILVERLIGHT
                CommandManager.InvalidateRequerySuggested();
#endif
            } else {
                OnCanExecuteChanged();
            }
        }
        protected virtual void OnCanExecuteChanged() {
            if(canExecuteChanged != null)
                canExecuteChanged(this, EventArgs.Empty);
        }
        bool ICommand.CanExecute(object parameter) {
            return CanExecute(GetGenericParameter(parameter, true));
        }
        void ICommand.Execute(object parameter) {
            Execute(GetGenericParameter(parameter));
        }
        static T GetGenericParameter(object parameter, bool suppressCastException = false) {
            Type targetType = Nullable.GetUnderlyingType(typeof(T)) ?? typeof(T);
            if(targetType.IsEnum && parameter is string) {
                parameter = Enum.Parse(targetType, (string)parameter, false);
            } else if(parameter is IConvertible && !typeof(T).IsAssignableFrom(parameter.GetType())) {
                parameter = Convert.ChangeType(parameter, targetType, CultureInfo.InvariantCulture);
            }
            if(parameter == null) return default(T);
            if(parameter is T) return (T)parameter;
            if(suppressCastException) return default(T);
            throw new InvalidCastException(string.Format("CommandParameter: Unable to cast object of type '{0}' to type '{1}'", parameter.GetType().FullName, typeof(T).FullName));
        }
    }
    public abstract class DelegateCommandBase<T> : CommandBase<T> {
        protected Action<T> executeMethod = null;

        void Init(Action<T> executeMethod, Func<T, bool> canExecuteMethod) {
            if(executeMethod == null && canExecuteMethod == null)
                throw new ArgumentNullException("executeMethod");
            this.executeMethod = executeMethod;
            this.canExecuteMethod = canExecuteMethod;
        }
#if !SILVERLIGHT
        public DelegateCommandBase(Action<T> executeMethod)
            : this(executeMethod, null, null) {
        }
        public DelegateCommandBase(Action<T> executeMethod, bool useCommandManager)
            : this(executeMethod, null, useCommandManager) {
        }
        public DelegateCommandBase(Action<T> executeMethod, Func<T, bool> canExecuteMethod, bool? useCommandManager = null)
            : base(useCommandManager) {
            Init(executeMethod, canExecuteMethod);
        }
#else
            public DelegateCommandBase(Action<T> executeMethod)
                : this(executeMethod, null) {
            }
            public DelegateCommandBase(Action<T> executeMethod, Func<T, bool> canExecuteMethod)
            :base(){
                Init(executeMethod, canExecuteMethod);
            }
#endif
    }
    public abstract class AsyncCommandBase<T> : CommandBase<T>, INotifyPropertyChanged {
        protected Func<T, Task> executeMethod = null;

        void Init(Func<T, Task> executeMethod, Func<T, bool> canExecuteMethod) {
            if(executeMethod == null && canExecuteMethod == null)
                throw new ArgumentNullException("executeMethod");
            this.executeMethod = executeMethod;
            this.canExecuteMethod = canExecuteMethod;
        }

#if !SILVERLIGHT
        public AsyncCommandBase(Func<T, Task> executeMethod)
            : this(executeMethod, null, null) {
        }
        public AsyncCommandBase(Func<T, Task> executeMethod, bool useCommandManager)
            : this(executeMethod, null, useCommandManager) {
        }
        public AsyncCommandBase(Func<T, Task> executeMethod, Func<T, bool> canExecuteMethod, bool? useCommandManager = null)
            : base(useCommandManager) {
            Init(executeMethod, canExecuteMethod);
        }
#else
        public AsyncCommandBase(Func<T, Task> executeMethod)
            : this(executeMethod, null) {
        }
        public AsyncCommandBase(Func<T, Task> executeMethod, Func<T, bool> canExecuteMethod)
        :base(){
            Init(executeMethod, canExecuteMethod);
        }
#endif

        event PropertyChangedEventHandler propertyChanged;
        event PropertyChangedEventHandler INotifyPropertyChanged.PropertyChanged {
            add { propertyChanged += value; }
            remove { propertyChanged -= value; }
        }
        protected void RaisePropertyChanged(string propName) {
            if(propertyChanged != null)
                propertyChanged(this, new PropertyChangedEventArgs(propName));
        }
    }
    public class DelegateCommand<T> : DelegateCommandBase<T> {
#if !SILVERLIGHT
        public DelegateCommand(Action<T> executeMethod)
            : this(executeMethod, null, null) {
        }
        public DelegateCommand(Action<T> executeMethod, bool useCommandManager)
            : this(executeMethod, null, useCommandManager) {
        }
        public DelegateCommand(Action<T> executeMethod, Func<T, bool> canExecuteMethod, bool? useCommandManager = null)
            : base(executeMethod, canExecuteMethod, useCommandManager) {
        }
#else
        public DelegateCommand(Action<T> executeMethod)
            : this(executeMethod, null) {
        }
        public DelegateCommand(Action<T> executeMethod, Func<T, bool> canExecuteMethod)
        : base (executeMethod, canExecuteMethod){
        }
#endif
        public override void Execute(T parameter) {
            if(!CanExecute(parameter))
                return;
            if(executeMethod == null) return;
            executeMethod(parameter);
        }
    }

    public class DelegateCommand : DelegateCommand<object> {
#if !SILVERLIGHT
        public DelegateCommand(Action executeMethod)
            : this(executeMethod, null, null) {
        }
        public DelegateCommand(Action executeMethod, bool useCommandManager)
            : this(executeMethod, null, useCommandManager) {
        }
        public DelegateCommand(Action executeMethod, Func<bool> canExecuteMethod, bool? useCommandManager = null)
            : base(
                executeMethod != null ? (Action<object>)(o => executeMethod()) : null,
                canExecuteMethod != null ? (Func<object, bool>)(o => canExecuteMethod()) : null,
                useCommandManager) {
        }
#else
            public DelegateCommand(Action executeMethod)
                : this(executeMethod, null) {
            }
            public DelegateCommand(Action executeMethod, Func<bool> canExecuteMethod)
                : base(
                    executeMethod != null ? (Action<object>)(o => executeMethod()) : null,
                    canExecuteMethod != null ? (Func<object, bool>)(o => canExecuteMethod()) : null) {
            }
#endif
    }

    public class AsyncCommand<T> : AsyncCommandBase<T>, IAsyncCommand {
        bool allowMultipleExecution = false;
        bool isExecuting = false;
        CancellationTokenSource cancellationTokenSource;
        bool shouldCancel = false;
        internal Task executeTask;

        public bool AllowMultipleExecution {
            get { return allowMultipleExecution; }
            set { allowMultipleExecution = value; }
        }
        public bool IsExecuting {
            get { return isExecuting; }
            private set {
                if(isExecuting == value) return;
                isExecuting = value;
                RaisePropertyChanged(BindableBase.GetPropertyName(() => IsExecuting));
                OnIsExecutingChanged();
            }
        }
        public CancellationTokenSource CancellationTokenSource {
            get { return cancellationTokenSource; }
            private set {
                if(cancellationTokenSource == value) return;
                cancellationTokenSource = value;
                RaisePropertyChanged(BindableBase.GetPropertyName(() => CancellationTokenSource));
            }
        }
        [Obsolete("This property is obsolete. Use the IsCancellationRequested property instead.")]
        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public bool ShouldCancel {
            get { return shouldCancel; }
            private set {
                if(shouldCancel == value) return;
                shouldCancel = value;
                RaisePropertyChanged(BindableBase.GetPropertyName(() => ShouldCancel));
            }
        }
        public bool IsCancellationRequested {
            get {
                if(CancellationTokenSource == null) return false;
                return CancellationTokenSource.IsCancellationRequested;
            }
        }
        public DelegateCommand CancelCommand { get; private set; }
        ICommand IAsyncCommand.CancelCommand { get { return CancelCommand; } }

#if !SILVERLIGHT
        public AsyncCommand(Func<T, Task> executeMethod)
            : this(executeMethod, null, null) {
        }
        public AsyncCommand(Func<T, Task> executeMethod, bool useCommandManager)
            : this(executeMethod, null, useCommandManager) {
        }
        public AsyncCommand(Func<T, Task> executeMethod, Func<T, bool> canExecuteMethod, bool? useCommandManager = null)
            : base(executeMethod, canExecuteMethod, useCommandManager) {
            CancelCommand = new DelegateCommand(Cancel, CanCancel, false);

        }
#else
        public AsyncCommand(Func<T, Task> executeMethod)
            : this(executeMethod, null) {
        }
        public AsyncCommand(Func<T, Task> executeMethod, Func<T, bool> canExecuteMethod)
        :base(executeMethod,canExecuteMethod){
            CancelCommand = new DelegateCommand(Cancel);
        }
#endif

        public override bool CanExecute(T parameter) {
            if(!AllowMultipleExecution && IsExecuting) return false;
            return base.CanExecute(parameter);
        }
        public override void Execute(T parameter) {
            if(!CanExecute(parameter))
                return;
            if(executeMethod == null) return;
            IsExecuting = true;
#if SILVERLIGHT
            Dispatcher dispatcher = Deployment.Current.Dispatcher;
#else
            Dispatcher dispatcher = Dispatcher.CurrentDispatcher;
#endif
            CancellationTokenSource = new CancellationTokenSource();
            executeTask = executeMethod(parameter).ContinueWith(x => {
                dispatcher.BeginInvoke(new Action(() => {
                    IsExecuting = false;
                    ShouldCancel = false;

                }));
            });
        }
        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public void Cancel() {
            if(!CanCancel()) return;
            ShouldCancel = true;
            CancellationTokenSource.Cancel();
        }
        bool CanCancel() {
            return IsExecuting;
        }
        void OnIsExecutingChanged() {
            CancelCommand.RaiseCanExecuteChanged();
            RaiseCanExecuteChanged();
        }
    }

    public class AsyncCommand : AsyncCommand<object> {
#if !SILVERLIGHT
        public AsyncCommand(Func<Task> executeMethod)
            : this(executeMethod, null, null) {
        }
        public AsyncCommand(Func<Task> executeMethod, bool useCommandManager)
            : this(executeMethod, null, useCommandManager) {
        }
        public AsyncCommand(Func<Task> executeMethod, Func<bool> canExecuteMethod, bool? useCommandManager = null)
            : base(
                executeMethod != null ? (Func<object, Task>)(o => executeMethod()) : null,
                canExecuteMethod != null ? (Func<object, bool>)(o => canExecuteMethod()) : null,
                useCommandManager) {
        }
#else
            public AsyncCommand(Func<Task> executeMethod)
                : this(executeMethod, null) {
            }
            public AsyncCommand(Func<Task> executeMethod, Func<bool> canExecuteMethod)
                : base(
                    executeMethod != null ? (Func<object, Task>)(o => executeMethod()) : null,
                    canExecuteMethod != null ? (Func<object, bool>)(o => canExecuteMethod()) : null) {
            }
#endif
    }
}
#pragma warning restore 612, 618