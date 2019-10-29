using DevExpress.Mvvm.Native;
using System;
using System.Collections;
using System.ComponentModel;
using System.Threading;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;
using System.Collections.Generic;
using System.Windows.Interop;
using System.Linq;

namespace DevExpress.Mvvm.UI {
    public static class DXSplashScreen {
        public static bool UseLegacyLocationLogic { get; set; }
        public static bool UseDefaultAltTabBehavior { get; set; }
        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public static bool DisableThreadingProblemsDetection { get; set; }
        public static NotInitializedStateMethodCallPolicy NotInitializedStateMethodCallPolicy { get; set; }
        public static UIThreadReleaseMode UIThreadReleaseMode { get; set; }
        public static int UIThreadDelay {
            get { return MainThreadDelay; }
            set { MainThreadDelay = value; }
        }
        static int MainThreadDelay { get; set; }

        static DXSplashScreen() {
            MainThreadDelay = 700;
            DisableThreadingProblemsDetection = false;
        }

        public static readonly DependencyProperty SplashScreenTypeProperty =
            DependencyProperty.RegisterAttached("SplashScreenType", typeof(Type), typeof(DXSplashScreen), new PropertyMetadata(null, OnSplashScreenTypeChanged));
        public static Type GetSplashScreenType(Window obj) {
            return (Type)obj.GetValue(SplashScreenTypeProperty);
        }
        public static void SetSplashScreenType(Window obj, Type value) {
            obj.SetValue(SplashScreenTypeProperty, value);
        }

        public static bool IsActive { get { return SplashContainer != null && SplashContainer.IsActive; } }
        internal static SplashScreenContainer SplashContainer = null;

        internal static void OnSplashScreenTypeChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e) {
            if(DesignerProperties.GetIsInDesignMode(obj))
                return;
            Window owner = (Window)obj;
            Type type = (Type)e.NewValue;
            Show(type, WindowStartupLocation.CenterScreen, owner.With(x => new SplashScreenOwner(x)));
            owner.Dispatcher.BeginInvoke(new Action(Close), DispatcherPriority.Loaded);
        }
        public static void Show<T>(Action action, WindowStartupLocation startupLocation = WindowStartupLocation.CenterScreen, SplashScreenOwner owner = null, SplashScreenClosingMode closingMode = SplashScreenClosingMode.Default) {
            Show<T>(startupLocation, owner, closingMode);
            try {
                action();
            } finally {
                Close();
            }
        }
        public static void Show(Func<object, Window> windowCreator, Func<object, object> splashScreenCreator, object windowCreatorParameter, object splashScreenCreatorParameter) {
            if(IsActive)
                throw new InvalidOperationException(DXSplashScreenExceptions.Exception1);
            if(SplashContainer == null) {
                SplashContainer = new SplashScreenContainer();
                SplashContainer.Closed += OnSplashScreenClosed;
            }
            SplashContainer.Show(windowCreator ?? DefaultSplashScreenWindowCreator, splashScreenCreator, windowCreatorParameter, splashScreenCreatorParameter, NotifyIsActiveChanged);
        }
        public static void Show<T>(WindowStartupLocation startupLocation = WindowStartupLocation.CenterScreen, SplashScreenOwner owner = null, SplashScreenClosingMode closingMode = SplashScreenClosingMode.Default) {
            Show(typeof(T), startupLocation, owner, closingMode);
        }
        public static void Show(Type splashScreenType, WindowStartupLocation startupLocation = WindowStartupLocation.CenterScreen, SplashScreenOwner owner = null, SplashScreenClosingMode closingMode = SplashScreenClosingMode.Default) {
            CheckSplashScreenType(splashScreenType);
            if(typeof(Window).IsAssignableFrom(splashScreenType)) {
                Func<object, Window> windowCreator = (p) => {
                    Window splashWindow = (Window)Activator.CreateInstance(SplashScreenHelper.FindParameter<Type>(p));
                    splashWindow.WindowStartupLocation = SplashScreenHelper.FindParameter(p, WindowStartupLocation.CenterScreen);
                    return splashWindow;
                };
                Show(windowCreator, null, new object[] { splashScreenType, startupLocation, owner, closingMode }, null);
            } else if(typeof(FrameworkElement).IsAssignableFrom(splashScreenType)) {
                Func<object, Window> windowCreator = (p) => {
                    Window res = DefaultSplashScreenWindowCreator(p);
                    WindowFadeAnimationBehavior.SetEnableAnimation(res, true);
                    return res;
                };
                Show(windowCreator, CreateDefaultSplashScreen, new object[] { startupLocation, owner, closingMode }, new object[] { splashScreenType });
            }
        }
        internal static void CheckSplashScreenType(Type splashScreenType) {
            if(typeof(Window).IsAssignableFrom(splashScreenType) && !typeof(ISplashScreen).IsAssignableFrom(splashScreenType))
                throw new InvalidOperationException(string.Format(DXSplashScreenExceptions.Exception6, splashScreenType.Name));
            if(!typeof(FrameworkElement).IsAssignableFrom(splashScreenType))
                throw new InvalidOperationException(DXSplashScreenExceptions.Exception2);
        }
        public static void Close() {
            if(!IsActive)
                throw new InvalidOperationException(DXSplashScreenExceptions.Exception3);
            SplashContainer.Close();
            NotifyIsActiveChanged();
        }
        public static void Progress(double value, double maxValue = 100d) {
            if(!IsActive)
                throw new InvalidOperationException(DXSplashScreenExceptions.Exception3);
            SplashContainer.Progress(value, maxValue);
        }
        public static void SetState(object state) {
            if(!IsActive)
                throw new InvalidOperationException(DXSplashScreenExceptions.Exception3);
            SplashContainer.SetState(state);
        }
        public static void CallSplashScreenMethod<T>(Action<T> action) {
            if(!IsActive)
                throw new InvalidOperationException(DXSplashScreenExceptions.Exception3);
            SplashContainer.CallSplashScreenMethod<T>(action);
        }
        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public static Func<object, Window> DefaultSplashScreenWindowCreator {
            get { return new Func<object, Window>(CreateDefaultSplashScreenWindow); }
        }
        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public static Func<object, object> DefaultSplashScreenContentCreator {
            get { return new Func<object, object>(CreateDefaultSplashScreen); }
        }

        static Window CreateDefaultSplashScreenWindow(object parameter) {
            return new SplashScreenWindow() {
                WindowStyle = WindowStyle.None,
                ResizeMode = ResizeMode.NoResize,
                AllowsTransparency = true,
                Background = new SolidColorBrush(Colors.Transparent),
                ShowInTaskbar = false,
                Topmost = true,
                SizeToContent = SizeToContent.WidthAndHeight,
                WindowStartupLocation = SplashScreenHelper.FindParameter(parameter, WindowStartupLocation.CenterScreen)
            };
        }
        static object CreateDefaultSplashScreen(object parameter) {
            var splashScreenType = SplashScreenHelper.FindParameter<Type>(parameter);
            var model = SplashScreenHelper.FindParameter<SplashScreenViewModel>(parameter);
            model = model == null ? new SplashScreenViewModel() : model.Clone();
            object view = null;
            if(splashScreenType != null) {
                view = Activator.CreateInstance(splashScreenType);
                view.With(x => x as FrameworkElement).Do(x => x.DataContext = model);
            }
            return view;
        }

        #region Notify IsActiveChanged
        static bool lastIsActive = false;
        static void OnSplashScreenClosed(object container, EventArgs args) {
            NotifyIsActiveChanged();
        }
        static void NotifyIsActiveChanged() {
            bool newIsActive = IsActive;
            if(lastIsActive == newIsActive)
                return;

            lastIsActive = newIsActive;
            var services = WeakSplashScreenStateAwareContainer.Default.GetRegisteredServices();
            foreach(var service in services)
                service.OnIsActiveChanged(newIsActive);
        }
        #endregion

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public class SplashScreenContainer {
            static IList<SplashScreenContainer> instances;
            static object instanceLocker;

            static SplashScreenContainer() {
                instanceLocker = new object();
                instances = new List<SplashScreenContainer>();
            }
            static void OnAppUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e) {
                SplashScreenContainer[] instancesCopy;
                lock(instanceLocker) {
                    if(instances.Count == 0)
                        return;
                    instancesCopy = new SplashScreenContainer[instances.Count];
                    instances.CopyTo(instancesCopy, 0);
                }

                foreach(SplashScreenContainer instance in instancesCopy) {
                    if(instance.IsActive)
                        instance.Close();
                }
            }

            internal EventHandler Closed;
            static bool hasUnhandledExceptionSubscriver = false;
            internal volatile AutoResetEvent SyncEvent = new AutoResetEvent(false);
            internal SplashScreenInfo ActiveInfo = null;
            object internalLocker = new object();
            IList<SplashScreenInfo> infosForRelease = new List<SplashScreenInfo>();
#if DEBUGTEST || DEBUG
            internal SplashScreenInfo OldInfo = null;
            internal bool Test_SkipWindowOpen = false;
#endif
            public bool IsActive {
                get {
                    lock(internalLocker) {
                        return ActiveInfo.IsActive;
                    }
                }
            }

            public SplashScreenContainer() {
                ActiveInfo = new SplashScreenInfo();
            }

            public void Show(Func<object, Window> windowCreator, Func<object, object> splashScreenCreator, object windowCreatorParameter, object splashScreenCreatorParameter, Action beforeStartThreadAction) {
                if(ViewModelBase.IsInDesignMode)
                    return;
                if(IsActive)
                    throw new InvalidOperationException(DXSplashScreenExceptions.Exception1);
                lock(instanceLocker) {
                    instances.Add(this);
                    if(!hasUnhandledExceptionSubscriver) {
                        hasUnhandledExceptionSubscriver = true;
                        Application.Current.Do(x => x.DispatcherUnhandledException += OnAppUnhandledException);
                    }
                }
                ActiveInfo.EnsureCallbacksContainer();
                ActiveInfo.InternalThread = new Thread(InternalThreadEntryPoint);
                ActiveInfo.InternalThread.SetApartmentState(ApartmentState.STA);
                beforeStartThreadAction?.Invoke();
                ActiveInfo.InternalThread.Start(new object[] { windowCreator, splashScreenCreator, windowCreatorParameter, splashScreenCreatorParameter, GetOwnerContainer(windowCreatorParameter), ActiveInfo });
                if(MainThreadDelay > 0)
                    SyncEvent.WaitOne(MainThreadDelay);
                else if(MainThreadDelay < 0)
                    SyncEvent.WaitOne();
            }
            public void Close() {
                if(ViewModelBase.IsInDesignMode)
                    return;
                if(!IsActive)
                    throw new InvalidOperationException(DXSplashScreenExceptions.Exception3);
                var callbacks = ActiveInfo.Callbacks;
                if(callbacks == null)
                    return;

                if(!callbacks.PushCloseCallback())
                    InvokeOnSplashScreenDispatcher(new Action<SplashScreenInfo>(CloseCore), DispatcherPriority.Render, ActiveInfo);
                ChangeActiveContainer();
            }
            public void Progress(double value, double maxValue) {
                if(ViewModelBase.IsInDesignMode)
                    return;
                if(!IsActive)
                    throw new InvalidOperationException(DXSplashScreenExceptions.Exception3);

                if(!ActiveInfo.Callbacks.PushSetProgressCallback(value, maxValue))
                    InvokeOnSplashScreenDispatcher(new Action<SplashScreenInfo, double, double>(SetProgressCore), new object[] { ActiveInfo, value, maxValue });
            }
            public void SetState(object state) {
                if(ViewModelBase.IsInDesignMode)
                    return;
                if(!IsActive)
                    throw new InvalidOperationException(DXSplashScreenExceptions.ServiceException2);
                if(!ActiveInfo.Callbacks.PushSetStateCallback(state))
                    InvokeOnSplashScreenDispatcher(new Action<SplashScreenInfo, object>(SetStateCore), new object[] { ActiveInfo, state });
            }
            public void CallSplashScreenMethod<T>(Action<T> action) {
                if(ViewModelBase.IsInDesignMode)
                    return;
                if(!IsActive)
                    throw new InvalidOperationException(DXSplashScreenExceptions.Exception3);
                if(ActiveInfo.Callbacks.PushSplashScreenMethodCallback(action))
                    return;
                EnsureCallSplashScreenMethodParameter<T>(ActiveInfo.SplashScreen);
                InvokeOnSplashScreenDispatcher(action, ActiveInfo.SplashScreen);
            }
            void InvokeOnSplashScreenDispatcher(Delegate method, params object[] args) {
                InvokeOnSplashScreenDispatcher(method, DispatcherPriority.Normal, args);
            }
            void InvokeOnSplashScreenDispatcher(Delegate method, DispatcherPriority priority, params object[] args) {
                if(ActiveInfo == null)
                    return;

                var dispatcher = ActiveInfo.Dispatcher;
                if(dispatcher != null)
                    dispatcher.BeginInvoke(method, priority, args);
            }
            void InternalThreadEntryPoint(object parameter) {
                Func<object, Window> windowCreator = ((object[])parameter)[0] as Func<object, Window>;
                Func<object, object> splashScreenCreator = ((object[])parameter)[1] as Func<object, object>;
                object windowCreatorParameter = ((object[])parameter)[2];
                object splashScreenCreatorParameter = ((object[])parameter)[3];
                var lockMode = (UIThreadReleaseMode?)SplashScreenHelper.FindParameter(parameter, UIThreadReleaseMode);
                object syncRoot = ((ICollection)(new Style()).Resources).SyncRoot;
                var info = SplashScreenHelper.FindParameter<SplashScreenInfo>(parameter) ?? ActiveInfo;
#if DEBUGTEST || DEBUG
                if(info.WaitEvent != null)
                    info.WaitEvent.WaitOne(2000);
#endif
                info.SplashScreen = windowCreator(windowCreatorParameter);
                info.Dispatcher = info.SplashScreen.Dispatcher;
                info.Callbacks.Initialize();
                if(!Monitor.TryEnter(syncRoot)) {
                    lockMode = null;
                    SyncEvent.Set();
                } else
                    Monitor.Exit(syncRoot);

                splashScreenCreator.Do(x => info.SplashScreen.Content = x(splashScreenCreatorParameter));
                SetProgressStateCore(info, true);
                info.InitializeOwner(parameter);
                SubscribeParentEvents(windowCreatorParameter);
                if(lockMode.HasValue && lockMode.Value == UIThreadReleaseMode.WaitForSplashScreenInitialized)
                    SyncEvent.Set();

                info.Callbacks.ExecuteExceptClose();
                bool skipOpen = info.CloseWithParent && info.RelationInfo.Return(x => x.ActualIsParentClosed, () => false);
#if DEBUGTEST || DEBUG
                Test_SkipWindowOpen = skipOpen;
#endif
                bool unlockRequired = lockMode.HasValue && lockMode.Value == UIThreadReleaseMode.WaitForSplashScreenLoaded;
                if(!skipOpen) {
                    if(unlockRequired)
                        info.SplashScreen.Loaded += OnSplashScreenLoaded;
                    PatchSplashScreenWindowStyle(info.SplashScreen, info.Owner != null);
                    info.Callbacks.ExecuteClose();
                    info.SplashScreen.ShowDialog();
                    if(unlockRequired)
                        info.SplashScreen.Loaded -= OnSplashScreenLoaded;
                    info.ActivateOwner();
                } else if(unlockRequired)
                    SyncEvent.Set();

                ReleaseResources(info);
            }

            object GetOwnerContainer(object parameter) {
                object result = SplashScreenHelper.FindParameter<WindowContainer>(parameter, null).Return(x => x.CreateOwnerContainer(), null)
                        ?? SplashScreenHelper.FindParameter<WindowRelationInfo>(parameter, null);

                if(result != null)
                    return result;

                WindowStartupLocation startupLocation = SplashScreenHelper.FindParameter(parameter, WindowStartupLocation.CenterScreen);
                return SplashScreenHelper.FindParameter<SplashScreenOwner>(parameter, null).With(x => x.CreateOwnerContainer(startupLocation));
            }
            void ReleaseResources(SplashScreenInfo container) {
                lock(instanceLocker) {
                    instances.Remove(this);
                    if(instances.Count == 0 && hasUnhandledExceptionSubscriver)
                        SplashScreenHelper.InvokeAsync(Application.Current, UnsubscribeUnhandledException, DispatcherPriority.Send, AsyncInvokeMode.AsyncOnly);
                }
                var dispatcher = Dispatcher.FromThread(container.InternalThread);
                container.RelationInfo.Do(x => x.ParentClosed -= OnSplashScreenOwnerClosed);
                container.ReleaseResources();
                if(Closed != null)
                    Closed.Invoke(this, EventArgs.Empty);
                lock(internalLocker) {
                    infosForRelease.Remove(container);
                }
                try {
                    if(dispatcher != null) {
                        dispatcher.BeginInvokeShutdown(DispatcherPriority.Normal);
                        Dispatcher.Run();
                    }
                } catch { }
            }
            void UnsubscribeUnhandledException() {
                lock(instanceLocker) {
                    if(instances.Count == 0 && hasUnhandledExceptionSubscriver) {
                        Application.Current.Do(x => x.DispatcherUnhandledException -= OnAppUnhandledException);
                        hasUnhandledExceptionSubscriver = false;
                    }
                }
            }
            void SubscribeParentEvents(object parameter) {
                SplashScreenClosingMode closingMode = SplashScreenHelper.FindParameter(parameter, SplashScreenClosingMode.Default);
                ActiveInfo.CloseWithParent = closingMode == SplashScreenClosingMode.Default || closingMode == SplashScreenClosingMode.OnParentClosed;
                if(ActiveInfo.CloseWithParent)
                    ActiveInfo.RelationInfo.Do(x => x.ParentClosed += OnSplashScreenOwnerClosed);
            }
            void PatchSplashScreenWindowStyle(Window splashScreen, bool hasOwner) {
                if(!SplashScreenHelper.PatchWindowStyle(splashScreen, hasOwner))
                    splashScreen.SourceInitialized += OnSplashScreenSourceInitialized;
            }
            void OnSplashScreenSourceInitialized(object sender, EventArgs e) {
                Window window = (Window)sender;
                window.SourceInitialized -= OnSplashScreenSourceInitialized;
                PatchSplashScreenWindowStyle(window, ActiveInfo.Return(x => x.Owner != null, () => false));
            }
            void OnSplashScreenLoaded(object sender, RoutedEventArgs e) {
                SyncEvent.Set();
            }
            void OnSplashScreenOwnerClosed(object sender, EventArgs e) {
                if(IsActive)
                    Close();
            }
            void ChangeActiveContainer() {
                lock(internalLocker) {
                    var oldContainer = ActiveInfo;
                    ActiveInfo = new SplashScreenInfo();
                    infosForRelease.Add(oldContainer);
#if DEBUGTEST || DEBUG
                    OldInfo = oldContainer;
#endif
                }
            }

            internal static void EnsureCallSplashScreenMethodParameter<T>(Window parameter) {
                if(!(parameter is ISplashScreen))
                    throw new InvalidOperationException(DXSplashScreenExceptions.Exception4);
                if(!typeof(T).IsAssignableFrom(parameter.GetType()))
                    throw new InvalidOperationException(DXSplashScreenExceptions.Exception5);
            }
            static void SetProgressStateCore(SplashScreenInfo info, bool isIndeterminate) {
                if(!(info.SplashScreen is ISplashScreen))
                    return;
                if(isIndeterminate != info.IsIndeterminate) {
                    ((ISplashScreen)info.SplashScreen).SetProgressState(isIndeterminate);
                    info.IsIndeterminate = isIndeterminate;
                }
            }
            internal static void CloseCore(SplashScreenInfo info) {
                if(!info.IsActive)
                    return;

                if(info.SplashScreen is ISplashScreen)
                    ((ISplashScreen)info.SplashScreen).CloseSplashScreen();
                else
                    info.SplashScreen.Close();
            }
            internal static void SetProgressCore(SplashScreenInfo info, double progress, double maxProgress) {
                if(!info.IsActive)
                    return;

                if(info.SplashScreen is ISplashScreen) {
                    ((ISplashScreen)info.SplashScreen).Progress(progress);
                    SetProgressStateCore(info, false);
                } else
                    GetViewModel(info).Do(x => { x.Progress = progress; x.MaxProgress = maxProgress; });
            }
            internal static void SetStateCore(SplashScreenInfo info, object state) {
                if(!info.IsActive)
                    return;

                if(!(info.SplashScreen is ISplashScreen))
                    GetViewModel(info).Do(x => x.State = state);
            }
            static SplashScreenViewModel GetViewModel(SplashScreenInfo info) {
                return (info.SplashScreen.Content as FrameworkElement).With(x => x.DataContext as SplashScreenViewModel);
            }

            [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
            public class SplashScreenInfo {
                internal volatile Window SplashScreen = null;
                internal volatile Thread InternalThread = null;
                internal volatile WindowRelationInfo RelationInfo = null;
                internal volatile WindowContainer Owner = null;
                internal SplashScreenCallbacks Callbacks;
                internal bool? IsIndeterminate;
                internal bool CloseWithParent;
                internal volatile Dispatcher Dispatcher;
                public bool IsActive { get { return InternalThread != null; } }
#if DEBUGTEST || DEBUG
                internal volatile AutoResetEvent WaitEvent = null;
#endif

                public void EnsureCallbacksContainer() {
                    if(Callbacks == null)
                        Callbacks = new SplashScreenCallbacks(this);

                    Callbacks.Policy = NotInitializedStateMethodCallPolicy;
                }
                internal void ActivateOwner() {
                    if(SplashScreen.IsActive)
                        Owner.Do(x => x.ActivateWindow());
                }
                internal void ReleaseResources() {
                    Callbacks.Dispose();
                    Callbacks = null;
                    Owner = null;
                    Dispatcher = null;
                    RelationInfo.Do(x => x.Release());
                    RelationInfo = null;
                    SplashScreen.Content = null;
                    SplashScreen = null;
                    InternalThread = null;
#if DEBUGTEST || DEBUG
                    WaitEvent = null;
#endif
                }
                internal void InitializeOwner(object parameter) {
                    RelationInfo = SplashScreenHelper.FindParameter<WindowContainer>(parameter, null).Return(x => x.CreateOwnerContainer(), null)
                        ?? SplashScreenHelper.FindParameter<WindowRelationInfo>(parameter, null);
                    Owner = RelationInfo.With(x => x.Parent);
                    RelationInfo.Do(x => x.AttachChild(SplashScreen, false));
                }
            }
        }
        internal class SplashScreenWindow : Window {
            const int WM_SYSCOMMAND = 0x0112;
            const int SC_CLOSE = 0xF060;

            public bool IsActiveOnClosing { get; private set; }
            protected IntPtr Handle { get; set; }

            public SplashScreenWindow() {
                ShowActivated = false;
                Loaded += OnWindowLoaded;
            }

            protected override void OnSourceInitialized(EventArgs e) {
                base.OnSourceInitialized(e);
                if(!SplashScreenHelper.HasAccess(this))
                    return;

                Handle = new WindowInteropHelper(this).Handle;
                HwndSource.FromHwnd(Handle).AddHook(WndProc);
            }
            protected override void OnClosed(EventArgs e) {
                if(Handle != IntPtr.Zero)
                    HwndSource.FromHwnd(Handle).Do(x => x.RemoveHook(WndProc));
            }
            protected override void OnClosing(CancelEventArgs e) {
                IsActiveOnClosing = IsActive;
                base.OnClosing(e);
            }
            IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled) {
                if(msg == WM_SYSCOMMAND) {
                    int cmd = (wParam.ToInt32() & 0xfff0);
                    if(cmd == SC_CLOSE)
                        handled = true;
                }
                return IntPtr.Zero;
            }
            void OnWindowLoaded(object sender, RoutedEventArgs e) {
                Loaded -= OnWindowLoaded;
                if(SizeToContent != SizeToContent.Manual) {
                    SizeToContent oldValue = SizeToContent;
                    SizeToContent = SizeToContent.Manual;
                    SizeToContent = oldValue;
                }
            }
        }

        internal class SplashScreenCallbacks : IDisposable {
            IList<SplashScreenCallbackBase> callbacks = new List<SplashScreenCallbackBase>();
            public bool HasCloseCallback { get; private set; }
            object syncRoot = new object();
            SplashScreenContainer.SplashScreenInfo info;
            bool isInitialized;
            public NotInitializedStateMethodCallPolicy Policy { get; set; }

            public SplashScreenCallbacks(SplashScreenContainer.SplashScreenInfo info) {
                this.info = info;
            }

            public bool PushSetStateCallback(object state) {
                return AddCallbackCore(new SetStateCallback(state));
            }
            public bool PushSetProgressCallback(double progress, double maxProgress) {
                return AddCallbackCore(new SetProgressCallback(progress, maxProgress));
            }
            public bool PushCloseCallback() {
                HasCloseCallback = true;
                return AddCallbackCore(new CloseCallback());
            }
            public bool PushSplashScreenMethodCallback<T>(Action<T> action) {
                return AddCallbackCore(new SplashScreenMethodCallback<T>(action));
            }
            bool AddCallbackCore(SplashScreenCallbackBase callback) {
                bool result = false;
                bool executedExisted = false;
                lock(syncRoot) {
                    if(!isInitialized) {
                        if(Policy == NotInitializedStateMethodCallPolicy.CallWhenReady) {
                            callbacks.Add(callback);
                            result = true;
                        } else if(Policy == NotInitializedStateMethodCallPolicy.Discard) {
                            callback.Dispose();
                            result = true;
                        } else
                            throw new InvalidOperationException("The SplashScreen has not been initialized yet.");
                    } else {
                        callback.Dispose();
                        executedExisted = callbacks.Count > 0 && info.Dispatcher != null;
                    }
                }
                if(executedExisted)
                    info.Dispatcher.BeginInvoke(DispatcherPriority.Loaded, new Action(ExecuteExceptClose));

                return result;
            }

            public void Initialize() {
                lock(syncRoot) {
                    isInitialized = true;
                }
            }
            public void ExecuteExceptClose() {
                lock(syncRoot) {
                    while(callbacks.Count > 0) {
                        var callback = callbacks[0];
                        if(callback is CloseCallback)
                            break;

                        callbacks.RemoveAt(0);
                        callback.Execute(info);
                        callback.Dispose();
                    }
                }
            }
            public void ExecuteClose() {
                if(!HasCloseCallback || callbacks.Count == 0)
                    return;

                var dispatcher = info.Dispatcher;
                if(dispatcher != null)
                    dispatcher.BeginInvoke(new Action<SplashScreenContainer.SplashScreenInfo>(ExecuteCloseCore), DispatcherPriority.Loaded, info);
            }
            void ExecuteCloseCore(SplashScreenContainer.SplashScreenInfo info) {
                lock(syncRoot) {
                    foreach(var callback in callbacks) {
                        callback.Execute(info);
                        callback.Dispose();
                    }
                    callbacks.Clear();
                }
            }

            public void Dispose() {
                foreach(var callback in callbacks)
                    callback.Dispose();

                callbacks.Clear();
                info = null;
            }
        }
        abstract class SplashScreenCallbackBase : IDisposable {
            public abstract void Execute(SplashScreenContainer.SplashScreenInfo info);
            public virtual void Dispose() { }
        }
        class SetStateCallback : SplashScreenCallbackBase {
            object state;
            public SetStateCallback(object state) {
                this.state = state;
            }
            public override void Execute(SplashScreenContainer.SplashScreenInfo info) {
                SplashScreenContainer.SetStateCore(info, state);
            }
            public override void Dispose() {
                state = null;
            }
        }
        class SetProgressCallback : SplashScreenCallbackBase {
            double progress;
            double maxProgress;
            public SetProgressCallback(double progress, double maxProgress) {
                this.progress = progress;
                this.maxProgress = maxProgress;
            }
            public override void Execute(SplashScreenContainer.SplashScreenInfo info) {
                SplashScreenContainer.SetProgressCore(info, progress, maxProgress);
            }
        }
        class CloseCallback : SplashScreenCallbackBase {
            public override void Execute(SplashScreenContainer.SplashScreenInfo info) {
                SplashScreenContainer.CloseCore(info);
            }
        }
        class SplashScreenMethodCallback<T> : SplashScreenCallbackBase {
            Action<T> callback;
            public SplashScreenMethodCallback(Action<T> callback) {
                this.callback = callback;
            }
            public override void Execute(SplashScreenContainer.SplashScreenInfo info) {
                SplashScreenContainer.EnsureCallSplashScreenMethodParameter<T>(info.SplashScreen);
                object window = info.SplashScreen;
                callback.Invoke((T)window);
            }
            public override void Dispose() {
                callback = null;
            }
        }

        internal class WeakSplashScreenStateAwareContainer {
            internal static WeakSplashScreenStateAwareContainer Default => instance ?? (instance = new WeakSplashScreenStateAwareContainer());
            static WeakSplashScreenStateAwareContainer instance;
            List<WeakReference> services = new List<WeakReference>();

            WeakSplashScreenStateAwareContainer() { }

            public void Register(ISplashScreenStateAware service) {
                services.Add(new WeakReference(service));
            }
            public void Unregister(ISplashScreenStateAware service) {
                var weakRef = services.FirstOrDefault(x => x.Target == service);
                if(weakRef != null)
                    services.Remove(weakRef);
            }

            public IList<ISplashScreenStateAware> GetRegisteredServices() {
                var toRemove = new List<WeakReference>();
                var result = new List<ISplashScreenStateAware>();
                foreach(var serviceRef in services) {
                    var service = serviceRef.Target as ISplashScreenStateAware;
                    if(service == null)
                        toRemove.Add(serviceRef);
                    else
                        result.Add(service);
                }

                toRemove.ForEach(x => services.Remove(x));
                return result;
            }
        }
        internal interface ISplashScreenStateAware {
            void OnIsActiveChanged(bool newValue);
        }
    }
}