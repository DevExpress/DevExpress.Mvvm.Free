using System.ComponentModel;
using System.Windows;
using System.Windows.Input;

namespace DevExpress.Mvvm.UI {
    public class KeyToCommand : EventToCommandBase {
        public static readonly DependencyProperty KeyGestureProperty =
            DependencyProperty.Register("KeyGesture", typeof(KeyGesture), typeof(KeyToCommand),
            new PropertyMetadata(null));
        public KeyGesture KeyGesture {
            get { return (KeyGesture)GetValue(KeyGestureProperty); }
            set { SetValue(KeyGestureProperty, value); }
        }
        public KeyToCommand() {
            EventName = "KeyUp";
        }
        protected override void Invoke(object sender, object eventArgs) {
            if(Command.CanExecute(CommandParameter))
                Command.Execute(CommandParameter);
        }
        protected override bool CanInvoke(object sender, object eventArgs) {
            bool res = base.CanInvoke(sender, eventArgs);
#if SILVERLIGHT
            if(KeyGesture == null || !(eventArgs is KeyEventArgs)) return res;
            KeyEventArgs keyEventArgs = (KeyEventArgs)eventArgs;
            return res && keyEventArgs.Key == KeyGesture.Key && Keyboard.Modifiers == KeyGesture.ModifierKeys;
#else
            if(KeyGesture == null || !(eventArgs is InputEventArgs)) return res;
            InputEventArgs inputEventArgs = (InputEventArgs)eventArgs;
            return res && KeyGesture.Matches(Source, inputEventArgs);
#endif
        }
    }
}