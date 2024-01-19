using DevExpress.Internal;
using System.Threading.Tasks;

namespace DevExpress.Mvvm.UI.Native {
    class WpfPredefinedToastNotification : IPredefinedToastNotification {
        WpfPredefinedToastNotificationContent content;
        CustomNotifier notifier;
        CustomNotification toast;

        public WpfPredefinedToastNotification(WpfPredefinedToastNotificationContent content, CustomNotifier notifier) {
            this.toast = new CustomNotification(content.ViewModel, notifier);
            this.content = content;
            this.notifier = notifier;
            this.notifier.ContentTemplate = NotificationServiceTemplatesHelper.PredefinedToastTemplate;
        }

        public void Hide() {
            notifier.Hide(toast);
        }
        public Task<ToastNotificationResultInternal> ShowAsync() {
            return notifier.ShowAsync(toast, content.Duration).ContinueWith(t => (ToastNotificationResultInternal)t.Result);
        }
    }
}