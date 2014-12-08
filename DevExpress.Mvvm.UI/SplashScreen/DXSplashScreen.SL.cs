using System;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

#if FREE && SILVERLIGHT
using DevExpress.Xpf.Core;
#endif

namespace DevExpress.Mvvm.UI {
    public static class DXSplashScreen {
        public static bool IsActive { get { return SplashScreenWindow != null; } }
        public static bool UseFadeEffect = true;
        public static Task Show<T>() where T : UIElement, ISplashScreen, new() {
            return Show(typeof(T));
        }
        public static Task Show(Type splashScreenType, bool? useFadeEffect = null) {
            if(typeof(UIElement).IsAssignableFrom(splashScreenType) && typeof(ISplashScreen).IsAssignableFrom(splashScreenType)) {
                UIElement splashScreen = (UIElement)Activator.CreateInstance(splashScreenType);
                return Show(splashScreen, useFadeEffect);
            }
            throw new InvalidOperationException(DXSplashScreenExceptions.Exception2);
        }
        public static Task Show(UIElement splashScreen, bool? useFadeEffect = null) {
            if(IsActive)
                throw new InvalidOperationException(DXSplashScreenExceptions.Exception1);
            SplashScreen = splashScreen;
            SplashScreenWindow = new DXSplashScreenPopup(splashScreen, useFadeEffect.HasValue ? useFadeEffect.Value : UseFadeEffect);
            SetProgressStateCore(true);
            return ShowCore();
        }
        public static void Close() {
            if(!IsActive)
                throw new InvalidOperationException(DXSplashScreenExceptions.Exception3);
            SplashScreenWindow.Dispatcher.BeginInvoke(delegate {
                SplashScreenWindow.HideSplashScreen();
                ReleaseResources();
            });
        }
        public static void Progress(double value) {
            if(!IsActive)
                throw new InvalidOperationException(DXSplashScreenExceptions.Exception3);
            if(SplashScreen is ISplashScreen) {
                Action<double> progressCore = (v) => {
                    ((ISplashScreen)SplashScreen).Progress(v);
                    SetProgressStateCore(false);
                };
                SplashScreenWindow.Dispatcher.BeginInvoke(new Action<double>(progressCore), value);
            }
        }
        public static void CallSplashScreenMethod<T>(Action<T> action) {
            if(!IsActive)
                throw new InvalidOperationException(DXSplashScreenExceptions.Exception3);
            if(!(SplashScreen is ISplashScreen))
                throw new InvalidOperationException(DXSplashScreenExceptions.Exception4);
            if(!typeof(T).IsAssignableFrom(SplashScreen.GetType()))
                throw new InvalidOperationException(DXSplashScreenExceptions.Exception5);
            SplashScreen.Dispatcher.BeginInvoke(action, SplashScreen);
        }

        static Task ShowCore() {
            ShowCoreTask = new TaskCompletionSource<bool>();
            SplashScreenWindow.Loaded += OnSplashScreenWindowLoaded;
            SplashScreenWindow.ShowSplashScreen();
            return ShowCoreTask.Task;
        }
        static void OnSplashScreenWindowLoaded(object sender, EventArgs e) {
            SplashScreenWindow.Loaded -= OnSplashScreenWindowLoaded;
            ShowCoreTask.SetResult(true);
        }
        static void SetProgressStateCore(bool isIndeterminate) {
            if(!(SplashScreen is ISplashScreen)) return;
            if(isIndeterminate != isIndeterminateCore) {
                ((ISplashScreen)SplashScreen).SetProgressState(isIndeterminate);
                isIndeterminateCore = isIndeterminate;
            }
        }
        static void ReleaseResources() {
            isIndeterminateCore = null;
            SplashScreenWindow.ReleaseResources();
            SplashScreenWindow.Loaded -= OnSplashScreenWindowLoaded;
            SplashScreenWindow = null;
            SplashScreen = null;
            ShowCoreTask = null;
        }

        static UIElement SplashScreen = null;
        static DXSplashScreenPopup SplashScreenWindow = null;
        static TaskCompletionSource<bool> ShowCoreTask;
        static bool? isIndeterminateCore = null;
    }
    [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
    public class DXSplashScreenPopup {
        public Dispatcher Dispatcher { get { return SplashPopup.Dispatcher; } }
        public UIElement SplashScreen { get; private set; }
        public double Left {
            get { return left; }
            set {
                left = value;
                OnLeftTopChanged();
            }
        }
        public double Top {
            get { return top; }
            set {
                top = value;
                OnLeftTopChanged();
            }
        }
        public event EventHandler Loaded;

        public DXSplashScreenPopup(UIElement splashScreen, bool useFadeEffect) {
            SplashPopup = new Popup();
            SplashScreen = splashScreen;
            UseFadeEffect = useFadeEffect;
        }
        public void ShowSplashScreen() {
            PrevFocusedControl = FocusManager.GetFocusedElement() as Control;
            SplashPopup.Child = SplashScreen;
            OnLeftTopChanged();
            UpdateFlowDirection();
            lastRenderTime = DateTime.Now;
            CompositionTarget.Rendering += OnRendering;
            if(UseFadeEffect)
                AgVeil.BeginWait(true);
            SplashPopup.IsOpen = true;
            UpdatePopupPos();
        }
        public void HideSplashScreen() {
            if(AgVeil.IsWait) AgVeil.EndWait();
            SplashPopup.IsOpen = false;
            SplashPopup.Child = null;
            if(PrevFocusedControl != null) {
                PrevFocusedControl.Focus();
                PrevFocusedControl = null;
            }
        }
        public void ReleaseResources() {
            if(AgVeil.IsWait) AgVeil.EndWait();
            CompositionTarget.Rendering -= OnRendering;
            SplashPopup.Child = null;
            SplashPopup = null;
        }

        void UpdateFlowDirection() {
            if(PrevFocusedControl != null)
                SplashPopup.FlowDirection = PrevFocusedControl.FlowDirection;
            else if(AppHelper.RootVisual != null)
                SplashPopup.FlowDirection = AppHelper.RootVisual.FlowDirection;
        }
        void OnLeftTopChanged() {
            if(!double.IsNaN(Left))
                SplashPopup.HorizontalOffset = Left;
            if(!double.IsNaN(Top))
                SplashPopup.VerticalOffset = Top;
        }
        void UpdatePopupPos() {
            SplashScreen.InvalidateMeasure();
            SplashScreen.InvalidateArrange();
            SplashScreen.UpdateLayout();
            if(SplashPopup == null) return;
            Dispatcher.BeginInvoke(new Action(() => {
                Size res = SplashScreen.DesiredSize;
                Size appSize = new Size(AppHelper.HostWidth, AppHelper.HostHeight);
                Left = Math.Floor((appSize.Width - res.Width) / 2);
                if(SplashPopup.FlowDirection == FlowDirection.RightToLeft)
                    Left = Left + res.Width;
                Top = Math.Floor((appSize.Height - res.Height) / 2);
                if(Top < 0) Top = 0;
                if(Left < 0) Left = 0;
            }));
        }
        void OnRendering(object sender, EventArgs e) {
            DateTime time = DateTime.Now;
            TimeSpan r = time - lastRenderTime;
            if(r.Milliseconds < 30) {
                CompositionTarget.Rendering -= OnRendering;
                Dispatcher.BeginInvoke(new Action(() => {
                    UpdatePopupPos();
                    if(Loaded != null)
                        Loaded(this, EventArgs.Empty);
                }));
            }
        }

        DateTime lastRenderTime;
        double left = double.NaN;
        double top = double.NaN;
        bool UseFadeEffect = true;
        Popup SplashPopup;
        Control PrevFocusedControl;
    }
}