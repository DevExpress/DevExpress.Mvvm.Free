using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DevExpress.Mvvm {
    public interface INavigationService {
        void Navigate(string target, object param = null, object parentViewModel = null);
        void Navigate(string target, object param, object parentViewModel, bool saveToJournal);
        void GoBack();
        void GoForward();
        bool CanGoBack { get; }
        bool CanGoForward { get; }
        object Current { get; }
    }
}