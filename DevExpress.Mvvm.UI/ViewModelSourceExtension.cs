using DevExpress.Mvvm.Native;
using System;
using System.Windows.Markup;

namespace DevExpress.Mvvm.UI {
    public class ViewModelSourceExtension : MarkupExtension {
        public Type Type { get; set; }
        public ViewModelSourceExtension() { }
        public ViewModelSourceExtension(Type type) {
            this.Type = type;
        }
        public override object ProvideValue(IServiceProvider serviceProvider) {
            return Type != null ? ViewModelSourceHelper.Create(Type) : null;
        }
    }
}