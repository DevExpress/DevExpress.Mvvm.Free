using System.ComponentModel;

namespace DevExpress.Mvvm.Native {
    public class PropertyChangedHelper {
        event PropertyChangedEventHandler PropertyChanged;
        public void AddHandler(PropertyChangedEventHandler handler) {
            PropertyChanged += handler;
        }
        public void RemoveHandler(PropertyChangedEventHandler handler) {
            PropertyChanged -= handler;
        }
        public void OnPropertyChanged(INotifyPropertyChanged obj, string propertyName) {
            var handler = PropertyChanged;
            if(handler != null)
                handler(obj, new PropertyChangedEventArgs(propertyName));
        }
    }
}