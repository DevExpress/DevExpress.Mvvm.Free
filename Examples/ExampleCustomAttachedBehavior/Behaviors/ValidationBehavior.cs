using DevExpress.Mvvm.UI.Interactivity;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Example.Behaviors {
    public class ValidationBehavior : Behavior<TextBox> {
        public static readonly DependencyProperty ValidForegroundProperty =
            DependencyProperty.Register("ValidForeground", typeof(Brush), typeof(ValidationBehavior),
            new PropertyMetadata(new SolidColorBrush(Colors.Black), (d, e) => ((ValidationBehavior)d).Update()));
        public static readonly DependencyProperty InvalidForegroundProperty =
            DependencyProperty.Register("InvalidForeground", typeof(Brush), typeof(ValidationBehavior), 
            new PropertyMetadata(new SolidColorBrush(Colors.Red), (d,e) => ((ValidationBehavior)d).Update()));
        public static readonly DependencyProperty InvalidValueProperty =
            DependencyProperty.Register("InvalidValue", typeof(string), typeof(ValidationBehavior),
            new PropertyMetadata(string.Empty, (d, e) => ((ValidationBehavior)d).Update()));
        public Brush ValidForeground {
            get { return (Brush)GetValue(ValidForegroundProperty); }
            set { SetValue(ValidForegroundProperty, value); }
        }
        public Brush InvalidForeground {
            get { return (Brush)GetValue(InvalidForegroundProperty); }
            set { SetValue(InvalidForegroundProperty, value); }
        }
        public string InvalidValue {
            get { return (string)GetValue(InvalidValueProperty); }
            set { SetValue(InvalidValueProperty, value); }
        }

        protected override void OnAttached() {
            base.OnAttached();
            AssociatedObject.TextChanged += OnAssociatedObjectTextChanged;
            Update();
        }
        protected override void OnDetaching() {
            AssociatedObject.TextChanged -= OnAssociatedObjectTextChanged;
            base.OnDetaching();
        }
        void OnAssociatedObjectTextChanged(object sender, TextChangedEventArgs e) {
            Update();
        }
        void Update() {
            if(!IsAttached) return;
            if(AssociatedObject.Text == InvalidValue)
                AssociatedObject.Foreground = InvalidForeground;
            else AssociatedObject.Foreground = ValidForeground;
        }
    }
}
