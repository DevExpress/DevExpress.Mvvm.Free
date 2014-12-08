using DevExpress.Mvvm;
using DevExpress.Mvvm.Native;
using DevExpress.Mvvm.UI;
using System;
using System.ComponentModel;
using System.Threading;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;

namespace DevExpress.Mvvm.UI {
    public static class DXSplashScreen {
        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public static Func<object, Window> DefaultSplashScreenWindowCreator {
            get { return new Func<object, Window>(CreateDefaultSplashScreenWindow); }
        }
        static Window CreateDefaultSplashScreenWindow(object parameter) {
            return new Window() {
                WindowStyle = WindowStyle.None,
                ResizeMode = ResizeMode.NoResize,
                AllowsTransparency = true,
                Background = new SolidColorBrush(Colors.Transparent),
                ShowInTaskbar = false,
                Topmost = true,
                SizeToContent = SizeToContent.WidthAndHeight,
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
            };
        }
        static object CreateDefaultSplashScreen(object parameter) {
            object[] parameters = (object[])parameter;
            Type splashScreenType = parameters[0] as Type;
            SplashScreenViewModel splashScreenViewModel = new SplashScreenViewModel();
            object view = null;
            if(splashScreenType != null) {
                view = Activator.CreateInstance(splashScreenType);
                view.With(x => x as FrameworkElement).Do(x => x.DataContext = splashScreenViewModel);
            }
            return view;
        }

        public static bool IsActive { get { return SplashContainer != null ? SplashContainer.IsActive : false; } }

        public static readonly DependencyProperty SplashScreenTypeProperty =
            DependencyProperty.RegisterAttached("SplashScreenType", typeof(Type), typeof(DXSplashScreen), new PropertyMetadata(null, OnSplashScreenTypeChanged));
        public static Type GetSplashScreenType(Window obj) {
            return (Type)obj.GetValue(SplashScreenTypeProperty);
        }
        public static void SetSplashScreenType(Window obj, Type value) {
            obj.SetValue(SplashScreenTypeProperty, value);
        }
        internal static void OnSplashScreenTypeChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e) {
            if(DesignerProperties.GetIsInDesignMode(obj))
                return;
            Window owner = (Window)obj;
            Type type = (Type)e.NewValue;
            Show(type);
            owner.Dispatcher.BeginInvoke(new Action(Close), DispatcherPriority.Loaded);
            owner.Dispatcher.BeginInvoke(new Func<bool>(owner.Activate), DispatcherPriority.Loaded);
        }

        public static void Show<T>(Action action) {
            Show<T>();
            try {
                action();
            } finally {
                Close();
            }
        }
        public static void Show(Func<object, Window> windowCreator, Func<object, object> splashScreenCreator, object windowCreatorParameter, object splashScreenCreatorParameter) {
            if(IsActive) throw new InvalidOperationException(DXSplashScreenExceptions.Exception1);
            if(windowCreator == null)
                windowCreator = DefaultSplashScreenWindowCreator;
            SplashContainer = new SplashScreenContainer();
            SplashContainer.Show(windowCreator, splashScreenCreator, windowCreatorParameter, splashScreenCreatorParameter);
        }
        public static void Show<T>() {
            Show(typeof(T));
        }
        public static void Show(Type splashScreenType) {
            if(typeof(Window).IsAssignableFrom(splashScreenType) && !typeof(ISplashScreen).IsAssignableFrom(splashScreenType))
                throw new InvalidOperationException(DXSplashScreenExceptions.Exception2);
            if(typeof(Window).IsAssignableFrom(splashScreenType) && typeof(ISplashScreen).IsAssignableFrom(splashScreenType)) {
                Func<object, Window> windowCreator = (p) => {
                    Type type = (Type)p;
                    Window splashWindow = (Window)Activator.CreateInstance(type);
                    splashWindow.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                    return splashWindow;
                };
                Func<object, object> splashScreenCreator = null;
                object windowCreatorParameter = splashScreenType;
                object splashScreenCreatorParameter = null;
                Show(windowCreator, splashScreenCreator, windowCreatorParameter, splashScreenCreatorParameter);
                return;
            }
            if(typeof(FrameworkElement).IsAssignableFrom(splashScreenType)) {
                Func<object, Window> splashScreenWindowCreator = (p) => {
                    Window res = DefaultSplashScreenWindowCreator(null);
                    WindowFadeAnimationBehavior.SetEnableAnimation(res, true);
                    return res;
                };
                Show(splashScreenWindowCreator, CreateDefaultSplashScreen,
                    new object[] { }, new object[] { splashScreenType });
                return;
            }
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

        internal static SplashScreenContainer SplashContainer = null;

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public class SplashScreenContainer {
            internal volatile Window SplashScreen = null;
            internal volatile Thread InternalThread = null;
            internal volatile AutoResetEvent SyncEvent = new AutoResetEvent(false);
            internal volatile AutoResetEvent EndSyncEvent = new AutoResetEvent(false);
            public bool IsActive { get { return InternalThread != null; } }

            public void Show(Func<object, Window> windowCreator, Func<object, object> splashScreenCreator, object windowCreatorParameter, object splashScreenCreatorParameter) {
                if(ViewModelBase.IsInDesignMode) return;
                if(IsActive) throw new InvalidOperationException(DXSplashScreenExceptions.Exception1);
                InternalThread = new Thread(InternalThreadEntryPoint);
                InternalThread.SetApartmentState(ApartmentState.STA);
                InternalThread.Start(new object[] { windowCreator, splashScreenCreator, windowCreatorParameter, splashScreenCreatorParameter });
                SyncEvent.WaitOne();
            }
            public void Close() {
                if(ViewModelBase.IsInDesignMode) return;
                if(!IsActive)
                    throw new InvalidOperationException(DXSplashScreenExceptions.Exception3);
                Action closeAction = () => {
                    if(SplashScreen is ISplashScreen) {
                        ((ISplashScreen)SplashScreen).CloseSplashScreen();
                        return;
                    }
                    SplashScreen.Close();
                };
                SplashScreen.Dispatcher.BeginInvoke(closeAction);
                EndSyncEvent.WaitOne();
            }
            public void Progress(double value, double maxValue) {
                if(ViewModelBase.IsInDesignMode) return;
                if(!IsActive)
                    throw new InvalidOperationException(DXSplashScreenExceptions.Exception3);
                Action<double, double> progressCore = (v1, v2) => {
                    if(SplashScreen is ISplashScreen) {
                        ((ISplashScreen)SplashScreen).Progress(v1);
                        SetProgressStateCore(false);
                    } else if(SplashScreen.Content is FrameworkElement) {
                        ((FrameworkElement)SplashScreen.Content).DataContext.With(x => x as SplashScreenViewModel).Do(x => {
                            x.Progress = v1; x.MaxProgress = v2;
                        });
                    }
                };
                SplashScreen.Dispatcher.BeginInvoke(progressCore, new object[] { value, maxValue });
            }
            public void SetState(object state) {
                if(ViewModelBase.IsInDesignMode) return;
                if(!DXSplashScreen.IsActive)
                    throw new InvalidOperationException(DXSplashScreenExceptions.ServiceException2);
                Action<object> setStateCore = (p) => {
                    if(SplashScreen is ISplashScreen)
                        return;
                    if(SplashScreen.Content is FrameworkElement) {
                        SplashScreenViewModel splashScreenViewModel = ((FrameworkElement)SplashScreen.Content).DataContext as SplashScreenViewModel;
                        if(splashScreenViewModel == null)
                            return;
                        splashScreenViewModel.State = p;
                        return;
                    }
                };
                SplashScreen.Dispatcher.BeginInvoke(setStateCore, state);
            }
            public void CallSplashScreenMethod<T>(Action<T> action) {
                if(ViewModelBase.IsInDesignMode) return;
                if(!IsActive)
                    throw new InvalidOperationException(DXSplashScreenExceptions.Exception3);
                if(!(SplashScreen is ISplashScreen))
                    throw new InvalidOperationException(DXSplashScreenExceptions.Exception4);
                if(!typeof(T).IsAssignableFrom(SplashScreen.GetType()))
                    throw new InvalidOperationException(DXSplashScreenExceptions.Exception5);
                SplashScreen.Dispatcher.BeginInvoke(action, SplashScreen);
            }
            void InternalThreadEntryPoint(object parameter) {
                Func<object, Window> windowCreator = ((object[])parameter)[0] as Func<object, Window>;
                Func<object, object> splashScreenCreator = ((object[])parameter)[1] as Func<object, object>;
                object windowCreatorParameter = ((object[])parameter)[2];
                object splashScreenCreatorParameter = ((object[])parameter)[3];

                SplashScreen = windowCreator(windowCreatorParameter);
                splashScreenCreator.Do(x => SplashScreen.Content = x(splashScreenCreatorParameter));
                SetProgressStateCore(true);
                SyncEvent.Set();
                SplashScreen.ShowDialog();
                ReleaseResources();
            }
            void ReleaseResources() {
                SplashScreen.Content = null;
                SplashScreen = null;
                InternalThread = null;
                isIndeterminateCore = null;
                Dispatcher.CurrentDispatcher.InvokeShutdown();
                EndSyncEvent.Set();
            }

            bool? isIndeterminateCore = null;
            void SetProgressStateCore(bool isIndeterminate) {
                if(!(SplashScreen is ISplashScreen)) return;
                if(isIndeterminate != isIndeterminateCore) {
                    ((ISplashScreen)SplashScreen).SetProgressState(isIndeterminate);
                    isIndeterminateCore = isIndeterminate;
                }
            }
        }
    }
}