using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows.Input;

namespace DevExpress.Mvvm.Native {
    public class EventFireCounter<TObject, TEventArgs> where TEventArgs : EventArgs {
        readonly Action<EventHandler> unsubscribe;
        protected readonly EventHandler handler;
        public int FireCount { get; private set; }
        public TEventArgs LastArgs { get; private set; }
        public EventFireCounter(Action<EventHandler> subscribe, Action<EventHandler> unsubscribe) {
            this.unsubscribe = unsubscribe;
            handler = new EventHandler(OnEvent);
            subscribe(OnEvent);
        }
        void OnEvent(object source, EventArgs eventArgs) {
            FireCount++;
            LastArgs = (TEventArgs)eventArgs;
        }
        public void Unsubscribe() {
            unsubscribe(OnEvent);
        }
    }
    public class CanExecuteChangedCounter : EventFireCounter<ICommand, EventArgs> {
        public CanExecuteChangedCounter(ICommand command)
            : base(h => command.CanExecuteChanged += h, h => command.CanExecuteChanged -= h) {
        }
    }
    public class CollectionChangedCounter : EventFireCounter<INotifyCollectionChanged, NotifyCollectionChangedEventArgs> {
        public CollectionChangedCounter(INotifyCollectionChanged collection)
            : base(h => collection.CollectionChanged += new NotifyCollectionChangedEventHandler((o, e) => { h(o, e); }), null) {
        }
    }
    public class PropertyChangedCounter : EventFireCounter<INotifyPropertyChanged, PropertyChangedEventArgs> {
        public PropertyChangedCounter(INotifyPropertyChanged obj, string propertyName)
            : base(h => obj.PropertyChanged += new PropertyChangedEventHandler((o, e) => { if(e.PropertyName == propertyName) h(o, e); }), null) {
        }
    }

}