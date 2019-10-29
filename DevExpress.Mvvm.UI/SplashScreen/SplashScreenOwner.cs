using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Security;
using System.Windows;
using System.Windows.Threading;
using DevExpress.Mvvm.Native;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Input;
using System.Threading;
using DevExpress.Mvvm.UI.Native;


namespace DevExpress.Mvvm.UI {

    public enum SplashScreenLocation {
        CenterContainer,
        CenterWindow,
        CenterScreen
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
    public enum NotInitializedStateMethodCallPolicy {
        CallWhenReady,
        Discard,
        Exception
    }
    public enum UIThreadReleaseMode {
        WaitForSplashScreenInitialized,
        WaitForSplashScreenLoaded
    }

    public class SplashScreenOwner {
        public static readonly DependencyProperty PreferVisualTreeForOwnerSearchProperty =
            DependencyProperty.RegisterAttached("PreferVisualTreeForOwnerSearch", typeof(bool), typeof(SplashScreenOwner), new PropertyMetadata(false));

        public static bool GetPreferVisualTreeForOwnerSearch(DependencyObject obj) {
            return (bool)obj.GetValue(PreferVisualTreeForOwnerSearchProperty);
        }
        public static void SetPreferVisualTreeForOwnerSearch(DependencyObject obj, bool value) {
            obj.SetValue(PreferVisualTreeForOwnerSearchProperty, value);
        }

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
            if(splashScreenStartupLocation == WindowStartupLocation.CenterScreen)
                result = new WindowArrangerContainer(Owner, SplashScreenLocation.CenterScreen) { ArrangeMode = SplashScreenArrangeMode.ArrangeOnStartupOnly };
            if(result == null || !result.IsInitialized)
                result = new WindowContainer(Owner);

            return result;
        }
    }

    internal class ContainerLocker {
        static object locker = new object();
        static Dictionary<IntPtr, ContainerLockInfo> lockedWindowsDict = new Dictionary<IntPtr, ContainerLockInfo>();
        static Dictionary<DependencyObject, ContainerLockInfo> lockedContainerDict = new Dictionary<DependencyObject, ContainerLockInfo>();
        static Dictionary<WindowContainer, ContainerLockInfo> infosFromContainer = new Dictionary<WindowContainer, ContainerLockInfo>();
        readonly SplashScreenLock lockMode;
        WindowContainer Container { get; set; }

        public ContainerLocker(WindowContainer container, SplashScreenLock lockMode) {
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
            if(lockMode == SplashScreenLock.None)
                return;

            if(container.Form != null && container.Form.IsDisposed) {
                UnlockContainer(container);
            } else
                SplashScreenHelper.InvokeAsync(container, () => {
                    bool activateWindow = activateWindowIfNeeded && !SplashScreenHelper.ApplicationHasActiveWindow();
                    UnlockContainer(container);
                    if(activateWindow)
                        container.ActivateWindow();
                    else if(Keyboard.FocusedElement == null)
                        SplashScreenHelper.GetApplicationActiveWindow(false).Do(x => x.Focus());
                }, DispatcherPriority.Render);
        }

        void OnOwnerInitialized(object sender, EventArgs e) {
            ((WindowContainer)sender).Initialized -= OnOwnerInitialized;
            Initialize();
        }
        void Initialize() {
            if(Container != null)
                SplashScreenHelper.InvokeAsync(Container, () => LockContainer(Container, lockMode), DispatcherPriority.Send, AsyncInvokeMode.AllowSyncInvoke);
        }

        #region Lock logic
        static void LockContainer(WindowContainer container, SplashScreenLock lockMode) {
            if(container == null || container.Handle == IntPtr.Zero)
                return;

            lockMode = GetActualLockMode(container, lockMode);
            if(lockMode == SplashScreenLock.None)
                return;

            lock(locker) {
                ContainerLockInfo lockInfo;
                if(lockMode == SplashScreenLock.LoadingContent) {
                    if(!lockedContainerDict.TryGetValue(container.WindowObject, out lockInfo))
                        lockedContainerDict.Add(container.WindowObject, (lockInfo = new ContainerLockInfo(0, lockMode)));
                } else if(!lockedWindowsDict.TryGetValue(container.Handle, out lockInfo))
                    lockedWindowsDict.Add(container.Handle, (lockInfo = new ContainerLockInfo(0, lockMode)));

                ++lockInfo.LockCounter;
                infosFromContainer.Add(container, lockInfo);
                if(lockInfo.LockCounter == 1)
                    DisableWindow(container, lockMode);
            }
        }
        static void UnlockContainer(WindowContainer container) {
            if(container == null || container.Handle == IntPtr.Zero)
                return;

            lock(locker) {
                ContainerLockInfo lockInfo;
                if(!infosFromContainer.TryGetValue(container, out lockInfo))
                    return;

                infosFromContainer.Remove(container);
                if(--lockInfo.LockCounter == 0) {
                    if(lockInfo.LockMode == SplashScreenLock.LoadingContent)
                        lockedContainerDict.Remove(container.WindowObject);
                    else
                        lockedWindowsDict.Remove(container.Handle);
                    EnableWindow(container, lockInfo.LockMode);
                }
            }
        }
        static void DisableWindow(WindowContainer container, SplashScreenLock lockMode) {
            if(lockMode == SplashScreenLock.Full) {
                SplashScreenHelper.SetWindowEnabled(container.Handle, false);
            } else {
                UIElement visual = lockMode == SplashScreenLock.InputOnly ? container.Window : container.WindowObject as UIElement;
                if(visual != null) {
                    visual.SetCurrentValue(UIElement.IsHitTestVisibleProperty, false);
                    visual.PreviewKeyDown += OnWindowKeyDown;
                }
            }
        }
        static void EnableWindow(WindowContainer container, SplashScreenLock lockMode) {
            if(lockMode == SplashScreenLock.Full) {
                SplashScreenHelper.SetWindowEnabled(container.Handle, true);
            } else {
                UIElement visual = lockMode == SplashScreenLock.InputOnly ? container.Window : container.WindowObject as UIElement;
                if(visual != null) {
                    if(visual.Dispatcher.CheckAccess())
                        visual.SetCurrentValue(UIElement.IsHitTestVisibleProperty, true);
                    visual.PreviewKeyDown -= OnWindowKeyDown;
                }
            }
        }
        static SplashScreenLock GetActualLockMode(WindowContainer container, SplashScreenLock lockMode) {
            SplashScreenLock result = SplashScreenLock.None;
            if(lockMode == SplashScreenLock.Full || (lockMode == SplashScreenLock.InputOnly && container.Form != null))
                result = SplashScreenLock.Full;
            else if(lockMode == SplashScreenLock.LoadingContent && container.WindowObject == container.Window)
                result = SplashScreenLock.InputOnly;
            else if((lockMode == SplashScreenLock.InputOnly && container.Window != null) || lockMode == SplashScreenLock.LoadingContent)
                result = lockMode;

            return result;
        }
        static void OnWindowKeyDown(object sender, System.Windows.Input.KeyEventArgs e) {
            e.Handled = true;
        }
        class ContainerLockInfo {
            public int LockCounter { get; set; }
            public SplashScreenLock LockMode { get; private set; }

            public ContainerLockInfo(int lockCounter, SplashScreenLock lockMode) {
                LockCounter = lockCounter;
                LockMode = lockMode;
            }
        }
        #endregion
    }

    internal class CompositionTargetBasedArranger : WindowArrangerBase {
        protected internal CompositionTargetBasedArranger(WindowArrangerContainer parent, SplashScreenLocation childLocation, SplashScreenArrangeMode arrangeMode)
            : base(parent, childLocation, arrangeMode) { }

        protected override void SubscribeChildEventsOverride() {
            CompositionTarget.Rendering += OnChildRendering;
        }
        protected override void SubscribeParentEventsOverride() {
            if(childLocation != SplashScreenLocation.CenterScreen)
                CompositionTarget.Rendering += OnParentRendering;
        }
        void OnChildRendering(object sender, EventArgs e) {
            if(IsReleased) {
                CompositionTarget.Rendering -= OnChildRendering;
                return;
            }

            if(((Dispatcher)sender).Thread.ManagedThreadId == Child.ManagedThreadId)
                UpdateChildLocation();
        }
        void OnParentRendering(object sender, EventArgs e) {
            if(IsReleased) {
                CompositionTarget.Rendering -= OnParentRendering;
                return;
            }

            if(((Dispatcher)sender).Thread.ManagedThreadId == Parent.ManagedThreadId)
                UpdateParentLocation();
        }
    }
    internal class WindowArranger : WindowArrangerBase {
        protected internal WindowArranger(WindowArrangerContainer parent, SplashScreenLocation childLocation, SplashScreenArrangeMode arrangeMode)
            : base(parent, childLocation, arrangeMode) { }

        protected override void CompleteInitializationOverride() {
            base.CompleteInitializationOverride();
            if(Child.Window.Dispatcher.CheckAccess())
                UpdateChildLocation();

            SplashScreenHelper.InvokeAsync(Parent, () => UpdateNextParentRectAndChildPosition(true), DispatcherPriority.Normal, AsyncInvokeMode.AllowSyncInvoke);
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
            if(childLocation == SplashScreenLocation.CenterScreen)
                return;
            if(Parent.Window != null) {
                Parent.Window.LocationChanged += OnParentSizeOrPositionChanged;
                Parent.Window.SizeChanged += OnParentSizeOrPositionChanged;
            } else if(Parent.Form != null) {
                Parent.Form.LocationChanged += OnParentSizeOrPositionChanged;
                Parent.Form.SizeChanged += OnParentSizeOrPositionChanged;
            }
            if(childLocation == SplashScreenLocation.CenterContainer && ParentContainer != null && Parent.Window != ParentContainer) {
                ParentContainer.SizeChanged += OnParentSizeOrPositionChanged;
                ParentContainer.LayoutUpdated += OnParentSizeOrPositionChanged;
            }
        }

        protected override void UnsubscribeParentEventsOverride() {
            if(childLocation == SplashScreenLocation.CenterScreen)
                return;
            if(Parent.Window != null) {
                Parent.Window.LocationChanged -= OnParentSizeOrPositionChanged;
                Parent.Window.SizeChanged -= OnParentSizeOrPositionChanged;
            } else if(Parent.Form != null) {
                Parent.Form.LocationChanged -= OnParentSizeOrPositionChanged;
                Parent.Form.SizeChanged -= OnParentSizeOrPositionChanged;
            }
            if(childLocation == SplashScreenLocation.CenterContainer && ParentContainer != null && Parent.Window != ParentContainer) {
                try {
                    ParentContainer.SizeChanged -= OnParentSizeOrPositionChanged;
                    ParentContainer.LayoutUpdated -= OnParentSizeOrPositionChanged;
                } catch { }
            }
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
                UpdateChildLocation();
        }
        void UpdateNextParentRectAndChildPosition(bool skipSizeCheck) {
            if(SkipArrange || !Parent.IsInitialized)
                return;

            UpdateParentLocation();
            if(!skipSizeCheck && lastParentPos == nextParentPos || nextParentPos.IsEmpty)
                return;

            SplashScreenHelper.InvokeAsync(Child, () => UpdateChildLocation(), DispatcherPriority.Normal, AsyncInvokeMode.AllowSyncInvoke);
        }
    }
    internal abstract class WindowArrangerBase : WindowRelationInfo {
        protected FrameworkElement ParentContainer { get { return Parent.WindowObject as FrameworkElement; } }
        protected Rect lastChildPos = Rect.Empty;
        protected Rect lastParentPos = Rect.Empty;
        protected Rect nextParentPos = Rect.Empty;
        protected SplashScreenLocation childLocation;
        protected bool SkipArrange { get { return IsReleased || arrangeMode == SplashScreenArrangeMode.Skip; } }
        protected bool IsArrangeValid { get { return nextParentPos == lastParentPos && lastChildPos.Width == Child.Window.ActualWidth && lastChildPos.Height == Child.Window.ActualHeight; } }
        SplashScreenArrangeMode arrangeMode;

        protected internal WindowArrangerBase(WindowArrangerContainer parent, SplashScreenLocation childLocation, SplashScreenArrangeMode arrangeMode) {
            this.childLocation = childLocation;
            this.arrangeMode = arrangeMode;
            AttachParent(parent);
        }

        protected override void ChildAttachedOverride() {
            Child.Window.WindowStartupLocation = WindowStartupLocation.Manual;
        }
        protected override void CompleteInitializationOverride() {
            if(Child.Window.Dispatcher.CheckAccess()) {
                switch(childLocation) {
                    case SplashScreenLocation.CenterContainer:
                        nextParentPos = ((WindowArrangerContainer)Parent).ControlStartupPosition;
                        break;
                    case SplashScreenLocation.CenterWindow:
                        nextParentPos = ((WindowArrangerContainer)Parent).WindowStartupPosition;
                        break;
                    case SplashScreenLocation.CenterScreen:
                        var screen = System.Windows.Forms.Screen.FromHandle(Parent.Handle);
                        nextParentPos = new Rect(new Point(screen.WorkingArea.X, screen.WorkingArea.Y), new Size(screen.WorkingArea.Width, screen.WorkingArea.Height));
                        break;
                }
            }
        }
        protected void UpdateParentLocation() {
            if(Parent == null) {
                nextParentPos = Rect.Empty;
                return;
            }

            switch(childLocation) {
                case SplashScreenLocation.CenterContainer:
                    nextParentPos = ActualIsParentClosed ? Rect.Empty : ((WindowArrangerContainer)Parent).GetControlRect();
                    break;
                case SplashScreenLocation.CenterWindow:
                    nextParentPos = ActualIsParentClosed ? Rect.Empty : ((WindowArrangerContainer)Parent).GetWindowRect();
                    break;
                case SplashScreenLocation.CenterScreen:
                    var screen = System.Windows.Forms.Screen.FromHandle(Parent.Handle);
                    nextParentPos = new Rect(new Point(screen.WorkingArea.X, screen.WorkingArea.Y), new Size(screen.WorkingArea.Width, screen.WorkingArea.Height));
                    break;
            }
        }
        protected void UpdateChildLocation() {
            if(SkipArrange || Child == null || !Child.IsInitialized || IsArrangeValid)
                return;
            if(arrangeMode == SplashScreenArrangeMode.ArrangeOnStartupOnly && lastParentPos != Rect.Empty && lastParentPos != nextParentPos) {
                arrangeMode = SplashScreenArrangeMode.Skip;
                return;
            }
            Rect bounds = nextParentPos;
            var window = Child.Window;
            if(!IsZero(window.ActualWidth) && !IsZero(window.ActualHeight)) {
                if(childLocation == SplashScreenLocation.CenterScreen)
                    bounds = GetDpiBasedBounds(bounds, window);
                var newPosition = new Point(bounds.X + (bounds.Width - window.ActualWidth) * 0.5, bounds.Y + (bounds.Height - window.ActualHeight) * 0.5);
                window.Left = Math.Round(newPosition.X);
                window.Top = Math.Round(newPosition.Y);
                lastChildPos = new Rect(window.Left, window.Top, window.Width, window.Height);
                lastParentPos = bounds;
            }
        }
        static bool IsZero(double value) {
            return value == 0d || double.IsNaN(value);
        }
        static Rect GetDpiBasedBounds(Rect bounds, Visual visual) {
            var presentationSource = PresentationSource.FromVisual(visual);
            if(presentationSource != null) {
                return new Rect(bounds.X / presentationSource.CompositionTarget.TransformToDevice.M11,
                    bounds.Y / presentationSource.CompositionTarget.TransformToDevice.M22,
                    bounds.Width / presentationSource.CompositionTarget.TransformToDevice.M11,
                    bounds.Height / presentationSource.CompositionTarget.TransformToDevice.M22);
            }
            return bounds;
        }
    }
    internal class WindowRelationInfo {
        public WindowContainer Parent { get; private set; }
        public WindowContainer Child { get; private set; }
        public bool IsInitialized { get; private set; }
        public bool IsReleased { get; private set; }
        public bool ActualIsParentClosed { get { return isParentClosed || (Parent != null && Parent.IsWindowClosedBeforeInit); } }
        bool isParentClosed;

        protected internal WindowRelationInfo(WindowContainer parent) {
            AttachParent(parent);
        }
        protected WindowRelationInfo() { }

        public void AttachChild(Window child, bool isChildHandleRequired) {
            if(Child != null)
                throw new ArgumentException("Child property is already set");

            Child = new WindowContainer(child, isChildHandleRequired);
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

        protected void AttachParent(WindowContainer parent) {
            if(parent == null)
                throw new ArgumentNullException("Parent");

            Parent = parent;
            CompleteContainerInitialization(Parent);
        }

        protected virtual void SubscribeParentEventsOverride() { }
        protected virtual void SubscribeChildEventsOverride() { }
        protected virtual void UnsubscribeParentEventsOverride() { }
        protected virtual void UnsubscribeChildEventsOverride() { }
        protected virtual void CompleteInitializationOverride() { }
        protected virtual void ChildAttachedOverride() { }
        protected virtual void OnParentClosed(object sender, EventArgs e) {
            isParentClosed = true;
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
            if(IsReleased || IsInitialized || Child == null || !Child.IsInitialized || Parent.Handle == IntPtr.Zero)
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

            Parent.Window.Do(x => x.Closed += OnParentClosed);
            Parent.Form.Do(x => x.FormClosed += OnParentClosed);
            SubscribeParentEventsOverride();
        }
        void UnsubscribeParentEvents() {
            if(Parent == null)
                return;

            Parent.Window.Do(x => x.Closed -= OnParentClosed);
            Parent.Form.Do(x => x.FormClosed -= OnParentClosed);
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
            return DXSplashScreen.UseLegacyLocationLogic
                ? (WindowRelationInfo)new WindowArranger(this, arrangeLocation, ArrangeMode)
                : new CompositionTargetBasedArranger(this, arrangeLocation, ArrangeMode);
        }
        public Rect GetWindowRect() {
            if(Form != null)
                return new Rect(Form.Left, Form.Top, Form.Width, Form.Height);
            return GetRealRect(Window);
        }
        public Rect GetControlRect() {
            return GetRealRect(FrameworkObject);
        }
        static Rect GetRealRect(FrameworkElement element) {
            if(element == null || !element.IsLoaded || PresentationSource.FromDependencyObject(element) == null)
                return Rect.Empty;
            return LayoutHelper.GetScreenRect(element);
        }

        protected override void CompleteInitializationOverride() {
            ControlStartupPosition = GetControlRect();
            WindowStartupPosition = GetWindowRect();
        }
    }
    internal class WindowContainer {
        public DependencyObject WindowObject { get; private set; }
        public Window Window { get; private set; }
        public System.Windows.Forms.Form Form { get; private set; }
        public IntPtr Handle { get; private set; }
        public bool IsInitialized { get; private set; }
        public bool IsWindowClosedBeforeInit { get; private set; }
        public int ManagedThreadId { get; private set; }
        protected FrameworkElement FrameworkObject { get; private set; }
        bool isHandleRequired;

        public WindowContainer(DependencyObject windowObject) : this(windowObject, true) { }
        public WindowContainer(DependencyObject windowObject, bool isHandleRequired) {
            if(windowObject == null)
                throw new ArgumentNullException("WindowObject");

            this.isHandleRequired = isHandleRequired;
            WindowObject = windowObject;
            FrameworkObject = WindowObject as FrameworkElement;
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
            TryInitializeWindow();
            if(IsInitialized || Window != null)
                return;

            if(!FrameworkObject.Return(x => x.IsLoaded, () => true))
                FrameworkObject.Loaded += OnControlLoaded;
            else
                TryInitializeWindowForm();
        }

        void TryInitializeWindowForm() {
            if(IsInitialized)
                return;

            HwndSource source = (WindowObject as Visual).With(x => PresentationSource.FromVisual(x) as HwndSource);
            if(source == null || source.Handle == IntPtr.Zero)
                return;

            Form = System.Windows.Forms.Control.FromChildHandle(source.Handle).With(x => x.FindForm());
            if(Form != null) {
                Handle = Form.Handle;
                ManagedThreadId = (int)(Form.Invoke(new Func<int>(() => Thread.CurrentThread.ManagedThreadId)));
                CompleteInitialization();
            }
        }

        void TryInitializeWindow() {
            if(IsInitialized || IsWindowClosedBeforeInit)
                return;

            Window = FindOwner();
            if(Window == null)
                return;

            if(isHandleRequired) {
                IntPtr handle;
                if(EnsureWindowHandle(out handle)) {
                    Handle = handle;
                    ManagedThreadId = Window.Dispatcher.Thread.ManagedThreadId;
                    CompleteInitialization();
                }
            } else if(Window.Dispatcher != null) {
                ManagedThreadId = Window.Dispatcher.Thread.ManagedThreadId;
                CompleteInitialization();
            }
        }

        Window FindOwner() {
            Window result = WindowObject as Window;
            if(result != null)
                return result;

            bool visualTreeSearch = SplashScreenOwner.GetPreferVisualTreeForOwnerSearch(WindowObject);
            if(visualTreeSearch)
                result = LayoutHelper.FindParentObject<Window>(WindowObject);
            if(result == null)
                result = Window.GetWindow(WindowObject);

            if(!visualTreeSearch && result != null && SplashScreenOwner.GetPreferVisualTreeForOwnerSearch(result)) {
                visualTreeSearch = true;
                var newWindow = LayoutHelper.FindParentObject<Window>(WindowObject);
                if(newWindow != null)
                    result = newWindow;
            }
            return result;
        }
        bool EnsureWindowHandle(out IntPtr handle) {
            handle = IntPtr.Zero;
            WindowInteropHelper helper = new WindowInteropHelper(Window);
            if(helper.Handle == IntPtr.Zero) {
                try {
                    helper.EnsureHandle();
                } catch(InvalidOperationException) {
                    IsWindowClosedBeforeInit = true;
                    return false;
                }
            }

            handle = helper.Handle;
            return true;
        }

        void OnWindowSourceInitialized(object sender, EventArgs e) {
            (sender as Window).SourceInitialized -= OnWindowSourceInitialized;
            Initialize();
        }
        void CompleteInitialization() {
            CompleteInitializationOverride();
            IsInitialized = true;
            Initialized.Do(x => x(this, EventArgs.Empty));
        }
        void OnControlLoaded(object sender, RoutedEventArgs e) {
            FrameworkObject.Loaded -= OnControlLoaded;
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
        internal const int WS_EX_TRANSPARENT = 0x00000020;
        internal const int WS_EX_TOOLWINDOW = 0x00000080;
        const int GWL_EXSTYLE = (-20);
        const int GWL_HWNDPARENT = -8;
#if DEBUGTEST || DEBUG
        internal static int Test_WindowStyleModifier { get; set; }
#endif
        public static void InvokeAsync(WindowContainer container, Action action, DispatcherPriority priority = DispatcherPriority.Normal, AsyncInvokeMode mode = AsyncInvokeMode.AsyncOnly) {
            if(container == null || !container.IsInitialized)
                return;

            if(container.Window != null)
                InvokeAsync(container.Window, action, priority, mode);
            else
                InvokeAsync(container.Form, action, mode);
        }
        public static void InvokeAsync(DispatcherObject dispatcherObject, Action action, DispatcherPriority priority = DispatcherPriority.Normal, AsyncInvokeMode mode = AsyncInvokeMode.AsyncOnly) {
            if(dispatcherObject == null || dispatcherObject.Dispatcher == null)
                return;

            if(mode == AsyncInvokeMode.AllowSyncInvoke && dispatcherObject.Dispatcher.CheckAccess())
                action.Invoke();
            else
                dispatcherObject.Dispatcher.BeginInvoke(action, priority);

        }
        public static void InvokeAsync(System.Windows.Forms.Control dispatcherObject, Action action, AsyncInvokeMode mode = AsyncInvokeMode.AsyncOnly) {
            if(dispatcherObject == null || dispatcherObject.IsDisposed)
                return;

            if(mode == AsyncInvokeMode.AllowSyncInvoke && !dispatcherObject.InvokeRequired)
                action.Invoke();
            else
                dispatcherObject.BeginInvoke(action);

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
            if(!HasAccess(window))
                return;

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
        internal static void SetWindowEnabled(IntPtr windowHandle, bool isEnabled) {
            if(windowHandle == IntPtr.Zero)
                return;

            EnableWindow(windowHandle, isEnabled);
        }
        [SecuritySafeCritical]
        public static bool PatchWindowStyle(Window window, bool hasOwner) {
            if(!HasAccess(window))
                return false;

            var wndHelper = new WindowInteropHelper(window);
            if(wndHelper.Handle == IntPtr.Zero)
                return false;
            int exStyle = GetWindowLong(wndHelper.Handle, GWL_EXSTYLE);
            int styleModifier = hasOwner
                ? WS_EX_TRANSPARENT
                : GetToolWindowStyle(window);
#if DEBUGTEST || DEBUG
            Test_WindowStyleModifier = styleModifier;
#endif
            SetWindowLong(wndHelper.Handle, GWL_EXSTYLE, exStyle | styleModifier);
            return true;
        }
        static int GetToolWindowStyle(Window window) {
            return !DXSplashScreen.UseDefaultAltTabBehavior && window.WindowStyle == WindowStyle.None && window is DXSplashScreen.SplashScreenWindow
                ? WS_EX_TOOLWINDOW
                : 0;
        }
        internal static bool HasAccess(Window window) {
            return !DXSplashScreen.DisableThreadingProblemsDetection || (window != null && window.Dispatcher != null && window.Dispatcher.CheckAccess());
        }
    }
}