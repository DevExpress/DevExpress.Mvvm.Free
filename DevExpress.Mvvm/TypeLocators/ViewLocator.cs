using System;
using System.Reflection;
using DevExpress.Mvvm.UI;

namespace DevExpress.Mvvm.UI {
    public interface IViewLocator {
        object ResolveView(string name);
        Type ResolveViewType(string name);
        string GetViewTypeName(Type type);
    }
}
namespace DevExpress.Mvvm.Native {
    static class ViewLocatorHelper {
        const string ViewLocatorTypeName = "DevExpress.Mvvm.UI.ViewLocator";
        static PropertyInfo ViewLocatorDefaultProperty;
        public static IViewLocator Default {
            get {
                if(ViewLocatorDefaultProperty == null) {
                    var viewLocatorType = DynamicAssemblyHelper.MvvmUIAssembly.GetType(ViewLocatorTypeName);
                    ViewLocatorDefaultProperty = viewLocatorType.GetProperty("Default", BindingFlags.Static | BindingFlags.Public);
                }
                return (IViewLocator)ViewLocatorDefaultProperty.GetValue(null, null);
            }
        }
    }
}