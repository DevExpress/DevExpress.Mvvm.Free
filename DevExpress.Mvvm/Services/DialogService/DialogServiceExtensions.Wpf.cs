#if SILVERLIGHT
using MessageBoxButton = DevExpress.Mvvm.DXMessageBoxButton;
#endif
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using DevExpress.Mvvm.Native;

namespace DevExpress.Mvvm.Xpf {
    [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
    public static class DialogServicePlatformExtension {
        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
#if !SILVERLIGHT
        public static MessageBoxResult ShowDialog(this IDialogService service, MessageBoxButton dialogButtons, string title, object viewModel) {
#else
        public static Task<MessageBoxResult> ShowDialog(this IDialogService service, MessageBoxButton dialogButtons, string title, object viewModel) {
#endif
            DialogServiceExtensions.VerifyService(service);
            var res = service.ShowDialog(UICommand.GenerateFromMessageBoxButton(dialogButtons, DialogServiceExtensions.GetLocalizer(service)), title, null, viewModel, null, null);
            return GetMessageBoxResult(res);
        }
        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
#if !SILVERLIGHT
        public static MessageBoxResult ShowDialog(this IDialogService service, MessageBoxButton dialogButtons, string title, string documentType, object parameter, object parentViewModel) {
#else
        public static Task<MessageBoxResult> ShowDialog(this IDialogService service, MessageBoxButton dialogButtons, string title, string documentType, object parameter, object parentViewModel) {
#endif
            DialogServiceExtensions.VerifyService(service);
            var res = service.ShowDialog(UICommand.GenerateFromMessageBoxButton(dialogButtons, DialogServiceExtensions.GetLocalizer(service)), title, documentType, null, parameter, parentViewModel);
            return GetMessageBoxResult(res);
        }
        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
#if !SILVERLIGHT
        public static MessageBoxResult ShowDialog(this IDialogService service, MessageBoxButton dialogButtons, string title, string documentType, object viewModel) {
#else
        public static Task<MessageBoxResult> ShowDialog(this IDialogService service, MessageBoxButton dialogButtons, string title, string documentType, object viewModel) {
#endif
            DialogServiceExtensions.VerifyService(service);
            var res = service.ShowDialog(UICommand.GenerateFromMessageBoxButton(dialogButtons, DialogServiceExtensions.GetLocalizer(service)), title, documentType, viewModel, null, null);
            return GetMessageBoxResult(res);
        }
#if SILVERLIGHT
        static Task<MessageBoxResult> GetMessageBoxResult(Task<UICommand> result) {
            if(result == null)
                return null;
            return result.ContinueWith<MessageBoxResult>(x => GetMessageBoxResult(x.Result));
        }
#endif
        static MessageBoxResult GetMessageBoxResult(UICommand result) {
            if(result == null)
                return MessageBoxResult.None;
            return (MessageBoxResult)result.Tag;
        }
    }
}