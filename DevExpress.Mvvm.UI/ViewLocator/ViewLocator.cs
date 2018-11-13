using System.Collections.Generic;
using System.Reflection;
using System.Windows;
using System;
#if !NETFX_CORE
using DevExpress.Mvvm.POCO;
#else
using Windows.UI.Xaml;
using DevExpress.Mvvm.Native;
using Windows.ApplicationModel;
#endif

namespace DevExpress.Mvvm.UI {
    public class ViewLocator : LocatorBase, IViewLocator {
        public static IViewLocator Default { get { return _default ?? Instance; } set { _default = value; } }
        static IViewLocator _default = null;
        internal static readonly IViewLocator Instance = new ViewLocator(Application.Current);

        readonly IEnumerable<Assembly> assemblies;
        protected override IEnumerable<Assembly> Assemblies { get { return assemblies; } }
        public ViewLocator(Application application)
#if !NETFX_CORE
            : this(EntryAssembly != null && !EntryAssembly.IsInDesignMode() ? new[] { EntryAssembly } : new Assembly[0]) {
#else
            : this(EntryAssembly != null && !DesignMode.DesignModeEnabled ? new[] { EntryAssembly } : new Assembly[0]) {
#endif

        }
        public ViewLocator(IEnumerable<Assembly> assemblies) {
            this.assemblies = assemblies;
        }
        public ViewLocator(params Assembly[] assemblies)
            : this((IEnumerable<Assembly>)assemblies) {
        }

        public Type ResolveViewType(string viewName) {
            IDictionary<string, string> properties;
            return ResolveType(viewName, out properties);
        }
        public string GetViewTypeName(Type type) {
            return ResolveTypeName(type, null);
        }
        object IViewLocator.ResolveView(string viewName) {
            Type viewType = ((IViewLocator)this).ResolveViewType(viewName);
            if(viewType != null)
                return CreateInstance(viewType, viewName);
            return CreateFallbackView(viewName);
        }
        protected virtual object CreateFallbackView(string documentType) {
            return ViewLocatorExtensions.CreateFallbackView(GetErrorMessage(documentType));
        }
        protected string GetErrorMessage(string documentType) {
            return ViewLocatorExtensions.GetErrorMessage_CannotResolveViewType(documentType);
        }
    }
}
