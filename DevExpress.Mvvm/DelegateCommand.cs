using DevExpress.Mvvm.Native;
using System;
using System.ComponentModel;
using System.Globalization;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace DevExpress.Mvvm {
    public abstract class CommandBase<T> : ICommand, IDelegateCommand {
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
        public CommandBase(bool useCommandManager = true) {
            this.useCommandManager = useCommandManager;
        }
#else
        public CommandBase() {
            this.useCommandManager = false;
        }
#endif

        public bool CanExecute(T parameter) {
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
            return CanExecute(GetGenericParameter(parameter));
        }
        void ICommand.Execute(object parameter) {
            Execute(GetGenericParameter(parameter));
        }
        static T GetGenericParameter(object parameter) {
            Type targetType = Nullable.GetUnderlyingType(typeof(T)) ?? typeof(T);
            if(targetType.IsEnum && parameter is string) {
                parameter = Enum.Parse(targetType, (string)parameter, false);
            } else if(parameter is IConvertible && !typeof(T).IsAssignableFrom(parameter.GetType())) {
                parameter = Convert.ChangeType(parameter, targetType, CultureInfo.InvariantCulture);
            }
            return parameter == null ? default(T) : (T)parameter;
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
        public DelegateCommandBase(Action<T> executeMethod, bool useCommandManager = true)
            : this(executeMethod, null, useCommandManager) {
        }
        public DelegateCommandBase(Action<T> executeMethod, Func<T, bool> canExecuteMethod, bool useCommandManager = true)
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
    public abstract class AsyncCommandBase<T> : CommandBase<T> {
        protected Func<T, Task> executeMethod = null;

        void Init(Func<T, Task> executeMethod, Func<T, bool> canExecuteMethod) {
            if(executeMethod == null && canExecuteMethod == null)
                throw new ArgumentNullException("executeMethod");
            this.executeMethod = executeMethod;
            this.canExecuteMethod = canExecuteMethod;
        }

#if !SILVERLIGHT
        public AsyncCommandBase(Func<T, Task> executeMethod, bool useCommandManager = true)
            : this(executeMethod, null, useCommandManager) {
        }
        public AsyncCommandBase(Func<T, Task> executeMethod, Func<T, bool> canExecuteMethod, bool useCommandManager = true)
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
    }
    public class DelegateCommand<T> : DelegateCommandBase<T> {
#if !SILVERLIGHT
        public DelegateCommand(Action<T> executeMethod, bool useCommandManager = true)
            : this(executeMethod, null, useCommandManager) {
        }
        public DelegateCommand(Action<T> executeMethod, Func<T, bool> canExecuteMethod, bool useCommandManager = true)
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
        public DelegateCommand(Action executeMethod, bool useCommandManager = true)
            : this(executeMethod, null, useCommandManager) {
        }
        public DelegateCommand(Action executeMethod, Func<bool> canExecuteMethod, bool useCommandManager = true)
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

        bool isExecuting = false;
        bool shouldCancel = false;
        internal Task executeTask;

        public bool IsExecuting {
            get { return isExecuting; }
            private set {
                if(isExecuting == value) return;
                isExecuting = value;
                RaisePropertyChanged(BindableBase.GetPropertyName(() => IsExecuting));
                OnIsExecutingChanged();
            }
        }
        public bool ShouldCancel {
            get { return shouldCancel; }
            private set {
                if(shouldCancel == value) return;
                shouldCancel = value;
                RaisePropertyChanged(BindableBase.GetPropertyName(() => ShouldCancel));
            }
        }
        public DelegateCommand CancelCommand { get; private set; }
        ICommand IAsyncCommand.CancelCommand { get { return CancelCommand; } }
        public event PropertyChangedEventHandler PropertyChanged;



#if !SILVERLIGHT
        public AsyncCommand(Func<T, Task> executeMethod, bool useCommandManager = true)
            : this(executeMethod, null, useCommandManager) {
        }
        public AsyncCommand(Func<T, Task> executeMethod, Func<T, bool> canExecuteMethod, bool useCommandManager = true)
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
            executeTask = executeMethod(parameter).ContinueWith(x => {
                dispatcher.BeginInvoke(new Action(() => {
                    IsExecuting = false;
                    ShouldCancel = false;
                }));
            });
        }
        public void Cancel() {
            if(!CanCancel()) return;
            ShouldCancel = true;
            IsExecuting = false;
            RaiseCanExecuteChanged();
        }
        bool CanCancel() {
            return IsExecuting;
        }
        void OnIsExecutingChanged() {
            CancelCommand.RaiseCanExecuteChanged();
            RaiseCanExecuteChanged();
        }
        void RaisePropertyChanged(string propName) {
            if(PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propName));
        }
    }

    public class AsyncCommand : AsyncCommand<object> {
#if !SILVERLIGHT
        public AsyncCommand(Func<Task> executeMethod, bool useCommandManager = true)
            : this(executeMethod, null, useCommandManager) {
        }
        public AsyncCommand(Func<Task> executeMethod, Func<bool> canExecuteMethod, bool useCommandManager = true)
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