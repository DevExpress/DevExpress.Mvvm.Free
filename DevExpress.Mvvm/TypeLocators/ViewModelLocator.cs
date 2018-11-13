using DevExpress.Mvvm.POCO;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Windows;
using System.Linq;
using System.Globalization;

namespace DevExpress.Mvvm {
    public interface IViewModelLocator {
        object ResolveViewModel(string name);
        Type ResolveViewModelType(string name);
        string GetViewModelTypeName(Type type);
    }
    public class ViewModelLocator : LocatorBase, IViewModelLocator {
        static IViewModelLocator _defaultInstance = new ViewModelLocator(Application.Current);
        static IViewModelLocator _default;
        public static IViewModelLocator Default { get { return _default ?? _defaultInstance; } set { _default = value; } }

        readonly IEnumerable<Assembly> assemblies;
        protected override IEnumerable<Assembly> Assemblies { get { return assemblies; } }
        public ViewModelLocator(Application application)
            : this(EntryAssembly != null && !ViewModelBase.IsInDesignMode ? new[] { EntryAssembly } : new Assembly[0]) {
        }
        public ViewModelLocator(params Assembly[] assemblies)
            : this((IEnumerable<Assembly>)assemblies) {
        }
        public ViewModelLocator(IEnumerable<Assembly> assemblies) {
            this.assemblies = assemblies;
        }
        public virtual Type ResolveViewModelType(string name) {
            IDictionary<string, string> properties;
            Type res = ResolveType(name, out properties);
            if (res == null) return null;
            bool isPOCO = GetIsPOCOViewModelType(res, properties);
            return isPOCO ? ViewModelSource.GetPOCOType(res) : res;
        }
        public virtual string GetViewModelTypeName(Type type) {
            Dictionary<string, string> properties = new Dictionary<string, string>();
            if(type.GetInterfaces().Any(x => x == typeof(IPOCOViewModel))) {
                SetIsPOCOViewModelType(properties, true);
                type = type.BaseType;
            }
            return ResolveTypeName(type, properties);
        }
        protected bool GetIsPOCOViewModelType(Type type, IDictionary<string, string> properties) {
            string isPOCO;
            if(type.GetCustomAttributes(typeof(DataAnnotations.POCOViewModelAttribute), true).Any())
                return true;
            if(properties.TryGetValue("IsPOCOViewModel", out isPOCO))
                return bool.Parse(isPOCO);
            return false;
        }
        protected void SetIsPOCOViewModelType(IDictionary<string, string> properties, bool value) {
            properties.Add("IsPOCOViewModel", value.ToString());
        }
        object IViewModelLocator.ResolveViewModel(string name) {
            Type type = ((IViewModelLocator)this).ResolveViewModelType(name);
            if (type == null)
                return null;
            return CreateInstance(type, name);
        }
    }
}