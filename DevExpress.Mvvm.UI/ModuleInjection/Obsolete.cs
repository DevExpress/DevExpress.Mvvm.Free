using DevExpress.Mvvm.Native;
using DevExpress.Mvvm.UI.ModuleInjection;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace DevExpress.Mvvm.UI {
    public sealed class ViewInjectionService : ServiceBase, IViewInjectionService {
        #region Static
        const string Exception = "A view model with the same key already exists in the {0} region.";

        public static readonly DependencyProperty ViewInjectionManagerProperty =
            DependencyProperty.Register("ViewInjectionManager", typeof(IViewInjectionManager), typeof(ViewInjectionService),
            new PropertyMetadata(null));
        public static readonly DependencyProperty StrategyManagerProperty =
            DependencyProperty.Register("StrategyManager", typeof(IStrategyManager), typeof(ViewInjectionService), new PropertyMetadata(null));
        public static readonly DependencyProperty ViewLocatorProperty =
            DependencyProperty.Register("ViewLocator", typeof(IViewLocator), typeof(ViewInjectionService),
            new PropertyMetadata(null));
        public static readonly DependencyProperty RegionNameProperty =
            DependencyProperty.Register("RegionName", typeof(string), typeof(ViewInjectionService),
            new PropertyMetadata(null));
        public static readonly DependencyProperty SelectedViewModelProperty =
            DependencyProperty.Register("SelectedViewModel", typeof(object), typeof(ViewInjectionService),
            new PropertyMetadata(null, (d, e) => ((ViewInjectionService)d).OnSelectedViewModelChanged(e)));
        public static readonly DependencyProperty SelectedViewModelChangedCommandProperty =
            DependencyProperty.Register("SelectedViewModelChangedCommand", typeof(ICommand), typeof(ViewInjectionService), new PropertyMetadata(null));
        public static readonly DependencyProperty ViewModelClosingCommandProperty =
            DependencyProperty.Register("ViewModelClosingCommand", typeof(ICommand), typeof(ViewInjectionService), new PropertyMetadata(null));
        #endregion
        public IViewInjectionManager ViewInjectionManager {
            get { return (IViewInjectionManager)GetValue(ViewInjectionManagerProperty); }
            set { SetValue(ViewInjectionManagerProperty, value); }
        }
        public IStrategyManager StrategyManager {
            get { return (IStrategyManager)GetValue(StrategyManagerProperty); }
            set { SetValue(StrategyManagerProperty, value); }
        }
        public IViewLocator ViewLocator {
            get { return (IViewLocator)GetValue(ViewLocatorProperty); }
            set { SetValue(ViewLocatorProperty, value); }
        }
        public string RegionName {
            get { return (string)GetValue(RegionNameProperty); }
            set { SetValue(RegionNameProperty, value); }
        }
        public IEnumerable<object> ViewModels {
            get { return viewModels.Values; }
        }
        public object SelectedViewModel {
            get { return (object)GetValue(SelectedViewModelProperty); }
            set { SetValue(SelectedViewModelProperty, value); }
        }
        public ICommand SelectedViewModelChangedCommand {
            get { return (ICommand)GetValue(SelectedViewModelChangedCommandProperty); }
            set { SetValue(SelectedViewModelChangedCommandProperty, value); }
        }
        public ICommand ViewModelClosingCommand {
            get { return (ICommand)GetValue(ViewModelClosingCommandProperty); }
            set { SetValue(ViewModelClosingCommandProperty, value); }
        }
        protected override bool AllowAttachInDesignMode { get { return false; } }

        IViewInjectionManager ActualViewInjectionManager { get { return ViewInjectionManager ?? DevExpress.Mvvm.ViewInjectionManager.Default; } }
        IStrategyManager ActualStrategyManager { get { return StrategyManager ?? DevExpress.Mvvm.UI.ModuleInjection.StrategyManager.Default; } }
        [EditorBrowsable(EditorBrowsableState.Never)]
        public IStrategy Strategy { get { return strategy ?? (strategy = ActualStrategyManager.CreateStrategy(AssociatedObject)); } }
        IStrategy strategy = null;

        protected override void OnAttached() {
            base.OnAttached();
            if(AssociatedObject.IsLoaded) OnAssociatedObjectLoaded(AssociatedObject, EventArgs.Empty);
            AssociatedObject.Loaded += OnAssociatedObjectLoaded;
            AssociatedObject.Unloaded += OnAssociatedObjectUnloaded;
            if(AssociatedObject.IsInitialized) OnAssociatedObjectInitialized(AssociatedObject, EventArgs.Empty);
            AssociatedObject.Initialized += OnAssociatedObjectInitialized;
            if(SelectedViewModel != null) Strategy.Select(SelectedViewModel, false);
        }
        protected override void OnDetaching() {
            AssociatedObject.Initialized -= OnAssociatedObjectInitialized;
            AssociatedObject.Loaded -= OnAssociatedObjectLoaded;
            AssociatedObject.Unloaded -= OnAssociatedObjectUnloaded;
            OnAssociatedObjectUnloaded(AssociatedObject, EventArgs.Empty);
            base.OnDetaching();
        }
        void OnAssociatedObjectLoaded(object sender, EventArgs e) {
            if(AssociatedObject.IsInitialized) OnAssociatedObjectInitialized(sender, EventArgs.Empty);
            ActualViewInjectionManager.RegisterService(this);
        }
        void OnAssociatedObjectUnloaded(object sender, EventArgs e) {
            ActualViewInjectionManager.UnregisterService(this);
            Strategy.Uninitialize();
        }
        void OnAssociatedObjectInitialized(object sender, EventArgs e) {
            Strategy.Initialize(new StrategyOwner(this));
        }
        void OnSelectedViewModelChanged(DependencyPropertyChangedEventArgs e) {
            SelectedViewModel = e.NewValue;
            SelectedViewModelChangedCommand.If(x => x.CanExecute(e)).Do(x => x.Execute(e));
            if(e.OldValue != null) ActualViewInjectionManager.RaiseNavigatedAwayEvent(e.OldValue);
            if(e.NewValue != null) ActualViewInjectionManager.RaiseNavigatedEvent(e.NewValue);
            if(IsAttached) Strategy.Select(e.NewValue, true);
        }

        Dictionary<object, object> viewModels = new Dictionary<object, object>();
        object IViewInjectionService.GetKey(object viewModel) {
            return viewModels.FirstOrDefault(x => x.Value == viewModel).Key;
        }
        void IViewInjectionService.Inject(object key, object viewModel, string viewName, Type viewType) {
            if(viewModel == null || !Strategy.IsInitialized) return;
            if(key == null) key = viewModel;
            if(key != null && viewModels.ContainsKey(key)) {
                if(ViewModelBase.IsInDesignMode) return;
                throw new InvalidOperationException(string.Format(Exception, string.IsNullOrEmpty(RegionName) ? "ViewInjectionService" : RegionName));
            }
            if(viewModels.ContainsKey(key)) return;
            Strategy.Inject(viewModel, viewType ?? (ViewLocator ?? DevExpress.Mvvm.UI.ViewLocator.Default).ResolveViewType(viewName));
            viewModels.Add(key, viewModel);
        }
        bool IViewInjectionService.Remove(object viewModel) {
            if(!Strategy.IsInitialized) return false;
            if(!CanRemoveCore(viewModel)) return false;
            Strategy.Remove(viewModel);
            RemoveCore(viewModel);
            return true;
        }
        bool CanRemoveCore(object viewModel) {
            if(viewModel == null || !viewModels.ContainsValue(viewModel)) return true;
            ViewModelClosingEventArgs e = new ViewModelClosingEventArgs(viewModel);
            ViewModelClosingCommand.If(x => x.CanExecute(e)).Do(x => x.Execute(e));
            ActualViewInjectionManager.RaiseViewModelClosingEvent(e);
            return !e.Cancel;
        }
        void RemoveCore(object viewModel) {
            var key = ((IViewInjectionService)this).GetKey(viewModel);
            if(key == null) return;
            viewModels.Remove(key);
        }

        #region StrategyOwner
        class StrategyOwner : IStrategyOwner {
            readonly ViewInjectionService owner;
            public StrategyOwner(ViewInjectionService owner) {
                this.owner = owner;
            }

            public string RegionName { get { return owner.RegionName; } }
            public DependencyObject Target { get { return owner.AssociatedObject; } }
            public string GetKey(object viewModel) {
                return ((IViewInjectionService)owner).GetKey(viewModel) as string;
            }
            public bool CanRemoveViewModel(object viewModel) {
                return owner.CanRemoveCore(viewModel);
            }
            public void RemoveViewModel(object viewModel) {
                owner.RemoveCore(viewModel);
            }
            public void SelectViewModel(object viewModel) {
                owner.SelectedViewModel = viewModel;
            }
            public void ConfigureChild(DependencyObject child) { }
        }
        #endregion
    }
}