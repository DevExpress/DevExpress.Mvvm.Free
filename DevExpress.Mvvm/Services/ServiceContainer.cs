using DevExpress.Mvvm.Native;
using System;
using System.Collections.Generic;
using System.Linq;


namespace DevExpress.Mvvm {
    public enum ServiceSearchMode { PreferLocal, LocalOnly, PreferParents }
    public class ServiceContainer : IServiceContainer {
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
        Dictionary<string, object> servicesByKey = new Dictionary<string, object>();
        Dictionary<Type, object> servicesByInterfaceType = new Dictionary<Type, object>();
        public void Clear() {
            servicesByKey.Clear();
            servicesByInterfaceType.Clear();
        }
        public void RegisterService(object service) {
            if(service == null)
                throw new ArgumentNullException("service");
            RegisterInterfacesCore(service, true);
        }
        public void RegisterService(string key, object service) {
            if(service == null)
                throw new ArgumentNullException("service");
            if(string.IsNullOrEmpty(key)) {
                RegisterService(service);
            } else {
                servicesByKey[key] = service;
                RegisterInterfacesCore(service, false);
            }
        }
        void RegisterInterfacesCore(object service, bool allowOverwrite) {
            foreach(Type type in service.GetType().GetInterfaces()) {
                if(allowOverwrite || !servicesByInterfaceType.ContainsKey(type))
                    servicesByInterfaceType[type] = service;
            }
        }
        public T GetService<T>(string key, ServiceSearchMode searchMode = ServiceSearchMode.PreferLocal) where T : class {
            CheckServiceType<T>();
            switch(searchMode) {
                case ServiceSearchMode.PreferLocal:
                    return GetLocalService<T>(key) ?? GetParentService<T>(key, ServiceSearchMode.PreferLocal);
                case ServiceSearchMode.LocalOnly:
                    return GetLocalService<T>(key);
                default:
                    return GetParentService<T>(key, ServiceSearchMode.PreferParents) ?? GetLocalService<T>(key);
            }
        }
        public T GetService<T>(ServiceSearchMode searchMode = ServiceSearchMode.PreferLocal) where T : class {
            CheckServiceType<T>();
            switch(searchMode) {
                case ServiceSearchMode.PreferLocal:
                    return GetLocalService<T>() ?? GetParentService<T>(ServiceSearchMode.PreferLocal);
                case ServiceSearchMode.LocalOnly:
                    return GetLocalService<T>();
                default:
                    return GetParentService<T>(ServiceSearchMode.PreferParents) ?? GetLocalService<T>();
            }
        }
        T GetLocalService<T>(string key) where T : class {
            return (T)servicesByKey.GetValueOrDefault(key);
        }
        T GetLocalService<T>() where T : class {
            return (T)servicesByInterfaceType.GetValueOrDefault(typeof(T));
        }
        T GetParentService<T>(string key, ServiceSearchMode searchMode) where T : class {
            return ParentServiceContainer != null ? ParentServiceContainer.GetService<T>(key, searchMode) : (T)null;
        }
        T GetParentService<T>(ServiceSearchMode searchMode) where T : class {
            return (ParentServiceContainer != null ? ParentServiceContainer.GetService<T>(searchMode) : (T)null);
        }
        void CheckServiceType<T>() {
            Type type = typeof(T);
            if(!type.IsInterface) {
                throw new ArgumentException("Services can be only accessed via interface types");
            }
        }
    }
}