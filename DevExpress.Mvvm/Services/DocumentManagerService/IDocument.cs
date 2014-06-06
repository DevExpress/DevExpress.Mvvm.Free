using System;
using System.Collections.Generic;
using System.Linq;

namespace DevExpress.Mvvm {
    public interface IDocument {
        object Id { get; set; }
        object Content { get; }
        object Title { get; set; }
        bool DestroyOnClose { get; set; }

        void Show();
        void Hide();
        void Close(bool force = true);
    }
}