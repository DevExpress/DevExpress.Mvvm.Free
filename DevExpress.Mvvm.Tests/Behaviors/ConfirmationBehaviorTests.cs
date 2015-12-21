using NUnit.Framework;
using DevExpress.Mvvm.Native;
using DevExpress.Mvvm.UI.Interactivity;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace DevExpress.Mvvm.UI.Tests {
    [TestFixture]
    public class ConfirmationBehaviorTests {
        public class TestViewModelBase : ViewModelBase {
            public IMessageBoxService MessageBoxService { get { return GetService<IMessageBoxService>(); } }
        }
        public class TestControl1 : Control {
            public ICommand Command { get; set; }
        }
        public class TestMessageBoxService : IMessageBoxService {
            public string MessageBoxTest;
            public string Caption;
            public MessageButton Button;
            public MessageResult Result = MessageResult.Yes;
            public MessageResult DefaultResult;
            public int ShowCount = 0;
            public MessageIcon Icon;
            public MessageResult Show(string messageBoxText, string caption, MessageButton button, MessageIcon icon, MessageResult defaultResult) {
                MessageBoxTest = messageBoxText;
                Caption = caption;
                Button = button;
                Icon = icon;
                DefaultResult = defaultResult;
                ShowCount++;
                return Result;
            }
        }

        MessageBoxService CreateMessageService() {
            return new MessageBoxService();
        }
        [Test]
        public void GetActualServiceTest1() {
            var b = new ConfirmationBehavior();
            var service = CreateMessageService();
            b.MessageBoxService = service;
            Assert.AreEqual(service, b.GetActualService());
        }
        [Test]
        public void GetActualServiceTest2() {
            UserControl root = new UserControl();
            var service = CreateMessageService();
            Interaction.GetBehaviors(root).Add(service);
            var viewModel = new TestViewModelBase();
            root.DataContext = viewModel;
            Button button = new Button();
            var b = new ConfirmationBehavior();
            Interaction.GetBehaviors(button).Add(b);
            root.Content = button;
            Assert.AreEqual(service, viewModel.MessageBoxService);
            Assert.AreEqual(service, b.GetActualService());
        }

        [Test]
        public void SetAssociatedObjectCommandPropertyTest1() {
            Button control = new Button();
            var b = new ConfirmationBehavior();
            Interaction.GetBehaviors(control).Add(b);
            var command = new DelegateCommand(() => { });
            var res = b.SetAssociatedObjectCommandProperty(command);
            Assert.IsTrue(res);
            Assert.AreEqual(command, control.Command);
        }
        [Test]
        public void SetAssociatedObjectCommandPropertyTest3() {
            TestControl1 control = new TestControl1();
            var b = new ConfirmationBehavior();
            Interaction.GetBehaviors(control).Add(b);
            var command = new DelegateCommand(() => { });
            var res = b.SetAssociatedObjectCommandProperty(command);
            Assert.IsTrue(res);
            Assert.AreEqual(command, control.Command);
        }

        [Test]
        public void CanExecuteChangedTest1() {
            int canExecute = 0;
            int confirmationCanExecute = 0;
            DelegateCommand command = new DelegateCommand(
                () => { },
                () => {
                    canExecute++;
                    return true;
                }
            );
            ConfirmationBehavior b = new ConfirmationBehavior();
            b.ConfirmationCommand.CanExecuteChanged += (d, e) => {
                confirmationCanExecute++;
            };
            b.Command = command;
            Assert.AreEqual(0, canExecute);
            Assert.AreEqual(1, confirmationCanExecute);
            Button control = new Button();
            Interaction.GetBehaviors(control).Add(b);
            Assert.AreEqual(1, canExecute);
            Assert.AreEqual(1, confirmationCanExecute);
            Assert.AreEqual(b.ConfirmationCommand, control.Command);
        }
        [Test]
        public void CanExecuteChangedTest2() {
            Button control = new Button();
            bool isCommandEnabled = true;
            DelegateCommand command = DelegateCommandFactory.Create(
                () => { },
                () => {
                    return isCommandEnabled;
                }, false);
            ConfirmationBehavior b = new ConfirmationBehavior();
            b.Command = command;
            Interaction.GetBehaviors(control).Add(b);
            Assert.IsTrue(control.IsEnabled);
            isCommandEnabled = false;
            command.RaiseCanExecuteChanged();
            Assert.IsFalse(control.IsEnabled);
        }

        [Test]
        public void ExecuteAndCommandParameterTest() {
            var service = new TestMessageBoxService();
            object executeCommandParameter = null;
            object canExecuteCommandParameter = null;
            DelegateCommand<object> command = new DelegateCommand<object>(
                x => {
                    executeCommandParameter = x;
                }, x => {
                    canExecuteCommandParameter = x;
                    return true;
                });
            ConfirmationBehavior b = new ConfirmationBehavior();
            b.MessageBoxService = service;
            Button control = new Button();
            Interaction.GetBehaviors(control).Add(b);
            b.Command = command;
            object controlCommandParameter = new object();
            control.CommandParameter = controlCommandParameter;
            Assert.IsNull(executeCommandParameter);
            control.Command.Execute(controlCommandParameter);
            Assert.AreEqual(controlCommandParameter, executeCommandParameter);
            Assert.AreEqual(controlCommandParameter, canExecuteCommandParameter);
            object confirmationBehaviorCommandParameter = new object();
            b.CommandParameter = confirmationBehaviorCommandParameter;
            Assert.AreEqual(controlCommandParameter, executeCommandParameter);
            Assert.AreEqual(confirmationBehaviorCommandParameter, canExecuteCommandParameter);
            control.Command.Execute(controlCommandParameter);
            Assert.AreEqual(confirmationBehaviorCommandParameter, executeCommandParameter);
            Assert.AreEqual(confirmationBehaviorCommandParameter, canExecuteCommandParameter);
        }
        [Test]
        public void ExecuteAndMessageBoxServiceTest() {
            int executeCount = 0;
            DelegateCommand command = new DelegateCommand(() => executeCount++, () => true);
            TestMessageBoxService service = new TestMessageBoxService();
            Button control = new Button();
            ConfirmationBehavior b = new ConfirmationBehavior();
            b.Command = command;
            Interaction.GetBehaviors(control).Add(b);
            b.MessageBoxService = service;
            control.Command.Execute(null);
            Assert.AreEqual(1, executeCount);
            Assert.AreEqual(1, service.ShowCount);
            Assert.AreEqual("Confirmation", service.Caption);
            Assert.AreEqual("Do you want to perform this action?", service.MessageBoxTest);
            Assert.AreEqual(MessageIcon.None, service.Icon);
            Assert.AreEqual(MessageButton.YesNo, service.Button);
            Assert.AreEqual(MessageResult.None, service.DefaultResult);

            b.MessageText = "MessageText";
            b.MessageTitle = "MessageTitle";
            b.MessageIcon = MessageBoxImage.Hand;
            b.MessageButton = MessageBoxButton.OKCancel;
            b.MessageDefaultResult = MessageBoxResult.Cancel;
            service.Result = MessageResult.OK;
            control.Command.Execute(null);
            Assert.AreEqual(2, executeCount);
            Assert.AreEqual(2, service.ShowCount);
            Assert.AreEqual("MessageTitle", service.Caption);
            Assert.AreEqual("MessageText", service.MessageBoxTest);
            Assert.AreEqual(MessageIcon.Hand, service.Icon);
            Assert.AreEqual(MessageButton.OKCancel, service.Button);
            Assert.AreEqual(MessageResult.Cancel, service.DefaultResult);

            service.Result = MessageResult.Cancel;
            control.Command.Execute(null);
            Assert.AreEqual(2, executeCount);
            Assert.AreEqual(3, service.ShowCount);
        }
        [Test]
        public void DisableConfirmationMessage() {
            int executeCount = 0;
            DelegateCommand command = new DelegateCommand(() => executeCount++, () => true);
            TestMessageBoxService service = new TestMessageBoxService();
            Button control = new Button();
            ConfirmationBehavior b = new ConfirmationBehavior();
            b.Command = command;
            Interaction.GetBehaviors(control).Add(b);
            b.MessageBoxService = service;
            b.EnableConfirmationMessage = false;
            control.Command.Execute(null);
            Assert.AreEqual(1, executeCount);
            Assert.AreEqual(0, service.ShowCount);
            Assert.IsNull(service.Caption);
            Assert.IsNull(service.MessageBoxTest);
        }
    }
}