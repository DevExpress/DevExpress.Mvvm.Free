
namespace DevExpress.Mvvm.UI.Native {
    public class CustomNotification {
        public object ViewModel { get; set; }
        CustomNotifier notifier;
        public CustomNotification(object viewModel, CustomNotifier notifier) {
            ViewModel = viewModel;
            this.notifier = notifier;
        }
        public void Hide() { notifier.Hide(this); }
        internal void Activate() { notifier.Activate(this); }
        internal void Dismiss() { notifier.Dismiss(this); }
        internal void StopTimer() { notifier.StopTimer(this); }
        internal void ResetTimer() { notifier.ResetTimer(this); }
        internal void TimeOut() { notifier.TimeOut(this); }
    }
}