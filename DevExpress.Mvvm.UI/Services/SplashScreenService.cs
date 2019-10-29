using DevExpress.Mvvm.UI.Interactivity;
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Linq;
using System.Collections.Generic;
using DevExpress.Mvvm.Native;
using DevExpress.Mvvm.UI.Native;
using System.Windows.Threading;

namespace DevExpress.Mvvm.UI {
    [TargetTypeAttribute(typeof(UserControl))]
    [TargetTypeAttribute(typeof(Window))]
    public class DXSplashScreenService : ViewServiceBase, ISplashScreenService, DXSplashScreen.ISplashScreenStateAware {
        public static readonly DependencyProperty SplashScreenTypeProperty;
        public static readonly DependencyProperty SplashScreenWindowStyleProperty;
        public static readonly DependencyProperty SplashScreenStartupLocationProperty;
        public static readonly DependencyProperty ShowSplashScreenOnLoadingProperty;
        public static readonly DependencyProperty ProgressProperty;
        public static readonly DependencyProperty MaxProgressProperty;
        public static readonly DependencyProperty StateProperty;
        public static readonly DependencyProperty SplashScreenOwnerProperty;
        public static readonly DependencyProperty SplashScreenClosingModeProperty;
        public static readonly DependencyProperty OwnerSearchModeProperty;
        public static readonly DependencyProperty FadeInDurationProperty;
        public static readonly DependencyProperty FadeOutDurationProperty;
        public static readonly DependencyProperty UseIndependentWindowProperty;
        public static readonly DependencyProperty IsSplashScreenActiveProperty;
        static readonly DependencyPropertyKey IsSplashScreenActivePropertyKey;

        static DXSplashScreenService() {
            DependencyPropertyRegistrator<DXSplashScreenService>.New()
                .Register(d => d.SplashScreenType, out SplashScreenTypeProperty, null)
                .Register(d => d.SplashScreenWindowStyle, out SplashScreenWindowStyleProperty, null,
                    (d, e) => d.OnSplashScreenWindowStyleChanged((Style)e.OldValue, (Style)e.NewValue))
                .Register(d => d.SplashScreenStartupLocation, out SplashScreenStartupLocationProperty, WindowStartupLocation.CenterScreen)
                .Register(d => d.ShowSplashScreenOnLoading, out ShowSplashScreenOnLoadingProperty, false)
                .Register(d => d.Progress, out ProgressProperty, SplashScreenViewModel.ProgressDefaultValue, d => d.OnProgressChanged())
                .Register(d => d.MaxProgress, out MaxProgressProperty, SplashScreenViewModel.MaxProgressDefaultValue, d => d.OnMaxProgressChanged())
                .Register(d => d.State, out StateProperty, SplashScreenViewModel.StateDefaultValue, d => d.OnStateChanged())
                .Register(d => d.SplashScreenOwner, out SplashScreenOwnerProperty, null)
                .Register(d => d.SplashScreenClosingMode, out SplashScreenClosingModeProperty, SplashScreenClosingMode.Default)
                .Register(d => d.OwnerSearchMode, out OwnerSearchModeProperty, SplashScreenOwnerSearchMode.Full)
                .Register(d => d.FadeInDuration, out FadeInDurationProperty, TimeSpan.FromSeconds(0.2))
                .Register(d => d.FadeOutDuration, out FadeOutDurationProperty, TimeSpan.FromSeconds(0.2))
                .Register(d => d.UseIndependentWindow, out UseIndependentWindowProperty, false, d => d.OnUseIndependentWindowChanged())
                .RegisterReadOnly(d => d.IsSplashScreenActive, out IsSplashScreenActivePropertyKey, out IsSplashScreenActiveProperty, false)
                ;
        }

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
        public FrameworkElement SplashScreenOwner {
            get { return (FrameworkElement)GetValue(SplashScreenOwnerProperty); }
            set { SetValue(SplashScreenOwnerProperty, value); }
        }
        public SplashScreenClosingMode SplashScreenClosingMode {
            get { return (SplashScreenClosingMode)GetValue(SplashScreenClosingModeProperty); }
            set { SetValue(SplashScreenClosingModeProperty, value); }
        }
        public SplashScreenOwnerSearchMode OwnerSearchMode {
            get { return (SplashScreenOwnerSearchMode)GetValue(OwnerSearchModeProperty); }
            set { SetValue(OwnerSearchModeProperty, value); }
        }
        public TimeSpan FadeInDuration {
            get { return (TimeSpan)GetValue(FadeInDurationProperty); }
            set { SetValue(FadeInDurationProperty, value); }
        }
        public TimeSpan FadeOutDuration {
            get { return (TimeSpan)GetValue(FadeOutDurationProperty); }
            set { SetValue(FadeOutDurationProperty, value); }
        }
        public bool UseIndependentWindow {
            get { return (bool)GetValue(UseIndependentWindowProperty); }
            set { SetValue(UseIndependentWindowProperty, value); }
        }
        public bool IsSplashScreenActive {
            get { return (bool)GetValue(IsSplashScreenActiveProperty); }
            private set { SetValue(IsSplashScreenActivePropertyKey, value); }
        }

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public new DataTemplateSelector ViewTemplateSelector { get { return null; } set { OnViewTemplateSelectorChanged(null, null); } }
        DependencyObject Owner {
            get {
                if(SplashScreenOwner != null)
                    return SplashScreenOwner;

                if(OwnerSearchMode == SplashScreenOwnerSearchMode.Full)
                    return AssociatedObject ?? SplashScreenHelper.GetApplicationActiveWindow(true);
                else if(OwnerSearchMode == SplashScreenOwnerSearchMode.IgnoreAssociatedObject)
                    return SplashScreenHelper.GetApplicationActiveWindow(true);

                return null;
            }
        }
        bool ISplashScreenService.IsSplashScreenActive { get { return IsSplashScreenActive; } }
        bool SplashScreenIsShownOnLoading = false;
        bool isSplashScreenShown = false;
        DXSplashScreen.SplashScreenContainer splashContainer = null;

        public DXSplashScreenService() {
            OnUseIndependentWindowChanged();
        }

        internal DXSplashScreen.SplashScreenContainer GetSplashContainer(bool ensureInstance) {
            if(splashContainer == null && ensureInstance)
                splashContainer = new DXSplashScreen.SplashScreenContainer();

            return splashContainer;
        }

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

        protected override void OnAttached() {
            base.OnAttached();
            if(ShowSplashScreenOnLoading && !AssociatedObject.IsLoaded && !IsSplashScreenActive) {
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

        void OnUseIndependentWindowChanged() {
            if(isSplashScreenShown)
                throw new InvalidOperationException("The property value cannot be changed while the DXSplashScreenService is active.");

            if(!UseIndependentWindow)
                DXSplashScreen.WeakSplashScreenStateAwareContainer.Default.Register(this);
            else
                DXSplashScreen.WeakSplashScreenStateAwareContainer.Default.Unregister(this);

            UpdateIsSplashScreenActive(null);
        }
        void DXSplashScreen.ISplashScreenStateAware.OnIsActiveChanged(bool newValue) {
            UpdateIsSplashScreenActive(newValue);
        }
        void UpdateIsSplashScreenActive(bool? newIsActive) {
            if (Dispatcher == null)
                return;
            if (Dispatcher.CheckAccess()) {
                bool isActive = newIsActive.HasValue ? newIsActive.Value : (UseIndependentWindow ? (splashContainer != null && splashContainer.IsActive) : DXSplashScreen.IsActive);
                IsSplashScreenActive = isActive;
                if(!isActive)
                    isSplashScreenShown = false;
            } else
                Dispatcher.BeginInvoke(DispatcherPriority.Send, new Action<bool?>(UpdateIsSplashScreenActive), newIsActive);
        }

        void ISplashScreenService.ShowSplashScreen(string documentType) {
            if(SplashScreenType != null && (!string.IsNullOrEmpty(documentType) || ViewTemplate != null || ViewLocator != null))
                throw new InvalidOperationException(DXSplashScreenExceptions.ServiceException1);
            if(IsSplashScreenActive)
                return;

            Func<object, object> contentCreator = null;
            object contentCreatorParams = null;
            var ssModel = CreateSplashScreenViewModel();
            IList<object> windowCreatorParams = new List<object>() { SplashScreenWindowStyle, SplashScreenStartupLocation, Owner.With(x => new SplashScreenOwner(x)),
                SplashScreenClosingMode, FadeInDuration, FadeOutDuration };
            if(SplashScreenType == null) {
                contentCreator = CreateSplashScreen;
                contentCreatorParams = new object[] { documentType, ViewLocator, ViewTemplate, ssModel };
            } else {
                DXSplashScreen.CheckSplashScreenType(SplashScreenType);
                if(typeof(Window).IsAssignableFrom(SplashScreenType))
                    windowCreatorParams.Add(SplashScreenType);
                else if(typeof(FrameworkElement).IsAssignableFrom(SplashScreenType)) {
                    contentCreator = DXSplashScreen.DefaultSplashScreenContentCreator;
                    contentCreatorParams = new object[] { SplashScreenType, ssModel };
                }
            }
            isSplashScreenShown = true;
            if(UseIndependentWindow) {
                var container = GetSplashContainer(true);
                container.Closed += OnSplashScreenClosed;
                container.Show(CreateSplashScreenWindow, contentCreator, windowCreatorParams.ToArray(), contentCreatorParams, null);
                IsSplashScreenActive = true;
            } else {
                DXSplashScreen.Show(CreateSplashScreenWindow, contentCreator, windowCreatorParams.ToArray(), contentCreatorParams);
                isSplashScreenShown = DXSplashScreen.IsActive;
            }
        }

        void OnSplashScreenClosed(object sender, EventArgs e) {
            var container = (DXSplashScreen.SplashScreenContainer)sender;
            container.Closed -= OnSplashScreenClosed;
            isSplashScreenShown = false;
            UpdateIsSplashScreenActive(null);
        }

        SplashScreenViewModel CreateSplashScreenViewModel() {
            var result = new SplashScreenViewModel() { State = State };
            if(Math.Abs(Progress - SplashScreenViewModel.ProgressDefaultValue) > 0.0001)
                result.Progress = Progress;
            if(Math.Abs(MaxProgress - SplashScreenViewModel.MaxProgressDefaultValue) > 0.0001)
                result.MaxProgress = MaxProgress;

            return result;
        }
        void ISplashScreenService.HideSplashScreen() {
            if(!IsSplashScreenActive) {
                isSplashScreenShown = false;
                return;
            }
            if(UseIndependentWindow && splashContainer.IsActive) {
                var container = GetSplashContainer(true);
                container.Closed -= OnSplashScreenClosed;
                if(container.IsActive)
                    container.Close();
                isSplashScreenShown = false;
                IsSplashScreenActive = false;
            } else {
                if(DXSplashScreen.IsActive)
                    DXSplashScreen.Close();
                isSplashScreenShown = false;
            }
        }
        void ISplashScreenService.SetSplashScreenProgress(double progress, double maxProgress) {
            if(!IsSplashScreenActive) return;
            if(UseIndependentWindow)
                GetSplashContainer(false).Progress(progress, maxProgress);
            else
                DXSplashScreen.Progress(progress, maxProgress);
        }
        void ISplashScreenService.SetSplashScreenState(object state) {
            if(!IsSplashScreenActive) return;
            if(UseIndependentWindow)
                GetSplashContainer(false).SetState(state);
            else
                DXSplashScreen.SetState(state);
        }

        static Window CreateSplashScreenWindow(object parameter) {
            Type windowType = SplashScreenHelper.FindParameter<Type>(parameter);
            Style windowStyle = SplashScreenHelper.FindParameter<Style>(parameter);
            IList<TimeSpan> fadeDurations = SplashScreenHelper.FindParameters<TimeSpan>(parameter);
            Window res;
            if(windowType != null)
                res = (Window)Activator.CreateInstance(windowType);
            else if(windowStyle != null)
                res = new DXSplashScreen.SplashScreenWindow();
            else
                res = DXSplashScreen.DefaultSplashScreenWindowCreator(parameter);

            res.WindowStartupLocation = SplashScreenHelper.FindParameter<WindowStartupLocation>(parameter, WindowStartupLocation.CenterScreen);
            if(windowStyle != null)
                res.Style = windowStyle;
            if(fadeDurations.Any(x => x.TotalMilliseconds > 0) && !Interaction.GetBehaviors(res).Any(x => x is WindowFadeAnimationBehavior))
                Interaction.GetBehaviors(res).Add(new WindowFadeAnimationBehavior() { FadeInDuration = fadeDurations[0], FadeOutDuration = fadeDurations[1] });

            return res;
        }
        static object CreateSplashScreen(object parameter) {
            object[] parameters = (object[])parameter;
            string documentType = parameters[0] as string;
            IViewLocator viewLocator = parameters[1] as IViewLocator;
            DataTemplate viewTemplate = parameters[2] as DataTemplate;
            var model = SplashScreenHelper.FindParameter<SplashScreenViewModel>(parameter);
            model = model == null ? new SplashScreenViewModel() : model.Clone();
            return ViewHelper.CreateAndInitializeView(viewLocator, documentType, model, null, null, viewTemplate, null);
        }
    }
    public enum SplashScreenOwnerSearchMode {
        Full,
        IgnoreAssociatedObject,
        OwnerOnly
    }
}