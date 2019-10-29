using System.Windows.Controls;
using DevExpress.Internal;
using DevExpress.Mvvm.UI.Native;
using NUnit.Framework;
using System;
using System.IO.Packaging;
using System.Linq;
using System.Security.Permissions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using System.Collections.Generic;
namespace DevExpress.Mvvm.UI.Tests {
    [TestFixture]
    public class NotificationTests {
        [TearDown]
        public void TearDown() {
            if(CustomNotifier.positioner != null) {
                Assert.AreEqual(0, CustomNotifier.positioner.Items.Count(i => i != null));
            }
        }

        [Test]
        public void PositionerTest() {
            var pos = new NotificationPositioner<string>();
            pos.Update(new Rect(0, 0, 800, 600), NotificationPosition.TopRight, 3);
            Point p;

            Assert.AreEqual(true, pos.HasEmptySlot());

            p = pos.Add("item1", 200, 50);
            Assert.AreEqual(800 - 200, p.X);
            Assert.AreEqual(20, p.Y);

            p = pos.Add("item2", 200, 50);
            Assert.AreEqual(800 - 200, p.X);
            Assert.AreEqual(20 + 50 + 10, p.Y);

            p = pos.Add("item3", 200, 50);
            Assert.AreEqual(800 - 200, p.X);
            Assert.AreEqual(20 + 50 + 10 + 50 + 10, p.Y);

            Assert.AreEqual(false, pos.HasEmptySlot());

            pos.Remove("item2");
            p = pos.Add("item4", 200, 50);
            Assert.AreEqual(800 - 200, p.X);
            Assert.AreEqual(20 + 50 + 10, p.Y);

            pos.Remove("item3");
            p = pos.Add("item5", 200, 50);
            Assert.AreEqual(800 - 200, p.X);
            Assert.AreEqual(20 + 50 + 10 + 50 + 10, p.Y);

            Assert.AreEqual(false, pos.HasEmptySlot());
            pos.Remove("item1");
            Assert.AreEqual(true, pos.HasEmptySlot());

            pos = new NotificationPositioner<string>();
            pos.Update(new Rect(0, 0, 800, 600), NotificationPosition.BottomRight, 3);
            p = pos.Add("item1", 200, 50);
            Assert.AreEqual(800 - 200, p.X);
            Assert.AreEqual(600 - 50 - 20, p.Y);

            p = pos.Add("item2", 200, 50);
            Assert.AreEqual(800 - 200, p.X);
            Assert.AreEqual(600 - 50 - 20 - 50 - 10, p.Y);

            p = pos.Add("item3", 200, 50);
            Assert.AreEqual(800 - 200, p.X);
            Assert.AreEqual(600 - 50 - 20 - 50 - 10 - 50 - 10, p.Y);

            Assert.AreEqual(false, pos.HasEmptySlot());

            pos.Remove("item2");
            p = pos.Add("item4", 200, 50);
            Assert.AreEqual(800 - 200, p.X);
            Assert.AreEqual(600 - 50 - 20 - 50 - 10, p.Y);

            pos.Remove("item3");
            p = pos.Add("item5", 200, 50);
            Assert.AreEqual(800 - 200, p.X);
            Assert.AreEqual(600 - 50 - 20 - 50 - 10 - 50 - 10, p.Y);

            Assert.AreEqual(false, pos.HasEmptySlot());
            pos.Remove("item1");
            Assert.AreEqual(true, pos.HasEmptySlot());
        }

        [Test]
        public void PositionerTest2() {
            var pos = new NotificationPositioner<string>();
            pos.Update(new Rect(0, 0, 800, 600), NotificationPosition.TopRight, 100);
            pos.Add("item1", 200, 480);
            Assert.AreEqual(false, pos.HasEmptySlot());

            pos = new NotificationPositioner<string>();
            pos.Update(new Rect(0, 0, 800, 600), NotificationPosition.TopRight, 100);
            pos.Add("item1", 200, 460);
            Assert.AreEqual(false, pos.HasEmptySlot());

            pos = new NotificationPositioner<string>();
            pos.Update(new Rect(0, 0, 800, 250), NotificationPosition.TopRight, 100);
            pos.Add("item1", 200, 100);
            pos.Add("item1", 200, 100);
            Assert.AreEqual(false, pos.HasEmptySlot());

            pos = new NotificationPositioner<string>();
            pos.Update(new Rect(0, 0, 800, 360), NotificationPosition.TopRight, 100);
            pos.Add("item1", 200, 100);
            pos.Add("item1", 200, 100);
            Assert.AreEqual(true, pos.HasEmptySlot());
        }

        [Test]
        public void PositionerTest3() {
            var pos = new NotificationPositioner<string>();
            pos.Update(new Rect(10, 25, 790, 575), NotificationPosition.TopRight, 100);
            Point p = pos.Add("item1", 200, 100);
            Assert.AreEqual(10 + 790 - 200, p.X);
            Assert.AreEqual(25 + 20, p.Y);

            pos = new NotificationPositioner<string>();
            pos.Update(new Rect(10, 25, 790, 575), NotificationPosition.BottomRight, 100);
            p = pos.Add("item1", 200, 100);
            Assert.AreEqual(10 + 790 - 200, p.X);
            Assert.AreEqual(600 - 20 - 100, p.Y);
        }

        [Test]
        public void PositionerTest4() {
            var pos = new NotificationPositioner<string>();
            pos.Update(new Rect(0, 50, 800, 650), NotificationPosition.TopRight, 100);
            pos.Add("item1", 200, 480);
            Assert.AreEqual(false, pos.HasEmptySlot());

            pos = new NotificationPositioner<string>();
            pos.Update(new Rect(0, 50, 800, 650), NotificationPosition.TopRight, 100);
            pos.Add("item1", 200, 460);
            Assert.AreEqual(false, pos.HasEmptySlot());

            pos = new NotificationPositioner<string>();
            pos.Update(new Rect(0, 50, 800, 300), NotificationPosition.TopRight, 100);
            pos.Add("item1", 200, 100);
            pos.Add("item1", 200, 100);
            Assert.AreEqual(false, pos.HasEmptySlot());

            pos = new NotificationPositioner<string>();
            pos.Update(new Rect(0, 50, 800, 410), NotificationPosition.TopRight, 100);
            pos.Add("item1", 200, 100);
            pos.Add("item1", 200, 100);
            Assert.AreEqual(true, pos.HasEmptySlot());
        }

        [Test]
        public void PositionerHasSlotTest() {
            var pos = new NotificationPositioner<string>();
            pos.Update(new Rect(0, 50, 800, 650), NotificationPosition.TopRight, 3);
            pos.Add("item1", 200, 100);
            Assert.AreEqual(true, pos.HasEmptySlot());
            pos.Remove("item1");
            Assert.AreEqual(true, pos.HasEmptySlot());
            pos.Add("item2", 200, 100);
            Assert.AreEqual(true, pos.HasEmptySlot());
            pos.Add("item3", 200, 100);
            Assert.AreEqual(true, pos.HasEmptySlot());
            pos.Add("item4", 200, 100);
            Assert.AreEqual(false, pos.HasEmptySlot());
        }

        static void WaitWithDispatcher(Task<NotificationResult> task) {
            while(task.Status != TaskStatus.RanToCompletion && task.Status != TaskStatus.Faulted) {
                DispatcherUtil.DoEvents();
            }
        }

        public static class DispatcherUtil {
            [SecurityPermissionAttribute(SecurityAction.Demand, Flags = SecurityPermissionFlag.UnmanagedCode)]
            public static void DoEvents() {
                DispatcherFrame frame = new DispatcherFrame();
                Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.Background,
                    new DispatcherOperationCallback(ExitFrame), frame);
                Dispatcher.PushFrame(frame);
                System.Windows.Forms.Application.DoEvents();
            }

            private static object ExitFrame(object frame) {
                ((DispatcherFrame)frame).Continue = false;
                return null;
            }
        }

        [Test]
        public void ResultsTest() {
            CustomNotification toast;
            Task<NotificationResult> task;
            var notifier = new CustomNotifier();

            toast = new CustomNotification(null, notifier);
            task = notifier.ShowAsync(toast, 1);
            WaitWithDispatcher(task);
            Assert.AreEqual(NotificationResult.TimedOut, task.Result);

            toast = new CustomNotification(null, notifier);
            task = notifier.ShowAsync(toast, 1);
            toast.Activate();
            WaitWithDispatcher(task);
            Assert.AreEqual(NotificationResult.Activated, task.Result);

            toast = new CustomNotification(null, notifier);
            task = notifier.ShowAsync(toast, 1);
            toast.Dismiss();
            WaitWithDispatcher(task);
            Assert.AreEqual(NotificationResult.UserCanceled, task.Result);

            toast = new CustomNotification(null, notifier);
            task = notifier.ShowAsync(toast, 1);
            toast.Hide();
            WaitWithDispatcher(task);
            Assert.AreEqual(NotificationResult.ApplicationHidden, task.Result);

            var tasks = Enumerable.Range(0, 10).Select(_ => notifier.ShowAsync(new CustomNotification(null, notifier), 1)).ToList();
            tasks.ToList().ForEach(WaitWithDispatcher);
            tasks.ToList().ForEach(t => Assert.AreEqual(NotificationResult.TimedOut, t.Result));
        }

        [Test]
        public void UpdatingPositionerTest() {
            CustomNotification toast;
            Task<NotificationResult> task;
            var notifier = new CustomNotifier();
            var tasks = Enumerable.Range(0, 5).Select(_ => notifier.ShowAsync(new CustomNotification(null, notifier), 1)).ToList();
            tasks.ToList().ForEach(WaitWithDispatcher);
            tasks.ToList().ForEach(t => Assert.AreEqual(NotificationResult.TimedOut, t.Result));

            toast = new CustomNotification(null, notifier);
            task = notifier.ShowAsync(toast, 1);
            notifier.UpdatePositioner(NotificationPosition.TopRight, 2);
            WaitWithDispatcher(task);
            Assert.AreEqual(NotificationResult.TimedOut, task.Result);
            tasks = Enumerable.Range(0, 10).Select(_ => notifier.ShowAsync(new CustomNotification(null, notifier), 1)).ToList();
            notifier.UpdatePositioner(NotificationPosition.TopRight, 2);
            tasks.ToList().ForEach(WaitWithDispatcher);
            tasks.ToList().ForEach(t => Assert.AreEqual(NotificationResult.TimedOut, t.Result));
        }

        [Test]
        public void ShowingTooManyToastsTest() {
            var notifier = new CustomNotifier();
            notifier.UpdatePositioner(NotificationPosition.TopRight, 100);

            var tasks = Enumerable.Range(0, 100).Select(_ => notifier.ShowAsync(new CustomNotification(null, notifier), 1)).ToList();
            tasks.ToList().ForEach(WaitWithDispatcher);
            tasks.ToList().ForEach(t => Assert.AreEqual(NotificationResult.TimedOut, t.Result));
        }

        [Test]
        public void SettingApplicationIDToNullTest() {
            var service = new NotificationService();
            service.ApplicationId = "";
            service.ApplicationId = null;
            Assert.IsTrue(true);
        }

        [Test]
        public void RemovePostponedNotificationTest() {
            var notifier = new CustomNotifier();
            notifier.UpdatePositioner(NotificationPosition.TopRight, 1);

            var toasts = Enumerable.Range(0, 3).Select(_ => new CustomNotification(null, notifier)).ToList();
            var tasks = toasts.Select(toast => notifier.ShowAsync(toast, 1)).ToList();
            toasts[2].Hide();
            tasks.ToList().ForEach(WaitWithDispatcher);
            Assert.AreEqual(NotificationResult.TimedOut, tasks[0].Result);
            Assert.AreEqual(NotificationResult.TimedOut, tasks[1].Result);
            Assert.AreEqual(NotificationResult.ApplicationHidden, tasks[2].Result);
        }

        class TestScreen : IScreen {
            public Rect bounds;
            public Rect GetWorkingArea(Point point) {
                return new Rect(
                    bounds.X - point.X,
                    bounds.Y - point.Y,
                    bounds.Width, bounds.Height);
            }
            public void Changed() {
                if(WorkingAreaChanged != null)
                    WorkingAreaChanged();
            }
            public event Action WorkingAreaChanged;
        }

        [Test]
        public void BasicResolutionChangedHandlingTest() {
            var testScreen = new TestScreen { bounds = new Rect(0, 0, 1000, 500) };
            var notifier = new CustomNotifier(testScreen);
            notifier.UpdatePositioner(NotificationPosition.BottomRight, 2);
            var toasts = Enumerable.Range(0, 3).Select(_ => new CustomNotification(null, notifier)).ToList();
            var tasks = toasts.Select(toast => notifier.ShowAsync(toast, 1)).ToList();

            var pos = CustomNotifier.positioner;
            var ps = pos.Items.Select(i => pos.GetItemPosition(i)).ToList();
            Assert.AreEqual(new Point(615, 390), ps[0]);
            Assert.AreEqual(new Point(615, 290), ps[1]);

            testScreen.bounds = new Rect(0, 0, 800, 600);
            testScreen.Changed();
            pos = CustomNotifier.positioner;
            ps = pos.Items.Select(i => pos.GetItemPosition(i)).ToList();
            Assert.AreEqual(new Point(415, 490), ps[0]);
            Assert.AreEqual(new Point(415, 390), ps[1]);

            tasks.ToList().ForEach(WaitWithDispatcher);
            tasks.ToList().ForEach(t => Assert.AreEqual(NotificationResult.TimedOut, t.Result));
        }

        [Test]
        public void NotificationsArentLostOnPositionerUpdateTest() {
            var testScreen = new TestScreen { bounds = new Rect(0, 0, 1000, 500) };
            var notifier1 = new CustomNotifier(testScreen);
            var notifier2 = new CustomNotifier(testScreen);
            var positioner = CustomNotifier.positioner;
            notifier1.UpdatePositioner(NotificationPosition.BottomRight, 5);
            var toasts1 = Enumerable.Range(0, 1).Select(_ => new CustomNotification(null, notifier1)).ToList();
            var toasts2 = Enumerable.Range(0, 1).Select(_ => new CustomNotification(null, notifier2)).ToList();
            var tasks1 = toasts1.Select(toast => notifier1.ShowAsync(toast, 1)).ToList();
            var tasks2 = toasts2.Select(toast => notifier2.ShowAsync(toast, 1)).ToList();
            tasks1.Union(tasks2).ToList().ForEach(WaitWithDispatcher);
            tasks1.Union(tasks2).ToList().ForEach(t => Assert.AreEqual(NotificationResult.TimedOut, t.Result));
        }

        [Test]
        public void UpdatePositionerTest() {
            var testScreen = new TestScreen { bounds = new Rect(0, 0, 800, 600) };
            var notifier = new CustomNotifier(testScreen);
            var pos = CustomNotifier.positioner;

            var toast = new CustomNotification(null, notifier);
            var task = notifier.ShowAsync(toast, 1);
            var p1 = CustomNotifier.positioner.GetItemPosition(CustomNotifier.positioner.Items[0]);
            Assert.AreEqual(415, p1.X);
            Assert.AreEqual(20, p1.Y);

            notifier.UpdatePositioner(NotificationPosition.BottomRight, 3);
            p1 = CustomNotifier.positioner.GetItemPosition(CustomNotifier.positioner.Items[0]);
            Assert.AreEqual(415, p1.X);
            Assert.AreEqual(490, p1.Y);

            WaitWithDispatcher(task);
        }

        [Test, Category("TODO")]
        public void ResolutionChangingTest() {
            var testScreen = new TestScreen { bounds = new Rect(0, 0, 1000, 500) };
            var notifier = new CustomNotifier(testScreen);
            notifier.UpdatePositioner(NotificationPosition.BottomRight, 2);
            var toasts = Enumerable.Range(0, 3).Select(_ => new CustomNotification(null, notifier)).ToList();
            var tasks = toasts.Select(toast => notifier.ShowAsync(toast, 1)).ToList();

            Assert.AreEqual(2, CustomNotifier.positioner.Items.Count(i => i != null));

            testScreen.bounds = new Rect(0, 0, 800, 600);
            testScreen.Changed();

            Assert.AreEqual(2, CustomNotifier.positioner.Items.Count(i => i != null));

            tasks.ToList().ForEach(WaitWithDispatcher);
        }

        [Test]
        public void PositionUpdateTest() {
            var pos = new NotificationPositioner<string>();
            pos.Update(new Rect(0, 0, 800, 600), NotificationPosition.TopRight, 3);
            Point p;

            p = pos.Add("item1", 200, 50);
            Assert.AreEqual(800 - 200, p.X);
            Assert.AreEqual(20, p.Y);

            pos.Update(new Rect(0, 0, 800, 600), NotificationPosition.BottomRight, 3);

            p = pos.GetItemPosition("item1");
            Assert.AreEqual(800 - 200, p.X);
            Assert.AreEqual(600 - 20 - 50, p.Y);
        }

        [Test]
        public void GetBackgroundTest() {
            string _ = PackUriHelper.UriSchemePack;
            Action<byte, byte, byte, string> assertMatch = (r, g, b, icon) => {
                string path = string.Format("pack://application:,,,/{0};component/Icons/{1}", "DevExpress.Mvvm.Tests.Free", icon);
                Uri uri = new Uri(path, UriKind.Absolute);
                var bmp = new System.Drawing.Bitmap(Application.GetResourceStream(uri).Stream);
                Assert.AreEqual(System.Windows.Media.Color.FromRgb(r, g, b), BackgroundCalculator.GetBestMatch(bmp));
            };
            assertMatch(9, 74, 178, "icon1.ico");
            assertMatch(210, 71, 38, "icon2.ico");
            assertMatch(81, 51, 171, "icon3.ico");
            assertMatch(210, 71, 38, "icon4.ico");
            assertMatch(81, 51, 171, "icon5.ico");
            assertMatch(0, 130, 153, "icon6.ico");
            assertMatch(9, 74, 178, "icon7.ico");
        }

        [Test]
        public void ThrowOnApplicationIDNullTest() {
            var service = new NotificationService();
            if(!service.AreWin8NotificationsAvailable)
                return;
            try {
                service.CreatePredefinedNotification("", "", "");
                Assert.Fail();
            } catch(ArgumentNullException e) {
                Assert.AreEqual("ApplicationId", e.ParamName);
            }
        }

        [Test]
        public void UpdateCustomToastPositionTest() {
            var service = new NotificationService();
            var toast = service.CreateCustomNotification(null);
            Assert.AreEqual(NotificationPosition.TopRight, CustomNotifier.positioner.position);
            service.CustomNotificationPosition = NotificationPosition.BottomRight;
            Assert.AreEqual(NotificationPosition.BottomRight, CustomNotifier.positioner.position);

            service = new NotificationService();
            service.CustomNotificationPosition = NotificationPosition.BottomRight;
            toast = service.CreateCustomNotification(null);
            Assert.AreEqual(NotificationPosition.BottomRight, CustomNotifier.positioner.position);
            service.CustomNotificationPosition = NotificationPosition.TopRight;
            Assert.AreEqual(NotificationPosition.TopRight, CustomNotifier.positioner.position);
        }

        [Test]
        public void ResettingTimerOfHiddenNotificationTest() {
            var customNotifier = new CustomNotifier(new TestScreen());
            var toast = new CustomNotification(null, customNotifier);
            customNotifier.ShowAsync(toast);
            customNotifier.Hide(toast);
            customNotifier.ResetTimer(toast);
            customNotifier.StopTimer(toast);
        }

        class TestTemplateSelector : DataTemplateSelector {
            public bool Called = false;
            public override DataTemplate SelectTemplate(object item, DependencyObject container) {
                Called = true;
                return base.SelectTemplate(item, container);
            }
        }

        [Test]
        public void CustomNotificationTemplateSelectorTest() {
            var customNotifier = new CustomNotifier(new TestScreen { bounds = new Rect(0, 0, 1000, 500) });
            var selector = new TestTemplateSelector();
            customNotifier.ContentTemplateSelector = selector;
            var toast = new CustomNotification(null, customNotifier);
            WaitWithDispatcher(customNotifier.ShowAsync(toast, 1));
            Assert.IsTrue(selector.Called);
        }

        [Test]
        public void ThrowOnAutoHeight() {
            var style = new Style();
            style.Setters.Add(new Setter(FrameworkElement.HeightProperty, double.NaN));

            var service = new NotificationService();
            service.CustomNotificationStyle = style;
            var toast = service.CreateCustomNotification(null);
            try {
                toast.ShowAsync();
                Assert.Fail();
            } catch(InvalidOperationException) { }
        }

  [Test]
        public void ConvertTimeSpanToMilliseconds() {
            var service = new NotificationService();
            service.CustomNotificationDuration = TimeSpan.MaxValue;
            var toast = (NotificationService.MvvmCustomNotification)service.CreateCustomNotification(null);
            Assert.AreEqual(int.MaxValue, toast.duration);

            service.CustomNotificationDuration = TimeSpan.MinValue;
            toast = (NotificationService.MvvmCustomNotification)service.CreateCustomNotification(null);
            Assert.AreEqual(0, toast.duration);

            service.CustomNotificationDuration = TimeSpan.FromMilliseconds((double)int.MaxValue + int.MaxValue);
            toast = (NotificationService.MvvmCustomNotification)service.CreateCustomNotification(null);
            Assert.AreEqual(int.MaxValue, toast.duration);

            service.CustomNotificationDuration = TimeSpan.FromMilliseconds(int.MaxValue);
            toast = (NotificationService.MvvmCustomNotification)service.CreateCustomNotification(null);
            Assert.AreEqual(int.MaxValue, toast.duration);

            service.CustomNotificationDuration = TimeSpan.FromMilliseconds(150);
            toast = (NotificationService.MvvmCustomNotification)service.CreateCustomNotification(null);
            Assert.AreEqual(150, toast.duration);

            service.CustomNotificationDuration = TimeSpan.FromMilliseconds(1);
            toast = (NotificationService.MvvmCustomNotification)service.CreateCustomNotification(null);
            Assert.AreEqual(1, toast.duration);
        }

    }
}