using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace DevExpress.Mvvm {
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
            IDocumentViewModel documentViewModel = documentContentOrDocumentViewModel as IDocumentViewModel;
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
            return viewModel is IDocumentContent || viewModel is IDocumentViewModel;
        }
        public static bool TitlePropertyHasImplicitImplementation(object documentContentOrDocumentViewModel) {
            IDocumentContent documentContent = documentContentOrDocumentViewModel as IDocumentContent;
            if(documentContent != null)
                return ExpressionHelper.PropertyHasImplicitImplementation(documentContent, i => i.Title);
            IDocumentViewModel documentViewModel = documentContentOrDocumentViewModel as IDocumentViewModel;
            if(documentViewModel != null)
                return ExpressionHelper.PropertyHasImplicitImplementation(documentViewModel, i => i.Title);
            throw new ArgumentException("", "documentContentOrDocumentViewModel");
        }
        public static object GetTitle(object documentContentOrDocumentViewModel) {
            IDocumentContent documentContent = documentContentOrDocumentViewModel as IDocumentContent;
            if(documentContent != null)
                return documentContent.Title;
            IDocumentViewModel documentViewModel = documentContentOrDocumentViewModel as IDocumentViewModel;
            if(documentViewModel != null)
                return documentViewModel.Title;
            throw new ArgumentException("", "documentContentOrDocumentViewModel");
        }
    }
}