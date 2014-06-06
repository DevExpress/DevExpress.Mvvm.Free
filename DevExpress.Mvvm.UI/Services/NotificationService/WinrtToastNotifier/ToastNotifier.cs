using System;
using System.Threading.Tasks;
using DevExpress.Internal.WinApi.Windows.UI.Notifications;

namespace DevExpress.Internal {
    public interface IPredefinedToastNotification {
        Task<ToastNotificationResultInternal> ShowAsync();
        void Hide();
    }
    public interface IPredefinedToastNotificationInfo {
        ToastTemplateType ToastTemplateType { get; }
        string[] Lines { get; }
        string ImagePath { get; }
        NotificationDuration Duration { get; }
        PredefinedSound Sound { get; }
    }
    public interface IPredefinedToastNotificationContent : IDisposable {
        IPredefinedToastNotificationInfo Info { get; }
        void SetDuration(NotificationDuration duration);
        void SetImage(byte[] image);
        void SetImage(string imagePath);
        void SetImage(System.Drawing.Image image);
        void SetSound(PredefinedSound sound);
    }
    public interface IPredefinedToastNotificationContentFactory {
        IPredefinedToastNotificationContent CreateContent(string bodyText);
        IPredefinedToastNotificationContent CreateOneLineHeaderContent(string headlineText, string bodyText);
        IPredefinedToastNotificationContent CreateOneLineHeaderContent(string headlineText, string bodyText1, string bodyText2);
        IPredefinedToastNotificationContent CreateTwoLineHeaderContent(string headlineText, string bodyText);
    }
    public interface IPredefinedToastNotificationFactory {
        IPredefinedToastNotificationContentFactory CreateContentFactory();
        IPredefinedToastNotification CreateToastNotification(IPredefinedToastNotificationContent content);
        IPredefinedToastNotification CreateToastNotification(string bodyText);
        IPredefinedToastNotification CreateToastNotificationOneLineHeaderContent(string headlineText, string bodyText);
        IPredefinedToastNotification CreateToastNotificationOneLineHeaderContent(string headlineText, string bodyText1, string bodyText2);
        IPredefinedToastNotification CreateToastNotificationTwoLineHeader(string headlineText, string bodyText);
    }
    public enum ToastNotificationResultInternal {
        Activated = 0,
        UserCanceled = 1,
        TimedOut = 2,
        ApplicationHidden = 3,
        Dropped = 4,
    }
}