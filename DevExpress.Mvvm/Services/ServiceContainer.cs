using DevExpress.Mvvm.Native;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Threading;

namespace DevExpress.Mvvm {
    public abstract class ServiceContainerBase {
        protected class ServiceInfo {
            public static ServiceInfo Create(string key, object service, bool yieldToParent = false) {
                if(service == null) return null;
                return new ServiceInfo(key, !string.IsNullOrEmpty(key), service, yieldToParent);
            }
            public static ServiceInfo Create(IServiceContainer container, Type t, string key, ServiceSearchMode searchMode) {
                bool hasKey;
                var service = container.GetService(t, key, searchMode, out hasKey);
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
            public Type ServiceType { get { return Service.With(x => x.GetType()); } }
            public bool YieldToParent { get; private set; }
            public bool Is(Type t) {
                return t.IsAssignableFrom(ServiceType);
            }
        }

        protected abstract IServiceContainer ParentServiceContainer { get; }
        readonly List<ServiceInfo> services;
        bool searchInParentServiceLocker = false;
        public ServiceContainerBase() {
            this.services = new List<ServiceInfo>();
        }

        protected void AddCore(ServiceInfo info) {
            lock(services) services.Add(info);
        }
        protected void RemoveCore(ServiceInfo info) {
            if(info == null) return;
            lock(services) services.Remove(info);
        }
        protected void ClearCore() {
            lock(services) services.Clear();
        }
        protected ServiceInfo GetCore(object service) {
            ServiceInfo res = null;
            lock(services) res = services.FirstOrDefault(x => x.Service == service);
            return res;
        }
        protected virtual object GetServiceCore(Type type, string key, ServiceSearchMode searchMode, out bool serviceHasKey) {
            CheckServiceType(type);
            ServiceInfo serviceInfo;
            lock(services) serviceInfo = FindService(type, key, searchMode);
            if(serviceInfo == null) {
                serviceHasKey = false;
                return null;
            }
            serviceHasKey = serviceInfo.HasKey;
            return serviceInfo.Service;
        }
        protected virtual IEnumerable<object> GetServicesCore(Type type, bool localOnly) {
            IEnumerable<ServiceInfo> localServices = null;
            lock(services) localServices = services.Where(x => x.Is(type)).ToList();
            foreach(var x in localServices) yield return x.Service;
            if(!localOnly && ParentServiceContainer != null)
                foreach(var x in ParentServiceContainer.GetServices(type, false))
                    yield return x;
        }
        ServiceInfo FindService(Type t, string key, ServiceSearchMode searchMode) {
            bool findServiceWithKey = !string.IsNullOrEmpty(key);
            if(searchMode == ServiceSearchMode.LocalOnly || ParentServiceContainer == null)
                return GetLocalService(t, findServiceWithKey, key);

            ServiceInfo parent;
            try {
                if(searchInParentServiceLocker)
                    throw new Exception("A ServiceContainer should not be a direct or indirect parent for itself.");
                searchInParentServiceLocker = true;
                parent = ServiceInfo.Create(ParentServiceContainer, t, key, searchMode);
            } finally { searchInParentServiceLocker = false; }

            if(searchMode == ServiceSearchMode.PreferParents && parent != null && (findServiceWithKey || !parent.HasKey)) return parent;
            ServiceInfo local = GetLocalService(t, findServiceWithKey, key);
            if(local == null) return parent;
            if(parent == null) return local;
            return searchMode == ServiceSearchMode.PreferParents || local.YieldToParent
                ? FindServiceCore(findServiceWithKey, parent, local)
                : FindServiceCore(findServiceWithKey, local, parent);
        }
        ServiceInfo FindServiceCore(bool findServiceWithKey, ServiceInfo primary, ServiceInfo secondary) {
            return findServiceWithKey || !primary.HasKey || secondary.HasKey ? primary : secondary;
        }
        ServiceInfo GetLocalService(Type t, bool findServiceWithKey, string key) {
            IEnumerable<ServiceInfo> _services = null;
            lock(services) _services = services.Where(x => x.Is(t)).ToList();
            var serviceWithKey = _services.LastOrDefault(x => x.Key == key);
            var serviceWithoutKey = _services.LastOrDefault(x => !x.HasKey);
            if(findServiceWithKey) return serviceWithKey;
            return serviceWithoutKey ?? _services.LastOrDefault();
        }
        void CheckServiceType(Type type) {
            if(!type.IsInterface) {
                throw new ArgumentException("Services can be only accessed via interface types");
            }
        }
    }
    public class ServiceContainer : ServiceContainerBase, IServiceContainer {
        static IServiceContainer _default = new DefaultServiceContainer();
        static IServiceContainer custom = null;
        public static IServiceContainer Default {
            get { return custom ?? _default; }
            set { custom = value; }
        }
        protected override IServiceContainer ParentServiceContainer {
            get {
                if(this == Default)
                    return null;
                var supportParentViewModel = owner as ISupportParentViewModel;
                var parentSupportServices = supportParentViewModel != null ? supportParentViewModel.ParentViewModel as ISupportServices : null;
                return ((parentSupportServices != null && parentSupportServices != owner) ? parentSupportServices.ServiceContainer : null) ?? Default;
            }
        }
        readonly object owner;
        public ServiceContainer(object owner) {
            this.owner = owner;
        }
        public void Clear() {
            ClearCore();
        }
        public void RegisterService(object service, bool yieldToParent) {
            RegisterService(null, service, yieldToParent);
        }
        public void RegisterService(string key, object service, bool yieldToParent) {
            if(service == null) throw new ArgumentNullException("service");
            UnregisterService(service);
            AddCore(ServiceInfo.Create(key, service, yieldToParent));
        }
        public void UnregisterService(object service) {
            RemoveCore(GetCore(service));
        }
        public T GetService<T>(string key, ServiceSearchMode searchMode = ServiceSearchMode.PreferLocal) where T : class {
            return ((IServiceContainer)this).GetService<T>(key, searchMode);
        }
        public T GetService<T>(ServiceSearchMode searchMode = ServiceSearchMode.PreferLocal) where T : class {
            return ((IServiceContainer)this).GetService<T>(searchMode);
        }

        IEnumerable<object> IServiceContainer.GetServices(Type type, bool localOnly) {
            return GetServicesCore(type, localOnly);
        }
        object IServiceContainer.GetService(Type type, string key, ServiceSearchMode searchMode, out bool serviceHasKey) {
            return GetServiceCore(type, key, searchMode, out serviceHasKey);
        }
        T IServiceContainer.GetService<T>(string key, ServiceSearchMode searchMode, out bool serviceHasKey) {
            return (T)GetServiceCore(typeof(T), key, searchMode, out serviceHasKey);
        }
        T IServiceContainer.GetService<T>(ServiceSearchMode searchMode) {
            bool serviceHasKey;
            return (T)GetServiceCore(typeof(T), null, searchMode, out serviceHasKey);
        }
        T IServiceContainer.GetService<T>(string key, ServiceSearchMode searchMode) {
            bool serviceHasKey;
            return (T)GetServiceCore(typeof(T), key, searchMode, out serviceHasKey);
        }
    }
    class DefaultServiceContainer : ServiceContainer {
        public DefaultServiceContainer() : base(null) { }
        protected virtual ResourceDictionary GetApplicationResources() {
            bool hasAccess = Application.Current.Return(x => x.Dispatcher.CheckAccess(), () => false);
            return hasAccess ? Application.Current.Resources : null;
        }
        Dictionary<string, object> GetApplicationResources(Type type) {
            var appResources = GetApplicationResources();
            if(appResources == null) return new Dictionary<string, object>();
            return appResources.Keys.OfType<string>()
                .ToDictionary(x => x, x => appResources[x])
                .Where(x => x.Value != null && type.IsAssignableFrom(x.Value.GetType()))
                .ToDictionary(x => x.Key, x => x.Value);
        }
        protected override object GetServiceCore(Type type, string key, ServiceSearchMode searchMode, out bool serviceHasKey) {
            object res = base.GetServiceCore(type, key, searchMode, out serviceHasKey);
            if(res != null) return res;
            var appResources = GetApplicationResources(type);
            if(!string.IsNullOrEmpty(key) && appResources.ContainsKey(key))
                return appResources[key];
            serviceHasKey = true;
            return appResources.FirstOrDefault().Value;
        }
        protected override IEnumerable<object> GetServicesCore(Type type, bool localOnly) {
            foreach(var x in base.GetServicesCore(type, localOnly))
                yield return x;
            foreach(var x in GetApplicationResources(type).Values)
                yield return x;
        }
    }
}