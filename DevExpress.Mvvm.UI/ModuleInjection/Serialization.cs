#if !FREE
using DevExpress.Mvvm.ModuleInjection;
using DevExpress.Mvvm.ModuleInjection.Native;
using DevExpress.Mvvm.Native;
using DevExpress.Mvvm.UI.Native;
using DevExpress.Xpf.Core.Serialization;
using System;
using System.Linq;
using System.Windows;

namespace DevExpress.Mvvm.UI.ModuleInjection {
    public class VisualStateService : ServiceBaseGeneric<DependencyObject>, IVisualStateService, IVisualStateServiceImplementation {
        #region Static
        public static readonly DependencyProperty AutoSaveStateProperty;
        public static readonly DependencyProperty AutoRestoreStateProperty;

        static VisualStateService() {
            DXSerializer.EnabledProperty.OverrideMetadata(typeof(VisualStateService), new UIPropertyMetadata(false));
            DependencyPropertyRegistrator<VisualStateService>.New()
                .Register(x => x.AutoSaveState, out AutoSaveStateProperty, true)
                .Register(x => x.AutoRestoreState, out AutoRestoreStateProperty, true);
        }
        #endregion

        public bool AutoSaveState { get { return (bool)GetValue(AutoSaveStateProperty); } set { SetValue(AutoSaveStateProperty, value); } }
        public bool AutoRestoreState { get { return (bool)GetValue(AutoRestoreStateProperty); } set { SetValue(AutoRestoreStateProperty, value); } }
        public string DefaultState { get; private set; }
        protected DOTargetWrapper Target { get; private set; }

        public string GetCurrentState() {
            var ownerWindow = Window.GetWindow(AssociatedObject);
            if (ownerWindow != null && !ownerWindow.IsLoaded)
                return null;
            return SerializationHelper.SerializeToString(x =>
                DXSerializer.Serialize(Target.Object, x, string.Empty,
                new DXOptionsLayout() { AcceptNestedObjects = AcceptNestedObjects.IgnoreChildrenOfDisabledObjects }));
        }
        public string GetSavedState() {
            var region = GetRegion();
            var viewModel = GetViewModel();
            if(region == null || viewModel == null) return null;
            string state;
            region.GetSavedVisualState(viewModel, Name, out state);
            return state;
        }
        public void SaveState(string state) {
            var region = GetRegion();
            var viewModel = GetViewModel();
            if(string.IsNullOrEmpty(state) || region == null || viewModel == null) return;
            region.SaveVisualState(viewModel, Name, state);
        }
        public void RestoreState(string state) {
            if(string.IsNullOrEmpty(state)) return;
            SerializationHelper.DeserializeFromString(state, x =>
                DXSerializer.Deserialize(Target.Object, x, string.Empty,
                new DXOptionsLayout() { AcceptNestedObjects = AcceptNestedObjects.IgnoreChildrenOfDisabledObjects }));
        }
        void IVisualStateServiceImplementation.EnforceSaveState() {
            if(isLoaded && AutoSaveState) SaveState(GetCurrentState());
        }
        string IVisualStateServiceImplementation.Id { get { return Name; } }

        protected override void OnAttached() {
            base.OnAttached();
            Target = new DOTargetWrapper(AssociatedObject);
            if(Target.IsNull) ModuleInjectionException.CannotAttach();
            DXSerializer.SetEnabled(AssociatedObject, false);
            Target.Initialized += OnInitialized;
            Target.Loaded += OnLoaded;
            Target.Unloaded += OnUnloaded;
            if(Target.IsInitialized)
                OnInitialized(Target.Object, null);
            if(Target.IsLoaded)
                OnLoaded(Target.Object, null);
            CheckId();
        }
        protected override void OnDetaching() {
            Target.Initialized -= OnInitialized;
            Target.Loaded -= OnLoaded;
            Target.Unloaded -= OnUnloaded;
            Target = null;
            base.OnDetaching();
        }
        bool isLoaded = false;
        void OnInitialized(object sender, EventArgs e) {
            InitDefaultState();
        }
        void OnLoaded(object sender, EventArgs e) {
            if(isLoaded) return;
            isLoaded = true;
            GetServicesClient().Do(x => x.ServiceContainer.RegisterService(Name, this, YieldToParent));
            CheckId();
            InitDefaultState();
            if(AutoRestoreState) RestoreState(GetSavedState());
        }
        void OnUnloaded(object sender, EventArgs e) {
            if(!isLoaded) return;
            if(AutoSaveState) SaveState(GetCurrentState());
            isLoaded = false;
            GetServicesClient().Do(x => x.ServiceContainer.UnregisterService(this));
        }
        protected override void OnServicesClientChanged(ISupportServices oldServiceClient, ISupportServices newServiceClient) {
            if(!isLoaded) return;
            base.OnServicesClientChanged(oldServiceClient, newServiceClient);
        }
        void InitDefaultState() {
            if(!string.IsNullOrEmpty(DefaultState)) return;
            DefaultState = GetCurrentState();
        }

        void CheckId() {
            var vm = GetViewModel();
            if(vm == null) return;
            VisualStateServiceHelper.CheckServices(vm, true, true);
        }
        object GetViewModel() {
            var service = UIRegionBase.GetInheritedService(Target.Object);
            if(service == null) return null;
            return LayoutTreeHelper.GetVisualParents(Target.Object)
                .Select(x => new DOTargetWrapper(x).DataContext)
                .FirstOrDefault(x => ((IUIRegion)service).ViewModels.Contains(x));
        }
        IRegionImplementation GetRegion() {
            var service = UIRegionBase.GetInheritedService(Target.Object);
            if(service == null) return null;
            return service.ActualModuleManager.GetRegionImplementation(service.RegionName);
        }
    }
}
#endif