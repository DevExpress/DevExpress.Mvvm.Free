using System.Collections.Generic;
using System.Reflection;
using System.Windows;
using DevExpress.Mvvm.POCO;

namespace DevExpress.Mvvm.UI {
    public class ViewLocator : ViewLocatorBase {
        static Assembly entryAssembly;
        protected static Assembly EntryAssembly {
            get {
                if(entryAssembly == null) {
#if !SILVERLIGHT
                    entryAssembly = Assembly.GetEntryAssembly();
#else
                    entryAssembly = Application.Current == null ? null : Application.Current.GetType().Assembly;
#endif
                }
                return entryAssembly;
            }
            set { entryAssembly = value; }
        }

        public static IViewLocator Default { get; set; }
        internal static readonly IViewLocator Instance = new ViewLocator(Application.Current);

        readonly IEnumerable<Assembly> assemblies;
        public ViewLocator(Application application)
            : this(EntryAssembly != null && !EntryAssembly.IsInDesignMode() ? new[] { EntryAssembly } : new Assembly[0]) {
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