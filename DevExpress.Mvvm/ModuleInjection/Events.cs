using System;
using System.ComponentModel;
using System.Windows;

namespace DevExpress.Mvvm.ModuleInjection {
    public class NavigationEventArgs : EventArgs {
        public string RegionName { get; private set; }
        public object OldViewModel { get; private set; }
        public string OldViewModelKey { get; private set; }
        public object NewViewModel { get; private set; }
        public string NewViewModelKey { get; private set; }
        public NavigationEventArgs(string regionName, object oldVM, object newVM, string oldVMKey, string newVMKey) {
            RegionName = regionName;
            OldViewModel = oldVM;
            NewViewModel = newVM;
            OldViewModelKey = oldVMKey;
            NewViewModelKey = newVMKey;
        }
    }
    public class ViewModelCreatedEventArgs : EventArgs {
        public string RegionName { get; private set; }
        public object ViewModel { get; private set; }
        public string ViewModelKey { get; private set; }
        public ViewModelCreatedEventArgs(string regionName, object viewModel, string viewModelKey) {
            RegionName = regionName;
            ViewModel = viewModel;
            ViewModelKey = viewModelKey;
        }
    }
    public class ViewModelRemovedEventArgs : EventArgs {
        public string RegionName { get; private set; }
        public object ViewModel { get; private set; }
        public string ViewModelKey { get; private set; }
        public ViewModelRemovedEventArgs(string regionName, object viewModel, string viewModelKey) {
            RegionName = regionName;
            ViewModel = viewModel;
            ViewModelKey = viewModelKey;
        }
    }
    public class ViewModelRemovingEventArgs : CancelEventArgs {
        public string RegionName { get; private set; }
        public object ViewModel { get; private set; }
        public string ViewModelKey { get; private set; }
        public ViewModelRemovingEventArgs(string regionName, object viewModel, string viewModelKey) {
            RegionName = regionName;
            ViewModel = viewModel;
            ViewModelKey = viewModelKey;
        }
    }
    public class WindowInjectionResult {
        public string RegionName { get; private set; }
        public object ViewModel { get; private set; }
        public object ViewModelKey { get; private set; }
        public MessageBoxResult? Result { get; private set; }
        public WindowInjectionResult(string regionName, object viewModel, object viewModelKey, MessageBoxResult? result) {
            RegionName = regionName;
            ViewModel = viewModel;
            ViewModelKey = viewModelKey;
            Result = result;
        }
    }

    public interface IRegionEventManager {
        [WeakEvent]
        event EventHandler<NavigationEventArgs> Navigation;
        [WeakEvent]
        event EventHandler<ViewModelCreatedEventArgs> ViewModelCreated;
        [WeakEvent]
        event EventHandler<ViewModelRemovingEventArgs> ViewModelRemoving;
        [WeakEvent]
        event EventHandler<ViewModelRemovedEventArgs> ViewModelRemoved;
    }
    public interface IViewModelEventManager {
        [WeakEvent]
        event EventHandler<NavigationEventArgs> Navigated;
        [WeakEvent]
        event EventHandler<NavigationEventArgs> NavigatedAway;
        [WeakEvent]
        event EventHandler<ViewModelRemovingEventArgs> ViewModelRemoving;
        [WeakEvent]
        event EventHandler<ViewModelRemovedEventArgs> ViewModelRemoved;
    }
}
namespace DevExpress.Mvvm.ModuleInjection.Native {
    public interface IRegionEventManagerImplementation : IRegionEventManager {
        void RaiseNavigation(object sender, NavigationEventArgs e);
        void RaiseViewModelCreated(object sender, ViewModelCreatedEventArgs e);
        void RaiseViewModelRemoving(object sender, ViewModelRemovingEventArgs e);
        void RaiseViewModelRemoved(object sender, ViewModelRemovedEventArgs e);
    }
    public interface IViewModelEventManagerImplementation : IViewModelEventManager {
        void RaiseNavigated(object sender, NavigationEventArgs e);
        void RaiseNavigatedAway(object sender, NavigationEventArgs e);
        void RaiseViewModelRemoving(object sender, ViewModelRemovingEventArgs e);
        void RaiseViewModelRemoved(object sender, ViewModelRemovedEventArgs e);
    }
}
namespace DevExpress.Mvvm.ModuleInjection.Native {
    public class RegionEventManager : IRegionEventManager, IRegionEventManagerImplementation {
        WeakEvent<EventHandler<NavigationEventArgs>, NavigationEventArgs> navigation = new WeakEvent<EventHandler<NavigationEventArgs>, NavigationEventArgs>();
        WeakEvent<EventHandler<ViewModelCreatedEventArgs>, ViewModelCreatedEventArgs> viewModelCreated = new WeakEvent<EventHandler<ViewModelCreatedEventArgs>, ViewModelCreatedEventArgs>();
        WeakEvent<EventHandler<ViewModelRemovingEventArgs>, ViewModelRemovingEventArgs> viewModelRemoving = new WeakEvent<EventHandler<ViewModelRemovingEventArgs>, ViewModelRemovingEventArgs>();
        WeakEvent<EventHandler<ViewModelRemovedEventArgs>, ViewModelRemovedEventArgs> viewModelRemoved = new WeakEvent<EventHandler<ViewModelRemovedEventArgs>, ViewModelRemovedEventArgs>();

        event EventHandler<NavigationEventArgs> IRegionEventManager.Navigation { add { navigation.Add(value); } remove { navigation.Remove(value); } }
        event EventHandler<ViewModelCreatedEventArgs> IRegionEventManager.ViewModelCreated { add { viewModelCreated.Add(value); } remove { viewModelCreated.Remove(value); } }
        event EventHandler<ViewModelRemovingEventArgs> IRegionEventManager.ViewModelRemoving { add { viewModelRemoving.Add(value); } remove { viewModelRemoving.Remove(value); } }
        event EventHandler<ViewModelRemovedEventArgs> IRegionEventManager.ViewModelRemoved { add { viewModelRemoved.Add(value); } remove { viewModelRemoved.Remove(value); } }

        void IRegionEventManagerImplementation.RaiseNavigation(object sender, NavigationEventArgs e) { navigation.Raise(sender, e); }
        void IRegionEventManagerImplementation.RaiseViewModelCreated(object sender, ViewModelCreatedEventArgs e) { viewModelCreated.Raise(sender, e); }
        void IRegionEventManagerImplementation.RaiseViewModelRemoving(object sender, ViewModelRemovingEventArgs e) { viewModelRemoving.Raise(sender, e); }
        void IRegionEventManagerImplementation.RaiseViewModelRemoved(object sender, ViewModelRemovedEventArgs e) { viewModelRemoved.Raise(sender, e); }
    }
    public class ViewModelEventManager : IViewModelEventManager, IViewModelEventManagerImplementation {
        WeakEvent<EventHandler<NavigationEventArgs>, NavigationEventArgs> navigated = new WeakEvent<EventHandler<NavigationEventArgs>, NavigationEventArgs>();
        WeakEvent<EventHandler<NavigationEventArgs>, NavigationEventArgs> navigatedAway = new WeakEvent<EventHandler<NavigationEventArgs>, NavigationEventArgs>();
        WeakEvent<EventHandler<ViewModelRemovingEventArgs>, ViewModelRemovingEventArgs> viewModelRemoving = new WeakEvent<EventHandler<ViewModelRemovingEventArgs>, ViewModelRemovingEventArgs>();
        WeakEvent<EventHandler<ViewModelRemovedEventArgs>, ViewModelRemovedEventArgs> viewModelRemoved = new WeakEvent<EventHandler<ViewModelRemovedEventArgs>, ViewModelRemovedEventArgs>();

        event EventHandler<NavigationEventArgs> IViewModelEventManager.Navigated { add { Add(navigated, value); } remove { Remove(navigated, value); } }
        event EventHandler<NavigationEventArgs> IViewModelEventManager.NavigatedAway { add { Add(navigatedAway, value); } remove { Remove(navigatedAway, value); } }
        event EventHandler<ViewModelRemovingEventArgs> IViewModelEventManager.ViewModelRemoving { add { Add(viewModelRemoving, value); } remove { Remove(viewModelRemoving, value); } }
        event EventHandler<ViewModelRemovedEventArgs> IViewModelEventManager.ViewModelRemoved { add { Add(viewModelRemoved, value); } remove { Remove(viewModelRemoved, value); } }

        void IViewModelEventManagerImplementation.RaiseNavigated(object sender, NavigationEventArgs e) { Raise(navigated, sender, e); }
        void IViewModelEventManagerImplementation.RaiseNavigatedAway(object sender, NavigationEventArgs e) { Raise(navigatedAway, sender, e); }
        void IViewModelEventManagerImplementation.RaiseViewModelRemoving(object sender, ViewModelRemovingEventArgs e) { Raise(viewModelRemoving, sender, e); }
        void IViewModelEventManagerImplementation.RaiseViewModelRemoved(object sender, ViewModelRemovedEventArgs e) { Raise(viewModelRemoved, sender, e); }

        readonly WeakReference viewModel;
        public ViewModelEventManager(object viewModel) {
            this.viewModel = new WeakReference(viewModel);
        }
        void Add<TEventArgs>(WeakEvent<EventHandler<TEventArgs>, TEventArgs> weakEvent, EventHandler<TEventArgs> eventHandler) where TEventArgs : EventArgs {
            if(!viewModel.IsAlive) return;
            weakEvent.Add(eventHandler);
        }
        void Remove<TEventArgs>(WeakEvent<EventHandler<TEventArgs>, TEventArgs> weakEvent, EventHandler<TEventArgs> eventHandler) where TEventArgs : EventArgs {
            weakEvent.Remove(eventHandler);
        }
        void Raise<TEventArgs>(WeakEvent<EventHandler<TEventArgs>, TEventArgs> weakEvent, object sender, TEventArgs e) where TEventArgs : EventArgs {
            weakEvent.Raise(sender, e);
        }
    }
}