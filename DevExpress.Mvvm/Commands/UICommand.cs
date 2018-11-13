using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using DevExpress.Mvvm.Native;

namespace DevExpress.Mvvm {
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

        public UICommand() { }
        public UICommand(object id, object caption, ICommand command, bool isDefault, bool isCancel, object tag = null) {
            this.id = id;
            this.caption = caption;
            this.command = command;
            this.isDefault = isDefault;
            this.isCancel = isCancel;
            this.tag = tag;
        }

        public static List<UICommand> GenerateFromMessageButton(MessageButton dialogButtons, IMessageButtonLocalizer buttonLocalizer, MessageResult? defaultButton = null, MessageResult? cancelButton = null) {
            return GenerateFromMessageButton(dialogButtons, false, buttonLocalizer, defaultButton, cancelButton);
        }
#if !NETFX_CORE
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
#endif
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
#if NETFX_CORE
            if(dialogButtons == MessageButton.AbortRetryIgnore) {
                UICommand abortCommand = CreateDefaultButtonCommand(MessageResult.Abort, usePlatformSpecificTag, buttonLocalizer.Localize);
                UICommand retryCommand = CreateDefaultButtonCommand(MessageResult.Retry, usePlatformSpecificTag, buttonLocalizer.Localize);
                UICommand ignoreCommand = CreateDefaultButtonCommand(MessageResult.Ignore, usePlatformSpecificTag, buttonLocalizer.Localize);
                abortCommand.IsDefault = defaultButton == null || defaultButton == MessageResult.Abort;
                retryCommand.IsDefault = defaultButton == MessageResult.Retry;
                ignoreCommand.IsDefault = defaultButton == MessageResult.Ignore;
                abortCommand.IsCancel = cancelButton == MessageResult.Abort;
                retryCommand.IsCancel = cancelButton == null || cancelButton == MessageResult.Retry;
                ignoreCommand.IsCancel = cancelButton == null || cancelButton == MessageResult.Ignore;

                commands.Add(abortCommand);
                commands.Add(retryCommand);
                commands.Add(ignoreCommand);
                return commands;
            }
            if(dialogButtons == MessageButton.Close) {
                UICommand closeCommand = CreateDefaultButtonCommand(MessageResult.Close, usePlatformSpecificTag, buttonLocalizer.Localize);
                closeCommand.IsDefault = defaultButton == null || defaultButton == MessageResult.Close;
                closeCommand.IsCancel = cancelButton == MessageResult.Close;

                commands.Add(closeCommand);
                return commands;
            }
            if(dialogButtons == MessageButton.RetryCancel) {
                UICommand retryCommand = CreateDefaultButtonCommand(MessageResult.Retry, usePlatformSpecificTag, buttonLocalizer.Localize);
                UICommand cancelCommand = CreateDefaultButtonCommand(MessageResult.Cancel, usePlatformSpecificTag, buttonLocalizer.Localize);
                retryCommand.IsDefault = defaultButton == null || defaultButton == MessageResult.Retry;
                cancelCommand.IsDefault = defaultButton == MessageResult.Cancel;
                retryCommand.IsCancel = cancelButton == MessageResult.Retry;
                cancelCommand.IsCancel = cancelButton == null || cancelButton == MessageResult.Cancel;

                commands.Add(retryCommand);
                commands.Add(cancelCommand);
                return commands;
            }
#endif
            return commands;
        }
        static UICommand CreateDefaultButtonCommand(MessageResult result, bool usePlatformSpecificTag, Func<MessageResult, string> getButtonCaption) {
#if !NETFX_CORE
            object tag = usePlatformSpecificTag ? result.ToMessageBoxResult() : (object)result;
#else
            object tag = (object)result;
#endif
            return new DefaultButtonCommand(tag, getButtonCaption(result), tag);
        }
        #region IUICommand
        EventHandler executed;
        event EventHandler IUICommand.Executed {
            add { executed += value;  }
            remove { executed -= value;            }
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