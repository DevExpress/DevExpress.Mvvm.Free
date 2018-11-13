using System;
using System.Windows.Controls;
using System.Windows;

#if !FREE
using DevExpress.Xpf.Utils.Themes;
using DevExpress.Xpf.Editors;
using DevExpress.Data.Utils;

namespace DevExpress.Xpf.Core {
#else

namespace DevExpress.Mvvm.UI {
#endif
    public class WaitIndicator : ContentControl {
#if DEBUGTEST && !FREE //DEMO_REMOVE
        [FxCopSpellCheckingIgnore] //DEMO_REMOVE
#endif //DEMO_REMOVE
        public static readonly DependencyProperty DeferedVisibilityProperty;
        public static readonly DependencyProperty ActualContentProperty;
        public static readonly DependencyProperty ShowShadowProperty;
        internal static readonly DependencyPropertyKey ActualContentPropertyKey;
        public static readonly DependencyProperty ContentPaddingProperty;

        static WaitIndicator() {
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
#if !FREE
            this.SetDefaultStyleKey(typeof(WaitIndicator));
#else
            DefaultStyleKey = typeof(WaitIndicator);
#endif
        }
        protected override void OnContentChanged(object oldContent, object newContent) {
            base.OnContentChanged(oldContent, newContent);
            ChangeContentIfNeed(newContent);
        }
        void ChangeContentIfNeed(object newContent) {
#if !FREE
            ActualContent = newContent ?? EditorLocalizer.Active.GetLocalizedString(EditorStringId.WaitIndicatorText);
#else
            ActualContent = newContent ?? "Loading...";
#endif
        }

        void OnDeferedVisibilityChanged() {
            if(DeferedVisibility)
                VisualStateManager.GoToState(this, "Visible", true);
            else
                VisualStateManager.GoToState(this, "Collapsed", true);
        }

#if DEBUGTEST && !FREE //DEMO_REMOVE
        [FxCopSpellCheckingIgnore] //DEMO_REMOVE
#endif //DEMO_REMOVE
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
#if FREE
            DefaultStyleKey = typeof(WaitIndicatorContainer);
        }
    }
#else
            this.SetDefaultStyleKey(typeof(WaitIndicatorContainer));
        }
    }

    public class ColumnWaitIndicator : ProgressBar {
        public ColumnWaitIndicator() {
            this.SetDefaultStyleKey(typeof(ColumnWaitIndicator));
        }
    }

    public enum WaitIndicatorThemeKeys {
        WaitIndicatorTemplate,
        WaitIndicatorContentTemplate,
        WaitIndicatorContainerTemplate
    }

    public class WaitIndicatorThemeKeyExtension : ThemeKeyExtensionBase<WaitIndicatorThemeKeys> {
    }
#endif
}