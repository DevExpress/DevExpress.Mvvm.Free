using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq.Expressions;
using DevExpress.Mvvm.Native;

namespace DevExpress.Mvvm {
    [System.Runtime.Serialization.DataContract]
    public abstract class BindableBase : INotifyPropertyChanged {
        public static string GetPropertyName<T>(Expression<Func<T>> expression) {
            return GetPropertyNameFast(expression);
        }
        internal static string GetPropertyNameFast(LambdaExpression expression) {
            MemberExpression memberExpression = expression.Body as MemberExpression;
            if(memberExpression == null) {
                throw new ArgumentException("MemberExpression is expected in expression.Body", "expression");
            }
            const string vblocalPrefix = "$VB$Local_";
            var member = memberExpression.Member;
            if(
                member.MemberType == System.Reflection.MemberTypes.Field &&
                member.Name != null &&
                member.Name.StartsWith(vblocalPrefix))
                return member.Name.Substring(vblocalPrefix.Length);
            return member.Name;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected T GetProperty<T>(Expression<Func<T>> expression) {
            return PropertyManager.GetProperty<T>(GetPropertyName(expression));
        }
        protected bool SetProperty<T>(ref T storage, T value, string propertyName, Action changedCallback) {
            return PropertyManager.SetProperty<T>(ref storage, value, propertyName, RaisePropertyChanged, changedCallback);
        }
        protected bool SetProperty<T>(ref T storage, T value, Expression<Func<T>> expression, Action changedCallback) {
            return this.SetProperty(ref storage, value, GetPropertyName(expression), changedCallback);
        }
        protected bool SetProperty<T>(ref T storage, T value, Expression<Func<T>> expression) {
            return this.SetProperty<T>(ref storage, value, expression, null);
        }
        protected bool SetProperty<T>(ref T storage, T value, string propertyName) {
            return this.SetProperty<T>(ref storage, value, propertyName, null);
        }
        protected bool SetProperty<T>(Expression<Func<T>> expression, T value) {
            return this.SetProperty(expression, value, (Action)null);
        }
        protected bool SetProperty<T>(Expression<Func<T>> expression, T value, Action changedCallback) {
            string propertyName = GetPropertyName(expression);
            return PropertyManager.SetProperty<T>(propertyName, value, RaisePropertyChanged, changedCallback);
        }
        protected bool SetProperty<T>(Expression<Func<T>> expression, T value, Action<T> changedCallback) {
            string propertyName = GetPropertyName(expression);
            return PropertyManager.SetProperty<T>(propertyName, value, RaisePropertyChanged, changedCallback);
        }

        protected void RaisePropertyChanged(string propertyName) {
            PropertyChanged.Do(x => x(this, new PropertyChangedEventArgs(propertyName)));
        }
        protected void RaisePropertyChanged() {
            RaisePropertiesChanged(null);
        }
        protected void RaisePropertyChanged<T>(Expression<Func<T>> expression) {
            RaisePropertyChanged(GetPropertyName(expression));
        }
        protected void RaisePropertiesChanged(params string[] propertyNames) {
            if(propertyNames == null || propertyNames.Length == 0) {
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

        PropertyManager propertyManager;
        internal PropertyManager PropertyManager { get { return propertyManager ?? (propertyManager = new PropertyManager()); } }
    }
}
namespace DevExpress.Mvvm.Native {
    public class PropertyManager {
        internal Dictionary<string, object> propertyBag = new Dictionary<string, object>();
        public bool SetProperty<T>(string propertyName, T value, Action<string> raiseNotification, Action changedCallback) {
            T oldValue;
            var res = SetPropertyCore(propertyName, value, out oldValue);
            if(res) {
                raiseNotification(propertyName);
                changedCallback.Do(x => x());
            }
            return res;
        }
        public bool SetProperty<T>(string propertyName, T value, Action<string> raiseNotification, Action<T> changedCallback) {
            T oldValue;
            var res = SetPropertyCore(propertyName, value, out oldValue);
            if(res) {
                raiseNotification(propertyName);
                changedCallback.Do(x => x(oldValue));
            }
            return res;
        }
        bool SetPropertyCore<T>(string propertyName, T value, out T oldValue) {
            oldValue = default(T);
            object val;
            if(propertyBag.TryGetValue(propertyName, out val))
                oldValue = (T)val;
            if(CompareValues<T>(oldValue, value))
                return false;
            propertyBag[propertyName] = value;
            return true;
        }

        public T GetProperty<T>(string propertyName) {
            object val;
            if(propertyBag.TryGetValue(propertyName, out val))
                return (T)val;
            return default(T);
        }
        public bool SetProperty<T>(ref T storage, T value, string propertyName, Action<string> raiseNotification, Action changedCallback) {
            T oldValue = storage;
            var res = SetPropertyCore(ref storage, value, propertyName);
            if(res) {
                raiseNotification(propertyName);
                changedCallback.Do(x => x());
            }
            return res;
        }
        public bool SetProperty<T>(ref T storage, T value, string propertyName, Action<string> raiseNotification, Action<T> changedCallback) {
            T oldValue = storage;
            var res = SetPropertyCore(ref storage, value, propertyName);
            if(res) {
                raiseNotification(propertyName);
                changedCallback.Do(x => x(oldValue));
            }
            return res;
        }
        bool SetPropertyCore<T>(ref T storage, T value, string propertyName) {
            if(PropertyManager.CompareValues<T>(storage, value))
                return false;
            storage = value;
            return true;
        }

        static bool CompareValues<T>(T storage, T value) {
            return object.Equals(storage, value);
        }
    }
}