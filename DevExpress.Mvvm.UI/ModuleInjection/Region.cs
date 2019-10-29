using DevExpress.Mvvm.ModuleInjection;
using DevExpress.Mvvm.ModuleInjection.Native;
using DevExpress.Mvvm.Native;
using DevExpress.Mvvm.UI.Interactivity;
using DevExpress.Mvvm.UI.Native;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace DevExpress.Mvvm.UI.ModuleInjection {
    public abstract class UIRegionBase : ServiceBaseGeneric<DependencyObject>, IUIRegion {
        #region Static
        public static readonly DependencyProperty InheritedServiceProperty;
        public static readonly DependencyProperty ModuleManagerProperty;
        public static readonly DependencyProperty StrategyManagerProperty;
        public static readonly DependencyProperty RegionNameProperty;
        static readonly DependencyPropertyKey SelectedViewModelPropertyKey;
        public static readonly DependencyProperty SelectedViewModelProperty;
        public static readonly DependencyProperty SetParentViewModelProperty;
        public static readonly DependencyProperty ParentViewModelProperty;

        public static UIRegionBase GetInheritedService(DependencyObject obj) { return (UIRegionBase)obj.GetValue(InheritedServiceProperty); }
        public static void SetInheritedService(DependencyObject obj, UIRegionBase value) { obj.SetValue(InheritedServiceProperty, value); }

        static UIRegionBase() {
            InheritedServiceProperty = DependencyProperty.RegisterAttached("InheritedService", typeof(UIRegionBase), typeof(UIRegionBase),
                new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.Inherits | FrameworkPropertyMetadataOptions.OverridesInheritanceBehavior));
            DependencyPropertyRegistrator<UIRegionBase>.New()
                .Register(x => x.ModuleManager, out ModuleManagerProperty, null)
                .Register(x => x.StrategyManager, out StrategyManagerProperty, null)
                .Register(x => x.RegionName, out RegionNameProperty, null)
                .RegisterReadOnly(x => x.SelectedViewModel, out SelectedViewModelPropertyKey, out SelectedViewModelProperty, null, (x, oldValue, newValue) => x.OnSelectedViewModelPropertyChanged(oldValue, newValue))
                .Register(x => x.SetParentViewModel, out SetParentViewModelProperty, true)
                .Register(x => x.ParentViewModel, out ParentViewModelProperty, null, (x, oldValue, newValue) => x.OnParentViewModelChanged(oldValue, newValue));
        }
        protected static void EnforceSaveLayout(object viewModel) {
            if(viewModel == null) return;
            var services = VisualStateServiceHelper.GetServices(viewModel, false, true);
            services.ToList().ForEach(x => x.EnforceSaveState());
        }
        #endregion Static
        public IStrategyManager StrategyManager { get { return (IStrategyManager)GetValue(StrategyManagerProperty); } set { SetValue(StrategyManagerProperty, value); } }
        public IModuleManagerImplementation ModuleManager { get { return (IModuleManagerImplementation)GetValue(ModuleManagerProperty); } set { SetValue(ModuleManagerProperty, value); } }
        public string RegionName { get { return (string)GetValue(RegionNameProperty); } set { SetValue(RegionNameProperty, value); } }
        public object SelectedViewModel { get { return (object)GetValue(SelectedViewModelProperty); } protected set { SetValue(SelectedViewModelPropertyKey, value); } }
        public bool SetParentViewModel { get { return (bool)GetValue(SetParentViewModelProperty); } set { SetValue(SetParentViewModelProperty, value); } }
        public object ParentViewModel { get { return (object)GetValue(ParentViewModelProperty); } set { SetValue(ParentViewModelProperty, value); } }

        public UIRegionBase() {
            viewModels = new ObservableCollection<object>();
        }
        protected DOTargetWrapper Target { get; private set; }
        protected internal IModuleManagerImplementation ActualModuleManager { get { return ModuleManager ?? DevExpress.Mvvm.ModuleInjection.ModuleManager.DefaultImplementation; } }
        protected internal IStrategyManager ActualStrategyManager { get { return StrategyManager ?? DevExpress.Mvvm.UI.ModuleInjection.StrategyManager.Default; } }
        protected virtual object ActualParentViewModel { get { return ParentViewModel ?? Target.With(x => x.DataContext); } }
        protected override bool AllowAttachInDesignMode { get { return false; } }

        protected virtual void InitViewModels() {
            foreach(var vm in viewModels) InitViewModel(vm);
        }
        protected virtual void InitViewModel(object vm) {
            var iSupportPVM = vm as ISupportParentViewModel;
            if(!SetParentViewModel || iSupportPVM == null) return;
            if (iSupportPVM == ActualParentViewModel) {
                Trace.WriteLine(
                    "MIF: UIRegion (" + RegionName + ") " +
                    "failed to set ParentViewModel. " +
                    "Bind the UIRegion.ParentViewModel property manually.");
                return;
            }
            iSupportPVM.ParentViewModel = ActualParentViewModel;
        }
        protected virtual void ClearViewModel(object vm) {
            var iSupportPVM = vm as ISupportParentViewModel;
            if(!SetParentViewModel || iSupportPVM == null) return;
            iSupportPVM.ParentViewModel = null;
        }
        protected abstract void DoInject(object vm, Type vType);
        protected abstract void DoUninject(object vm);
        protected abstract void DoClear();
        protected abstract void OnSelectedViewModelChanged(object oldValue, object newValue, bool focus);
        void OnSelectedViewModelPropertyChanged(object oldValue, object newValue) {
            OnSelectedViewModelChanged(oldValue, newValue, focusOnSelectedViewModelChanged);
            var oldVMKey = ActualModuleManager.GetRegion(RegionName).GetKey(oldValue);
            var newVMKey = ActualModuleManager.GetRegion(RegionName).GetKey(newValue);
            var e = new NavigationEventArgs(RegionName, oldValue, newValue, oldVMKey, newVMKey);
            ActualModuleManager.OnNavigation(RegionName, e);
        }

        protected override void OnAttached() {
            base.OnAttached();
            Target = new DOTargetWrapper(AssociatedObject);
            if(Target.IsNull) ModuleInjectionException.CannotAttach();
            OnInitialized();
            Target.DataContextChanged += OnTargetDataContextChanged;
        }
        protected override void OnDetaching() {
            Target.DataContextChanged -= OnTargetDataContextChanged;
            OnUninitializing();
            Target = null;
            base.OnDetaching();
        }
        protected virtual void OnInitialized() {
            ActualModuleManager.GetRegionImplementation(RegionName).RegisterUIRegion(this);
        }
        protected virtual void OnUninitializing() {
            ActualModuleManager.GetRegionImplementation(RegionName).UnregisterUIRegion(this);
        }
        void OnParentViewModelChanged(object oldValue, object newValue) {
            InitViewModels();
        }
        void OnTargetDataContextChanged(object sender, DependencyPropertyChangedEventArgs e) {
            InitViewModels();
        }

        #region IUIRegion
        IEnumerable<object> IUIRegion.ViewModels { get { return viewModels; } }
        object IUIRegion.SelectedViewModel {
            get { return SelectedViewModel; }
            set {
                if(SelectedViewModel == value) {
                    OnSelectedViewModelChanged(SelectedViewModel, SelectedViewModel, focusOnSelectedViewModelChanged);
                    return;
                }
                SelectedViewModel = value;
            }
        }
        bool focusOnSelectedViewModelChanged = true;

        void IUIRegion.Inject(object viewModel, Type viewType) {
            if(viewModel == null) return;
            viewModels.Add(viewModel);
            InitViewModel(viewModel);
            DoInject(viewModel, viewType);
        }
        void IUIRegion.Remove(object viewModel) {
            if(!viewModels.Contains(viewModel)) return;
            EnforceSaveLayout(viewModel);
            DoUninject(viewModel);
            viewModels.Remove(viewModel);
            ClearViewModel(viewModel);
        }
        void IUIRegion.Clear() {
            List<object> vms = new List<object>(viewModels);
            DoClear();
            foreach(var vm in vms) ClearViewModel(vm);
            viewModels.Clear();
            SelectedViewModel = null;
        }
        readonly ObservableCollection<object> viewModels;

        void IUIRegion.SelectViewModel(object vm, bool focus) {
            focusOnSelectedViewModelChanged = focus;
            ((IUIRegion)this).SelectedViewModel = vm;
            focusOnSelectedViewModelChanged = true;
        }
        object IUIRegion.GetView(object viewModel) {
            return GetView(viewModel);
        }
        protected abstract object GetView(object viewModel);
        #endregion
        #region StrategyOwner
        protected class StrategyOwnerBase : IStrategyOwner {
            public UIRegionBase Owner { get; private set; }
            public DependencyObject Target { get; private set; }
            public string RegionName { get { return Owner.RegionName; } }

            public StrategyOwnerBase(UIRegionBase owner, DependencyObject target) {
                Owner = owner;
                Target = target;
            }
            public string GetKey(object viewModel) {
                return Owner.ActualModuleManager.GetRegion(RegionName).GetKey(viewModel);
            }
            public virtual void SelectViewModel(object viewModel) {
                Owner.SelectedViewModel = viewModel;
            }
            public virtual void RemoveViewModel(object viewModel) {
                var key = Owner.ActualModuleManager.GetRegion(RegionName).GetKey(viewModel);
                Owner.viewModels.Remove(viewModel);
                Owner.ClearViewModel(viewModel);
                Owner.ActualModuleManager.OnViewModelRemoved(RegionName, new ViewModelRemovedEventArgs(RegionName, viewModel, key));
            }
            public virtual bool CanRemoveViewModel(object viewModel) {
                if(viewModel == null) return true;
                EnforceSaveLayout(viewModel);
                var key = Owner.ActualModuleManager.GetRegion(RegionName).GetKey(viewModel);
                var e = new ViewModelRemovingEventArgs(RegionName, viewModel, key);
                Owner.ActualModuleManager.OnViewModelRemoving(RegionName, e);
                return !e.Cancel;
            }
            public virtual void ConfigureChild(DependencyObject child) {
                SetInheritedService(child, Owner);
            }
        }
        #endregion
    }
    public class UIRegion : UIRegionBase {
        #region Static
        public static readonly DependencyProperty RegionProperty;
        public static string GetRegion(DependencyObject obj) { return (string)obj.GetValue(RegionProperty); }
        public static void SetRegion(DependencyObject obj, string value) { obj.SetValue(RegionProperty, value); }
        static void OnRegionChanged(DependencyObject obj, string oldValue, string newValue) {
            BehaviorCollection bCol = Interaction.GetBehaviors(obj);
            UIRegion oldUIRegion = bCol.OfType<UIRegion>().FirstOrDefault();
            if(oldUIRegion != null)
                bCol.Remove(oldUIRegion);
            if(!string.IsNullOrEmpty(newValue))
                bCol.Add(new UIRegion() { RegionName = newValue });
        }
        static UIRegion() {
            RegionProperty =
                DependencyProperty.RegisterAttached("Region", typeof(string), typeof(UIRegion),
                new PropertyMetadata(null, (d, e) => OnRegionChanged(d, (string)e.OldValue, (string)e.NewValue)));
        }
        #endregion Static
        protected override object ActualParentViewModel { get { return ParentViewModel ?? Strategy.GetParentViewModel(); } }
        IStrategy Strategy { get { return strategy ?? (strategy = ActualStrategyManager.CreateStrategy(AssociatedObject)); } }
        IStrategy strategy = null;

        protected override void OnAttached() {
            base.OnAttached();
            Target.Loaded += OnLoaded;
            if(Target.IsLoaded)
                OnLoaded(Target.Object, EventArgs.Empty);
        }
        protected override void OnDetaching() {
            Target.Loaded -= OnLoaded;
            base.OnDetaching();
        }
        protected override void OnInitialized() {
            Strategy.Initialize(new StrategyOwner(this));
            base.OnInitialized();
            OnSelectedViewModelChanged(SelectedViewModel, SelectedViewModel, false);
        }
        protected override void OnUninitializing() {
            Strategy.Uninitialize();
            base.OnUninitializing();
        }
        protected override object GetView(object viewModel) {
            return Strategy.GetView(viewModel);
        }
        protected override void DoInject(object vm, Type viewType) {
            Strategy.Inject(vm, viewType);
        }
        protected override void DoUninject(object vm) {
            Strategy.Remove(vm);
        }
        protected override void DoClear() {
            Strategy.Clear();
        }
        protected override void OnSelectedViewModelChanged(object oldValue, object newValue, bool focus) {
            if(Strategy.IsInitialized && Target.IsLoaded) Strategy.Select(newValue, focus);
        }
        void OnLoaded(object sender, EventArgs e) {
            if(SelectedViewModel != null)
                OnSelectedViewModelChanged(SelectedViewModel, SelectedViewModel, false);
            else SelectedViewModel = Strategy.SelectedViewModel;
        }

        #region StrategyOwner
        class StrategyOwner : StrategyOwnerBase {
            protected new UIRegion Owner { get { return (UIRegion)base.Owner; } }
            public StrategyOwner(UIRegion owner)
                : base(owner, owner.AssociatedObject) { }
            public override void SelectViewModel(object viewModel) {
                if(Owner.Target != null && Owner.Target.IsLoaded)
                    base.SelectViewModel(viewModel);
            }
        }
        #endregion
    }
    public class UIWindowRegion : UIRegionBase, IUIWindowRegion {
        #region Static
        public static readonly DependencyProperty WindowFactoryProperty;
        public static readonly DependencyProperty WindowStyleProperty;
        public static readonly DependencyProperty WindowStyleSelectorProperty;
        public static readonly DependencyProperty WindowShowModeProperty;
        public static readonly DependencyProperty WindowStartupLocationProperty;
        public static readonly DependencyProperty SetWindowOwnerProperty;
        public static readonly DependencyProperty IsMainWindowProperty;

        static UIWindowRegion() {
            DependencyPropertyRegistrator<UIWindowRegion>.New()
                .Register(x => x.WindowFactory, out WindowFactoryProperty, null)
                .Register(x => x.WindowStyle, out WindowStyleProperty, null)
                .Register(x => x.WindowStyleSelector, out WindowStyleSelectorProperty, null)
                .Register(x => x.WindowShowMode, out WindowShowModeProperty, WindowShowMode.Default)
                .Register(x => x.WindowStartupLocation, out WindowStartupLocationProperty, WindowStartupLocation.CenterScreen)
                .Register(x => x.SetWindowOwner, out SetWindowOwnerProperty, true)
                .Register(x => x.IsMainWindow, out IsMainWindowProperty, false);
        }
        #endregion
        public DataTemplate WindowFactory { get { return (DataTemplate)GetValue(WindowFactoryProperty); } set { SetValue(WindowFactoryProperty, value); } }
        public Style WindowStyle { get { return (Style)GetValue(WindowStyleProperty); } set { SetValue(WindowStyleProperty, value); } }
        public StyleSelector WindowStyleSelector { get { return (StyleSelector)GetValue(WindowStyleSelectorProperty); } set { SetValue(WindowStyleSelectorProperty, value); } }
        public WindowShowMode WindowShowMode { get { return (WindowShowMode)GetValue(WindowShowModeProperty); } set { SetValue(WindowShowModeProperty, value); } }
        public WindowStartupLocation WindowStartupLocation { get { return (WindowStartupLocation)GetValue(WindowStartupLocationProperty); } set { SetValue(WindowStartupLocationProperty, value); } }
        public bool SetWindowOwner { get { return (bool)GetValue(SetWindowOwnerProperty); } set { SetValue(SetWindowOwnerProperty, value); } }
        public bool IsMainWindow { get { return (bool)GetValue(IsMainWindowProperty); } set { SetValue(IsMainWindowProperty, value); } }

        protected override object GetView(object viewModel) {
            return GetStrategy(viewModel).With(x => x.GetView(viewModel));
        }
        protected override void DoInject(object vm, Type viewType) {
            var strategy = CreateStrategy(vm);
            if(WindowShowMode == WindowShowMode.Default)
                strategy.Show(vm, viewType);
            else if(WindowShowMode == WindowShowMode.Dialog)
                strategy.ShowDialog(vm, viewType);
        }
        protected override void DoUninject(object vm) {
            GetStrategy(vm).Do(x => x.Close());
            RemoveStrategy(vm);
        }
        protected override void DoClear() {
            DoForeachStrategy(x => x.Close());
            ClearStrategies();
        }
        protected override void OnSelectedViewModelChanged(object oldValue, object newValue, bool focus) {
            GetStrategy(newValue).Do(x => x.Activate());
        }
        protected virtual FrameworkElement CreateWindow(object vm) {
            Window w;
            if(WindowFactory != null) {
                w = (Window)WindowFactory.LoadContent();
            } else {
                w = new Window();
            }
            w.WindowStartupLocation = WindowStartupLocation;
            if(WindowStyle != null) w.Style = WindowStyle;
            if(WindowStyleSelector != null) w.Style = WindowStyleSelector.SelectStyle(vm, w);
            if(SetWindowOwner && !IsMainWindow && Application.Current != null) {
                w.Owner =
                    Application.Current.Windows.OfType<Window>().FirstOrDefault(x => x.IsActive)
                    ?? Application.Current.MainWindow;
            }
            if(!IsMainWindow && AssociatedObject is FrameworkElement)
                ViewServiceBase.UpdateThemeName(w, (FrameworkElement)AssociatedObject);
            if(IsMainWindow && Application.Current != null)
                Application.Current.MainWindow = w;
            return w;
        }

        readonly Dictionary<object, IWindowStrategy> strategies = new Dictionary<object, IWindowStrategy>();
        IWindowStrategy CreateStrategy(object vm) {
            var window = CreateWindow(vm);
            var strategy = ActualStrategyManager.CreateWindowStrategy(window);
            strategy.Initialize(new StrategyOwner(this, window));
            strategies.Add(vm, strategy);
            return strategy;
        }
        IWindowStrategy GetStrategy(object vm) {
            if(vm == null || !strategies.ContainsKey(vm)) return null;
            return strategies[vm];
        }
        void RemoveStrategy(object vm) {
            var strategy = GetStrategy(vm);
            if(strategy == null) return;
            result = strategy.Result;
            strategies.Remove(vm);
            if (SelectedViewModel == vm)
                SelectedViewModel = null;
        }
        void ClearStrategies() {
            foreach(var strategy in strategies.Values)
                strategy.Close();
            strategies.Clear();
            result = null;
            setResult = null;
        }
        void DoForeachStrategy(Action<IWindowStrategy> action) {
            strategies.Values.ToList().ForEach(action);
        }

        #region IUIWindowRegion
        MessageBoxResult? setResult;
        MessageBoxResult? result;
        MessageBoxResult? IUIWindowRegion.Result { get { return setResult ?? result; } }
        void IUIWindowRegion.SetResult(MessageBoxResult result) {
            setResult = result;
        }
        #endregion
        #region StrategyOwner
        class StrategyOwner : StrategyOwnerBase {
            public new UIWindowRegion Owner { get { return (UIWindowRegion)base.Owner; } }
            public StrategyOwner(UIWindowRegion owner, FrameworkElement window)
                : base(owner, window) { }
            public override void RemoveViewModel(object viewModel) {
                Owner.RemoveStrategy(viewModel);
                base.RemoveViewModel(viewModel);
            }
            public override void ConfigureChild(DependencyObject child) {
                base.ConfigureChild(child);
            }
        }
        #endregion
    }
}