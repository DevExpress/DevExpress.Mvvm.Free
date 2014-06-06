using System.Windows;

#if SILVERLIGHT
namespace DevExpress.Mvvm {
    public enum DXMessageBoxButton { OK, OKCancel, YesNo, YesNoCancel }
}
#endif
namespace DevExpress.Mvvm {
    public interface IMessageBoxButtonLocalizer {
        string Localize(MessageBoxResult button);
    }
    public class DefaultMessageBoxButtonLocalizer : IMessageBoxButtonLocalizer {
        public string Localize(MessageBoxResult button) {
            switch(button) {
                case MessageBoxResult.OK:
                    return "OK";
                case MessageBoxResult.Cancel:
                    return "Cancel";
                case MessageBoxResult.Yes:
                    return "Yes";
                case MessageBoxResult.No:
                    return "No";
            }
            return string.Empty;
        }
    }
}