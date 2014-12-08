using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using DevExpress.Mvvm.Native;

namespace DevExpress.Mvvm {
    public enum ServiceSearchMode { PreferLocal, LocalOnly, PreferParents }
    public interface IServiceContainer {
        void Clear();
        void RegisterService(object service, bool yieldToParent = false);
        void RegisterService(string key, object service, bool yieldToParent = false);
        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        T GetService<T>(string key, ServiceSearchMode searchMode, out bool serviceHasKey) where T : class;
        T GetService<T>(ServiceSearchMode searchMode = ServiceSearchMode.PreferLocal) where T : class;
        T GetService<T>(string key, ServiceSearchMode searchMode = ServiceSearchMode.PreferLocal) where T : class;
    }
    public static class ServiceContainerExtensions {
        public static T GetRequiredService<T>(this IServiceContainer serviceContainer, string key) where T : class {
            return serviceContainer.GetRequiredService<T>(key, ServiceSearchMode.PreferLocal);
        }
        public static T GetRequiredService<T>(this IServiceContainer serviceContainer) where T : class {
            return serviceContainer.GetRequiredService<T>(null, ServiceSearchMode.PreferLocal);
        }
        [EditorBrowsable(EditorBrowsableState.Never)]
        public static T GetRequiredService<T>(this IServiceContainer serviceContainer, string key, ServiceSearchMode searchMode) where T : class {
            VerifyServiceContainer(serviceContainer);
            return CheckService(serviceContainer.GetService<T>(key, searchMode));
        }
        [EditorBrowsable(EditorBrowsableState.Never)]
        public static T GetRequiredService<T>(this IServiceContainer serviceContainer, ServiceSearchMode searchMode) where T : class {
            VerifyServiceContainer(serviceContainer);
            return CheckService(serviceContainer.GetService<T>(searchMode));
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