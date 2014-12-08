#if SILVERLIGHT
using MessageBoxButton = DevExpress.Mvvm.DXMessageBoxButton;
#endif
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using DevExpress.Mvvm.Native;

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
        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
#if !SILVERLIGHT
        public static MessageBoxResult ShowDialog(this IDialogService service, MessageBoxButton dialogButtons, string title, object viewModel) {
#else
        public static Task<MessageBoxResult> ShowDialog(this IDialogService service, MessageBoxButton dialogButtons, string title, object viewModel) {
#endif
            VerifyService(service);
            var res = service.ShowDialog(UICommand.GenerateFromMessageBoxButton(dialogButtons, GetLocalizer(service)), title, null, viewModel, null, null);
            return GetMessageBoxResult(res);
        }
#if !SILVERLIGHT
        public static MessageResult ShowDialog(this IDialogService service, MessageButton dialogButtons, string title, object viewModel) {
#else
        public static Task<MessageResult> ShowDialog(this IDialogService service, MessageButton dialogButtons, string title, object viewModel) {
#endif
            VerifyService(service);
            var res = service.ShowDialog(UICommand.GenerateFromMessageButton(dialogButtons, GetLocalizer(service)), title, null, viewModel, null, null);
            return GetMessageResult(res);
        }

#if !SILVERLIGHT
        public static UICommand ShowDialog(this IDialogService service, IEnumerable<UICommand> dialogCommands, string title, string documentType, object viewModel) {
#else
        public static Task<UICommand> ShowDialog(this IDialogService service, IEnumerable<UICommand> dialogCommands, string title, string documentType, object viewModel) {
#endif
            VerifyService(service);
            return service.ShowDialog(dialogCommands, title, documentType, viewModel, null, null);
        }
        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
#if !SILVERLIGHT
        public static MessageBoxResult ShowDialog(this IDialogService service, MessageBoxButton dialogButtons, string title, string documentType, object viewModel) {
#else
        public static Task<MessageBoxResult> ShowDialog(this IDialogService service, MessageBoxButton dialogButtons, string title, string documentType, object viewModel) {
#endif
            VerifyService(service);
            var res = service.ShowDialog(UICommand.GenerateFromMessageBoxButton(dialogButtons, GetLocalizer(service)), title, documentType, viewModel, null, null);
            return GetMessageBoxResult(res);
        }
#if !SILVERLIGHT
        public static MessageResult ShowDialog(this IDialogService service, MessageButton dialogButtons, string title, string documentType, object viewModel) {
#else
        public static Task<MessageResult> ShowDialog(this IDialogService service, MessageButton dialogButtons, string title, string documentType, object viewModel) {
#endif
            VerifyService(service);
            var res = service.ShowDialog(UICommand.GenerateFromMessageButton(dialogButtons, GetLocalizer(service)), title, documentType, viewModel, null, null);
            return GetMessageResult(res);
        }

#if !SILVERLIGHT
        public static UICommand ShowDialog(this IDialogService service, IEnumerable<UICommand> dialogCommands, string title, string documentType, object parameter, object parentViewModel) {
#else
        public static Task<UICommand> ShowDialog(this IDialogService service, IEnumerable<UICommand> dialogCommands, string title, string documentType, object parameter, object parentViewModel) {
#endif
            VerifyService(service);
            return service.ShowDialog(dialogCommands, title, documentType, null, parameter, parentViewModel);
        }
        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
#if !SILVERLIGHT
        public static MessageBoxResult ShowDialog(this IDialogService service, MessageBoxButton dialogButtons, string title, string documentType, object parameter, object parentViewModel) {
#else
        public static Task<MessageBoxResult> ShowDialog(this IDialogService service, MessageBoxButton dialogButtons, string title, string documentType, object parameter, object parentViewModel) {
#endif
            VerifyService(service);
            var res = service.ShowDialog(UICommand.GenerateFromMessageBoxButton(dialogButtons, GetLocalizer(service)), title, documentType, null, parameter, parentViewModel);
            return GetMessageBoxResult(res);
        }
#if !SILVERLIGHT
        public static MessageResult ShowDialog(this IDialogService service, MessageButton dialogButtons, string title, string documentType, object parameter, object parentViewModel) {
#else
        public static Task<MessageResult> ShowDialog(this IDialogService service, MessageButton dialogButtons, string title, string documentType, object parameter, object parentViewModel) {
#endif
            VerifyService(service);
            var res = service.ShowDialog(UICommand.GenerateFromMessageButton(dialogButtons, GetLocalizer(service)), title, documentType, null, parameter, parentViewModel);
            return GetMessageResult(res);
        }

        static void VerifyService(IDialogService service) {
            if (service == null)
                throw new ArgumentNullException("service");
        }
        static IMessageButtonLocalizer GetLocalizer(IDialogService service) {
            return service as IMessageButtonLocalizer ?? (service as IMessageBoxButtonLocalizer).With(x => x.ToMessageButtonLocalizer()) ?? new DefaultMessageButtonLocalizer();
        }
#if SILVERLIGHT
        static Task<MessageResult> GetMessageResult(Task<UICommand> result) {
            if(result == null)
                return null;
            return result.ContinueWith<MessageResult>(x => GetMessageResult(x.Result));
        }
        static Task<MessageBoxResult> GetMessageBoxResult(Task<UICommand> result) {
            if(result == null)
                return null;
            return result.ContinueWith<MessageBoxResult>(x => GetMessageBoxResult(x.Result));
        }
#endif
        static MessageResult GetMessageResult(UICommand result) {
            if(result == null)
                return MessageResult.None;
            return (MessageResult)result.Tag;
        }
        static MessageBoxResult GetMessageBoxResult(UICommand result) {
            if(result == null)
                return MessageBoxResult.None;
            return (MessageBoxResult)result.Tag;
        }
    }
}