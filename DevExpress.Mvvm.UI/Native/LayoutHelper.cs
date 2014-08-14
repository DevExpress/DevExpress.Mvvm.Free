using System;
using System.Collections;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Media;
using System.Collections.Generic;
using System.ComponentModel;

namespace DevExpress.Mvvm.UI.Native {
    public static class LayoutHelper {

#if !SILVERLIGHT
        public static UIElement GetTopContainerWithAdornerLayer(UIElement element) {
            FrameworkElement fElement = element as FrameworkElement;
            if(fElement != null && GetParent(element) == null) {
                return FindElement(fElement, (e) => (AdornerLayer.GetAdornerLayer(e) != null));
            }
            DependencyObject currentObject = element;
            UIElement topContainer = null;
            while(currentObject != null) {
                UIElement currentUIElement = currentObject as UIElement;
                if(currentUIElement != null && AdornerLayer.GetAdornerLayer(currentUIElement) != null)
                    topContainer = (UIElement)currentObject;
                currentObject = VisualTreeHelper.GetParent(currentObject);
            }
            return topContainer;
        }
        static bool CheckIsDesignTimeRoot(DependencyObject d) {
            FrameworkElement elem = d as FrameworkElement;
            if(elem != null) {
                elem = VisualTreeHelper.GetParent(d) as FrameworkElement;
                if(elem != null) {
                    elem = elem.TemplatedParent as FrameworkElement;
                    if(elem != null && (elem.GetType().Name.Contains("DesignTimeWindow") || elem.GetType().Name.Contains("WindowInstance"))) return true;
                }
            }
            return false;
        }

        public static Size MeasureElementWithSingleChild(FrameworkElement element, Size constraint) {
            FrameworkElement child = (VisualTreeHelper.GetChildrenCount(element) > 0) ? (VisualTreeHelper.GetChild(element, 0) as FrameworkElement) : null;
            if(child != null) {
                child.Measure(constraint);
                return child.DesiredSize;
            }
            return new Size();
        }
        public static Size ArrangeElementWithSingleChild(FrameworkElement element, Size arrangeSize, Point position) {
            FrameworkElement child = (VisualTreeHelper.GetChildrenCount(element) > 0) ? (VisualTreeHelper.GetChild(element, 0) as FrameworkElement) : null;
            if(child != null)
                child.Arrange(new Rect(position, arrangeSize));
            return arrangeSize;
        }
        public static Size ArrangeElementWithSingleChild(FrameworkElement element, Size arrangeSize) {
            return ArrangeElementWithSingleChild(element, arrangeSize, new Point(0, 0));
        }
#else
        public static DependencyObject GetNearestParent(DependencyObject o, bool visualTreeOnly = false) {
            DependencyObject result = null;
            try {
                result = VisualTreeHelper.GetParent(o);
                if(!visualTreeOnly && result == null && o is FrameworkElement)
                    result = (o as FrameworkElement).Parent;
            } catch {
            }
            return result;
        }

        public static bool IsInVisualTree(DependencyObject o) {
            DependencyObject root = FindRoot(o);
            return Application.Current.RootVisual != null && root == Application.Current.RootVisual || (root is Popup && ((Popup)root).IsOpen);
        }
        public static DependencyObject FindVisualRoot(DependencyObject d) {
            DependencyObject current = d;
            while(GetVisualParent(current) != null)
                current = GetVisualParent(current);
            return current;
        }
        public static DependencyObject GetVisualParent(DependencyObject o) {
            return GetNearestParent(o, true);
        }
        public static List<DependencyObject> GetParents(DependencyObject node) {
            List<DependencyObject> res = new List<DependencyObject>();
            node = VisualTreeHelper.GetParent(node);
            while(node != null) {
                res.Add(node);
                node = VisualTreeHelper.GetParent(node);
            }
            return res;
        }
        public static List<DependencyObject> GetFrameworkElementParents(DependencyObject node) {
            List<DependencyObject> res = new List<DependencyObject>();
            FrameworkElement frameworkElement = node as FrameworkElement;
            if(frameworkElement != null)
                frameworkElement = frameworkElement.Parent as FrameworkElement;
            while(frameworkElement != null) {
                res.Add(frameworkElement);
                frameworkElement = frameworkElement.Parent as FrameworkElement;
            }
            return res;
        }
#endif
#if !SILVERLIGHT
        public static FrameworkElement GetTopLevelVisual(DependencyObject d) {
            FrameworkElement topElement = d as FrameworkElement;
            while(d != null) {
                d = VisualTreeHelper.GetParent(d);
                if(d is FrameworkElement)
                    topElement = d as FrameworkElement;
            }
            return topElement;
        }
#else
        public static DependencyObject GetTopLevelVisual(DependencyObject d) {
            return FindRoot(d);
        }
#endif

        public static T FindAmongParents<T>(DependencyObject o, DependencyObject stopObject) where T : DependencyObject {
            while(!(o == null || o is T || o == stopObject)) {
                o = GetParent(o);
            }
            return o as T;
        }

        public static IEnumerable GetRootPath(DependencyObject root, DependencyObject element) {
            DependencyObject parent = element;
            while(parent != null) {
                yield return parent;
                if(parent == root)
                    break;
                parent = GetParent(parent);
            }
        }
        public static FrameworkElement GetRoot(FrameworkElement element) {
            return FindRoot(element) as FrameworkElement;
        }
        public static DependencyObject FindRoot(DependencyObject d) {
            DependencyObject current = d;
            while(GetParent(current) != null)
                current = GetParent(current);
            return current;
        }
        public static DependencyObject GetParent(DependencyObject d, bool uselogicalTree = false) {
#if !SILVERLIGHT
            if(DesignerProperties.GetIsInDesignMode(d)) {
                if(CheckIsDesignTimeRoot(d)) return null;
            }
#endif
            return GetParentCore(d, uselogicalTree);
        }
        static DependencyObject GetParentCore(DependencyObject d, bool uselogicalTree = false) {
#if !SILVERLIGHT
            DependencyObject parent = LogicalTreeHelper.GetParent(d);
            if(!uselogicalTree || parent == null)
                if(d is Visual) parent = VisualTreeHelper.GetParent(d);
            return parent;
#else
            return GetNearestParent(d);
#endif
        }

        public static T FindParentObject<T>(DependencyObject child) where T : class {
            while(child != null) {
                if(child is T)
                    return child as T;
#if !SILVERLIGHT
                child = VisualTreeHelper.GetParent(child);
#else
                child = GetParent(child);
#endif
            }
            return null;
        }
        public static T FindLayoutOrVisualParentObject<T>(DependencyObject child, bool useLogicalTree = false) where T : class {
            return FindLayoutOrVisualParentObject(child, typeof(T), useLogicalTree) as T;
        }
        public static DependencyObject FindLayoutOrVisualParentObject(DependencyObject child, Predicate<DependencyObject> predicate, bool useLogicalTree = false) {
            return FindLayoutOrVisualParentObjectCore(child, predicate, useLogicalTree);
        }
        public static DependencyObject FindLayoutOrVisualParentObject(DependencyObject child, Type parentType, bool useLogicalTree = false) {
            return FindLayoutOrVisualParentObjectCore(child, element => parentType.IsAssignableFrom(element.GetType()), useLogicalTree);
        }
        static DependencyObject FindLayoutOrVisualParentObjectCore(DependencyObject child, Predicate<DependencyObject> predicate, bool useLogicalTree) {
            while(child != null) {
                if(predicate(child))
                    return child;
                child = GetParent(child, useLogicalTree);
            }
            return null;
        }

        public static FrameworkElement FindElement(FrameworkElement treeRoot, Predicate<FrameworkElement> predicate) {
            VisualTreeEnumerator en = new VisualTreeEnumerator(treeRoot);
            while(en.MoveNext()) {
                FrameworkElement element = en.Current as FrameworkElement;
                if(element != null && predicate(element))
                    return element;
            }
            return null;
        }
        public static FrameworkElement FindElementByName(FrameworkElement treeRoot, string name) {
            return FindElement(treeRoot, element => element.Name == name);
        }
        public static FrameworkElement FindElementByType(FrameworkElement treeRoot, Type type) {
            return FindElement(treeRoot, element => element.GetType() == type);
        }
        public static T FindElementByType<T>(FrameworkElement treeRoot) where T: FrameworkElement {
            return (T)FindElementByType(treeRoot, typeof(T));
        }

        public static bool IsChildElement(DependencyObject root, DependencyObject element) {
            DependencyObject parent = element;
            while(parent != null) {
                if(parent == root) return true;
                parent = GetParentCore(parent);
            }
            return false;
        }


        public static void ForEachElement(FrameworkElement treeRoot, ElementHandler elementHandler) {
            VisualTreeEnumerator en = new VisualTreeEnumerator(treeRoot);
            while(en.MoveNext()) {
                FrameworkElement element = en.Current as FrameworkElement;
                if(element != null)
                    elementHandler(element);
            }
        }
        public static bool IsPointInsideElementBounds(Point position, FrameworkElement element, Thickness margin) {
            Rect rect = new Rect(-margin.Left, -margin.Top, element.ActualWidth + margin.Right + margin.Left, element.ActualHeight + margin.Bottom + margin.Top);
            return rect.Contains(position);
        }

#if !SILVERLIGHT
        public static bool IsVisibleInTree(UIElement element, bool visualTreeOnly = false) {
            return element.IsVisible;
        }
#else
        public static bool IsVisibleInTree(UIElement element, bool visualTreeOnly = false) {
            DependencyObject node = element;
            UIElement rootVisual = Application.Current.RootVisual;
            UIElement uiElem = node as UIElement;
            while(node != null) {
                uiElem = node as UIElement;
                if(uiElem != null && uiElem.Visibility != Visibility.Visible)
                    return false;
                if(uiElem == rootVisual)
                    return true;
                node = GetNearestParent(node, visualTreeOnly);
            }
            return IsPopupVisible(uiElem, visualTreeOnly);
        }
        static bool IsPopupVisible(UIElement element, bool visualTreeOnly) {
            DependencyObject parent = visualTreeOnly ? GetNearestParent(element) : element;
            if(parent is Popup) {
                return ((Popup)parent).IsOpen;
            }
            return false;
        }
#endif

        public static bool IsElementLoaded(FrameworkElement element) {
#if !SILVERLIGHT
            return element.IsLoaded;
#else
            if(element.Parent != null) return true;
            if(VisualTreeHelper.GetParent(element) != null) return true;
            Application application = Application.Current;
            if(application == null) return false;
            UIElement rootVisual = application.RootVisual;
            return element == rootVisual;
#endif
        }

#if !SILVERLIGHT && !DESIGN
        public static Rect GetScreenRect(FrameworkElement element) {
            if(element is Window) {
                Window elementWindow = (Window)element;
                if(elementWindow.WindowStyle == WindowStyle.None)
                    GetScreenRectCore(elementWindow, elementWindow);
                else {
                    if(elementWindow.WindowState == WindowState.Maximized) {
                        var screen = System.Windows.Forms.Screen.FromRectangle(new System.Drawing.Rectangle(
                            (int)elementWindow.Left, (int)elementWindow.Top, (int)elementWindow.ActualWidth, (int)elementWindow.ActualHeight));
                        var workingArea = screen.WorkingArea;
                        var leftTop = new Point(workingArea.Location.X, workingArea.Location.Y);
                        var size = new Size(workingArea.Size.Width, workingArea.Size.Height);
                        return new Rect(leftTop, size);
                    } else {
                        var leftTop = new Point(elementWindow.Left, elementWindow.Top);
                        var presentationSource = PresentationSource.FromVisual(elementWindow);
                        if(presentationSource != null) {
                            double dpiX = 96.0 * presentationSource.CompositionTarget.TransformToDevice.M11;
                            double dpiY = 96.0 * presentationSource.CompositionTarget.TransformToDevice.M22;
                            leftTop = new Point(leftTop.X * 96.0 / dpiX, leftTop.Y * 96.0 / dpiY);
                        }
                        return new Rect(leftTop, new Size(element.ActualWidth, element.ActualHeight));
                    }
                }
            }
            return GetScreenRectCore(Window.GetWindow(element), element);
        }
        static Rect GetScreenRectCore(Window window, FrameworkElement element) {
            var leftTop = element.PointToScreen(new Point());
            var presentationSource = window == null ? null : PresentationSource.FromVisual(window);
            if(presentationSource != null) {
                double dpiX = 96.0 * presentationSource.CompositionTarget.TransformToDevice.M11;
                double dpiY = 96.0 * presentationSource.CompositionTarget.TransformToDevice.M22;
                leftTop = new Point(leftTop.X * 96.0 / dpiX, leftTop.Y * 96.0 / dpiY);
            }
            return new Rect(leftTop, new Size(element.ActualWidth, element.ActualHeight));
        }
#endif

        public delegate void ElementHandler(FrameworkElement e);
    }
}