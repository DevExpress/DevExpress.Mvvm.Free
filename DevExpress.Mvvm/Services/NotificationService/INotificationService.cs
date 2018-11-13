using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace DevExpress.Mvvm {
    public interface INotification {
        Task<NotificationResult> ShowAsync();
        void Hide();
    }

    public enum NotificationResult {
        Activated,
        UserCanceled,
        TimedOut,
        ApplicationHidden,
        Dropped
    }

    public interface INotificationService {
        INotification CreatePredefinedNotification(
            string text1,
            string text2,
            string text3,
            ImageSource image = null);
        INotification CreateCustomNotification(object viewModel);
    }
}