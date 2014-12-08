using System.Collections.Generic;
using System.Windows;
#if !FREE && !NETFX_CORE
using DevExpress.Mvvm.UI.Native;
#endif
using System;
#if NETFX_CORE
using DevExpress.Mvvm.Native;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;
#else
using System.Windows.Media;
using System.Windows.Media.Media3D;
#endif

namespace DevExpress.Mvvm.UI {
    public static class LayoutTreeHelper {
        static DependencyObject GetParent(DependencyObject element) {
#if !SILVERLIGHT && !NETFX_CORE
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
        internal static IEnumerable<DependencyObject> GetVisualParentsCore(DependencyObject child, DependencyObject stopNode, bool includeStartNode) {
            if(includeStartNode)
                yield return child;
            DependencyObject parent = GetParent(child);
            bool isStopNode = false;
            bool checkStopNode = stopNode != null;
            while(parent != null && !isStopNode) {
                yield return parent;
                parent = GetParent(parent);
                if(checkStopNode && parent == stopNode) {
                    isStopNode = true;
                    yield return parent;
                }
            }
        }
        internal static IEnumerable<DependencyObject> GetVisualChildrenCore(DependencyObject parent, bool includeStartNode) {
            if(includeStartNode)
                yield return parent;
            for(int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++) {
                foreach(DependencyObject subChild in GetVisualChildrenCore(VisualTreeHelper.GetChild(parent, i), true)) {
                    yield return subChild;
                }
            }
        }
    }
}