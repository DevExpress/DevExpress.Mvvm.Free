using System;
using System.Collections.Generic;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace DevExpress.Mvvm.UI {
    public abstract class ViewLocatorBase : IViewLocator {
        protected abstract IEnumerable<Assembly> Assemblies { get; }
        Dictionary<string, Type> shortNameToTypeMapping = new Dictionary<string, Type>();
        Dictionary<string, Type> fullNameToTypeMapping = new Dictionary<string, Type>();
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
            if(shortNameToTypeMapping.TryGetValue(viewName, out typeFromDictioanry)
                || fullNameToTypeMapping.TryGetValue(viewName, out typeFromDictioanry))
                return typeFromDictioanry;

            if(enumerator == null) enumerator = GetTypes();
            while(enumerator.MoveNext()) {
                if(!shortNameToTypeMapping.ContainsKey(enumerator.Current.Name)) {
                    shortNameToTypeMapping[enumerator.Current.Name] = enumerator.Current;
                    fullNameToTypeMapping[enumerator.Current.FullName] = enumerator.Current;
                }
                if(enumerator.Current.Name == viewName || enumerator.Current.FullName == viewName)
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
                Type[] types;
                try {
                    types = asm.GetTypes();
                } catch(ReflectionTypeLoadException e) {
                    types = e.Types;
                }
                foreach(Type type in types) {
                    yield return type;
                }
            }
        }
    }
}