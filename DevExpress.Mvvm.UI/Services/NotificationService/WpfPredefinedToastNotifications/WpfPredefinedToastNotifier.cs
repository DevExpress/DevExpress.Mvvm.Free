using DevExpress.Internal;
using System;
using System.Drawing;
using System.Windows.Interop;
using System.Windows.Media.Imaging;

namespace DevExpress.Mvvm.UI.Native {
    public class WpfToastNotificationFactory : IPredefinedToastNotificationFactory {
        CustomNotifier notifier;
        public WpfToastNotificationFactory() {
            this.notifier = new CustomNotifier();
            this.notifier.UpdatePositioner(NotificationPosition.TopRight, 3);
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
        public virtual IPredefinedToastNotificationContentFactory CreateContentFactory() {
            return new WpfPredefinedToastNotificationContentFactory();
        }
        class WpfPredefinedToastNotificationContentFactory : IPredefinedToastNotificationContentFactory {
            #region
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
                Icon icon = Icon.ExtractAssociatedIcon(Environment.GetCommandLineArgs()[0]);
                BitmapSource bitmapSource = Imaging.CreateBitmapSourceFromHIcon(
                            icon.Handle,
                            new System.Windows.Int32Rect(0, 0, icon.Width, icon.Height),
                            BitmapSizeOptions.FromEmptyOptions());
                var vm = new PredefinedToastNotificationVewModel();
                vm.Icon = bitmapSource;
                vm.BackgroundColor = BackgroundCalculator.GetBestMatch(icon.ToBitmap());
                return vm;
            }
        }
    }
}