using DevExpress.Mvvm.ModuleInjection.Native;
using DevExpress.Mvvm.Native;
using DevExpress.Mvvm.UI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace DevExpress.Mvvm.ModuleInjection {
    public enum LogicalSerializationMode { Disabled, Enabled }
    public enum VisualSerializationMode { Disabled, PerKey, PerViewType }

    public interface IModuleManagerBase {
        void Save(string regionName, out string logicalState, out string visualState);
        bool Restore(string logicalState, string visualState);
        void Register(string regionName, IModule module);
        void Unregister(string regionName, string key);
        IModule GetModule(string regionName, string key);
        IRegion GetRegion(string regionName);
        IEnumerable<IRegion> GetRegions(object viewModel);
        IRegionEventManager GetEvents(string regionName);
        IViewModelEventManager GetEvents(object viewModel);
    }
    public interface IModuleManager : IModuleManagerBase {
        bool IsInjected(string regionName, string key);
        void Inject(string regionName, string key, object parameter = null);
        void Navigate(string regionName, string key);
        void Remove(string regionName, string key, bool raiseViewModelRemovingEvent = true);
        void Clear(string regionName);
    }
    public interface IModuleWindowManager : IModuleManagerBase {
        bool IsShown(string regionName, string key);
        Task<WindowInjectionResult> Show(string regionName, string key, object parameter = null);
        void Activate(string regionName, string key);
        void Close(string regionName, string key, MessageBoxResult? dialogResult = null, bool raiseViewModelRemovingEvent = true);
        void Clear(string regionName);
    }
    public static class ModuleManagerExtensions {
        public static void Save(this IModuleManagerBase manager, out string logicalState, out string visualState) {
            Verifier.VerifyManager(manager);
            manager.Save(null, out logicalState, out visualState);
        }
        public static IRegion GetRegion(this IModuleManagerBase manager, object viewModel) {
            Verifier.VerifyManager(manager);
            var regions = manager.GetRegions(viewModel);
            return regions.Any() ? regions.Single() : null;
        }

        public static void RegisterOrInjectOrNavigate(this IModuleManager manager, string regionName, IModule module, object parameter = null) {
            Verifier.VerifyManager(manager);
            Verifier.VerifyModule(module);
            if(manager.GetModule(regionName, module.Key) == null)
                manager.Register(regionName, module);
            InjectOrNavigate(manager, regionName, module.Key, parameter);
        }
        public static void InjectOrNavigate(this IModuleManager manager, string regionName, string key, object parameter = null) {
            Verifier.VerifyManager(manager);
            if(manager.GetModule(regionName, key) == null)
                ModuleInjectionException.ModuleMissing(regionName, key);
            if(!manager.IsInjected(regionName, key))
                manager.Inject(regionName, key, parameter);
            manager.Navigate(regionName, key);
        }
        public static Task<WindowInjectionResult> RegisterOrShowOrActivate(this IModuleWindowManager manager, string regionName, IModule module, object parameter = null) {
            Verifier.VerifyManager(manager);
            Verifier.VerifyModule(module);
            if(manager.GetModule(regionName, module.Key) == null)
                manager.Register(regionName, module);
            return ShowOrActivate(manager, regionName, module.Key, parameter);
        }
        public static Task<WindowInjectionResult> ShowOrActivate(this IModuleWindowManager manager, string regionName, string key, object parameter = null) {
            Verifier.VerifyManager(manager);
            if(manager.GetModule(regionName, key) == null)
                ModuleInjectionException.ModuleMissing(regionName, key);
            if(!manager.IsShown(regionName, key))
                return manager.Show(regionName, key, parameter);
            var res = ((IModuleManagerImplementation)manager).GetWindowInjectionResult(regionName, key);
            manager.Activate(regionName, key);
            return res;
        }
    }
}
namespace DevExpress.Mvvm.ModuleInjection.Native {
    public interface IModuleManagerImplementation : IModuleManagerBase, IModuleManager, IModuleWindowManager {
        bool KeepViewModelsAlive { get; }
        IViewModelLocator ViewModelLocator { get; }
        IViewLocator ViewLocator { get; }
        IStateSerializer ViewModelStateSerializer { get; }

        IRegionImplementation GetRegionImplementation(string regionName);
        Task<WindowInjectionResult> GetWindowInjectionResult(string regionName, string key);

        void OnNavigation(string regionName, NavigationEventArgs e);
        void OnViewModelRemoving(string regionName, ViewModelRemovingEventArgs e);
        void OnViewModelRemoved(string regionName, ViewModelRemovedEventArgs e);
        void RaiseViewModelCreated(ViewModelCreatedEventArgs e);
    }
}
namespace DevExpress.Mvvm.ModuleInjection {
    public class ModuleManager : IModuleManagerBase, IModuleManager, IModuleWindowManager, IModuleManagerImplementation {
        static IModuleManager _defaultInstance = new ModuleManager();
        static IModuleManager _default;
        public static IModuleManager DefaultManager { get { return _default ?? _defaultInstance; } set { _default = value; } }
        public static IModuleWindowManager DefaultWindowManager { get { return (IModuleWindowManager)DefaultManager; } }
        [EditorBrowsable(EditorBrowsableState.Never)]
        public static IModuleManagerImplementation DefaultImplementation { get { return (IModuleManagerImplementation)DefaultManager; } }

        readonly bool allowSaveRestoreLayout;
        readonly bool isTestingMode;
        readonly List<ModuleWrapper> modules = new List<ModuleWrapper>();
        readonly List<IRegionImplementation> regions = new List<IRegionImplementation>();
        public ModuleManager(bool keepViewModelsAlive = false)
            : this(null, null, null, true, false, keepViewModelsAlive) { }
        public ModuleManager(bool allowSaveRestoreLayout, bool isTestingMode, bool keepViewModelsAlive = false)
            : this(null, null, null, allowSaveRestoreLayout, isTestingMode, keepViewModelsAlive) { }
        public ModuleManager(IViewModelLocator viewModelLocator, IViewLocator viewLocator, IStateSerializer viewModelStateSerializer, bool allowSaveRestoreLayout, bool isTestingMode, bool keepViewModelsAlive = false) {
            this.keepViewModelsAlive = keepViewModelsAlive;
            this.allowSaveRestoreLayout = allowSaveRestoreLayout;
            this.isTestingMode = isTestingMode;
            this.viewModelLocator = viewModelLocator;
            this.viewLocator = viewLocator;
            this.viewModelStateSerializer = viewModelStateSerializer;
        }

        protected IModule GetModule(string regionName, string key, bool throwIfNull) {
            Verifier.VerifyRegionName(regionName);
            Verifier.VerifyKey(key);
            var res = modules.FirstOrDefault(x => x.RegionName == regionName && x.Module.Key == key)
                .With(x => x.Module);
            if(res == null && throwIfNull)
                ModuleInjectionException.ModuleMissing(regionName, key);
            return res;
        }
        protected IRegionImplementation GetRegion(string regionName) {
            Verifier.VerifyRegionName(regionName);
            var res = regions.FirstOrDefault(x => x.RegionName == regionName);
            if(res == null) {
                res = CreateRegion(regionName);
                regions.Add(res);
            }
            return res;
        }
        protected virtual IRegionImplementation CreateRegion(string regionName) {
            Verifier.VerifyRegionName(regionName);
            return new Region(regionName, this, isTestingMode);
        }

        #region ModuleWrapper
        class ModuleWrapper {
            public string RegionName { get; private set; }
            public IModule Module { get; private set; }
            public ModuleWrapper(string regionName, IModule module) {
                RegionName = regionName;
                Module = module;
            }
        }
        #endregion
        #region Events
        readonly Dictionary<string, RegionEventManager> regionEvents = new Dictionary<string, RegionEventManager>();
        readonly Dictionary<WeakReference, ViewModelEventManager> viewModelEvents = new Dictionary<WeakReference, ViewModelEventManager>();
        IRegionEventManagerImplementation GetEvents(string regionName, bool createIfNotExist) {
            Verifier.VerifyRegionName(regionName);
            if(!regionEvents.ContainsKey(regionName)) {
                if(!createIfNotExist) return null;
                regionEvents.Add(regionName, new RegionEventManager());
            }
            return regionEvents[regionName];
        }
        IViewModelEventManagerImplementation GetEvents(object viewModel, bool createIfNotExist) {
            Verifier.VerifyViewModel(viewModel);
            var keysToRemove = viewModelEvents.Keys.Where(x => !x.IsAlive).ToList();
            keysToRemove.ForEach(x => viewModelEvents.Remove(x));
            var key = viewModelEvents.Keys.Where(x => x.Target == viewModel).FirstOrDefault();
            if(key == null) {
                if(!createIfNotExist) return null;
                key = new WeakReference(viewModel);
                viewModelEvents.Add(key, new ViewModelEventManager(viewModel));
            }
            return viewModelEvents[key];
        }
        void RaiseNavigationEvent(string regionName, NavigationEventArgs e) {
            if(e.OldViewModel != null)
                GetEvents(e.OldViewModel, false).Do(x => x.RaiseNavigatedAway(this, e));
            if(e.NewViewModel != null)
                GetEvents(e.NewViewModel, false).Do(x => x.RaiseNavigated(this, e));
            GetEvents(regionName, false).Do(x => x.RaiseNavigation(this, e));
        }
        void RaiseViewModelRemovingEvent(string regionName, ViewModelRemovingEventArgs e) {
            if(e.ViewModel != null)
                GetEvents(e.ViewModel, false).Do(x => x.RaiseViewModelRemoving(this, e));
            GetEvents(regionName, false).Do(x => x.RaiseViewModelRemoving(this, e));
        }
        void RaiseViewModelRemovedEvent(string regionName, ViewModelRemovedEventArgs e) {
            if(e.ViewModel != null)
                GetEvents(e.ViewModel, false).Do(x => x.RaiseViewModelRemoved(this, e));
            GetEvents(regionName, false).Do(x => x.RaiseViewModelRemoved(this, e));
        }
        void RaiseViewModelCreatedEvent(string regionName, ViewModelCreatedEventArgs e) {
            GetEvents(regionName, false).Do(x => x.RaiseViewModelCreated(this, e));
        }
        #endregion
        #region IModuleManagerBase
        void IModuleManagerBase.Register(string regionName, IModule module) {
            Verifier.VerifyRegionName(regionName);
            Verifier.VerifyModule(module);
            if(GetModule(regionName, module.Key, false) != null)
                ModuleInjectionException.ModuleAlreadyExists(regionName, module.Key);
            modules.Add(new ModuleWrapper(regionName, module));
        }
        void IModuleManagerBase.Unregister(string regionName, string key) {
            var module = GetModule(regionName, key, false);
            if(module == null) return;
            ((IModuleManager)this).Remove(regionName, module.Key, false);
            modules.Remove(modules.First(x => x.Module == module));
        }
        IModule IModuleManagerBase.GetModule(string regionName, string key) {
            return GetModule(regionName, key, false);
        }
        IRegion IModuleManagerBase.GetRegion(string regionName) {
            return GetRegion(regionName);
        }
        IEnumerable<IRegion> IModuleManagerBase.GetRegions(object viewModel) {
            return regions.Where(x => x.ViewModels.Contains(viewModel)).ToList();
        }
        IRegionEventManager IModuleManagerBase.GetEvents(string regionName) {
            return GetEvents(regionName, true);
        }
        IViewModelEventManager IModuleManagerBase.GetEvents(object viewModel) {
            return GetEvents(viewModel, true);
        }

        void IModuleManagerBase.Save(string regionName, out string logicalState, out string visualState) {
            if(!allowSaveRestoreLayout) {
                logicalState = null;
                visualState = null;
                return;
            }
            var regionsToSerialize = string.IsNullOrEmpty(regionName) ? new List<IRegionImplementation>(regions) : new List<IRegionImplementation>(new[] { GetRegion(regionName) });
            LogicalInfo logicalInfo = new LogicalInfo();
            VisualInfo visualInfo = new VisualInfo();
            foreach(var region in regionsToSerialize) {
                RegionInfo logicalRegionInfo;
                RegionVisualInfo visualRegionInfo;
                region.GetInfo(out logicalRegionInfo, out visualRegionInfo);
                if(logicalRegionInfo != null) logicalInfo.Regions.Add(logicalRegionInfo);
                if(visualRegionInfo != null) visualInfo.Regions.Add(visualRegionInfo);
            }
            logicalState = LogicalInfo.Serialize(logicalInfo);
            visualState = VisualInfo.Serialize(visualInfo);
        }
        bool IModuleManagerBase.Restore(string logicalState, string visualState) {
            if(!allowSaveRestoreLayout)
                return false;
            VisualInfo visualInfo = VisualInfo.Deserialize(visualState);
            LogicalInfo logicalInfo = LogicalInfo.Deserialize(logicalState);
            var regionsFromLogicalInfo = logicalInfo != null ? logicalInfo.Regions.Select(x => x.RegionName) : new string[] { };
            var regionsFromVisualInfo = visualInfo != null ? visualInfo.Regions.Select(x => x.RegionName) : new string[] { };
            var regionNames = regionsFromLogicalInfo.Union(regionsFromVisualInfo);
            var regions = regionNames.Select(x => GetRegion(x)).ToList();

            Func<LogicalInfo, string, RegionInfo> getRegionInfo = (info, regionName) =>
                info != null ? info.Regions.FirstOrDefault(x => x.RegionName == regionName) : null;
            Func<VisualInfo, string, RegionVisualInfo> getRegionVisualInfo = (info, regionName) =>
                info != null ? info.Regions.FirstOrDefault(x => x.RegionName == regionName) : null;
            regions.ForEach(region => {
                if (region.LogicalSerializationMode == LogicalSerializationMode.Disabled)
                    return;
                if(getRegionInfo(logicalInfo, region.RegionName) != null)
                    ((IModuleManager)this).Clear(region.RegionName);
            });
            regions.ForEach(region => region.SetInfo(
                getRegionInfo(logicalInfo, region.RegionName),
                getRegionVisualInfo(visualInfo, region.RegionName)));
            if(logicalInfo != null) {
                regions.ForEach(x => x.ApplyInfo(true, false));
                regions.ForEach(x => x.ApplyInfo(false, true));
            }
            return logicalInfo != null;
        }
        #endregion
        #region IModuleManager
        bool IModuleManager.IsInjected(string regionName, string key) {
            var module = GetModule(regionName, key, false);
            if(module == null) return false;
            return GetRegion(regionName).Contains(key);
        }
        void IModuleManager.Inject(string regionName, string key, object parameter) {
            var module = GetModule(regionName, key, true);
            GetRegion(regionName).Inject(module, parameter);
        }
        void IModuleManager.Navigate(string regionName, string key) {
            GetRegion(regionName).Navigate(key);
        }
        void IModuleManager.Remove(string regionName, string key, bool raiseViewModelRemovingEvent) {
            if(!((IModuleManager)this).IsInjected(regionName, key)) return;
            var region = GetRegion(regionName);
            var vm = region.GetViewModel(key);
            if(vm != null && raiseViewModelRemovingEvent) {
                ViewModelRemovingEventArgs e = new ViewModelRemovingEventArgs(regionName, vm, key);
                RaiseViewModelRemovingEvent(regionName, e);
                if(e.Cancel) return;
            }
            region.Remove(key);
            if(vm != null) {
                ViewModelRemovedEventArgs e = new ViewModelRemovedEventArgs(regionName, vm, key);
                RaiseViewModelRemovedEvent(regionName, e);
            }
        }
        void IModuleManager.Clear(string regionName) {
            var region = GetRegion(regionName);
            var vms = region.ViewModels.ToDictionary(x => region.GetKey(x), x => x);
            GetRegion(regionName).Clear();
            foreach(var vm in vms) {
                ViewModelRemovedEventArgs e = new ViewModelRemovedEventArgs(regionName, vm.Value, vm.Key);
                RaiseViewModelRemovedEvent(regionName, e);
            }
        }
        #endregion
        #region IModuleWindowManager
        class WindowClosedHandler {
            readonly ModuleManager owner;
            readonly TaskCompletionSource<WindowInjectionResult> task;
            readonly Action<WindowClosedHandler> dispose;
            public string RegionName { get; private set; }
            public string Key { get; private set; }

            public WindowClosedHandler(string regionName, string key, ModuleManager owner, TaskCompletionSource<WindowInjectionResult> task, Action<WindowClosedHandler> dispose) {
                RegionName = regionName;
                Key = key;
                this.owner = owner;
                this.task = task;
                this.dispose = dispose;
                owner.GetEvents(regionName, true).ViewModelRemoved += OnWindowClosed;
            }
            void OnWindowClosed(object sender, ViewModelRemovedEventArgs e) {
                if(e.ViewModelKey != Key) return;
                var serv = owner.GetUIWindowRegion(e.RegionName);
                owner.GetEvents(e.RegionName, true).ViewModelRemoved -= OnWindowClosed;
                task.SetResult(new WindowInjectionResult(e.RegionName, e.ViewModel, e.ViewModelKey, serv != null ? serv.Result : null));
                dispose(this);
            }
        }
        readonly List<WindowClosedHandler> windowClosedHandlers = new List<WindowClosedHandler>();
        Task<WindowInjectionResult> GetWindowInjectionResult(string regionName, string key, bool returnNullIfNotShown) {
            if(returnNullIfNotShown && !((IModuleWindowManager)this).IsShown(regionName, key)) return null;
            var res = new TaskCompletionSource<WindowInjectionResult>();
            windowClosedHandlers.Add(new WindowClosedHandler(regionName, key, this, res, x => windowClosedHandlers.Remove(x)));
            return res.Task;
        }

        bool IModuleWindowManager.IsShown(string regionName, string key) {
            return ((IModuleManager)this).IsInjected(regionName, key);
        }
        Task<WindowInjectionResult> IModuleWindowManager.Show(string regionName, string key, object parameter) {
            var res = GetWindowInjectionResult(regionName, key, false);
            ((IModuleManager)this).Inject(regionName, key, parameter);
            return res;
        }
        void IModuleWindowManager.Activate(string regionName, string key) {
            ((IModuleManager)this).Navigate(regionName, key);
        }
        void IModuleWindowManager.Close(string regionName, string key, MessageBoxResult? dialogResult, bool raiseViewModelRemovingEvent) {
            var serv = GetUIWindowRegion(regionName);
            if(dialogResult != null && serv != null) serv.SetResult(dialogResult.Value);
            ((IModuleManager)this).Remove(regionName, key, raiseViewModelRemovingEvent);
        }
        void IModuleWindowManager.Clear(string regionName) {
            ((IModuleManager)this).Clear(regionName);
        }
        IUIWindowRegion GetUIWindowRegion(string regionName) {
            return GetRegion(regionName).UIRegions.OfType<IUIWindowRegion>().LastOrDefault();
        }
        #endregion
        #region IModuleManagerImplementation
        readonly IViewModelLocator viewModelLocator;
        readonly IViewLocator viewLocator;
        readonly IStateSerializer viewModelStateSerializer;
        readonly bool keepViewModelsAlive;
        bool IModuleManagerImplementation.KeepViewModelsAlive { get { return keepViewModelsAlive; } }
        IViewModelLocator IModuleManagerImplementation.ViewModelLocator { get { return viewModelLocator ?? Mvvm.ViewModelLocator.Default; } }
        IViewLocator IModuleManagerImplementation.ViewLocator { get { return viewLocator ?? ViewLocatorHelper.Default; } }
        IStateSerializer IModuleManagerImplementation.ViewModelStateSerializer { get { return viewModelStateSerializer ?? StateSerializer.Default; } }

        void IModuleManagerImplementation.OnNavigation(string regionName, NavigationEventArgs e) {
            var region = GetRegion(regionName);
            if(object.Equals(region.SelectedKey, e.NewViewModelKey)) return;
            region.OnNavigation(e.NewViewModelKey, e.NewViewModel);
            RaiseNavigationEvent(regionName, e);
        }
        void IModuleManagerImplementation.OnViewModelRemoving(string regionName, ViewModelRemovingEventArgs e) {
            var region = GetRegion(regionName);
            if(!region.Contains(e.ViewModelKey)) return;
            RaiseViewModelRemovingEvent(regionName, e);
        }
        void IModuleManagerImplementation.OnViewModelRemoved(string regionName, ViewModelRemovedEventArgs e) {
            var region = GetRegion(regionName);
            region.Remove(e.ViewModelKey);
            RaiseViewModelRemovedEvent(regionName, e);
        }
        void IModuleManagerImplementation.RaiseViewModelCreated(ViewModelCreatedEventArgs e) {
            RaiseViewModelCreatedEvent(e.RegionName, e);
        }

        IRegionImplementation IModuleManagerImplementation.GetRegionImplementation(string regionName) {
            return GetRegion(regionName);
        }
        Task<WindowInjectionResult> IModuleManagerImplementation.GetWindowInjectionResult(string regionName, string key) {
            return GetWindowInjectionResult(regionName, key, true);
        }
        #endregion
    }
}