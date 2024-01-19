using DevExpress.Mvvm.UI.Interactivity;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;

namespace DevExpress.Mvvm.UI {
    [TargetType(typeof(FrameworkElement))]
    public class ValidationErrorsHostBehavior : Behavior<FrameworkElement> {
        #region Dependency Properties
        public static readonly DependencyProperty ErrorsProperty =
            DependencyProperty.Register("Errors", typeof(IList<ValidationError>), typeof(ValidationErrorsHostBehavior), new PropertyMetadata(null,
                (d, e) => ((ValidationErrorsHostBehavior)d).OnErrorsChanged(e),
                (d, v) => v ?? new ObservableCollection<ValidationError>()));
        public static readonly DependencyProperty HasErrorsProperty =
            DependencyProperty.Register("HasErrors", typeof(bool), typeof(ValidationErrorsHostBehavior), new PropertyMetadata(false));
        #endregion

        public ValidationErrorsHostBehavior() {
            Errors = new ObservableCollection<ValidationError>();
        }
        public IList<ValidationError> Errors {
            get { return (IList<ValidationError>)GetValue(ErrorsProperty); }
            set { SetValue(ErrorsProperty, value); }
        }
        public bool HasErrors {
            get { return (bool)GetValue(HasErrorsProperty); }
            set { SetValue(HasErrorsProperty, value); }
        }
        protected override void OnAttached() {
            base.OnAttached();
            Validation.AddErrorHandler(AssociatedObject, OnAssociatedObjectError);
        }
        protected override void OnDetaching() {
            base.OnDetaching();
            Validation.RemoveErrorHandler(AssociatedObject, OnAssociatedObjectError);
        }
        void OnAssociatedObjectError(object sender, ValidationErrorEventArgs e) {
            if(e.Action == ValidationErrorEventAction.Added)
                Errors.Add(e.Error);
            else
                Errors.Remove(e.Error);
            e.Handled = true;
            UpdateHasErrors();
        }
        protected virtual void OnErrorsChanged(DependencyPropertyChangedEventArgs e) {
            INotifyCollectionChanged oldValue = e.OldValue as INotifyCollectionChanged;
            INotifyCollectionChanged newValue = e.NewValue as INotifyCollectionChanged;
            if(oldValue != null)
                oldValue.CollectionChanged -= OnErrorsCollectionChanged;
            if(newValue != null)
                newValue.CollectionChanged += OnErrorsCollectionChanged;
            UpdateHasErrors();
        }
        protected void UpdateHasErrors() {
            HasErrors = Errors.Count != 0;
        }
        void OnErrorsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e) {
            UpdateHasErrors();
        }
    }
}