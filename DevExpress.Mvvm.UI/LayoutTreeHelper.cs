using System.Collections.Generic;
using System.Windows;
using System.Linq;
using DevExpress.Mvvm.Native;
#if !FREE && !NETFX_CORE
using DevExpress.Mvvm.UI.Native;
#endif
using System;
#if !FREE
using DevExpress.Xpf.Core.Native;
#endif
#if NETFX_CORE
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;
#else
using System.Windows.Media;
using System.Windows.Media.Media3D;
#endif

namespace DevExpress.Mvvm.UI {
    public static class LayoutTreeHelper {
        static DependencyObject GetParent(DependencyObject element) {
#if !NETFX_CORE
            if(element is Visual || element is Visual3D) 
                return VisualTreeHelper.GetParent(element);
            if(element is FrameworkContentElement)
                return LogicalTreeHelper.GetParent(element);
            return null;
#else
            return VisualTreeHelper.GetParent(element);
#endif
        }

        public static IEnumerable<DependencyObject> GetVisualParents(DependencyObject child, DependencyObject stopNode = null) {
            return GetVisualParentsCore(child, stopNode, false);
        }
        public static IEnumerable<DependencyObject> GetVisualChildren(DependencyObject parent) {
            return GetVisualChildrenCore(parent, false);
        }
#if !NETFX_CORE
        public static IEnumerable<DependencyObject> GetLogicalChildren(DependencyObject parent) {
            return GetLogicalChildrenCore(parent, false);
        }
#endif
        internal static IEnumerable<DependencyObject> GetVisualParentsCore(DependencyObject child, DependencyObject stopNode, bool includeStartNode) {
            var result = LinqExtensions.Unfold(child, x => x != stopNode ? GetParent(x) : null, x => x == null);
            return includeStartNode ? result : result.Skip(1);
        }

        internal static IEnumerable<DependencyObject> GetVisualChildrenCore(DependencyObject parent, bool includeStartNode, Func<DependencyObject, bool> skipChildren = null) {
            return GetChildrenCore(
                parent, 
                includeStartNode, 
                x => Enumerable.Range(0, x != null ? VisualTreeHelper.GetChildrenCount(x) : 0).Select(index => VisualTreeHelper.GetChild(x, index)), 
                skipChildren);
        }
#if !NETFX_CORE
        internal static IEnumerable<DependencyObject> GetLogicalChildrenCore(DependencyObject parent, bool includeStartNode, Func<DependencyObject, bool> skipChildren = null) {
            return GetChildrenCore(
                parent, 
                includeStartNode, 
                x => LogicalTreeHelper.GetChildren(x).OfType<DependencyObject>(), 
                skipChildren);
        }
#endif
        static IEnumerable<DependencyObject> GetChildrenCore(DependencyObject parent, bool includeStartNode, Func<DependencyObject, IEnumerable<DependencyObject>> getChildren, Func<DependencyObject, bool> skipChildren = null) {
            var result = parent
                .Yield()
                .Flatten(x => skipChildren != null && skipChildren(x) ? Enumerable.Empty<DependencyObject>() : getChildren(x));
            return includeStartNode ? result : result.Skip(1);
        }
    }
}
