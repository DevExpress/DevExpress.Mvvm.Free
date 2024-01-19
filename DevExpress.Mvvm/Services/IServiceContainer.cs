using DevExpress.Mvvm.Native;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace DevExpress.Mvvm {
    public enum ServiceSearchMode { PreferLocal, LocalOnly, PreferParents }
    public interface IServiceContainer {
        void Clear();
        void RegisterService(object service, bool yieldToParent = false);
        void RegisterService(string key, object service, bool yieldToParent = false);
        void UnregisterService(object service);

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        object GetService(Type type, string key, ServiceSearchMode searchMode, out bool serviceHasKey);
        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        IEnumerable<object> GetServices(Type type, bool localOnly);

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        T GetService<T>(string key, ServiceSearchMode searchMode, out bool serviceHasKey) where T : class;
        T GetService<T>(ServiceSearchMode searchMode = ServiceSearchMode.PreferLocal) where T : class;
        T GetService<T>(string key, ServiceSearchMode searchMode = ServiceSearchMode.PreferLocal) where T : class;
    }
    public static class ServiceContainerExtensions {
        public static object GetService(this IServiceContainer serviceContainer, Type type, string key, ServiceSearchMode searchMode = ServiceSearchMode.PreferLocal) {
            VerifyServiceContainer(serviceContainer);
            bool serviceHasKey;
            return serviceContainer.GetService(type, key, searchMode, out serviceHasKey);
        }
        public static IEnumerable<T> GetServices<T>(this IServiceContainer serviceContainer, bool localOnly = true) where T : class {
            VerifyServiceContainer(serviceContainer);
            return serviceContainer.GetServices(typeof(T), localOnly).OfType<T>();
        }

        public static T GetRequiredService<T>(this IServiceContainer serviceContainer, ServiceSearchMode searchMode = ServiceSearchMode.PreferLocal) where T : class {
            VerifyServiceContainer(serviceContainer);
            bool serviceHasKey;
            var res = (T)serviceContainer.GetService(typeof(T), null, searchMode, out serviceHasKey);
            return CheckService(res);
        }
        public static T GetRequiredService<T>(this IServiceContainer serviceContainer, string key, ServiceSearchMode searchMode = ServiceSearchMode.PreferLocal) where T : class {
            VerifyServiceContainer(serviceContainer);
            bool serviceHasKey;
            var res = (T)serviceContainer.GetService(typeof(T), key, searchMode, out serviceHasKey);
            return CheckService(res);
        }

        static void VerifyServiceContainer(IServiceContainer serviceContainer) {
            if(serviceContainer == null)
                throw new ArgumentNullException("serviceContainer");
        }
        static T CheckService<T>(T service) where T : class {
            if(service == null)
                throw new ServiceNotFoundException();
            return service;
        }
    }
    public class ServiceNotFoundException : Exception {
        public ServiceNotFoundException() :
            base("The target service is not found") {
        }
    }
}