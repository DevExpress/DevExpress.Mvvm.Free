
namespace DevExpress.Mvvm.UI {
    static class DXSplashScreenExceptions {
        public const string Exception1 = "SplashScreen has been displayed. Only one splash screen can be displayed simultaneously.";
        public const string Exception2 = "Incorrect SplashScreen Type.";
        public const string Exception3 = "Show SplashScreen before calling this method.";
        public const string Exception4 = "This method is not supported if UserControl is used as SplashScreen.";
        public const string Exception5 = "Splash Screen should be inherited from the defined type.";
        public const string Exception6 = "{0} should implement the ISplashScreen interface";

        public const string ServiceException1 = "Cannot use ViewLocator, ViewTemplate and DocumentType if SplashScreenType is set. If you set the SplashScreenType property, do not set the other properties.";
        public const string ServiceException2 = "Show splash screen before calling this method.";
        public const string ServiceException3 = "This method is not supported when SplashScreenType is used.";
    }
}