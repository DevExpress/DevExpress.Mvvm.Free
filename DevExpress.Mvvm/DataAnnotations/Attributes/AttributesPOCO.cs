using System;
using System.Reflection;

namespace DevExpress.Mvvm.DataAnnotations {
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class POCOViewModelAttribute : Attribute {
        public bool ImplementIDataErrorInfo;
        public bool ImplementINotifyPropertyChanging;
        public bool InvokeOnPropertyChangedMethodBeforeRaisingINPC;
    }
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class CommandAttribute : Attribute {
        bool? useCommandManager;
        public CommandAttribute(bool isCommand) {
            this.IsCommand = isCommand;
        }
        public CommandAttribute()
            : this(true) {
        }
        public string Name { get; set; }
        public string CanExecuteMethodName { get; set; }
        public bool IsCommand { get; private set; }
        internal MethodInfo CanExecuteMethod { get; set; }
        public bool UseCommandManager {
            set { useCommandManager = value; }
            get { throw new NotSupportedException(); }
        }
        internal bool? GetUseCommandManager() { return useCommandManager; }
        internal bool AllowMultipleExecutionCore { get; set; }
    }
    public class AsyncCommandAttribute : CommandAttribute {
        public AsyncCommandAttribute(bool isAsyncCommand)
            : base(isAsyncCommand) { }
        public AsyncCommandAttribute()
            : base() { }
        public bool AllowMultipleExecution { get { return AllowMultipleExecutionCore; } set { AllowMultipleExecutionCore = value; } }
    }
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class ServicePropertyAttribute : Attribute {
        public ServicePropertyAttribute(bool isServiceProperty) {
            this.IsServiceProperty = isServiceProperty;
        }
        public ServicePropertyAttribute(ServiceSearchMode searchMode)
            : this(true) {
            this.SearchMode = searchMode;
        }
        public ServicePropertyAttribute()
            : this(true) {
        }
        public string Key { get; set; }
        public ServiceSearchMode SearchMode { get; set; }
        public bool IsServiceProperty { get; private set; }
    }
    [AttributeUsage(AttributeTargets.Property)]
    public class BindablePropertyAttribute : Attribute {
        public BindablePropertyAttribute()
            : this(true) {
        }
        public BindablePropertyAttribute(bool isBindable) {
            this.IsBindable = isBindable;
        }
        public bool IsBindable { get; private set; }
        public string OnPropertyChangedMethodName { get; set; }
        public string OnPropertyChangingMethodName { get; set; }
        internal MethodInfo OnPropertyChangedMethod { get; set; }
        internal MethodInfo OnPropertyChangingMethod { get; set; }
    }
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true, Inherited = true)]
    public class DependsOnPropertiesAttribute : Attribute {
        public string[] Properties { get; private set; }
        public DependsOnPropertiesAttribute(string prop1) {
            Init(prop1);
        }
        public DependsOnPropertiesAttribute(string prop1, string prop2) {
            Init(prop1, prop2);
        }
        public DependsOnPropertiesAttribute(string prop1, string prop2, string prop3) {
            Init(prop1, prop2, prop3);
        }
        public DependsOnPropertiesAttribute(string prop1, string prop2, string prop3, string prop4) {
            Init(prop1, prop2, prop3, prop4);
        }
        public DependsOnPropertiesAttribute(string prop1, string prop2, string prop3, string prop4, string prop5) {
            Init(prop1, prop2, prop3, prop4, prop5);
        }
        public DependsOnPropertiesAttribute(string prop1, string prop2, string prop3, string prop4, string prop5, string prop6) {
            Init(prop1, prop2, prop3, prop4, prop5, prop6);
        }
        public DependsOnPropertiesAttribute(string prop1, string prop2, string prop3, string prop4, string prop5, string prop6, string prop7) {
            Init(prop1, prop2, prop3, prop4, prop5, prop6, prop7);
        }
        public DependsOnPropertiesAttribute(string prop1, string prop2, string prop3, string prop4, string prop5, string prop6, string prop7, string prop8) {
            Init(prop1, prop2, prop3, prop4, prop5, prop6, prop7, prop8);
        }
        public DependsOnPropertiesAttribute(string prop1, string prop2, string prop3, string prop4, string prop5, string prop6, string prop7, string prop8, string prop9) {
            Init(prop1, prop2, prop3, prop4, prop5, prop6, prop7, prop8, prop9);
        }
        public DependsOnPropertiesAttribute(string prop1, string prop2, string prop3, string prop4, string prop5, string prop6, string prop7, string prop8, string prop9, string prop10) {
            Init(prop1, prop2, prop3, prop4, prop5, prop6, prop7, prop8, prop9, prop10);
        }
        void Init(params string[] properties) { Properties = properties; }
    }
}