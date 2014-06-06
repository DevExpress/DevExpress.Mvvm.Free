using System;
using System.Collections.Generic;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace DevExpress.Mvvm.UI {
    public abstract class ViewLocatorBase : IViewLocator {
        protected abstract IEnumerable<Assembly> Assemblies { get; }
        protected Dictionary<string, Type> types = new Dictionary<string, Type>();
        protected IEnumerator<Type> enumerator;

        object IViewLocator.ResolveView(string name) {
            return ResolveViewCore(name) ?? CreateFallbackView(name);
        }
        protected virtual object CreateFallbackView(string documentType) {
            return new ContentPresenter() {
                Content = new TextBlock() {
                    Text = string.Format("\"{0}\" type not found.", documentType),
                    FontSize = 25,
                    Foreground = new System.Windows.Media.SolidColorBrush(Colors.Red),
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                }
            };
        }

       protected object ResolveViewCore(string name) {
            Type typeFromDictioanry;
            if(types.TryGetValue(name, out typeFromDictioanry))
                return CreateInstance(typeFromDictioanry);

            if(enumerator == null)
                enumerator = GetTypes();
            while(enumerator.MoveNext()) {
                if(!types.ContainsKey(enumerator.Current.Name)) {
                    types[enumerator.Current.Name] = enumerator.Current;
                }
                if(enumerator.Current.Name == name)
                    return CreateInstance(enumerator.Current);
            }
            return null;
        }
        object CreateInstance(Type type) {
            return Activator.CreateInstance(type);
        }
        protected virtual IEnumerator<Type> GetTypes() {
            foreach(Assembly asm in Assemblies) {
                foreach(Type type in asm.GetTypes()) {
                    yield return type;
                }
            }
        }
    }
}