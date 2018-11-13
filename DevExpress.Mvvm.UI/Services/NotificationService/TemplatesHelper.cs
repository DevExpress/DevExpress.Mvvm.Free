using DevExpress.Internal;
using System;
using System.Windows;
using System.Windows.Data;
using System.Globalization;
using System.Windows.Media;

namespace DevExpress.Mvvm.UI.Native {
    public static class NotificationServiceTemplatesHelper {
        static ResourceDictionary resourceDictionary = null;
        static ResourceDictionary GetResourceDictionary() {
            if(resourceDictionary == null) {
                resourceDictionary = new ResourceDictionary();
                string path = string.Format("pack://application:,,,/{0};component/Services/NotificationService/PredefinedToastNotification.xaml", AssemblyInfo.SRAssemblyXpfMvvmUIFree);
                resourceDictionary.Source = new Uri(path, UriKind.Absolute);
            }
            return resourceDictionary;
        }

        static DataTemplate defaultCustomToastTemplate = null;
        public static DataTemplate DefaultCustomToastTemplate {
            get {
                if(defaultCustomToastTemplate == null) {
                    ResourceDictionary dict = GetResourceDictionary();
                    defaultCustomToastTemplate = (DataTemplate)dict["DefaultCustomToastTemplate"];
                }
                return defaultCustomToastTemplate;
            }
        }
        static DataTemplate predefinedToastTemplate = null;
        public static DataTemplate PredefinedToastTemplate {
            get {
                if(predefinedToastTemplate == null) {
                    ResourceDictionary dict = GetResourceDictionary();
                    predefinedToastTemplate = (DataTemplate)dict["PredefinedToastTemplate"];
                }
                return predefinedToastTemplate;
            }
        }
    }

    public class PredefinedNotificationViewModelBackgroundColorToBrushConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            var vm = value as PredefinedToastNotificationVewModel;
            return new SolidColorBrush { Color = vm == null ? Colors.Transparent : vm.BackgroundColor };
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }
}