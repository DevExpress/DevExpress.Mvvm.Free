using DevExpress.Mvvm.Native;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace DevExpress.Mvvm {
    class DefaultServiceContainer : ServiceContainer {
        public DefaultServiceContainer() : base(null) { }
        protected virtual ResourceDictionary GetApplicationResources() {
            bool hasAccess = Application.Current.Return(x => x.Dispatcher.CheckAccess(), () => false);
            return hasAccess ? Application.Current.Resources : null;
        }
        Dictionary<string, object> GetApplicationResources(Type type) {
            var appResources = GetApplicationResources();
            if(appResources == null)
                return new Dictionary<string, object>();
            return appResources.Keys.OfType<string>()
                .ToDictionary(x => x, x => appResources[x])
                .Where(x => x.Value != null && type.IsAssignableFrom(x.Value.GetType()))
                .ToDictionary(x => x.Key, x => x.Value);
        }
        protected override object GetServiceCore(Type type, string key, ServiceSearchMode searchMode, out bool serviceHasKey) {
            object res = base.GetServiceCore(type, key, searchMode, out serviceHasKey);
            if(res != null)
                return res;
            var appResources = GetApplicationResources(type);
            object service;
            if(!string.IsNullOrEmpty(key) && appResources.TryGetValue(key, out service))
                return service;
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