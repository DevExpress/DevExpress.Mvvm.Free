using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
#if NETFX_CORE
using Windows.UI.Xaml;
using Windows.UI.Xaml.Input;
using DevExpress.Mvvm.UI.Native;
#else
using DevExpress.Mvvm.UI.Native;
#endif

namespace DevExpress.Mvvm.UI {
    public class KeyToCommand : EventToCommandBase {
#if !NETFX_CORE
        public static readonly DependencyProperty KeyGestureProperty =
            DependencyProperty.Register("KeyGesture", typeof(KeyGesture), typeof(KeyToCommand),
            new PropertyMetadata(null));
        public KeyGesture KeyGesture {
            get { return (KeyGesture)GetValue(KeyGestureProperty); }
            set { SetValue(KeyGestureProperty, value); }
        }
#else
        public static readonly DependencyProperty KeyGestureProperty =
            DependencyProperty.Register("KeyGesture", typeof(string), typeof(KeyToCommand),
            new PropertyMetadata(null, (d, e) => ((KeyToCommand)d).OnKeyGestureChanged(e)));

        void OnKeyGestureChanged(DependencyPropertyChangedEventArgs e) {
            keyGesture = (string)e.NewValue;
        }
        public string KeyGesture {
            get { return (string)GetValue(KeyGestureProperty); }
            set { SetValue(KeyGestureProperty, value); }
        }
        KeyGesture keyGesture = null;
#endif
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
#elif NETFX_CORE

            if(keyGesture == null || !(eventArgs is KeyRoutedEventArgs)) return res;
            KeyRoutedEventArgs keyEventArgs = (KeyRoutedEventArgs)eventArgs;
            return res && keyEventArgs.Key == keyGesture.Key && ModifierKeysHelper.GetKeyboardModifiers() == keyGesture.ModifierKeys;
#else
            if(KeyGesture == null || !(eventArgs is InputEventArgs)) return res;
            InputEventArgs inputEventArgs = (InputEventArgs)eventArgs;
            return res && KeyGesture.Matches(Source, inputEventArgs);
#endif
        }
    }
}