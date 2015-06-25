using System;
using System.Collections.Generic;
using System.Linq;

namespace DevExpress.Mvvm {
    public enum DocumentState {
        Visible, Hidden, Destroyed
    }
    public interface IDocument {
        object Id { get; set; }
        object Content { get; }
        object Title { get; set; }
        bool DestroyOnClose { get; set; }

        void Show();
        void Hide();
        void Close(bool force = true);
    }
    public interface IDocumentInfo {
        DocumentState State { get; }
        string DocumentType { get; }
    }
}