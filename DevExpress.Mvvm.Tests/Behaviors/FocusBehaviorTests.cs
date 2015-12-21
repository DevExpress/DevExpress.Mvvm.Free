using NUnit.Framework;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using DevExpress.Mvvm.Native;
using DevExpress.Mvvm.POCO;
using DevExpress.Mvvm.UI.Interactivity;
using System.Windows.Input;
using System.Threading;
using System.Windows.Threading;
using System.ComponentModel;

namespace DevExpress.Mvvm.UI.Tests {
    [TestFixture]
    public class FocusBehaviorTests : BaseWpfFixture {
        [Test]
        public void DefalutValues() {
            FocusBehavior behavior = new FocusBehavior();
            Assert.AreEqual(TimeSpan.FromMilliseconds(0), FocusBehavior.DefaultFocusDelay);
            Assert.AreEqual(null, behavior.FocusDelay);
            Assert.AreEqual("Loaded", behavior.EventName);
        }
        [Test]
        public void GetFocusDelay() {
            FocusBehavior behavior = new FocusBehavior();
            Action<double, double> checkFocusDelay = (double expectedInWpf, double expectedInSilverlight) => {
                Assert.AreEqual(TimeSpan.FromMilliseconds(expectedInWpf), behavior.GetFocusDelay());
            };

            Assert.AreEqual(null, behavior.FocusDelay);
            checkFocusDelay(0, 500);
            behavior.FocusDelay = TimeSpan.FromMilliseconds(800);
            checkFocusDelay(800, 800);

            behavior.FocusDelay = TimeSpan.FromMilliseconds(0);
            behavior.EventName = "MouseEnter";
            checkFocusDelay(0, 0);

            behavior.FocusDelay = TimeSpan.FromMilliseconds(800);
            checkFocusDelay(800, 800);
        }

        [Test, Asynchronous]
        public void FocusOnLoaded() {
            TextBox control = new TextBox();
            FocusBehavior behavior = new FocusBehavior();
            Interaction.GetBehaviors(control).Add(behavior);
            behavior.FocusDelay = TimeSpan.FromMilliseconds(0);
            Grid grid = new Grid();
            grid.Children.Add(new Button());
            grid.Children.Add(control);
            grid.Children.Add(new Button());
            Window.Content = grid;

            EnqueueShowWindow();
            EnqueueCallback(() => {
                CheckFocusedElement(control);
            });
            EnqueueTestComplete();
        }
        [Test, Asynchronous]
        public void FocusOnLoadedWithDelay() {
            TextBox control = new TextBox();
            FocusBehavior behavior = new FocusBehavior();
            Interaction.GetBehaviors(control).Add(behavior);
            behavior.FocusDelay = TimeSpan.FromMilliseconds(400);
            Grid grid = new Grid();
            grid.Children.Add(new Button());
            grid.Children.Add(control);
            grid.Children.Add(new Button());
            Window.Content = grid;


            EnqueueShowWindow();
            EnqueueDelay(500);
            EnqueueCallback(() => {
                CheckFocusedElement(control);
            });
            EnqueueTestComplete();
        }
        void CheckFocusedElement(Control expectedFocusedElement) {
            Assert.IsTrue(expectedFocusedElement.IsFocused);
        }
        void CheckUnfocusedElement(Control expectedFocusedElement) {
            Assert.IsFalse(expectedFocusedElement.IsFocused);
        }

        [Test, Asynchronous]
        public void FocusEvent() {
            TestControl controlWithEvent = new TestControl();
            FocusBehavior behavior = new FocusBehavior() { EventName = "TestEvent" };
            Interaction.GetBehaviors(controlWithEvent).Add(behavior);
            behavior.FocusDelay = TimeSpan.FromMilliseconds(0);
            Grid grid = new Grid();
            grid.Children.Add(new Button());
            grid.Children.Add(controlWithEvent);
            Window.Content = grid;
            EnqueueShowWindow();
            EnqueueCallback(() => {
                CheckUnfocusedElement(controlWithEvent);
                controlWithEvent.RaiseTestEvent();
                CheckFocusedElement(controlWithEvent);
            });
            EnqueueTestComplete();
        }
        [Test, Asynchronous]
        public void FocusEventAndSourceObject() {
            TextBox control = new TextBox();
            TestControl controlWithEvent = new TestControl();
            FocusBehavior behavior = new FocusBehavior() { EventName = "TestEvent", SourceObject = controlWithEvent };
            Interaction.GetBehaviors(control).Add(behavior);
            behavior.FocusDelay = TimeSpan.FromMilliseconds(0);
            Grid grid = new Grid();
            grid.Children.Add(new Button());
            grid.Children.Add(control);
            grid.Children.Add(new Button());
            grid.Children.Add(controlWithEvent);
            Window.Content = grid;
            EnqueueShowWindow();
            EnqueueCallback(() => {
                CheckUnfocusedElement(control);
                controlWithEvent.RaiseTestEvent();
                CheckFocusedElement(control);
            });
            EnqueueTestComplete();
        }
        [Test, Asynchronous]
        public void FocusEventAndSourceName() {
            TextBox control = new TextBox();
            TestControl controlWithEvent = new TestControl() { Name = "controlWithEvent" };
            FocusBehavior behavior = new FocusBehavior() { EventName = "TestEvent", SourceName = "controlWithEvent" };
            Interaction.GetBehaviors(control).Add(behavior);
            behavior.FocusDelay = TimeSpan.FromMilliseconds(0);
            Grid grid = new Grid();
            grid.Children.Add(new Button());
            grid.Children.Add(control);
            grid.Children.Add(new Button());
            grid.Children.Add(controlWithEvent);
            Window.Content = grid;
            EnqueueShowWindow();
            EnqueueCallback(() => {
                CheckUnfocusedElement(control);
                controlWithEvent.RaiseTestEvent();
                CheckFocusedElement(control);
            });
            EnqueueTestComplete();
        }

        [Test, Asynchronous]
        public void FocusPropertyName() {
            TestControl controlWithProperty_1 = new TestControl();
            TestControl controlWithProperty_2 = new TestControl();
            FocusBehavior behavior_1 = new FocusBehavior() { PropertyName = "TestProperty_1" };
            FocusBehavior behavior_2 = new FocusBehavior() { PropertyName = "TestProperty_2" };
            Interaction.GetBehaviors(controlWithProperty_1).Add(behavior_1);
            Interaction.GetBehaviors(controlWithProperty_2).Add(behavior_2);

            behavior_1.FocusDelay = TimeSpan.FromMilliseconds(0);
            behavior_2.FocusDelay = TimeSpan.FromMilliseconds(0);
            Grid grid = new Grid();
            Button button = new Button();
            grid.Children.Add(button);
            grid.Children.Add(controlWithProperty_1);
            grid.Children.Add(controlWithProperty_2);
            Window.Content = grid;
            EnqueueShowWindow();

            EnqueueCallback(() => {
                CheckUnfocusedElement(controlWithProperty_1);
                CheckUnfocusedElement(controlWithProperty_2);
                controlWithProperty_1.TestProperty_1 = "Hello";
                CheckFocusedElement(controlWithProperty_1);
                controlWithProperty_2.TestProperty_2 = "Hello_";
                CheckFocusedElement(controlWithProperty_2);
            });
            EnqueueTestComplete();
        }
        [Test, Asynchronous]
        public void FocusPropertyNameWithDelay() {
            TestControl controlWithProperty_1 = new TestControl();
            TestControl controlWithProperty_2 = new TestControl();
            FocusBehavior behavior_1 = new FocusBehavior() { PropertyName = "TestProperty_1" };
            FocusBehavior behavior_2 = new FocusBehavior() { PropertyName = "TestProperty_2" };
            Interaction.GetBehaviors(controlWithProperty_1).Add(behavior_1);
            Interaction.GetBehaviors(controlWithProperty_2).Add(behavior_2);

            behavior_1.FocusDelay = TimeSpan.FromMilliseconds(400);
            behavior_2.FocusDelay = TimeSpan.FromMilliseconds(400);
            Grid grid = new Grid();
            Button button = new Button();
            grid.Children.Add(button);
            grid.Children.Add(controlWithProperty_1);
            grid.Children.Add(controlWithProperty_2);
            Window.Content = grid;

            EnqueueShowWindow();
            EnqueueCallback(() => {
                CheckUnfocusedElement(controlWithProperty_1);
                CheckUnfocusedElement(controlWithProperty_2);
                controlWithProperty_1.TestProperty_1 = "Hello";
                CheckUnfocusedElement(controlWithProperty_1);
            });
            EnqueueDelay(TimeSpan.FromMilliseconds(500));
            EnqueueCallback(() => {
                CheckFocusedElement(controlWithProperty_1);
                controlWithProperty_2.TestProperty_2 = "Hello_";
                CheckUnfocusedElement(controlWithProperty_2);
            });
            EnqueueDelay(TimeSpan.FromMilliseconds(500));
            EnqueueCallback(() => {
                CheckFocusedElement(controlWithProperty_2);
            });
            EnqueueTestComplete();
        }

        [Test, Asynchronous]
        public void FocusPropertyNameSourceObject() {
            TestControl controlWithProperty = new TestControl();
            TextBox control = new TextBox() { IsReadOnly = false };
            FocusBehavior behavior = new FocusBehavior() { PropertyName = "IsReadOnly", SourceObject = control };
            Interaction.GetBehaviors(controlWithProperty).Add(behavior);
            behavior.FocusDelay = TimeSpan.FromMilliseconds(0);
            Grid grid = new Grid();
            Button button = new Button();
            grid.Children.Add(button);
            grid.Children.Add(control);
            grid.Children.Add(controlWithProperty);
            Window.Content = grid;
            EnqueueShowWindow();

            EnqueueCallback(() => {
                CheckUnfocusedElement(controlWithProperty);
                control.IsReadOnly = true;
                CheckFocusedElement(controlWithProperty);
            });
            EnqueueTestComplete();
        }

        [Test, Asynchronous]
        public void FocusPropertyNameSourceName() {
            TextBox control = new TextBox();
            TestControl controlWithProperty = new TestControl() { Name = "controlWithProperty" };
            FocusBehavior behavior = new FocusBehavior() { PropertyName = "TestProperty_1", SourceName = "controlWithProperty" };
            Interaction.GetBehaviors(control).Add(behavior);
            behavior.FocusDelay = TimeSpan.FromMilliseconds(0);
            Grid grid = new Grid();
            Button button = new Button();
            grid.Children.Add(button);
            grid.Children.Add(control);
            grid.Children.Add(controlWithProperty);
            Window.Content = grid;
            EnqueueShowWindow();

            EnqueueCallback(() => {
                CheckUnfocusedElement(control);
                controlWithProperty.TestProperty_1 = "Test";
                CheckFocusedElement(control);
            });
            EnqueueTestComplete();
        }

        [Test, Asynchronous]
        public void FocusPropertyObjectChange() {
            TextBox control_1 = new TextBox() { IsReadOnly = false };
            TextBox control_2 = new TextBox() { IsReadOnly = false };
            TestControl controlWithProperty = new TestControl();
            FocusBehavior behavior = new FocusBehavior() { PropertyName = "IsReadOnly", SourceObject = control_1 };
            Interaction.GetBehaviors(controlWithProperty).Add(behavior);
            behavior.FocusDelay = TimeSpan.FromMilliseconds(0);
            Grid grid = new Grid();
            Button button = new Button();
            grid.Children.Add(button);
            grid.Children.Add(controlWithProperty);
            grid.Children.Add(control_1);
            grid.Children.Add(control_2);
            Window.Content = grid;
            EnqueueShowWindow();

            EnqueueCallback(() => {
                CheckUnfocusedElement(controlWithProperty);
                control_1.IsReadOnly = true;
                CheckFocusedElement(controlWithProperty);
                button.Focus();
                behavior.SourceObject = control_2;
                CheckUnfocusedElement(controlWithProperty);
                control_2.IsReadOnly = true;
                CheckFocusedElement(controlWithProperty);
            });
            EnqueueTestComplete();
        }


        public class TestControl : TextBox, INotifyPropertyChanged {
            public static readonly DependencyProperty TestProperty_1Property =
                 DependencyProperty.Register("TestProperty_1", typeof(string), typeof(TestControl), new PropertyMetadata(null));
            public string TestProperty_1 {
                get { return (string)GetValue(TestProperty_1Property); }
                set { SetValue(TestProperty_1Property, value); }
            }
            string testProperty_2 = string.Empty;
            public string TestProperty_2 {
                get { return testProperty_2; }
                set {
                    if(testProperty_2 == value) return;
                    testProperty_2 = value;
                    if(PropertyChanged != null)
                        PropertyChanged(this, new PropertyChangedEventArgs("TestProperty_2"));
                }
            }

            public event EventHandler TestEvent;
            public void RaiseTestEvent() {
                if(TestEvent != null)
                    TestEvent(this, EventArgs.Empty);
            }

            public event PropertyChangedEventHandler PropertyChanged;
        }
    }
}