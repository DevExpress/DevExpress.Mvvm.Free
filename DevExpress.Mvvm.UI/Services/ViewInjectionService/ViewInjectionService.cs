using DevExpress.Mvvm.Native;
using DevExpress.Mvvm.UI.ViewInjection;
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
        static readonly DependencyPropertyKey ViewModelsPropertyKey =
            DependencyProperty.RegisterReadOnly("ViewModels", typeof(IEnumerable<object>), typeof(ViewInjectionService), new PropertyMetadata(null));
        public static readonly DependencyProperty ViewModelsProperty = ViewModelsPropertyKey.DependencyProperty;
        public static readonly DependencyProperty SelectedViewModelProperty =
            DependencyProperty.Register("SelectedViewModel", typeof(object), typeof(ViewInjectionService),
            new PropertyMetadata(null, (d,e) => ((ViewInjectionService)d).OnSelectedViewModelChanged(e)));
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
            get { return (IEnumerable<object>)GetValue(ViewModelsProperty); }
            private set { SetValue(ViewModelsPropertyKey, value); }
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
        IStrategyManager ActualStrategyManager { get { return StrategyManager ?? DevExpress.Mvvm.UI.ViewInjection.StrategyManager.Default; } }
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

            if(SelectedViewModel != null) Strategy.SelectedViewModel = SelectedViewModel;
            Strategy.SelectedViewModelChanged += OnStrategySelectedViewModelChanged;
            Strategy.ViewModelClosing += OnStrategyViewModelClosing;
        }
        protected override void OnDetaching() {
            Strategy.SelectedViewModelChanged -= OnStrategySelectedViewModelChanged;
            Strategy.ViewModelClosing -= OnStrategyViewModelClosing;

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
            Strategy.Initialize(this);
            ViewModels = Strategy.ViewModels;
        }

        void OnSelectedViewModelChanged(DependencyPropertyChangedEventArgs e) {
            if(IsAttached) Strategy.SelectedViewModel = e.NewValue;
        }
        void OnStrategySelectedViewModelChanged(object sender, PropertyValueChangedEventArgs<object> e) {
            SelectedViewModel = e.NewValue;
            SelectedViewModelChangedCommand.If(x => x.CanExecute(e)).Do(x => x.Execute(e));
            if(e.OldValue != null) ActualViewInjectionManager.RaiseNavigatedAwayEvent(e.OldValue);
            if(e.NewValue != null) ActualViewInjectionManager.RaiseNavigatedEvent(e.NewValue);
        }
        void OnStrategyViewModelClosing(object sender, ViewModelClosingEventArgs e) {
            ViewModelClosingCommand.If(x => x.CanExecute(e)).Do(x => x.Execute(e));
            ActualViewInjectionManager.RaiseViewModelClosingEvent(e);
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
            Strategy.Inject(viewModel, viewName, viewType);
            viewModels.Add(key, viewModel);
        }
        bool IViewInjectionService.Remove(object viewModel) {
            if(!Strategy.IsInitialized) return false;
            var res = Strategy.Remove(viewModel);
            if(res) {
                var key = ((IViewInjectionService)this).GetKey(viewModel);
                viewModels.Remove(key);
            }
            return res;
        }
    }
}