using DevExpress.Mvvm;
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
        public static readonly DependencyProperty ProgressProperty =
            DependencyProperty.Register("Progress", typeof(double), typeof(DXSplashScreenService),
            new PropertyMetadata(SplashScreenViewModel.ProgressDefaultValue, (d,e) => ((DXSplashScreenService)d).OnProgressChanged()));
        public static readonly DependencyProperty MaxProgressProperty =
            DependencyProperty.Register("MaxProgress", typeof(double), typeof(DXSplashScreenService),
            new PropertyMetadata(SplashScreenViewModel.MaxProgressDefaultValue, (d,e) => ((DXSplashScreenService)d).OnMaxProgressChanged()));
        public static readonly DependencyProperty StateProperty =
            DependencyProperty.Register("State", typeof(object), typeof(DXSplashScreenService),
            new PropertyMetadata(SplashScreenViewModel.StateDefaultValue, (d,e) => ((DXSplashScreenService)d).OnStateChanged()));
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
        public double Progress {
            get { return (double)GetValue(ProgressProperty); }
            set { SetValue(ProgressProperty, value); }
        }
        public double MaxProgress {
            get { return (double)GetValue(MaxProgressProperty); }
            set { SetValue(MaxProgressProperty, value); }
        }
        public object State {
            get { return (object)GetValue(StateProperty); }
            set { SetValue(StateProperty, value); }
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
            if(ShowSplashScreenOnLoading && !AssociatedObject.IsLoaded && !DXSplashScreen.IsActive) {
                AssociatedObject.Loaded += OnAssociatedObjectLoaded;
                SplashScreenIsShownOnLoading = true;
                ((ISplashScreenService)this).ShowSplashScreen();
            }
        }
        protected override void OnDetaching() {
            HideSplashScreenOnAssociatedObjectLoaded();
            base.OnDetaching();
        }
        void OnAssociatedObjectLoaded(object sender, RoutedEventArgs e) {
            HideSplashScreenOnAssociatedObjectLoaded();
        }
        void HideSplashScreenOnAssociatedObjectLoaded() {
            if(SplashScreenIsShownOnLoading) {
                AssociatedObject.Loaded -= OnAssociatedObjectLoaded;
                SplashScreenIsShownOnLoading = false;
                ((ISplashScreenService)this).HideSplashScreen();
            }
        }

        void OnProgressChanged() {
            if(isSplashScreenShown)
                ((ISplashScreenService)this).SetSplashScreenProgress(Progress, MaxProgress);
        }
        void OnMaxProgressChanged() {
            if(isSplashScreenShown)
                ((ISplashScreenService)this).SetSplashScreenProgress(Progress, MaxProgress);
        }
        void OnStateChanged() {
            if(isSplashScreenShown)
                ((ISplashScreenService)this).SetSplashScreenState(State);
        }

        bool isSplashScreenShown = false;
        bool ISplashScreenService.IsSplashScreenActive {
            get { return DXSplashScreen.IsActive; }
        }
        void ISplashScreenService.ShowSplashScreen(string documentType) {
            if(SplashScreenType != null && (!string.IsNullOrEmpty(documentType) || ViewTemplate != null || ViewLocator != null))
                throw new InvalidOperationException(DXSplashScreenExceptions.ServiceException1);
            if(DXSplashScreen.IsActive) return;

            if(SplashScreenType != null)
                DXSplashScreen.Show(SplashScreenType);
            else
                DXSplashScreen.Show(CreateSplashScreenWindow, CreateSplashScreen,
                    new object[] { SplashScreenWindowStyle, SplashScreenStartupLocation },
                    new object[] { documentType, ViewLocator, ViewTemplate });
            isSplashScreenShown = DXSplashScreen.IsActive;
            if(Math.Abs(Progress - SplashScreenViewModel.ProgressDefaultValue) > 0.0001)
                OnProgressChanged();
            if(Math.Abs(MaxProgress - SplashScreenViewModel.MaxProgressDefaultValue) > 0.0001)
                OnMaxProgressChanged();
            if(!object.Equals(State, SplashScreenViewModel.StateDefaultValue))
                OnStateChanged();
        }
        void ISplashScreenService.HideSplashScreen() {
            if(!DXSplashScreen.IsActive) return;
            DXSplashScreen.Close();
            if(AssociatedObject != null) {
                Window ownerWindow = Window.GetWindow(AssociatedObject);
                if(ownerWindow != null && !ownerWindow.IsActive)
                    ownerWindow.Activate();
            }
            isSplashScreenShown = DXSplashScreen.IsActive;
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
            var SplashScreenViewModel = new SplashScreenViewModel();
            return ViewHelper.CreateAndInitializeView(viewLocator, documentType, SplashScreenViewModel, null, null, viewTemplate, null);
        }
    }
}