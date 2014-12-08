using System;
using System.Collections.Generic;
using System.Reflection;
using System.Windows;
#if !NETFX_CORE
using System.Windows.Controls;
using System.Windows.Media;
#else
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml;
using Windows.UI;
using Windows.UI.Xaml.Media;
using DevExpress.Mvvm.Native;
#endif

namespace DevExpress.Mvvm.UI {
    public abstract class ViewLocatorBase : IViewLocator {
        protected abstract IEnumerable<Assembly> Assemblies { get; }
        Dictionary<string, Type> types = new Dictionary<string, Type>();
        IEnumerator<Type> enumerator;

        object IViewLocator.ResolveView(string viewName) {
            Type viewType = ((IViewLocator)this).ResolveViewType(viewName);
            if(viewType != null)
                return Activator.CreateInstance(viewType);
            return CreateFallbackView(viewName);
        }
        Type IViewLocator.ResolveViewType(string viewName) {
            if(string.IsNullOrEmpty(viewName))
                return null;

            Type typeFromDictioanry;
            if(types.TryGetValue(viewName, out typeFromDictioanry))
                return typeFromDictioanry;

            if(enumerator == null)
                enumerator = GetTypes();
            while(enumerator.MoveNext()) {
                if(!types.ContainsKey(enumerator.Current.Name)) {
                    types[enumerator.Current.Name] = enumerator.Current;
                }
                if(enumerator.Current.Name == viewName)
                    return enumerator.Current;
            }
            return null;
        }

        protected virtual object CreateFallbackView(string documentType) {
            return ViewLocatorExtensions.CreateFallbackView(GetErrorMessage(documentType));
        }
        protected string GetErrorMessage(string documentType) {
            return ViewLocatorExtensions.GetErrorMessage_CannotResolveViewType(documentType);
        }
        protected virtual IEnumerator<Type> GetTypes() {
            foreach(Assembly asm in Assemblies) {
#if !NETFX_CORE
                Type[] types = asm.GetTypes();
#else
                Type[] types = asm.GetExportedTypes();
#endif
                foreach(Type type in types) {
                    yield return type;
                }
            }
        }
    }
}