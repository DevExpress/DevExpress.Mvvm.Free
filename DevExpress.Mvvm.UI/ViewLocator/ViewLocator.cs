using System.Collections.Generic;
using System.Reflection;
using System.Windows;
#if !NETFX_CORE
using DevExpress.Mvvm.POCO;
#else
using Windows.UI.Xaml;
using DevExpress.Mvvm.Native;
using Windows.ApplicationModel;
#endif

namespace DevExpress.Mvvm.UI {
    public class ViewLocator : ViewLocatorBase {
        static Assembly entryAssembly;
        protected static Assembly EntryAssembly {
            get {
                if(entryAssembly == null) {
#if SILVERLIGHT
                    entryAssembly = Application.Current == null ? null : Application.Current.GetType().Assembly;
#elif NETFX_CORE
                    entryAssembly = Application.Current == null ? null : Application.Current.GetType().GetAssembly();
#else
                    entryAssembly = Assembly.GetEntryAssembly();
#endif
                }
                return entryAssembly;
            }
            set { entryAssembly = value; }
        }

        public static IViewLocator Default { get { return _default ?? Instance; } set { _default = value; } }
        static IViewLocator _default = null;
        internal static readonly IViewLocator Instance = new ViewLocator(Application.Current);

        readonly IEnumerable<Assembly> assemblies;
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

        protected override IEnumerable<Assembly> Assemblies {
            get {
                return assemblies;
            }
        }
    }
}