using System;
using System.Reflection;

namespace DevExpress.Mvvm.DataAnnotations {
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class POCOViewModelAttribute : Attribute {
        public bool ImplementIDataErrorInfo;
    }
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class CommandAttribute : Attribute {
#if !SILVERLIGHT
        bool? useCommandManager;
#endif

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
#if !SILVERLIGHT
        public bool UseCommandManager {
            set { useCommandManager = value; }
            get { throw new NotSupportedException(); }
        }
        internal bool? GetUseCommandManager() { return useCommandManager; }
#else
        internal bool? GetUseCommandManager() { return false; }
#endif
    }
    public class AsyncCommandAttribute : CommandAttribute {
        public AsyncCommandAttribute(bool isAsincCommand)
            : base(isAsincCommand) { }
        public AsyncCommandAttribute()
            : base() { }
        public bool AllowMultipleExecution { get; set; }
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
}