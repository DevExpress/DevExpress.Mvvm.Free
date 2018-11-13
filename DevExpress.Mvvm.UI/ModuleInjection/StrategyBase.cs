using DevExpress.Mvvm.Native;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace DevExpress.Mvvm.UI.ModuleInjection {
    public interface IStrategyOwner {
        string RegionName { get; }
        DependencyObject Target { get; }

        string GetKey(object viewModel);
        void SelectViewModel(object viewModel);
        void RemoveViewModel(object viewModel);
        bool CanRemoveViewModel(object viewModel);
        void ConfigureChild(DependencyObject child);
    }
    public interface IBaseStrategy {
        IStrategyOwner Owner { get; }
        bool IsInitialized { get; }
        void Initialize(IStrategyOwner owner);
        void Uninitialize();
        object GetView(object viewModel);
    }
    public interface IStrategy : IBaseStrategy {
        object SelectedViewModel { get; }
        void Inject(object viewModel, Type viewType);
        void Remove(object viewModel);
        void Select(object viewModel, bool focus);
        void Clear();
        object GetParentViewModel();
    }
    public interface IWindowStrategy : IBaseStrategy {
        MessageBoxResult? Result { get; }
        void Show(object viewModel, Type viewType);
        void ShowDialog(object viewModel, Type viewType);
        void Activate();
        void Close();
    }
    public abstract class BaseStrategy<T> : IBaseStrategy where T : DependencyObject {
        public bool IsInitialized { get; private set; }
        public IStrategyOwner Owner { get; private set; }
        protected T Target { get { return (T)Owner.Target; } }
        protected ViewDataTemplateSelector ViewSelector { get; private set; }

        public BaseStrategy() {
            ViewSelector = new ViewDataTemplateSelector();
        }
        protected virtual void InitializeCore() { }
        protected virtual void UninitializeCore() { }
        protected virtual object GetView(object viewModel) {
            return Target;
        }
        void IBaseStrategy.Initialize(IStrategyOwner owner) {
            if(IsInitialized) return;
            IsInitialized = true;
            Owner = owner;
            InitializeCore();
        }
        void IBaseStrategy.Uninitialize() {
            if(!IsInitialized) return;
            UninitializeCore();
            IsInitialized = false;
        }
        object IBaseStrategy.GetView(object viewModel) {
            return GetView(viewModel);
        }
    }
    public class ViewDataTemplateSelector : DataTemplateSelector {
        readonly List<ViewModelInfo> infos;
        public ViewDataTemplateSelector() {
            this.infos = new List<ViewModelInfo>();
        }
        public override DataTemplate SelectTemplate(object item, DependencyObject container) {
            return GetViewTemplate(item) ?? base.SelectTemplate(item, container);
        }
        public void Add(object viewModel, Type viewType) {
            UpdateInfos();
            infos.Add(new ViewModelInfo(viewModel, viewType));
        }
        public void Remove(object viewModel) {
            UpdateInfos();
        }
        void UpdateInfos() {
            List<ViewModelInfo> toRemove = new List<ViewModelInfo>();
            foreach(var info in infos)
                if(!info.ViewModel.IsAlive) toRemove.Add(info);
            foreach(var info in toRemove)
                infos.Remove(info);
        }
        DataTemplate GetViewTemplate(object viewModel) {
            if(viewModel == null) return null;
            UpdateInfos();
            var info = infos.FirstOrDefault(x => x.ViewModel.Target == viewModel);
            if(info == null) return null;
            info.UpdateViewTemplate();
            return info.ViewTemplate;
        }
        class ViewModelInfo {
            public WeakReference ViewModel { get; private set; }
            public Type ViewType { get; private set; }
            public DataTemplate ViewTemplate { get; private set; }
            public ViewModelInfo(object viewModel, Type viewType) {
                ViewModel = new WeakReference(viewModel);
                ViewType = viewType;
            }
            public void UpdateViewTemplate() {
                if(ViewTemplate != null) return;
                if(ViewType != null) {
                    ViewTemplate = ViewLocatorExtensions.CreateViewTemplate(ViewType);
                    return;
                }
                ViewTemplate = null;
                return;
            }
        }
    }

    public class DOTargetWrapper {
        public readonly DependencyObject Object;
        FrameworkElement FE { get { return Object as FrameworkElement; } }
        FrameworkContentElement FCE { get { return Object as FrameworkContentElement; } }
        public DOTargetWrapper(DependencyObject obj) { Object = obj; }

        public bool IsNull { get { return FE == null && FCE == null; } }
        public bool IsInitialized { get { return FE.Return(x => x.IsInitialized, () => FCE.IsInitialized); } }
        public bool IsLoaded { get { return FE.Return(x => x.IsLoaded, () => FCE.IsLoaded); } }
        public object DataContext { get { return FE.Return(x => x.DataContext, () => FCE.DataContext); } }

        public event EventHandler Initialized {
            add { FE.Do(x => x.Initialized += value); FCE.Do(x => x.Initialized += value); }
            remove { FE.Do(x => x.Initialized -= value); FCE.Do(x => x.Initialized -= value); }
        }
        public event RoutedEventHandler Loaded {
            add { FE.Do(x => x.Loaded += value); FCE.Do(x => x.Loaded += value); }
            remove { FE.Do(x => x.Loaded -= value); FCE.Do(x => x.Loaded -= value); }
        }
        public event RoutedEventHandler Unloaded {
            add { FE.Do(x => x.Unloaded += value); FCE.Do(x => x.Unloaded += value); }
            remove { FE.Do(x => x.Unloaded -= value); FCE.Do(x => x.Unloaded -= value); }
        }
        public event DependencyPropertyChangedEventHandler DataContextChanged {
            add { FE.Do(x => x.DataContextChanged += value); FCE.Do(x => x.DataContextChanged += value); }
            remove { FE.Do(x => x.DataContextChanged -= value); FCE.Do(x => x.DataContextChanged -= value); }
        }
    }
}