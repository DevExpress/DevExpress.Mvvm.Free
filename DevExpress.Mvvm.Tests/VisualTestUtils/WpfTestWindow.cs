#if !SILVERLIGHT
using NUnit.Framework;
#else
using Microsoft.Silverlight.Testing;
#endif
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Threading;
using System.Diagnostics;
using System.Text;

namespace DevExpress {
    public class WpfTestWindow {
#if SILVERLIGHT
        WorkItemTest workItemTest;
#endif
        TestWindow window;
        Window realWindow;

        static WpfTestWindow() {
#if !SILVERLIGHT
            DispatcherHelper.ForceIncreasePriorityContextIdleMessages();
#endif
        }
        public WpfTestWindow() { }

        public virtual int TimeoutInSeconds { get { return 5; } }
        protected virtual TestWindow CreateTestWindow() { return TestWindow.GetContainer(); }
        protected virtual Window CreateRealWindow() { return new Window(); }
        protected virtual void SetUpCore() {
#if SILVERLIGHT
            System.Windows.Browser.HtmlPage.Plugin.Focus();
            workItemTest = new WorkItemTest() { UnitTestHarness = DevExpress.TestHelper.TestHarness };
#endif
        }
        protected virtual void TearDownCore() {
            if(realWindow != null) {
                realWindow.Close();
                realWindow.Content = null;
            }
            if (window != null) {
                window.Close();
                window.Content = null;
            }
            window = null;
            realWindow = null;
            DispatcherHelper.DoEvents();
        }
        protected virtual void FixtureSetUpCore() {
        }
        protected virtual void FixtureTearDownCore() {
        }

        [SetUp]
        public void SetUp() {
            SetUpCore();
        }

        [TearDown]
        public void TearDown() {
            TearDownCore();
        }

        [TestFixtureSetUp]
        public void FixtureSetUp() {
            FixtureSetUpCore();
        }

        [TestFixtureTearDown]
        public void FixtureTearDown() {
#if SILVERLIGHT
            System.Windows.Media.CompositionTarget.Rendering -= CompositionTarget_Rendering;
#endif
            FixtureTearDownCore();
        }

        public TestWindow Window {
            get {
                if(window == null) {
                    window = CreateTestWindow();
                    SetThemeForWindow(window);
                }
                return window;
            }
        }
        public Window RealWindow {
            get {
                if(realWindow == null) {
                    realWindow = CreateRealWindow();
                    SetThemeForWindow(realWindow);
                }
                return realWindow;
            }
        }
        protected virtual void SetThemeForWindow(System.Windows.Controls.ContentControl window) {
        }
        public virtual void EnqueueCallback(Action testCallbackDelegate) {
#if SILVERLIGHT
            workItemTest.EnqueueCallback(testCallbackDelegate);
#else
            testCallbackDelegate();
#endif
        }
        protected sealed class TimeoutGuard : IDisposable {
            DispatcherTimer timer = new DispatcherTimer();

            public TimeoutGuard(int timeoutInSeconds, string message) {
                timer.Interval = new TimeSpan(0, 0, timeoutInSeconds);
                timer.Tick += (s, e) => {
                    timer.Stop();
                    throw new Exception(message);
                };
                timer.Start();
            }
            public void Dispose() {
                if(timer.IsEnabled)
                    timer.Stop();
            }
        }
#if !SILVERLIGHT
        public virtual void EnqueueDialog(Action testCallbackDelegate, string message = null) {
            using(new TimeoutGuard(TimeoutInSeconds, GetActionTimeOutMessage(message, testCallbackDelegate))) {
                testCallbackDelegate();
            }
        }
        public virtual void EnqueueDialog<T>(Func<T> testCallbackDelegate, Action<T> result, string message = null) {
            T resultValue = default(T);
            using(new TimeoutGuard(TimeoutInSeconds, GetActionTimeOutMessage(message, testCallbackDelegate))) {
                resultValue = testCallbackDelegate();
            }
            result(resultValue);
        }
#else
        public virtual void EnqueueDialog(Func<Task> testCallbackDelegate, string message = null) {
            bool done = false;
            EnqueueCallback(() => testCallbackDelegate().ContinueWith(t => done = true));
            EnqueueConditional(() => done, GetActionTimeOutMessage(message, testCallbackDelegate));
        }
        public virtual void EnqueueDialog<T>(Func<Task<T>> testCallbackDelegate, Action<T> result, string message = null) {
            T resultValue = default(T);
            bool done = false;
            EnqueueCallback(() => testCallbackDelegate().ContinueWith(t => {
                resultValue = t.Result;
                done = true;
            }));
            EnqueueConditional(() => done, GetActionTimeOutMessage(message, testCallbackDelegate));
            EnqueueCallback(() => result(resultValue));
        }
#endif
        public void EnqueueTestWindowMainCallback(Action testCallbackDelegate) {
            EnqueueShowWindow();
            EnqueueLastCallback(testCallbackDelegate);
        }
        public void EnqueueRealWindowMainCallback(Action testCallbackDelegate) {
            EnqueueShowRealWindow();
            EnqueueLastCallback(testCallbackDelegate);
        }
        public virtual void EnqueueLastCallback(Action testCallbackDelegate) {
            EnqueueCallback(testCallbackDelegate);
            EnqueueTestComplete();
        }
        public void EnqueueDelay(int delayInMillisceconds) {
            EnqueueDelay(TimeSpan.FromMilliseconds(delayInMillisceconds));
        }
        public void EnqueueDelay(TimeSpan delay) {
#if SILVERLIGHT
            DateTime? start = null;
            workItemTest.EnqueueConditional(() => {
                if(start == null) {
                    start = DateTime.Now;
                }
                return delay < DateTime.Now - start;
            });
#else
            DateTime start = DateTime.Now;
            while(delay > DateTime.Now - start) {
                DispatcherHelper.DoEvents();
            }

#endif
        }
        public virtual void EnqueueConditional(Func<bool> conditionalDelegate, string message) {
#if SILVERLIGHT
            DateTime? start = null;
            workItemTest.EnqueueConditional(() => {
                if(start == null) {
                    start = DateTime.Now;
                }
                if (TimeSpan.FromSeconds(TimeoutInSeconds) < DateTime.Now - start) {
                    throw new Exception(GetTimeOutMessage(message, conditionalDelegate));
                }
                return conditionalDelegate();
            });
#else
            Assert.AreEqual(true, conditionalDelegate(), GetTimeOutMessage(message, conditionalDelegate));
#endif
        }
        public virtual void EnqueueWait(Func<bool> conditionalDelegate) {
            EnqueWaitForObject(() => conditionalDelegate() ? conditionalDelegate : null, o => { });
        }
#if SILVERLIGHT
        public virtual void EnqueWaitForAsync(Task task) {
            if(task == null) return;
            EnqueWaitForObject(() => task.Wait(1) ? task : null, o => { });
        }
        public virtual void EnqueWaitForObject(Func<object> getObject, Action<object> setObject) {
            EnqueueConditional(() => {
                object obj = getObject();
                if(obj != null) {
                    setObject(obj);
                    return true;
                }
                return false;
            });
        }
        public virtual void EnqueDelayOrWaitForObject(Func<object> getObject, Action<object> setObject, int delayInMillisceconds = 5000) {
            DateTime? start= null;
            workItemTest.EnqueueConditional(() => {
                if(start == null) start = DateTime.Now;
                object obj = getObject();
                if(obj != null) {
                    setObject(obj);
                    return true;
                }
                return TimeSpan.FromMilliseconds(delayInMillisceconds) < DateTime.Now - start;
            });
        }
#else
        public virtual void EnqueueWaitRealWindow(Func<bool> conditionalDelegate) {
            EnqueWaitForObjectRealWindow(() => conditionalDelegate() ? conditionalDelegate : null, o => { });
        }
        public virtual void EnqueWaitForAsync(Thread t) {
            EnqueWaitForAsync(t, DispatcherPriority.Background);
        }
        public virtual void EnqueWaitForAsync(Thread t, DispatcherPriority priority) {
            if(t == null) return;
            EnqueWaitForObject(() => t.Join(1) ? t : null, o => { }, priority);
        }
        public virtual void EnqueWaitForAsync(ManualResetEvent e) {
            EnqueWaitForAsync(e, DispatcherPriority.Background);
        }
        public virtual void EnqueWaitForAsync(ManualResetEvent e, DispatcherPriority priority) {
            if(e == null) return;
            EnqueWaitForObject(() => e.WaitOne(1) ? e : null, o => { }, priority);
        }
        public virtual void EnqueWaitForAsync(Task task) {
            EnqueWaitForAsync(task, DispatcherPriority.Background);
        }
        public virtual void EnqueWaitForAsync(Task task, DispatcherPriority priority) {
            if(task == null) return;
            EnqueWaitForObject(() => task.Wait(1) ? task : null, o => { }, priority);
        }
        public virtual void EnqueWaitForObject(Func<object> getObject, Action<object> setObject) {
            EnqueWaitForObject(getObject, setObject, DispatcherPriority.Background);
        }
        public virtual void EnqueWaitForObjectRealWindow(Func<object> getObject, Action<object> setObject) {
            EnqueWaitForObjectRealWindow(getObject, setObject, DispatcherPriority.Background);
        }
        public virtual void EnqueWaitForObjectRealWindow(Func<object> getObject, Action<object> setObject, DispatcherPriority priority) {
            EnqueueCallback(() => {
                DateTime start = DateTime.Now;
                while(TimeSpan.FromSeconds(TimeoutInSeconds) > DateTime.Now - start) {
                    object obj = getObject();
                    if(obj != null) {
                        setObject(obj);
                        return;
                    }
                    DispatcherHelper.DoEvents(priority);
                }
                throw new Exception(GetTimeOutMessage(null, () => false));
            });
        }
        public virtual void EnqueWaitForObject(Func<object> getObject, Action<object> setObject, DispatcherPriority priority) {
            EnqueueCallback(() => {
                DateTime start = DateTime.Now;
                while(TimeSpan.FromSeconds(TimeoutInSeconds) > DateTime.Now - start) {
                    object obj = getObject();
                    if(obj != null) {
                        setObject(obj);
                        return;
                    }
                    DispatcherHelper.UpdateLayoutAndDoEvents(Window, priority);
                }
                throw new Exception(GetTimeOutMessage(null, () => false));
            });
        }
        public virtual void EnqueDelayOrWaitForObject(Func<object> getObject, Action<object> setObject, int delayInMillisceconds) {
            EnqueueCallback(() => {
                DateTime start = DateTime.Now;
                while(TimeSpan.FromMilliseconds(delayInMillisceconds) > DateTime.Now - start) {
                    object obj = getObject();
                    if(obj != null) {
                        setObject(obj);
                        return;
                    }
                    DispatcherHelper.UpdateLayoutAndDoEvents(Window);
                }
            });
        }
#endif
        string GetActionTimeOutMessage(string message, Delegate testDelegate) {
            return string.IsNullOrEmpty(message) ? string.Format("Action aborted with timeout {0} seconds: {1}", TimeoutInSeconds, testDelegate.Method) : message;
        }
        string GetTimeOutMessage(string message, Func<bool> conditionalDelegate) {
            return string.IsNullOrEmpty(message) ? string.Format("The condition failed with timeout {0} seconds: {1}", TimeoutInSeconds, conditionalDelegate.Method) : message;
        }
        public virtual void EnqueueConditional(Func<bool> conditionalDelegate) {
            EnqueueConditional(conditionalDelegate, null);
        }
        public virtual void EnqueueTestComplete() {
#if SILVERLIGHT
            workItemTest.EnqueueTestComplete();
#endif
        }
        public virtual void EnqueueShowRealWindow() {
            EnqueueLoadedEventAction(RealWindow, () => RealWindow.Show());
#if SILVERLIGHT
            EnqueueDelay(100);
            EnqueueWindowUpdateLayout();
#endif
        }
        public virtual void EnqueueShowWindow() {
            EnqueueShowWindow(Window);
#if SILVERLIGHT
            EnqueueDelay(100);
            EnqueueWindowUpdateLayout();
#endif
        }
        public virtual void EnqueueShowWindow(TestWindow window) {
            EnqueueLoadedEventAction(window, () => window.Show());
        }

        public virtual void EnqueueWindowUpdateLayout() {
            EnqueueCallback(delegate {
                DispatcherHelper.UpdateLayoutAndDoEvents(Window);
            });
        }
#if !SILVERLIGHT
        public virtual void EnqueueWindowUpdateLayout(DispatcherPriority priority) {
            EnqueueCallback(delegate {
                DispatcherHelper.UpdateLayoutAndDoEvents(Window, priority);
            });
        }
        #endif
        public void EnqueueLoadedEventAction(FrameworkElement element, Action action) {
            EnqueueLoadedEventAction(() => element, action);
        }
        public void EnqueueLoadedEventAction(Func<FrameworkElement> getElement, Action action) {
            EnqueueWaitEventEventAction(getElement, action, (getElementDelegate, handler) => getElementDelegate().Loaded += handler);
        }
        public void EnqueueClickButton(Func<ButtonBase> getButtonDelegate) {
            EnqueueWaitEventEventAction(() => getButtonDelegate(), () => UITestHelper.ClickButton(getButtonDelegate()), (getElementDelegate_, handler) => ((ButtonBase)getElementDelegate_()).Click += handler);
        }
        public void EnqueueClickButton(ButtonBase button) {
            EnqueueClickButton(() => button);
        }
        protected delegate void SubscribeDelegate(Func<FrameworkElement> getElementDelegate, RoutedEventHandler handler);
        protected void EnqueueWaitEventEventAction(Func<FrameworkElement> getElementDelegate, Action action, SubscribeDelegate subscribeDelegate) {
#if SILVERLIGHT
            bool eventFired = false;
            EnqueueCallback(() => subscribeDelegate(getElementDelegate, delegate { eventFired = true; }));
            EnqueueCallback(action);
            EnqueueConditional(() => eventFired);
#else
            action();
#endif
        }
#if SILVERLIGHT
        bool rendered;
        protected void EnqueueWaitRenderAction() {
            EnqueueCallback(() => {
                System.Windows.Media.CompositionTarget.Rendering -= CompositionTarget_Rendering;
                System.Windows.Media.CompositionTarget.Rendering += CompositionTarget_Rendering;
                rendered = false;
            });
            EnqueueConditional(() => {
                if(rendered) {
                    rendered = false;
                    return true;
                }
                return false;
            });
        }

        void CompositionTarget_Rendering(object sender, EventArgs e) {
            System.Windows.Media.CompositionTarget.Rendering -= CompositionTarget_Rendering;
            rendered = true;
        }
#else
        protected void EnqueueWaitRenderAction() { }
#endif
    }
}