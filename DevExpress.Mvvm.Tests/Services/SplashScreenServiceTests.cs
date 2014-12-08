using DevExpress.Mvvm;
using DevExpress.Mvvm.UI;
using DevExpress.Mvvm.UI.Interactivity;
using NUnit.Framework;
using System;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using System.Windows.Data;
using DevExpress.Mvvm.POCO;

namespace DevExpress.Mvvm.UI.Tests {
    internal class SplashScreenTestWindow : Window, ISplashScreen {
        public Button WindowContent { get; private set; }
        public double Progress { get; private set; }
        public bool IsIndeterminate { get; private set; }
        public string TextProp { get; private set; }
        public void Text(string value) {
            TextProp = value;
        }

        public SplashScreenTestWindow() {
            Instance = this;
            Progress = double.NaN;
            IsIndeterminate = true;
            Width = 100;
            Height = 100;
            Content = WindowContent = new Button();
            this.ShowInTaskbar = false;
        }
        void ISplashScreen.Progress(double value) {
            Progress = value;
        }
        void ISplashScreen.CloseSplashScreen() {
            Close();
        }
        void ISplashScreen.SetProgressState(bool isIndeterminate) {
            IsIndeterminate = isIndeterminate;
        }

        public static volatile SplashScreenTestWindow Instance = null;
        public static void DoEvents(DispatcherPriority priority = DispatcherPriority.Background) {
            DispatcherFrame frame = new DispatcherFrame();
            DXSplashScreen.SplashContainer.SplashScreen.Dispatcher.BeginInvoke(
                priority,
                new DispatcherOperationCallback(ExitFrame),
                frame);
            Dispatcher.PushFrame(frame);
        }
        static object ExitFrame(object f) {
            ((DispatcherFrame)f).Continue = false;
            return null;
        }
    }
    internal class SplashScreenTestUserControl : UserControl {
        public static volatile Window Window;
        public static volatile SplashScreenViewModel ViewModel;
        public static SplashScreenTestUserControl Instance { get; set; }
        public static void DoEvents(DispatcherPriority priority = DispatcherPriority.Background) {
            DispatcherFrame frame = new DispatcherFrame();
            DXSplashScreen.SplashContainer.SplashScreen.Dispatcher.BeginInvoke(
                priority,
                new DispatcherOperationCallback(ExitFrame),
                frame);
            Dispatcher.PushFrame(frame);
        }
        static object ExitFrame(object f) {
            Window = Window.GetWindow(Instance);
            ViewModel = (SplashScreenViewModel)Instance.DataContext;
            ((DispatcherFrame)f).Continue = false;
            return null;
        }
        public SplashScreenTestUserControl() {
            Instance = this;
        }
    }

    public class DXSplashScreenBaseTestFixture : BaseWpfFixture {
        protected override void TearDownCore() {
            base.TearDownCore();
            SplashScreenTestUserControl.Window = null;
            SplashScreenTestUserControl.ViewModel = null;
            SplashScreenTestUserControl.Instance = null;
            SplashScreenTestWindow.Instance = null;
            CloseDXSplashScreen();
        }
        protected void CloseDXSplashScreen() {
            if(!DXSplashScreen.IsActive)
                return;
            DXSplashScreen.Close();
            var t = DXSplashScreen.SplashContainer.InternalThread;
            if(t != null)
                t.Join();
        }
    }
    [TestFixture]
    public class DXSplashScreenTests : DXSplashScreenBaseTestFixture {
        [Test, ExpectedException(typeof(InvalidOperationException))]
        public void InvalidUsage_Test01() {
            DXSplashScreen.Show<SplashScreenTestWindow>();
            DXSplashScreen.Show<SplashScreenTestWindow>();
        }
        [Test, ExpectedException(typeof(InvalidOperationException))]
        public void InvalidUsage_Test02() {
            DXSplashScreen.Close();
        }
        [Test, ExpectedException(typeof(InvalidOperationException))]
        public void InvalidUsage_Test03() {
            DXSplashScreen.Progress(0);
        }

        [Test, Ignore]
        public void Simple_Test() {
            DXSplashScreen.Show<SplashScreenTestWindow>();
            Assert.IsNotNull(DXSplashScreen.SplashContainer.InternalThread);
            Assert.IsNotNull(DXSplashScreen.SplashContainer.SplashScreen);
            SplashScreenTestWindow.DoEvents();
            Assert.IsNotNull(SplashScreenTestWindow.Instance, "SplashScreenTestWindow.Instance == null");
            Assert.IsNotNull(SplashScreenTestWindow.Instance.WindowContent, "SplashScreenTestWindow.Instance == null");
            Assert.IsTrue(SplashScreenTestWindow.Instance.WindowContent.IsVisible);
            Assert.AreEqual(double.NaN, SplashScreenTestWindow.Instance.Progress);
            Assert.IsTrue(SplashScreenTestWindow.Instance.IsIndeterminate);
            CloseDXSplashScreen();
            Assert.IsNull(DXSplashScreen.SplashContainer.InternalThread);
            Assert.IsNull(DXSplashScreen.SplashContainer.SplashScreen);
        }
        [Test]
        public void Complex_Test() {
            DXSplashScreen.Show<SplashScreenTestWindow>();
            Assert.IsNotNull(DXSplashScreen.SplashContainer.InternalThread);
            Assert.IsNotNull(DXSplashScreen.SplashContainer.SplashScreen);
            Assert.IsTrue(SplashScreenTestWindow.Instance.IsIndeterminate);
            DXSplashScreen.Progress(0);
            SplashScreenTestWindow.DoEvents();
            Assert.IsFalse(SplashScreenTestWindow.Instance.IsIndeterminate);
            for(int i = 1; i < 10; i++) {
                DXSplashScreen.Progress(i);
                SplashScreenTestWindow.DoEvents();
                Assert.AreEqual(i, SplashScreenTestWindow.Instance.Progress);
            }
            CloseDXSplashScreen();
            Assert.IsNull(DXSplashScreen.SplashContainer.InternalThread);
            Assert.IsNull(DXSplashScreen.SplashContainer.SplashScreen);
        }
        [Test]
        public void IsActive_Test() {
            Assert.IsFalse(DXSplashScreen.IsActive);
            DXSplashScreen.Show<SplashScreenTestWindow>();
            Assert.IsTrue(DXSplashScreen.IsActive);
            CloseDXSplashScreen();
            Assert.IsFalse(DXSplashScreen.IsActive);
        }

        [Test]
        public void CustomSplashScreen_Test() {
            Func<object, Window> windowCreator = (p) => {
                Assert.AreEqual(1, p);
                return new SplashScreenTestWindow();
            };
            Func<object, object> splashScreenCreator = (p) => {
                Assert.AreEqual(1, p);
                return new TextBlock();
            };
            DXSplashScreen.Show(windowCreator, splashScreenCreator, 1, 1);
            Assert.IsNotNull(DXSplashScreen.SplashContainer.InternalThread);
            Assert.IsNotNull(DXSplashScreen.SplashContainer.SplashScreen);
            CloseDXSplashScreen();
            Assert.IsNull(DXSplashScreen.SplashContainer.InternalThread);
            Assert.IsNull(DXSplashScreen.SplashContainer.SplashScreen);
        }

        [Test]
        public void ShowWindowISplashScreen_Test() {
            DXSplashScreen.Show<SplashScreenTestWindow>();
            SplashScreenTestWindow.DoEvents();
            Assert.IsTrue(SplashScreenTestWindow.Instance.IsIndeterminate);
            Assert.AreEqual(double.NaN, SplashScreenTestWindow.Instance.Progress);
            DXSplashScreen.Progress(100);
            SplashScreenTestWindow.DoEvents();
            Assert.IsFalse(SplashScreenTestWindow.Instance.IsIndeterminate);
            Assert.AreEqual(100, SplashScreenTestWindow.Instance.Progress);
            DXSplashScreen.Progress(100, 200);
            SplashScreenTestWindow.DoEvents();
            Assert.IsFalse(SplashScreenTestWindow.Instance.IsIndeterminate);
            Assert.AreEqual(100, SplashScreenTestWindow.Instance.Progress);
            DXSplashScreen.CallSplashScreenMethod<SplashScreenTestWindow>(x => x.Text("test"));
            SplashScreenTestWindow.DoEvents();
            Assert.IsFalse(SplashScreenTestWindow.Instance.IsIndeterminate);
            Assert.AreEqual(100, SplashScreenTestWindow.Instance.Progress);
            Assert.AreEqual("test", ((SplashScreenTestWindow)SplashScreenTestWindow.Instance).TextProp);
            DXSplashScreen.SetState("test");
            SplashScreenTestWindow.DoEvents();
        }
        [Test, ExpectedException(typeof(InvalidOperationException))]
        public void ShowWindowNotISplashScreen_Test() {
            DXSplashScreen.Show<Window>();
        }
        [Test]
        public void ShowUserControl_Test() {
            DXSplashScreen.Show<SplashScreenTestUserControl>();
            SplashScreenTestUserControl.DoEvents();
            Assert.AreEqual(0, SplashScreenTestUserControl.ViewModel.Progress);
            Assert.AreEqual(100, SplashScreenTestUserControl.ViewModel.MaxProgress);
            Assert.AreEqual("Loading...", SplashScreenTestUserControl.ViewModel.State);
            Assert.AreEqual(true, SplashScreenTestUserControl.ViewModel.IsIndeterminate);
            DXSplashScreen.Progress(50);
            SplashScreenTestUserControl.DoEvents();
            Assert.AreEqual(50, SplashScreenTestUserControl.ViewModel.Progress);
            Assert.AreEqual(100, SplashScreenTestUserControl.ViewModel.MaxProgress);
            Assert.AreEqual("Loading...", SplashScreenTestUserControl.ViewModel.State);
            Assert.AreEqual(false, SplashScreenTestUserControl.ViewModel.IsIndeterminate);
            DXSplashScreen.Progress(100, 200);
            SplashScreenTestUserControl.DoEvents();
            Assert.AreEqual(100, SplashScreenTestUserControl.ViewModel.Progress);
            Assert.AreEqual(200, SplashScreenTestUserControl.ViewModel.MaxProgress);
            Assert.AreEqual("Loading...", SplashScreenTestUserControl.ViewModel.State);
            Assert.AreEqual(false, SplashScreenTestUserControl.ViewModel.IsIndeterminate);
            DXSplashScreen.SetState("Test");
            SplashScreenTestUserControl.DoEvents();
            Assert.AreEqual(100, SplashScreenTestUserControl.ViewModel.Progress);
            Assert.AreEqual(200, SplashScreenTestUserControl.ViewModel.MaxProgress);
            Assert.AreEqual("Test", SplashScreenTestUserControl.ViewModel.State);
            Assert.AreEqual(false, SplashScreenTestUserControl.ViewModel.IsIndeterminate);
        }
        [Test]
        public void ShowUserControlAndCheckWindowProperties_Test() {
            DXSplashScreen.Show<SplashScreenTestUserControl>();
            SplashScreenTestUserControl.DoEvents();
            Window wnd = SplashScreenTestUserControl.Window;

            AutoResetEvent SyncEvent = new AutoResetEvent(false);
            bool hasError = true;
            wnd.Dispatcher.BeginInvoke(new Action(() => {
                hasError = false;
                hasError = hasError | !(WindowStartupLocation.CenterScreen == wnd.WindowStartupLocation);
                hasError = hasError | !(true == WindowFadeAnimationBehavior.GetEnableAnimation(wnd));
                hasError = hasError | !(null == wnd.Style);
                hasError = hasError | !(WindowStyle.None == wnd.WindowStyle);
                hasError = hasError | !(ResizeMode.NoResize == wnd.ResizeMode);
                hasError = hasError | !(true == wnd.AllowsTransparency);
                hasError = hasError | !(Colors.Transparent == ((SolidColorBrush)wnd.Background).Color);
                hasError = hasError | !(false == wnd.ShowInTaskbar);
                hasError = hasError | !(true == wnd.Topmost);
                hasError = hasError | !(SizeToContent.WidthAndHeight == wnd.SizeToContent);
                SyncEvent.Set();
            }));
            SyncEvent.WaitOne(TimeSpan.FromSeconds(2));
            Assert.IsFalse(hasError);
            CloseDXSplashScreen();
        }

        [Test]
        public void TestQ338517_1() {
            DXSplashScreen.Show<SplashScreenTestWindow>();
            Assert.IsNotNull(DXSplashScreen.SplashContainer.InternalThread);
            Assert.IsNotNull(DXSplashScreen.SplashContainer.SplashScreen);
            SplashScreenTestWindow.DoEvents();
            Assert.IsTrue(SplashScreenTestWindow.Instance.WindowContent.IsVisible);
            Assert.AreEqual(double.NaN, SplashScreenTestWindow.Instance.Progress);
            Assert.IsTrue(SplashScreenTestWindow.Instance.IsIndeterminate);
            DXSplashScreen.CallSplashScreenMethod<SplashScreenTestWindow>(x => x.Text("Test"));
            SplashScreenTestWindow.DoEvents();
            Assert.AreEqual("Test", ((SplashScreenTestWindow)SplashScreenTestWindow.Instance).TextProp);
            CloseDXSplashScreen();
            Assert.IsNull(DXSplashScreen.SplashContainer.InternalThread);
            Assert.IsNull(DXSplashScreen.SplashContainer.SplashScreen);
        }
        [Test, ExpectedException(typeof(InvalidOperationException))]
        public void TestQ338517_2() {
            DXSplashScreen.Show<SplashScreenTestWindow>();
            DXSplashScreen.CallSplashScreenMethod<UserControl>(x => x.Tag = "Test");
        }
    }
    [TestFixture]
    public class DXSplashScreenServiceTests : DXSplashScreenBaseTestFixture {
        public class ContainerVM {
            public virtual double Progress { get; set; }
            public virtual double MaxProgress { get; set; }
            public virtual object State { get; set; }
        }
        [Test]
        public void TestB238799() {
            DXSplashScreenService s = new DXSplashScreenService();
            ((ISplashScreenService)s).HideSplashScreen();
            ((ISplashScreenService)s).HideSplashScreen();
        }
        [Test]
        public void ShowWindowISplashScreen() {
            ISplashScreenService service = new DXSplashScreenService() {
                SplashScreenType = typeof(SplashScreenTestWindow),
            };

            service.ShowSplashScreen();
            SplashScreenTestWindow.DoEvents();
            Assert.IsTrue(SplashScreenTestWindow.Instance.IsIndeterminate);
            Assert.AreEqual(double.NaN, SplashScreenTestWindow.Instance.Progress);
            service.SetSplashScreenProgress(100, 100);
            SplashScreenTestWindow.DoEvents();
            Assert.IsFalse(SplashScreenTestWindow.Instance.IsIndeterminate);
            Assert.AreEqual(100, SplashScreenTestWindow.Instance.Progress);
            service.SetSplashScreenProgress(100, 200);
            SplashScreenTestWindow.DoEvents();
            Assert.IsFalse(SplashScreenTestWindow.Instance.IsIndeterminate);
            Assert.AreEqual(100, SplashScreenTestWindow.Instance.Progress);
            DXSplashScreen.CallSplashScreenMethod<SplashScreenTestWindow>(x => x.Text("test"));
            SplashScreenTestWindow.DoEvents();
            Assert.IsFalse(SplashScreenTestWindow.Instance.IsIndeterminate);
            Assert.AreEqual(100, SplashScreenTestWindow.Instance.Progress);
            Assert.AreEqual("test", ((SplashScreenTestWindow)SplashScreenTestWindow.Instance).TextProp);
            DXSplashScreen.SetState("test");
            service.SetSplashScreenState("test");
            SplashScreenTestWindow.DoEvents();
            service.HideSplashScreen();
        }
        [Test, ExpectedException(typeof(InvalidOperationException))]
        public void ShowWindowNotISplashScreen2() {
            ISplashScreenService service = new DXSplashScreenService() {
                SplashScreenType = typeof(Window),
            };
            service.ShowSplashScreen();
        }
        [Test]
        public void ShowUserControl() {
            ISplashScreenService service = new DXSplashScreenService() {
                ViewTemplate = new DataTemplate() { VisualTree = new FrameworkElementFactory(typeof(SplashScreenTestUserControl)) },
            };
            ShowUserControlCore(service);
        }
        [Test]
        public void BindServiceProperties() {
            var service = new DXSplashScreenService() {
                ViewTemplate = new DataTemplate() { VisualTree = new FrameworkElementFactory(typeof(SplashScreenTestUserControl)) },
            };
            ISplashScreenService iService = service;
            Border container = new Border();
            ContainerVM vm = ViewModelSource.Create(() => new ContainerVM());
            container.DataContext = vm;
            vm.State = "Loading2";
            BindingOperations.SetBinding(service, DXSplashScreenService.ProgressProperty, new Binding("Progress"));
            BindingOperations.SetBinding(service, DXSplashScreenService.MaxProgressProperty, new Binding("MaxProgress"));
            BindingOperations.SetBinding(service, DXSplashScreenService.StateProperty, new Binding("State"));
            Interaction.GetBehaviors(container).Add(service);

            service.ShowSplashScreen();
            SplashScreenTestUserControl.DoEvents();
            Assert.AreEqual(0, SplashScreenTestUserControl.ViewModel.Progress);
            Assert.AreEqual(0, SplashScreenTestUserControl.ViewModel.MaxProgress);
            Assert.AreEqual("Loading2", SplashScreenTestUserControl.ViewModel.State);
            Assert.AreEqual(false, SplashScreenTestUserControl.ViewModel.IsIndeterminate);
            vm.Progress = 50; vm.MaxProgress = 100;
            SplashScreenTestUserControl.DoEvents();
            Assert.AreEqual(50, SplashScreenTestUserControl.ViewModel.Progress);
            Assert.AreEqual(100, SplashScreenTestUserControl.ViewModel.MaxProgress);
            Assert.AreEqual("Loading2", SplashScreenTestUserControl.ViewModel.State);
            Assert.AreEqual(false, SplashScreenTestUserControl.ViewModel.IsIndeterminate);
            vm.Progress = 100; vm.MaxProgress = 200;
            SplashScreenTestUserControl.DoEvents();
            Assert.AreEqual(100, SplashScreenTestUserControl.ViewModel.Progress);
            Assert.AreEqual(200, SplashScreenTestUserControl.ViewModel.MaxProgress);
            Assert.AreEqual("Loading2", SplashScreenTestUserControl.ViewModel.State);
            Assert.AreEqual(false, SplashScreenTestUserControl.ViewModel.IsIndeterminate);
            vm.State = "Test";
            SplashScreenTestUserControl.DoEvents();
            Assert.AreEqual(100, SplashScreenTestUserControl.ViewModel.Progress);
            Assert.AreEqual(200, SplashScreenTestUserControl.ViewModel.MaxProgress);
            Assert.AreEqual("Test", SplashScreenTestUserControl.ViewModel.State);
            Assert.AreEqual(false, SplashScreenTestUserControl.ViewModel.IsIndeterminate);
            iService.HideSplashScreen();
        }
        [Test]
        public void ShowUserControlViaSplashScreenType() {
            ISplashScreenService service = new DXSplashScreenService() {
                SplashScreenType = typeof(SplashScreenTestUserControl),
            };
            ShowUserControlCore(service);
        }
        void ShowUserControlCore(ISplashScreenService service) {
            service.ShowSplashScreen();
            SplashScreenTestUserControl.DoEvents();
            Assert.AreEqual(0, SplashScreenTestUserControl.ViewModel.Progress);
            Assert.AreEqual(100, SplashScreenTestUserControl.ViewModel.MaxProgress);
            Assert.AreEqual("Loading...", SplashScreenTestUserControl.ViewModel.State);
            Assert.AreEqual(true, SplashScreenTestUserControl.ViewModel.IsIndeterminate);
            service.SetSplashScreenProgress(50, 100);
            SplashScreenTestUserControl.DoEvents();
            Assert.AreEqual(50, SplashScreenTestUserControl.ViewModel.Progress);
            Assert.AreEqual(100, SplashScreenTestUserControl.ViewModel.MaxProgress);
            Assert.AreEqual("Loading...", SplashScreenTestUserControl.ViewModel.State);
            Assert.AreEqual(false, SplashScreenTestUserControl.ViewModel.IsIndeterminate);
            service.SetSplashScreenProgress(100, 200);
            SplashScreenTestUserControl.DoEvents();
            Assert.AreEqual(100, SplashScreenTestUserControl.ViewModel.Progress);
            Assert.AreEqual(200, SplashScreenTestUserControl.ViewModel.MaxProgress);
            Assert.AreEqual("Loading...", SplashScreenTestUserControl.ViewModel.State);
            Assert.AreEqual(false, SplashScreenTestUserControl.ViewModel.IsIndeterminate);
            service.SetSplashScreenState("Test");
            SplashScreenTestUserControl.DoEvents();
            Assert.AreEqual(100, SplashScreenTestUserControl.ViewModel.Progress);
            Assert.AreEqual(200, SplashScreenTestUserControl.ViewModel.MaxProgress);
            Assert.AreEqual("Test", SplashScreenTestUserControl.ViewModel.State);
            Assert.AreEqual(false, SplashScreenTestUserControl.ViewModel.IsIndeterminate);
            service.HideSplashScreen();
        }
        [Test]
        public void ShowUserControlAndCheckWindowProperties() {
            ISplashScreenService service = new DXSplashScreenService() {
                ViewTemplate = new DataTemplate() { VisualTree = new FrameworkElementFactory(typeof(SplashScreenTestUserControl)) },
            };
            service.ShowSplashScreen();
            SplashScreenTestUserControl.DoEvents();
            Window wnd = SplashScreenTestUserControl.Window;
            AutoResetEvent SyncEvent = new AutoResetEvent(false);
            bool hasError = true;
            wnd.Dispatcher.BeginInvoke(new Action(() => {
                hasError = false;
                hasError = hasError | !(WindowStartupLocation.CenterScreen == wnd.WindowStartupLocation);
                hasError = hasError | !(true == WindowFadeAnimationBehavior.GetEnableAnimation(wnd));
                hasError = hasError | !(null == wnd.Style);
                hasError = hasError | !(WindowStyle.None == wnd.WindowStyle);
                hasError = hasError | !(ResizeMode.NoResize == wnd.ResizeMode);
                hasError = hasError | !(true == wnd.AllowsTransparency);
                hasError = hasError | !(Colors.Transparent == ((SolidColorBrush)wnd.Background).Color);
                hasError = hasError | !(false == wnd.ShowInTaskbar);
                hasError = hasError | !(true == wnd.Topmost);
                hasError = hasError | !(SizeToContent.WidthAndHeight == wnd.SizeToContent);
                SyncEvent.Set();
            }));
            SyncEvent.WaitOne(TimeSpan.FromSeconds(2));
            Assert.IsFalse(hasError);
            service.HideSplashScreen();
        }
        [Test]
        public void ShowUserControlAndCheckWindowProperties2() {
            Style wndStyle = new Style();
            ISplashScreenService service = new DXSplashScreenService() {
                ViewTemplate = new DataTemplate() { VisualTree = new FrameworkElementFactory(typeof(SplashScreenTestUserControl)) },
                SplashScreenWindowStyle = wndStyle,
                SplashScreenStartupLocation = WindowStartupLocation.Manual,

            };
            service.ShowSplashScreen();
            SplashScreenTestUserControl.DoEvents();
            Window wnd = SplashScreenTestUserControl.Window;
            AutoResetEvent SyncEvent = new AutoResetEvent(false);
            bool hasError = true;
            wnd.Dispatcher.BeginInvoke(new Action(() => {
                hasError = false;
                hasError = hasError | !(WindowStartupLocation.Manual == wnd.WindowStartupLocation);
                hasError = hasError | !(false == WindowFadeAnimationBehavior.GetEnableAnimation(wnd));
                hasError = hasError | !(wndStyle == wnd.Style);
                hasError = hasError | !(WindowStyle.SingleBorderWindow == wnd.WindowStyle);
                hasError = hasError | !(ResizeMode.CanResize == wnd.ResizeMode);
                hasError = hasError | !(false == wnd.AllowsTransparency);
                hasError = hasError | !(true == wnd.ShowInTaskbar);
                hasError = hasError | !(false == wnd.Topmost);
                hasError = hasError | !(SizeToContent.Manual == wnd.SizeToContent);
                SyncEvent.Set();
            }));
            SyncEvent.WaitOne(TimeSpan.FromSeconds(2));
            Assert.IsFalse(hasError);
            service.HideSplashScreen();
        }
        [Test]
        public void ViewTemplateShouldBeSealed() {
            DataTemplate temp = new DataTemplate() { VisualTree = new FrameworkElementFactory(typeof(SplashScreenTestUserControl)) };
            Assert.IsFalse(temp.IsSealed);
            DXSplashScreenService service = new DXSplashScreenService() {
                ViewTemplate = temp,
            };
            Assert.IsTrue(temp.IsSealed);
        }
        [Test, ExpectedException(typeof(InvalidOperationException))]
        public void ViewTemplateSelectorIsNotSupported() {
            DXSplashScreenService service = new DXSplashScreenService() {
                ViewTemplateSelector = new DataTemplateSelector(),
            };
        }
    }
}