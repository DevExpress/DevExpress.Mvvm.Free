using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using DevExpress.Mvvm.Native;

namespace DevExpress.Mvvm {
    public enum DialogButtonAlignment {
        Right,
        Center,
        Left,
    }

    public class UICommand : BindableBase, IUICommand {
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
        bool allowCloseWindow = true;
        public bool AllowCloseWindow {
            get { return allowCloseWindow; }
            set { SetProperty(ref allowCloseWindow, value, () => AllowCloseWindow); }
        }

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public DialogButtonAlignment ActualAlignment {
            get {
                if(alignment != DialogButtonAlignment.Right)
                    return alignment;
                if(placement != Dock.Right && placement.Equals(Dock.Left))
                    return DialogButtonAlignment.Left;
                return alignment;
            }
        }

        DialogButtonAlignment alignment = DialogButtonAlignment.Right;
        public DialogButtonAlignment Alignment {
            get { return alignment; }
            set { SetProperty(ref alignment, value, () => Alignment); }
        }

        Dock placement = Dock.Right;

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public Dock Placement {
            get { return placement; }
            set { SetProperty(ref placement, value, () => Placement); }
        }
        public UICommand() { }
        public UICommand(object id, object caption, ICommand command, bool isDefault, bool isCancel, object tag = null, bool allowCloseWindow = true, Dock placement = Dock.Right, DialogButtonAlignment alignment = DialogButtonAlignment.Right) {
            this.id = id;
            this.caption = caption;
            this.command = command;
            this.isDefault = isDefault;
            this.isCancel = isCancel;
            this.tag = tag;
            this.allowCloseWindow = allowCloseWindow;
            this.placement = placement;
            this.alignment = alignment;
        }

        public static List<UICommand> GenerateFromMessageButton(MessageButton dialogButtons, IMessageButtonLocalizer buttonLocalizer, MessageResult? defaultButton = null, MessageResult? cancelButton = null) {
            return GenerateFromMessageButton(dialogButtons, false, buttonLocalizer, defaultButton, cancelButton);
        }
        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public static List<UICommand> GenerateFromMessageBoxButton(MessageBoxButton dialogButtons, IMessageBoxButtonLocalizer buttonLocalizer, MessageBoxResult? defaultButton = null, MessageBoxResult? cancelButton = null) {
            return GenerateFromMessageBoxButton(dialogButtons, buttonLocalizer.ToMessageButtonLocalizer(), defaultButton, cancelButton);
        }
        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public static List<UICommand> GenerateFromMessageBoxButton(MessageBoxButton dialogButtons, IMessageButtonLocalizer buttonLocalizer, MessageBoxResult? defaultButton = null, MessageBoxResult? cancelButton = null) {
            MessageResult? defaultResult = defaultButton == null ? (MessageResult?)null : defaultButton.Value.ToMessageResult();
            MessageResult? cancelResult = cancelButton == null ? (MessageResult?)null : cancelButton.Value.ToMessageResult();
            return GenerateFromMessageButton(dialogButtons.ToMessageButton(), true, buttonLocalizer, defaultResult, cancelResult);
        }
        static List<UICommand> GenerateFromMessageButton(MessageButton dialogButtons, bool usePlatformSpecificTag, IMessageButtonLocalizer buttonLocalizer, MessageResult? defaultButton, MessageResult? cancelButton) {
            List<UICommand> commands = new List<UICommand>();
            if(dialogButtons == MessageButton.OK) {
                UICommand okCommand = CreateDefaultButtonCommand(MessageResult.OK, usePlatformSpecificTag, buttonLocalizer.Localize);
                okCommand.IsDefault = defaultButton == null || defaultButton == MessageResult.OK;
                okCommand.IsCancel = cancelButton == MessageResult.OK;

                commands.Add(okCommand);
                return commands;
            }
            if(dialogButtons == MessageButton.OKCancel) {
                UICommand okCommand = CreateDefaultButtonCommand(MessageResult.OK, usePlatformSpecificTag, buttonLocalizer.Localize);
                UICommand cancelCommand = CreateDefaultButtonCommand(MessageResult.Cancel, usePlatformSpecificTag, buttonLocalizer.Localize);
                okCommand.IsDefault = defaultButton == null || defaultButton == MessageResult.OK;
                cancelCommand.IsDefault = defaultButton == MessageResult.Cancel;
                okCommand.IsCancel = cancelButton == MessageResult.OK;
                cancelCommand.IsCancel = cancelButton == null || cancelButton == MessageResult.Cancel;

                commands.Add(okCommand);
                commands.Add(cancelCommand);
                return commands;
            }
            if(dialogButtons == MessageButton.YesNo) {
                UICommand yesCommand = CreateDefaultButtonCommand(MessageResult.Yes, usePlatformSpecificTag, buttonLocalizer.Localize);
                UICommand noCommand = CreateDefaultButtonCommand(MessageResult.No, usePlatformSpecificTag, buttonLocalizer.Localize);
                yesCommand.IsDefault = defaultButton == null || defaultButton == MessageResult.Yes;
                noCommand.IsDefault = defaultButton == MessageResult.No;
                yesCommand.IsCancel = cancelButton == MessageResult.Yes;
                noCommand.IsCancel = cancelButton == null || cancelButton == MessageResult.No;

                commands.Add(yesCommand);
                commands.Add(noCommand);
                return commands;
            }
            if(dialogButtons == MessageButton.YesNoCancel) {
                UICommand yesCommand = CreateDefaultButtonCommand(MessageResult.Yes, usePlatformSpecificTag, buttonLocalizer.Localize);
                UICommand noCommand = CreateDefaultButtonCommand(MessageResult.No, usePlatformSpecificTag, buttonLocalizer.Localize);
                UICommand cancelCommand = CreateDefaultButtonCommand(MessageResult.Cancel, usePlatformSpecificTag, buttonLocalizer.Localize);
                yesCommand.IsDefault = defaultButton == null || defaultButton == MessageResult.Yes;
                noCommand.IsDefault = defaultButton == MessageResult.No;
                cancelCommand.IsDefault = defaultButton == MessageResult.Cancel;
                yesCommand.IsCancel = cancelButton == MessageResult.Yes;
                noCommand.IsCancel = cancelButton == null || cancelButton == MessageResult.No;
                cancelCommand.IsCancel = cancelButton == null || cancelButton == MessageResult.Cancel;

                commands.Add(yesCommand);
                commands.Add(noCommand);
                commands.Add(cancelCommand);
                return commands;
            }
            return commands;
        }
        static UICommand CreateDefaultButtonCommand(MessageResult result, bool usePlatformSpecificTag, Func<MessageResult, string> getButtonCaption) {
            object tag = usePlatformSpecificTag ? result.ToMessageBoxResult() : (object)result;
            return new DefaultButtonCommand(tag, getButtonCaption(result), tag);
        }
        #region IUICommand
        EventHandler executed;
        event EventHandler IUICommand.Executed {
            add { executed += value; }
            remove { executed -= value; }
        }
        void IUICommand.RaiseExecuted() {
            if(executed != null)
                executed(this, EventArgs.Empty);
        }
        #endregion
        #region DefaultButtonCommand
        class DefaultButtonCommand : UICommand {
            public DefaultButtonCommand(object id, string caption, object tag) {
                this.id = id;
                this.caption = caption;
                this.tag = tag;
            }
        }
        #endregion DefaultButtonCommand
    }
}