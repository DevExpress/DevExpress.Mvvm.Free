using DevExpress.Mvvm.Native;
using System;
using System.Collections.Generic;
using System.Linq;


namespace DevExpress.Mvvm {
    public class ServiceContainer : IServiceContainer {
        class ServiceInfo {
            public ServiceInfo(bool hasKey, object service, bool yieldToParent) {
                this.HasKey = hasKey;
                this.Service = service;
                this.YieldToParent = yieldToParent;
            }
            public bool HasKey { get; private set; }
            public object Service { get; private set; }
            public bool YieldToParent { get; private set; }
        }

        public static IServiceContainer Default = new ServiceContainer(null);

        readonly object owner;
        IServiceContainer ParentServiceContainer {
            get {
                if(this == Default)
                    return null;
                var supportParentViewModel = owner as ISupportParentViewModel;
                var parentSupportServices = supportParentViewModel != null ? supportParentViewModel.ParentViewModel as ISupportServices : null;
                return ((parentSupportServices != null && parentSupportServices != owner) ? parentSupportServices.ServiceContainer : null) ?? Default;
            }
        }
        public ServiceContainer(object owner) {
            this.owner = owner;
        }
        Dictionary<string, ServiceInfo> servicesByKey = new Dictionary<string, ServiceInfo>();
        Dictionary<Type, ServiceInfo> servicesByInterfaceType = new Dictionary<Type, ServiceInfo>();
        public void Clear() {
            servicesByKey.Clear();
            servicesByInterfaceType.Clear();
        }
        public void RegisterService(object service, bool yieldToParent) {
            RegisterService(null, service, yieldToParent);
        }
        public void RegisterService(string key, object service, bool yieldToParent) {
            if(service == null)
                throw new ArgumentNullException("service");
            ServiceInfo serviceInfo = new ServiceInfo(!string.IsNullOrEmpty(key), service, yieldToParent);
            RegisterInterfacesCore(serviceInfo, !serviceInfo.HasKey);
            if(serviceInfo.HasKey)
                servicesByKey[key] = serviceInfo;
        }
        void RegisterInterfacesCore(ServiceInfo serviceInfo, bool allowOverwrite) {
            foreach(Type type in serviceInfo.Service.GetType().GetInterfaces()) {
                if(allowOverwrite || !servicesByInterfaceType.ContainsKey(type))
                    servicesByInterfaceType[type] = serviceInfo;
            }
        }
        public T GetService<T>(string key, ServiceSearchMode searchMode = ServiceSearchMode.PreferLocal) where T : class {
            bool serviceHasKey;
            IServiceContainer serviceCOntainer = this;
            return serviceCOntainer.GetService<T>(key, searchMode, out serviceHasKey);
        }
        public T GetService<T>(ServiceSearchMode searchMode = ServiceSearchMode.PreferLocal) where T : class {
            return this.GetService<T>(null, searchMode);
        }
        T IServiceContainer.GetService<T>(string key, ServiceSearchMode searchMode, out bool serviceHasKey) {
            CheckServiceType<T>();
            ServiceInfo serviceInfo = FindService<T>(key, searchMode);
            if(serviceInfo == null) {
                serviceHasKey = false;
                return null;
            }
            serviceHasKey = serviceInfo.HasKey;
            return (T)serviceInfo.Service;
        }
        ServiceInfo FindService<T>(string key, ServiceSearchMode searchMode) where T : class {
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
            return findServiceWithKey ? primary : !primary.HasKey ? primary : !secondary.HasKey ? secondary : primary;
        }
        ServiceInfo GetLocalService<T>(bool findServiceWithKey, string key) where T : class {
            return findServiceWithKey ? servicesByKey.GetValueOrDefault(key) : servicesByInterfaceType.GetValueOrDefault(typeof(T));
        }
        ServiceInfo GetParentService<T>(string key, ServiceSearchMode searchMode) where T : class {
            bool serviceHasKey;
            T service = ParentServiceContainer.GetService<T>(key, searchMode, out serviceHasKey);
            return service == null ? null : new ServiceInfo(serviceHasKey, service, false);
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
}