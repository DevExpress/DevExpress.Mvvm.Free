using System;
using System.Collections.Generic;
using System.Linq;

namespace DevExpress.Mvvm {
    public interface ISupportServices {
        IServiceContainer ServiceContainer { get; }
    }
}