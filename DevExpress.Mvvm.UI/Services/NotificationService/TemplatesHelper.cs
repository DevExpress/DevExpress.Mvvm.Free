using DevExpress.Internal;
using System;
using System.Windows;

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
}