using System;
using System.Windows.Markup;
namespace DevExpress.Mvvm.UI {
    public interface IEventArgsConverter {
        object Convert(object sender, object args);
    }
    public interface IEventArgsTwoWayConverter : IEventArgsConverter {
        void ConvertBack(object sender, object args, object parameter);
    }

    public abstract class EventArgsConverterBase<TArgs> : MarkupExtension, IEventArgsTwoWayConverter {
        object IEventArgsConverter.Convert(object sender, object args) {
            return (args is TArgs) ? Convert(sender, (TArgs)args) : null;
        }
        void IEventArgsTwoWayConverter.ConvertBack(object sender, object args, object parameter) {
            if(args is TArgs)
                ConvertBack(sender, (TArgs)args, parameter);
        }
        public override object ProvideValue(IServiceProvider serviceProvider) {
            return this;
        }
        protected abstract object Convert(object sender, TArgs args);
        protected virtual void ConvertBack(object sender, TArgs args, object parameter) { }
    }
}