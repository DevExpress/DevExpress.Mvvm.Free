#if SILVERLIGHT
using MessageBoxButton = DevExpress.Mvvm.DXMessageBoxButton;
#endif
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;

namespace DevExpress.Mvvm {
    public class UICommand : BindableBase {
        object id = null;
        public object Id {
            get { return id; }
            set { SetProperty(ref id, value, () => Id); }
        }
        object caption = null;
        public object Caption {
            get { return caption; }
            set { SetProperty(ref caption, value, () => Caption); }
        }
        ICommand command = null;
        public ICommand Command {
            get { return command; }
            set { SetProperty(ref command, value, () => Command); }
        }
        bool isDefault = false;
        public bool IsDefault {
            get { return isDefault; }
            set { SetProperty(ref isDefault, value, () => IsDefault); }
        }
        bool isCancel = false;
        public bool IsCancel {
            get { return isCancel; }
            set { SetProperty(ref isCancel, value, () => IsCancel); }
        }
        object tag = null;
        public object Tag {
            get { return tag; }
            set { SetProperty(ref tag, value, () => Tag); }
        }

        public UICommand() { }
        public UICommand(object id, object caption, ICommand command, bool isDefault, bool isCancel, object tag = null) {
            this.id = id;
            this.caption = caption;
            this.command = command;
            this.isDefault = isDefault;
            this.isCancel = isCancel;
            this.tag = tag;
        }

        public static List<UICommand> GenerateFromMessageBoxButton(MessageBoxButton dialogButtons, IMessageBoxButtonLocalizer buttonLocalizer, MessageBoxResult? defaultButton = null, MessageBoxResult? cancelButton = null) {
            List<UICommand> commands = new List<UICommand>();
            if(dialogButtons == MessageBoxButton.OK) {
                UICommand okCommand = CreateDefaultButonCommand(MessageBoxResult.OK, buttonLocalizer.Localize);
                okCommand.IsDefault = defaultButton == null || defaultButton == MessageBoxResult.OK;
                okCommand.IsCancel = cancelButton == MessageBoxResult.OK;

                commands.Add(okCommand);
                return commands;
            }
            if(dialogButtons == MessageBoxButton.OKCancel) {
                UICommand okCommand = CreateDefaultButonCommand(MessageBoxResult.OK, buttonLocalizer.Localize);
                UICommand cancelCommand = CreateDefaultButonCommand(MessageBoxResult.Cancel, buttonLocalizer.Localize);
                okCommand.IsDefault = defaultButton == null || defaultButton == MessageBoxResult.OK;
                cancelCommand.IsDefault = defaultButton == MessageBoxResult.Cancel;
                okCommand.IsCancel = cancelButton == MessageBoxResult.OK;
                cancelCommand.IsCancel = cancelButton == null || cancelButton == MessageBoxResult.Cancel;

                commands.Add(okCommand);
                commands.Add(cancelCommand);
                return commands;
            }
            if(dialogButtons == MessageBoxButton.YesNo) {
                UICommand yesCommand = CreateDefaultButonCommand(MessageBoxResult.Yes, buttonLocalizer.Localize);
                UICommand noCommand = CreateDefaultButonCommand(MessageBoxResult.No, buttonLocalizer.Localize);
                yesCommand.IsDefault = defaultButton == null || defaultButton == MessageBoxResult.Yes;
                noCommand.IsDefault = defaultButton == MessageBoxResult.No;
                yesCommand.IsCancel = cancelButton == MessageBoxResult.Yes;
                noCommand.IsCancel = cancelButton == null || cancelButton == MessageBoxResult.No;

                commands.Add(yesCommand);
                commands.Add(noCommand);
                return commands;
            }
            if(dialogButtons == MessageBoxButton.YesNoCancel) {
                UICommand yesCommand = CreateDefaultButonCommand(MessageBoxResult.Yes, buttonLocalizer.Localize);
                UICommand noCommand = CreateDefaultButonCommand(MessageBoxResult.No, buttonLocalizer.Localize);
                UICommand cancelCommand = CreateDefaultButonCommand(MessageBoxResult.Cancel, buttonLocalizer.Localize);
                yesCommand.IsDefault = defaultButton == null || defaultButton == MessageBoxResult.Yes;
                noCommand.IsDefault = defaultButton == MessageBoxResult.No;
                cancelCommand.IsDefault = defaultButton == MessageBoxResult.Cancel;
                yesCommand.IsCancel = cancelButton == MessageBoxResult.Yes;
                noCommand.IsCancel = cancelButton == null || cancelButton == MessageBoxResult.No;
                cancelCommand.IsCancel = cancelButton == null || cancelButton == MessageBoxResult.Cancel;

                commands.Add(yesCommand);
                commands.Add(noCommand);
                commands.Add(cancelCommand);
                return commands;
            }
            return commands;
        }
        static UICommand CreateDefaultButonCommand(MessageBoxResult result, Func<MessageBoxResult, string> getButtonCaption) {
            return new UICommand() {
                Id = result,
                Caption = getButtonCaption(result),
                Command = null,
                Tag = result,
            };
        }
    }
}