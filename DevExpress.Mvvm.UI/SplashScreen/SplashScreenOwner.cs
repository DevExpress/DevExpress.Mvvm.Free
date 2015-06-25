using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Security;
using System.Windows;
using System.Windows.Threading;
using DevExpress.Mvvm.Native;
using System.Windows.Interop;

using DevExpress.Mvvm.UI.Native;

namespace DevExpress.Mvvm.UI {

    public enum SplashScreenLocation {
        CenterContainer,
        CenterWindow
    }
    public enum SplashScreenLock {
        None,
        InputOnly,
        Full,
        LoadingContent
    }
    public enum SplashScreenClosingMode {
        Default,
        OnParentClosed,
        ManualOnly
    }
    enum AsyncInvokeMode {
        AllowSyncInvoke,
        AsyncOnly
    }
    enum SplashScreenArrangeMode {
        Default,
        ArrangeOnStartupOnly,
        Skip
    }

    public class SplashScreenOwner {
        public DependencyObject Owner { get; private set; }

        public SplashScreenOwner(DependencyObject owner) {
            if(owner == null)
                throw new ArgumentNullException("Owner");

            Owner = owner;
        }

        internal WindowContainer CreateOwnerContainer(WindowStartupLocation splashScreenStartupLocation) {
            WindowContainer result = null;
            if(splashScreenStartupLocation == WindowStartupLocation.CenterOwner)
                result = new WindowArrangerContainer(Owner, SplashScreenLocation.CenterWindow) { ArrangeMode = SplashScreenArrangeMode.ArrangeOnStartupOnly };
            if(result == null || !result.IsInitialized)
                result = new WindowContainer(Owner);

            return result;
        }
    }

    internal class WindowLocker {
        readonly SplashScreenLock lockMode;
        WindowContainer Container { get; set; }

        public WindowLocker(WindowContainer container, SplashScreenLock lockMode) {
            Container = container;
            this.lockMode = lockMode;
            if(!Container.IsInitialized)
                Container.Initialized += OnOwnerInitialized;
            else
                Initialize();
        }

        public void Release(bool activateWindowIfNeeded) {
            if(Container == null)
                return;

            Container.Initialized -= OnOwnerInitialized;
            var container = Container;
            Container = null;
            if(container.Window == null || lockMode == SplashScreenLock.None)
                return;

            SplashScreenHelper.InvokeAsync(container.Window, () => {
                if(activateWindowIfNeeded && !SplashScreenHelper.ApplicationHasActiveWindow())
                    container.ActivateWindow();

                SplashScreenHelper.UnlockWindow(container);
            }, DispatcherPriority.Render);
        }

        void OnOwnerInitialized(object sender, EventArgs e) {
            ((WindowContainer)sender).Initialized -= OnOwnerInitialized;
            Initialize();
        }
        void Initialize() {
            if(Container != null)
                SplashScreenHelper.InvokeAsync(Container.Window, () => SplashScreenHelper.LockWindow(Container, lockMode), DispatcherPriority.Send, AsyncInvokeMode.AllowSyncInvoke);
        }

    }
    internal class WindowArranger : WindowRelationInfo {
        new public WindowArrangerContainer Parent { get { return base.Parent as WindowArrangerContainer; } }
        FrameworkElement ParentContainer { get { return Parent.WindowObject as FrameworkElement; } }
        SplashScreenLocation childLocation;
        Rect lastChildPos = Rect.Empty;
        Rect lastParentPos = Rect.Empty;
        Rect nextParentPos = Rect.Empty;
        bool isParentClosed;
        SplashScreenArrangeMode arrangeMode;
        bool SkipArrange { get { return IsReleased || arrangeMode == SplashScreenArrangeMode.Skip; } }
        bool IsArrangeValid { get { return nextParentPos == lastParentPos && lastChildPos.Width == Child.Window.ActualWidth && lastChildPos.Height == Child.Window.ActualHeight; } }

        protected internal WindowArranger(WindowArrangerContainer parent, SplashScreenLocation childLocation, SplashScreenArrangeMode arrangeMode)
            : base(parent) {
            this.childLocation = childLocation;
            this.arrangeMode = arrangeMode;
        }

        protected override void ChildAttachedOverride() {
            Child.Window.WindowStartupLocation = WindowStartupLocation.Manual;
        }
        protected override void CompleteInitializationOverride() {
            if(Child.Window.Dispatcher.CheckAccess()) {
                nextParentPos = childLocation == SplashScreenLocation.CenterContainer ? Parent.ControlStartupPosition : Parent.WindowStartupPosition;
                UpdateChildPosition();
            }
            SplashScreenHelper.InvokeAsync(Parent.Window, () => UpdateNextParentRectAndChildPosition(true), DispatcherPriority.Normal, AsyncInvokeMode.AllowSyncInvoke);
        }
        protected override void SubscribeChildEventsOverride() {
            Child.Window.SizeChanged += ChildSizeChanged;
            Child.Window.ContentRendered += ChildSizeChanged;
        }
        protected override void UnsubscribeChildEventsOverride() {
            Child.Window.SizeChanged -= ChildSizeChanged;
            Child.Window.ContentRendered -= ChildSizeChanged;
        }
        protected override void SubscribeParentEventsOverride() {
            if(Parent.Window != null) {
                Parent.Window.LocationChanged += OnParentSizeOrPositionChanged;
                Parent.Window.SizeChanged += OnParentSizeOrPositionChanged;
            }
            if(childLocation == SplashScreenLocation.CenterContainer && ParentContainer != null && Parent.Window != ParentContainer) {
                ParentContainer.SizeChanged += OnParentSizeOrPositionChanged;
                ParentContainer.LayoutUpdated += OnParentSizeOrPositionChanged;
            }
        }
        protected override void UnsubscribeParentEventsOverride() {
            if(Parent.Window != null) {
                Parent.Window.LocationChanged -= OnParentSizeOrPositionChanged;
                Parent.Window.SizeChanged -= OnParentSizeOrPositionChanged;
            }
            if(childLocation == SplashScreenLocation.CenterContainer && ParentContainer != null && Parent.Window != ParentContainer) {
                try {
                    ParentContainer.SizeChanged -= OnParentSizeOrPositionChanged;
                    ParentContainer.LayoutUpdated -= OnParentSizeOrPositionChanged;
                } catch { }
            }
        }
        protected override void OnParentClosed(object sender, EventArgs e) {
            isParentClosed = true;
            base.OnParentClosed(sender, e);
        }

        void OnParentSizeOrPositionChanged(object sender, EventArgs e) {
            if(SkipArrange)
                return;

            UpdateNextParentRectAndChildPosition(false);
        }
        void ChildSizeChanged(object sender, EventArgs e) {
            if(SkipArrange)
                return;

            if(!Parent.IsInitialized || Child.Window == null ||
                (lastChildPos.Width == Child.Window.ActualWidth && lastChildPos.Height == Child.Window.ActualHeight))
                return;

            if(lastParentPos.IsEmpty)
                SplashScreenHelper.InvokeAsync(Parent, () => UpdateNextParentRectAndChildPosition(true), DispatcherPriority.Normal, AsyncInvokeMode.AllowSyncInvoke);
            else
                UpdateChildPosition();
        }
        void UpdateNextParentRectAndChildPosition(bool skipSizeCheck) {
            if(SkipArrange || !Parent.IsInitialized)
                return;

            nextParentPos = isParentClosed
                ? Rect.Empty
                : childLocation == SplashScreenLocation.CenterContainer ? Parent.GetControlRect() : Parent.GetWindowRect();
            if(!skipSizeCheck && lastParentPos == nextParentPos || nextParentPos.IsEmpty)
                return;

            SplashScreenHelper.InvokeAsync(Child, () => UpdateChildPosition(), DispatcherPriority.Normal, AsyncInvokeMode.AllowSyncInvoke);
        }
        void UpdateChildPosition() {
            if(SkipArrange || Child == null || !Child.IsInitialized || IsArrangeValid)
                return;
            if(arrangeMode == SplashScreenArrangeMode.ArrangeOnStartupOnly && lastParentPos != Rect.Empty && lastParentPos != nextParentPos) {
                arrangeMode = SplashScreenArrangeMode.Skip;
                return;
            }
            Rect bounds = nextParentPos;
            var window = Child.Window;
            if(!IsZero(window.ActualWidth) && !IsZero(window.ActualHeight)) {
                window.Left = (int)(bounds.X + (bounds.Width - window.ActualWidth) * 0.5);
                window.Top = (int)(bounds.Y + (bounds.Height - window.ActualHeight) * 0.5);
                lastChildPos = new Rect(window.Left, window.Top, window.Width, window.Height);
            }
            lastParentPos = bounds;
        }
        static bool IsZero(double value) {
            return value == 0d || double.IsNaN(value);
        }
    }
    internal class WindowRelationInfo {
        public WindowContainer Parent { get; private set; }
        public WindowContainer Child { get; private set; }
        public bool IsInitialized { get; private set; }
        public bool IsReleased { get; private set; }

        protected internal WindowRelationInfo(WindowContainer parent) {
            if(parent == null)
                throw new ArgumentNullException("Parent");

            Parent = parent;
            CompleteContainerInitialization(Parent);
        }

        public void AttachChild(Window child) {
            if(Child != null)
                throw new ArgumentException("Child property is already set");

            Child = new WindowContainer(child);
            ChildAttachedOverride();
            CompleteContainerInitialization(Child);
        }
        public virtual void Release() {
            if(IsReleased)
                return;

            IsReleased = true;
            UnsubscribeChildEvents();
            UnsubscribeParentEvents();
            Child.Initialized -= OnContainerInitialized;
            Parent.Initialized -= OnContainerInitialized;
            Child = null;
            Parent = null;
        }

        protected virtual void SubscribeParentEventsOverride() { }
        protected virtual void SubscribeChildEventsOverride() { }
        protected virtual void UnsubscribeParentEventsOverride() { }
        protected virtual void UnsubscribeChildEventsOverride() { }
        protected virtual void CompleteInitializationOverride() { }
        protected virtual void ChildAttachedOverride() { }
        protected virtual void OnParentClosed(object sender, EventArgs e) {
            ParentClosed.Do(x => x(this, EventArgs.Empty));
        }

        #region Initialization
        void OnContainerInitialized(object sender, EventArgs e) {
            ((WindowContainer)sender).Initialized -= OnContainerInitialized;
            CompleteContainerInitialization((WindowContainer)sender);
        }
        void CompleteContainerInitialization(WindowContainer container) {
            if(!container.IsInitialized) {
                container.Initialized += OnContainerInitialized;
                return;
            }

            if(container == Parent)
                SubscribeParentEvents();
            else
                SubscribeChildEvents();

            CompleteInitialization();
        }
        void CompleteInitialization() {
            if(IsReleased || IsInitialized || Child == null || Child.Handle == IntPtr.Zero || Parent.Handle == IntPtr.Zero)
                return;

            CompleteInitializationOverride();
            SplashScreenHelper.InvokeAsync(Child, SetChildParent);
            IsInitialized = true;
        }
        void SetChildParent() {
            if(IsReleased)
                return;

            SplashScreenHelper.SetParent(Child.Window, Parent.Handle);
        }
        #endregion

        void SubscribeChildEvents() {
            if(Child == null || Child.Window == null)
                return;

            Child.Window.Closed += OnChildClosed;
            SubscribeChildEventsOverride();
        }
        void UnsubscribeChildEvents() {
            if(Child == null || Child.Window == null)
                return;

            Child.Window.Closed -= OnChildClosed;
            UnsubscribeChildEventsOverride();
        }
        void OnChildClosed(object sender, EventArgs e) {
            UnsubscribeChildEvents();
        }

        void SubscribeParentEvents() {
            if(Parent == null)
                return;

            Parent.Window.Closed += OnParentClosed;
            SubscribeParentEventsOverride();
        }
        void UnsubscribeParentEvents() {
            if(Parent == null || Parent.Window == null)
                return;

            Parent.Window.Closed -= OnParentClosed;
            UnsubscribeParentEventsOverride();
        }

        public event EventHandler ParentClosed;
    }
    internal class WindowArrangerContainer : WindowContainer {
        public Rect ControlStartupPosition { get; private set; }
        public Rect WindowStartupPosition { get; private set; }
        public SplashScreenArrangeMode ArrangeMode { get; set; }
        SplashScreenLocation arrangeLocation;

        public WindowArrangerContainer(DependencyObject parentObject, SplashScreenLocation arrangeLocation)
            : base(parentObject) {
            this.arrangeLocation = arrangeLocation;
        }

        public override WindowRelationInfo CreateOwnerContainer() {
            return new WindowArranger(this, arrangeLocation, ArrangeMode);
        }
        public Rect GetWindowRect() {
            return Window == null || !Window.IsLoaded ? Rect.Empty : LayoutHelper.GetScreenRect(Window);
        }
        public Rect GetControlRect() {
            return !(WindowObject as FrameworkElement).Return(x => x.IsLoaded, () => false) || PresentationSource.FromDependencyObject(WindowObject) == null
                ? Rect.Empty
                : LayoutHelper.GetScreenRect(WindowObject as FrameworkElement);
        }

        protected override void CompleteInitializationOverride() {
            ControlStartupPosition = GetControlRect();
            WindowStartupPosition = GetWindowRect();
        }
    }
    internal class WindowContainer {
        public DependencyObject WindowObject { get; private set; }
        public Window Window { get; private set; }
        public IntPtr Handle { get; private set; }
        public bool IsInitialized { get; private set; }

        public WindowContainer(DependencyObject windowObject) {
            if(windowObject == null)
                throw new ArgumentNullException("WindowObject");

            WindowObject = windowObject;
            Initialize();
        }

        public virtual WindowRelationInfo CreateOwnerContainer() {
            return new WindowRelationInfo(this);
        }
        public void ActivateWindow() {
            SplashScreenHelper.InvokeAsync(this, ActivateWindowCore, DispatcherPriority.Send, AsyncInvokeMode.AllowSyncInvoke);
        }
        protected virtual void CompleteInitializationOverride() { }

        void ActivateWindowCore() {
            if(Window != null && !Window.IsActive && Window.IsVisible && !SplashScreenHelper.ApplicationHasActiveWindow())
                Window.Activate();
        }
        void Initialize() {
            CompleteInitialization();
            if(IsInitialized)
                return;

            if(!(WindowObject as FrameworkElement).Return(x => x.IsLoaded, () => true))
                (WindowObject as FrameworkElement).Loaded += OnControlLoaded;
        }
        void CompleteInitialization() {
            if(IsInitialized)
                return;

            Window window = (WindowObject as Window) ?? Window.GetWindow(WindowObject);
            if(window == null)
                return;

            WindowInteropHelper helper = new WindowInteropHelper(window);
            helper.EnsureHandle();
            Window = window;
            Handle = helper.Handle;
            CompleteInitializationOverride();
            IsInitialized = true;
            Initialized.Do(x => x(this, EventArgs.Empty));
        }
        void OnControlLoaded(object sender, RoutedEventArgs e) {
            (sender as FrameworkElement).Loaded -= OnControlLoaded;
            Initialize();
        }

        public event EventHandler Initialized;
    }

    static class SplashScreenHelper {
        [DllImport("user32.dll")]
        static extern bool EnableWindow(IntPtr hWnd, bool bEnable);
        [DllImport("user32.dll")]
        static extern int SetWindowLong(IntPtr hWnd, int nIndex, IntPtr dwNewLong);
        [DllImport("user32.dll", SetLastError = true)]
        static extern int GetWindowLong(IntPtr hWnd, int nIndex);
        [DllImport("user32.dll")]
        static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);
        const int WS_EX_TRANSPARENT = 0x00000020;
        const int GWL_EXSTYLE = (-20);
        const int GWL_HWNDPARENT = -8;

        static object locker = new object();
        static Dictionary<IntPtr, WindowLockInfo> lockedWindowsDict = new Dictionary<IntPtr, WindowLockInfo>();

        class WindowLockInfo {
            public int LockCounter { get; set; }
            public SplashScreenLock LockMode { get; private set; }
            public bool IsHitTestVisible { get; private set; }

            public WindowLockInfo(int lockCounter, SplashScreenLock lockMode, bool isHitTestVisible) {
                LockCounter = lockCounter;
                LockMode = lockMode;
                IsHitTestVisible = isHitTestVisible;
            }
        }

        public static void InvokeAsync(WindowContainer container, Action action, DispatcherPriority priority = DispatcherPriority.Normal, AsyncInvokeMode mode = AsyncInvokeMode.AsyncOnly) {
            if(container != null)
                InvokeAsync(container.Window, action, priority, mode);
        }
        public static void InvokeAsync(DispatcherObject dispatcherObject, Action action, DispatcherPriority priority = DispatcherPriority.Normal, AsyncInvokeMode mode = AsyncInvokeMode.AsyncOnly) {
            if(dispatcherObject == null || dispatcherObject.Dispatcher == null)
                return;

            if(mode == AsyncInvokeMode.AllowSyncInvoke && dispatcherObject.Dispatcher.CheckAccess())
                action.Invoke();
            else
                dispatcherObject.Dispatcher.BeginInvoke(action, priority);

        }
        public static T FindParameter<T>(object parameter, T fallbackValue = default(T)) {
            if(parameter is T)
                return (T)parameter;

            if(parameter is object[])
                foreach(object val in (object[])parameter)
                    if(val is T)
                        return (T)val;

            return fallbackValue;
        }
        public static IList<T> FindParameters<T>(object parameter) {
            if(parameter is T)
                return new List<T>() { (T)parameter };

            var result = new List<T>();
            if(parameter is object[])
                foreach(object val in (object[])parameter)
                    if(val is T)
                        result.Add((T)val);

            return result.Count > 0 ? result : null;
        }
        [SecuritySafeCritical]
        public static void SetParent(Window window, IntPtr parentHandle) {
            if(window.IsVisible) {
                SetWindowLong(new WindowInteropHelper(window).Handle, GWL_HWNDPARENT, parentHandle);
            } else {
                WindowInteropHelper windowInteropHelper = new WindowInteropHelper(window);
                windowInteropHelper.Owner = parentHandle;
            }
        }
        public static bool ApplicationHasActiveWindow() {
            return GetApplicationActiveWindow(false) != null;
        }
        public static Window GetApplicationActiveWindow(bool mainWindowIfNull) {
            if(Application.Current == null || !Application.Current.Dispatcher.CheckAccess())
                return null;

            foreach(Window window in Application.Current.Windows)
                if(window.Dispatcher.CheckAccess() && window.IsActive)
                    return window;

            return mainWindowIfNull ? Application.Current.Return(x => x.MainWindow, null) : null;
        }

        [SecuritySafeCritical]
        static void SetWindowEnabled(IntPtr windowHandle, bool isEnabled) {
            if(windowHandle == IntPtr.Zero)
                return;

            EnableWindow(windowHandle, isEnabled);
        }
        [SecuritySafeCritical]
        public static bool PatchWindowStyle(Window window) {
            var wndHelper = new WindowInteropHelper(window);
            if(wndHelper.Handle == IntPtr.Zero)
                return false;
            int exStyle = GetWindowLong(wndHelper.Handle, GWL_EXSTYLE);
            SetWindowLong(wndHelper.Handle, GWL_EXSTYLE, exStyle | WS_EX_TRANSPARENT);
            return true;
        }
        #region Lock logic
        public static void LockWindow(WindowContainer container, SplashScreenLock lockMode) {
            if(container == null || container.Handle == IntPtr.Zero || lockMode == SplashScreenLock.None)
                return;

            IntPtr handle = container.Handle;
            lock(locker) {
                WindowLockInfo lockInfo;
                if(!lockedWindowsDict.TryGetValue(handle, out lockInfo))
                    lockedWindowsDict.Add(handle, (lockInfo = new WindowLockInfo(1, lockMode, container.Window.IsHitTestVisible)));
                else
                    ++lockInfo.LockCounter;

                if(lockInfo.LockCounter == 1)
                    DisableWindow(container, lockMode);
            }
        }
        public static void UnlockWindow(WindowContainer container) {
            if(container.Handle == IntPtr.Zero)
                return;

            IntPtr handle = container.Handle;
            lock(locker) {
                WindowLockInfo lockInfo;
                if(!lockedWindowsDict.TryGetValue(handle, out lockInfo))
                    return;

                if(--lockInfo.LockCounter == 0) {
                    lockedWindowsDict.Remove(handle);
                    EnableWindow(container, lockInfo);
                }
            }
        }
        static void DisableWindow(WindowContainer container, SplashScreenLock lockMode) {
            if(lockMode == SplashScreenLock.Full)
                SplashScreenHelper.SetWindowEnabled(container.Handle, false);
            else if(lockMode == SplashScreenLock.InputOnly) {
                container.Window.IsHitTestVisible = false;
                container.Window.PreviewKeyDown += OnWindowKeyDown;
            } else if(lockMode == SplashScreenLock.LoadingContent) {
                FrameworkElement content = container.WindowObject as FrameworkElement;
                if(content != null) {
                    content.PreviewKeyDown -= OnWindowKeyDown;
                    content.IsHitTestVisible = false;
                }
            }
        }
        static void EnableWindow(WindowContainer container, WindowLockInfo lockInfo) {
            if(lockInfo.LockMode == SplashScreenLock.Full)
                SplashScreenHelper.SetWindowEnabled(container.Handle, true);
            else if(lockInfo.LockMode == SplashScreenLock.InputOnly) {
                container.Window.IsHitTestVisible = lockInfo.IsHitTestVisible;
                container.Window.PreviewKeyDown -= OnWindowKeyDown;
            } else if(lockInfo.LockMode == SplashScreenLock.LoadingContent) {
                FrameworkElement content = container.WindowObject as FrameworkElement;
                if(content != null) {
                    content.PreviewKeyDown -= OnWindowKeyDown;
                    content.IsHitTestVisible = lockInfo.IsHitTestVisible;
                }
            }
        }
        static void OnWindowKeyDown(object sender, System.Windows.Input.KeyEventArgs e) {
            e.Handled = true;
        }
        #endregion
    }
}