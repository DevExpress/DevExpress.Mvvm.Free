using DevExpress.Internal;
using DevExpress.Internal.WinApi.Windows.UI.Notifications;
using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Media.Imaging;

namespace DevExpress.Mvvm.UI.Native {
    internal class WpfPredefinedToastNotificationContent : IPredefinedToastNotificationContent {
        const int msDefaultDuration = 6000;
        const int msLongDuration = 25000;
        public PredefinedToastNotificationVewModel ViewModel { get; private set; }
        public int Duration { get; set; }
        public WpfPredefinedToastNotificationContent(PredefinedToastNotificationVewModel viewModel) {
            ViewModel = viewModel;
            Duration = msDefaultDuration;
        }
        public void SetImage(byte[] image) {
            var bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.StreamSource = new MemoryStream(image);
            bitmap.EndInit();
            ViewModel.Image = bitmap;
        }
        public void SetImage(System.Drawing.Image image) {
            throw new NotImplementedException();
        }
        public void SetImage(string path) {
            throw new NotImplementedException();
        }
        public void SetSound(DevExpress.Internal.PredefinedSound sound) {
            if (sound != DevExpress.Internal.PredefinedSound.NoSound)
                Debug.WriteLine("Only Windows 8 toast notifications support sound.");
        }
        public void SetDuration(DevExpress.Internal.NotificationDuration duration) {
            Duration = duration == DevExpress.Internal.NotificationDuration.Long ? msLongDuration : msDefaultDuration;
        }
        public void Dispose() { }
        public DevExpress.Internal.IPredefinedToastNotificationInfo Info {
            get { return new WpfTToastNotificationInfo(); }
        }
        class WpfTToastNotificationInfo : DevExpress.Internal.IPredefinedToastNotificationInfo {
            public ToastTemplateType ToastTemplateType { get; set; }
            public string[] Lines { get; set; }
            public string ImagePath { get; set; }
            public DevExpress.Internal.NotificationDuration Duration { get; set; }
            public DevExpress.Internal.PredefinedSound Sound { get; set; }
        }
    }
}