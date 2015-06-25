using NUnit.Framework;
using System;
using System.Windows.Controls;
using DevExpress.Mvvm.UI.Interactivity;
using System.Windows;
using DevExpress.Mvvm.POCO;
using DevExpress.Mvvm.DataAnnotations;
using System.Windows.Input;

namespace DevExpress.Mvvm.UI.Tests {
    [TestFixture]
    public class FunctionToCommandBehaviorTests : BaseWpfFixture {
        Button Button { get; set; }
        MethodToCommandBehavior Behavior { get; set; }

        #region Helpers
        void PrepareBehavior(string command, string functionName, object source = null, string canExecuteFunction = null, string commandParameter = "CommandParameter", bool attachToView = true) {
            Behavior = new MethodToCommandBehavior();
            Behavior.Command = command;
            Behavior.Method = functionName;
            Behavior.CanExecuteFunction = canExecuteFunction;
            Behavior.CommandParameter = commandParameter;
            Behavior.Source = source;
            if(attachToView)
                Interaction.GetBehaviors(Button).Add(Behavior);
        }
        protected override void SetUpCore() {
            base.SetUpCore();
            Button = new Button();
            Window.Content = Button;
        }
        protected override void TearDownCore() {
            Behavior = null;
            Button = null;
            base.TearDownCore();
        }
        void ClickButton() {
            UITestHelper.ClickButton(Button);
        }
        #endregion

        [Test]
        public void ExceptionOnNonBoolCanExecute_Test() {
            PrepareBehavior("Command", "Operation1", new FCBObject());
            AssertHelper.AssertThrows<ArgumentException>(() => Behavior.CanExecuteFunction = "Operation2",
                x => Assert.IsTrue(x.Message.Contains("FCBObject.Operation2")));
        }
        [Test]
        public void ExceptionOnCommandNotFound_Test() {
            PrepareBehavior("Command", "Operation1", new FCBObject());
            AssertHelper.AssertThrows<ArgumentException>(() => Behavior.Command = "Comm",
                x => Assert.IsTrue(x.Message.Contains("Cannot find property with name Comm in the Button class")));
        }
        [Test]
        public void ExceptionOnCommandParameterNotFound_Test() {
            PrepareBehavior("Command", "Operation1", new FCBObject());
            AssertHelper.AssertThrows<ArgumentException>(() => Behavior.CommandParameter = "CommandParam",
                x => Assert.IsTrue(x.Message.Contains("Cannot find property with name CommandParam in the Button class")));
        }
        [Test]
        public void UseDefaultCanExecuteFunction_Test() {
            var source = new FCBObject();
            PrepareBehavior("Command", "Operation2", source);
            Assert.AreEqual("CanExecuteOperation2", source.CanExecuteResult);
        }
        [Test]
        public void UseDataContextIfSourceNotSpecified_Test00() {
            var source = new FCBObject();
            Button.DataContext = source;
            PrepareBehavior("Command", "Operation2");
            Assert.AreEqual("CanExecuteOperation2", source.CanExecuteResult);
        }
        [Test]
        public void UseDataContextIfSourceNotSpecified_Test01() {
            var source = new FCBObject();
            Window.DataContext = source;
            PrepareBehavior("Command", "Operation2");
            Assert.AreEqual("CanExecuteOperation2", source.CanExecuteResult);
        }
        [Test]
        public void UseDataContextIfSourceNotSpecified_Test02() {
            var source = new FCBObject();
            Window.DataContext = source;
            FCBButton wce = new FCBButton();
            Window.Content = wce;
            PrepareBehavior("Command", "Operation2", null, null, null, false);
            Interaction.GetBehaviors(wce).Add(Behavior);
            Assert.IsNotNull(wce.Command);
        }
        [Test]
        public void UseDataContextIfSourceNotSpecified_Test03() {
            var source = new FCBObject();
            FCBButton wce = new FCBButton();
            wce.DataContext = source;
            Window.Content = wce;
            PrepareBehavior("Command", "Operation2", null, null, null, false);
            Interaction.GetBehaviors(wce).Add(Behavior);
            Assert.IsNotNull(wce.Command);
        }
        [Test]
        public void UseCustomCanExecuteFunction_Test() {
            var source = new FCBObject();
            PrepareBehavior("Command", "Operation2", source, "CanExecutePublic");
            Behavior.Arg1 = false;
            Assert.AreEqual("CanExecutePublic", source.CanExecuteResult);
            Assert.IsFalse(Button.IsEnabled);
            Behavior.Arg1 = true;
            Assert.AreEqual("CanExecutePublic", source.CanExecuteResult);
            Assert.IsTrue(Button.IsEnabled);
        }
        [Test]
        public void SetCommandPropertyOnlyWhenAttached_Test() {
            PrepareBehavior("Command", "Operation1", new FCBObject(), null, null, false);
            Assert.IsNull(Button.Command);
            Interaction.GetBehaviors(Button).Add(Behavior);
            Assert.IsNotNull(Button.Command);
            Interaction.GetBehaviors(Button).Remove(Behavior);
            Assert.IsNull(Button.Command);
        }
        [Test]
        public void ExecuteCommand_Test00() {
            var source = new FCBObject();
            PrepareBehavior("Command", "Operation2", source);
            ClickButton();
            Assert.AreEqual("Operation2", source.ExecuteResult);
        }
        [Test]
        public void ExecuteCommand_Test01() {
            var source = new FCBObject();
            PrepareBehavior("Command", "Operation3", source);
            ClickButton();
            Assert.AreEqual(null, source.ExecuteResult);
            Behavior.Arg1 = "Test";
            Assert.AreEqual(null, source.ExecuteResult);
            ClickButton();
            Assert.AreEqual("Test", source.ExecuteResult);
        }
        [Test]
        public void ExecuteCommand_Test02() {
            var source = new FCBObject();
            PrepareBehavior("Command", "Operation4", source);
            ClickButton();
            Assert.AreEqual(null, source.ExecuteResult);
            Behavior.Arg5 = "1";
            ClickButton();
            Assert.AreEqual(null, source.ExecuteResult);
            Behavior.Arg1 = "{0},{1},{2},{3}";
            ClickButton();
            Assert.AreEqual(",,,1", source.ExecuteResult);
            Behavior.Arg3 = "4";
            ClickButton();
            Assert.AreEqual(",4,,1", source.ExecuteResult);
        }
        [Test]
        public void UpdateFunctionCommandFromPoco_Test() {
            var source = FCBPocoObject.Create();
            PrepareBehavior("Command", "Operation2", source, "CanExecuteCustom");
            Assert.AreEqual(null, source.CanExecuteResult);
            source.Result = "test";
            Assert.AreEqual(null, source.CanExecuteResult);
            source.UpdateMethodToCommandCanExecute(x => x.Operation3());
            Assert.AreEqual(null, source.CanExecuteResult);
            source.UpdateMethodToCommandCanExecute(x => x.Operation2());
            Assert.AreEqual("test", source.CanExecuteResult);
        }
        [Test]
        public void UseMethodDefaultParamsIfCommandParameterEmpty_Test00() {
            var source = new FCBObject();
            PrepareBehavior("Command", "Operation5", source, null, null);
            Assert.IsNull(source.CanExecuteResult);
            Assert.IsTrue(Button.IsEnabled);
            Assert.IsNull(Button.CommandParameter);
            ClickButton();
            Assert.AreEqual("7", source.ExecuteResult);
        }
        [Test]
        public void UseMethodDefaultParamsIfCommandParameterEmpty_Test01() {
            var source = new FCBObject();
            FCBButton wce = new FCBButton();
            wce.DataContext = source;
            Window.Content = wce;
            PrepareBehavior("Command", "Operation5", null, null, null, false);
            Interaction.GetBehaviors(wce).Add(Behavior);
            Assert.IsNull(source.CanExecuteResult);
            Assert.IsTrue(Button.IsEnabled);
            Assert.IsNull(Button.CommandParameter);
            wce.Command.Execute(null);
            Assert.AreEqual("7", source.ExecuteResult);
        }
        [Test]
        public void DataContextShouldNotAffectBehaviorResultIfSourceSet_Test() {
            var source = new FCBObject();
            PrepareBehavior("Command", "Operation2", source);
            Assert.AreEqual("CanExecuteOperation2", source.CanExecuteResult);
            source.ClearResult();
            Assert.IsNull(source.CanExecuteResult);
            Button.DataContext = new FCBObject();
            Assert.IsNull(source.CanExecuteResult);
            Behavior.Source = null;
            Assert.AreEqual("CanExecuteOperation2", (Button.DataContext as FCBObject).CanExecuteResult);
        }
        [Test]
        public void SetTargetProperty_Test00() {
            var source = new FCBObject();
            PrepareBehavior("Command", "Operation4", source, attachToView: false);
            var target = new Button();
            Behavior.Target = target;
            Interaction.GetBehaviors(Button).Add(Behavior);
            Assert.IsNotNull(target.Command);
            Assert.IsNotNull(target.CommandParameter);
            Assert.IsNull(Button.Command);
            Assert.IsNull(Button.CommandParameter);
        }
        [Test]
        public void SetTargetProperty_Test01() {
            var source = new FCBObject();
            PrepareBehavior("Command", "Operation4", source);
            Assert.IsNotNull(Button.Command);
            Assert.IsNotNull(Button.CommandParameter);
            Interaction.GetBehaviors(Button).Remove(Behavior);
            Assert.IsNull(Button.Command);
            Assert.IsNull(Button.CommandParameter);
        }
        [Test]
        public void SetTargetProperty_Test02() {
            var source = new FCBObject();
            PrepareBehavior("Command", "Operation4", source, attachToView: false);
            var target = new Button();
            Behavior.Target = target;
            Interaction.GetBehaviors(Button).Add(Behavior);
            Assert.IsNotNull(target.Command);
            Assert.IsNotNull(target.CommandParameter);
            Assert.IsNull(Button.Command);
            Assert.IsNull(Button.CommandParameter);
            Interaction.GetBehaviors(Button).Remove(Behavior);
            Assert.IsNull(target.Command);
            Assert.IsNull(target.CommandParameter);
        }
    }

    public class FCBPocoObject {
        public static FCBPocoObject Create() {
            return ViewModelSource.Create(() => new FCBPocoObject());
        }
        protected FCBPocoObject() { }
        public string CanExecuteResult { get; private set; }
        public string Result { get; set; }
        [Command(false)]
        public void Operation2() {
        }
        [Command(false)]
        public void Operation3() {
        }
        protected bool CanExecuteCustom() {
            CanExecuteResult = Result;
            return true;
        }
    }
    class FCBObject {
        public string ExecuteResult { get; private set; }
        public string CanExecuteResult { get; private set; }
        public void ClearResult() {
            ExecuteResult = null;
            CanExecuteResult = null;
        }
        public int Operation1() {
            ExecuteResult = "Operation1";
            return 0;
        }
        public void Operation2() {
            ExecuteResult = "Operation2";
        }
        public bool CanExecuteOperation2() {
            CanExecuteResult = "CanExecuteOperation2";
            return true;
        }
        public bool CanExecutePublic(bool result) {
            CanExecuteResult = "CanExecutePublic";
            return result;
        }
        public void Operation3(string value) {
            ExecuteResult = value;
        }
        int Operation4(string patern, string val1, string val2, string val3, string val4) {
            ExecuteResult = string.IsNullOrEmpty(patern) ? null : string.Format(patern, GetVal(val1), GetVal(val2), GetVal(val3), GetVal(val4));
            return 1;
        }
        public void Operation5(int a, double b, double c = 3, double d = 4) {
            ExecuteResult = (a + (int)(b + c + d)).ToString();
        }
        static string GetVal(string st) {
            return st ?? string.Empty;
        }
    }
    class FCBButton : FrameworkContentElement {
        public static readonly DependencyProperty CommandProperty =
            DependencyProperty.Register("Command", typeof(ICommand), typeof(FCBButton), new PropertyMetadata(null));

        public ICommand Command {
            get { return (ICommand)GetValue(CommandProperty); }
            set { SetValue(CommandProperty, value); }
        }
    }
}