using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;

namespace DevExpress.Mvvm {
    public static class DialogServiceExtensions {

        public static Task<UICommand> ShowDialogAsync(this IDialogService service, IEnumerable<UICommand> dialogCommands, string title, object viewModel) {
            VerifyService(service);
            return service.ShowDialogAsync(dialogCommands, title, null, viewModel, null, null);
        }
        public static Task<MessageResult> ShowDialogAsync(this IDialogService service, MessageButton dialogButtons, string title, object viewModel) {
            VerifyService(service);
            var res = service.ShowDialogAsync(UICommand.GenerateFromMessageButton(dialogButtons, GetLocalizer(service)), title, null, viewModel, null, null);
            return GetResult(res);
        }

        public static Task<UICommand> ShowDialogAsync(this IDialogService service, IEnumerable<UICommand> dialogCommands, string title, string documentType, object viewModel) {
            VerifyService(service);
            return service.ShowDialogAsync(dialogCommands, title, documentType, viewModel, null, null);
        }
        public static Task<MessageResult> ShowDialogAsync(this IDialogService service, MessageButton dialogButtons, string title, string documentType, object viewModel) {
            VerifyService(service);
            var res = service.ShowDialogAsync(UICommand.GenerateFromMessageButton(dialogButtons, GetLocalizer(service)), title, documentType, viewModel, null, null);
            return GetResult(res);
        }

        public static Task<UICommand> ShowDialogAsync(this IDialogService service, IEnumerable<UICommand> dialogCommands, string title, string documentType, object parameter, object parentViewModel) {
            VerifyService(service);
            return service.ShowDialogAsync(dialogCommands, title, documentType, null, parameter, parentViewModel);
        }
        public static Task<MessageResult> ShowDialogAsync(this IDialogService service, MessageButton dialogButtons, string title, string documentType, object parameter, object parentViewModel) {
            VerifyService(service);
            var res = service.ShowDialogAsync(UICommand.GenerateFromMessageButton(dialogButtons, GetLocalizer(service)), title, documentType, null, parameter, parentViewModel);
            return GetResult(res);
        }

        static void VerifyService(IDialogService service) {
            if (service == null)
                throw new ArgumentNullException("service");
        }
        static IMessageButtonLocalizer GetLocalizer(IDialogService service) {
            return service as IMessageButtonLocalizer ?? new DefaultMessageButtonLocalizer();
        }
        static Task<MessageResult> GetResult(Task<UICommand> result) {
            if(result == null)
                return null;
            return result.ContinueWith<MessageResult>(x => GetResult(x.Result));
        }
        static MessageResult GetResult(UICommand result) {
            if(result == null)
                return MessageResult.None;
            return (MessageResult)result.Tag;
        }
    }
}