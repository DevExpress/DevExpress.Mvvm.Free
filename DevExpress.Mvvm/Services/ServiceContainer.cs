using DevExpress.Mvvm.Native;
using System;
using System.Collections.Generic;
using System.Linq;
#if !NETFX_CORE
using System.Windows;
#else
using Windows.UI.Xaml;
using Windows.ApplicationModel.Core;
#endif

namespace DevExpress.Mvvm {
    public class ServiceContainer : IServiceContainer {
        class ServiceInfo {
            public static ServiceInfo Create(string key, object service, bool yieldToParent = false) {
                if(service == null) return null;
                return new ServiceInfo(key, !string.IsNullOrEmpty(key), service, yieldToParent);
            }
            public static ServiceInfo Create<T>(IServiceContainer container, string key, ServiceSearchMode searchMode) where T : class {
                bool hasKey;
                var service = container.GetService<T>(key, searchMode, out hasKey);
                if(service == null) return null;
                return new ServiceInfo(key, hasKey, service, false);
            }
            ServiceInfo(string key, bool hasKey, object service, bool yieldToParent) {
                this.Key = key;
                this.HasKey = hasKey;
                this.Service = service;
                this.YieldToParent = yieldToParent;
            }
            public bool HasKey { get; private set; }
            public string Key { get; private set; }
            public object Service { get; private set; }
            public bool YieldToParent { get; private set; }
            public bool Is<T>() {
                return Service is T;
            }
        }

        static IServiceContainer _default = new DefaultServiceContainer();
        static IServiceContainer custom = null;
        public static IServiceContainer Default {
            get { return custom ?? _default; }
            set { custom = value; }
        }
        readonly object owner;
        bool searchInParentServiceInProgress = false;
        IServiceContainer ParentServiceContainer {
            get {
                if(this == Default)
                    return null;
                var supportParentViewModel = owner as ISupportParentViewModel;
                var parentSupportServices = supportParentViewModel != null ? supportParentViewModel.ParentViewModel as ISupportServices : null;
                return ((parentSupportServices != null && parentSupportServices != owner) ? parentSupportServices.ServiceContainer : null) ?? Default;
            }
        }
        List<ServiceInfo> services = new List<ServiceInfo>();

        public ServiceContainer(object owner) {
            this.owner = owner;
        }
        public void Clear() {
            services.Clear();
        }
        public void RegisterService(object service, bool yieldToParent) {
            RegisterService(null, service, yieldToParent);
        }
        public void RegisterService(string key, object service, bool yieldToParent) {
            if(service == null) throw new ArgumentNullException("service");
            UnregisterService(service);
            services.Add(ServiceInfo.Create(key, service, yieldToParent));
        }
        public void UnregisterService(object service) {
            services.FirstOrDefault(x => x.Service == service).Do(x => services.Remove(x));
        }
        public T GetService<T>(string key, ServiceSearchMode searchMode = ServiceSearchMode.PreferLocal) where T : class {
            bool serviceHasKey;
            IServiceContainer serviceCOntainer = this;
            return serviceCOntainer.GetService<T>(key, searchMode, out serviceHasKey);
        }
        public T GetService<T>(ServiceSearchMode searchMode = ServiceSearchMode.PreferLocal) where T : class {
            return this.GetService<T>(null, searchMode);
        }
        protected virtual T GetServiceCore<T>(string key, ServiceSearchMode searchMode, out bool serviceHasKey) where T : class {
            CheckServiceType<T>();
            ServiceInfo serviceInfo = FindService<T>(key, searchMode);
            if(serviceInfo == null) {
                serviceHasKey = false;
                return null;
            }
            serviceHasKey = serviceInfo.HasKey;
            return (T)serviceInfo.Service;
        }
        T IServiceContainer.GetService<T>(string key, ServiceSearchMode searchMode, out bool serviceHasKey) {
            return GetServiceCore<T>(key, searchMode, out serviceHasKey);
        }
        ServiceInfo FindService<T>(string key, ServiceSearchMode searchMode) where T : class {
            if(searchInParentServiceInProgress)
                throw new Exception("A ServiceContainer should not be a direct or indirect parent for itself.");
            bool findServiceWithKey = !string.IsNullOrEmpty(key);
            if(searchMode == ServiceSearchMode.LocalOnly || ParentServiceContainer == null)
                return GetLocalService<T>(findServiceWithKey, key);
            ServiceInfo parent = GetParentService<T>(key, searchMode);
            if(searchMode == ServiceSearchMode.PreferParents && parent != null && (findServiceWithKey || !parent.HasKey)) return parent;
            ServiceInfo local = GetLocalService<T>(findServiceWithKey, key);
            if(local == null) return parent;
            if(parent == null) return local;
            return searchMode == ServiceSearchMode.PreferParents || local.YieldToParent
                ? FindServiceCore(findServiceWithKey, parent, local)
                : FindServiceCore(findServiceWithKey, local, parent);
        }
        ServiceInfo FindServiceCore(bool findServiceWithKey, ServiceInfo primary, ServiceInfo secondary) {
            return findServiceWithKey || !primary.HasKey || secondary.HasKey ? primary : secondary;
        }
        ServiceInfo GetLocalService<T>(bool findServiceWithKey, string key) where T : class {
            var _services = services.Where(x => x.Is<T>());
            var serviceWithKey = _services.LastOrDefault(x => x.Key == key);
            var serviceWithoutKey = _services.LastOrDefault(x => !x.HasKey);
            if(findServiceWithKey) return serviceWithKey;
            return serviceWithoutKey ?? _services.LastOrDefault();
        }
        ServiceInfo GetParentService<T>(string key, ServiceSearchMode searchMode) where T : class {
            searchInParentServiceInProgress = true;
            try {
                return ServiceInfo.Create<T>(ParentServiceContainer, key, searchMode);
            } finally {
                searchInParentServiceInProgress = false;
            }
        }

        void CheckServiceType<T>() {
            Type type = typeof(T);
#if !NETFX_CORE
            if(!type.IsInterface) {
#else
            if(!type.IsInterface()) {
#endif
                throw new ArgumentException("Services can be only accessed via interface types");
            }
        }
    }
    class DefaultServiceContainer : ServiceContainer {
        public DefaultServiceContainer() : base(null) { }
        protected virtual ResourceDictionary GetApplicationResources() {
#if SILVERLIGHT
            bool hasAccess = Deployment.Current.Dispatcher.CheckAccess();
#elif !NETFX_CORE
            bool hasAccess = Application.Current.Return(x => x.Dispatcher.CheckAccess(), () => false);
#else
            bool hasAccess = Application.Current != null && CoreApplication.MainView.CoreWindow.Return(x => x.Dispatcher.HasThreadAccess, () => false);
#endif
            return hasAccess ? Application.Current.Resources : null;
        }
        protected override T GetServiceCore<T>(string key, ServiceSearchMode searchMode, out bool serviceHasKey) {
            object res = base.GetServiceCore<T>(key, searchMode, out serviceHasKey);
            if(res != null) return (T)res;
            var appResources = GetApplicationResources();
            if(appResources != null) {
                if(string.IsNullOrEmpty(key))
                    res = appResources.Values.OfType<T>().FirstOrDefault();
                else {
                    var resKey = appResources.Keys.OfType<object>().FirstOrDefault(x => x.Equals(key));
                    if(resKey != null)
                        res = appResources[resKey];
                    else res = appResources.Values.OfType<T>().FirstOrDefault();
                }
                serviceHasKey = true;
            }
            return (T)res;
        }
    }
}