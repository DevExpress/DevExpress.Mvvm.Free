using DevExpress.Mvvm.Native;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Input;

namespace DevExpress.Mvvm.UI {
    public class MethodToCommandBehavior : FunctionBindingBehaviorBase {
        protected const string Error_SourceFunctionShouldBeBool = "The return value of the '{0}.{1}' function should be bool.";

        #region Dependency properties
        public static readonly DependencyProperty CommandProperty =
            DependencyProperty.Register("Command", typeof(string), typeof(MethodToCommandBehavior),
            new PropertyMetadata("Command", (d, e) => ((MethodToCommandBehavior)d).OnCommandChanged(e)));
        public static readonly DependencyProperty CommandParameterProperty =
            DependencyProperty.Register("CommandParameter", typeof(string), typeof(MethodToCommandBehavior),
            new PropertyMetadata("CommandParameter", (d, e) => ((MethodToCommandBehavior)d).OnResultAffectedPropertyChanged()));
        public static readonly DependencyProperty CanExecuteFunctionProperty =
            DependencyProperty.Register("CanExecuteFunction", typeof(string), typeof(MethodToCommandBehavior),
            new PropertyMetadata(null, (d, e) => ((MethodToCommandBehavior)d).OnResultAffectedPropertyChanged()));
        public static readonly DependencyProperty MethodProperty =
                DependencyProperty.Register("Method", typeof(string), typeof(MethodToCommandBehavior),
            new PropertyMetadata(null, (d, e) => ((MethodToCommandBehavior)d).OnResultAffectedPropertyChanged()));

        public string Command {
            get { return (string)GetValue(CommandProperty); }
            set { SetValue(CommandProperty, value); }
        }
        public string CommandParameter {
            get { return (string)GetValue(CommandParameterProperty); }
            set { SetValue(CommandParameterProperty, value); }
        }
        public string CanExecuteFunction {
            get { return (string)GetValue(CanExecuteFunctionProperty); }
            set { SetValue(CanExecuteFunctionProperty, value); }
        }
        public string Method {
            get { return (string)GetValue(MethodProperty); }
            set { SetValue(MethodProperty, value); }
        }
        #endregion

        bool IsActive { get { return IsAttached && !string.IsNullOrEmpty(Command) && ActualSource != null && !string.IsNullOrEmpty(ActualFunction); } }
        string ActualCanExecuteFunction { get { return !string.IsNullOrEmpty(CanExecuteFunction) ? CanExecuteFunction : string.Format("CanExecute{0}", ActualFunction); } }
        protected ICommand ResultCommand { get; private set; }
        protected override string ActualFunction { get { return Method; } }

        public MethodToCommandBehavior() {
            ResultCommand = new DelegateCommand<object>(ExecuteCommand, CanExecuteCommand, false);
        }

        protected override void OnAttached() {
            base.OnAttached();
            SetTargetCommandValue(Command);
        }
        protected override void OnDetaching() {
            ReleaseTargetCommandValue(Command);
            base.OnDetaching();
        }
        void OnCommandChanged(DependencyPropertyChangedEventArgs e) {
            ReleaseTargetCommandValue((string)e.OldValue);
            SetTargetCommandValue((string)e.NewValue);
        }
        void SetTargetCommandValue(string commandProperty) {
            if(!IsAttached || string.IsNullOrEmpty(commandProperty))
                return;
            GetObjectPropertySetter(AssociatedObject, commandProperty, true).Do(x => x(ResultCommand));
        }
        void ReleaseTargetCommandValue(string commandProperty) {
            if(!IsAttached || string.IsNullOrEmpty(commandProperty))
                return;
            GetObjectPropertySetter(AssociatedObject, commandProperty).Do(x => x(null));
        }

        protected List<ArgInfo> UnpackArgs(object parameter) {
            if(parameter == null)
                return null;
            if(!(parameter is IEnumerable))
                return new List<ArgInfo>() { new ArgInfo(parameter) };

            return ((IEnumerable)parameter).Cast<object>().Select(x => x is ArgInfo ? x as ArgInfo : new ArgInfo(x)).ToList();
        }
        protected override void OnResultAffectedPropertyChanged() {
            if(!IsActive || string.IsNullOrEmpty(CommandParameter))
                return;

            Action<object> parameterSetter = GetObjectPropertySetter(AssociatedObject, CommandParameter, true);
            if(parameterSetter != null)
                parameterSetter(GetArgsInfo(this));

            ((IDelegateCommand)ResultCommand).RaiseCanExecuteChanged();
        }
        void ExecuteCommand(object parameter) {
            if(!IsActive)
                return;

            List<ArgInfo> args = UnpackArgs(parameter);
            InvokeSourceFunction(ActualSource, ActualFunction, args, ExecuteMethodInfoChecker);
        }
        bool CanExecuteCommand(object parameter) {
            if(!IsActive)
                return false;

            List<ArgInfo> args = UnpackArgs(parameter);
            object result = InvokeSourceFunction(ActualSource, ActualCanExecuteFunction, args, CanExecuteMethodInfoChecker);
            return result == DependencyProperty.UnsetValue ? true : (bool)result;
        }

        static bool CanExecuteMethodInfoChecker(MethodInfo info, Type targetType, string functionName) {
            if(info != null && info.ReturnType != typeof(bool))
                throw new ArgumentException(string.Format(Error_SourceFunctionShouldBeBool, targetType.Name, info.Name));

            return info != null;
        }
        static bool ExecuteMethodInfoChecker(MethodInfo info, Type targetType, string functionName) {
            if(info == null)
                System.Diagnostics.Trace.WriteLine(string.Format(Error_SourceMethodNotFound, targetType.Name, functionName));

            return info != null;
        }
    }
}