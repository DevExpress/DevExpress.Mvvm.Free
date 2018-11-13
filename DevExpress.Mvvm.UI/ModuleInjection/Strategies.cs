using DevExpress.Mvvm.Native;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace DevExpress.Mvvm.UI.ModuleInjection {
    public abstract class StrategyBase<T> : BaseStrategy<T>, IStrategy where T : DependencyObject {
        protected ObservableCollection<object> ViewModels { get; private set; }
        protected object SelectedViewModel {
            get { return selectedViewModel; }
            set {
                object oldValue = selectedViewModel;
                selectedViewModel = value;
                OnSelectedViewModelPropertyChanged(oldValue, selectedViewModel, focusOnSelectedViewModelChanged);
            }
        }
        object selectedViewModel;
        bool focusOnSelectedViewModelChanged = true;

        public StrategyBase() {
            ViewModels = new ObservableCollection<object>();
        }
        object IStrategy.SelectedViewModel { get { return SelectedViewModel; } }
        void IStrategy.Inject(object viewModel, Type viewType) {
            if(viewModel == null || ViewModels.Contains(viewModel)) return;
            ViewSelector.Add(viewModel, viewType);
            ViewModels.Add(viewModel);
            OnInjected(viewModel);
        }
        void IStrategy.Remove(object viewModel) {
            if(viewModel == null || !ViewModels.Contains(viewModel)) return;
            ViewSelector.Remove(viewModel);
            ViewModels.Remove(viewModel);
            OnRemoved(viewModel);
        }
        void IStrategy.Select(object viewModel, bool focus) {
            focusOnSelectedViewModelChanged = focus;
            SelectedViewModel = viewModel;
            focusOnSelectedViewModelChanged = true;
        }
        void IStrategy.Clear() {
            OnClearing();
            ViewModels.Clear();
            OnClear();
        }
        object IStrategy.GetParentViewModel() {
            return GetParentViewModel();
        }

        protected override void InitializeCore() {
            base.InitializeCore();
            Owner.ConfigureChild(Target);
        }
        protected virtual void OnInjected(object viewModel) { }
        protected virtual void OnRemoved(object viewModel) { }
        protected virtual void OnClearing() { }
        protected virtual void OnClear() { }
        protected virtual void OnSelectedViewModelPropertyChanged(object oldValue, object newValue, bool focus) {
            if(oldValue == newValue) return;
            OnSelectedViewModelChanged(oldValue, newValue);
        }
        protected virtual void OnSelectedViewModelChanged(object oldValue, object newValue) {
            Owner.SelectViewModel(newValue);
        }
        protected virtual object GetParentViewModel() {
            return (Target as FrameworkElement).With(x => x.DataContext) ?? (Target as FrameworkContentElement).With(x => x.DataContext);
        }
    }
    public abstract class CommonStrategyBase<T, TWrapper> : StrategyBase<T>
        where T : DependencyObject
        where TWrapper : class, ITargetWrapper<T>, new() {

        protected TWrapper Wrapper { get; private set; }
        protected override void InitializeCore() {
            base.InitializeCore();
            Wrapper = new TWrapper() { Target = this.Target };
        }
        protected override void UninitializeCore() {
            Wrapper.Target = null;
            Wrapper = null;
            base.UninitializeCore();
        }
    }
    public abstract class AggregateStrategy<T> : BaseStrategy<T>, IStrategy where T : DependencyObject {
        IStrategy actualStrategy;
        protected override void InitializeCore() {
            base.InitializeCore();
            if(actualStrategy == null)
                actualStrategy = SelectStrategy();
            actualStrategy.Initialize(Owner);
        }
        protected override void UninitializeCore() {
            actualStrategy.Uninitialize();
            base.UninitializeCore();
        }
        protected abstract IStrategy SelectStrategy();

        object IStrategy.SelectedViewModel { get { return actualStrategy.SelectedViewModel; } }
        void IStrategy.Inject(object viewModel, Type viewType) { actualStrategy.Inject(viewModel, viewType); }
        void IStrategy.Remove(object viewModel) { actualStrategy.Remove(viewModel); }
        void IStrategy.Select(object viewModel, bool focus) { actualStrategy.Select(viewModel, focus); }
        void IStrategy.Clear() { actualStrategy.Clear(); }
        object IStrategy.GetParentViewModel() { return actualStrategy.GetParentViewModel(); }
    }

    public interface ITargetWrapper<T> where T : DependencyObject {
        T Target { get; set; }
    }
    public interface IPanelWrapper<T> : ITargetWrapper<T> where T : DependencyObject {
        IEnumerable Children { get; }
        void AddChild(UIElement child);
        void RemoveChild(UIElement child);
    }
    public interface IContentPresenterWrapper<T> : ITargetWrapper<T> where T : DependencyObject {
        object Content { get; set; }
        DataTemplate ContentTemplate { get; set; }
        DataTemplateSelector ContentTemplateSelector { get; set; }
    }
    public interface IItemsControlWrapper<T> : ITargetWrapper<T> where T : DependencyObject {
        object ItemsSource { get; set; }
        DataTemplate ItemTemplate { get; set; }
        DataTemplateSelector ItemTemplateSelector { get; set; }
    }
    public interface ISelectorWrapper<T> : IItemsControlWrapper<T> where T : DependencyObject {
        object SelectedItem { get; set; }
        event EventHandler SelectionChanged;
    }

    public class PanelStrategy<T, TWrapper> : CommonStrategyBase<T, TWrapper>
        where T : DependencyObject
        where TWrapper : class, IPanelWrapper<T>, new() {

        protected override void OnInjected(object viewModel) {
            base.OnInjected(viewModel);
            Wrapper.AddChild(new ContentPresenter() { Content = viewModel, ContentTemplateSelector = ViewSelector });
        }
        protected override void OnRemoved(object viewModel) {
            base.OnRemoved(viewModel);
            var child = FindChild(viewModel);
            Wrapper.RemoveChild(child);
            if(viewModel == SelectedViewModel)
                SelectedViewModel = null;
        }
        protected override void OnClearing() {
            base.OnClearing();
            foreach(var vm in ViewModels) {
                var child = FindChild(vm);
                Wrapper.RemoveChild(child);
            }
        }
        protected override void OnClear() {
            base.OnClear();
            SelectedViewModel = null;
        }
        ContentPresenter FindChild(object viewModel) {
            return Wrapper.Children.Cast<ContentPresenter>().FirstOrDefault(x => x.Content == viewModel);
        }
    }
    public class ContentPresenterStrategy<T, TWrapper> : CommonStrategyBase<T, TWrapper>
        where T : DependencyObject
        where TWrapper : class, IContentPresenterWrapper<T>, new() {
        protected override void InitializeCore() {
            base.InitializeCore();
            isFirstInject = true;
            if(Wrapper.ContentTemplate == null && Wrapper.ContentTemplateSelector == null)
                Wrapper.ContentTemplateSelector = ViewSelector;
        }
        protected override void OnInjected(object viewModel) {
            base.OnInjected(viewModel);
            if(isFirstInject && SelectedViewModel == null)
                SelectedViewModel = viewModel;
            isFirstInject = false;
        }
        protected override void OnRemoved(object viewModel) {
            base.OnRemoved(viewModel);
            if(viewModel == SelectedViewModel)
                SelectedViewModel = null;
        }
        protected override void OnClear() {
            base.OnClear();
            SelectedViewModel = null;
        }
        protected override void OnSelectedViewModelChanged(object oldValue, object newValue) {
            base.OnSelectedViewModelChanged(oldValue, newValue);
            Wrapper.Content = newValue;
        }
        bool isFirstInject = true;
    }
    public class ItemsControlStrategy<T, TWrapper> : CommonStrategyBase<T, TWrapper>
        where T : DependencyObject
        where TWrapper : class, IItemsControlWrapper<T>, new() {
        protected override void InitializeCore() {
            base.InitializeCore();
            InitItemsSource();
            InitItemTemplate();
        }
        protected virtual void InitItemsSource() {
            Wrapper.ItemsSource = ViewModels;
        }
        protected virtual void InitItemTemplate() {
            if(Wrapper.ItemTemplate == null && Wrapper.ItemTemplateSelector == null)
                Wrapper.ItemTemplateSelector = ViewSelector;
        }
        protected override void OnRemoved(object viewModel) {
            base.OnRemoved(viewModel);
            if(viewModel == SelectedViewModel)
                SelectedViewModel = null;
        }
        protected override void OnClear() {
            base.OnClear();
            SelectedViewModel = null;
        }
    }
    public class SelectorStrategy<T, TWrapper> : ItemsControlStrategy<T, TWrapper>
        where T : DependencyObject
        where TWrapper : class, ISelectorWrapper<T>, new() {

        protected override void InitializeCore() {
            base.InitializeCore();
            Wrapper.SelectionChanged += OnTargetControlSelectionChanged;
        }
        protected override void UninitializeCore() {
            Wrapper.SelectionChanged -= OnTargetControlSelectionChanged;
            base.UninitializeCore();
        }
        protected override void OnSelectedViewModelChanged(object oldValue, object newValue) {
            base.OnSelectedViewModelChanged(oldValue, newValue);
            Wrapper.SelectedItem = newValue;
        }
        protected virtual void OnTargetControlSelectionChanged(object sender, EventArgs e) {
            SelectedViewModel = Wrapper.SelectedItem;
        }
    }

    public class PanelWrapper : IPanelWrapper<Panel> {
        public Panel Target { get; set; }
        public IEnumerable Children { get { return Target.Children; } }
        public void AddChild(UIElement child) { Target.Children.Add(child); }
        public void RemoveChild(UIElement child) { Target.Children.Remove(child); }
    }
    public class ContentPresenterWrapper : IContentPresenterWrapper<ContentPresenter> {
        public ContentPresenter Target { get; set; }
        public object Content { get { return Target.Content; } set { Target.Content = value; } }
        public DataTemplate ContentTemplate { get { return Target.ContentTemplate; } set { Target.ContentTemplate = value; } }
        public DataTemplateSelector ContentTemplateSelector { get { return Target.ContentTemplateSelector; } set { Target.ContentTemplateSelector = value; } }
    }
    public class ContentControlWrapper : IContentPresenterWrapper<ContentControl> {
        public ContentControl Target { get; set; }
        public object Content { get { return Target.Content; } set { Target.Content = value; } }
        public DataTemplate ContentTemplate { get { return Target.ContentTemplate; } set { Target.ContentTemplate = value; } }
        public DataTemplateSelector ContentTemplateSelector { get { return Target.ContentTemplateSelector; } set { Target.ContentTemplateSelector = value; } }
    }
    public class ItemsControlWrapper : IItemsControlWrapper<ItemsControl> {
        public ItemsControl Target { get; set; }
        public object ItemsSource { get { return Target.ItemsSource; } set { Target.ItemsSource = (IEnumerable)value; } }
        public virtual DataTemplate ItemTemplate { get { return Target.ItemTemplate; } set { Target.ItemTemplate = value; } }
        public virtual DataTemplateSelector ItemTemplateSelector { get { return Target.ItemTemplateSelector; } set { Target.ItemTemplateSelector = value; } }
    }
    public class SelectorWrapper : ItemsControlWrapper, ISelectorWrapper<Selector> {
        public new Selector Target { get { return (Selector)base.Target; } set { base.Target = value; } }
        public object SelectedItem { get { return Target.SelectedItem; } set { Target.SelectedItem = value; } }
        public event EventHandler SelectionChanged {
            add { Target.SelectionChanged += new SelectionChangedEventHandler(value); }
            remove { Target.SelectionChanged -= new SelectionChangedEventHandler(value); }
        }
    }
    public class TabControlWrapper : SelectorWrapper, ISelectorWrapper<TabControl> {
        public new TabControl Target { get { return (TabControl)base.Target; } set { base.Target = value; } }
        public override DataTemplate ItemTemplate { get { return Target.ContentTemplate; } set { Target.ContentTemplate = value; } }
        public override DataTemplateSelector ItemTemplateSelector { get { return Target.ContentTemplateSelector; } set { Target.ContentTemplateSelector = value; } }
    }
}