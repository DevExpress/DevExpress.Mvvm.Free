using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq.Expressions;

namespace DevExpress.Mvvm {
    public abstract class BindableBase : INotifyPropertyChanged {
        public static string GetPropertyName<T>(Expression<Func<T>> expression) {
            return GetPropertyNameFast(expression);
        }
        internal static string GetPropertyNameFast(LambdaExpression expression) {
            MemberExpression memberExpression = expression.Body as MemberExpression;
            if(memberExpression == null) {
                throw new ArgumentException("expression");
            }
            return memberExpression.Member.Name;
        }
        static bool CompareValues<T>(T storage, T value) {
            return object.Equals(storage, value);
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected bool SetProperty<T>(ref T storage, T value, string propertyName, Action changedCallback) {
            if(CompareValues<T>(storage, value))
                return false;
            T oldValue = storage;
            storage = value;
            CallChangedCallBackAndRaisePropertyChanged(propertyName, changedCallback);
            return true;
        }

        void CallChangedCallBackAndRaisePropertyChanged(string propertyName, Action changedCallback) {
            RaisePropertyChanged(propertyName);
            if(changedCallback != null)
                changedCallback();
        }

        protected bool SetProperty<T>(ref T storage, T value, string propertyName) {
            return SetProperty<T>(ref storage, value, propertyName, null);
        }
        protected void RaisePropertyChanged(string propertyName) {
            if(PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        protected void RaisePropertyChanged() {
            RaisePropertiesChanged(null);
        }
        protected void RaisePropertyChanged<T>(Expression<Func<T>> expression) {
            RaisePropertyChanged(GetPropertyName(expression));
        }
        protected void RaisePropertiesChanged(params string[] propertyNames) {
            if(propertyNames == null) {
                RaisePropertyChanged(string.Empty);
                return;
            }
            foreach(string propertyName in propertyNames) {
                RaisePropertyChanged(propertyName);
            }
        }
        protected void RaisePropertiesChanged<T1, T2>(Expression<Func<T1>> expression1, Expression<Func<T2>> expression2) {
            RaisePropertyChanged(expression1);
            RaisePropertyChanged(expression2);
        }
        protected void RaisePropertiesChanged<T1, T2, T3>(Expression<Func<T1>> expression1, Expression<Func<T2>> expression2, Expression<Func<T3>> expression3) {
            RaisePropertyChanged(expression1);
            RaisePropertyChanged(expression2);
            RaisePropertyChanged(expression3);
        }
        protected void RaisePropertiesChanged<T1, T2, T3, T4>(Expression<Func<T1>> expression1, Expression<Func<T2>> expression2, Expression<Func<T3>> expression3, Expression<Func<T4>> expression4) {
            RaisePropertyChanged(expression1);
            RaisePropertyChanged(expression2);
            RaisePropertyChanged(expression3);
            RaisePropertyChanged(expression4);
        }
        protected void RaisePropertiesChanged<T1, T2, T3, T4, T5>(Expression<Func<T1>> expression1, Expression<Func<T2>> expression2, Expression<Func<T3>> expression3, Expression<Func<T4>> expression4, Expression<Func<T5>> expression5) {
            RaisePropertyChanged(expression1);
            RaisePropertyChanged(expression2);
            RaisePropertyChanged(expression3);
            RaisePropertyChanged(expression4);
            RaisePropertyChanged(expression5);
        }
        protected bool SetProperty<T>(ref T storage, T value, Expression<Func<T>> expression, Action changedCallback) {
            string propertyName = GetPropertyName(expression);
            return SetProperty(ref storage, value, propertyName, changedCallback);
        }
        protected bool SetProperty<T>(ref T storage, T value, Expression<Func<T>> expression) {
            return SetProperty<T>(ref storage, value, expression, null);
        }

        Dictionary<string, object> propertyBag;
        internal Dictionary<string, object> PropertyBag { get { return propertyBag ?? (propertyBag = new Dictionary<string, object>()); } }
        protected bool SetProperty<T>(Expression<Func<T>> expression, T value) {
            return SetProperty(expression, value, null);
        }
        protected bool SetProperty<T>(Expression<Func<T>> expression, T value, Action changedCallback) {
            string propertyName = GetPropertyName(expression);
            T currentValue = default(T);
            object val;
            if(PropertyBag.TryGetValue(propertyName, out val))
                currentValue = (T)val;
            if(CompareValues<T>(currentValue, value))
                return false;
            PropertyBag[propertyName] = value;
            CallChangedCallBackAndRaisePropertyChanged(propertyName, changedCallback);
            return true;
        }
        protected T GetProperty<T>(Expression<Func<T>> expression) {
            string propertyName = GetPropertyName(expression);
            object val;
            if(PropertyBag.TryGetValue(propertyName, out val))
                return (T)val;
            return default(T);
        }
    }
}