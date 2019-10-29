using DevExpress.Data;
using DevExpress.Internal;
using DevExpress.Mvvm.Native;
using DevExpress.Mvvm.UI.Interactivity;
using DevExpress.Mvvm.UI.Interactivity.Internal;
using DevExpress.Mvvm.UI.Native;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;

namespace DevExpress.Mvvm.UI {
    public enum NotificationSetting {
        Enabled = 0,
        DisabledForApplication = 1,
        DisabledForUser = 2,
        DisabledByGroupPolicy = 3,
        DisabledByManifest = 4
    }

    public enum ToastDismissalReason : long {
        UserCanceled = 0,
        ApplicationHidden = 1,
        TimedOut = 2
    }
    public enum PredefinedSound {
        Notification_Default,
        NoSound,
        Notification_IM,
        Notification_Mail,
        Notification_Reminder,
        Notification_SMS,
        Notification_Looping_Alarm,
        Notification_Looping_Alarm2,
        Notification_Looping_Alarm3,
        Notification_Looping_Alarm4,
        Notification_Looping_Alarm5,
        Notification_Looping_Alarm6,
        Notification_Looping_Alarm7,
        Notification_Looping_Alarm8,
        Notification_Looping_Alarm9,
        Notification_Looping_Alarm10,
        Notification_Looping_Call,
        Notification_Looping_Call2,
        Notification_Looping_Call3,
        Notification_Looping_Call4,
        Notification_Looping_Call5,
        Notification_Looping_Call6,
        Notification_Looping_Call7,
        Notification_Looping_Call8,
        Notification_Looping_Call9,
        Notification_Looping_Call10,
    }

    public enum PredefinedNotificationDuration {
        Default,
        Long
    }

    public enum NotificationTemplate {
        LongText,
        ShortHeaderAndLongText,
        LongHeaderAndShortText,
        ShortHeaderAndTwoTextFields
    }

    public enum NotificationPosition {
        BottomRight, TopRight
    }

    public enum NotificationScreen {
        Primary, ApplicationWindow
    }

    [TargetTypeAttribute(typeof(UserControl))]
    [TargetTypeAttribute(typeof(Window))]
    public class NotificationService : ServiceBase, INotificationService {
        class MvvmPredefinedNotification : INotification {
            public IPredefinedToastNotification Notification { get; set; }
            public Task<NotificationResult> ShowAsync() {
                return Notification.ShowAsync().ContinueWith(t => (NotificationResult)t.Result);
            }
            public void Hide() {
                Notification.Hide();
            }
        }
        internal class MvvmCustomNotification : INotification {
            CustomNotifier notifier;
            CustomNotification notification;
            Window window;
            internal int duration;
            public MvvmCustomNotification(object viewModel, CustomNotifier notifier, Window window, int duration) {
                this.notifier = notifier;
                this.window = window;
                this.duration = duration;
                this.notification = new CustomNotification(viewModel, notifier);
            }
            public void Hide() {
                notifier.Hide(notification);
            }
            public Task<NotificationResult> ShowAsync() {
                Point position = new Point();
                if(window != null) {
                    var pointToScreen = window.PointToScreen(new Point());
                    if(pointToScreen != null) {
                        position = new Point(pointToScreen.X + window.Width / 2, pointToScreen.Y + window.Height / 2);
                    }
                }
                notifier.ChangeScreen(position);
                return notifier.ShowAsync(notification, duration);
            }
        }
        public static readonly DependencyProperty UseWin8NotificationsIfAvailableProperty =
            DependencyProperty.Register("UseWin8NotificationsIfAvailable", typeof(bool), typeof(NotificationService), new PropertyMetadata(true,
                (d, e) => ((NotificationService)d).OnUseWin8NotificationsIfAvailableChanged()));

        public static readonly DependencyProperty CustomNotificationStyleProperty =
            DependencyProperty.Register("CustomNotificationStyle", typeof(Style), typeof(NotificationService), new PropertyMetadata(null,
                (d, e) => ((NotificationService)d).OnCustomNotificationStyleChanged()));

        public static readonly DependencyProperty CustomNotificationTemplateProperty =
            DependencyProperty.Register("CustomNotificationTemplate", typeof(DataTemplate), typeof(NotificationService), new PropertyMetadata(null,
                (d, e) => ((NotificationService)d).OnCustomNotificationTemplateChanged()));

        public static readonly DependencyProperty CustomNotificationTemplateSelectorProperty =
            DependencyProperty.Register("CustomNotificationTemplateSelector", typeof(DataTemplateSelector), typeof(NotificationService), new PropertyMetadata(null,
                (d, e) => ((NotificationService)d).OnCustomNotificationTemplateSelectorChanged()));

        public static readonly DependencyProperty CustomNotificationDurationProperty =
            DependencyProperty.Register("CustomNotificationDuration", typeof(TimeSpan), typeof(NotificationService), new PropertyMetadata(TimeSpan.FromMilliseconds(6000)));

        public static readonly DependencyProperty CustomNotificationPositionProperty =
            DependencyProperty.Register("CustomNotificationPosition", typeof(NotificationPosition), typeof(NotificationService), new PropertyMetadata(NotificationPosition.TopRight,
                (d, e) => ((NotificationService)d).UpdateCustomNotifierPositioner()));

        public static readonly DependencyProperty CustomNotificationVisibleMaxCountProperty =
            DependencyProperty.Register("CustomNotificationVisibleMaxCount", typeof(int), typeof(NotificationService), new PropertyMetadata(3,
                (d, e) => ((NotificationService)d).UpdateCustomNotifierPositioner()));

        public static readonly DependencyProperty PredefinedNotificationTemplateProperty =
            DependencyProperty.Register("PredefinedNotificationTemplate", typeof(NotificationTemplate), typeof(NotificationService), new PropertyMetadata(NotificationTemplate.LongText));

        public static readonly DependencyProperty ApplicationIdProperty =
            DependencyProperty.Register("ApplicationId", typeof(string), typeof(NotificationService), new PropertyMetadata(null));

        public static readonly DependencyProperty PredefinedNotificationDurationProperty =
            DependencyProperty.Register("PredefinedNotificationDuration", typeof(PredefinedNotificationDuration), typeof(NotificationService), new PropertyMetadata(PredefinedNotificationDuration.Default));

        public static readonly DependencyProperty SoundProperty =
            DependencyProperty.Register("Sound", typeof(PredefinedSound), typeof(NotificationService), new PropertyMetadata(PredefinedSound.Notification_Default));

        public NotificationScreen CustomNotificationScreen {
            get { return (NotificationScreen)GetValue(CustomNotificationScreenProperty); }
            set { SetValue(CustomNotificationScreenProperty, value); }
        }
        public static readonly DependencyProperty CustomNotificationScreenProperty =
            DependencyProperty.Register("CustomNotificationScreen", typeof(NotificationScreen), typeof(NotificationService), new PropertyMetadata(NotificationScreen.Primary));
        IPredefinedToastNotificationFactory predefinedNotificationsFactory;
        IPredefinedToastNotificationFactory PredefinedNotificationsFactory {
            get {
                if(predefinedNotificationsFactory == null) {
                    if(UseWin8NotificationsIfAvailable && AreWin8NotificationsAvailable) {
                        if(ApplicationId == null)
                            throw new ArgumentNullException("ApplicationId");
                        predefinedNotificationsFactory = new WinRTToastNotificationFactory(ApplicationId);
                    } else {
                        predefinedNotificationsFactory = new WpfToastNotificationFactory(PredefinedNotifier);
                    }
                }
                return predefinedNotificationsFactory;
            }
        }
        CustomNotifier customNotifier;
        CustomNotifier CustomNotifier {
            get {
                if(customNotifier == null) {
                    customNotifier = new CustomNotifier();
                }
                return customNotifier;
            }
        }
        CustomNotifier predefinedNotifier;
        CustomNotifier PredefinedNotifier {
            get {
                if(predefinedNotifier == null) {
                    predefinedNotifier = new CustomNotifier();
                }
                return predefinedNotifier;
            }
        }
        public bool UseWin8NotificationsIfAvailable {
            get { return (bool)GetValue(UseWin8NotificationsIfAvailableProperty); }
            set { SetValue(UseWin8NotificationsIfAvailableProperty, value); }
        }
        public Style CustomNotificationStyle {
            get { return (Style)GetValue(CustomNotificationStyleProperty); }
            set { SetValue(CustomNotificationStyleProperty, value); }
        }
        public DataTemplate CustomNotificationTemplate {
            get { return (DataTemplate)GetValue(CustomNotificationTemplateProperty); }
            set { SetValue(CustomNotificationTemplateProperty, value); }
        }
        public DataTemplateSelector CustomNotificationTemplateSelector {
            get { return (DataTemplateSelector)GetValue(CustomNotificationTemplateSelectorProperty); }
            set { SetValue(CustomNotificationTemplateSelectorProperty, value); }
        }
        public TimeSpan CustomNotificationDuration {
            get { return (TimeSpan)GetValue(CustomNotificationDurationProperty); }
            set { SetValue(CustomNotificationDurationProperty, value); }
        }
        public NotificationPosition CustomNotificationPosition {
            get { return (NotificationPosition)GetValue(CustomNotificationPositionProperty); }
            set { SetValue(CustomNotificationPositionProperty, value); }
        }
        public int CustomNotificationVisibleMaxCount {
            get { return (int)GetValue(CustomNotificationVisibleMaxCountProperty); }
            set { SetValue(CustomNotificationVisibleMaxCountProperty, value); }
        }
        void UpdateCustomNotifierPositioner() {
            PredefinedNotifier.UpdatePositioner(CustomNotificationPosition, CustomNotificationVisibleMaxCount);
            CustomNotifier.UpdatePositioner(CustomNotificationPosition, CustomNotificationVisibleMaxCount);
        }
        void OnCustomNotificationTemplateChanged() {
            CustomNotifier.ContentTemplate = CustomNotificationTemplate;
        }
        void OnCustomNotificationTemplateSelectorChanged() {
            CustomNotifier.ContentTemplateSelector = CustomNotificationTemplateSelector;
        }
        void OnCustomNotificationStyleChanged() {
            CustomNotifier.Style = CustomNotificationStyle;
        }
        void OnUseWin8NotificationsIfAvailableChanged() {
            predefinedNotificationsFactory = null;
        }
        public string ApplicationId {
            get { return (string)GetValue(ApplicationIdProperty); }
            set { SetValue(ApplicationIdProperty, value); }
        }
        public NotificationTemplate PredefinedNotificationTemplate {
            get { return (NotificationTemplate)GetValue(PredefinedNotificationTemplateProperty); }
            set { SetValue(PredefinedNotificationTemplateProperty, value); }
        }
        public PredefinedSound Sound {
            get { return (PredefinedSound)GetValue(SoundProperty); }
            set { SetValue(SoundProperty, value); }
        }
        public PredefinedNotificationDuration PredefinedNotificationDuration {
            get { return (PredefinedNotificationDuration)GetValue(PredefinedNotificationDurationProperty); }
            set { SetValue(PredefinedNotificationDurationProperty, value); }
        }
        public bool AreWin8NotificationsAvailable {
            get { return DevExpress.Internal.WinApi.ToastNotificationManager.AreToastNotificationsSupported; }
        }
        public INotification CreateCustomNotification(object viewModel) {
            Window window = CustomNotificationScreen == UI.NotificationScreen.ApplicationWindow ?
                Window.GetWindow(AssociatedObject) : null;
            return new MvvmCustomNotification(viewModel, CustomNotifier, window,
                (int)Math.Max(0, Math.Min(int.MaxValue, CustomNotificationDuration.TotalMilliseconds)));
        }

        public INotification CreatePredefinedNotification(
            string text1,
            string text2,
            string text3,
            ImageSource image = null) {
            IPredefinedToastNotificationContentFactory cf = PredefinedNotificationsFactory.CreateContentFactory();
            IPredefinedToastNotificationContent content = null;
            switch(PredefinedNotificationTemplate) {
                case NotificationTemplate.LongText:
                    content = cf.CreateContent(text1);
                    break;
                case NotificationTemplate.ShortHeaderAndLongText:
                    content = cf.CreateOneLineHeaderContent(text1, text2);
                    break;
                case NotificationTemplate.LongHeaderAndShortText:
                    content = cf.CreateTwoLineHeaderContent(text1, text2);
                    break;
                case NotificationTemplate.ShortHeaderAndTwoTextFields:
                    content = cf.CreateOneLineHeaderContent(text1, text2, text3);
                    break;
            }
            if(image != null) {
                var dpi = PrimaryScreen.GetDpi();
                var width = PredefinedNotificationsFactory.ImageSize;
                var size = new Size(width * dpi.X, width * dpi.Y);
                content.SetImage(ImageLoader2.ImageToByteArray(image, GetBaseUri, size));
            }
            content.SetDuration((DevExpress.Internal.NotificationDuration)PredefinedNotificationDuration);
            content.SetSound((DevExpress.Internal.PredefinedSound)Sound);
            return new MvvmPredefinedNotification { Notification = PredefinedNotificationsFactory.CreateToastNotification(content) };
        }
    }
}