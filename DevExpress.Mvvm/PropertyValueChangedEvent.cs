using System;
using System.ComponentModel;

namespace DevExpress.Mvvm {
    public delegate void PropertyValueChangedEventHandler<T>(object sender, PropertyValueChangedEventArgs<T> e);
    public class PropertyValueChangedEventArgs<T> : EventArgs {
        public T NewValue { get; private set; }
        public T OldValue { get; private set; }
        public PropertyValueChangedEventArgs(T oldValue, T newValue) {
            NewValue = newValue;
            OldValue = oldValue;
        }
    }

    public delegate void PropertyValueChangingEventHandler<T>(object sender, PropertyValueChangingEventArgs<T> e);
    public class PropertyValueChangingEventArgs<T> : CancelEventArgs {
        public T NewValue { get; private set; }
        public T OldValue { get; private set; }
        public PropertyValueChangingEventArgs(T oldValue, T newValue) {
            NewValue = newValue;
            OldValue = oldValue;
        }
    }
}