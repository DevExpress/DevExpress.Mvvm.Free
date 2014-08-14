using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DevExpress.Mvvm {
    public interface IServiceContainer {
        void Clear();
        void RegisterService(object service);
        void RegisterService(string key, object service);
        T GetService<T>(string key, ServiceSearchMode searchMode = ServiceSearchMode.PreferLocal)
            where T : class;
        T GetService<T>(ServiceSearchMode searchMode = ServiceSearchMode.PreferLocal)
            where T : class;
    }
    public static class ServiceContainerExtensions {
        public static T GetRequiredService<T>(this IServiceContainer serviceContainer, string key, ServiceSearchMode searchMode = ServiceSearchMode.PreferLocal) where T : class {
            return CheckService(serviceContainer.GetService<T>(key, searchMode));
        }
        public static T GetRequiredService<T>(this IServiceContainer serviceContainer, ServiceSearchMode searchMode = ServiceSearchMode.PreferLocal) where T : class {
            return CheckService(serviceContainer.GetService<T>(searchMode));
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