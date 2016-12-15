using DevExpress.Mvvm.Native;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

#region Events
namespace DevExpress.Mvvm {
    public delegate void ViewModelClosingEventHandler(object sender, ViewModelClosingEventArgs e);
    public class ViewModelClosingEventArgs : CancelEventArgs {
        public object ViewModel { get; private set; }
        public ViewModelClosingEventArgs(object viewModel) {
            ViewModel = viewModel;
        }
    }
}
#endregion

#region IViewInjectionService
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
}
#endregion

#region IViewInjectionManager
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
#endregion

#region ViewInjectionManager
namespace DevExpress.Mvvm {
    public enum ViewInjectionMode { Default, Persistent }
    public class ViewInjectionManager : IViewInjectionManager {
        #region Static
        const string Exception1 = "Cannot register services with the same RegionName";
        const string Exception2 = "Cannot inject item with this key, because it already exists in this region.";

        static IViewInjectionManager _defaultInstance = new ViewInjectionManager(ViewInjectionMode.Default);
        static IViewInjectionManager _default;
        public static IViewInjectionManager Default { get { return _default ?? _defaultInstance; } set { _default = value; } }

        static IViewInjectionManager _persistentManager = new ViewInjectionManager(ViewInjectionMode.Persistent);
        public static IViewInjectionManager PersistentManager { get { return _persistentManager; } }
        #endregion

        public virtual void RegisterService(IViewInjectionService service) {
            ServiceManager.Add(service);
            QueueManager.UpdateQueues();
        }
        public virtual void UnregisterService(IViewInjectionService service) {
            ServiceManager.Remove(service);
            if(Mode == ViewInjectionMode.Persistent) {
                foreach(object vm in service.ViewModels.ToList())
                    service.Remove(vm);
            }
        }
        public IViewInjectionService GetService(string regionName) {
            return ServiceManager.FindService(x => x.RegionName == regionName);
        }

        public virtual void Inject(string regionName, object key, Func<object> viewModelFactory, string viewName, Type viewType) {
            bool isInjected = InjectCore(regionName, key, viewModelFactory, viewName, viewType);
            if(Mode == ViewInjectionMode.Persistent)
                QueueManager.PutToPersistentInjectionQueue(regionName, key, viewModelFactory, viewName, viewType);
            else if(!isInjected)
                QueueManager.PutToInjectionQueue(regionName, key, viewModelFactory, viewName, viewType);
            QueueManager.UpdateQueues();
        }
        protected bool InjectCore(string regionName, object key, Func<object> factory, string viewName, Type viewType) {
            var service = GetService(regionName);
            if(service != null) {
                if(Mode == ViewInjectionMode.Persistent && service.GetViewModel(key) != null)
                    return true;
                service.Inject(key, factory(), viewName, viewType);
                return true;
            }
            return false;
        }
        public virtual void Remove(string regionName, object key) {
            QueueManager.RemoveFromQueues(regionName, key);
            var service = GetService(regionName);
            if(service != null)
                service.Remove(service.GetViewModel(key));
        }
        public virtual void Navigate(string regionName, object key) {
            if(!NavigateCore(regionName, key))
                QueueManager.PutToNavigationQueue(regionName, key);
        }
        protected bool NavigateCore(string regionName, object key) {
            var service = GetService(regionName);
            if(service == null) return false;
            object vm = service.GetViewModel(key);
            if(vm != null) {
                service.SelectedViewModel = vm;
                return true;
            }
            return false;
        }

        public void RegisterNavigatedEventHandler(object viewModel, Action eventHandler) {
            Messenger.Register<NavigatedMessage>(viewModel, viewModel.GetHashCode(), false, eventHandler);
        }
        public void RegisterNavigatedAwayEventHandler(object viewModel, Action eventHandler) {
            Messenger.Register<NavigatedAwayMessage>(viewModel, viewModel.GetHashCode(), false, eventHandler);
        }
        public void RegisterViewModelClosingEventHandler(object viewModel, Action<ViewModelClosingEventArgs> eventHandler) {
            Messenger.Register<ViewModelClosingEventArgs>(viewModel, viewModel.GetHashCode(), false, eventHandler);
        }
        public void UnregisterNavigatedEventHandler(object viewModel, Action eventHandler = null) {
            Messenger.Unregister<NavigatedMessage>(viewModel, viewModel.GetHashCode(), eventHandler);
        }
        public void UnregisterNavigatedAwayEventHandler(object viewModel, Action eventHandler = null) {
            Messenger.Unregister<NavigatedAwayMessage>(viewModel, viewModel.GetHashCode(), eventHandler);
        }
        public void UnregisterViewModelClosingEventHandler(object viewModel, Action<ViewModelClosingEventArgs> eventHandler = null) {
            Messenger.Unregister<ViewModelClosingEventArgs>(viewModel, viewModel.GetHashCode(), eventHandler);
        }

        public void RaiseNavigatedEvent(object viewModel) {
            Messenger.Send<NavigatedMessage>(new NavigatedMessage(), viewModel.GetHashCode());
        }
        public void RaiseNavigatedAwayEvent(object viewModel) {
            Messenger.Send<NavigatedAwayMessage>(new NavigatedAwayMessage(), viewModel.GetHashCode());
        }
        public void RaiseViewModelClosingEvent(ViewModelClosingEventArgs e) {
            Messenger.Send<ViewModelClosingEventArgs>(e, e.ViewModel.GetHashCode());
        }

        protected ViewInjectionMode Mode { get; private set; }
        public ViewInjectionManager(ViewInjectionMode mode) {
            Mode = mode;
            ServiceManager = new ServiceManagerHelper();
            QueueManager = new QueueManagerHelper(this);
            Messenger = new MessengerEx();
        }
        ServiceManagerHelper ServiceManager;
        QueueManagerHelper QueueManager;
        MessengerEx Messenger;
        #region Inner Classes
        class ServiceManagerHelper {
            readonly List<WeakReference> ServiceReferences = new List<WeakReference>();
            void UpdateServiceReferences() {
                List<WeakReference> referencesToDelete = new List<WeakReference>();
                foreach(var reference in ServiceReferences)
                    if(!reference.IsAlive) referencesToDelete.Add(reference);
                foreach(var reference in referencesToDelete)
                    ServiceReferences.Remove(reference);
            }
            WeakReference GetServiceReference(IViewInjectionService service) {
                UpdateServiceReferences();
                return ServiceReferences.FirstOrDefault(x => x.Target == service);
            }
            public void Add(IViewInjectionService service) {
                if(service == null || string.IsNullOrEmpty(service.RegionName)) return;
                if(GetServiceReference(service) != null) return;
                if(FindService(x => x.RegionName == service.RegionName) != null)
                    throw new InvalidOperationException(Exception1);
                ServiceReferences.Add(new WeakReference(service));
            }
            public void Remove(IViewInjectionService service) {
                if(service == null) return;
                var serviceReference = GetServiceReference(service);
                if(serviceReference == null) return;
                ServiceReferences.Remove(serviceReference);
            }
            public IViewInjectionService FindService(Func<IViewInjectionService, bool> predicate) {
                UpdateServiceReferences();
                var reference = ServiceReferences.FirstOrDefault(x => predicate((IViewInjectionService)x.Target));
                return reference != null ? (IViewInjectionService)reference.Target : null;
            }
            public IEnumerable<IViewInjectionService> FindAllServices(Func<IViewInjectionService, bool> predicate) {
                List<IViewInjectionService> res = new List<IViewInjectionService>();
                UpdateServiceReferences();
                var references = ServiceReferences.FindAll(x => predicate((IViewInjectionService)x.Target));
                foreach(var reference in references)
                    res.Add((IViewInjectionService)reference.Target);
                return res;
            }
        }
        class QueueManagerHelper {
            ViewInjectionManager Owner;
            public QueueManagerHelper(ViewInjectionManager owner) {
                Owner = owner;
            }
            public void PutToInjectionQueue(string regionName, object key, Func<object> factory, string viewName, Type viewType) {
                InjectionQueue.Add(new InjectionItem(regionName, key, factory, viewName, viewType));
            }
            public void PutToPersistentInjectionQueue(string regionName, object key, Func<object> factory, string viewName, Type viewType) {
                foreach(InjectionItem item in PersistentInjectionQueue) {
                    if(item.RegionName == regionName && object.Equals(item.Key, key))
                        throw new InvalidOperationException(Exception2);
                }
                PersistentInjectionQueue.Add(new InjectionItem(regionName, key, factory, viewName, viewType));
            }
            public void PutToNavigationQueue(string regionName, object key) {
                NavigationQueue.Add(new NavigationItem(regionName, key));
            }
            public void RemoveFromQueues(string regionName, object key) {
                var injectionItem = InjectionQueue.FirstOrDefault(x => x.RegionName == regionName && object.Equals(x.Key, key));
                injectionItem.Do(x => InjectionQueue.Remove(x));
                var persistentInjectionItem = PersistentInjectionQueue.FirstOrDefault(x => x.RegionName == regionName && object.Equals(x.Key, key));
                persistentInjectionItem.Do(x => PersistentInjectionQueue.Remove(x));
                var navigationItem = NavigationQueue.FirstOrDefault(x => x.RegionName == regionName && object.Equals(x.Key, key));
                navigationItem.Do(x => NavigationQueue.Remove(x));
            }
            public void UpdateQueues() {
                ProcessQueue(InjectionQueue, x => Owner.InjectCore(x.RegionName, x.Key, x.Factory, x.ViewName, x.ViewType));
                ProcessQueue(PersistentInjectionQueue, x => {
                    Owner.InjectCore(x.RegionName, x.Key, x.Factory, x.ViewName, x.ViewType);
                    return false;
                });
                ProcessQueue(NavigationQueue, x => Owner.NavigateCore(x.RegionName, x.Key));
            }

            void ProcessQueue<T>(IList<T> queue, Func<T, bool> processAction) {
                List<T> processedItems = new List<T>();
                foreach(T item in queue.ToList())
                    if(processAction(item)) processedItems.Add(item);
                foreach(T processedItem in processedItems)
                    queue.Remove(processedItem);
            }
            readonly List<InjectionItem> InjectionQueue = new List<InjectionItem>();
            readonly List<InjectionItem> PersistentInjectionQueue = new List<InjectionItem>();
            readonly List<NavigationItem> NavigationQueue = new List<NavigationItem>();

            class InjectionItem {
                public string RegionName { get; private set; }
                public object Key { get; private set; }
                public Func<object> Factory { get; private set; }
                public string ViewName { get; private set; }
                public Type ViewType { get; private set; }
                public InjectionItem(string regionName, object key, Func<object> factory, string viewName, Type viewType) {
                    RegionName = regionName;
                    Key = key;
                    Factory = factory;
                    ViewName = viewName;
                    ViewType = viewType;
                }
            }
            class NavigationItem {
                public readonly string RegionName;
                public readonly object Key;
                public NavigationItem(string regionName, object key) {
                    RegionName = regionName;
                    Key = key;
                }
            }
        }
        class MessengerEx : Messenger {
            public MessengerEx() : base(false, ActionReferenceType.WeakReference) { }
            public void Register<TMessage>(object recipient, object token, bool receiveInheritedMessages, Action action) {
                IActionInvoker actionInvoker = CreateActionInvoker<TMessage>(recipient, action);
                RegisterCore(token, receiveInheritedMessages, typeof(TMessage), actionInvoker);
                RequestCleanup();
            }
            public void Unregister<TMessage>(object recipient, object token, Action action) {
                UnregisterCore(recipient, token, action, typeof(TMessage));
            }
            IActionInvoker CreateActionInvoker<TMessage>(object recipient, Action action) {
                if(action.Method.IsStatic)
                    return new StrongReferenceActionInvoker(recipient, action);
                return new WeakReferenceActionInvoker(recipient, action);
            }
        }
        class NavigatedMessage { }
        class NavigatedAwayMessage { }
        internal class ViewInjectionServiceException : Exception {
            public string RegionName { get; private set; }
            public ViewInjectionServiceException(string regionName, string message) : base(message) {
                RegionName = regionName;
            }

            const string _viewInjectionManager_PreInjectRequiresKey = "The PreInject procedure requires key to be not null and serializable.";
            const string _viewInjectionManager_InjectRequiresPreInject = "This Injection procedure requires the PreInjection method to be called before.";
            const string _viewInjectionManager_KeyShouldBeSerializable = "A key should be serializable.";
            const string _viewInjectionManager_CannotInjectNullViewModel = "Cannot inject a view model, because it is null.";
            const string _viewInjectionManager_CannotResolveViewModel = "ViewModelLocator cannot resolve a view model (the view model name is {0}).";
            const string _viewInjectionService_ViewModelAlreadyExists = "A view model with the same key already exists.";
            const string _strategyManager_NoStrategy = "Cannot find an appropriate strategy for the {0} container type.";
            const string _invalidSelectedViewModel = "Cannot set the SelectedViewModel property to a value that does not exist in the ViewModels collection. Inject the view model before selecting it.";
            const string _contentControl_ContentAlreadySet = "It is impossible to use ViewInjectionService for the control that has the Content property set.";
            const string _itemsControl_ItemsSourceAlreadySet = "It is impossible to use ViewInjectionService for the control that has the ItemsSource property set.";
            const string _viewTypeIsNotSupported = "This region does not support passing viewName/viewType into the injection procedure. Customize view at the target control level.";

            public static ViewInjectionServiceException ViewInjectionManager_PreInjectRequiresKey() {
                return new ViewInjectionServiceException(null, _viewInjectionManager_PreInjectRequiresKey);
            }
            public static ViewInjectionServiceException ViewInjectionManager_InjectRequiresPreInject() {
                return new ViewInjectionServiceException(null, _viewInjectionManager_InjectRequiresPreInject);
            }
            public static ViewInjectionServiceException ViewInjectionManager_KeyShouldBeSerializable() {
                return new ViewInjectionServiceException(null, _viewInjectionService_ViewModelAlreadyExists);
            }
            public static ViewInjectionServiceException ViewInjectionManager_CannotInjectNullViewModel() {
                return new ViewInjectionServiceException(null, _viewInjectionManager_CannotInjectNullViewModel);
            }
            public static ViewInjectionServiceException ViewInjectionManager_CannotResolveViewModel(string viewModelName) {
                return new ViewInjectionServiceException(null, string.Format(_viewInjectionManager_CannotResolveViewModel, viewModelName));
            }
            public static ViewInjectionServiceException ViewInjectionService_ViewModelAlreadyExists(string regionName) {
                return new ViewInjectionServiceException(regionName, _viewInjectionService_ViewModelAlreadyExists);
            }
            public static ViewInjectionServiceException StrategyManager_NoStrategy(object target) {
                return new ViewInjectionServiceException(null, string.Format(_strategyManager_NoStrategy, target.GetType().Name));
            }
            public static ViewInjectionServiceException InvalidSelectedViewModel(string regionNamee) {
                return new ViewInjectionServiceException(regionNamee, _invalidSelectedViewModel);
            }
            public static ViewInjectionServiceException ContentControl_ContentAlreadySet(string regionName) {
                return new ViewInjectionServiceException(regionName, _contentControl_ContentAlreadySet);
            }
            public static ViewInjectionServiceException ItemsControl_ItemsSourceAlreadySet(string regionName) {
                return new ViewInjectionServiceException(regionName, _itemsControl_ItemsSourceAlreadySet);
            }
            public static ViewInjectionServiceException ViewTypeIsNotSupported(string regionName) {
                return new ViewInjectionServiceException(regionName, _viewTypeIsNotSupported);
            }
        }
        #endregion
    }
}
#endregion