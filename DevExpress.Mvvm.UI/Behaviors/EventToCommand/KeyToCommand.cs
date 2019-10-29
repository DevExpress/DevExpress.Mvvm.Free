using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using DevExpress.Mvvm.UI.Native;

namespace DevExpress.Mvvm.UI {
    public class KeyToCommand : EventToCommandBase {
        public static readonly DependencyProperty KeyGestureProperty =
            DependencyProperty.Register("KeyGesture", typeof(KeyGesture), typeof(KeyToCommand),
            new PropertyMetadata(null));
        public KeyGesture KeyGesture {
            get { return (KeyGesture)GetValue(KeyGestureProperty); }
            set { SetValue(KeyGestureProperty, value); }
        }
        static KeyToCommand() {
            EventNameProperty.OverrideMetadata(typeof(KeyToCommand), new PropertyMetadata("KeyUp"));
        }
        protected override void Invoke(object sender, object eventArgs) {
            if(CommandCanExecute(CommandParameter))
                CommandExecute(CommandParameter);
        }
        protected override bool CanInvoke(object sender, object eventArgs) {
            bool res = base.CanInvoke(sender, eventArgs);
            if(KeyGesture == null || !(eventArgs is InputEventArgs)) return res;
            InputEventArgs inputEventArgs = (InputEventArgs)eventArgs;
            return res && KeyGesture.Matches(Source, inputEventArgs);
        }
    }
}