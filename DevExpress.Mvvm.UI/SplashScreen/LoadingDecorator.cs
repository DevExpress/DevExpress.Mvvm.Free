using DevExpress.Mvvm.Native;
using System;
using System.Runtime.InteropServices;
using System.Security;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Threading;
using System.Collections.Concurrent;
using DevExpress.Mvvm.UI.Native;

namespace DevExpress.Mvvm.UI {

    [ContentProperty("LoadingChild")]
    public class LoadingDecorator : Decorator {
        const string Exception1 = "LoadingDecorator shows its SplashScreen in a separate thread, so it is impossible to put a DependencyObject into the SplashScreenDataContext.";

        #region Static
        public static readonly DependencyProperty UseFadeEffectProperty;
        public static readonly DependencyProperty UseSplashScreenProperty;
        public static readonly DependencyProperty SplashScreenTemplateProperty;
        public static readonly DependencyProperty SplashScreenDataContextProperty;
        public static readonly DependencyProperty IsSplashScreenShownProperty;
        public static readonly DependencyProperty LockWindowProperty;
        public static readonly DependencyProperty SplashScreenLocationProperty;

        static LoadingDecorator() {
            UseFadeEffectProperty = DependencyProperty.Register("UseFadeEffect", typeof(bool), typeof(LoadingDecorator),
                new PropertyMetadata(true));
            UseSplashScreenProperty = DependencyProperty.Register("UseSplashScreen", typeof(bool), typeof(LoadingDecorator),
                new PropertyMetadata(true));
            SplashScreenTemplateProperty = DependencyProperty.Register("SplashScreenTemplate", typeof(DataTemplate), typeof(LoadingDecorator),
                new PropertyMetadata(null, (d, e) => ((LoadingDecorator)d).OnSplashScreenTemplateChanged()));
            SplashScreenDataContextProperty = DependencyProperty.Register("SplashScreenDataContext", typeof(object), typeof(LoadingDecorator),
                new PropertyMetadata(null));
            IsSplashScreenShownProperty = DependencyProperty.Register("IsSplashScreenShown", typeof(bool?), typeof(LoadingDecorator),
                new PropertyMetadata(null, (d, e) => ((LoadingDecorator)d).OnIsSplashScreenShownChanged()));
            LockWindowProperty = DependencyProperty.Register("LockWindow", typeof(bool), typeof(LoadingDecorator),
                new PropertyMetadata(true));
            SplashScreenLocationProperty = DependencyProperty.Register("SplashScreenLocation", typeof(SplashScreenLocation), typeof(LoadingDecorator),
                new PropertyMetadata(SplashScreenLocation.CenterContainer));
        }
        #endregion

        #region Dependency Properties
        public bool UseFadeEffect {
            get { return (bool)GetValue(UseFadeEffectProperty); }
            set { SetValue(UseFadeEffectProperty, value); }
        }
        public bool UseSplashScreen {
            get { return (bool)GetValue(UseSplashScreenProperty); }
            set { SetValue(UseSplashScreenProperty, value); }
        }
        public DataTemplate SplashScreenTemplate {
            get { return (DataTemplate)GetValue(SplashScreenTemplateProperty); }
            set { SetValue(SplashScreenTemplateProperty, value); }
        }
        public object SplashScreenDataContext {
            get { return (object)GetValue(SplashScreenDataContextProperty); }
            set { SetValue(SplashScreenDataContextProperty, value); }
        }
        public bool? IsSplashScreenShown {
            get { return (bool?)GetValue(IsSplashScreenShownProperty); }
            set { SetValue(IsSplashScreenShownProperty, value); }
        }
        public bool LockWindow {
            get { return (bool)GetValue(LockWindowProperty); }
            set { SetValue(LockWindowProperty, value); }
        }
        public SplashScreenLocation SplashScreenLocation {
            get { return (SplashScreenLocation)GetValue(SplashScreenLocationProperty); }
            set { SetValue(SplashScreenLocationProperty, value); }
        }
        #endregion

        #region Props
        DXSplashScreen.SplashScreenContainer splashContainer = null;
        DXSplashScreen.SplashScreenContainer SplashContainer {
            get {
                if(splashContainer == null)
                    splashContainer = new DXSplashScreen.SplashScreenContainer();
                return splashContainer;
            }
        }
        FrameworkElement loadingChild = null;
        public FrameworkElement LoadingChild {
            get { return loadingChild; }
            set {
                if(loadingChild == value) return;
                loadingChild = value;
                if(ViewModelBase.IsInDesignMode) {
                    Child = loadingChild;
                    return;
                }
                OnLoadingChildChanged();
            }
        }
        #endregion

        public LoadingDecorator() {
            Loaded += OnLoaded;
            Unloaded += OnUnloaded;
        }

        void OnIsSplashScreenShownChanged() {
            if(IsSplashScreenShown == true) {
                if(IsLoaded)
                    ShowSplashScreen();
                else
                    InvokeOnThread(Dispatcher, () => { if(IsSplashScreenShown == true) ShowSplashScreen(); }, DispatcherPriority.Render);
            }
            if(IsSplashScreenShown == false)
                CloseSplashScreen();
        }
        void OnSplashScreenTemplateChanged() {
            if(SplashScreenTemplate != null)
                SplashScreenTemplate.Seal();
        }
        void OnLoaded(object sender, RoutedEventArgs e) {
            Loaded -= OnLoaded;
            if(ViewModelBase.IsInDesignMode)
                return;
            OnLoadingChildChanged();
        }
        void OnUnloaded(object sender, RoutedEventArgs e) {
            CloseSplashScreenOnLoading();
        }
        void OnLoadingChildChanged() {
            Child = null;
            if(!IsLoaded || LoadingChild == null)
                return;
            LoadingChild.Loaded += OnLoadingChildLoaded;
            if(IsSplashScreenShown == null)
                ShowSplashScreen();
            InvokeOnThread(Dispatcher, () => Child = LoadingChild);
        }
        void OnLoadingChildLoaded(object sender, RoutedEventArgs e) {
            FrameworkElement child = (FrameworkElement)sender;
            child.Loaded -= OnLoadingChildLoaded;
            CloseSplashScreenOnLoading();
        }
        void SplashScreenDataContextChanged() {
            if(SplashScreenDataContext is DependencyObject)
                throw new InvalidOperationException(Exception1);
        }

        void ShowSplashScreen() {
            if(!UseSplashScreen || SplashContainer.IsActive)
                return;

            SplashContainer.Show(CreateSplashScreenWindow, CreateSplashScreen, GetSplashScreenCreatorParams(), new object[] { SplashScreenTemplate, SplashScreenDataContext });
        }
        void CloseSplashScreen() {
            if(!SplashContainer.IsActive)
                return;
            SplashContainer.Close();
        }
        void CloseSplashScreenOnLoading() {
            if(IsSplashScreenShown != null || !SplashContainer.IsActive)
                return;

            InvokeOnThread(Dispatcher, CloseSplashScreen, DispatcherPriority.Render);
        }
        object[] GetSplashScreenCreatorParams() {
            return new object[] { UseFadeEffect, new SplashScreenOwner(this), SplashScreenLocation, LockWindow };
        }
        static Window CreateSplashScreenWindow(object parameter) {
            object[] parameters = (object[])parameter;
            bool useFadeEffect = (bool)parameters[0];
            SplashScreenOwner owner = (SplashScreenOwner)parameters[1];
            SplashScreenLocation childLocation = (SplashScreenLocation)parameters[2];
            bool lockWindow = (bool)parameters[3];
            return new LoadingDecoratorWindow(useFadeEffect, owner, childLocation, lockWindow);

        }
        static object CreateSplashScreen(object parameter) {
            object[] parameters = (object[])parameter;
            DataTemplate splashScreenTemplate = (DataTemplate)parameters[0];
            object splashScreenDataContext = parameters[1];
            if(splashScreenTemplate == null) {
                return new WaitIndicator() {
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    DeferedVisibility = true,
                    ShowShadow = false,
                    Margin = new Thickness(),
                    ContentPadding = new Thickness()
                };
            } else {
                var res = splashScreenTemplate.LoadContent();
                if(res is FrameworkElement)
                    ((FrameworkElement)res).DataContext = splashScreenDataContext;
                return res;
            }
        }
        static void InvokeOnThread(Dispatcher dispatcher, Action action, DispatcherPriority priority = DispatcherPriority.Normal, bool forceAsyncInvoke = true) {
            if(dispatcher == null)
                return;

            if(!forceAsyncInvoke && dispatcher.CheckAccess())
                action();
            else
                dispatcher.BeginInvoke(action, priority);
        }

        class LoadingDecoratorWindow : Window {
            #region Props
            WindowArranger arranger;
            WindowLocker locker;
            bool hasFocusOnClosing;
            #endregion

            public LoadingDecoratorWindow(bool useFadeEffect, SplashScreenOwner parentContainer, SplashScreenLocation childLocation, bool lockParent) {
                WindowStyle = WindowStyle.None;
                AllowsTransparency = true;
                ShowInTaskbar = false;
                Background = new SolidColorBrush(Colors.Transparent);
                SizeToContent = SizeToContent.WidthAndHeight;
                WindowStartupLocation = WindowStartupLocation.Manual;
                Left = parentContainer.ControlStartupPosition.Left;
                Top = parentContainer.ControlStartupPosition.Top;
                Width = parentContainer.ControlStartupPosition.Width;
                Height = parentContainer.ControlStartupPosition.Height;
                Topmost = false;
                ShowActivated = false;
                IsHitTestVisible = false;
                Focusable = false;
                CreateArranger(parentContainer, childLocation);
                CreateLocker(parentContainer, lockParent);
                WindowFadeAnimationBehavior.SetEnableAnimation(this, useFadeEffect);
                Loaded += OnWindowLoaded;
            }

            void OnWindowLoaded(object sender, RoutedEventArgs e) {
                Loaded -= OnWindowLoaded;
                if(SizeToContent != SizeToContent.Manual) {
                    SizeToContent oldValue = SizeToContent;
                    SizeToContent = SizeToContent.Manual;
                    SizeToContent = oldValue;
                }
            }
            protected override void OnClosing(System.ComponentModel.CancelEventArgs e) {
                hasFocusOnClosing = IsActive;
                if(!e.Cancel)
                    ReleaseBorderDecorator();
                base.OnClosing(e);
            }
            protected override void OnClosed(EventArgs e) {
                ReleaseBorderDecorator();
                ReleaseLocker();
                ReleaseArranger();
                base.OnClosed(e);
            }
            void OnParentWindowClosed(object sender, EventArgs e) {
                Dispatcher.Invoke(new Action(Close), null);
            }
            [SecuritySafeCritical]
            protected override void OnSourceInitialized(EventArgs e) {
                base.OnSourceInitialized(e);
                var wndHelper = new WindowInteropHelper(this);
                int exStyle = GetWindowLong(wndHelper.Handle, GWL_EXSTYLE);
                SetWindowLong(wndHelper.Handle, GWL_EXSTYLE, exStyle | WS_EX_TRANSPARENT);
            }

            #region Helpers
            void ReleaseLocker() {
                if(locker != null) {
                    locker.Release(hasFocusOnClosing);
                    locker = null;
                }
            }
            void CreateLocker(SplashScreenOwner parentContainer, bool lockWindow) {
                if(lockWindow)
                    locker = new WindowLocker(parentContainer);
            }
            void ReleaseArranger() {
                if(arranger != null) {
                    arranger.ParentClosed -= OnParentWindowClosed;
                    arranger.Release();
                    arranger = null;
                }
            }
            void CreateArranger(SplashScreenOwner parentContainer, SplashScreenLocation childLocation) {
                arranger = new WindowArranger(parentContainer, childLocation);
                arranger.ParentClosed += OnParentWindowClosed;
                arranger.AttachChild(this);
            }
            void ReleaseBorderDecorator() {
            }
            #endregion

            #region Static
            [DllImport("user32.dll", SetLastError = true)]
            static extern int GetWindowLong(IntPtr hWnd, int nIndex);
            [DllImport("user32.dll")]
            static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);
            public const int WS_EX_TRANSPARENT = 0x00000020;
            public const int GWL_EXSTYLE = (-20);
            #endregion
        }

        class WindowLocker {
            #region Static
            [DllImport("user32.dll")]
            static extern bool EnableWindow(IntPtr hWnd, bool bEnable);
            static ConcurrentDictionary<IntPtr, int> windowLockCounter = new ConcurrentDictionary<IntPtr, int>();
            #endregion

            #region Props
            SplashScreenOwner owner;
            #endregion

            public WindowLocker(SplashScreenOwner owner) {
                this.owner = owner;
                if(!owner.IsInitialized)
                    owner.Initialized += OnOwnerInitialized;
                else
                    OnInitialized();
            }

            #region Initialization
            void OnOwnerInitialized(object sender, EventArgs e) {
                ((SplashScreenOwner)sender).Initialized -= OnOwnerInitialized;
                OnInitialized();
            }
            void OnInitialized() {
                if(owner != null)
                    LockWindow(owner.WindowHandle);
            }
            #endregion

            #region Lock logic
            [SecuritySafeCritical]
            static void LockWindow(IntPtr windowHandle) {
                if(windowHandle == IntPtr.Zero)
                    return;

                while(true) {
                    int lockCount = 0;
                    if(!windowLockCounter.TryGetValue(windowHandle, out lockCount)) {
                        if(!windowLockCounter.TryAdd(windowHandle, 0))
                            continue;
                    }
                    if(!windowLockCounter.TryUpdate(windowHandle, lockCount + 1, lockCount))
                        continue;
                    if(++lockCount == 1)
                        EnableWindow(windowHandle, false);
                    break;
                }
            }
            [SecuritySafeCritical]
            static void UnlockWindow(IntPtr windowHandle) {
                if(windowHandle == IntPtr.Zero)
                    return;

                while(true) {
                    int lockCount = 0;
                    if(!windowLockCounter.TryGetValue(windowHandle, out lockCount)) {
                        if(!windowLockCounter.TryAdd(windowHandle, 0))
                            continue;
                    }
                    if(!windowLockCounter.TryUpdate(windowHandle, lockCount - 1, lockCount))
                        continue;
                    if(--lockCount == 0)
                        EnableWindow(windowHandle, true);
                    break;
                }
            }
            public void Release(bool activateWindowIfNeeded) {
                if(owner == null)
                    return;

                var window = owner.Window;
                IntPtr handle = owner.WindowHandle;
                owner.Initialized -= OnOwnerInitialized;
                owner = null;
                if(window == null)
                    return;

                InvokeOnThread(window.Dispatcher, () => {
                    if(activateWindowIfNeeded && !window.IsActive)
                        window.Activate();

                    UnlockWindow(handle);
                }, DispatcherPriority.Render, false);
            }
            #endregion
        }
        class WindowArranger {
            #region Static
            [DllImport("user32.dll")]
            static extern int SetWindowLong(IntPtr hWnd, int nIndex, IntPtr dwNewLong);
            const int GWL_HWNDPARENT = -8;
            #endregion

            #region Props
            Window childWindow;
            IntPtr childHandle;
            Window ParentWindow { get { return parent.Window; } }
            IntPtr ParentHandle { get { return parent.WindowHandle; } }
            FrameworkElement ParentContainer { get { return parent.Control; } }
            SplashScreenOwner parent;
            SplashScreenLocation childLocation;
            Rect lastChildPos = Rect.Empty;
            Rect lastParentPos = Rect.Empty;
            Rect nextParentPos = Rect.Empty;
            bool isInitialized;
            bool isDisposed;
            bool isParentClosed;
            #endregion

            public WindowArranger(SplashScreenOwner parent, SplashScreenLocation childLocation) {
                if(parent == null)
                    throw new ArgumentNullException("parent");

                this.childLocation = childLocation;
                this.parent = parent;
                if(parent.IsInitialized)
                    ParentInitializationComplete();
                else
                    parent.Initialized += OnParentInitialized;
            }

            #region Initialization and Release
            void OnParentInitialized(object sender, EventArgs e) {
                ((SplashScreenOwner)sender).Initialized -= OnParentInitialized;
                ParentInitializationComplete();
            }
            void ParentInitializationComplete() {
                if(isDisposed || isInitialized)
                    return;

                SubscribeParentEvents();
                CompleteInitialization();
            }
            void InitializeChild(Window child) {
                if(child != null) {
                    childWindow = child;
                    SubscribeChildEvents();
                }

                if(!childWindow.IsInitialized || !childWindow.IsLoaded)
                    return;

                childHandle = new WindowInteropHelper(childWindow).Handle;
                CompleteInitialization();
            }
            [SecuritySafeCritical]
            void CompleteInitialization() {
                if(isDisposed || childHandle == IntPtr.Zero || ParentHandle == IntPtr.Zero || isInitialized)
                    return;

                if(childWindow.Dispatcher.CheckAccess()) {
                    nextParentPos = childLocation == SplashScreenLocation.CenterContainer ? parent.ControlStartupPosition : parent.WindowStartupPosition;
                    UpdateChildPosition();
                }
                InvokeOnParentThread(() => UpdateNextParentRectAndChildPosition(true), DispatcherPriority.Normal);
                InvokeOnChildThread(SetChildParent, DispatcherPriority.Render, true);
                isInitialized = true;
            }
            [SecuritySafeCritical]
            void SetChildParent() {
                if(isDisposed || childHandle == IntPtr.Zero || ParentHandle == IntPtr.Zero)
                    return;

                SetWindowLong(childHandle, GWL_HWNDPARENT, ParentHandle);
            }
            public void AttachChild(Window child) {
                if(childWindow != null)
                    throw new ArgumentException("Child property is already set");

                InitializeChild(child);
            }
            public void Release() {
                if(isDisposed)
                    return;

                UnsubscribeChildEvents();
                UnsubscribeParentEvents();
                childWindow = null;
                childHandle = IntPtr.Zero;
                parent.Initialized -= OnParentInitialized;
                parent = null;
                isDisposed = true;
            }
            #endregion

            #region Invoke via dispatcher
            public void InvokeOnParentThread(Action action, DispatcherPriority priority, bool forceAsyncInvoke = false) {
                InvokeOnThread(ParentWindow.Return(x => x.Dispatcher, () => null), action, priority, forceAsyncInvoke);
            }
            void InvokeOnChildThread(Action action, DispatcherPriority priority, bool forceAsyncInvoke = false) {
                InvokeOnThread(childWindow.Return(x => x.Dispatcher, () => null), action, priority, forceAsyncInvoke);
            }
            #endregion

            #region Arrange
            void UpdateNextParentRectAndChildPosition(bool skipSizeCheck) {
                if(!isInitialized || isDisposed)
                    return;

                nextParentPos = isParentClosed
                    ? Rect.Empty
                    : childLocation == SplashScreenLocation.CenterContainer ? parent.GetControlRect() : parent.GetWindowRect();
                if(!skipSizeCheck && lastParentPos == nextParentPos || nextParentPos.IsEmpty)
                    return;

                InvokeOnChildThread(() => UpdateChildPosition(), DispatcherPriority.Normal);
            }
            void UpdateChildPosition() {
                Rect bounds = nextParentPos;
                if(childWindow == null || (bounds == lastParentPos && lastChildPos.Width == childWindow.ActualWidth && lastChildPos.Height == childWindow.ActualHeight))
                    return;

                childWindow.Left = (int)(bounds.X + (bounds.Width - childWindow.ActualWidth) * 0.5);
                childWindow.Top = (int)(bounds.Y + (bounds.Height - childWindow.ActualHeight) * 0.5);
                lastChildPos = new Rect(childWindow.Left, childWindow.Top, childWindow.Width, childWindow.Height);
                lastParentPos = bounds;
            }
            #endregion

            #region Child
            void SubscribeChildEvents() {
                if(childWindow == null)
                    return;

                childWindow.Loaded += ChildInitializeComplete;
                childWindow.Initialized += ChildInitializeComplete;
                childWindow.SizeChanged += ChildSizeChanged;
                childWindow.Closed += ChildClosed;
            }
            void UnsubscribeChildEvents() {
                if(childWindow == null)
                    return;

                childWindow.Loaded -= ChildInitializeComplete;
                childWindow.Initialized -= ChildInitializeComplete;
                childWindow.SizeChanged -= ChildSizeChanged;
                childWindow.Closed -= ChildClosed;
            }
            void ChildClosed(object sender, EventArgs e) {
                UnsubscribeChildEvents();
            }
            void ChildSizeChanged(object sender, EventArgs e) {
                if(ParentHandle == IntPtr.Zero || childWindow == null ||
                    (lastChildPos.Width == childWindow.ActualWidth && lastChildPos.Height == childWindow.ActualHeight))
                    return;

                if(lastParentPos.IsEmpty)
                    InvokeOnParentThread(() => UpdateNextParentRectAndChildPosition(true), DispatcherPriority.Normal);
                else
                    UpdateChildPosition();
            }
            void ChildInitializeComplete(object sender, EventArgs e) {
                InitializeChild(null);
            }
            #endregion

            #region Parent
            void SubscribeParentEvents() {
                if(ParentWindow != null) {
                    ParentWindow.LocationChanged += OnParentSizeOrPositionChanged;
                    ParentWindow.SizeChanged += OnParentSizeOrPositionChanged;
                    ParentWindow.Closed += OnParentClosed;
                }

                if(childLocation == SplashScreenLocation.CenterContainer && ParentContainer != null && ParentWindow != ParentContainer) {
                    ParentContainer.SizeChanged += OnParentSizeOrPositionChanged;
                    ParentContainer.LayoutUpdated += OnParentSizeOrPositionChanged;
                }
            }
            void UnsubscribeParentEvents() {
                if(ParentWindow != null) {
                    ParentWindow.LocationChanged -= OnParentSizeOrPositionChanged;
                    ParentWindow.SizeChanged -= OnParentSizeOrPositionChanged;
                    ParentWindow.Closed -= OnParentClosed;
                }

                if(childLocation == SplashScreenLocation.CenterContainer && ParentContainer != null && ParentWindow != ParentContainer) {
                    try {
                        ParentContainer.SizeChanged -= OnParentSizeOrPositionChanged;
                        ParentContainer.LayoutUpdated -= OnParentSizeOrPositionChanged;
                    } catch { }
                }
            }
            void OnParentSizeOrPositionChanged(object sender, EventArgs e) {
                if(childWindow == null)
                    return;

                UpdateNextParentRectAndChildPosition(false);
            }
            void OnParentClosed(object sender, EventArgs e) {
                isParentClosed = true;
                RaiseParentCloseEvent();
            }
            #endregion

            #region Event
            public event EventHandler ParentClosed;
            void RaiseParentCloseEvent() {
                ParentClosed.Do(x => x(this, EventArgs.Empty));
            }
            #endregion
        }
        class SplashScreenOwner {
            #region Props
            public FrameworkElement Control { get; private set; }
            public Window Window { get; private set; }
            public IntPtr WindowHandle { get; private set; }
            public bool IsInitialized { get; private set; }
            public Rect ControlStartupPosition { get; private set; }
            public Rect WindowStartupPosition { get; private set; }
            #endregion

            public SplashScreenOwner(FrameworkElement control) {
                if(control == null)
                    throw new ArgumentNullException("control");

                Control = control;
                Initialize();
            }

            #region Initialize
            void Initialize() {
                if(IsInitialized)
                    return;

                if(!Control.IsLoaded) {
                    Control.Loaded += OnControlLoaded;
                    return;
                }

                InvokeOnThread(Control.Dispatcher, CompleteInitialization, DispatcherPriority.Normal, false);
            }
            void CompleteInitialization() {
                if(IsInitialized)
                    return;

                HwndSource hwnd = (HwndSource)HwndSource.FromVisual(Control);
                Window = hwnd.RootVisual as Window;
                WindowHandle = hwnd.Handle;

                ControlStartupPosition = GetControlRect();
                WindowStartupPosition = GetWindowRect();
                IsInitialized = true;
                RaiseInitialized();
            }
            void OnControlLoaded(object sender, RoutedEventArgs e) {
                Control.Loaded -= OnControlLoaded;
                Initialize();
            }
            #endregion

            #region Location
            public Rect GetWindowRect() {
                return Window == null || !Window.IsLoaded ? Rect.Empty : LayoutHelper.GetScreenRect(Window);
            }
            public Rect GetControlRect() {
                return Control == null || !Control.IsLoaded || PresentationSource.FromVisual(Control) == null ? Rect.Empty : LayoutHelper.GetScreenRect(Control);
            }
            #endregion

            #region Event
            public event EventHandler Initialized;
            void RaiseInitialized() {
                Initialized.Do(x => x(this, EventArgs.Empty));
            }
            #endregion
        }
    }
    public enum SplashScreenLocation {
        CenterContainer,
        CenterWindow
    }
}