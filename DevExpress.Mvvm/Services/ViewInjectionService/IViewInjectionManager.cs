using System;
using System.ComponentModel;

namespace DevExpress.Mvvm {
    public interface IViewInjectionManager {
        [EditorBrowsable(EditorBrowsableState.Never)]
        void RegisterService(IViewInjectionService service);
        [EditorBrowsable(EditorBrowsableState.Never)]
        void UnregisterService(IViewInjectionService service);
        IViewInjectionService GetService(string regionName);

        [EditorBrowsable(EditorBrowsableState.Never)]
        void Inject(string regionName, object key, Func<object> viewModelFactory, string viewName, Type viewType);
        void Remove(string regionName, object key);
        void Navigate(string regionName, object key);

        void RegisterNavigatedEventHandler(object viewModel, Action eventHandler);
        void RegisterNavigatedAwayEventHandler(object viewModel, Action eventHandler);
        void RegisterViewModelClosingEventHandler(object viewModel, Action<ViewModelClosingEventArgs> eventHandler);
        void UnregisterNavigatedEventHandler(object viewModel, Action eventHandler = null);
        void UnregisterNavigatedAwayEventHandler(object viewModel, Action eventHandler = null);
        void UnregisterViewModelClosingEventHandler(object viewModel, Action<ViewModelClosingEventArgs> eventHandler = null);

        [EditorBrowsable(EditorBrowsableState.Never)]
        void RaiseNavigatedEvent(object viewModel);
        [EditorBrowsable(EditorBrowsableState.Never)]
        void RaiseNavigatedAwayEvent(object viewModel);
        [EditorBrowsable(EditorBrowsableState.Never)]
        void RaiseViewModelClosingEvent(ViewModelClosingEventArgs e);
    }
    public static class ViewInjectionManagerExtensions {
        public static void Inject(this IViewInjectionManager service, string regionName, object key, Func<object> viewModelFactory) {
            VerifyService(service);
            service.Inject(regionName, key, viewModelFactory, null, null);
        }
        public static void Inject(this IViewInjectionManager service, string regionName, object key, Func<object> viewModelFactory, string viewName) {
            VerifyService(service);
            service.Inject(regionName, key, viewModelFactory, viewName, null);
        }
        public static void Inject(this IViewInjectionManager service, string regionName, object key, Func<object> viewModelFactory, Type viewType) {
            VerifyService(service);
            service.Inject(regionName, key, viewModelFactory, null, viewType);
        }

        static void VerifyService(IViewInjectionManager service) {
            if(service == null)
                throw new ArgumentNullException("service");
        }
    }
}