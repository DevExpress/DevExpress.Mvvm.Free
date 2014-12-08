using DevExpress.Mvvm;
using DevExpress.Mvvm.Native;
using DevExpress.Mvvm.UI;
using DevExpress.Mvvm.UI.Interactivity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
#if NETFX_CORE
using System.Threading.Tasks;
using Windows.UI.Popups;
#else
using System.Windows.Controls;
#endif
#if SILVERLIGHT
namespace DevExpress.Mvvm.UI {
    public class MessageBoxService : ServiceBase, IMessageBoxService {
        MessageResult IMessageBoxService.Show(string messageBoxText, string caption, MessageButton button, MessageResult defaultResult) {
            return MessageBox.Show(messageBoxText, caption, button.ToMessageBoxButton()).ToMessageResult();
        }
    }
}
#elif NETFX_CORE
namespace DevExpress.Mvvm.UI {
    public class MessageBoxService : ServiceBase, IMessageBoxService {
        public async Task<UICommand> ShowAsync(string messageBoxText, string caption, IList<UICommand> commands) {
            MessageDialog messageDialog = new MessageDialog(messageBoxText, caption);
            uint defaultCommandIndex = 0;
            uint cancelCommandIndex = uint.MaxValue;
            uint commandIndex = 0;
            foreach(var command in commands) {
                if(command.IsCancel)
                    cancelCommandIndex = commandIndex;
                if(command.IsDefault)
                    defaultCommandIndex = commandIndex;
                messageDialog.Commands.Add(ConvertToUIPopupCommand(command));
                commandIndex++;
            }
            messageDialog.DefaultCommandIndex = defaultCommandIndex;
            messageDialog.CancelCommandIndex = cancelCommandIndex;
            var result = await messageDialog.ShowAsync().AsTask();
            return commands.FirstOrDefault(c => c.Id == result.Id || c == result.Id);
        }
        IUICommand ConvertToUIPopupCommand(UICommand command) {
            return new Windows.UI.Popups.UICommand((string)command.Caption, ConvertToAction(command), command.Id ?? command);
        }
        UICommandInvokedHandler ConvertToAction(UICommand command) {
            return new UICommandInvokedHandler((d) => { command.Command.If(x => x.CanExecute(command)).Do(y => y.Execute(command)); });
        }
    }
}
#else
namespace DevExpress.Mvvm.UI {
    public class MessageBoxService : ServiceBase, IMessageBoxService {
        MessageResult IMessageBoxService.Show(string messageBoxText, string caption, MessageButton button, MessageIcon icon, MessageResult defaultResult) {
            var owner = AssociatedObject.With(x => Window.GetWindow(x));
            if(owner == null)
                return MessageBox.Show(messageBoxText, caption, button.ToMessageBoxButton(), icon.ToMessageBoxImage(), defaultResult.ToMessageBoxResult()).ToMessageResult();
            else
                return MessageBox.Show(owner, messageBoxText, caption, button.ToMessageBoxButton(), icon.ToMessageBoxImage(), defaultResult.ToMessageBoxResult()).ToMessageResult();
        }
    }
}
#endif