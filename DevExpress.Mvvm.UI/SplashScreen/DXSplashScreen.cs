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

namespace DevExpress.Mvvm.UI {
    public static class DXSplashScreen {
        public static bool UseLegacyLocationLogic { get; set; }
        public static bool UseDefaultAltTabBehavior { get; set; }

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
            if(SplashContainer == null)
                SplashContainer = new SplashScreenContainer();
            SplashContainer.Show(windowCreator ?? DefaultSplashScreenWindowCreator, splashScreenCreator, windowCreatorParameter, splashScreenCreatorParameter);
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
            var splashScreenViewModel = new SplashScreenViewModel();
            object view = null;
            if(splashScreenType != null) {
                view = Activator.CreateInstance(splashScreenType);
                view.With(x => x as FrameworkElement).Do(x => x.DataContext = splashScreenViewModel);
            }
            return view;
        }

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public class SplashScreenContainer {
            static IList<SplashScreenContainer> instances;
            static object instanceLocker;

            static SplashScreenContainer() {
                instanceLocker = new object();
                instances = new List<SplashScreenContainer>();
                Application.Current.Do(x => x.DispatcherUnhandledException += OnAppUnhandledException);
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

            public void Show(Func<object, Window> windowCreator, Func<object, object> splashScreenCreator, object windowCreatorParameter, object splashScreenCreatorParameter) {
                if(ViewModelBase.IsInDesignMode)
                    return;
                if(IsActive)
                    throw new InvalidOperationException(DXSplashScreenExceptions.Exception1);
                lock(instanceLocker) {
                    instances.Add(this);
                }
                ActiveInfo.InternalThread = new Thread(InternalThreadEntryPoint);
                ActiveInfo.InternalThread.SetApartmentState(ApartmentState.STA);
                ActiveInfo.InternalThread.Start(new object[] { windowCreator, splashScreenCreator, windowCreatorParameter, splashScreenCreatorParameter, GetOwnerContainer(windowCreatorParameter) });
                SyncEvent.WaitOne();
            }
            public void Close() {
                if(ViewModelBase.IsInDesignMode)
                    return;
                if(!IsActive)
                    throw new InvalidOperationException(DXSplashScreenExceptions.Exception3);
                ActiveInfo.SplashScreen.Dispatcher.BeginInvoke(new Action<SplashScreenInfo>(CloseCore), ActiveInfo);
                ChangeActiveContainer();
            }
            public void Progress(double value, double maxValue) {
                if(ViewModelBase.IsInDesignMode)
                    return;
                if(!IsActive)
                    throw new InvalidOperationException(DXSplashScreenExceptions.Exception3);

                ActiveInfo.SplashScreen.Dispatcher.BeginInvoke(new Action<SplashScreenInfo, double, double>(SetProgressCore), new object[] { ActiveInfo, value, maxValue });
            }
            public void SetState(object state) {
                if(ViewModelBase.IsInDesignMode)
                    return;
                if(!IsActive)
                    throw new InvalidOperationException(DXSplashScreenExceptions.ServiceException2);
                ActiveInfo.SplashScreen.Dispatcher.BeginInvoke(new Action<SplashScreenInfo, object>(SetStateCore), new object[] { ActiveInfo, state });
            }
            public void CallSplashScreenMethod<T>(Action<T> action) {
                if(ViewModelBase.IsInDesignMode)
                    return;
                if(!IsActive)
                    throw new InvalidOperationException(DXSplashScreenExceptions.Exception3);
                if(!(ActiveInfo.SplashScreen is ISplashScreen))
                    throw new InvalidOperationException(DXSplashScreenExceptions.Exception4);
                if(!typeof(T).IsAssignableFrom(ActiveInfo.SplashScreen.GetType()))
                    throw new InvalidOperationException(DXSplashScreenExceptions.Exception5);
                ActiveInfo.SplashScreen.Dispatcher.BeginInvoke(action, ActiveInfo.SplashScreen);
            }
            void InternalThreadEntryPoint(object parameter) {
                Func<object, Window> windowCreator = ((object[])parameter)[0] as Func<object, Window>;
                Func<object, object> splashScreenCreator = ((object[])parameter)[1] as Func<object, object>;
                object windowCreatorParameter = ((object[])parameter)[2];
                object splashScreenCreatorParameter = ((object[])parameter)[3];
                object syncRoot = ((ICollection)(new Style()).Resources).SyncRoot;
                var info = ActiveInfo;
                info.SplashScreen = windowCreator(windowCreatorParameter);
                bool isResourcesLocked = Monitor.TryEnter(syncRoot);
                if(!isResourcesLocked)
                    SyncEvent.Set();
                else
                    Monitor.Exit(syncRoot);

                splashScreenCreator.Do(x => info.SplashScreen.Content = x(splashScreenCreatorParameter));
                SetProgressStateCore(info, true);
                info.InitializeOwner(parameter);
                SubscribeParentEvents(windowCreatorParameter);
                if(isResourcesLocked)
                    SyncEvent.Set();
                bool skipOpen = info.CloseWithParent && info.RelationInfo.Return(x => x.ActualIsParentClosed, () => false);
#if DEBUGTEST || DEBUG
                Test_SkipWindowOpen = skipOpen;
#endif
                if(!skipOpen) {
                    PatchSplashScreenWindowStyle(info.SplashScreen, info.Owner != null);
                    info.SplashScreen.ShowDialog();
                    info.ActivateOwner();
                }
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
                }
                container.RelationInfo.Do(x => x.ParentClosed -= OnSplashScreenOwnerClosed);
                container.ReleaseResources();
                lock(internalLocker) {
                    infosForRelease.Remove(container);
                }
                Dispatcher.CurrentDispatcher.InvokeShutdown();
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

            static void SetProgressStateCore(SplashScreenInfo info, bool isIndeterminate) {
                if(!(info.SplashScreen is ISplashScreen))
                    return;
                if(isIndeterminate != info.IsIndeterminate) {
                    ((ISplashScreen)info.SplashScreen).SetProgressState(isIndeterminate);
                    info.IsIndeterminate = isIndeterminate;
                }
            }
            static void CloseCore(SplashScreenInfo info) {
                if(!info.IsActive)
                    return;

                if(info.SplashScreen is ISplashScreen)
                    ((ISplashScreen)info.SplashScreen).CloseSplashScreen();
                else
                    info.SplashScreen.Close();
            }
            static void SetProgressCore(SplashScreenInfo info, double progress, double maxProgress) {
                if(!info.IsActive)
                    return;

                if(info.SplashScreen is ISplashScreen) {
                    ((ISplashScreen)info.SplashScreen).Progress(progress);
                    SetProgressStateCore(info, false);
                } else
                    GetViewModel(info).Do(x => { x.Progress = progress; x.MaxProgress = maxProgress; });
            }
            static void SetStateCore(SplashScreenInfo info, object state) {
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
                internal bool? IsIndeterminate;
                internal bool CloseWithParent;
                public bool IsActive { get { return InternalThread != null; } }

                internal void ActivateOwner() {
                    if(SplashScreen.IsActive)
                        Owner.Do(x => x.ActivateWindow());
                }
                internal void ReleaseResources() {
                    Owner = null;
                    RelationInfo.Do(x => x.Release());
                    RelationInfo = null;
                    SplashScreen.Content = null;
                    SplashScreen = null;
                    InternalThread = null;
                }
                internal void InitializeOwner(object parameter) {
                    RelationInfo = SplashScreenHelper.FindParameter<WindowContainer>(parameter, null).Return(x => x.CreateOwnerContainer(), null)
                        ?? SplashScreenHelper.FindParameter<WindowRelationInfo>(parameter, null);
                    Owner = RelationInfo.With(x => x.Parent);
                    RelationInfo.Do(x => x.AttachChild(SplashScreen));
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
    }
}