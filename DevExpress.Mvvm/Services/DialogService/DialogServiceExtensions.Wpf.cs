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
        public static MessageBoxResult ShowDialog(this IDialogService service, MessageBoxButton dialogButtons, string title, object viewModel) {
            DialogServiceExtensions.VerifyService(service);
            var res = service.ShowDialog(UICommand.GenerateFromMessageBoxButton(dialogButtons, DialogServiceExtensions.GetLocalizer(service)), title, null, viewModel, null, null);
            return GetMessageBoxResult(res);
        }
        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public static MessageBoxResult ShowDialog(this IDialogService service, MessageBoxButton dialogButtons, string title, string documentType, object parameter, object parentViewModel) {
            DialogServiceExtensions.VerifyService(service);
            var res = service.ShowDialog(UICommand.GenerateFromMessageBoxButton(dialogButtons, DialogServiceExtensions.GetLocalizer(service)), title, documentType, null, parameter, parentViewModel);
            return GetMessageBoxResult(res);
        }
        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public static MessageBoxResult ShowDialog(this IDialogService service, MessageBoxButton dialogButtons, string title, string documentType, object viewModel) {
            DialogServiceExtensions.VerifyService(service);
            var res = service.ShowDialog(UICommand.GenerateFromMessageBoxButton(dialogButtons, DialogServiceExtensions.GetLocalizer(service)), title, documentType, viewModel, null, null);
            return GetMessageBoxResult(res);
        }
        static MessageBoxResult GetMessageBoxResult(UICommand result) {
            if(result == null)
                return MessageBoxResult.None;
            return (MessageBoxResult)result.Tag;
        }
    }
}