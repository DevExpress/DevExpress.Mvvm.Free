using NUnit.Framework;
using DevExpress.Mvvm.UI.Interactivity;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows;
using System;

namespace DevExpress.Mvvm.UI.Tests {
    [TestFixture]
    public class DependencyPropertyBehaviorTests : BaseWpfFixture {
        protected void ShowWindow(UIElement element) {
            RealWindow.Content = element;
            EnqueueShowRealWindow();
            EnqueueWindowUpdateLayout();
        }
        [Test, Asynchronous]
        public void MainCasePasswordBoxTest() {
            var propertyChangedViewModel = new PropertyChangedViewModel();
            var passwordBox = new PasswordBox() { DataContext = propertyChangedViewModel };
            ShowWindow(passwordBox);
            var behavior = new DependencyPropertyBehavior();
            behavior.EventName = "PasswordChanged";
            behavior.PropertyName = "Password";
            Interaction.GetBehaviors(passwordBox).Add(behavior);
            BindingOperations.SetBinding(behavior, DependencyPropertyBehavior.BindingProperty, new Binding("Text") { Mode = BindingMode.TwoWay });
            Assert.AreEqual(0, propertyChangedViewModel.TextChangedCounter);
            EnqueueShowWindow();
            EnqueueCallback(() => {
                passwordBox.Password = "123456";
            });
            EnqueueWindowUpdateLayout();
            EnqueueCallback(() => {
                Assert.AreEqual(1, propertyChangedViewModel.TextChangedCounter);
                Assert.AreEqual("123456", propertyChangedViewModel.Text);
                passwordBox.Password = "";
            });
            EnqueueWindowUpdateLayout();
            EnqueueCallback(() => {
                Assert.AreEqual(2, propertyChangedViewModel.TextChangedCounter);
                propertyChangedViewModel.Text = "654321";
            });
            EnqueueWindowUpdateLayout();
            EnqueueCallback(() => {
                Assert.AreEqual("654321", passwordBox.Password);
            });
            EnqueueTestComplete();
        }
        [Test, Asynchronous]
        public void MainCaseTextBoxTest() {
            var propertyChangedViewModel = new PropertyChangedViewModel();
            var textBox = new TextBox() { DataContext = propertyChangedViewModel };
            ShowWindow(textBox);
            var behaviorSelectedText = new DependencyPropertyBehavior() { EventName = "SelectionChanged", PropertyName = "SelectedText" };
            var behaviorSelectionStart = new DependencyPropertyBehavior() { EventName = "SelectionChanged", PropertyName = "SelectionStart" };
            var behaviorSelectionLength = new DependencyPropertyBehavior() { EventName = "SelectionChanged", PropertyName = "SelectionLength" };
            Interaction.GetBehaviors(textBox).Add(behaviorSelectedText);
            Interaction.GetBehaviors(textBox).Add(behaviorSelectionStart);
            Interaction.GetBehaviors(textBox).Add(behaviorSelectionLength);
            BindingOperations.SetBinding(textBox, TextBox.TextProperty, new Binding("Text") { Mode = BindingMode.TwoWay });
            BindingOperations.SetBinding(behaviorSelectedText, DependencyPropertyBehavior.BindingProperty, new Binding("SelectedText") { Mode = BindingMode.TwoWay });
            BindingOperations.SetBinding(behaviorSelectionStart, DependencyPropertyBehavior.BindingProperty, new Binding("SelectionStart") { Mode = BindingMode.TwoWay });
            BindingOperations.SetBinding(behaviorSelectionLength, DependencyPropertyBehavior.BindingProperty, new Binding("SelectionLength") { Mode = BindingMode.TwoWay });
            EnqueueShowWindow();
            EnqueueCallback(() => {
                propertyChangedViewModel.Text = "12345678901234567890";
                propertyChangedViewModel.SelectionStart = 5;
                propertyChangedViewModel.SelectionLength = 10;
            });
            EnqueueWindowUpdateLayout();
            EnqueueCallback(() => {
                Assert.AreEqual("6789012345", propertyChangedViewModel.SelectedText);
            });
            EnqueueTestComplete();
        }
        [Test, Asynchronous]
        public void MainCaseNestedPropertiesTextBoxTest() {
            var propertyChangedViewModel = new PropertyChangedViewModel();
            var button = new Button() { DataContext = propertyChangedViewModel };
            var textBox = new TextBox();
            button.Content = textBox;
            ShowWindow(button);
            var behaviorSelectedText = new DependencyPropertyBehavior() { EventName = "Content.SelectionChanged", PropertyName = "Content.SelectedText" };
            var behaviorSelectionStart = new DependencyPropertyBehavior() { EventName = "Content.SelectionChanged", PropertyName = "Content.SelectionStart" };
            var behaviorSelectionLength = new DependencyPropertyBehavior() { EventName = "Content.SelectionChanged", PropertyName = "Content.SelectionLength" };
            Interaction.GetBehaviors(button).Add(behaviorSelectedText);
            Interaction.GetBehaviors(button).Add(behaviorSelectionStart);
            Interaction.GetBehaviors(button).Add(behaviorSelectionLength);
            BindingOperations.SetBinding(textBox, TextBox.TextProperty, new Binding("Text") { Mode = BindingMode.TwoWay });
            BindingOperations.SetBinding(behaviorSelectedText, DependencyPropertyBehavior.BindingProperty, new Binding("SelectedText") { Mode = BindingMode.TwoWay });
            BindingOperations.SetBinding(behaviorSelectionStart, DependencyPropertyBehavior.BindingProperty, new Binding("SelectionStart") { Mode = BindingMode.TwoWay });
            BindingOperations.SetBinding(behaviorSelectionLength, DependencyPropertyBehavior.BindingProperty, new Binding("SelectionLength") { Mode = BindingMode.TwoWay });
            EnqueueShowWindow();
            EnqueueCallback(() => {
                propertyChangedViewModel.Text = "12345678901234567890";
                propertyChangedViewModel.SelectionStart = 5;
                propertyChangedViewModel.SelectionLength = 10;
            });
            EnqueueWindowUpdateLayout();
            EnqueueCallback(() => {
                Assert.AreEqual("6789012345", propertyChangedViewModel.SelectedText);
            });
            EnqueueTestComplete();
        }
        [Test, Asynchronous, Ignore("Ignore")]
        public void ConvertableTypesTest() {
            var propertyChangedViewModel = new PropertyChangedViewModel() { FloatProperty = 100 };
            var button = new Button() { DataContext = propertyChangedViewModel };
            ShowWindow(button);
            var behaviorSelectedText = new DependencyPropertyBehavior() { EventName = "", PropertyName = "Width" };
            Interaction.GetBehaviors(button).Add(behaviorSelectedText);
            BindingOperations.SetBinding(behaviorSelectedText, DependencyPropertyBehavior.BindingProperty, new Binding("FloatProperty") { Mode = BindingMode.TwoWay });
            Assert.AreEqual(100, button.Width);
            EnqueueShowWindow();
            EnqueueCallback(() => {
                propertyChangedViewModel.FloatProperty = 101;
            });
            EnqueueWindowUpdateLayout();
            EnqueueCallback(() => {
                Assert.AreEqual(101, button.Width);
                button.Width = 102;
            });
            EnqueueWindowUpdateLayout();
            EnqueueCallback(() => {
                Assert.AreEqual(102, button.Width);
            });
            EnqueueTestComplete();
        }
        [Test, Asynchronous]
        public void OneWayBinding() {
            var vm = new PropertyChangedViewModel();
            var control = new DependencyPropertyBehaviorTestControl() { DataContext = vm };
            vm.SetReadOnlyProperty("1");
            var behavior = new DependencyPropertyBehavior() { PropertyName = "Property" };
            BindingOperations.SetBinding(behavior, DependencyPropertyBehavior.BindingProperty, new Binding("ReadOnlyProperty") { Mode = BindingMode.OneWay });
            Interaction.GetBehaviors(control).Add(behavior);
            Assert.AreEqual(1, control.propertyChangedCounter);
            Assert.AreEqual("1", control.Property);
            vm.SetReadOnlyProperty("2");
            Assert.AreEqual(2, control.propertyChangedCounter);
            Assert.AreEqual("2", control.Property);
        }
    }
    public class PropertyChangedViewModel : BindableBase {
        public PropertyChangedViewModel NestedPropertyChangedViewModel { get; set; }
        public int TextChangedCounter = 0;
        string text;
        public string Text {
            get { return text; }
            set {
                SetProperty(ref text, value, () => Text);
                TextChangedCounter++;
            }
        }
        public int SelectedTextChangedCounter = 0;
        string selectedText;
        public string SelectedText {
            get { return selectedText; }
            set {
                SetProperty(ref selectedText, value, () => SelectedText);
                SelectedTextChangedCounter++;
            }
        }
        public int SelectionStartChangedCounter = 0;
        int selectionStart;
        public int SelectionStart {
            get { return selectionStart; }
            set {
                SetProperty(ref selectionStart, value, () => SelectionStart);
                SelectionStartChangedCounter++;
            }
        }
        public int SelectionLengthChangedCounter = 0;
        int selectionLength;
        public int SelectionLength {
            get { return selectionLength; }
            set {
                SetProperty(ref selectionLength, value, () => SelectionLength);
                SelectionLengthChangedCounter++;
            }
        }
        public int FloatPropertyChangedCounter = 0;
        int floatProperty;
        public int FloatProperty {
            get { return floatProperty; }
            set {
                SetProperty(ref floatProperty, value, () => FloatProperty);
                FloatPropertyChangedCounter++;
            }
        }
        public string ReadOnlyProperty {
            get { return GetProperty(() => ReadOnlyProperty); }
            protected set { SetProperty(() => ReadOnlyProperty, value); }
        }
        public void SetReadOnlyProperty(string value) {
            ReadOnlyProperty = value;
        }
    }
    public class DependencyPropertyBehaviorTestControl : Control {
        public int propertyChangedCounter = 0;
        string property;
        public string Property {
            get { return property; }
            set {
                property = value;
                propertyChangedCounter++;
            }
        }
    }
}