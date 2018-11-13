using System;

namespace DevExpress.Mvvm.ModuleInjection {
    public interface IModule {
        string Key { get; }
        Func<object> ViewModelFactory { get; }
        string ViewModelName { get; }
        string ViewName { get; }
        Type ViewType { get; }
    }
    public class Module : IModule {
        public string Key { get; private set; }
        public Func<object> ViewModelFactory { get; private set; }
        public string ViewModelName { get; private set; }
        public string ViewName { get; private set; }
        public Type ViewType { get; private set; }

        public Module(string key, Func<object> viewModelFactory)
            : this(key, viewModelFactory, null, null, null) {
            Verifier.VerifyViewModelFactory(viewModelFactory);
        }
        public Module(string key, Func<object> viewModelFactory, string viewName)
            : this(key, viewModelFactory, null, viewName, null) {
            Verifier.VerifyViewModelFactory(viewModelFactory);
        }
        public Module(string key, Func<object> viewModelFactory, Type viewType)
            : this(key, viewModelFactory, null, null, viewType) {
            Verifier.VerifyViewModelFactory(viewModelFactory);
        }
        public Module(string key, string viewModelName, string viewName)
            : this(key, null, viewModelName, viewName, null) {
            Verifier.VerifyViewModelName(viewModelName);
        }
        Module(string key, Func<object> viewModelFactory, string viewModelName, string viewName, Type viewType) {
            Verifier.VerifyKey(key);
            Key = key;
            ViewModelFactory = viewModelFactory;
            ViewModelName = viewModelName;
            ViewName = viewName;
            ViewType = viewType;
        }
    }
}