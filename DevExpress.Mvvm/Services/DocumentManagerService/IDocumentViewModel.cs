using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

namespace DevExpress.Mvvm {
    [Obsolete("Use the IDocumentContent interface instead.")]
    public interface IDocumentViewModel {
        bool Close();
        object Title { get; }
    }
    public interface IDocumentContent {
        IDocumentOwner DocumentOwner { get; set; }
        object Title { get; }
        void OnClose(CancelEventArgs e);
        void OnDestroy();
    }
    public interface IDocumentOwner {
        void Close(IDocumentContent documentContent, bool force = true);
    }
}
namespace DevExpress.Mvvm.Native {
    public static class DocumentViewModelHelper {
        public static void OnClose(object documentContentOrDocumentViewModel, CancelEventArgs e) {
            IDocumentContent documentContent = documentContentOrDocumentViewModel as IDocumentContent;
            if(documentContent != null) {
                documentContent.OnClose(e);
                return;
            }
#pragma warning disable 612, 618
            IDocumentViewModel documentViewModel = documentContentOrDocumentViewModel as IDocumentViewModel;
#pragma warning restore 612, 618
            if(documentViewModel != null) {
                e.Cancel = !documentViewModel.Close();
                return;
            }
        }
        public static void OnDestroy(object documentContentOrDocumentViewModel) {
            IDocumentContent documentContent = documentContentOrDocumentViewModel as IDocumentContent;
            if(documentContent != null)
                documentContent.OnDestroy();
        }
        public static bool IsDocumentContentOrDocumentViewModel(object viewModel) {
#pragma warning disable 612, 618
            return viewModel is IDocumentContent || viewModel is IDocumentViewModel;
#pragma warning restore 612, 618
        }
        public static bool TitlePropertyHasImplicitImplementation(object documentContentOrDocumentViewModel) {
            IDocumentContent documentContent = documentContentOrDocumentViewModel as IDocumentContent;
            if(documentContent != null)
                return ExpressionHelper.PropertyHasImplicitImplementation(documentContent, i => i.Title);
#pragma warning disable 612, 618
            IDocumentViewModel documentViewModel = documentContentOrDocumentViewModel as IDocumentViewModel;
            if(documentViewModel != null)
                return ExpressionHelper.PropertyHasImplicitImplementation(documentViewModel, i => i.Title);
#pragma warning restore 612, 618
            throw new ArgumentException("", "documentContentOrDocumentViewModel");
        }
        public static object GetTitle(object documentContentOrDocumentViewModel) {
            IDocumentContent documentContent = documentContentOrDocumentViewModel as IDocumentContent;
            if(documentContent != null)
                return documentContent.Title;
#pragma warning disable 612, 618
            IDocumentViewModel documentViewModel = documentContentOrDocumentViewModel as IDocumentViewModel;
            if(documentViewModel != null)
                return documentViewModel.Title;
#pragma warning restore 612, 618
            throw new ArgumentException("", "documentContentOrDocumentViewModel");
        }
    }
}