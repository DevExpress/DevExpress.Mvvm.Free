using System.Windows;
namespace DevExpress.Mvvm.UI {
    public interface IEventArgsConverter {
        object Convert(object sender, object args);
    }
    public abstract class EventArgsConverterBase<TArgs> : DependencyObject, IEventArgsConverter {
        object IEventArgsConverter.Convert(object sender, object args) {
            return (args is TArgs) ? Convert(sender, (TArgs)args) : null;
        }
        protected abstract object Convert(object sender, TArgs args);
    }
}