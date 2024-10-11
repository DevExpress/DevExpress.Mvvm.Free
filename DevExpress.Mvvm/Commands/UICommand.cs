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

    public enum AsyncDisplayMode {
        None,
        Wait,
        WaitCancel
    }

    public enum GlyphAlignment {
        Left,
        Top,
        Right,
        Bottom
    }

    public class UICommand : BindableBase, IUICommand {
        object id = null;
        public object Id {
            get { return id; }
            set { SetProperty(ref id, value, nameof(Id)); }
        }
        object caption = null;
        public object Caption {
            get { return caption; }
            set {
                SetProperty(ref caption, value, nameof(Caption)); }
        }
        ICommand command = null;
        public ICommand Command {
            get { return command; }
            set { SetProperty(ref command, value, nameof(Command)); }
        }
        bool isDefault = false;
        public bool IsDefault {
            get { return isDefault; }
            set { SetProperty(ref isDefault, value, nameof(IsDefault)); }
        }
        bool isCancel = false;
        public bool IsCancel {
            get { return isCancel; }
            set { SetProperty(ref isCancel, value, nameof(IsCancel)); }
        }
        object tag = null;
        public object Tag {
            get { return tag; }
            set { SetProperty(ref tag, value, nameof(Tag)); }
        }
        bool allowCloseWindow = true;

        public bool AllowCloseWindow {
            get {
                return  allowCloseWindow;
            }
            set { SetProperty(ref allowCloseWindow, value, nameof(AllowCloseWindow)); }
        }

        AsyncDisplayMode asyncDisplayMode = AsyncDisplayMode.None;
        public AsyncDisplayMode AsyncDisplayMode {
            get {
                return asyncDisplayMode;
            }
            set { SetProperty(ref asyncDisplayMode, value, nameof(AsyncDisplayMode)); }
        }

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public DialogButtonAlignment ActualAlignment {
            get {
                if(alignment == DialogButtonAlignment.Right) {
                    if(placement != null) {
                        var dockType = placement.GetType();
                        if(dockType.FullName == "System.Windows.Controls.Dock") {
                            var dock = Enum.GetName(dockType, placement);
                            if(dock == nameof(DialogButtonAlignment.Left))
                    return DialogButtonAlignment.Left;
                        }
                    }
                }
                return alignment;
            }
        }

        DialogButtonAlignment alignment = DialogButtonAlignment.Right;
        public DialogButtonAlignment Alignment {
            get { return alignment; }
            set { SetProperty(ref alignment, value, nameof(Alignment)); }
        }

        object glyph = null;
        public object Glyph {
            get { return glyph; }
            set { SetProperty(ref glyph, value, nameof(Glyph)); }
        }

        GlyphAlignment glyphAlignment = GlyphAlignment.Left;
        public GlyphAlignment GlyphAlignment {
            get { return glyphAlignment; }
            set { SetProperty(ref glyphAlignment, value, nameof(GlyphAlignment)); }
        }

        object placement = null;
        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public object Placement {
            get { return placement; }
            set { SetProperty(ref placement, value, nameof(Placement)); }
        }
        public UICommand() { }
        public UICommand(object id, object caption, ICommand command) {
            this.id = id;
            this.caption = caption;
            this.command = command;
        }
        public UICommand(object id, object caption, ICommand<CancelEventArgs> command) : this(id, caption, (ICommand)command) {
        }
        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public UICommand(object id, object caption, ICommand command, bool isDefault, bool isCancel, object tag = null, bool allowCloseWindow = true, object placement = null, DialogButtonAlignment alignment = DialogButtonAlignment.Right, AsyncDisplayMode asyncDisplayMode = AsyncDisplayMode.None, object glyph = null, GlyphAlignment glyphAlignment = GlyphAlignment.Left)
            : this(id, caption, command) {
            this.isDefault = isDefault;
            this.isCancel = isCancel;
            this.tag = tag;
            this.allowCloseWindow = allowCloseWindow;
            this.placement = placement;
            this.alignment = alignment;
            this.glyph = glyph;
            this.glyphAlignment = glyphAlignment;
            AsyncDisplayMode = asyncDisplayMode;
        }
        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public UICommand(object id, object caption, ICommand<CancelEventArgs> command, bool isDefault, bool isCancel, object tag = null, bool allowCloseWindow = true, object placement = null, DialogButtonAlignment alignment = DialogButtonAlignment.Right, AsyncDisplayMode asyncDisplayMode = AsyncDisplayMode.None, object glyph = null, GlyphAlignment glyphAlignment = GlyphAlignment.Left) 
            : this(id: id,
                  caption: caption,
                  command: (ICommand)command,
                  isDefault: isDefault,
                  isCancel: isCancel,
                  tag: tag,
                  allowCloseWindow:
                  allowCloseWindow,
                  placement: placement,
                  alignment: alignment,
                  glyph: glyph,
                  glyphAlignment: glyphAlignment,
                  asyncDisplayMode: asyncDisplayMode) {
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
                this.tag = tag;
                this.caption = caption;
            }
        }
        #endregion DefaultButtonCommand
    }
}