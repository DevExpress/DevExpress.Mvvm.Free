using System;
using System.ComponentModel;

namespace DevExpress.Internal {
    public class ToastNotificationFailedException : Exception {
        public ToastNotificationFailedException(Exception inner, int errorCode)
            : base(null, inner) {
            ErrorCode = errorCode;
        }
        public override string Message {
            get { return InnerException.Message; }
        }
        public int ErrorCode { get; private set; }
        [System.Security.SecuritySafeCritical]
        internal static ToastNotificationFailedException ToException(int hResult) {
            try {
                System.Runtime.InteropServices.Marshal.ThrowExceptionForHR(hResult);
                return null;
            }
            catch(Exception e) {
                return new ToastNotificationFailedException(e, hResult);
            }
        }
    }
    [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
    public enum NotificationSetting {
        Enabled = 0,
        DisabledForApplication = 1,
        DisabledForUser = 2,
        DisabledByGroupPolicy = 3,
        DisabledByManifest = 4
    }
    [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
    public enum ToastDismissalReason : long {
        UserCanceled = 0,
        ApplicationHidden = 1,
        TimedOut = 2
    }
    [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
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
    [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
    public enum NotificationDuration {
        Default,
        Long
    }
}