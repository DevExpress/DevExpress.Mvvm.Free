using System;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Media.Animation;


namespace DevExpress.Mvvm.UI {
    public class WaitIndicator : ContentControl {
        public static int? DesiredFrameRate { get; set; }

        public static readonly DependencyProperty DeferedVisibilityProperty;
        public static readonly DependencyProperty ActualContentProperty;
        public static readonly DependencyProperty ShowShadowProperty;
        internal static readonly DependencyPropertyKey ActualContentPropertyKey;
        public static readonly DependencyProperty ContentPaddingProperty;

        static WaitIndicator() {
            DesiredFrameRate = (int?)Timeline.DesiredFrameRateProperty.DefaultMetadata.DefaultValue;
            Type ownerType = typeof(WaitIndicator);
            DeferedVisibilityProperty = DependencyProperty.Register("DeferedVisibility", typeof(bool), ownerType, new PropertyMetadata(false, OnDeferedVisibilityPropertyChanged));
            ShowShadowProperty = DependencyProperty.Register("ShowShadow", typeof(bool), ownerType, new PropertyMetadata(true));
            ContentPaddingProperty = DependencyProperty.Register("ContentPadding", typeof(Thickness), ownerType, new PropertyMetadata(new Thickness()));
            ActualContentPropertyKey = DependencyProperty.RegisterReadOnly("ActualContent", typeof(object), ownerType, new FrameworkPropertyMetadata(null));
            ActualContentProperty = ActualContentPropertyKey.DependencyProperty;
        }

        public override void OnApplyTemplate() {
            base.OnApplyTemplate();
            OnDeferedVisibilityChanged();
            ChangeContentIfNeed(Content);
        }

        static void OnDeferedVisibilityPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            ((WaitIndicator)d).OnDeferedVisibilityChanged();
        }

        public WaitIndicator() {
            DefaultStyleKey = typeof(WaitIndicator);
        }
        protected override void OnContentChanged(object oldContent, object newContent) {
            base.OnContentChanged(oldContent, newContent);
            ChangeContentIfNeed(newContent);
        }
        void ChangeContentIfNeed(object newContent) {
            ActualContent = newContent ?? "Loading...";
        }

        void OnDeferedVisibilityChanged() {
            if(DeferedVisibility)
                VisualStateManager.GoToState(this, "Visible", true);
            else
                VisualStateManager.GoToState(this, "Collapsed", true);
        }

        public bool DeferedVisibility {
            get { return (bool)GetValue(DeferedVisibilityProperty); }
            set { SetValue(DeferedVisibilityProperty, value); }
        }
        public object ActualContent {
            get { return (object)GetValue(ActualContentProperty); }
            internal set { this.SetValue(ActualContentPropertyKey, value); }
        }
        public bool ShowShadow {
            get { return (bool)GetValue(ShowShadowProperty); }
            set { SetValue(ShowShadowProperty, value); }
        }
        public Thickness ContentPadding {
            get { return (Thickness)GetValue(ContentPaddingProperty); }
            set { SetValue(ContentPaddingProperty, value); }
        }
    }

    public class WaitIndicatorContainer : ContentControl {
        public WaitIndicatorContainer() {
            DefaultStyleKey = typeof(WaitIndicatorContainer);
        }
    }
}