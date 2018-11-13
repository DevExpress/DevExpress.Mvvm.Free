using System;
using System.Text.RegularExpressions;

namespace DevExpress.Mvvm.ModuleInjection {
    public class ModuleInjectionException : Exception {
        const string moduleMissing = "Cannot find a module with the passed key. Register module before working with it.";
        const string moduleExists = "A module with the same key already exists.";
        const string cannotAttach = "This service can be only attached to a FrameworkElement or FrameworkContentElement";
        const string noStrategy = "Cannot find an appropriate strategy for the {0} container type.";
        const string nullVM = "A view model to inject cannot be null.";
        const string vmNotSupportServices = "This ViewModel does not implement the ISupportServices interface.";
        const string vmNotSupportParameter = "This ViewModel does not implement the ISupportParameter interface.";
        const string visualStateServiceName = "VisualStateService with the same Name already exists. If you are using several VisualStateServices in one View, be sure that they have different names.";
        const string cannotResolveVM = "Cannot create a view model instance by the {0} type name. Setup ViewModelLocator.";

        public static void ModuleMissing(string regionName, string key) {
            throw new ModuleInjectionException(regionName, key, moduleMissing);
        }
        public static void ModuleAlreadyExists(string regionName, string key) {
            throw new ModuleInjectionException(regionName, key, moduleExists);
        }
        public static void NullVM() {
            throw new ModuleInjectionException(null, null, nullVM);
        }
        public static void VMNotSupportServices() {
            throw new ModuleInjectionException(null, null, vmNotSupportServices);
        }
        public static void VMNotSupportParameter() {
            throw new ModuleInjectionException(null, null, vmNotSupportParameter);
        }
        public static void VisualStateServiceName() {
            throw new ModuleInjectionException(null, null, visualStateServiceName);
        }
        public static void CannotResolveVM(string vmName) {
            throw new ModuleInjectionException(null, null, string.Format(cannotResolveVM, vmName));
        }

        public static void CannotAttach() {
            throw new ModuleInjectionException(null, null, cannotAttach);
        }
        public static void NoStrategy(Type containerType) {
            throw new ModuleInjectionException(null, null, string.Format(noStrategy, containerType.Name));
        }

        public string Key { get; private set; }
        public string RegionName { get; private set; }
        ModuleInjectionException(string regionName, string key, string message) : base(message) {
            RegionName = regionName;
        }
    }
    static class Verifier {
        public static void VerifyManager(IModuleManagerBase manager) {
            if(manager == null) throw new ArgumentNullException("manager");
        }
        public static void VerifyRegionName(string regionName) {
            if(string.IsNullOrEmpty(regionName)) throw new ArgumentNullException("regionName");
        }
        public static void VerifyKey(string key) {
            if(string.IsNullOrEmpty(key)) throw new ArgumentNullException("key");
        }
        public static void VerifyModule(IModule module) {
            if(module == null) throw new ArgumentNullException("module");
        }
        public static void VerifyViewModel(object viewModel) {
            if(viewModel == null) throw new ArgumentNullException("viewModel");
        }
        public static void VerifyVisualSerializationService(IVisualStateService service) {
            if(service == null) throw new ArgumentNullException("service");
        }
        public static void VerifyViewModelFactory(Func<object> viewModelFactory) {
            if(viewModelFactory == null) throw new ArgumentNullException("viewModelFactory");
        }
        public static void VerifyViewModelName(string viewModelName) {
            if(string.IsNullOrEmpty(viewModelName)) throw new ArgumentNullException("viewModelName");
        }

        public static void VerifyViewModelISupportServices(object viewModel) {
            if(!(viewModel is ISupportServices))
                ModuleInjectionException.VMNotSupportServices();
        }
        public static void VerifyViewModelISupportParameter(object viewModel) {
            if(!(viewModel is ISupportParameter))
                ModuleInjectionException.VMNotSupportParameter();
        }
    }
}