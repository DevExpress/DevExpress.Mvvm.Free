using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using DevExpress.Mvvm.Native;
using NUnit.Framework;

namespace DevExpress.Mvvm.Tests {
    [TestFixture]
    public class UICommandTests {
        [Test]
        public void InitValuesTest() {
            UICommand command = new UICommand();
            Assert.AreEqual(null, command.Id);
            Assert.AreEqual(null, command.Command);
            Assert.AreEqual(null, command.Caption);
            Assert.AreEqual(null, command.Tag);
            Assert.AreEqual(false, command.IsCancel);
            Assert.AreEqual(false, command.IsDefault);
        }
        [Test]
        public void ConstructorTest() {
            DelegateCommand c = new DelegateCommand(() => { });
            UICommand command = new UICommand(0, "label", c, true, true, 0);
            Assert.AreEqual(0, command.Id);
            Assert.AreEqual(c, command.Command);
            Assert.AreEqual("label", command.Caption);
            Assert.AreEqual(0, command.Tag);
            Assert.AreEqual(true, command.IsCancel);
            Assert.AreEqual(true, command.IsDefault);
        }
        [Test]
        public void PropertyChangedTest() {
            UICommand command = new UICommand();

            bool idChanged = false;
            bool commandChanged = false;
            bool labelChanged = false;
            bool tagChanged = false;
            bool isCancelChanged = false;
            bool isDefaultChanged = false;

            PropertyChangedEventHandler propertyChanged = (s, e) => {
                if(e.PropertyName == GetPropertyName(() => command.Id))
                    idChanged = true;
                if(e.PropertyName == GetPropertyName(() => command.Command))
                    commandChanged = true;
                if(e.PropertyName == GetPropertyName(() => command.Tag))
                    tagChanged = true;
                if(e.PropertyName == GetPropertyName(() => command.Caption))
                    labelChanged = true;
                if(e.PropertyName == GetPropertyName(() => command.IsDefault))
                    isDefaultChanged = true;
                if(e.PropertyName == GetPropertyName(() => command.IsCancel))
                    isCancelChanged = true;
            };
            command.PropertyChanged += propertyChanged;

            command.Id = 0;
            Assert.IsTrue(idChanged);
            command.Command = new DelegateCommand(() => { });
            Assert.IsTrue(commandChanged);
            command.Tag = 0;
            Assert.IsTrue(tagChanged);
            command.IsCancel = true;
            Assert.IsTrue(isCancelChanged);
            command.IsDefault = true;
            Assert.IsTrue(isDefaultChanged);
            command.Caption = "label";
            Assert.IsTrue(labelChanged);
        }
        static string GetPropertyName<T>(Expression<Func<T>> expression) {
            return (expression.Body as MemberExpression).Member.Name;
        }
        [Test]
        public void GenerateUICommandsTest1() {
            IList<UICommand> commands;

            commands = GenerateUICommandsCore(MessageButton.OK);
            Assert.AreEqual(1, commands.Count);
            Assert.AreEqual("OK", commands[0].Caption);
            Assert.AreEqual(true, commands[0].IsDefault);
            Assert.AreEqual(false, commands[0].IsCancel);

            commands = GenerateUICommandsCore(MessageButton.OKCancel);
            Assert.AreEqual(2, commands.Count);
            Assert.AreEqual("OK", commands[0].Caption);
            Assert.AreEqual(true, commands[0].IsDefault);
            Assert.AreEqual(false, commands[0].IsCancel);
            Assert.AreEqual("Cancel", commands[1].Caption);
            Assert.AreEqual(false, commands[1].IsDefault);
            Assert.AreEqual(true, commands[1].IsCancel);

            commands = GenerateUICommandsCore(MessageButton.YesNo);
            Assert.AreEqual(2, commands.Count);
            Assert.AreEqual("Yes", commands[0].Caption);
            Assert.AreEqual(true, commands[0].IsDefault);
            Assert.AreEqual(false, commands[0].IsCancel);
            Assert.AreEqual("No", commands[1].Caption);
            Assert.AreEqual(false, commands[1].IsDefault);
            Assert.AreEqual(true, commands[1].IsCancel);

            commands = GenerateUICommandsCore(MessageButton.YesNoCancel);
            Assert.AreEqual(3, commands.Count);
            Assert.AreEqual("Yes", commands[0].Caption);
            Assert.AreEqual(true, commands[0].IsDefault);
            Assert.AreEqual(false, commands[0].IsCancel);
            Assert.AreEqual("No", commands[1].Caption);
            Assert.AreEqual(false, commands[1].IsDefault);
            Assert.AreEqual(true, commands[1].IsCancel);
            Assert.AreEqual("Cancel", commands[2].Caption);
            Assert.AreEqual(false, commands[2].IsDefault);
            Assert.AreEqual(true, commands[2].IsCancel);
        }
        [Test]
        public void GenerateUICommandsTest2() {
            IList<UICommand> commands;

            commands = GenerateUICommandsCore(MessageButton.OK, MessageResult.Cancel, MessageResult.OK);
            Assert.AreEqual(1, commands.Count);
            Assert.AreEqual("OK", commands[0].Caption);
            Assert.AreEqual(false, commands[0].IsDefault);
            Assert.AreEqual(true, commands[0].IsCancel);

            commands = GenerateUICommandsCore(MessageButton.OKCancel, MessageResult.Cancel, MessageResult.No);
            Assert.AreEqual(2, commands.Count);
            Assert.AreEqual("OK", commands[0].Caption);
            Assert.AreEqual(false, commands[0].IsDefault);
            Assert.AreEqual(false, commands[0].IsCancel);
            Assert.AreEqual("Cancel", commands[1].Caption);
            Assert.AreEqual(true, commands[1].IsDefault);
            Assert.AreEqual(false, commands[1].IsCancel);

            commands = GenerateUICommandsCore(MessageButton.YesNo, MessageResult.OK, MessageResult.Yes);
            Assert.AreEqual(2, commands.Count);
            Assert.AreEqual("Yes", commands[0].Caption);
            Assert.AreEqual(false, commands[0].IsDefault);
            Assert.AreEqual(true, commands[0].IsCancel);
            Assert.AreEqual("No", commands[1].Caption);
            Assert.AreEqual(false, commands[1].IsDefault);
            Assert.AreEqual(false, commands[1].IsCancel);

            commands = GenerateUICommandsCore(MessageButton.YesNoCancel, MessageResult.No, MessageResult.Cancel);
            Assert.AreEqual(3, commands.Count);
            Assert.AreEqual("Yes", commands[0].Caption);
            Assert.AreEqual(false, commands[0].IsDefault);
            Assert.AreEqual(false, commands[0].IsCancel);
            Assert.AreEqual("No", commands[1].Caption);
            Assert.AreEqual(true, commands[1].IsDefault);
            Assert.AreEqual(false, commands[1].IsCancel);
            Assert.AreEqual("Cancel", commands[2].Caption);
            Assert.AreEqual(false, commands[2].IsDefault);
            Assert.AreEqual(true, commands[2].IsCancel);
        }

        List<UICommand> GenerateUICommandsCore(MessageButton dialogButtons, MessageResult? defaultButton = null, MessageResult? cancelButton = null) {
            return UICommand.GenerateFromMessageButton(dialogButtons, new DefaultMessageButtonLocalizer(), defaultButton, cancelButton);
        }
    }
}