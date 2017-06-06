using DevExpress.Internal;
using System;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using System.Windows.Interop;
using System.Windows.Media.Imaging;

namespace DevExpress.Mvvm.UI.Native {
    public class WpfToastNotificationFactory : IPredefinedToastNotificationFactory {
        CustomNotifier notifier;
        public WpfToastNotificationFactory(CustomNotifier notifier) {
            this.notifier = notifier;
        }
        public IPredefinedToastNotification CreateToastNotification(IPredefinedToastNotificationContent content) {
            return new WpfPredefinedToastNotification((WpfPredefinedToastNotificationContent)content, notifier);
        }
        public IPredefinedToastNotification CreateToastNotification(string bodyText) {
            return CreateToastNotification(DefaultFactory.CreateContent(bodyText));
        }
        public IPredefinedToastNotification CreateToastNotificationOneLineHeaderContent(string headlineText, string bodyText) {
            return CreateToastNotification(DefaultFactory.CreateOneLineHeaderContent(headlineText, bodyText));
        }
        public IPredefinedToastNotification CreateToastNotificationOneLineHeaderContent(string headlineText, string bodyText1, string bodyText2) {
            return CreateToastNotification(DefaultFactory.CreateOneLineHeaderContent(headlineText, bodyText1, bodyText2));
        }
        public IPredefinedToastNotification CreateToastNotificationTwoLineHeader(string headlineText, string bodyText) {
            return CreateToastNotification(DefaultFactory.CreateTwoLineHeaderContent(headlineText, bodyText));
        }
        IPredefinedToastNotificationContentFactory factoryCore;
        IPredefinedToastNotificationContentFactory DefaultFactory {
            get {
                if(factoryCore == null)
                    factoryCore = CreateContentFactory();
                return factoryCore;
            }
        }

        public double ImageSize { get { return 90; } }

        public virtual IPredefinedToastNotificationContentFactory CreateContentFactory() {
            return new WpfPredefinedToastNotificationContentFactory();
        }
        class WpfPredefinedToastNotificationContentFactory : IPredefinedToastNotificationContentFactory {
            #region IPredefinedToastNotificationContentFactory Members
            public IPredefinedToastNotificationContent CreateContent(string bodyText) {
                PredefinedToastNotificationVewModel vm = CreateDefaultViewModel();
                vm.ToastTemplate = NotificationTemplate.LongText;
                vm.Text1 = bodyText;
                return new WpfPredefinedToastNotificationContent(vm);
            }
            public IPredefinedToastNotificationContent CreateOneLineHeaderContent(string headlineText, string bodyText1, string bodyText2) {
                PredefinedToastNotificationVewModel vm = CreateDefaultViewModel();
                vm.ToastTemplate = NotificationTemplate.ShortHeaderAndTwoTextFields;
                vm.Text1 = headlineText;
                vm.Text2 = bodyText1;
                vm.Text3 = bodyText2;
                return new WpfPredefinedToastNotificationContent(vm);
            }
            public IPredefinedToastNotificationContent CreateOneLineHeaderContent(string headlineText, string bodyText) {
                PredefinedToastNotificationVewModel vm = CreateDefaultViewModel();
                vm.ToastTemplate = NotificationTemplate.ShortHeaderAndLongText;
                vm.Text1 = headlineText;
                vm.Text2 = bodyText;
                return new WpfPredefinedToastNotificationContent(vm);
            }
            public IPredefinedToastNotificationContent CreateTwoLineHeaderContent(string headlineText, string bodyText) {
                PredefinedToastNotificationVewModel vm = CreateDefaultViewModel();
                vm.ToastTemplate = NotificationTemplate.LongHeaderAndShortText;
                vm.Text1 = headlineText;
                vm.Text2 = bodyText;
                return new WpfPredefinedToastNotificationContent(vm);
            }
            #endregion
            static PredefinedToastNotificationVewModel CreateDefaultViewModel() {
                var vm = new PredefinedToastNotificationVewModel();
                Icon icon = ExtractAssociatedIcon(Environment.GetCommandLineArgs()[0]);
                if(icon != null) {
                    BitmapSource bitmapSource = Imaging.CreateBitmapSourceFromHIcon(
                            icon.Handle,
                            new System.Windows.Int32Rect(0, 0, icon.Width, icon.Height),
                            BitmapSizeOptions.FromEmptyOptions());
                    vm.Icon = bitmapSource;
                    vm.BackgroundColor = BackgroundCalculator.GetBestMatch(icon.ToBitmap());
                } else {
                    vm.BackgroundColor = BackgroundCalculator.DefaultGrayColor;
                }
                return vm;
            }
            [SecuritySafeCritical]
            static Icon ExtractAssociatedIcon(string path) {
                if(string.IsNullOrEmpty(path))
                    return null;
                if(!File.Exists(path)) {
                    path += ".exe";
                    if(!File.Exists(path)) {
                        return null;
                    }
                }
                int index = 0;
                IntPtr handle = ExtractAssociatedIcon(IntPtr.Zero, new StringBuilder(path), ref index);
                if(handle != IntPtr.Zero)
                    return Icon.FromHandle(handle);
                return null;
            }
            [DllImport("shell32.dll", EntryPoint = "ExtractAssociatedIcon", CharSet = CharSet.Auto)]
            internal static extern IntPtr ExtractAssociatedIcon(IntPtr hInst, StringBuilder iconPath, ref int index);
        }
    }
}