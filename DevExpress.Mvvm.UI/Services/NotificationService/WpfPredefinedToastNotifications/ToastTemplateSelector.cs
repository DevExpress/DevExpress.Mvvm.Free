using System.Windows;
using System.Windows.Controls;

namespace DevExpress.Mvvm.UI.Native {
    public class ToastTemplateSelector : DataTemplateSelector {
        public DataTemplate SimpleTemplate { get; set; }
        public DataTemplate OneLineHeaderTemplate { get; set; }
        public DataTemplate TwoLineHeaderTemplate { get; set; }
        public DataTemplate OneLineHeaderTwoLinesBodyTemplate { get; set; }

        public override DataTemplate SelectTemplate(object item, System.Windows.DependencyObject container) {
            switch(((PredefinedToastNotificationVewModel)item).ToastTemplate) {
                case NotificationTemplate.LongText:
                    return SimpleTemplate;
                case NotificationTemplate.ShortHeaderAndLongText:
                    return OneLineHeaderTemplate;
                case NotificationTemplate.LongHeaderAndShortText:
                    return TwoLineHeaderTemplate;
                case NotificationTemplate.ShortHeaderAndTwoTextFields:
                    return OneLineHeaderTwoLinesBodyTemplate;
            }
            return null;
        }
    }
}