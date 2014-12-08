using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace DevExpress.Mvvm {
    public interface IViewInjectionService {
        [EditorBrowsable(EditorBrowsableState.Never)]
        string RegionName { get; }
        IEnumerable<object> ViewModels { get; }
        object SelectedViewModel { get; set; }

        object GetKey(object viewModel);
        [EditorBrowsable(EditorBrowsableState.Never)]
        void Inject(object key, object viewModel, string viewName, Type viewType);
        bool Remove(object viewModel);
    }
    public static class ViewInjectionServiceExtensions {
        public static void Inject(this IViewInjectionService service, object key, object viewModel) {
            VerifyService(service);
            service.Inject(key, viewModel, string.Empty, null);
        }
        public static void Inject(this IViewInjectionService service, object key, object viewModel, string viewName) {
            VerifyService(service);
            service.Inject(key, viewModel, viewName, null);
        }
        public static void Inject(this IViewInjectionService service, object key, object viewModel, Type viewType) {
            VerifyService(service);
            service.Inject(key, viewModel, null, viewType);
        }

        public static object GetViewModel(this IViewInjectionService service, object key) {
            VerifyService(service);
            return service.ViewModels.FirstOrDefault(x => object.Equals(service.GetKey(x), key));
        }
        static void VerifyService(IViewInjectionService service) {
            if(service == null)
                throw new ArgumentNullException("service");
        }
    }

    public delegate void ViewModelClosingEventHandler(object sender, ViewModelClosingEventArgs e);
    public class ViewModelClosingEventArgs : CancelEventArgs {
        public object ViewModel { get; private set; }
        public ViewModelClosingEventArgs(object viewModel) {
            ViewModel = viewModel;
        }
    }
}