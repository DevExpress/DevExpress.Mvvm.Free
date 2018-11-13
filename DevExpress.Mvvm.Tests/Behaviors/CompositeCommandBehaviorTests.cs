using NUnit.Framework;
using System;
using System.Windows.Input;
using DevExpress.Mvvm.Native;
using System.Windows.Controls;
using DevExpress.Mvvm.UI.Interactivity;
using DevExpress.Mvvm.UI;
using System.Windows;

namespace DevExpress.Mvvm.Tests.Behaviors {
    [TestFixture]
    public class CompositeCommandBehaviorTests : BaseWpfFixture {
        #region Props
        Button TestControl { get; set; }
        CompositeCommandBehavior CompositeCommand { get; set; }
        #endregion

        #region Helpers
        void PrepareDefaultButtonBehavior(bool setBehaviorPropName, string propName = null) {
            TestControl = new Button();
            CompositeCommand = new CompositeCommandBehavior();
            if(setBehaviorPropName)
                CompositeCommand.CommandPropertyName = propName;
            CompositeCommand.Commands.Add(new CommandItem() {
                Command = new MockCommand(1),
            });
            Interaction.GetBehaviors(TestControl).Add(CompositeCommand);
        }
        #endregion

        protected override void TearDownCore() {
            CompositeCommand = null;
            TestControl = null;
            base.TearDownCore();
        }

        [Test]
        public void SetAttachedObjectDefaultCommandPropertyTest() {
            PrepareDefaultButtonBehavior(false);
            Window.Content = TestControl;
            EnqueueShowWindow();
            Assert.AreEqual(CompositeCommand.CompositeCommand, TestControl.Command);
        }
        [Test]
        public void SetAttachedObjectCustomCommandPropertyTest() {
            PrepareDefaultButtonBehavior(true, "CommandParameter");
            Window.Content = TestControl;
            EnqueueShowWindow();
            Assert.AreEqual(null, TestControl.Command);
            Assert.AreEqual(CompositeCommand.CompositeCommand, TestControl.CommandParameter);
        }
        [Test]
        public void SetAttachedObjectCommandPropertyToNullTest() {
            PrepareDefaultButtonBehavior(true, null);
            Window.Content = TestControl;
            EnqueueShowWindow();
            Assert.AreEqual(null, TestControl.Command);
        }
        [Test]
        public void SetAttachedObjectCommandPropertyToNonExistedTest() {
            PrepareDefaultButtonBehavior(true, "Command1");
            Window.Content = TestControl;
            EnqueueShowWindow();
            Assert.AreEqual(null, TestControl.Command);
        }
        [Test]
        public void SetAttachedObjectCommandPropertyAndChangeTest() {
            PrepareDefaultButtonBehavior(false);
            Window.Content = TestControl;
            EnqueueShowWindow();
            Assert.AreEqual(CompositeCommand.CompositeCommand, TestControl.Command);
            CompositeCommand.CommandPropertyName = "DataContext";
            Assert.AreEqual(null, TestControl.Command);
            Assert.AreEqual(CompositeCommand.CompositeCommand, TestControl.DataContext);
        }
        [Test]
        public void PropertyNotChangedOnReleaseTest() {
            PrepareDefaultButtonBehavior(true, "DataContext");
            Window.Content = TestControl;
            EnqueueShowWindow();
            Assert.AreEqual(CompositeCommand.CompositeCommand, TestControl.DataContext);
            TestControl.DataContext = 1;
            CompositeCommand.CommandPropertyName = null;
            Assert.AreEqual(1, TestControl.DataContext);
        }
        [Test]
        public void ResultCommandCanExecuteTest() {
            CompositeCommand = new CompositeCommandBehavior();
            CompositeCommand.Commands.Add(new CommandItem() {
                Command = new MockCommand(1)
            });
            CompositeCommand.Commands.Add(new CommandItem() {
                Command = new MockCommand(2)
            });
            CompositeCommand.Commands.Add(new CommandItem() {
                Command = new MockCommand(3)
            });

            Assert.IsTrue(CompositeCommand.CompositeCommand.CanExecute(null));
            (CompositeCommand.Commands[1].Command as MockCommand).IsEnabled = false;
            Assert.IsFalse(CompositeCommand.CompositeCommand.CanExecute(null));
            CompositeCommand.Commands[1].CheckCanExecute = false;
            Assert.IsTrue(CompositeCommand.CompositeCommand.CanExecute(null));
            (CompositeCommand.Commands[2].Command as MockCommand).IsEnabled = false;
            Assert.IsFalse(CompositeCommand.CompositeCommand.CanExecute(null));
            CompositeCommand.Commands.RemoveAt(2);
            Assert.IsTrue(CompositeCommand.CompositeCommand.CanExecute(null));
            CompositeCommand.Commands.Clear();
            Assert.IsFalse(CompositeCommand.CompositeCommand.CanExecute(null));
        }
        [Test]
        public void ResultCommandExecuteTest() {
            CompositeCommand = new CompositeCommandBehavior();
            CompositeCommand.Commands.Add(new CommandItem() {
                Command = new MockCommand(1),
                CommandParameter = 11
            });
            CompositeCommand.Commands.Add(new CommandItem() {
                Command = new MockCommand(2),
                CommandParameter = 12
            });
            CompositeCommand.Commands.Add(new CommandItem() {
                Command = new MockCommand(3)
            });

            CompositeCommand.CompositeCommand.Execute(14);
            MockCommand command = CompositeCommand.Commands[0].Command as MockCommand;
            Assert.AreEqual(1, command.CommandExecuteResult);
            Assert.AreEqual(11, command.CommandExecuteParameter);
            command = CompositeCommand.Commands[1].Command as MockCommand;
            Assert.AreEqual(2, command.CommandExecuteResult);
            Assert.AreEqual(12, command.CommandExecuteParameter);
            command = CompositeCommand.Commands[2].Command as MockCommand;
            Assert.AreEqual(3, command.CommandExecuteResult);
            Assert.AreEqual(null, command.CommandExecuteParameter);
        }
        [Test]
        public void FindAssociatedObjectPropertyTest() {
            MockControl control = new MockControl();
            CompositeCommand = new CompositeCommandBehavior() {
                CommandPropertyName = "CustomData"
            };
            CompositeCommand.Commands.Add(new CommandItem() {
                Command = new MockCommand(1),
            });
            Interaction.GetBehaviors(control).Add(CompositeCommand);
            Window.Content = control;
            EnqueueShowWindow();
            Assert.AreEqual(CompositeCommand.CompositeCommand, control.GetValue(MockControl.CustomDataProperty));
            CompositeCommand.CommandPropertyName = null;
            Assert.AreEqual(null, control.GetValue(MockControl.CustomDataProperty));
            CompositeCommand.CommandPropertyName = "CustomDataProperty";
            Assert.AreEqual(CompositeCommand.CompositeCommand, control.GetValue(MockControl.CustomDataProperty));
            CompositeCommand.CommandPropertyName = "DataContextProperty";
            Assert.AreEqual(null, control.GetValue(MockControl.CustomDataProperty));
            Assert.AreEqual(CompositeCommand.CompositeCommand, control.DataContext);
            CompositeCommand.CommandPropertyName = "CustomData2";
            Assert.AreEqual(CompositeCommand.CompositeCommand, control.CustomData2);
        }

        [Test]
        public void CompositeCommandCanExecuteConditionTest() {
            CompositeCommand = new CompositeCommandBehavior();
            Assert.AreEqual(CompositeCommandExecuteCondition.AllCommandsCanBeExecuted, CompositeCommand.CanExecuteCondition);

            var firstCommand = new MockCommand(1) { IsEnabled = false };
            var secondCommand = new MockCommand(2);
            CompositeCommand.Commands.Add(new CommandItem() { Command = firstCommand, CommandParameter = 11 });
            CompositeCommand.Commands.Add(new CommandItem() { Command = secondCommand, CommandParameter = 12 });

            Assert.IsFalse(CompositeCommand.CompositeCommand.CanExecute(null));
            CompositeCommand.CompositeCommand.Execute(null);
            Assert.IsNull(firstCommand.CommandExecuteResult);
            Assert.IsNull(secondCommand.CommandExecuteResult);

            CompositeCommand.CanExecuteCondition = CompositeCommandExecuteCondition.AnyCommandCanBeExecuted;
            Assert.IsTrue(CompositeCommand.CompositeCommand.CanExecute(null));
            CompositeCommand.CompositeCommand.Execute(null);
            Assert.IsNull(firstCommand.CommandExecuteResult);
            Assert.AreEqual(2, secondCommand.CommandExecuteResult);

            secondCommand.Reset();
            secondCommand.IsEnabled = false;
            Assert.IsFalse(CompositeCommand.CompositeCommand.CanExecute(null));
            CompositeCommand.CompositeCommand.Execute(null);
            Assert.IsNull(firstCommand.CommandExecuteResult);
            Assert.IsNull(secondCommand.CommandExecuteResult);
        }

        [Test]
        public void CommandItemMemoryLeaksTest() {
            var command = new MockCommand(1);
            var commandItemReference = CreateCommandItemReference(command);
            MemoryLeaksHelper.EnsureCollected(new[] { commandItemReference });
        }
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
        WeakReference CreateCommandItemReference(ICommand command) {
            return new WeakReference(new CommandItem { Command = command });
        }

        #region Mock
        class MockControl : Control {
            public static readonly DependencyProperty CustomDataProperty =
                DependencyProperty.Register("CustomData", typeof(object), typeof(MockControl), new PropertyMetadata(null));

            public object CustomData2 { get; set; }
        }

        class MockCommand : ICommand {
            object executeResult;
            bool isEnabled;
            public bool IsEnabled {
                get { return isEnabled; }
                set {
                    if(isEnabled == value)
                        return;

                    isEnabled = value;
                    CanExecuteChanged.Do(x => x(this, EventArgs.Empty));
                }
            }
            public object CommandExecuteResult { get; private set; }
            public object CommandExecuteParameter { get; private set; }

            public MockCommand(object executeResult) {
                this.executeResult = executeResult;
                IsEnabled = true;
            }

            public void Reset() {
                CommandExecuteResult = null;
            }

            public bool CanExecute(object parameter) {
                return IsEnabled;
            }

            public event EventHandler CanExecuteChanged;

            public void Execute(object parameter) {
                CommandExecuteResult = executeResult;
                CommandExecuteParameter = parameter;
            }
        }
        #endregion
    }
}