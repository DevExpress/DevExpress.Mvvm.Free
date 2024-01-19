using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace DevExpress.Mvvm {
    public delegate void ActiveDocumentChangedEventHandler(object sender, ActiveDocumentChangedEventArgs e);
    public class ActiveDocumentChangedEventArgs : EventArgs {
        public IDocument OldDocument { get; private set; }
        public IDocument NewDocument { get; private set; }
        public ActiveDocumentChangedEventArgs(IDocument oldDocument, IDocument newDocument) {
            OldDocument = oldDocument;
            NewDocument = newDocument;
        }
    }
    public interface IDocumentManagerService {
        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        IDocument CreateDocument(string documentType, object viewModel, object parameter, object parentViewModel);

        IDocument ActiveDocument { get; set; }
        event ActiveDocumentChangedEventHandler ActiveDocumentChanged;

        IEnumerable<IDocument> Documents { get; }
    }
}