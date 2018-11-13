using System;

#if MVVM
namespace DevExpress.Mvvm.UI.Native {
#else
namespace DevExpress.Xpf.Core.Native {
#endif
    [AttributeUsage(AttributeTargets.Field)]
    public class IgnoreDependencyPropertiesConsistencyCheckerAttribute : Attribute {
    }
}