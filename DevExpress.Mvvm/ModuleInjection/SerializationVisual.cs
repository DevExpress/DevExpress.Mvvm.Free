using System.Collections.Generic;
using System.Linq;

namespace DevExpress.Mvvm.ModuleInjection {
    public interface IVisualStateService {
        string DefaultState { get; }
        string GetCurrentState();
        string GetSavedState();
        void SaveState(string state);
        void RestoreState(string state);
    }
    public static class VisualStateServiceExtensions {
        public static void SaveState(this IVisualStateService service) {
            Verifier.VerifyVisualSerializationService(service);
            string layout;
            string storedLayout;
            if(!IsStateChanged(service, out layout, out storedLayout)) return;
            service.SaveState(layout);
        }
        public static void RestoreState(this IVisualStateService service) {
            Verifier.VerifyVisualSerializationService(service);
            string layout;
            string storedLayout;
            if(!IsStateChanged(service, out layout, out storedLayout)) return;
            service.RestoreState(storedLayout);
        }
        public static void RestoreDefaultState(this IVisualStateService service) {
            Verifier.VerifyVisualSerializationService(service);
            string layout;
            string defaultLayout;
            if(IsDefaultState(service, out layout, out defaultLayout)) return;
            service.RestoreState(defaultLayout);
        }
        public static bool IsDefaultState(this IVisualStateService service) {
            Verifier.VerifyVisualSerializationService(service);
            string layout;
            string defaultLayout;
            return IsDefaultState(service, out layout, out defaultLayout);
        }
        public static bool IsStateChanged(this IVisualStateService service) {
            Verifier.VerifyVisualSerializationService(service);
            string layout;
            string storedLayout;
            return IsStateChanged(service, out layout, out storedLayout);
        }
        static bool IsDefaultState(IVisualStateService service, out string layout, out string defaultLayout) {
            defaultLayout = service.DefaultState;
            layout = service.GetCurrentState();
            if(string.IsNullOrEmpty(defaultLayout))
                return true;
            if(string.IsNullOrEmpty(layout))
                return true;
            return layout == defaultLayout;
        }
        static bool IsStateChanged(IVisualStateService service, out string state, out string savedState) {
            state = service.GetCurrentState();
            savedState = service.GetSavedState();
            if(string.IsNullOrEmpty(state))
                return false;
            if(!string.IsNullOrEmpty(savedState))
                return state != savedState;
            var defaultLayout = service.DefaultState;
            if(!string.IsNullOrEmpty(defaultLayout))
                return state != defaultLayout;
            return true;
        }
    }
}
namespace DevExpress.Mvvm.ModuleInjection.Native {
    public interface IVisualStateServiceImplementation : IVisualStateService {
        string Id { get; }
        void EnforceSaveState();
    }
    public static class VisualStateServiceHelper {
        public static IEnumerable<IVisualStateServiceImplementation> GetServices(object viewModel, bool throwIfNotSupportServices, bool checkServiceIds) {
            IEnumerable<IVisualStateServiceImplementation> res;
            GetServices(viewModel, throwIfNotSupportServices, checkServiceIds, out res);
            return res;
        }
        public static void CheckServices(object viewModel, bool throwIfNotSupportServices, bool checkServiceIds) {
            IEnumerable<IVisualStateServiceImplementation> res;
            GetServices(viewModel, throwIfNotSupportServices, checkServiceIds, out res);
        }
        static void GetServices(object viewModel, bool throwIfNotSupportServices, bool checkServiceIds, out IEnumerable<IVisualStateServiceImplementation> services) {
            if(throwIfNotSupportServices)
                Verifier.VerifyViewModelISupportServices(viewModel);
            if(!(viewModel is ISupportServices)) {
                services = new IVisualStateServiceImplementation[] { };
                return;
            }
            var serviceContainer = ((ISupportServices)viewModel).ServiceContainer;
            services = serviceContainer.GetServices<IVisualStateService>(true).Cast<IVisualStateServiceImplementation>();
            if(checkServiceIds && services.GroupBy(x => x.Id).Where(x => x.Count() > 1).Any())
                ModuleInjectionException.VisualStateServiceName();
        }
    }
}