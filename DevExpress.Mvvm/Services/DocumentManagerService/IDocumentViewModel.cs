using System;
using System.Collections.Generic;
using System.Linq;

namespace DevExpress.Mvvm {
    public interface IDocumentViewModel {
        bool Close();
        object Title { get; }
    }
}