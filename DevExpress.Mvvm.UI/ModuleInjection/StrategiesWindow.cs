using DevExpress.Mvvm.UI.Native;
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace DevExpress.Mvvm.UI.ModuleInjection {
    public abstract class WindowStrategyBase<T> : BaseStrategy<T>, IWindowStrategy where T : DependencyObject {
        public object ViewModel { get; private set; }
        public MessageBoxResult? Result { get; private set; }

        protected override void UninitializeCore() {
            if(ViewModel != null) ViewSelector.Remove(ViewModel);
            ViewModel = null;
            base.UninitializeCore();
        }
        protected abstract void ShowCore();
        protected abstract MessageBoxResult ShowDialogCore();
        protected abstract void AfterShowDialogCore();
        protected abstract void ActivateCore();
        protected abstract void CloseCore();
        void BeforeShow(object viewModel, Type viewType) {
            ViewModel = viewModel;
            if(viewType != null) ViewSelector.Add(ViewModel, viewType);
        }

        void IWindowStrategy.Show(object viewModel, Type viewType) {
            BeforeShow(viewModel, viewType);
            ShowCore();
        }
        void IWindowStrategy.ShowDialog(object viewModel, Type viewType) {
            BeforeShow(viewModel, viewType);
            Result = ShowDialogCore();
            AfterShowDialogCore();
        }
        void IWindowStrategy.Activate() {
            ActivateCore();
        }
        void IWindowStrategy.Close() {
            if(!IsInitialized) return;
            CloseCore();
            ((IBaseStrategy)this).Uninitialize();
        }
    }
    public class WindowStrategy<T, TWrapper> : WindowStrategyBase<T>
       where T : DependencyObject
       where TWrapper : class, IWindowWrapper<T>, new() {

        protected TWrapper Wrapper { get; private set; }
        protected override void InitializeCore() {
            base.InitializeCore();
            Wrapper = new TWrapper() { Target = this.Target };
        }
        protected override void UninitializeCore() {
            Unsubscribe();
            Wrapper.Target = null;
            Wrapper = null;
            base.UninitializeCore();
        }

        protected override void ShowCore() {
            ConfigureWrapper();
            Wrapper.Show();
        }
        protected override MessageBoxResult ShowDialogCore() {
            ConfigureWrapper();
            suppressClosed = true;
            var res = Wrapper.ShowDialog();
            suppressClosed = false;
            return res;
        }
        protected override void AfterShowDialogCore() {
            OnWindowClosed(null, EventArgs.Empty);
        }
        protected override void ActivateCore() {
            Wrapper.Activate();
        }
        protected override void CloseCore() {
            Unsubscribe();
            Wrapper.Close();
        }

        protected virtual void ConfigureWrapper() {
            Wrapper.DataContext = ViewModel;
            if(Wrapper.Content == null)
                Wrapper.Content = ViewModel;
            if(Wrapper.ContentTemplate == null && Wrapper.ContentTemplateSelector == null)
                Wrapper.ContentTemplateSelector = ViewSelector;
            Owner.ConfigureChild(Wrapper.Target);
            Subscribe();
        }
        protected virtual void Subscribe() {
            Unsubscribe();
            Wrapper.Closing += OnWindowClosing;
            Wrapper.Closed += OnWindowClosed;
            Wrapper.Activated += OnWindowActivated;
        }
        protected virtual void Unsubscribe() {
            Wrapper.Closing -= OnWindowClosing;
            Wrapper.Closed -= OnWindowClosed;
            Wrapper.Activated -= OnWindowActivated;
        }

        void OnWindowClosing(object sender, CancelEventArgs e) {
            if(e.Cancel) return;
            if(!Owner.CanRemoveViewModel(ViewModel))
                e.Cancel = true;
        }
        bool suppressClosed = false;
        void OnWindowClosed(object sender, EventArgs e) {
            if (suppressClosed) return;
            Owner.RemoveViewModel(ViewModel);
        }
        void OnWindowActivated(object sender, EventArgs e) {
            Owner.SelectViewModel(ViewModel);
        }
    }
    public interface IWindowWrapper<T> : ITargetWrapper<T> where T : DependencyObject {
        object DataContext { get; set; }
        object Content { get; set; }
        DataTemplate ContentTemplate { get; set; }
        DataTemplateSelector ContentTemplateSelector { get; set; }

        void Show();
        MessageBoxResult ShowDialog();
        void Activate();
        void Close();

        event CancelEventHandler Closing;
        event EventHandler Closed;
        event EventHandler Activated;
    }

    public class WindowWrapper : IWindowWrapper<Window> {
        public static MessageBoxResult ConvertDialogResult(bool? result) {
            switch(result) {
                case true: return MessageBoxResult.OK;
                case false: return MessageBoxResult.Cancel;
                case null:
                default: return MessageBoxResult.None;
            }
        }

        public Window Target { get; set; }
        public object Content { get { return Target.Content; } set { Target.Content = value; } }
        public DataTemplate ContentTemplate { get { return Target.ContentTemplate; } set { Target.ContentTemplate = value; } }
        public DataTemplateSelector ContentTemplateSelector {
            get { return Target.ContentTemplateSelector; }
            set {
                Target.ContentTemplateSelector = value;
                if(value != null) ContentTemplate = value.SelectTemplate(Content, Target);
            }
        }
        public object DataContext { get { return Target.DataContext; } set { Target.DataContext = value; } }
        public event EventHandler Activated { add { Target.Activated += value; } remove { Target.Activated -= value; } }
        public event CancelEventHandler Closing { add { Target.Closing += value; } remove { Target.Closing -= value; } }
        public event EventHandler Closed { add { Target.Closed += value; } remove { Target.Closed -= value; } }

        public virtual void Show() {
            Target.Show();
        }
        public virtual MessageBoxResult ShowDialog() {
            return ConvertDialogResult(WindowProxy.GetWindowSurrogate(Target).ShowDialog());
        }
        public virtual void Activate() {
            Target.Activate();
        }
        public virtual void Close() {
            Target.Close();
        }
    }
}