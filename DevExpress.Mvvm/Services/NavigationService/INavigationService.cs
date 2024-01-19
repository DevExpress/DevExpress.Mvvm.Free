using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DevExpress.Mvvm {
    public interface INavigationService {
        void ClearCache();
        void ClearNavigationHistory();
        void Navigate(string viewType, object viewModel, object param, object parentViewModel, bool saveToJournal);
        bool CanNavigate { get; }
        void GoBack(object param);
        void GoForward(object param);
        void GoBack();
        void GoForward();
        bool CanGoBack { get; }
        bool CanGoForward { get; }
        event EventHandler CanGoBackChanged;
        event EventHandler CanGoForwardChanged;
        object Current { get; }
        event EventHandler CurrentChanged;
    }

    public static class INavigationServiceExtensions {
        public static void Navigate(this INavigationService service, string viewType, object param = null, object parentViewModel = null) {
            NavigateCore(service, viewType, null, param, parentViewModel, true);
        }
        public static void Navigate(this INavigationService service, object viewModel, object param = null, object parentViewModel = null) {
            NavigateCore(service, null, viewModel, param, parentViewModel, true);
        }
        public static void Navigate(this INavigationService service, string viewType, object param, object parentViewModel, bool saveToJournal) {
            NavigateCore(service, viewType, null, param, parentViewModel, saveToJournal);
        }
        static void NavigateCore(INavigationService service, string viewType, object viewModel, object param, object parentViewModel, bool saveToJournal) {
            VerifyService(service);
            service.Navigate(viewType, viewModel, param, parentViewModel, saveToJournal);
        }
        static void VerifyService(INavigationService service) {
            if(service == null)
                throw new ArgumentNullException("service");
        }
    }
}