using DevExpress.Mvvm.Native;
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.ComponentModel;
using DevExpress.Mvvm.UI.Native;

namespace DevExpress.Mvvm.UI {
    public abstract class DocumentUIServiceBase : ViewServiceBase {
        public static readonly DependencyProperty DocumentProperty =
            DependencyProperty.RegisterAttached("Document", typeof(IDocument), typeof(DocumentUIServiceBase), new PropertyMetadata(null));
        public static IDocument GetDocument(DependencyObject obj) {
            return (IDocument)obj.GetValue(DocumentProperty);
        }
        public static void SetDocument(DependencyObject obj, IDocument value) {
            obj.SetValue(DocumentProperty, value);
        }
        public static void SetTitleBinding(object documentContentView, DependencyProperty property, FrameworkElement target, bool convertToString = false) {
            IDocumentViewModel documentViewModel = ViewHelper.GetViewModelFromView(documentContentView) as IDocumentViewModel;
            if(documentViewModel == null) return;
            if(ExpressionHelper.PropertyHasImplicitImplementation(documentViewModel, i => i.Title)) {
                Binding binding = new Binding("Title") { Source = documentViewModel };
#if !SILVERLIGHT
                if(convertToString)
                    binding.Converter = new ObjectToStringConverter();
#endif
                target.SetBinding(property, binding);
            } else {
                new TitleUpdater(convertToString, documentViewModel, target, property).Update(target, documentViewModel);
            }
        }
        class TitleUpdater : IDisposable {
            #region
            static readonly DependencyProperty TitleUpdaterInternalProperty =
                DependencyProperty.RegisterAttached("TitleUpdaterInternal", typeof(TitleUpdater), typeof(TitleUpdater), new PropertyMetadata(null));
            #endregion
            bool convertToString;
            DependencyProperty targetProperty;
            PropertyChangedWeakEventHandler<DependencyObject> updateHandler;
            WeakReference documentViewModelRef;

            public TitleUpdater(bool convertToString, IDocumentViewModel documentViewModel, DependencyObject target, DependencyProperty targetProperty) {
                TitleUpdater oldUpdater = (TitleUpdater)target.GetValue(TitleUpdaterInternalProperty);
                if(oldUpdater != null)
                    oldUpdater.Dispose();
                this.convertToString = convertToString;
                target.SetValue(TitleUpdaterInternalProperty, this);
                this.targetProperty = targetProperty;
                this.updateHandler = new PropertyChangedWeakEventHandler<DependencyObject>(target, OnDocumentViewModelPropertyChanged);
                INotifyPropertyChanged notify = documentViewModel as INotifyPropertyChanged;
                DocumentViewModel = notify;
                if(notify != null)
                    notify.PropertyChanged += this.updateHandler.Handler;
            }
            public void Dispose() {
                INotifyPropertyChanged notify = DocumentViewModel;
                if(notify != null)
                    notify.PropertyChanged -= this.updateHandler.Handler;
                this.updateHandler = null;
                DocumentViewModel = null;
            }
            public void Update(DependencyObject target, IDocumentViewModel documentViewModel) {
                object title = documentViewModel.Title;
#if !SILVERLIGHT
                if(convertToString)
                    title = title == null ? string.Empty : title.ToString();
#endif
                target.SetValue(targetProperty, title);
            }
            INotifyPropertyChanged DocumentViewModel {
                get { return documentViewModelRef == null ? null : (INotifyPropertyChanged)documentViewModelRef.Target; }
                set { documentViewModelRef = value == null ? null : new WeakReference(value); }
            }
            static void OnDocumentViewModelPropertyChanged(DependencyObject target, object sender, PropertyChangedEventArgs e) {
                if(e.PropertyName != "Title") return;
                TitleUpdater updater = (TitleUpdater)target.GetValue(TitleUpdaterInternalProperty);
                updater.Update(target, (IDocumentViewModel)sender);
            }
        }
#if !SILVERLIGHT
        class ObjectToStringConverter : IValueConverter {
            public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
                return value == null ? string.Empty : value.ToString();
            }
            public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
                throw new NotSupportedException();
            }
        }
#endif
    }
}