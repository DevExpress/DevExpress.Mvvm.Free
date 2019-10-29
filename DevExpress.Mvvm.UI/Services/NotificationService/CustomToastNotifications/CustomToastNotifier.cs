using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using DevExpress.Mvvm.Native;

namespace DevExpress.Mvvm.UI.Native {
    public delegate void UnregisterCallback<E>(EventHandler<E> eventHandler) where E : EventArgs;
    public interface IAnotherWeakEventHandler<E> where E : EventArgs {
        EventHandler<E> Handler { get; }
    }
    public class AnotherWeakEventHandler<T, E> : IAnotherWeakEventHandler<E> where T : class where E : EventArgs {
        private delegate void OpenEventHandler(T @this, object sender, E e);
        private WeakReference m_TargetRef;
        private OpenEventHandler m_OpenHandler;
        private EventHandler<E> m_Handler;
        private UnregisterCallback<E> m_Unregister;
        public AnotherWeakEventHandler(EventHandler<E> eventHandler, UnregisterCallback<E> unregister) {
            m_TargetRef = new WeakReference(eventHandler.Target);
            m_OpenHandler = (OpenEventHandler)Delegate.CreateDelegate(typeof(OpenEventHandler), null, eventHandler.Method);
            m_Handler = Invoke;
            m_Unregister = unregister;
        }
        public void Invoke(object sender, E e) {
            T target = (T)m_TargetRef.Target;
            if(target != null)
                m_OpenHandler.Invoke(target, sender, e);
            else if(m_Unregister != null) {
                m_Unregister(m_Handler);
                m_Unregister = null;
            }
        }
        public EventHandler<E> Handler { get { return m_Handler; } }
        public static implicit operator EventHandler<E>(AnotherWeakEventHandler<T, E> weh) { return weh.m_Handler; }
    }
    public static class AnotherEventHandlerUtils {
        public static EventHandler<E> MakeWeak<E>(EventHandler<E> eventHandler, UnregisterCallback<E> unregister) where E : EventArgs {
            if(eventHandler == null)
                throw new ArgumentNullException("eventHandler");
            if(eventHandler.Method.IsStatic || eventHandler.Target == null)
                throw new ArgumentException("Only instance methods are supported.", "eventHandler");
            Type wehType = typeof(AnotherWeakEventHandler<,>).MakeGenericType(eventHandler.Method.DeclaringType, typeof(E));
            ConstructorInfo wehConstructor = wehType.GetConstructor(new Type[] { typeof(EventHandler<E>), typeof(UnregisterCallback<E>) });
            IAnotherWeakEventHandler<E> weh = (IAnotherWeakEventHandler<E>)wehConstructor.Invoke(new object[] { eventHandler, unregister });
            return weh.Handler;
        }
    }

    public interface IScreen {
        Rect GetWorkingArea(System.Windows.Point point);
        event Action WorkingAreaChanged;
    }

    public class PrimaryScreen : IScreen {
        static PrimaryScreen() {
            Microsoft.Win32.SystemEvents.DisplaySettingsChanged += SystemEvents_DisplaySettingsChanged;
        }

        static void SystemEvents_DisplaySettingsChanged(object sender, EventArgs e) {
            if(DisplaySettingsChanged != null)
                DisplaySettingsChanged(sender, e);
        }

        static event EventHandler<EventArgs> DisplaySettingsChanged;

        public PrimaryScreen() {
            DisplaySettingsChanged += AnotherEventHandlerUtils.MakeWeak<EventArgs>((s, e) => {
                if(WorkingAreaChanged != null)
                    WorkingAreaChanged();
            }, handler => DisplaySettingsChanged -= handler);
        }

        public static System.Windows.Point GetDpi() {
            using(var textBox = new System.Windows.Forms.TextBox()) {
                using(var graphics = textBox.CreateGraphics()) {
                    return new System.Windows.Point(graphics.DpiX / 96, graphics.DpiY / 96);
                }
            }
        }

        public Rect GetWorkingArea(System.Windows.Point point) {
            var dpi = GetDpi();
            var area = Screen.GetWorkingArea(new System.Drawing.Point((int)point.X + 1, (int)point.Y + 1));
            return new Rect(area.X / dpi.X, area.Y / dpi.Y, area.Width / dpi.X, area.Height / dpi.Y);
        }

        public event Action WorkingAreaChanged;
    }

    public class CustomNotifier {
        internal
        class ToastInfo {
            public Window win;
            public CustomNotification toast;
            public Timer timer;
            public TaskCompletionSource<NotificationResult> source;
        }

        readonly List<ToastInfo> toastsQueue = new List<ToastInfo>();
        const int maxVisibleToasts = 3;
        internal static NotificationPositioner<ToastInfo> positioner;
        IScreen screen;
        System.Windows.Point currentScreenPosition = new System.Windows.Point();

        public Style Style { get; set; }

        public CustomNotifier(IScreen screen = null) {
            this.screen = screen ?? new PrimaryScreen();
            this.screen.WorkingAreaChanged += screen_WorkingAreaChanged;
            UpdatePositioner(NotificationPosition.TopRight, maxVisibleToasts);
        }

        List<ToastInfo> VisibleItems {
            get { return positioner.Items.Where(i => i != null).ToList(); }
        }

        public void ChangeScreen(System.Windows.Point position) {
            if(VisibleItems.Any() || currentScreenPosition == position)
                return;
            currentScreenPosition = position;
            UpdatePositioner(positioner.position, positioner.maxCount);
        }

        void screen_WorkingAreaChanged() {
            positioner.Update(screen.GetWorkingArea(currentScreenPosition));
            foreach (ToastInfo info in VisibleItems) {
                info.timer.Stop();
                info.timer.Start();
                var newPos = positioner.GetItemPosition(info);
                info.win.Left = newPos.X;
                info.win.Top = newPos.Y;
            }
        }

        public void UpdatePositioner(NotificationPosition position, int maxCount) {
            if(positioner == null) {
                positioner = new NotificationPositioner<ToastInfo>();
            }
            positioner.Update(screen.GetWorkingArea(currentScreenPosition), position, maxCount);
        }

        public Task<NotificationResult> ShowAsync(CustomNotification toast, int msDuration = 3000) {
            var info = new ToastInfo {
                toast = toast,
                timer = new Timer() { Interval = msDuration },
                source = new TaskCompletionSource<NotificationResult>()
            };
            toastsQueue.Add(info);
            ShowNext();
            return info.source.Task;
        }

        void ShowNext() {
            if(!positioner.HasEmptySlot() || !toastsQueue.Any())
                return;

            ToastInfo info = toastsQueue[0];
            toastsQueue.RemoveAt(0);

            var content = new ToastContentControl() { Toast = info.toast };
            content.Content = info.toast.ViewModel;
            content.Style = Style;
            if(ContentTemplateSelector == null) {
                content.ContentTemplate = ContentTemplate ?? NotificationServiceTemplatesHelper.DefaultCustomToastTemplate;
            }
            content.ContentTemplateSelector = ContentTemplateSelector;

            try {
                info.win = new ToastWindow();
            } catch {
                content.TimeOutCommand.Execute(null);
                return;
            }

            info.win.Content = content;
            info.win.DataContext = info.toast.ViewModel;

            if(double.IsNaN(content.Width) || double.IsNaN(content.Height))
                throw new InvalidOperationException("The height or width of a custom notification can not be set to Auto");

            System.Windows.Point position = positioner.Add(info, content.Width, content.Height);

            info.win.Left = position.X;
            info.win.Top = position.Y;

            try {
                info.win.Show();
            } catch(System.ComponentModel.Win32Exception) {
                content.TimeOutCommand.Execute(null);
                return;
            }
            info.timer.Tick += (s, e) => {
                content.TimeOutCommand.Execute(null);
                info.timer.Stop();
            };
            info.timer.Start();
        }

        internal void Activate(CustomNotification toast) {
            RemoveVisibleToast(toast, NotificationResult.Activated);
            ShowNext();
        }

        internal void Dismiss(CustomNotification toast) {
            RemoveVisibleToast(toast, NotificationResult.UserCanceled);
            ShowNext();
        }

        internal void TimeOut(CustomNotification toast) {
            RemoveVisibleToast(toast, NotificationResult.TimedOut);
            ShowNext();
        }

        internal void Hide(CustomNotification toast) {
            ToastInfo info = toastsQueue.FirstOrDefault(i => i.toast == toast);
            if(info != null) {
                toastsQueue.Remove(info);
                info.source.SetResult(NotificationResult.ApplicationHidden);
                return;
            }
            RemoveVisibleToast(toast, NotificationResult.ApplicationHidden);
            ShowNext();
        }

        void RemoveVisibleToast(CustomNotification toast, NotificationResult result) {
            ToastInfo info = GetVisibleToastInfo(toast);
            if(info == null) {
                return;
            }
            info.win.Close();
            info.timer.Stop();
            positioner.Remove(info);
            info.source.SetResult(result);
        }

        private ToastInfo GetVisibleToastInfo(CustomNotification toast) {
            return positioner.Items.FirstOrDefault(t => t != null && t.toast == toast);
        }

        internal void StopTimer(CustomNotification toast) {
            GetVisibleToastInfo(toast).Do(t => t.timer.Stop());
        }

        internal void ResetTimer(CustomNotification toast) {
            GetVisibleToastInfo(toast).Do(t => t.timer.Start());
        }

        public DataTemplate ContentTemplate { get; set; }
        public DataTemplateSelector ContentTemplateSelector { get; set; }
    }
}