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
    class CommandManagerHelper {
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
        public static void Subscribe(EventHandler canExecuteChangedHandler) {
            CommandManager.RequerySuggested += canExecuteChangedHandler;
        }
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
        public static void Unsubscribe(EventHandler canExecuteChangedHandler) {
            CommandManager.RequerySuggested -= canExecuteChangedHandler;
        }
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
        public static void InvalidateRequerySuggested() {
            CommandManager.InvalidateRequerySuggested();
        }
    }
    public abstract class CommandBase {
        static bool defaultUseCommandManager = true;
        public static bool DefaultUseCommandManager { get { return defaultUseCommandManager; } set { defaultUseCommandManager = value; } }
    }
    public interface ICommand<T> : ICommand {
        void Execute(T param);
        bool CanExecute(T param);
    }
    public abstract class CommandBase<T> : CommandBase, ICommand<T>, IDelegateCommand {
        protected Func<T, bool> canExecuteMethod = null;
        protected bool useCommandManager;
        event EventHandler canExecuteChanged;

        public event EventHandler CanExecuteChanged {
            add {
                if(useCommandManager) {
                    CommandManagerHelper.Subscribe(value);
                } else {
                    canExecuteChanged += value;
                }
            }
            remove {
                if(useCommandManager) {
                    CommandManagerHelper.Unsubscribe(value);
                } else {
                    canExecuteChanged -= value;
                }
            }
        }

        public CommandBase(bool? useCommandManager = null) {
            this.useCommandManager = useCommandManager ?? DefaultUseCommandManager;
        }

        public virtual bool CanExecute(T parameter) {
            if(canExecuteMethod == null) return true;
            return canExecuteMethod(parameter);
        }
        public abstract void Execute(T parameter);

        public void RaiseCanExecuteChanged() {
            if(useCommandManager) {
                CommandManagerHelper.InvalidateRequerySuggested();
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
            parameter = TypeCastHelper.TryCast(parameter, typeof(T));
            if(parameter == null || parameter is T) return (T)parameter;
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
    }
    public abstract class AsyncCommandBase<T> : CommandBase<T>, INotifyPropertyChanged {
        protected Func<T, Task> executeMethod = null;

        void Init(Func<T, Task> executeMethod, Func<T, bool> canExecuteMethod) {
            if(executeMethod == null && canExecuteMethod == null)
                throw new ArgumentNullException("executeMethod");
            this.executeMethod = executeMethod;
            this.canExecuteMethod = canExecuteMethod;
        }

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
        public DelegateCommand(Action<T> executeMethod)
            : this(executeMethod, null, null) {
        }
        public DelegateCommand(Action<T> executeMethod, bool useCommandManager)
            : this(executeMethod, null, useCommandManager) {
        }
        public DelegateCommand(Action<T> executeMethod, Func<T, bool> canExecuteMethod, bool? useCommandManager = null)
            : base(executeMethod, canExecuteMethod, useCommandManager) {
        }
        public override void Execute(T parameter) {
            if(!CanExecute(parameter))
                return;
            if(executeMethod == null) return;
            executeMethod(parameter);
        }
    }

    public class DelegateCommand : DelegateCommand<object> {
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
    }

    public class AsyncCommand<T> : AsyncCommandBase<T>, IAsyncCommand {
        bool allowMultipleExecution = false;
        bool isExecuting = false;
        CancellationTokenSource cancellationTokenSource;
        bool shouldCancel = false;
        internal Task executeTask;
        DispatcherOperation completeTaskOperation;

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
        [Obsolete("Use the IsCancellationRequested property instead.")]
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

        public AsyncCommand(Func<T, Task> executeMethod)
            : this(executeMethod, null, false, null) {
        }
        public AsyncCommand(Func<T, Task> executeMethod, bool useCommandManager)
            : this(executeMethod, null, false, useCommandManager) {
        }
        public AsyncCommand(Func<T, Task> executeMethod, Func<T, bool> canExecuteMethod, bool? useCommandManager = null)
            : this(executeMethod, canExecuteMethod, false, useCommandManager) {
        }
        public AsyncCommand(Func<T, Task> executeMethod, Func<T, bool> canExecuteMethod, bool allowMultipleExecution, bool? useCommandManager = null)
            : base(executeMethod, canExecuteMethod, useCommandManager) {
            CancelCommand = new DelegateCommand(Cancel, CanCancel, false);
            AllowMultipleExecution = allowMultipleExecution;
        }

        public override bool CanExecute(T parameter) {
            if(!AllowMultipleExecution && IsExecuting) return false;
            return base.CanExecute(parameter);
        }
        public override void Execute(T parameter) {
            if(!CanExecute(parameter))
                return;
            if(executeMethod == null) return;
            IsExecuting = true;
            Dispatcher dispatcher = Dispatcher.CurrentDispatcher;
            CancellationTokenSource = new CancellationTokenSource();
            executeTask = executeMethod(parameter).ContinueWith(x => {
                completeTaskOperation = dispatcher.BeginInvoke(new Action(() => {
                    IsExecuting = false;
                    ShouldCancel = false;
                    completeTaskOperation = null;
                }));
            });
        }
        public void Wait(TimeSpan timeout) {
            if(executeTask == null || !IsExecuting) return;
            executeTask.Wait(timeout);
            completeTaskOperation.Do(x => x.Wait(timeout));
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
        public AsyncCommand(Func<Task> executeMethod)
            : this(executeMethod, null, false, null) {
        }
        public AsyncCommand(Func<Task> executeMethod, bool useCommandManager)
            : this(executeMethod, null, false, useCommandManager) {
        }
        public AsyncCommand(Func<Task> executeMethod, Func<bool> canExecuteMethod, bool? useCommandManager = null)
            : this(executeMethod, canExecuteMethod, false, useCommandManager) {
        }
        public AsyncCommand(Func<Task> executeMethod, Func<bool> canExecuteMethod, bool allowMultipleExecution, bool? useCommandManager = null)
            : base(
                executeMethod != null ? (Func<object, Task>)(o => executeMethod()) : null,
                canExecuteMethod != null ? (Func<object, bool>)(o => canExecuteMethod()) : null,
                allowMultipleExecution,
                useCommandManager) {
        }
    }
}
#pragma warning restore 612, 618