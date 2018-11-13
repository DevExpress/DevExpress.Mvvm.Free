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
    public interface IPredefinedToastNotificationInfoGeneric {
        string AppLogoImagePath { get; }
        string HeroImagePath { get; }
        ImageCropType AppLogoImageCrop { get; }
        string AttributionText { get; }
        DateTimeOffset? DisplayTimestamp { get; }
        Action<System.Xml.XmlDocument> UpdateToastContent { get; set; }
    }
    public interface IPredefinedToastNotificationContent : IDisposable {
        IPredefinedToastNotificationInfo Info { get; }
        void SetDuration(NotificationDuration duration);
        void SetImage(byte[] image);
        void SetImage(string imagePath);
        void SetImage(System.Drawing.Image image);
        void SetSound(PredefinedSound sound);
    }
    public interface IPredefinedToastNotificationContentGeneric {
        void SetImage(string imagePath, ImagePlacement placement);
        void SetImage(System.Drawing.Image image, ImagePlacement placement);
        void SetAppLogoImageCrop(ImageCropType appLogoImageCrop);
        void SetAttributionText(string attributionText);
        void SetDisplayTimestamp(DateTimeOffset? displayTimestamp);
        void SetUpdateToastContentAction(Action<System.Xml.XmlDocument> updateToastContentAction);
    }
    public interface IPredefinedToastNotificationContentFactory {
        IPredefinedToastNotificationContent CreateContent(string bodyText);
        IPredefinedToastNotificationContent CreateOneLineHeaderContent(string headlineText, string bodyText);
        IPredefinedToastNotificationContent CreateOneLineHeaderContent(string headlineText, string bodyText1, string bodyText2);
        IPredefinedToastNotificationContent CreateTwoLineHeaderContent(string headlineText, string bodyText);
    }
    public interface IPredefinedToastNotificationContentFactoryGeneric {
        IPredefinedToastNotificationContent CreateToastGeneric(string headlineText, string bodyText1, string bodyText2);
    }
    public interface IPredefinedToastNotificationFactory {
        double ImageSize { get; }
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
    public enum ImagePlacement {
        Inline = 0,
        Hero = 1,
        AppLogo = 2
    }
    public enum ImageCropType {
        Default = 0,
        None = 1,
        Circle = 2
    }
}