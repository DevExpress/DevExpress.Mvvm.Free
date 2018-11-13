using System;
using System.Collections.Generic;
using System.Linq;

namespace DevExpress.Mvvm {
    public interface ISupportNavigation : ISupportParameter {
        void OnNavigatedTo();
        void OnNavigatedFrom();
    }
}