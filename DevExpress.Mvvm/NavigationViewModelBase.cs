using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DevExpress.Mvvm {
    public abstract class NavigationViewModelBase : ViewModelBase, ISupportNavigation {
        protected override void OnInitializeInDesignMode() {
            base.OnInitializeInDesignMode();
            OnNavigatedTo();
        }
        #region ISupportNavigation Members
        protected virtual void OnNavigatedTo() { }
        protected virtual void OnNavigatedFrom() {
        }
        void ISupportNavigation.OnNavigatedTo() {
            OnNavigatedTo();
        }
        void ISupportNavigation.OnNavigatedFrom() {
            OnNavigatedFrom();
        }
        #endregion
    }
}