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
using System.Windows.Interop;
using System.Reflection;
using System.Security;
using System.Security.Policy;
using System.Linq;
using DevExpress.Mvvm.Native;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Collections.Generic;

namespace DevExpress.Mvvm.UI.Tests {
    internal class SplashScreenTestWindow : Window, ISplashScreen {
        public Button WindowContent { get; private set; }
        public double Progress { get; private set; }
        public bool IsIndeterminate { get; private set; }
        public string TextProp { get; private set; }
        public bool CloseRequested { get; private set; }
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
            SetCurrentValue(ShowInTaskbarProperty, false);
        }
        void ISplashScreen.Progress(double value) {
            Progress = value;
        }
        void ISplashScreen.CloseSplashScreen() {
            Close();
            CloseRequested = true;
        }
        void ISplashScreen.SetProgressState(bool isIndeterminate) {
            IsIndeterminate = isIndeterminate;
        }

        public static volatile SplashScreenTestWindow Instance = null;
        public static void DoEvents(DispatcherPriority priority = DispatcherPriority.Background) {
            DispatcherObject dispObj = (Instance ?? DXSplashScreen.SplashContainer.ActiveInfo.SplashScreen);
            if(dispObj == null)
                return;
            DispatcherFrame frame = new DispatcherFrame();
            dispObj.Dispatcher.BeginInvoke(
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
        public static Func<ControlTemplate> TemplateCreator;
        public static SplashScreenTestUserControl Instance { get; set; }
        public static void DoEvents(DispatcherPriority priority = DispatcherPriority.Background) {
            DispatcherObject dispObj = DXSplashScreen.SplashContainer.ActiveInfo.SplashScreen;
            if(dispObj == null)
                return;
            DispatcherFrame frame = new DispatcherFrame();
            dispObj.Dispatcher.BeginInvoke(
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
            if(TemplateCreator != null)
                Template = TemplateCreator.Invoke();
        }
    }

    public class DXSplashScreenBaseTestFixture : BaseWpfFixture {
        protected override void SetUpCore() {
            base.SetUpCore();
            DXSplashScreen.UIThreadDelay = 700;
            DXSplashScreen.NotInitializedStateMethodCallPolicy = NotInitializedStateMethodCallPolicy.CallWhenReady;
        }
        protected override void TearDownCore() {
            SplashScreenTestUserControl.Window = null;
            SplashScreenTestUserControl.ViewModel = null;
            SplashScreenTestUserControl.Instance = null;
            SplashScreenTestUserControl.TemplateCreator = null;
            SplashScreenTestWindow.Instance = null;
            var info = DXSplashScreen.SplashContainer?.ActiveInfo;
            var oldInfo = DXSplashScreen.SplashContainer?.OldInfo;
            if(info != null && info.WaitEvent != null)
                info.WaitEvent.Set();
            if(oldInfo != null && oldInfo.WaitEvent != null)
                oldInfo.WaitEvent.Set();
            CloseDXSplashScreen();
            base.TearDownCore();
        }
        protected void CloseDXSplashScreen() {
            SplashScreenTestsHelper.CloseDXSplashScreen();
        }
        protected Rect GetSplashScreenBounds() {
            var splashScreen = DXSplashScreen.SplashContainer.ActiveInfo.SplashScreen;
            Rect pos = Rect.Empty;
            SplashScreenHelper.InvokeAsync(splashScreen, () => {
                pos = new Rect(splashScreen.Left, splashScreen.Top, splashScreen.ActualWidth, splashScreen.ActualHeight);
            });
            SplashScreenTestUserControl.DoEvents();
            return pos;
        }
        protected WindowStartupLocation? GetSplashScreenStartupLocation() {
            WindowStartupLocation? pos = null;
            var spScr = DXSplashScreen.SplashContainer.ActiveInfo.SplashScreen;
            SplashScreenHelper.InvokeAsync(spScr, () => pos = spScr.WindowStartupLocation);
            SplashScreenTestUserControl.DoEvents();
            return pos;
        }
        protected static bool HasFadeAnimation(Window wnd) {
            return Interaction.GetBehaviors(wnd).Any(x => x is WindowFadeAnimationBehavior);
        }

        protected static bool AreRectsClose(Rect rect1, Rect rect2, double accuracy = 5) {
            return ArePointsClose(rect1.TopLeft, rect2.TopLeft)
                && rect1.Size == rect2.Size;
        }
        protected static bool ArePointsClose(Point point1, Point point2, double accuracy = 5) {
            return Math.Abs(point1.X - point2.X) < accuracy
                && Math.Abs(point1.Y - point2.Y) < accuracy;
        }
    }
    [TestFixture]
    public class DXSplashScreenTests : DXSplashScreenBaseTestFixture {
        [Test, Order(1)]
        public void InvalidUsage_Test01() {
            Assert.Throws<InvalidOperationException>(() => {
                DXSplashScreen.Show<SplashScreenTestWindow>();
                DXSplashScreen.Show<SplashScreenTestWindow>();
            });
        }
        [Test, Order(2)]
        public void InvalidUsage_Test02() {
            Assert.Throws<InvalidOperationException>(() => {
                DXSplashScreen.Close();
            });
        }
        [Test, Order(3)]
        public void InvalidUsage_Test03() {
            Assert.Throws<InvalidOperationException>(() => {
                DXSplashScreen.Progress(0);
            });
        }

        [Test, Ignore("Ignore")]
        public void Simple_Test() {
            DXSplashScreen.Show<SplashScreenTestWindow>();
            Assert.IsNotNull(DXSplashScreen.SplashContainer.ActiveInfo.InternalThread);
            Assert.IsNotNull(DXSplashScreen.SplashContainer.ActiveInfo.SplashScreen);
            SplashScreenTestWindow.DoEvents();
            Assert.IsNotNull(SplashScreenTestWindow.Instance, "SplashScreenTestWindow.Instance == null");
            Assert.IsNotNull(SplashScreenTestWindow.Instance.WindowContent, "SplashScreenTestWindow.Instance == null");
            Assert.IsTrue(SplashScreenTestWindow.Instance.WindowContent.IsVisible);
            Assert.AreEqual(double.NaN, SplashScreenTestWindow.Instance.Progress);
            Assert.IsTrue(SplashScreenTestWindow.Instance.IsIndeterminate);
            CloseDXSplashScreen();
            Assert.IsNull(DXSplashScreen.SplashContainer.OldInfo.InternalThread);
            Assert.IsNull(DXSplashScreen.SplashContainer.OldInfo.SplashScreen);
        }
        [Test, Order(4)]
        public void Complex_Test() {
            DXSplashScreen.Show<SplashScreenTestWindow>();
            Assert.IsNotNull(DXSplashScreen.SplashContainer.ActiveInfo.InternalThread);
            Assert.IsNotNull(DXSplashScreen.SplashContainer.ActiveInfo.Dispatcher);
            Assert.IsNotNull(DXSplashScreen.SplashContainer.ActiveInfo.SplashScreen);
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
            Assert.IsNull(DXSplashScreen.SplashContainer.OldInfo.InternalThread);
            Assert.IsNull(DXSplashScreen.SplashContainer.OldInfo.SplashScreen);
        }
        [Test, Order(5)]
        public void IsActive_Test() {
            Assert.IsFalse(DXSplashScreen.IsActive);
            DXSplashScreen.Show<SplashScreenTestWindow>();
            Assert.IsTrue(DXSplashScreen.IsActive);
            CloseDXSplashScreen();
            Assert.IsFalse(DXSplashScreen.IsActive);
        }

        [Test, Order(6)]
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
            Assert.IsNotNull(DXSplashScreen.SplashContainer.ActiveInfo.InternalThread);
            Assert.IsNotNull(DXSplashScreen.SplashContainer.ActiveInfo.SplashScreen);
            CloseDXSplashScreen();
            Assert.IsNull(DXSplashScreen.SplashContainer.OldInfo.InternalThread);
            Assert.IsNull(DXSplashScreen.SplashContainer.OldInfo.SplashScreen);
        }

        [Test, Order(7)]
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
        [Test, Order(8)]
        public void ShowWindowNotISplashScreen_Test() {
            Assert.Throws<InvalidOperationException>(() => {
                DXSplashScreen.Show<Window>();
            });
        }
        [Test, Order(9)]
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
        [Test, Order(10)]
        public void ShowUserControlAndCheckWindowProperties_Test() {
            DXSplashScreen.Show<SplashScreenTestUserControl>();
            SplashScreenTestUserControl.DoEvents();
            Window wnd = SplashScreenTestUserControl.Window;

            AutoResetEvent SyncEvent = new AutoResetEvent(false);
            bool hasError = true;
            wnd.Dispatcher.BeginInvoke(new Action(() => {
                hasError = false;
                hasError = hasError | !(WindowStartupLocation.CenterScreen == wnd.WindowStartupLocation);
                hasError = hasError | !(true == HasFadeAnimation(wnd));
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

        [Test, Order(11)]
        public void TestQ338517_1() {
            DXSplashScreen.Show<SplashScreenTestWindow>();
            Assert.IsNotNull(DXSplashScreen.SplashContainer.ActiveInfo.InternalThread);
            Assert.IsNotNull(DXSplashScreen.SplashContainer.ActiveInfo.SplashScreen);
            SplashScreenTestWindow.DoEvents();
            Assert.IsTrue(SplashScreenTestWindow.Instance.WindowContent.IsVisible);
            Assert.AreEqual(double.NaN, SplashScreenTestWindow.Instance.Progress);
            Assert.IsTrue(SplashScreenTestWindow.Instance.IsIndeterminate);
            DXSplashScreen.CallSplashScreenMethod<SplashScreenTestWindow>(x => x.Text("Test"));
            SplashScreenTestWindow.DoEvents();
            Assert.AreEqual("Test", ((SplashScreenTestWindow)SplashScreenTestWindow.Instance).TextProp);
            CloseDXSplashScreen();
            Assert.IsNull(DXSplashScreen.SplashContainer.OldInfo.InternalThread);
            Assert.IsNull(DXSplashScreen.SplashContainer.OldInfo.SplashScreen);
        }
        [Test, Order(12)]
        public void TestQ338517_2() {
            DXSplashScreen.Show<SplashScreenTestWindow>();
            Assert.Throws<InvalidOperationException>(() => {
                DXSplashScreen.CallSplashScreenMethod<UserControl>(x => x.Tag = "Test");
            });
        }
        [Test, Order(13)]
        public void SplashScreenOwner_Test00() {
            var owner = new SplashScreenOwner(new Border());
            DXSplashScreen.Show<SplashScreenTestUserControl>(WindowStartupLocation.CenterScreen, owner);
            var info = DXSplashScreen.SplashContainer.ActiveInfo;
            Assert.AreEqual(owner.Owner, info.Owner.WindowObject);
            Assert.IsNotNull(info.InternalThread);
            Assert.IsFalse(info.Owner.IsInitialized);
            CloseDXSplashScreen();
        }
        [Test, Order(14)]
        public void SplashScreenOwner_Test01() {
            DXSplashScreen.Show<SplashScreenTestUserControl>();
            var info = DXSplashScreen.SplashContainer.ActiveInfo;
            Assert.IsNull(info.Owner);
            Assert.IsNotNull(info.InternalThread);
            Assert.IsNull(info.RelationInfo);
            CloseDXSplashScreen();
        }
        [Test, Order(15)]
        public void SplashScreenOwner_Test02() {
            var owner = new SplashScreenOwner(new Border());
            DXSplashScreen.Show(CreateDefaultWindow, CreateDefaultContent, new object[] { owner }, null);
            var info = DXSplashScreen.SplashContainer.ActiveInfo;
            Assert.AreEqual(owner.Owner, info.Owner.WindowObject);
            Assert.IsNotNull(info.InternalThread);
            Assert.IsFalse(info.Owner.IsInitialized);
            CloseDXSplashScreen();
        }
        [Test, Order(16)]
        public void SplashScreenOwner_Test03() {
            var owner = new WindowContainer(new Border());
            DXSplashScreen.Show(CreateDefaultWindow, CreateDefaultContent, new object[] { owner }, null);
            var info = DXSplashScreen.SplashContainer.ActiveInfo;
            Assert.AreEqual(owner, info.Owner);
            Assert.IsNotNull(info.InternalThread);
            Assert.IsFalse(info.Owner.IsInitialized);
            CloseDXSplashScreen();
        }
        [Test, Order(17)]
        public void SplashScreenOwner_Test04() {
            var owner = new WindowContainer(new Border());
            DXSplashScreen.Show(CreateDefaultWindow, CreateDefaultContent, new object[] { owner.CreateOwnerContainer() }, null);
            var info = DXSplashScreen.SplashContainer.ActiveInfo;
            Assert.AreEqual(owner, info.Owner);
            Assert.IsNotNull(info.InternalThread);
            Assert.IsFalse(info.Owner.IsInitialized);
            CloseDXSplashScreen();
        }
        [Test, Order(18)]
        public void SplashScreenOwner_Test05() {
            DXSplashScreen.Show(CreateDefaultWindow, CreateDefaultContent, null, null);
            var info = DXSplashScreen.SplashContainer.ActiveInfo;
            Assert.IsNull(info.Owner);
            Assert.IsNotNull(info.InternalThread);
            Assert.IsNull(info.RelationInfo);
            CloseDXSplashScreen();
        }
        [Test, Order(19)]
        public void NotShowIfOwnerClosedTest00_T268403() {
            RealWindow.Show();
            DispatcherHelper.DoEvents();
            var owner = new SplashScreenOwner(RealWindow);
            RealWindow.Close();
            if(DXSplashScreen.SplashContainer != null)
                DXSplashScreen.SplashContainer.Test_SkipWindowOpen = false;
            DXSplashScreen.Show(CreateDefaultWindow, CreateDefaultContent, new object[] { owner }, null);
            var info = DXSplashScreen.SplashContainer.ActiveInfo;
            EnqueueWait(() => info.InternalThread == null);
            Assert.IsFalse(DXSplashScreen.IsActive);
            Assert.IsTrue(DXSplashScreen.SplashContainer.Test_SkipWindowOpen);
            CloseDXSplashScreen();
        }
        [Test, Order(20)]
        public void NotShowIfOwnerClosedTest01_T268403() {
            RealWindow.Show();
            RealWindow.Close();
            DispatcherHelper.DoEvents();
            var owner = new SplashScreenOwner(RealWindow);
            if(DXSplashScreen.SplashContainer != null)
                DXSplashScreen.SplashContainer.Test_SkipWindowOpen = true;
            DXSplashScreen.Show(CreateDefaultWindow, CreateDefaultContent, new object[] { owner, SplashScreenClosingMode.ManualOnly }, null);
            EnqueueDelay(100);
            DispatcherHelper.DoEvents();
            var info = DXSplashScreen.SplashContainer.ActiveInfo;
            Assert.IsNotNull(info.InternalThread);
            Assert.IsFalse(info.RelationInfo.IsInitialized, "relation");
            Assert.IsTrue(DXSplashScreen.IsActive);
            Assert.IsFalse(DXSplashScreen.SplashContainer.Test_SkipWindowOpen, "skipOpen");
            CloseDXSplashScreen();
        }
        [Test, Order(21)]
        public void SplashScreenStartupLocation_Test00() {
            var owner = new SplashScreenOwner(RealWindow);
            RealWindow.WindowStartupLocation = WindowStartupLocation.Manual;
            RealWindow.Left = 200;
            RealWindow.Width = 300;
            RealWindow.Top = 100;
            RealWindow.Height = 240;
            RealWindow.Show();
            DispatcherHelper.DoEvents();
            SplashScreenTestUserControl.TemplateCreator = SplashScreenTestsHelper.CreateControlTemplate;
            DXSplashScreen.Show<SplashScreenTestUserControl>(WindowStartupLocation.CenterOwner, owner);
            SplashScreenTestUserControl.DoEvents();
            Rect pos = GetSplashScreenBounds();
            Assert.AreEqual(true, ArePointsClose(new Point(290, 150), pos.TopLeft));
            CloseDXSplashScreen();
        }
        [Test, Order(22)]
        public void SplashScreenStartupLocation_Test01() {
            RealWindow.Show();
            DispatcherHelper.DoEvents();
            DXSplashScreen.Show<SplashScreenTestUserControl>(WindowStartupLocation.CenterOwner);
            SplashScreenTestUserControl.DoEvents();
            Assert.AreEqual(WindowStartupLocation.CenterOwner, GetSplashScreenStartupLocation().Value);
            CloseDXSplashScreen();
        }
        [Test, Order(23)]
        public void SplashScreenStartupLocation_Test02() {
            RealWindow.Show();
            DispatcherHelper.DoEvents();
            DXSplashScreen.Show<SplashScreenTestUserControl>(WindowStartupLocation.CenterScreen);
            SplashScreenTestUserControl.DoEvents();
            Assert.AreEqual(WindowStartupLocation.CenterScreen, GetSplashScreenStartupLocation().Value);
            CloseDXSplashScreen();
        }
        [Test, Order(24)]
        public void SplashScreenStartupLocation_Test03() {
            RealWindow.Show();
            DispatcherHelper.DoEvents();
            DXSplashScreen.Show<SplashScreenTestUserControl>(WindowStartupLocation.Manual);
            SplashScreenTestUserControl.DoEvents();
            Assert.AreEqual(WindowStartupLocation.Manual, GetSplashScreenStartupLocation().Value);
            CloseDXSplashScreen();
        }
        [Test, Order(25)]
        public void SplashScreenStartupLocation_Test04() {
            var owner = new SplashScreenOwner(RealWindow);
            RealWindow.WindowStartupLocation = WindowStartupLocation.Manual;
            RealWindow.Left = 200;
            RealWindow.Width = 300;
            RealWindow.Top = 100;
            RealWindow.Height = 240;
            RealWindow.Show();
            DispatcherHelper.DoEvents();
            DXSplashScreen.Show(CreateDefaultWindow, CreateDefaultContent, new object[] { owner, WindowStartupLocation.CenterOwner }, null);
            SplashScreenTestUserControl.DoEvents();
            Rect pos = GetSplashScreenBounds();
            Assert.AreEqual(true, ArePointsClose(new Point(270, 130), pos.TopLeft));
            CloseDXSplashScreen();
        }
        [Test, Order(26)]
        public void SplashScreenStartupLocation_Test05() {
            RealWindow.Show();
            DispatcherHelper.DoEvents();
            DXSplashScreen.Show<SplashScreenTestUserControl>(WindowStartupLocation.CenterScreen, new SplashScreenOwner(RealWindow));
            SplashScreenTestUserControl.DoEvents();
            Rect pos = GetSplashScreenBounds();
            var screen = System.Windows.Forms.Screen.PrimaryScreen;
            Assert.IsNotNull(screen);
            var area = screen.WorkingArea;
            var expectedPos = new Point(area.X + (area.Width - pos.Width) * 0.5, area.Y + (area.Height - pos.Height) * 0.5);
            Assert.IsTrue(Math.Abs(expectedPos.X - pos.Left) < 2);
            Assert.IsTrue(Math.Abs(expectedPos.Y - pos.Top) < 2);

            var splashScreen = DXSplashScreen.SplashContainer.ActiveInfo.SplashScreen;
            SplashScreenHelper.InvokeAsync(splashScreen, () => {
                SplashScreenTestUserControl.Instance.Width = 400;
                SplashScreenTestUserControl.Instance.Height = 300;
            });
            SplashScreenTestUserControl.DoEvents();
            DispatcherHelper.DoEvents();

            pos = GetSplashScreenBounds();
            expectedPos = new Point(area.X + (area.Width - pos.Width) * 0.5, area.Y + (area.Height - pos.Height) * 0.5);
            Assert.IsTrue(Math.Abs(expectedPos.X - pos.Left) < 2);
            Assert.IsTrue(Math.Abs(expectedPos.Y - pos.Top) < 2);
            CloseDXSplashScreen();
        }
        [Test, Order(27)]
        public void SplashScreenClosingMode_Test00() {
            RealWindow.Show();
            DispatcherHelper.DoEvents();
            DXSplashScreen.Show<SplashScreenTestUserControl>(WindowStartupLocation.CenterScreen, new SplashScreenOwner(RealWindow), SplashScreenClosingMode.Default);
            SplashScreenTestUserControl.DoEvents();
            Assert.IsTrue(DXSplashScreen.IsActive);
            RealWindow.Close();
            DispatcherHelper.DoEvents();
            Assert.IsFalse(DXSplashScreen.IsActive);
        }
        [Test, Order(28)]
        public void SplashScreenClosingMode_Test01() {
            RealWindow.Show();
            DispatcherHelper.DoEvents();
            DXSplashScreen.Show<SplashScreenTestUserControl>(WindowStartupLocation.CenterScreen, new SplashScreenOwner(RealWindow), SplashScreenClosingMode.ManualOnly);
            SplashScreenTestUserControl.DoEvents();
            Assert.IsTrue(DXSplashScreen.IsActive);
            RealWindow.Close();
            DispatcherHelper.DoEvents();
            Assert.IsTrue(DXSplashScreen.IsActive);
            CloseDXSplashScreen();
        }
        [Test, Order(29)]
        public void SplashScreenClosingMode_Test02() {
            RealWindow.Show();
            DispatcherHelper.DoEvents();
            DXSplashScreen.Show<SplashScreenTestUserControl>(WindowStartupLocation.CenterScreen, new SplashScreenOwner(RealWindow), SplashScreenClosingMode.OnParentClosed);
            SplashScreenTestUserControl.DoEvents();
            Assert.IsTrue(DXSplashScreen.IsActive);
            RealWindow.Close();
            DispatcherHelper.DoEvents();
            Assert.IsFalse(DXSplashScreen.IsActive);
        }
        [Test, Order(30)]
        public void SplashScreenClosingMode_Test03() {
            RealWindow.Show();
            DispatcherHelper.DoEvents();
            DXSplashScreen.Show<SplashScreenTestUserControl>(WindowStartupLocation.CenterScreen, new SplashScreenOwner(RealWindow), SplashScreenClosingMode.OnParentClosed);
            SplashScreenTestUserControl.DoEvents();
            Assert.IsTrue(DXSplashScreen.IsActive);
            CloseDXSplashScreen();
            Assert.IsFalse(DXSplashScreen.IsActive);
            RealWindow.Close();
        }
        [Test, Order(31)]
        public void SplashScreenClosingMode_Test04() {
            RealWindow.Show();
            DispatcherHelper.DoEvents();
            DXSplashScreen.Show(CreateDefaultWindow, CreateDefaultContent, new object[] { new SplashScreenOwner(RealWindow), SplashScreenClosingMode.Default }, null);
            SplashScreenTestUserControl.DoEvents();
            Assert.IsTrue(DXSplashScreen.IsActive);
            RealWindow.Close();
            DispatcherHelper.DoEvents();
            Assert.IsFalse(DXSplashScreen.IsActive);
        }
        [Test, Order(32)]
        public void SplashScreenClosingMode_Test05() {
            RealWindow.Show();
            DispatcherHelper.DoEvents();
            DXSplashScreen.Show(CreateDefaultWindow, CreateDefaultContent, new object[] { new SplashScreenOwner(RealWindow), SplashScreenClosingMode.ManualOnly }, null);
            SplashScreenTestUserControl.DoEvents();
            Assert.IsTrue(DXSplashScreen.IsActive);
            RealWindow.Close();
            DispatcherHelper.DoEvents();
            Assert.IsTrue(DXSplashScreen.IsActive);
            CloseDXSplashScreen();
            Assert.IsFalse(DXSplashScreen.IsActive);
        }
        [Test, Order(33)]
        public void SplashScreenClosingMode_Test06() {
            RealWindow.Show();
            DispatcherHelper.DoEvents();
            DXSplashScreen.Show(CreateDefaultWindow, CreateDefaultContent, new object[] { new SplashScreenOwner(RealWindow), SplashScreenClosingMode.OnParentClosed }, null);
            SplashScreenTestUserControl.DoEvents();
            Assert.IsTrue(DXSplashScreen.IsActive);
            RealWindow.Close();
            DispatcherHelper.DoEvents();
            Assert.IsFalse(DXSplashScreen.IsActive);
        }
        [Test, Order(34)]
        public void SplashScreenClosingMode_Test07() {
            RealWindow.Show();
            DispatcherHelper.DoEvents();
            DXSplashScreen.Show(CreateDefaultWindow, CreateDefaultContent, new object[] { new SplashScreenOwner(RealWindow), SplashScreenClosingMode.OnParentClosed }, null);
            SplashScreenTestUserControl.DoEvents();
            Assert.IsTrue(DXSplashScreen.IsActive);
            CloseDXSplashScreen();
            Assert.IsFalse(DXSplashScreen.IsActive);
            RealWindow.Close();
        }
        [Test, Order(35)]
        public void NonInitializedCallbacks_Test00() {
            EnsureSplashScreenContainer(true);
            DXSplashScreen.UIThreadDelay = 0;
            DXSplashScreen.Show<SplashScreenTestUserControl>();
            DXSplashScreen.SetState("init");
            DXSplashScreen.Progress(1, 2);
            Assert.IsNull(SplashScreenTestUserControl.Instance);
            var info = DXSplashScreen.SplashContainer.ActiveInfo;
            info.WaitEvent.Set();
            EnqueueWait(() => SplashScreenTestUserControl.Instance != null);
            SplashScreenTestUserControl.DoEvents();
            Assert.IsNotNull(SplashScreenTestUserControl.Instance);
            Assert.IsNotNull(SplashScreenTestUserControl.ViewModel);
            Assert.AreEqual("init", SplashScreenTestUserControl.ViewModel.State);
            Assert.AreEqual(1, SplashScreenTestUserControl.ViewModel.Progress);
            Assert.AreEqual(2, SplashScreenTestUserControl.ViewModel.MaxProgress);
            DispatcherHelper.DoEvents();
        }
        [Test, Order(36)]
        public void NonInitializedCallbacks_Test01() {
            EnsureSplashScreenContainer(true);
            DXSplashScreen.UIThreadDelay = 0;
            DXSplashScreen.Show<SplashScreenTestUserControl>();
            Assert.IsTrue(DXSplashScreen.IsActive);
            var info = DXSplashScreen.SplashContainer.ActiveInfo;
            DXSplashScreen.Close();
            Assert.IsFalse(DXSplashScreen.IsActive);
            Assert.IsTrue(info.IsActive);
            info.WaitEvent.Set();
            EnqueueWait(() => !info.IsActive);
        }
        [Test, Order(37)]
        public void NonInitializedCallbacks_Test02() {
            EnsureSplashScreenContainer(true);
            DXSplashScreen.NotInitializedStateMethodCallPolicy = NotInitializedStateMethodCallPolicy.Discard;
            DXSplashScreen.UIThreadDelay = 0;
            DXSplashScreen.Show<SplashScreenTestUserControl>();
            DXSplashScreen.SetState("init");
            DXSplashScreen.Progress(1, 2);
            Assert.IsNull(SplashScreenTestUserControl.Instance);
            var info = DXSplashScreen.SplashContainer.ActiveInfo;
            info.WaitEvent.Set();
            EnqueueWait(() => SplashScreenTestUserControl.Instance != null);
            SplashScreenTestUserControl.DoEvents();
            Assert.IsNotNull(SplashScreenTestUserControl.Instance);
            Assert.IsNotNull(SplashScreenTestUserControl.ViewModel);
            Assert.AreEqual(SplashScreenViewModel.StateDefaultValue, SplashScreenTestUserControl.ViewModel.State);
            Assert.AreEqual(SplashScreenViewModel.ProgressDefaultValue, SplashScreenTestUserControl.ViewModel.Progress);
            Assert.AreEqual(SplashScreenViewModel.MaxProgressDefaultValue, SplashScreenTestUserControl.ViewModel.MaxProgress);
            DispatcherHelper.DoEvents();
        }
        [Test, Order(38)]
        public void NonInitializedCallbacks_Test03() {
            EnsureSplashScreenContainer(true);
            DXSplashScreen.NotInitializedStateMethodCallPolicy = NotInitializedStateMethodCallPolicy.Discard;
            DXSplashScreen.UIThreadDelay = 0;
            DXSplashScreen.Show<SplashScreenTestUserControl>();
            var info = DXSplashScreen.SplashContainer.ActiveInfo;
            Assert.IsTrue(DXSplashScreen.IsActive);
            DXSplashScreen.Close();
            Assert.IsFalse(DXSplashScreen.IsActive);
            Assert.IsTrue(info.IsActive);
            info.WaitEvent.Set();
            EnqueueWait(() => SplashScreenTestUserControl.Instance != null);
            SplashScreenTestUserControl.DoEvents();
            DispatcherHelper.DoEvents();
            EnqueueDelay(2000);
            Assert.IsTrue(info.IsActive);
            info.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(info.SplashScreen.Close));
            SplashScreenTestsHelper.JoinThread(info);
        }
        [Test, Order(39)]
        public void NonInitializedCallbacks_Test04() {
            EnsureSplashScreenContainer(true);
            DXSplashScreen.NotInitializedStateMethodCallPolicy = NotInitializedStateMethodCallPolicy.Exception;
            DXSplashScreen.UIThreadDelay = 0;
            DXSplashScreen.Show<SplashScreenTestUserControl>();
            Assert.Throws<InvalidOperationException>(() => {
                DXSplashScreen.SetState("state");
            });
            Assert.Throws<InvalidOperationException>(() => {
                DXSplashScreen.Progress(1, 2);
            });
            Assert.Throws<InvalidOperationException>(() => {
                DXSplashScreen.Close();
            });
            Assert.Throws<InvalidOperationException>(() => {
                DXSplashScreen.CallSplashScreenMethod<Window>(x => { });
            });

            var info = DXSplashScreen.SplashContainer.ActiveInfo;
            info.WaitEvent.Set();
            EnqueueWait(() => SplashScreenTestUserControl.Instance != null);
        }
        [Test, Order(40)]
        public void NonInitializedCallbacks_Test05() {
            EnsureSplashScreenContainer(true);
            DXSplashScreen.UIThreadDelay = 0;
            DXSplashScreen.Show<SplashScreenTestWindow>();
            Assert.IsNull(SplashScreenTestWindow.Instance);
            DXSplashScreen.SetState("init");
            DXSplashScreen.Progress(2, 3);
            DXSplashScreen.CallSplashScreenMethod<SplashScreenTestWindow>(x => x.Text("init"));
            var info = DXSplashScreen.SplashContainer.ActiveInfo;
            info.WaitEvent.Set();
            EnqueueWait(() => SplashScreenTestWindow.Instance != null);
            SplashScreenTestWindow.DoEvents();
            Assert.IsNotNull(SplashScreenTestWindow.Instance);
            Assert.AreEqual("init", SplashScreenTestWindow.Instance.TextProp);
            Assert.AreEqual(2, SplashScreenTestWindow.Instance.Progress);
            DispatcherHelper.DoEvents();
        }
        [Test, Order(41)]
        public void NonInitializedCallbacks_Test06() {
            EnsureSplashScreenContainer(true);
            DXSplashScreen.UIThreadDelay = 0;
            DXSplashScreen.Show<SplashScreenTestWindow>();
            Assert.IsTrue(DXSplashScreen.IsActive);
            Assert.IsNull(SplashScreenTestWindow.Instance);
            var info = DXSplashScreen.SplashContainer.ActiveInfo;
            DXSplashScreen.Close();
            Assert.IsFalse(DXSplashScreen.IsActive);
            Assert.IsTrue(info.IsActive);
            info.WaitEvent.Set();
            EnqueueWait(() => SplashScreenTestWindow.Instance != null && SplashScreenTestWindow.Instance.CloseRequested);
        }
        [Test, Order(43)]
        public void NonInitializedCallbacks_Test07() {
            EnsureSplashScreenContainer(true);
            DXSplashScreen.UIThreadDelay = 0;
            DXSplashScreen.NotInitializedStateMethodCallPolicy = NotInitializedStateMethodCallPolicy.Discard;
            DXSplashScreen.Show<SplashScreenTestWindow>();
            Assert.IsNull(SplashScreenTestWindow.Instance);
            DXSplashScreen.SetState("init");
            DXSplashScreen.Progress(2, 3);
            DXSplashScreen.CallSplashScreenMethod<SplashScreenTestWindow>(x => x.Text("init"));
            var info = DXSplashScreen.SplashContainer.ActiveInfo;
            info.WaitEvent.Set();
            EnqueueWait(() => SplashScreenTestWindow.Instance != null);
            SplashScreenTestWindow.DoEvents();
            Assert.IsNotNull(SplashScreenTestWindow.Instance);
            Assert.IsNull(SplashScreenTestWindow.Instance.TextProp);
            Assert.AreEqual(double.NaN, SplashScreenTestWindow.Instance.Progress);
            DispatcherHelper.DoEvents();
        }
        [Test, Order(44)]
        public void NonInitializedCallbacks_Test08() {
            EnsureSplashScreenContainer(true);
            DXSplashScreen.UIThreadDelay = 0;
            DXSplashScreen.NotInitializedStateMethodCallPolicy = NotInitializedStateMethodCallPolicy.Discard;
            DXSplashScreen.Show<SplashScreenTestWindow>();
            Assert.IsNull(SplashScreenTestWindow.Instance);
            var info = DXSplashScreen.SplashContainer.ActiveInfo;
            Assert.IsTrue(DXSplashScreen.IsActive);
            DXSplashScreen.Close();
            Assert.IsFalse(DXSplashScreen.IsActive);
            Assert.IsTrue(info.IsActive);
            info.WaitEvent.Set();
            EnqueueWait(() => SplashScreenTestWindow.Instance != null);
            SplashScreenTestWindow.DoEvents();
            DispatcherHelper.DoEvents();
            EnqueueDelay(2000);
            Assert.IsTrue(info.IsActive);
            Assert.IsFalse(SplashScreenTestWindow.Instance.CloseRequested);
            info.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(info.SplashScreen.Close));
            SplashScreenTestsHelper.JoinThread(info);
        }
        [Test, Order(45)]
        public void NonInitializedCallbacks_Test09() {
            EnsureSplashScreenContainer(true);
            DXSplashScreen.NotInitializedStateMethodCallPolicy = NotInitializedStateMethodCallPolicy.Exception;
            DXSplashScreen.UIThreadDelay = 0;
            DXSplashScreen.Show<SplashScreenTestWindow>();
            Assert.Throws<InvalidOperationException>(() => {
                DXSplashScreen.SetState("state");
            });
            Assert.Throws<InvalidOperationException>(() => {
                DXSplashScreen.Progress(1, 2);
            });
            Assert.Throws<InvalidOperationException>(() => {
                DXSplashScreen.Close();
            });
            Assert.Throws<InvalidOperationException>(() => {
                DXSplashScreen.CallSplashScreenMethod<Window>(x => { });
            });

            var info = DXSplashScreen.SplashContainer.ActiveInfo;
            info.WaitEvent.Set();
            EnqueueWait(() => SplashScreenTestWindow.Instance != null);
            SplashScreenTestWindow.DoEvents();
        }

        static Window CreateDefaultWindow(object parameter) {
            return new Window() { Width = 160, Height = 180 };
        }
        static object CreateDefaultContent(object parameter) {
            return new SplashScreenTestUserControl();
        }
        void EnsureSplashScreenContainer(bool waitForMainThread) {
            if(DXSplashScreen.SplashContainer == null)
                DXSplashScreen.SplashContainer = new DXSplashScreen.SplashScreenContainer();
            if(waitForMainThread)
                DXSplashScreen.SplashContainer.ActiveInfo.WaitEvent = new AutoResetEvent(false);
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
        [Test]
        public void ShowWindowNotISplashScreen2() {
            ISplashScreenService service = new DXSplashScreenService() {
                SplashScreenType = typeof(Window),
            };
            Assert.Throws<InvalidOperationException>(() => {
                service.ShowSplashScreen();
            });
        }
        [Test]
        public void ShowUserControl() {
            ISplashScreenService service = SplashScreenTestsHelper.CreateDefaultService();
            ShowUserControlCore(service);
        }
        [Test]
        public void BindServiceProperties() {
            var service = SplashScreenTestsHelper.CreateDefaultService();
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
            ISplashScreenService service = SplashScreenTestsHelper.CreateDefaultService();
            service.ShowSplashScreen();
            SplashScreenTestUserControl.DoEvents();
            Window wnd = SplashScreenTestUserControl.Window;
            AutoResetEvent SyncEvent = new AutoResetEvent(false);
            bool hasError = true;
            wnd.Dispatcher.BeginInvoke(new Action(() => {
                hasError = false;
                hasError = hasError | !(WindowStartupLocation.CenterScreen == wnd.WindowStartupLocation);
                hasError = hasError | !(true == HasFadeAnimation(wnd));
                hasError = hasError | !(null == wnd.Style);
                hasError = hasError | !(WindowStyle.None == wnd.WindowStyle);
                hasError = hasError | !(ResizeMode.NoResize == wnd.ResizeMode);
                hasError = hasError | !(true == wnd.AllowsTransparency);
                hasError = hasError | !(Colors.Transparent == ((SolidColorBrush)wnd.Background).Color);
                hasError = hasError | !(false == wnd.ShowInTaskbar);
                hasError = hasError | !(true == wnd.Topmost);
                hasError = hasError | !(false == wnd.IsActive);
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
            CheckWindowStyle(wndStyle, SplashScreenTestUserControl.Window);
            service.HideSplashScreen();
        }
        [Test]
        public void ShowUserControlAndCheckWindowProperties3() {
            Style wndStyle = new Style();
            ISplashScreenService service = new DXSplashScreenService() {
                SplashScreenType = typeof(SplashScreenTestUserControl),
                SplashScreenWindowStyle = wndStyle,
                SplashScreenStartupLocation = WindowStartupLocation.Manual,
            };
            service.ShowSplashScreen();
            SplashScreenTestUserControl.DoEvents();
            CheckWindowStyle(wndStyle, SplashScreenTestUserControl.Window);
            service.HideSplashScreen();
        }
        [Test]
        public void ShowUserControlAndCheckWindowProperties4() {
            Style wndStyle = new Style();
            wndStyle.Setters.Add(new Setter(System.Windows.Window.ShowActivatedProperty, false));
            wndStyle.Setters.Add(new Setter(System.Windows.Window.ShowInTaskbarProperty, true));
            ISplashScreenService service = new DXSplashScreenService() {
                SplashScreenType = typeof(SplashScreenTestWindow),
                SplashScreenWindowStyle = wndStyle,
                SplashScreenStartupLocation = WindowStartupLocation.Manual,
            };
            service.ShowSplashScreen();
            SplashScreenTestWindow.DoEvents();
            CheckWindowStyle(wndStyle, SplashScreenTestWindow.Instance);
            service.HideSplashScreen();
        }
        void CheckWindowStyle(Style style, Window wnd) {
            AutoResetEvent SyncEvent = new AutoResetEvent(false);
            bool hasError = true;
            wnd.Dispatcher.BeginInvoke(new Action(() => {
                hasError = false;
                hasError = hasError | !(WindowStartupLocation.Manual == wnd.WindowStartupLocation);
                hasError = hasError | !(true == HasFadeAnimation(wnd));
                hasError = hasError | !(style == wnd.Style);
                hasError = hasError | !(WindowStyle.SingleBorderWindow == wnd.WindowStyle);
                hasError = hasError | !(ResizeMode.CanResize == wnd.ResizeMode);
                hasError = hasError | !(false == wnd.AllowsTransparency);
                hasError = hasError | !(true == wnd.ShowInTaskbar);
                hasError = hasError | !(false == wnd.Topmost);
                hasError = hasError | !(false == wnd.IsActive);
                hasError = hasError | !(SizeToContent.Manual == wnd.SizeToContent);
                SyncEvent.Set();
            }));
            SyncEvent.WaitOne(TimeSpan.FromSeconds(2));
            Assert.IsFalse(hasError);
        }
        [Test]
        public void ViewTemplateShouldBeSealed() {
            DataTemplate temp = SplashScreenTestsHelper.CreateDefaultTemplate();
            Assert.IsFalse(temp.IsSealed);
            DXSplashScreenService service = new DXSplashScreenService() {
                ViewTemplate = temp,
            };
            Assert.IsTrue(temp.IsSealed);
        }
        [Test]
        public void ViewTemplateSelectorIsNotSupported() {
            Assert.Throws<InvalidOperationException>(() => {
                DXSplashScreenService service = new DXSplashScreenService() {
                    ViewTemplateSelector = new DataTemplateSelector(),
                };
            });
        }
#if !DXCORE3
        [Test]
#endif
        public void WindowShouldBeActivatedOnCloseSplashScreen_Test() {
            DXSplashScreenService service = CreateDefaultSplashScreenAndShow();
            var wnd = SplashScreenTestUserControl.Window;
            Assert.IsTrue(RealWindow.IsActive);
            wnd.Dispatcher.BeginInvoke(new Action(() => {
                wnd.Activate();
            }));
            SplashScreenTestUserControl.DoEvents();
            Assert.IsFalse(RealWindow.IsActive, "RealWindow - !IsActive");
            wnd.Dispatcher.BeginInvoke(new Action(() => {
                Assert.IsTrue(wnd.IsActive, "SplashScreen - IsActive");
            }));

            CloseDXSplashScreen();
            DispatcherHelper.DoEvents();
            Assert.IsTrue(RealWindow.IsActive, "RealWindow - IsActive");
        }
        [Test]
        public void SplashScreenOwnerPriority_Test00() {
            var fakeOwner = new Border();
            DXSplashScreenService service = CreateDefaultSplashScreenAndShow(fakeOwner, activateWindow:false);
            var info = DXSplashScreen.SplashContainer.ActiveInfo;
            Assert.AreEqual(fakeOwner, info.Owner.WindowObject);
            Assert.IsFalse(info.Owner.IsInitialized);
            Assert.IsNull(info.Owner.Window);
        }
        [Test]
        public void SplashScreenOwnerPriority_Test01() {
            var fakeOwner = new Border();
            DXSplashScreenService service = CreateDefaultSplashScreenAndShow(RealWindow, activateWindow: false, windowContent:fakeOwner);
            var info = DXSplashScreen.SplashContainer.ActiveInfo;
            Assert.AreEqual(RealWindow, info.Owner.WindowObject);
            Assert.IsTrue(info.Owner.IsInitialized);
            Assert.AreEqual(RealWindow, info.Owner.Window);
            Assert.AreEqual(new WindowInteropHelper(RealWindow).Handle, info.Owner.Handle);
        }
        [Test]
        public void SplashScreenOwnerPriority_Test02() {
            var fakeOwner = new Border();
            DXSplashScreenService service = CreateDefaultSplashScreenAndShow(null, activateWindow: false, windowContent: fakeOwner);
            var info = DXSplashScreen.SplashContainer.ActiveInfo;
            Assert.AreEqual(fakeOwner, info.Owner.WindowObject);
            Assert.IsTrue(info.Owner.IsInitialized);
            Assert.AreEqual(RealWindow, info.Owner.Window);
        }
        [Test]
        public void SplashScreenOwnerPriority_Test03() {
            var fakeOwner = new Border();
            DXSplashScreenService service = CreateDefaultSplashScreenAndShow(RealWindow, SplashScreenOwnerSearchMode.IgnoreAssociatedObject, false, fakeOwner);
            var info = DXSplashScreen.SplashContainer.ActiveInfo;
            Assert.AreEqual(RealWindow, info.Owner.WindowObject);
            Assert.IsTrue(info.Owner.IsInitialized);
            Assert.AreEqual(RealWindow, info.Owner.Window);
        }
        [Test]
        public void SplashScreenOwnerPriority_Test04() {
            var service = CreateDefaultSplashScreenAndShow(null, SplashScreenOwnerSearchMode.OwnerOnly);
            var info = DXSplashScreen.SplashContainer.ActiveInfo;
            Assert.IsNull(info.Owner);
            Assert.IsNull(info.RelationInfo);
        }
        [Test]
        public void SplashScreenOwnerPriority_Test05() {
            var service = CreateDefaultSplashScreenAndShow(RealWindow, SplashScreenOwnerSearchMode.OwnerOnly);
            var info = DXSplashScreen.SplashContainer.ActiveInfo;
            Assert.AreEqual(RealWindow, info.Owner.WindowObject);
            Assert.IsTrue(info.Owner.IsInitialized);
            Assert.AreEqual(RealWindow, info.Owner.Window);
        }
        [Test]
        public void SplashScreenClosingMode_Test00() {
            var service = CreateDefaultSplashScreenAndShow(null);
            Assert.IsTrue(DXSplashScreen.IsActive);
            RealWindow.Close();
            SplashScreenTestUserControl.DoEvents();
            DispatcherHelper.DoEvents();
            Assert.IsFalse(DXSplashScreen.IsActive);
        }
        [Test]
        public void SplashScreenClosingMode_Test01() {
            var service = CreateDefaultSplashScreenAndShow(null, SplashScreenOwnerSearchMode.Full, true, null, SplashScreenClosingMode.OnParentClosed);
            Assert.IsTrue(DXSplashScreen.IsActive);
            RealWindow.Close();
            SplashScreenTestUserControl.DoEvents();
            DispatcherHelper.DoEvents();
            Assert.IsFalse(DXSplashScreen.IsActive);
        }
        [Test]
        public void SplashScreenClosingMode_Test02() {
            var service = CreateDefaultSplashScreenAndShow(null, SplashScreenOwnerSearchMode.Full, true, null, SplashScreenClosingMode.ManualOnly);
            Assert.IsTrue(DXSplashScreen.IsActive);
            RealWindow.Close();
            SplashScreenTestUserControl.DoEvents();
            DispatcherHelper.DoEvents();
            Assert.IsTrue(DXSplashScreen.IsActive);
            CloseDXSplashScreen();
        }

        [Test]
        public void ShowSplashScreenImmediatelAfterClose_Test00_T186338() {
            ISplashScreenService service = SplashScreenTestsHelper.CreateDefaultService();
            service.ShowSplashScreen();
            SplashScreenTestUserControl.DoEvents();
            Assert.IsTrue(service.IsSplashScreenActive);
            service.HideSplashScreen();
            Assert.IsFalse(service.IsSplashScreenActive);
            service.ShowSplashScreen();
            Assert.IsTrue(service.IsSplashScreenActive);
            SplashScreenTestUserControl.DoEvents();
            service.HideSplashScreen();
            Assert.IsFalse(service.IsSplashScreenActive);
        }
        [Test]
        public void ShowSplashScreenImmediatelAfterClose_Test01_T186338() {
            ISplashScreenService service = SplashScreenTestsHelper.CreateDefaultService();
            ISplashScreenService service1 = SplashScreenTestsHelper.CreateDefaultService();
            service.ShowSplashScreen();
            SplashScreenTestUserControl.DoEvents();
            service.HideSplashScreen();
            Assert.IsFalse(service.IsSplashScreenActive);
            service1.ShowSplashScreen();
            Assert.IsTrue(service1.IsSplashScreenActive);
            SplashScreenTestUserControl.DoEvents();
            service1.HideSplashScreen();
        }
        [Test]
        public void SplashScreenStartupLocationCenterOwner_Test00() {
            var service = SplashScreenTestsHelper.CreateDefaultService();
            var windowStyle = new Style(typeof(Window));
            windowStyle.Setters.Add(new Setter(FrameworkElement.WidthProperty, 180d));
            windowStyle.Setters.Add(new Setter(FrameworkElement.HeightProperty, 160d));
            service.SplashScreenWindowStyle = windowStyle;
            service.SplashScreenOwner = RealWindow;
            service.SplashScreenStartupLocation = WindowStartupLocation.CenterOwner;
            RealWindow.WindowStartupLocation = WindowStartupLocation.Manual;
            RealWindow.Left = 400;
            RealWindow.Width = 200;
            RealWindow.Top = 200;
            RealWindow.Height = 300;
            RealWindow.Show();
            DispatcherHelper.DoEvents();
            service.ShowSplashScreen();
            SplashScreenTestUserControl.DoEvents();
            Rect pos = GetSplashScreenBounds();
            SplashScreenTestUserControl.DoEvents();
            Assert.AreEqual(true, AreRectsClose(new Rect(410, 270, 180, 160), pos));
            CloseDXSplashScreen();
        }
        [Test]
        public void SplashScreenStartupLocationCenterOwner_Test01() {
            SplashScreenTestUserControl.TemplateCreator = SplashScreenTestsHelper.CreateControlTemplate;
            var service = SplashScreenTestsHelper.CreateDefaultService();
            service.SplashScreenStartupLocation = WindowStartupLocation.CenterOwner;
            service.SplashScreenOwner = RealWindow;
            RealWindow.WindowStartupLocation = WindowStartupLocation.Manual;
            RealWindow.Left = 200;
            RealWindow.Width = 300;
            RealWindow.Top = 100;
            RealWindow.Height = 240;
            RealWindow.Show();
            DispatcherHelper.DoEvents();
            service.ShowSplashScreen();
            SplashScreenTestUserControl.DoEvents();
            Rect pos = GetSplashScreenBounds();
            SplashScreenTestUserControl.DoEvents();
            Assert.AreEqual(true, AreRectsClose(new Rect(290, 150, 120, 140), pos));
            CloseDXSplashScreen();
        }
        [Test]
        public void SplashScreenStartupLocationCenterOwner_Test02() {
            var service = SplashScreenTestsHelper.CreateDefaultService();
            service.SplashScreenStartupLocation = WindowStartupLocation.CenterOwner;
            RealWindow.Show();
            DispatcherHelper.DoEvents();
            service.ShowSplashScreen();
            SplashScreenTestUserControl.DoEvents();
            var pos = GetSplashScreenStartupLocation();
            Assert.AreEqual(WindowStartupLocation.CenterOwner, pos.Value);
            CloseDXSplashScreen();
        }
        [Test]
        public void FadeDuration_Test00() {
            var service = SplashScreenTestsHelper.CreateDefaultService();
            service.ShowSplashScreen();
            SplashScreenTestUserControl.DoEvents();
            WindowFadeAnimationBehavior fb = null;
            var spScr = DXSplashScreen.SplashContainer.ActiveInfo.SplashScreen;
            SplashScreenHelper.InvokeAsync(spScr, () => fb = Interaction.GetBehaviors(spScr).OfType<WindowFadeAnimationBehavior>().First());
            SplashScreenTestUserControl.DoEvents();
            Assert.IsNotNull(fb);
        }
        [Test]
        public void FadeDuration_Test01() {
            var service = SplashScreenTestsHelper.CreateDefaultService();
            service.FadeInDuration = TimeSpan.FromMilliseconds(150);
            service.FadeOutDuration = TimeSpan.FromMilliseconds(100);
            service.ShowSplashScreen();
            SplashScreenTestUserControl.DoEvents();
            double fid = 0;
            double fod = 0;
            var spScr = DXSplashScreen.SplashContainer.ActiveInfo.SplashScreen;
            SplashScreenHelper.InvokeAsync(spScr, () => {
                var wfab = Interaction.GetBehaviors(spScr).OfType<WindowFadeAnimationBehavior>().First();
                fod = wfab.FadeOutDuration.TotalMilliseconds;
                fid = wfab.FadeInDuration.TotalMilliseconds;
            });
            SplashScreenTestUserControl.DoEvents();
            Assert.AreEqual(fid, 150);
            Assert.AreEqual(fod, 100);
        }
        [Test]
        public void FadeDuration_Test02() {
            var service = SplashScreenTestsHelper.CreateDefaultService();
            service.FadeInDuration = TimeSpan.FromMilliseconds(150);
            service.FadeOutDuration = TimeSpan.FromMilliseconds(0);
            service.ShowSplashScreen();
            SplashScreenTestUserControl.DoEvents();
            double fid = 0;
            double fod = 0;
            var spScr = DXSplashScreen.SplashContainer.ActiveInfo.SplashScreen;
            SplashScreenHelper.InvokeAsync(spScr, () => {
                var wfab = Interaction.GetBehaviors(spScr).OfType<WindowFadeAnimationBehavior>().First();
                fod = wfab.FadeOutDuration.TotalMilliseconds;
                fid = wfab.FadeInDuration.TotalMilliseconds;
            });
            SplashScreenTestUserControl.DoEvents();
            Assert.AreEqual(fid, 150);
            Assert.AreEqual(fod, 0);
        }
        [Test]
        public void FadeDuration_Test03() {
            var service = SplashScreenTestsHelper.CreateDefaultService();
            service.FadeInDuration = TimeSpan.FromMilliseconds(0);
            service.FadeOutDuration = TimeSpan.FromMilliseconds(100);
            service.ShowSplashScreen();
            SplashScreenTestUserControl.DoEvents();
            double fid = 0;
            double fod = 0;
            var spScr = DXSplashScreen.SplashContainer.ActiveInfo.SplashScreen;
            SplashScreenHelper.InvokeAsync(spScr, () => {
                var wfab = Interaction.GetBehaviors(spScr).OfType<WindowFadeAnimationBehavior>().First();
                fod = wfab.FadeOutDuration.TotalMilliseconds;
                fid = wfab.FadeInDuration.TotalMilliseconds;
            });
            SplashScreenTestUserControl.DoEvents();
            Assert.AreEqual(fid, 0);
            Assert.AreEqual(fod, 100);
        }
        [Test]
        public void FadeDuration_Test04() {
            var service = SplashScreenTestsHelper.CreateDefaultService();
            service.FadeInDuration = TimeSpan.FromMilliseconds(0);
            service.FadeOutDuration = TimeSpan.FromMilliseconds(0);
            service.ShowSplashScreen();
            SplashScreenTestUserControl.DoEvents();
            WindowFadeAnimationBehavior fb = null;
            var spScr = DXSplashScreen.SplashContainer.ActiveInfo.SplashScreen;
            SplashScreenHelper.InvokeAsync(spScr, () => fb = Interaction.GetBehaviors(spScr).OfType<WindowFadeAnimationBehavior>().FirstOrDefault());
            SplashScreenTestUserControl.DoEvents();
            Assert.IsNull(fb);
        }

        [Test]
        public void UseIndependentWindow_Test00() {
            var service = SplashScreenTestsHelper.CreateDefaultService();
            service.ShowSplashScreen();
            Assert.IsNull(service.GetSplashContainer(false));
            AssertIsActive(service, true);
            Assert.IsTrue(DXSplashScreen.IsActive);
            SplashScreenTestsHelper.CloseSplashScreenService(service);
        }
        [Test]
        public void UseIndependentWindow_Test01() {
            var service = SplashScreenTestsHelper.CreateDefaultService();
            service.UseIndependentWindow = true;
            service.ShowSplashScreen();
            Assert.IsNotNull(service.GetSplashContainer(false));
            AssertIsActive(service, true);
            Assert.IsFalse(DXSplashScreen.IsActive);
            SplashScreenTestsHelper.CloseSplashScreenService(service);
        }
        [Test]
        public void UseIndependentWindow_Test02() {
            var service = SplashScreenTestsHelper.CreateDefaultService();
            service.ShowSplashScreen();
            AssertHelper.AssertThrows<InvalidOperationException>(() => service.UseIndependentWindow = true);
            SplashScreenTestsHelper.CloseSplashScreenService(service);
        }
        [Test]
        public void UseIndependentWindow_Test03() {
            var service = SplashScreenTestsHelper.CreateDefaultService();
            var service1 = SplashScreenTestsHelper.CreateDefaultService();
            service.ShowSplashScreen();
            AssertIsActive(service, true);
            AssertIsActive(service1, true);

            service1.UseIndependentWindow = true;
            AssertIsActive(service1, false);
            service1.ShowSplashScreen();
            AssertIsActive(service1, true);

            SplashScreenTestsHelper.CloseSplashScreenService(service);
            AssertIsActive(service, false);
            AssertIsActive(service1, true);

            SplashScreenTestsHelper.CloseSplashScreenService(service1);
            AssertIsActive(service1, false);
        }
        [Test]
        public void UseIndependentWindow_Test04() {
            var service = SplashScreenTestsHelper.CreateDefaultService();
            var service1 = SplashScreenTestsHelper.CreateDefaultService();
            service.ShowSplashScreen();
            AssertIsActive(service, true);
            AssertIsActive(service1, true);

            service1.UseIndependentWindow = true;
            AssertIsActive(service1, false);
            service1.ShowSplashScreen();
            AssertIsActive(service1, true);

            SplashScreenTestsHelper.CloseSplashScreenService(service1);
            AssertIsActive(service, true);
            AssertIsActive(service1, false);

            SplashScreenTestsHelper.CloseSplashScreenService(service);
            AssertIsActive(service, false);
        }
        [Test]
        public void UseIndependentWindow_Test05() {
            var service = SplashScreenTestsHelper.CreateDefaultService();
            var service1 = SplashScreenTestsHelper.CreateDefaultService();
            var service2 = SplashScreenTestsHelper.CreateDefaultService();
            service.UseIndependentWindow = true;
            service1.UseIndependentWindow = true;
            service2.UseIndependentWindow = true;
            service.ShowSplashScreen();

            AssertIsActive(service, true);
            AssertIsActive(service1, false);
            AssertIsActive(service2, false);

            service1.ShowSplashScreen();
            AssertIsActive(service, true);
            AssertIsActive(service1, true);
            AssertIsActive(service2, false);

            service2.ShowSplashScreen();
            AssertIsActive(service, true);
            AssertIsActive(service1, true);
            AssertIsActive(service2, true);
            Assert.IsFalse(DXSplashScreen.IsActive);

            SplashScreenTestsHelper.CloseSplashScreenService(service);
            SplashScreenTestsHelper.CloseSplashScreenService(service1);
            AssertIsActive(service, false);
            AssertIsActive(service1, false);
            AssertIsActive(service2, true);

            SplashScreenTestsHelper.CloseSplashScreenService(service2);
            AssertIsActive(service2, false);
        }
        [Test]
        public void UseIndependentWindow_Test06() {
            ISplashScreenService service = new DXSplashScreenService() {
                SplashScreenType = typeof(SplashScreenTestWindow),
                UseIndependentWindow = true
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
            AssertHelper.AssertThrows<InvalidOperationException>(() => DXSplashScreen.Progress(10, 20));
            SplashScreenTestWindow.DoEvents();
            Assert.AreEqual(100, SplashScreenTestWindow.Instance.Progress);
            SplashScreenTestsHelper.CloseSplashScreenService(service);
        }
        [Test, Ignore("Ignore")]
        public void UseIndependentWindow_Test07() {
            ISplashScreenService service = new DXSplashScreenService() {
                ViewTemplate = SplashScreenTestsHelper.CreateDefaultTemplate(),
                UseIndependentWindow = true
            };
            service.ShowSplashScreen();
            SplashScreenTestUserControl.DoEvents();
            Assert.AreEqual(0, SplashScreenTestUserControl.ViewModel.Progress);
            Assert.AreEqual(100, SplashScreenTestUserControl.ViewModel.MaxProgress);
            Assert.AreEqual("Loading...", SplashScreenTestUserControl.ViewModel.State);
            Assert.AreEqual(true, SplashScreenTestUserControl.ViewModel.IsIndeterminate);
            DXSplashScreen.Progress(50);
            DXSplashScreen.SetState("Test");
            SplashScreenTestUserControl.DoEvents();
            Assert.AreEqual(0, SplashScreenTestUserControl.ViewModel.Progress);
            Assert.AreEqual(100, SplashScreenTestUserControl.ViewModel.MaxProgress);
            Assert.AreEqual("Loading...", SplashScreenTestUserControl.ViewModel.State);
            Assert.AreEqual(true, SplashScreenTestUserControl.ViewModel.IsIndeterminate);
            service.SetSplashScreenProgress(50, 100);
            service.SetSplashScreenState("Test");
            SplashScreenTestUserControl.DoEvents();
            Assert.AreEqual(50, SplashScreenTestUserControl.ViewModel.Progress);
            Assert.AreEqual(100, SplashScreenTestUserControl.ViewModel.MaxProgress);
            Assert.AreEqual("Test", SplashScreenTestUserControl.ViewModel.State);
            Assert.AreEqual(false, SplashScreenTestUserControl.ViewModel.IsIndeterminate);
        }
        [Test]
        public void SplashScreenStyleShouldBePatched_T257139_Test00() {
            var fakeOwner = new Border();
            DXSplashScreenService service = CreateDefaultSplashScreenAndShow(fakeOwner, activateWindow: false);
            Assert.AreEqual(SplashScreenHelper.WS_EX_TRANSPARENT, SplashScreenHelper.Test_WindowStyleModifier);
        }
        [Test]
        public void SplashScreenStyleShouldBePatched_T257139_Test01() {
            DXSplashScreen.UseDefaultAltTabBehavior = true;
            DXSplashScreenService service = CreateDefaultSplashScreenAndShow(null, activateWindow: false, ownerSearchMode: SplashScreenOwnerSearchMode.OwnerOnly);
            Assert.AreEqual(0, SplashScreenHelper.Test_WindowStyleModifier);
            DXSplashScreen.UseDefaultAltTabBehavior = false;
        }
        [Test]
        public void SplashScreenStyleShouldBePatched_T257139_T335564_Test02() {
            DXSplashScreenService service = CreateDefaultSplashScreenAndShow(null, activateWindow: false, ownerSearchMode: SplashScreenOwnerSearchMode.OwnerOnly);
            Assert.AreEqual(SplashScreenHelper.WS_EX_TOOLWINDOW, SplashScreenHelper.Test_WindowStyleModifier);
        }
        [Test]
        public void IsSplashScreenActiveDep_Test01_T728696() {
            ISplashScreenService s = new DXSplashScreenService { UseIndependentWindow = false, SplashScreenType = typeof(SplashScreenTestUserControl) };

            Assert.IsFalse(s.IsSplashScreenActive);

            var task = new Task(() => DXSplashScreen.Show(typeof(SplashScreenTestUserControl)));
            task.Start();
            EnqueueWait(() => task.IsCompleted);

            SplashScreenTestUserControl.DoEvents();
            Assert.IsTrue(s.IsSplashScreenActive);
            DXSplashScreen.Close();
            Assert.IsFalse(s.IsSplashScreenActive);
        }
        [Test, Ignore("Unstable")]
        public void UpdateIsSplashScreenActiveOnHiddenWindow_Test00() {
            UpdateIsSplashScreenActiveOnHiddenWindow_TestBase(false);
        }
        [Test, Ignore("Unstable")]
        public void UpdateIsSplashScreenActiveOnHiddenWindow_Test01() {
            UpdateIsSplashScreenActiveOnHiddenWindow_TestBase(true);
        }
        void UpdateIsSplashScreenActiveOnHiddenWindow_TestBase(bool useIndependentWindow) {
            SplashScreenTestWindow1.StartVisibility = Visibility.Hidden;
            ISplashScreenService s = new DXSplashScreenService { UseIndependentWindow = useIndependentWindow, SplashScreenType = typeof(SplashScreenTestWindow1) };
            var descriptor = DependencyPropertyDescriptor.FromProperty(DXSplashScreenService.IsSplashScreenActiveProperty, typeof(DXSplashScreenService));
            List<bool> eventValues = new List<bool>();
            var handler = new EventHandler((o, e) => eventValues.Add(s.IsSplashScreenActive));
            descriptor.AddValueChanged(s, handler);
            try {
                s.ShowSplashScreen();
                EnqueueWait(() => eventValues.Count == 2);
                Assert.IsTrue(eventValues[0]);
                Assert.IsFalse(eventValues[1]);
            } finally {
                descriptor.RemoveValueChanged(s, handler);
            }
        }
        [Test]
        public void UpdateIsSplashScreenActiveOnHiddenWindow_Test02() {
            SplashScreenTestWindow1.StartVisibility = Visibility.Collapsed;
            ISplashScreenService s = new DXSplashScreenService { UseIndependentWindow = false, SplashScreenType = typeof(SplashScreenTestWindow1) };
            s.ShowSplashScreen();
            SplashScreenTestWindow.DoEvents();
            Assert.IsTrue(s.IsSplashScreenActive);
            s.HideSplashScreen();
            EnqueueWait(() => !s.IsSplashScreenActive);
            Assert.IsFalse(s.IsSplashScreenActive);
        }
        [Test]
        public void UpdateIsSplashScreenActiveOnHiddenWindow_Test03() {
            SplashScreenTestWindow1.StartVisibility = Visibility.Collapsed;
            ISplashScreenService s = new DXSplashScreenService { UseIndependentWindow = true, SplashScreenType = typeof(SplashScreenTestWindow1) };
            s.ShowSplashScreen();
            SplashScreenTestWindow.DoEvents();
            Assert.IsTrue(s.IsSplashScreenActive);
            s.HideSplashScreen();
            EnqueueWait(() => !s.IsSplashScreenActive);
            Assert.IsFalse(s.IsSplashScreenActive);
        }
        [Test]
        public void UpdateIsSplashScreenActiveOnHiddenWindow_Test04() {
            ISplashScreenService s = new DXSplashScreenService { UseIndependentWindow = false, SplashScreenType = typeof(SplashScreenTestWindow) };
            s.ShowSplashScreen();
            SplashScreenTestWindow.DoEvents();
            Assert.IsTrue(s.IsSplashScreenActive);

            SplashScreenTestWindow.Instance.Dispatcher.BeginInvoke(new Action(() => SplashScreenTestWindow.Instance.Close()));
            EnqueueWait(() => !s.IsSplashScreenActive);
            Assert.IsFalse(s.IsSplashScreenActive);
        }
        [Test]
        public void UpdateIsSplashScreenActiveOnHiddenWindow_Test05() {
            ISplashScreenService s = new DXSplashScreenService { UseIndependentWindow = true, SplashScreenType = typeof(SplashScreenTestWindow) };
            s.ShowSplashScreen();
            SplashScreenTestWindow.DoEvents();
            Assert.IsTrue(s.IsSplashScreenActive);

            SplashScreenTestWindow.Instance.Dispatcher.BeginInvoke(new Action(() => SplashScreenTestWindow.Instance.Close()));
            EnqueueWait(() => !s.IsSplashScreenActive);
            Assert.IsFalse(s.IsSplashScreenActive);
        }
        class SplashScreenTestWindow1 : SplashScreenTestWindow {
            internal static Visibility StartVisibility { get; set; } = Visibility.Hidden;
            public SplashScreenTestWindow1() {
                Visibility = StartVisibility;
            }
        }

        void AssertIsActive(DXSplashScreenService service, bool checkValue) {
            Assert.AreEqual(checkValue, ((ISplashScreenService)service).IsSplashScreenActive);
        }
        DXSplashScreenService CreateDefaultSplashScreenAndShow(FrameworkElement owner = null, SplashScreenOwnerSearchMode ownerSearchMode = SplashScreenOwnerSearchMode.Full,
                bool activateWindow = true, FrameworkElement windowContent = null, SplashScreenClosingMode? closingMode = null) {
            return SplashScreenTestsHelper.CreateDefaultSplashScreenAndShow(RealWindow, owner, ownerSearchMode, activateWindow, windowContent, closingMode);
        }
    }
#if !DXCORE3
    [TestFixture, Platform("NET")]
    public class DXSplashScreenServiceIsolatedAppDomainTests {
        Window RealWindow { get; set; }

        [Test]
        public void SplashScreenOwnerPriority_Test04() {
            IsolatedDomainTestHelper.RunTest(() => {
                Application app = new Application();
                var window = new Window();
                app.MainWindow = window;
                app.Startup += (o, e) => {
                    RealWindow = new Window();
                    window.Show();
                    DispatcherHelper.UpdateLayoutAndDoEvents(window);
                    RealWindow.Show();
                    if(!RealWindow.IsActive)
                        RealWindow.Activate();
                    DispatcherHelper.UpdateLayoutAndDoEvents(RealWindow);
                    var fakeOwner = new Border();
                    DXSplashScreenService service = SplashScreenTestsHelper.CreateDefaultSplashScreenAndShow(null, null, SplashScreenOwnerSearchMode.IgnoreAssociatedObject, true, fakeOwner);
                    var info = DXSplashScreen.SplashContainer.ActiveInfo;
                    Assert.IsNotNull(info.Owner.WindowObject);
                    Assert.AreEqual(SplashScreenHelper.GetApplicationActiveWindow(true), info.Owner.WindowObject);
                    Assert.IsTrue(info.Owner.IsInitialized);
                    Assert.AreEqual(RealWindow, info.Owner.Window);
                    SplashScreenTestsHelper.CloseDXSplashScreen();
                    RealWindow.Close();
                    RealWindow.Content = null;
                    DispatcherHelper.UpdateLayoutAndDoEvents(RealWindow);
                    RealWindow = null;
                    window.Close();
                    DispatcherHelper.UpdateLayoutAndDoEvents(window);
                };
                app.Run();
            });

        }
        [Test]
        public void CloseOnAppUnhandledException_Test_T149746() {
            IsolatedDomainTestHelper.RunTest(() => {
                Assert.IsNull(DXSplashScreen.SplashContainer);
                try {
                    Application app = new Application();
                    app.Startup += (o, e) => {
                        var service = new DXSplashScreenService() {
                            ViewTemplate = new DataTemplate() { VisualTree = new FrameworkElementFactory(typeof(SplashScreenTestUserControl)) },
                        };
                        service.ShowSplashScreen();
                        SplashScreenTestUserControl.DoEvents();
                        Assert.IsNotNull(DXSplashScreen.SplashContainer);
                        Assert.IsTrue(DXSplashScreen.IsActive);
                        throw new NotImplementedException();
                    };
                    app.Run();
                } catch(NotImplementedException) { }
                DispatcherHelper.DoEvents();
                Assert.IsNotNull(DXSplashScreen.SplashContainer);
                Assert.IsFalse(DXSplashScreen.IsActive);
                var thread = DXSplashScreen.SplashContainer.OldInfo.InternalThread;
                if(thread != null)
                    thread.Join();
            });
        }
    }
#endif
    static class SplashScreenTestsHelper {
        public static void CloseDXSplashScreen() {
            JoinThread(DXSplashScreen.SplashContainer.With(x => x.OldInfo));
            if(!DXSplashScreen.IsActive)
                return;
            DXSplashScreen.Close();
            JoinThread(DXSplashScreen.SplashContainer.With(x => x.OldInfo));
        }
        public static void CloseSplashScreenService(ISplashScreenService service) {
            var dxservice = service as DXSplashScreenService;
            bool useDefaultClose = dxservice == null || dxservice.GetSplashContainer(false).Return(x => !x.IsActive, () => true);
            if(useDefaultClose) {
                CloseDXSplashScreen();
                return;
            }

            var container = dxservice.GetSplashContainer(false);
            service.HideSplashScreen();
            JoinThread(container.OldInfo);
        }
        internal static void JoinThread(DXSplashScreen.SplashScreenContainer.SplashScreenInfo info) {
            if(info == null)
                return;
            var t = info.InternalThread;
            if(t != null && t.IsAlive)
                t.Join();
        }
        public static ControlTemplate CreateControlTemplate() {
            var border = new FrameworkElementFactory(typeof(Border));
            border.SetValue(Border.WidthProperty, 120d);
            border.SetValue(Border.HeightProperty, 140d);
            return new ControlTemplate(typeof(SplashScreenTestUserControl)) {
                VisualTree = border
            };
        }
        public static DataTemplate CreateDefaultTemplate() {
            return new DataTemplate() { VisualTree = new FrameworkElementFactory(typeof(SplashScreenTestUserControl)) };
        }
        public static DXSplashScreenService CreateDefaultService() {
            return new DXSplashScreenService() { ViewTemplate = CreateDefaultTemplate() };
        }
        public static DXSplashScreenService CreateDefaultSplashScreenAndShow(Window window, FrameworkElement owner = null, SplashScreenOwnerSearchMode ownerSearchMode = SplashScreenOwnerSearchMode.Full
                 , bool activateWindow = true, FrameworkElement windowContent = null, SplashScreenClosingMode? closingMode = null) {
            DXSplashScreenService service = CreateDefaultService();
            service.SplashScreenOwner = owner;
            if(closingMode.HasValue)
                service.SplashScreenClosingMode = closingMode.Value;
            service.OwnerSearchMode = ownerSearchMode;
            if(windowContent == null)
                Interaction.GetBehaviors(window).Add(service);
            else {
                window.Do(x => x.Content = windowContent);
                Interaction.GetBehaviors(windowContent).Add(service);
            }
            if(window != null) {
                window.Show();
                if(activateWindow && !window.IsActive)
                    window.Activate();
                DispatcherHelper.UpdateLayoutAndDoEvents(window);
            }
            service.ShowSplashScreen();
            SplashScreenTestUserControl.DoEvents();
            return service;
        }
    }
}