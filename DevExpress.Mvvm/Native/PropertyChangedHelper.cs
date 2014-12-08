using System;
using System.ComponentModel;
using System.Linq.Expressions;
#if !FREE && !NETFX_CORE
using System.Windows.Threading;
#endif

namespace DevExpress.Mvvm.Native {
    public class PropertyChangedHelper {
        event PropertyChangedEventHandler PropertyChanged;
        public void AddHandler(PropertyChangedEventHandler handler) {
            PropertyChanged += handler;
        }
        public void RemoveHandler(PropertyChangedEventHandler handler) {
            PropertyChanged -= handler;
        }
        public void OnPropertyChanged(INotifyPropertyChanged obj, string propertyName) {
            var handler = PropertyChanged;
            if(handler != null)
                handler(obj, new PropertyChangedEventArgs(propertyName));
        }
    }
#if !FREE && !NETFX_CORE
    public static class PropertyChangedTracker {
        public static PropertyChangedTracker<T, TProperty> GetPropertyChangedTracker<T, TProperty>(this T obj, Expression<Func<T, TProperty>> propertyExpression, Action changedCallBack)
            where T : class {
            return new PropertyChangedTracker<T, TProperty>(obj, propertyExpression, changedCallBack);
        }
    }
    public class PropertyChangedTracker<T, TProperty> where T : class {
        readonly T obj;
        readonly Func<T, TProperty> propertyAccessor;
        readonly Dispatcher dispatcher;
        readonly string propertyName;
        readonly Action changedCallBack;
        public PropertyChangedTracker(T obj, Expression<Func<T, TProperty>> propertyExpression, Action changedCallBack) {
            this.obj = obj;
            this.propertyName = BindableBase.GetPropertyNameFast(propertyExpression);
            this.propertyAccessor = propertyExpression.Compile();
            this.dispatcher = Dispatcher.CurrentDispatcher;
            this.changedCallBack = changedCallBack;
            ((INotifyPropertyChanged)obj).PropertyChanged += OnPropertyChanged;
            UpdateValue();
        }
        public TProperty Value { get; private set; }
        public int ChangeCount { get; private set; }

        void OnPropertyChanged(object sender, PropertyChangedEventArgs e) {
            if(propertyName != e.PropertyName)
                return;
            dispatcher.VerifyAccess();
            ChangeCount++;
            UpdateValue();
            changedCallBack();
        }
        void UpdateValue() {
            Value = propertyAccessor(obj);
        }
    }
#endif
}