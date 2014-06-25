using DevExpress.Mvvm;
using DevExpress.Mvvm.DataAnnotations;
using DevExpress.Mvvm.POCO;
using System;
using System.Windows.Media.Imaging;

namespace Example.ViewModel {
    [POCOViewModel]
    public class MainViewModel {
        [ServiceProperty(Key = "defaultNotificationService")]
        protected virtual INotificationService DefaultNotificationService { get { return null; } }
        [ServiceProperty(Key = "customNotificationService")]
        protected virtual INotificationService CustomNotificationService { get { return null; } }
        public void ShowDefaultNotification() {
            INotification notification = DefaultNotificationService.CreatePredefinedNotification(
                "Hello", null, null, new BitmapImage(new Uri(@"/ExampleNotificationService;component/moon.png", UriKind.Relative)));
            notification.ShowAsync();
        }
        public void ShowCustomNotification() {
            CustomNotificationViewModel vm = ViewModelSource.Create(() => new CustomNotificationViewModel());
            vm.Header = "Custom Notification";
            vm.Text = "Hello";
            INotification notification = CustomNotificationService.CreateCustomNotification(vm);
            notification.ShowAsync();
        }
    }
    [POCOViewModel]
    public class CustomNotificationViewModel {
        public virtual string Header { get; set; }
        public virtual string Text { get; set; }
    }
}
