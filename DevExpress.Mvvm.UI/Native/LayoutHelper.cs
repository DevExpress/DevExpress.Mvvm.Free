using System;
using System.Collections;
using System.Windows;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Media;
namespace DevExpress.Mvvm.UI.Native {
    public static class LayoutHelper {

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
        public static T FindParentObject<T>(DependencyObject child) where T : class {
            while(child != null) {
                if(child is T)
                    return child as T;
                child = VisualTreeHelper.GetParent(child);
            }
            return null;
        }
        public static FrameworkElement GetTopLevelVisual(DependencyObject d) {
            FrameworkElement topElement = d as FrameworkElement;
            while(d != null) {
                d = VisualTreeHelper.GetParent(d);
                if(d is FrameworkElement)
                    topElement = d as FrameworkElement;
            }
            return topElement;
        }
        public static FrameworkElement GetRoot(FrameworkElement element) {
            return FindRoot(element) as FrameworkElement;
        }
        public static Rect GetScreenRect(FrameworkElement element) {
            if(element is Window) {
                Window elementWindow = (Window)element;
                if(elementWindow.WindowStyle == WindowStyle.None)
                    return GetScreenRectCore(elementWindow, elementWindow);
                else {
                    if (elementWindow.WindowState == WindowState.Maximized) {
                        var screen = System.Windows.Forms.Screen.FromHandle(new System.Windows.Interop.WindowInteropHelper(elementWindow).Handle);
                        var workingArea = screen.WorkingArea;
                        var leftTop = new Point(workingArea.Location.X, workingArea.Location.Y);
                        var size = new Size(workingArea.Size.Width, workingArea.Size.Height);
                        var presentationSource = PresentationSource.FromVisual(elementWindow);
                        if (presentationSource != null) {
                            leftTop = new Point(leftTop.X / presentationSource.CompositionTarget.TransformToDevice.M11, leftTop.Y / presentationSource.CompositionTarget.TransformToDevice.M22);
                            size = new Size(size.Width / presentationSource.CompositionTarget.TransformToDevice.M11, size.Height / presentationSource.CompositionTarget.TransformToDevice.M22);
                        }
                        return new Rect(leftTop, size);
                    } else {
                        return new Rect(new Point(elementWindow.Left, elementWindow.Top), new Size(elementWindow.Width, elementWindow.Height));
                    }
                }
            }
            if(element == null) {
                var screen = System.Windows.Forms.Screen.PrimaryScreen;
                return new Rect(new Point(), new Size(screen.Bounds.Width, screen.Bounds.Height));
            }
            return GetScreenRectCore(Window.GetWindow(element), element);
        }
        static Rect GetScreenRectCore(Window window, FrameworkElement element) {
            Point leftTop;
            Point rightBottom;
            if(element.FlowDirection == FlowDirection.RightToLeft && (element is Window)) {
                leftTop = element.PointToScreen(new Point(element.ActualWidth, 0));
                rightBottom = element.PointToScreen(new Point(0, element.ActualHeight));
            } else {
                leftTop = element.PointToScreen(new Point());
                rightBottom = element.PointToScreen(new Point(element.ActualWidth, element.ActualHeight));
            }
            var presentationSource = window == null ? null : PresentationSource.FromVisual(window);
            if(presentationSource != null) {
                double dpiX = presentationSource.CompositionTarget.TransformToDevice.M11;
                double dpiY = presentationSource.CompositionTarget.TransformToDevice.M22;
                leftTop = new Point(leftTop.X / dpiX, leftTop.Y / dpiY);
                rightBottom = new Point(rightBottom.X / dpiX, rightBottom.Y / dpiY);
            }
            return new Rect(leftTop, rightBottom);
        }


        static DependencyObject GetParentCore(DependencyObject d, bool useLogicalTree = false) {
            DependencyObject parent = LogicalTreeHelper.GetParent(d);
            if(!useLogicalTree || parent == null)
                if(d is Visual) parent = VisualTreeHelper.GetParent(d);
            return parent;
        }
        static bool IsVisibleInTreeCore(UIElement element, bool visualTreeOnly = false) {
            return element.IsVisible;
        }
        static bool IsElementLoadedCore(FrameworkElement element) {
            return element.IsLoaded;
        }

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
        public static DependencyObject FindRoot(DependencyObject d, bool useLogicalTree = false) {
            DependencyObject result = d;
            DependencyObject current = GetParent(d, useLogicalTree);
            while(current != null) {
                result = current;
                current = GetParent(current, useLogicalTree);
            }
            return result;
        }
        public static DependencyObject GetParent(DependencyObject d, bool useLogicalTree = false) {
            if(DesignerProperties.GetIsInDesignMode(d)) {
                if(CheckIsDesignTimeRoot(d)) return null;
            }
            return GetParentCore(d, useLogicalTree);
        }
        public static T FindLayoutOrVisualParentObject<T>(DependencyObject child, bool useLogicalTree = false, DependencyObject stopSearchNode = null) where T : class {
            return FindLayoutOrVisualParentObject(child, typeof(T), useLogicalTree, stopSearchNode) as T;
        }
        public static DependencyObject FindLayoutOrVisualParentObject(DependencyObject child, Predicate<DependencyObject> predicate, bool useLogicalTree = false, DependencyObject stopSearchNode = null) {
            return FindLayoutOrVisualParentObjectCore(child, predicate, useLogicalTree, stopSearchNode);
        }
        public static DependencyObject FindLayoutOrVisualParentObject(DependencyObject child, Type parentType, bool useLogicalTree = false, DependencyObject stopSearchNode = null) {
            return FindLayoutOrVisualParentObjectCore(child, element => parentType.IsAssignableFrom(element.GetType()), useLogicalTree, stopSearchNode);
        }
        static DependencyObject FindLayoutOrVisualParentObjectCore(DependencyObject child, Predicate<DependencyObject> predicate, bool useLogicalTree, DependencyObject stopSearchNode = null) {
            while(child != null) {
                if(child == stopSearchNode)
                    break;
                if(predicate(child))
                    return child;
                child = GetParent(child, useLogicalTree);
            }
            return null;
        }

        internal static DependencyObject FindElementCore(DependencyObject treeRoot, Predicate<DependencyObject> predicate) {
            VisualTreeEnumerator en = new VisualTreeEnumerator(treeRoot);
            while(en.MoveNext()) {
                DependencyObject element = en.Current;
                if(element != null && predicate(element))
                    return element;
            }
            return null;
        }
        public static FrameworkElement FindElement(FrameworkElement treeRoot, Predicate<FrameworkElement> predicate) {
            return FindElementCore(treeRoot, x => {
                if(x is FrameworkElement)
                    return predicate((FrameworkElement)x);
                return false;
            }) as FrameworkElement;
        }
        public static FrameworkElement FindElementByName(FrameworkElement treeRoot, string name) {
            return FindElement(treeRoot, element => element.Name == name);
        }
        public static FrameworkElement FindElementByType(FrameworkElement treeRoot, Type type) {
            return FindElement(treeRoot, element => element.GetType() == type);
        }
        public static T FindElementByType<T>(FrameworkElement treeRoot) where T : FrameworkElement {
            return (T)FindElementByType(treeRoot, typeof(T));
        }

        public static bool IsChildElement(DependencyObject root, DependencyObject element) {
            return IsChildElement(root, element, false);
        }
        public static bool IsChildElement(DependencyObject root, DependencyObject element, bool useLogicalTree) {
            DependencyObject parent = element;
            while(parent != null) {
                if(parent == root)
                    return true;
                parent = GetParentCore(parent, useLogicalTree);
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
        public static bool IsVisibleInTree(UIElement element, bool visualTreeOnly = false) {
            return IsVisibleInTreeCore(element, visualTreeOnly);
        }
        public static bool IsElementLoaded(FrameworkElement element) {
            return IsElementLoadedCore(element);
        }

        public delegate void ElementHandler(FrameworkElement e);
    }
}