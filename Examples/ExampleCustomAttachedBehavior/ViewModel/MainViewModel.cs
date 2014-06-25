using DevExpress.Mvvm.DataAnnotations;
using System.Windows.Media;

namespace Example.ViewModel {
    [POCOViewModel]
    public class MainViewModel {
        public virtual string InvalidValue { get; set; }
        public virtual Brush ValidBrush { get; set; }
        public virtual Brush InvalidBrush { get; set; }

        public void SetRedValidBrush() {
            ValidBrush = new SolidColorBrush(Colors.Red);
        }
        public void SetBlueValidBrush() {
            ValidBrush = new SolidColorBrush(Colors.Blue);
        }
        public void SetBlackValidBrush() {
            ValidBrush = new SolidColorBrush(Colors.Black);
        }

        public void SetRedInvalidBrush() {
            InvalidBrush = new SolidColorBrush(Colors.Red);
        }
        public void SetBlueInvalidBrush() {
            InvalidBrush = new SolidColorBrush(Colors.Blue);
        }
        public void SetBlackInvalidBrush() {
            InvalidBrush = new SolidColorBrush(Colors.Black);
        }

        public MainViewModel() {
            InvalidValue = "Error";
            SetBlackValidBrush();
            SetRedInvalidBrush();
        }
    }
}
