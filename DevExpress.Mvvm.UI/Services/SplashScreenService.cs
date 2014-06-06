using DevExpress.Mvvm;
using DevExpress.Mvvm.Native;
using DevExpress.Mvvm.UI;
using DevExpress.Mvvm.UI.Interactivity;
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace DevExpress.Mvvm.UI {
    [TargetTypeAttribute(typeof(UserControl))]
    [TargetTypeAttribute(typeof(Window))]
    public class DXSplashScreenService : ViewServiceBase, ISplashScreenService {
        public static readonly DependencyProperty SplashScreenTypeProperty =
            DependencyProperty.Register("SplashScreenType", typeof(Type), typeof(DXSplashScreenService),
            new PropertyMetadata(null));
        public static readonly DependencyProperty SplashScreenWindowStyleProperty =
            DependencyProperty.Register("SplashScreenWindowStyle", typeof(Style), typeof(DXSplashScreenService),
            new PropertyMetadata(null, (d, e) => ((DXSplashScreenService)d).OnSplashScreenWindowStyleChanged((Style)e.OldValue, (Style)e.NewValue)));
        public static readonly DependencyProperty SplashScreenStartupLocationProperty =
            DependencyProperty.Register("SplashScreenStartupLocation", typeof(WindowStartupLocation), typeof(DXSplashScreenService),
            new PropertyMetadata(WindowStartupLocation.CenterScreen));
        public static readonly DependencyProperty ShowSplashScreenOnLoadingProperty =
            DependencyProperty.Register("ShowSplashScreenOnLoading", typeof(bool), typeof(DXSplashScreenService), new PropertyMetadata(false));
        public Type SplashScreenType {
            get { return (Type)GetValue(SplashScreenTypeProperty); }
            set { SetValue(SplashScreenTypeProperty, value); }
        }
        public Style SplashScreenWindowStyle {
            get { return (Style)GetValue(SplashScreenWindowStyleProperty); }
            set { SetValue(SplashScreenWindowStyleProperty, value); }
        }
        public WindowStartupLocation SplashScreenStartupLocation {
            get { return (WindowStartupLocation)GetValue(SplashScreenStartupLocationProperty); }
            set { SetValue(SplashScreenStartupLocationProperty, value); }
        }
        public bool ShowSplashScreenOnLoading {
            get { return (bool)GetValue(ShowSplashScreenOnLoadingProperty); }
            set { SetValue(ShowSplashScreenOnLoadingProperty, value); }
        }

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public new DataTemplateSelector ViewTemplateSelector { get { return null; } set { OnViewTemplateSelectorChanged(null, null); } }
        protected override void OnViewTemplateSelectorChanged(DataTemplateSelector oldValue, DataTemplateSelector newValue) {
            throw new InvalidOperationException("ViewTemplateSelector is not supported by DXSplashScreenService");
        }
        protected override void OnViewTemplateChanged(DataTemplate oldValue, DataTemplate newValue) {
            base.OnViewTemplateChanged(oldValue, newValue);
            if(newValue != null)
                newValue.Seal();
        }
        void OnSplashScreenWindowStyleChanged(Style oldValue, Style newValue) {
            if(newValue != null)
                newValue.Seal();
        }

        bool SplashScreenIsShownOnLoading = false;
        protected override void OnAttached() {
            base.OnAttached();
            AssociatedObject.Loaded += OnAssociatedObjectLoaded;
            if(ShowSplashScreenOnLoading && !DXSplashScreen.IsActive) {
                SplashScreenIsShownOnLoading = true;
                ((ISplashScreenService)this).ShowSplashScreen();
            }
        }
        protected override void OnDetaching() {
            AssociatedObject.Loaded -= OnAssociatedObjectLoaded;
            base.OnDetaching();
        }
        void OnAssociatedObjectLoaded(object sender, RoutedEventArgs e) {
            AssociatedObject.Loaded -= OnAssociatedObjectLoaded;
            if(SplashScreenIsShownOnLoading) {
                SplashScreenIsShownOnLoading = false;
                ((ISplashScreenService)this).HideSplashScreen();
            }
        }

        bool ISplashScreenService.IsSplashScreenActive {
            get { return DXSplashScreen.IsActive; }
        }
        void ISplashScreenService.ShowSplashScreen(string documentType) {
            if(SplashScreenType != null &&
                (!string.IsNullOrEmpty(documentType) || ViewTemplate != null || ViewLocator != null)) {
                    if(!string.IsNullOrEmpty(documentType) || ViewTemplate != null || ViewLocator != null)
                        throw new InvalidOperationException(DXSplashScreenExceptions.ServiceException1);
            }
            if(DXSplashScreen.IsActive) return;

            if(SplashScreenType != null)
                DXSplashScreen.Show(SplashScreenType);
            else
                DXSplashScreen.Show(CreateSplashScreenWindow, CreateSplashScreen,
                    new object[] { SplashScreenWindowStyle, SplashScreenStartupLocation },
                    new object[] { documentType, ViewLocator, ViewTemplate, SplashScreenType });
        }
        void ISplashScreenService.HideSplashScreen() {
            if(!DXSplashScreen.IsActive) return;
            DXSplashScreen.Close();
            if(AssociatedObject != null) {
                Window ownerWindow = Window.GetWindow(AssociatedObject);
                if(ownerWindow != null && !ownerWindow.IsActive)
                    ownerWindow.Activate();
            }
        }
        void ISplashScreenService.SetSplashScreenProgress(double progress, double maxProgress) {
            if(!DXSplashScreen.IsActive) return;
            DXSplashScreen.Progress(progress, maxProgress);
        }
        void ISplashScreenService.SetSplashScreenState(object state) {
            if(!DXSplashScreen.IsActive) return;
            DXSplashScreen.SetState(state);
        }

        static Window CreateSplashScreenWindow(object parameter) {
            object[] parameters = (object[])parameter;
            Style windowStyle = (Style)parameters[0];
            WindowStartupLocation startupLocation = (WindowStartupLocation)parameters[1];
            Window res;
            if(windowStyle != null) {
                res = new Window() { Style = windowStyle, WindowStartupLocation = startupLocation };
            } else {
                res = DXSplashScreen.DefaultSplashScreenWindowCreator(null);
                res.WindowStartupLocation = startupLocation;
                WindowFadeAnimationBehavior.SetEnableAnimation(res, true);
            }
            return res;
        }
        static object CreateSplashScreen(object parameter) {
            object[] parameters = (object[])parameter;
            string documentType = parameters[0] as string;
            IViewLocator viewLocator = parameters[1] as IViewLocator;
            DataTemplate viewTemplate = parameters[2] as DataTemplate;
            Type splashScreenType = parameters[3] as Type;
            var SplashScreenViewModel = new SplashScreenViewModel();
            object view = null;
            if(splashScreenType != null) {
                view = Activator.CreateInstance(splashScreenType);
                view.With(x => x as FrameworkElement).Do(x => x.DataContext = SplashScreenViewModel);
            } else
                view = ViewHelper.CreateAndInitializeView(viewLocator, documentType, SplashScreenViewModel, null, null, viewTemplate, null);
            return view;
        }
    }
}