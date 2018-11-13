using System;
using System.Collections;
using System.Windows;
using System.Collections.Generic;
using System.ComponentModel;
#if !NETFX_CORE
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Media;
#else
using Windows.UI.Xaml;
using Windows.Foundation;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Controls.Primitives;
using DevExpress.Mvvm.Native;
#if !FREE
using DevExpress.Xpf.Core.Native;
#endif
#endif
#if DXWINDOW
namespace DevExpress.Internal.DXWindow {
    public static class LayoutHelper {
#elif DESIGN
namespace DevExpress.Design.UI {
    public static class WpfLayoutHelper {
#elif MVVM || NETFX_CORE
namespace DevExpress.Mvvm.UI.Native {
    public static class LayoutHelper {
#else
namespace DevExpress.Xpf.Core.Native {
    public static class LayoutHelper {
#endif

#if !NETFX_CORE
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
#if !DESIGN && !MVVM
        public static UIElement HitTest(UIElement element, Point point) {
            UIElement result = null;
            HitTestFilterCallback filterCallback = e => {
                UIElement uiElement = e as UIElement;
                return uiElement == null || uiElement.IsVisible ? HitTestFilterBehavior.Continue : HitTestFilterBehavior.ContinueSkipSelfAndChildren;
            };
            HitTestResultCallback resultCallback = e => {
                result = e.VisualHit as UIElement;
                return result != null ? HitTestResultBehavior.Stop : HitTestResultBehavior.Continue;
            };
            VisualTreeHelper.HitTest(element, filterCallback, resultCallback, new PointHitTestParameters(point));
            return result;
        }
#endif
#if !DESIGN
        public static Rect GetScreenRect(FrameworkElement element) {
#if !FREE
            if(element is DXWindow) {
                DXWindow elementDXWindow = (DXWindow)element;
                var windowChild = VisualTreeHelper.GetChild(elementDXWindow, 0) as FrameworkElement;
                return GetScreenRectCore(elementDXWindow, windowChild ?? elementDXWindow);
            }
#endif
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
#if !NETFX_CORE && !FREE
                        return GetStandardWindowRect(elementWindow);
#else
                        return new Rect(new Point(elementWindow.Left, elementWindow.Top), new Size(elementWindow.Width, elementWindow.Height));
#endif
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
#if FREE
            if(element.FlowDirection == FlowDirection.RightToLeft && (element is Window)) {
#else 
            if(element.FlowDirection == FlowDirection.RightToLeft && (element is DXWindow || !(element is Window))) {
#endif
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

#if !NETFX_CORE && !FREE
        static Rect GetStandardWindowRect(Window window) {
            if(window is ThemedWindow)
                return new Rect(new Point(window.Left, window.Top), new Size(window.Width, window.Height));
            Rect rect;
            if (NativeMethods.GetWindowRect_DwmGetWindowAttribute(new System.Windows.Interop.WindowInteropHelper(window).Handle, out rect))
                return ScreenHelper.GetScaledRect(rect);
            return new Rect(new Point(window.Left, window.Top), new Size(window.Width, window.Height));
        }
#endif
#endif
#if !FREE
        public static Rect GetRelativeElementRect(UIElement element, UIElement parent) {
            GeneralTransform transform = element.TransformToVisual(parent);
            return transform.TransformBounds(new Rect(element.RenderSize));
        }
        public static bool IsChildElementEx(DependencyObject root, DependencyObject element, bool useLogicalTree = false) {
            DependencyObject parent = element;
            while(parent != null) {
                if(parent == root)
                    return true;
                if(parent is ContextMenu)
                    parent = ((ContextMenu)parent).PlacementTarget;
                else {
                    var popup = parent as Popup;
                    if(popup != null && popup.PlacementTarget != null)
                        parent = popup.PlacementTarget;
                    else
                        parent = GetParentCore(parent, useLogicalTree);
                }
            }
            return false;
        }
#endif

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
#else
        public static bool IsInVisualTree(DependencyObject o) {
            //TODO: doesn`t work for content of Resource or attached DependencyObjectCollection
            DependencyObject current = o;
            DependencyObject node = GetParent(o, true);
            Popup popup;
            while(node != null) {
                popup = node as Popup;
                if(popup != null && !popup.IsOpen)
                    return false;

                current = node;
                node = GetParent(node, true);
            }
            popup = current as Popup;
            if(popup != null)
                return popup.IsOpen;

            return current == Window.Current.Content || current == FindRoot(Window.Current.Content as FrameworkElement, false);
        }
        public static T FindParentObject<T>(DependencyObject child, Predicate<T> isSearchTarget = null, bool useLogicalTree = false) where T : class {
            while(child != null) {
                T currentItem = child as T;
                if(currentItem != null && (isSearchTarget == null || isSearchTarget(currentItem)))
                    return currentItem;
                child = GetParent(child, useLogicalTree);
            }
            return null;
        }
        public static Rect GetRelativeElementRect(UIElement element, UIElement parent) {
            GeneralTransform transform = element.TransformToVisual(parent);
            return transform.TransformBounds(new Rect(new Point(), element.RenderSize));
        }
        public static T FindAmongLogicalParents<T>(DependencyObject o, DependencyObject stopObject) where T : DependencyObject {
            while(!(o == null || o is T || o == stopObject)) {
                o = GetLogicalParent(o);
            }
            return o as T;
        }
        public static DependencyObject GetLogicalParent(DependencyObject o) {
            DependencyObject result = null;
            try { // !!! Do not remove. Sometimes an exception is raised if o is not in visual tree
                if(o is FrameworkElement)
                    result = (o as FrameworkElement).Parent;
                if(result == null)
                    result = VisualTreeHelper.GetParent(o);
            } catch {
            }
            return result;
        }
        public static T FindElementByName<T>(FrameworkElement treeRoot, string name) where T : FrameworkElement {
            Type type = typeof(T);
            return (T)FindElement(treeRoot, element => type.IsAssignableFrom(element.GetType()) && element.Name == name);
        }
        public static bool IsChildElement(DependencyObject root, DependencyObject element, Func<DependencyObject, DependencyObject> getParent) {
            DependencyObject parent = element;
            while(parent != null) {
                if(parent == root)
                    return true;
                parent = getParent(parent);
            }
            return false;
        }
        public static bool IsPopupRoot(DependencyObject root) {
            bool result = false;
            var rootCanvas = root as Canvas;
            if(rootCanvas != null && rootCanvas.Parent == null && VisualTreeHelper.GetParent(rootCanvas) == null) {
                foreach(var child in rootCanvas.Children) {
                    result = (child as FrameworkElement).With(x => x.Parent) is Popup && VisualTreeHelper.GetParent(child) == rootCanvas;
                    if(result)
                        break;
                }
            }

            return result;
        }

        static DependencyObject GetParentCore(DependencyObject o, bool useLogicalTree = false) {
            DependencyObject result = null;
            try { // !!! Do not remove. Sometimes an exception is raised if o is not in visual tree
                result = VisualTreeHelper.GetParent(o);
                if(useLogicalTree) {
                    DependencyObject logicalParent = (o as FrameworkElement).With(x => x.Parent);
                    //Win10 magic
                    if(result != logicalParent && logicalParent is Popup)
                        result = logicalParent;
                    if(result == null)
                        result = logicalParent;
                }
            } catch { }

            return result;
        }
        static bool IsVisibleInTreeCore(UIElement element, bool visualTreeOnly = false) {
            DependencyObject node = element;
            UIElement rootVisual = Window.Current.With(x => x.Content);
            UIElement uiElem = node as UIElement;
            Popup popup = null;
            while(node != null) {
                uiElem = node as UIElement;
                popup = node as Popup;
                if((uiElem != null && uiElem.Visibility != Visibility.Visible) || (popup != null && !popup.IsOpen))
                    return false;
                if(uiElem == rootVisual)
                    return true;

                node = GetParentCore(node, true);
            }
            return popup != null && popup.IsOpen;
        }
        static bool IsElementLoadedCore(FrameworkElement element) {
            if(element.Parent != null || VisualTreeHelper.GetParent(element) != null)
                return true;

            UIElement rootVisual = Window.Current.With(x => x.Content);
            return element == rootVisual;
        }

        static TElement FindElementCore<TElement>(DependencyObject treeRoot, Predicate<FrameworkElement> predicate, Predicate<DependencyObject> skipTreeNode)
            where TElement : FrameworkElement {
            VisualTreeEnumeratorWithConditionalStop en = new VisualTreeEnumeratorWithConditionalStop(treeRoot, skipTreeNode);
            var searchType = typeof(TElement);
            while(en.MoveNext()) {
                DependencyObject element = en.Current;
                FrameworkElement fElement = element as FrameworkElement;
                if(element != null && searchType.IsAssignableFrom(element.GetType()) && fElement != null && (predicate == null || predicate(fElement)))
                    return element as TElement;
            }
            return null;
        }
        public static FrameworkElement FindElement(FrameworkElement treeRoot, Predicate<FrameworkElement> predicate, Predicate<DependencyObject> skipTreeNode) {
            if(skipTreeNode != null)
                return FindElementCore<FrameworkElement>(treeRoot, predicate, skipTreeNode);
            else
                return FindElement(treeRoot, predicate);
        }
        public static T FindElementByName<T>(FrameworkElement treeRoot, string name, Predicate<DependencyObject> skipTreeNode) where T : FrameworkElement {
            if(skipTreeNode != null)
                return FindElementCore<T>(treeRoot, element => element.Name == name, skipTreeNode);
            else
                return FindElementByName<T>(treeRoot, name);
        }
        public static T FindElementByType<T>(FrameworkElement treeRoot, Predicate<DependencyObject> skipTreeNode) where T : FrameworkElement {
            if(skipTreeNode != null)
                return FindElementCore<T>(treeRoot, null, skipTreeNode);
            else
                return FindElementByType<T>(treeRoot);
        }
        public static Point GetScreenPosition(UIElement element, Point elementPosition) {
            if(element == null)
                return new Point();

            var root = AppHelper.RootVisual ?? FindRoot(element, true) as UIElement;
            var transform = element.TransformToVisual(root);
            return transform.TransformPoint(elementPosition);
        }
        public static Point GetRelativePosition(UIElement element, Point screenPosition) {
            if(element == null)
                return new Point();

            var root = AppHelper.RootVisual ?? FindRoot(element, true) as UIElement;
            var transform = root.TransformToVisual(element);
            return transform.TransformPoint(screenPosition);
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
#if !NETFX_CORE
            if(DesignerProperties.GetIsInDesignMode(d)) {
                if(CheckIsDesignTimeRoot(d)) return null;
            }
#endif
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
#if NETFX_CORE
            return FindElement(treeRoot, element => type.IsAssignableFrom(element.GetType()));
#else
            return FindElement(treeRoot, element => element.GetType() == type);
#endif
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
