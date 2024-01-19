using DevExpress.Mvvm.Native;
using System.ComponentModel;
using System.Windows;

namespace DevExpress.Mvvm {
    public interface IMessageButtonLocalizer {
        string Localize(MessageResult button);
    }
    [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
    public interface IMessageBoxButtonLocalizer {
        string Localize(MessageBoxResult button);
    }
    public class DefaultMessageButtonLocalizer : IMessageButtonLocalizer {
        public string Localize(MessageResult button) {
            switch(button) {
                case MessageResult.OK:
                    return "OK";
                case MessageResult.Cancel:
                    return "Cancel";
                case MessageResult.Yes:
                    return "Yes";
                case MessageResult.No:
                    return "No";
            }
            return string.Empty;
        }
    }
    [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
    public class DefaultMessageBoxButtonLocalizer : IMessageBoxButtonLocalizer {
        DefaultMessageButtonLocalizer localizer = new DefaultMessageButtonLocalizer();

        public string Localize(MessageBoxResult button) {
            return localizer.Localize(button.ToMessageResult());
        }
    }
    [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
    public static class MessageBoxButtonLocalizerExtensions {
        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public static IMessageButtonLocalizer ToMessageButtonLocalizer(this IMessageBoxButtonLocalizer localizer) {
            return new MessageBoxButtonLocalizerWrapper(localizer);
        }

        class MessageBoxButtonLocalizerWrapper : IMessageButtonLocalizer {
            IMessageBoxButtonLocalizer localizer;

            public MessageBoxButtonLocalizerWrapper(IMessageBoxButtonLocalizer localizer) {
                this.localizer = localizer;
            }
            string IMessageButtonLocalizer.Localize(MessageResult button) {
                return localizer.Localize(button.ToMessageBoxResult());
            }
        }
    }
}