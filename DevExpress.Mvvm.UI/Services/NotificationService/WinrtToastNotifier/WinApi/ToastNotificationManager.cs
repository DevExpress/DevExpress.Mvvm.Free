using System;
using System.Security;
using DevExpress.Utils;
using DevExpress.Internal.WinApi.Windows.UI.Notifications;

namespace DevExpress.Internal.WinApi {
    [CLSCompliant(false)]
    public interface IToastNotificationAdapter {
        IToastNotification Create(IPredefinedToastNotificationInfo info);
        void Show(IToastNotification notification);
        void Hide(IToastNotification notification);
    }
    [SecuritySafeCritical]
    public static class ToastNotificationManager {
        static Version Win8Version = new Version(6, 2, 9200, 0);
        static bool IsWin8OrHigher {
            get {
                OperatingSystem OS = Environment.OSVersion;
                return (OS.Platform == PlatformID.Win32NT) && (OS.Version >= Win8Version);
            }
        }
        public static bool IsGenericTemplateSupported {
            get { return WindowsVersionProvider.IsWin10AnniversaryUpdateOrHigher; }
        }
        public static bool AreToastNotificationsSupported {
            get { return IsWin8OrHigher; }
        }
        static IToastNotificationManager defaultManager;
        internal static IToastNotificationManager GetDefaultManager() {
            if(defaultManager == null)
                defaultManager = ComFunctions.RoGetActivationFactory<IToastNotificationManager>();
            return defaultManager;
        }
        internal static Window.Data.Xml.Dom.IXmlDocument GetDocument(IPredefinedToastNotificationInfo info) {
            return GetDocument(GetDefaultManager(), info);
        }
        internal static Window.Data.Xml.Dom.IXmlDocument GetDocument(IToastNotificationManager manager, IPredefinedToastNotificationInfo info) {
            return WinRTToastNotificationContent.GetDocument(manager, info);
        }
        internal static string GetXml(IPredefinedToastNotificationInfo info) {
            return GetXml((Window.Data.Xml.Dom.IXmlNodeSerializer)GetDocument(info));
        }
        internal static string GetXml(Window.Data.Xml.Dom.IXmlNodeSerializer content) {
            string xml;
            content.GetXml(out xml);
            return xml;
        }
        internal static void LoadXml(Window.Data.Xml.Dom.IXmlDocumentIO content, string xml) {
            content.LoadXml(xml);
        }
        internal static IToastNotificationAdapter CreateToastNotificationAdapter(string appID) {
            return AreToastNotificationsSupported ?
                        (IToastNotificationAdapter)new ToastNotificationAdapter(appID, GetDefaultManager()) :
                        (IToastNotificationAdapter)new EmptyToastNotificationAdapter();
        }
        #region ToastNotifierAdapter
        class ToastNotificationAdapter : IToastNotificationAdapter {
            string appId;
            IToastNotifier notifier;
            IToastNotificationFactory factory;
            IToastNotificationManager manager;
            public ToastNotificationAdapter(string appId, IToastNotificationManager manager) {
                this.appId = appId;
                this.manager = manager;
            }
            IToastNotification IToastNotificationAdapter.Create(IPredefinedToastNotificationInfo info) {
                var content = ToastNotificationManager.GetDocument(manager, info);
                if(factory == null)
                    factory = ComFunctions.RoGetActivationFactory<IToastNotificationFactory>();
                IToastNotification result;
                ComFunctions.CheckHRESULT(factory.CreateToastNotification(content, out result));
                return result;
            }
            void IToastNotificationAdapter.Show(IToastNotification notification) {
                if(notifier == null)
                    ComFunctions.CheckHRESULT(manager.CreateToastNotifierWithId(appId, out notifier));
                if(notifier != null && notification != null)
                    notifier.Show(notification);
            }
            void IToastNotificationAdapter.Hide(IToastNotification notification) {
                if(notifier == null)
                    ComFunctions.CheckHRESULT(manager.CreateToastNotifierWithId(appId, out notifier));
                if(notifier != null && notification != null)
                    notifier.Hide(notification);
            }
        }
        class EmptyToastNotificationAdapter : IToastNotificationAdapter {
            IToastNotification IToastNotificationAdapter.Create(IPredefinedToastNotificationInfo info) { return null; }
            void IToastNotificationAdapter.Show(IToastNotification notification) { }
            void IToastNotificationAdapter.Hide(IToastNotification notification) { }
        }
        #endregion
    }
}