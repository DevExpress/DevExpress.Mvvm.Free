using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DevExpress.Mvvm {
    public static class MessageBoxServiceExtensions {
        public static async Task<MessageResult> ShowAsync(this IMessageBoxService service, string messageBoxText) {
            return await service.ShowAsync(messageBoxText, string.Empty);
        }
        public static async Task<MessageResult> ShowAsync(this IMessageBoxService service, string messageBoxText, string caption) {
            return await service.ShowAsync(messageBoxText, caption, MessageButton.OK);
        }
        public static async Task<MessageResult> ShowAsync(this IMessageBoxService service, string messageBoxText, string caption, MessageButton button) {
            return await service.ShowAsync(messageBoxText, caption, button, MessageResult.OK);
        }
        public static async Task<MessageResult> ShowAsync(this IMessageBoxService service, string messageBoxText, string caption, MessageButton button, MessageResult defaultResult) {
            return await service.ShowAsync(messageBoxText, caption, button, defaultResult, MessageResult.OK);
        }
        public static async Task<MessageResult> ShowAsync(this IMessageBoxService service, string messageBoxText, string caption, MessageButton button, MessageResult defaultResult, MessageResult cancelResult) {
            VerifyService(service);
            var buttons = UICommand.GenerateFromMessageButton(button, GetLocalizer(service), defaultResult, cancelResult);
            UICommand uicommand = await service.ShowAsync(messageBoxText, caption, buttons);
            return (MessageResult)uicommand.Id;
        }
        static void VerifyService(IMessageBoxService service) {
            if(service == null)
                throw new ArgumentNullException("service");
        }
        static IMessageButtonLocalizer GetLocalizer(IMessageBoxService service) {
            return service as IMessageButtonLocalizer ?? new DefaultMessageButtonLocalizer();
        }
    }
}