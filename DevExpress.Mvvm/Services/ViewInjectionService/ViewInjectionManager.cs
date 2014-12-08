using DevExpress.Mvvm.Native;
using System;
using System.Collections.Generic;
using System.Linq;

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
                        throw new InvalidOperationException("Exception2");
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
                #if !NETFX_CORE
                    if(action.Method.IsStatic)
                #else
                    if(action.GetMethodInfo().IsStatic)
                #endif
                        return new StrongReferenceActionInvoker(recipient, action);
                #if SILVERLIGHT
                    if(ShouldStoreActionItself(action))
                        return new SLWeakReferenceActionInvoker(recipient, action);
                #endif
                return new WeakReferenceActionInvoker(recipient, action);
            }
#if SILVERLIGHT
            static bool ShouldStoreActionItself(Delegate action) {
                if(!action.Method.IsPublic)
                    return true;
                if(action.Target != null && !action.Target.GetType().IsPublic && !action.Target.GetType().IsNestedPublic)
                    return true;
                var name = action.Method.Name;
                if(name.Contains("<") && name.Contains(">"))
                    return true;
                return false;
            }
#endif
        }
        class NavigatedMessage { }
        class NavigatedAwayMessage { }
        #endregion
    }
}