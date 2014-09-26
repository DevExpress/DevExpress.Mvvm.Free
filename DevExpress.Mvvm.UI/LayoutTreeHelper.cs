using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace DevExpress.Mvvm.UI {
    public static class LayoutTreeHelper {
        static DependencyObject GetVisualParent(DependencyObject element) {
#if !SILVERLIGHT
            return (element is Visual || element is Visual3D) ? VisualTreeHelper.GetParent(element) : null;
#else
            return VisualTreeHelper.GetParent(element);
#endif
        }


        public static IEnumerable<DependencyObject> GetVisualParents(DependencyObject child, DependencyObject stopNode = null) {
            foreach(DependencyObject parent in GetVisualParentsCore(child, stopNode, false))
                yield return parent;
        }
        public static IEnumerable<DependencyObject> GetVisualChildren(DependencyObject parent) {
            foreach(DependencyObject child in GetVisualChildrenCore(parent, false))
                yield return child;
        }
        internal static IEnumerable<DependencyObject> GetVisualParentsCore(DependencyObject child, DependencyObject stopNode, bool includeStartNode) {
            if(includeStartNode)
                yield return child;
            DependencyObject parent = GetVisualParent(child);
            bool isStopSearchNode = false;
            while(parent != null && !isStopSearchNode) {
                yield return parent;
                parent = GetVisualParent(parent);
                if(parent == stopNode) {
                    isStopSearchNode = true;
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