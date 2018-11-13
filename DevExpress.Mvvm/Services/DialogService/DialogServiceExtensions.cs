using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using DevExpress.Mvvm.Native;

namespace DevExpress.Mvvm {
    public static class DialogServiceExtensions {
        public static UICommand ShowDialog(this IDialogService service, IEnumerable<UICommand> dialogCommands, string title, object viewModel) {
            VerifyService(service);
            return service.ShowDialog(dialogCommands, title, null, viewModel, null, null);
        }
        public static MessageResult ShowDialog(this IDialogService service, MessageButton dialogButtons, string title, object viewModel) {
            VerifyService(service);
            var res = service.ShowDialog(UICommand.GenerateFromMessageButton(dialogButtons, GetLocalizer(service)), title, null, viewModel, null, null);
            return GetMessageResult(res);
        }

        public static UICommand ShowDialog(this IDialogService service, IEnumerable<UICommand> dialogCommands, string title, string documentType, object viewModel) {
            VerifyService(service);
            return service.ShowDialog(dialogCommands, title, documentType, viewModel, null, null);
        }
        public static MessageResult ShowDialog(this IDialogService service, MessageButton dialogButtons, string title, string documentType, object viewModel) {
            VerifyService(service);
            var res = service.ShowDialog(UICommand.GenerateFromMessageButton(dialogButtons, GetLocalizer(service)), title, documentType, viewModel, null, null);
            return GetMessageResult(res);
        }

        public static UICommand ShowDialog(this IDialogService service, IEnumerable<UICommand> dialogCommands, string title, string documentType, object parameter, object parentViewModel) {
            VerifyService(service);
            return service.ShowDialog(dialogCommands, title, documentType, null, parameter, parentViewModel);
        }
        public static MessageResult ShowDialog(this IDialogService service, MessageButton dialogButtons, string title, string documentType, object parameter, object parentViewModel) {
            VerifyService(service);
            var res = service.ShowDialog(UICommand.GenerateFromMessageButton(dialogButtons, GetLocalizer(service)), title, documentType, null, parameter, parentViewModel);
            return GetMessageResult(res);
        }

        internal static void VerifyService(IDialogService service) {
            if (service == null)
                throw new ArgumentNullException("service");
        }
        internal static IMessageButtonLocalizer GetLocalizer(IDialogService service) {
            return service as IMessageButtonLocalizer ?? (service as IMessageBoxButtonLocalizer).With(x => x.ToMessageButtonLocalizer()) ?? new DefaultMessageButtonLocalizer();
        }
        static MessageResult GetMessageResult(UICommand result) {
            if(result == null)
                return MessageResult.None;
            return (MessageResult)result.Tag;
        }
    }
}