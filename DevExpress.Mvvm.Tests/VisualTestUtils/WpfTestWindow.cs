#if DEBUGTEST || MVVM
using NUnit.Framework;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Threading;
using System.Diagnostics;
using System.Text;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;

#if MVVM
namespace DevExpress {
#else
using DevExpress.Xpf.Core;
namespace DevExpress.Xpf.Core.Tests {
#endif
    public class WpfTestWindow {
        public static readonly DependencyProperty MethodsToSkipProperty = DependencyProperty.RegisterAttached("MethodsToSkip", typeof(IEnumerable<string>), typeof(WpfTestWindow), new PropertyMetadata(Enumerable.Empty<string>()));

        public static IEnumerable<string> GetMethodsToSkip(DependencyObject obj) {
            return (IEnumerable<string>)obj.GetValue(MethodsToSkipProperty);
        }
        public static void SetMethodsToSkip(DependencyObject obj, IEnumerable<string> value) {
            obj.SetValue(MethodsToSkipProperty, value);
        }


        TestWindow window;
        Window realWindow;

        static WpfTestWindow() {
            DispatcherHelper.ForceIncreasePriorityContextIdleMessages();
        }
        public WpfTestWindow() {
            MethodsToSkip = Enumerable.Empty<string>();
        }
 
        public virtual int TimeoutInSeconds { get { return 5; } }
        protected virtual TestWindow CreateTestWindow() { return TestWindow.GetContainer(); }
        protected virtual Window CreateRealWindow() { return new Window(); }

        protected virtual void SetUpCore() {
        }

        protected virtual void TearDownCore() {
            if(realWindow != null) {
                realWindow.SourceInitialized -= OnRealWindowSourceInitialized;
                realWindow.Close();
                realWindow.Content = null;
            }
            if (window != null) {
                window.Close();
                window.Content = null;
                window.MaxWidth = double.PositiveInfinity;
                window.MinWidth = 0.0;
                window.MaxHeight = double.PositiveInfinity;
                window.MinHeight = 0.0;
                window.Width = double.NaN;
                window.Height = double.NaN;
            }
            window = null;
            realWindow = null;
            DispatcherHelper.DoEvents();
        }
        IEnumerable<string> MethodsToSkip;
        static Type voidType = typeof(void);

        protected virtual void FixtureSetUpCore() {
            List<string> methodsToSkip = new List<string>();
#if !MVVM
            Type currentType = GetType();
            string themeName = string.IsNullOrEmpty(Theme.CurrentThemeName) ? Theme.DeepBlueName : Theme.CurrentThemeName;
            CheckToSkip(themeName, currentType.GetCustomAttributes(typeof(ExcludeThemedTestAttribute), false).OfType<ExcludeThemedTestAttribute>());
            var testMethods = currentType.GetMethods(BindingFlags.Public | BindingFlags.Instance).Where(method => !method.IsSpecialName && method.ReturnType == voidType && Attribute.IsDefined(method, typeof(TestAttribute)));
            foreach (var methodInfo in testMethods) {
                foreach (ExcludeThemedTestAttribute attr in methodInfo.GetCustomAttributes(typeof(ExcludeThemedTestAttribute), false)) {
                    if (attr.ExcludedThemes.Any(t => string.Equals(t.Name, themeName)))
                        methodsToSkip.Add(methodInfo.Name);
                }
            }
#endif
            MethodsToSkip = methodsToSkip;
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

        [OneTimeSetUp]
        public void FixtureSetUp() {
            FixtureSetUpCore();
        }

        [OneTimeTearDown]
        public void FixtureTearDown() {
            FixtureTearDownCore();
        }

        public TestWindow Window {
            get {
                if(window == null) {
                    window = CreateTestWindow();
                    SetMethodsToSkip(window, MethodsToSkip);
                    SetThemeForWindow(window);
                }
                return window;
            }
        }
        public Window RealWindow {
            get {
                if(realWindow == null) {
                    realWindow = CreateRealWindow();
                    realWindow.SourceInitialized += OnRealWindowSourceInitialized;
                    SetThemeForWindow(realWindow);
                }
                return realWindow;
            }
        }

        void OnRealWindowSourceInitialized(object sender, EventArgs e) {
            CheckToSkip(MethodsToSkip);
        }
        protected virtual void SetThemeForWindow(System.Windows.Controls.ContentControl window) {
#if !MVVM
            if(!String.IsNullOrEmpty(Theme.CurrentThemeName))
                ThemeManager.SetThemeName(window, Theme.CurrentThemeName);
#endif
        }
        public virtual void EnqueueCallback(Action testCallbackDelegate) {
            testCallbackDelegate();
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
            DateTime start = DateTime.Now;
            while(delay > DateTime.Now - start) {
                DispatcherHelper.DoEvents();
            }
        }
        public virtual void EnqueueConditional(Func<bool> conditionalDelegate, string message) {
            Assert.AreEqual(true, conditionalDelegate(), GetTimeOutMessage(message, conditionalDelegate));
        }
        public virtual void EnqueueWait(Func<bool> conditionalDelegate) {
            EnqueWaitForObject(() => conditionalDelegate() ? conditionalDelegate : null, o => { });
        }

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
                    DispatcherHelper.UpdateLayoutAndDoEvents(RealWindow, priority);
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

        string GetActionTimeOutMessage(string message, Delegate testDelegate) {
            return string.IsNullOrEmpty(message) ? string.Format("Action aborted with timeout {0} seconds: {1}", TimeoutInSeconds, testDelegate.Method) : message;
        }
        protected string GetTimeOutMessage(string message, Func<bool> conditionalDelegate) {
            return string.IsNullOrEmpty(message) ? string.Format("The condition failed with timeout {0} seconds: {1}", TimeoutInSeconds, conditionalDelegate.Method) : message;
        }
        public virtual void EnqueueConditional(Func<bool> conditionalDelegate) {
            EnqueueConditional(conditionalDelegate, null);
        }
        public virtual void EnqueueTestComplete() {
        }
        public virtual void EnqueueShowRealWindow() {
            EnqueueLoadedEventAction(RealWindow, () => RealWindow.Show());
        }
        public virtual void EnqueueShowWindow() {
            EnqueueShowWindow(Window);
        }
        public virtual void EnqueueShowWindow(TestWindow window) {
            EnqueueLoadedEventAction(window, () => window.Show());
        }

        public virtual void EnqueueWindowUpdateLayout() {
            EnqueueCallback(delegate {
                DispatcherHelper.UpdateLayoutAndDoEvents(Window);
            });
        }
        public virtual void EnqueueWindowUpdateLayout(DispatcherPriority priority) {
            EnqueueCallback(delegate {
                DispatcherHelper.UpdateLayoutAndDoEvents(Window, priority);
            });
        }
#if !MVVM
        public virtual void EnqueuePumpAll(Func<DevExpress.Data.AsyncServerModeDataController> getAsyncDataController, bool updateLayout = true) {
            EnqueueCallback(delegate {
                try {
                    System.Threading.Thread.Sleep(100);
                    DevExpress.Data.AsyncServerModeDataController asyncDataController = getAsyncDataController();
                    DevExpress.Data.Async.CommandGetAllFilteredAndSortedRows commandGetRows = asyncDataController.Server.GetAllFilteredAndSortedRows();
                    while(!asyncDataController.Server.WaitFor(commandGetRows)) System.Threading.Thread.Sleep(1);
                } catch(ObjectDisposedException) {
                    System.Threading.Thread.Sleep(1000);
                }
            });
            if(updateLayout)
                EnqueueWindowUpdateLayout();
        }
        public virtual void EnqueuePumpAll(DevExpress.Data.AsyncServerModeDataController asyncDataController, bool updateLayout = true) {
            EnqueuePumpAll(() => asyncDataController, updateLayout);
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
            action();
        }
        protected void EnqueueWaitRenderAction() { }
        public static void CheckToSkip(IEnumerable<string> methodsToSkip) {
            if (methodsToSkip.Count() == 0)
                return;
#if !MVVM
            StackTrace stackTrace = new StackTrace();
            string themeName = string.IsNullOrEmpty(Theme.CurrentThemeName) ? Theme.DeepBlueName : Theme.CurrentThemeName;
            foreach(var stackFrame in stackTrace.GetFrames()) {
                MethodBase methodBase = stackFrame.GetMethod();
                if(methodBase.IsPublic && methodsToSkip.Contains(methodBase.Name))
                    Assert.Ignore("Skipped for {0} theme", themeName);
            }
        }
        void CheckToSkip(string themeName, IEnumerable<ExcludeThemedTestAttribute> excludeThemedTestAttributes) {
            foreach(ExcludeThemedTestAttribute attr in excludeThemedTestAttributes) {
                if(attr.ExcludedThemes.Any(t => string.Equals(t.Name, themeName)))
                    Assert.Ignore("Skipped for {0} theme", themeName);
            }
        }
    }

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
    public sealed class ExcludeThemedTestAttribute : Attribute {
        public bool ExcludeTouchThemes { get; set; }
        public bool ExcludeBlackThemes { get; set; }
        public string ExcludeTheme { get; set; }
        public bool SkipIfNotDefaultTheme { get; set; }

        public IEnumerable<Theme> ExcludedThemes {
            get {
                if(SkipIfNotDefaultTheme) {
                    return Theme.Themes.Where(t => t != Theme.DeepBlue);
                }
                return Theme.Themes.Where(t => string.Equals(t.Name, ExcludeTheme) || IsBlackTheme(t) && ExcludeBlackThemes || IsTouchTheme(t) && ExcludeTouchThemes);
            }
        }
        public ExcludeThemedTestAttribute() { }

        bool IsTouchTheme(Theme theme) {
            return theme.Name.Contains("Touch");
        }
        bool IsBlackTheme(Theme theme) {
            return theme == Theme.Office2010Black || theme == Theme.MetropolisDark || theme == Theme.TouchlineDark;
#endif
        }
    }
}
#endif