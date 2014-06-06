#if SILVERLIGHT
using MessageBoxButton = DevExpress.Mvvm.DXMessageBoxButton;
#endif
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;

namespace DevExpress.Mvvm {
    public static class DialogServiceExtensions {
#if !SILVERLIGHT
        public static UICommand ShowDialog(this IDialogService service, IEnumerable<UICommand> dialogCommands, string title, object viewModel) {
#else
        public static Task<UICommand> ShowDialog(this IDialogService service, IEnumerable<UICommand> dialogCommands, string title, object viewModel) {
#endif
            VerifyService(service);
            return service.ShowDialog(dialogCommands, title, null, viewModel, null, null);
        }
#if !SILVERLIGHT
        public static MessageBoxResult ShowDialog(this IDialogService service, MessageBoxButton dialogButtons, string title, object viewModel) {
#else
        public static Task<MessageBoxResult> ShowDialog(this IDialogService service, MessageBoxButton dialogButtons, string title, object viewModel) {
#endif
            VerifyService(service);
            var res = service.ShowDialog(UICommand.GenerateFromMessageBoxButton(dialogButtons, GetLocalizer(service)), title, null, viewModel, null, null);
            return GetResult(res);
        }

#if !SILVERLIGHT
        public static UICommand ShowDialog(this IDialogService service, IEnumerable<UICommand> dialogCommands, string title, string documentType, object viewModel) {
#else
        public static Task<UICommand> ShowDialog(this IDialogService service, IEnumerable<UICommand> dialogCommands, string title, string documentType, object viewModel) {
#endif
            VerifyService(service);
            return service.ShowDialog(dialogCommands, title, documentType, viewModel, null, null);
        }
#if !SILVERLIGHT
        public static MessageBoxResult ShowDialog(this IDialogService service, MessageBoxButton dialogButtons, string title, string documentType, object viewModel) {
#else
        public static Task<MessageBoxResult> ShowDialog(this IDialogService service, MessageBoxButton dialogButtons, string title, string documentType, object viewModel) {
#endif
            VerifyService(service);
            var res = service.ShowDialog(UICommand.GenerateFromMessageBoxButton(dialogButtons, GetLocalizer(service)), title, documentType, viewModel, null, null);
            return GetResult(res);
        }

#if !SILVERLIGHT
        public static UICommand ShowDialog(this IDialogService service, IEnumerable<UICommand> dialogCommands, string title, string documentType, object parameter, object parentViewModel) {
#else
        public static Task<UICommand> ShowDialog(this IDialogService service, IEnumerable<UICommand> dialogCommands, string title, string documentType, object parameter, object parentViewModel) {
#endif
            VerifyService(service);
            return service.ShowDialog(dialogCommands, title, documentType, null, parameter, parentViewModel);
        }
#if !SILVERLIGHT
        public static MessageBoxResult ShowDialog(this IDialogService service, MessageBoxButton dialogButtons, string title, string documentType, object parameter, object parentViewModel) {
#else
        public static Task<MessageBoxResult> ShowDialog(this IDialogService service, MessageBoxButton dialogButtons, string title, string documentType, object parameter, object parentViewModel) {
#endif
            VerifyService(service);
            var res = service.ShowDialog(UICommand.GenerateFromMessageBoxButton(dialogButtons, GetLocalizer(service)), title, documentType, null, parameter, parentViewModel);
            return GetResult(res);
        }

        static void VerifyService(IDialogService service) {
            if (service == null)
                throw new ArgumentNullException("service");
        }
        static IMessageBoxButtonLocalizer GetLocalizer(IDialogService service) {
            return service as IMessageBoxButtonLocalizer ?? new DefaultMessageBoxButtonLocalizer();
        }
#if SILVERLIGHT
        static Task<MessageBoxResult> GetResult(Task<UICommand> result) {
            if(result == null)
                return null;
            return result.ContinueWith<MessageBoxResult>(x => GetResult(x.Result));
        }
#endif
        static MessageBoxResult GetResult(UICommand result) {
            if(result == null)
                return MessageBoxResult.None;
            return (MessageBoxResult)result.Tag;
        }
    }
}