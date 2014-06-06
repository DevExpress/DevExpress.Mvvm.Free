#if SILVERLIGHT
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MessageBoxButton = DevExpress.Mvvm.DXMessageBoxButton;
#else
using NUnit.Framework;
#endif
using System.ComponentModel;
using DevExpress.Mvvm.Native;
using System.Collections.Generic;
using System.Windows;

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
                if(e.PropertyName == ExpressionHelper.GetPropertyName(() => command.Id))
                    idChanged = true;
                if(e.PropertyName == ExpressionHelper.GetPropertyName(() => command.Command))
                    commandChanged = true;
                if(e.PropertyName == ExpressionHelper.GetPropertyName(() => command.Tag))
                    tagChanged = true;
                if(e.PropertyName == ExpressionHelper.GetPropertyName(() => command.Caption))
                    labelChanged = true;
                if(e.PropertyName == ExpressionHelper.GetPropertyName(() => command.IsDefault))
                    isDefaultChanged = true;
                if(e.PropertyName == ExpressionHelper.GetPropertyName(() => command.IsCancel))
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

        [Test]
        public void GenerateUICommandsTest1() {
            IList<UICommand> commands;

            commands = GenerateUICommandsCore(MessageBoxButton.OK);
            Assert.AreEqual(1, commands.Count);
            Assert.AreEqual("OK", commands[0].Caption);
            Assert.AreEqual(true, commands[0].IsDefault);
            Assert.AreEqual(false, commands[0].IsCancel);

            commands = GenerateUICommandsCore(MessageBoxButton.OKCancel);
            Assert.AreEqual(2, commands.Count);
            Assert.AreEqual("OK", commands[0].Caption);
            Assert.AreEqual(true, commands[0].IsDefault);
            Assert.AreEqual(false, commands[0].IsCancel);
            Assert.AreEqual("Cancel", commands[1].Caption);
            Assert.AreEqual(false, commands[1].IsDefault);
            Assert.AreEqual(true, commands[1].IsCancel);

            commands = GenerateUICommandsCore(MessageBoxButton.YesNo);
            Assert.AreEqual(2, commands.Count);
            Assert.AreEqual("Yes", commands[0].Caption);
            Assert.AreEqual(true, commands[0].IsDefault);
            Assert.AreEqual(false, commands[0].IsCancel);
            Assert.AreEqual("No", commands[1].Caption);
            Assert.AreEqual(false, commands[1].IsDefault);
            Assert.AreEqual(true, commands[1].IsCancel);

            commands = GenerateUICommandsCore(MessageBoxButton.YesNoCancel);
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

            commands = GenerateUICommandsCore(MessageBoxButton.OK, MessageBoxResult.Cancel, MessageBoxResult.OK);
            Assert.AreEqual(1, commands.Count);
            Assert.AreEqual("OK", commands[0].Caption);
            Assert.AreEqual(false, commands[0].IsDefault);
            Assert.AreEqual(true, commands[0].IsCancel);

            commands = GenerateUICommandsCore(MessageBoxButton.OKCancel, MessageBoxResult.Cancel, MessageBoxResult.No);
            Assert.AreEqual(2, commands.Count);
            Assert.AreEqual("OK", commands[0].Caption);
            Assert.AreEqual(false, commands[0].IsDefault);
            Assert.AreEqual(false, commands[0].IsCancel);
            Assert.AreEqual("Cancel", commands[1].Caption);
            Assert.AreEqual(true, commands[1].IsDefault);
            Assert.AreEqual(false, commands[1].IsCancel);

            commands = GenerateUICommandsCore(MessageBoxButton.YesNo, MessageBoxResult.OK, MessageBoxResult.Yes);
            Assert.AreEqual(2, commands.Count);
            Assert.AreEqual("Yes", commands[0].Caption);
            Assert.AreEqual(false, commands[0].IsDefault);
            Assert.AreEqual(true, commands[0].IsCancel);
            Assert.AreEqual("No", commands[1].Caption);
            Assert.AreEqual(false, commands[1].IsDefault);
            Assert.AreEqual(false, commands[1].IsCancel);

            commands = GenerateUICommandsCore(MessageBoxButton.YesNoCancel, MessageBoxResult.No, MessageBoxResult.Cancel);
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

        List<UICommand> GenerateUICommandsCore(MessageBoxButton dialogButtons, MessageBoxResult? defaultButton = null, MessageBoxResult? cancelButton = null) {
            return UICommand.GenerateFromMessageBoxButton(dialogButtons, new DefaultMessageBoxButtonLocalizer(), defaultButton, cancelButton);
        }
    }
}