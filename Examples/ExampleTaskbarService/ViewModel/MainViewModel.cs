using DevExpress.Mvvm;
using DevExpress.Mvvm.DataAnnotations;
using System;
using System.Windows.Shell;

namespace Example.ViewModel {
    [POCOViewModel]
    public class MainViewModel {
        public virtual string Description { get; set; }
        public virtual double ProgressValue { get; set; }
        [BindableProperty(OnPropertyChangedMethodName = "UpdateProgressState")]
        public virtual bool IsNormalProgressState { get; set; }
        [BindableProperty(OnPropertyChangedMethodName = "UpdateProgressState")]
        public virtual bool IsPausedProgressState { get; set; }
        [BindableProperty(OnPropertyChangedMethodName = "UpdateProgressState")]
        public virtual bool IsErrorProgressState { get; set; }
        [BindableProperty(OnPropertyChangedMethodName = "UpdateProgressState")]
        public virtual bool IsIndetermintateProgressState { get; set; }
        [BindableProperty(OnPropertyChangedMethodName = "UpdateProgressState")]
        public virtual bool IsNoneProgressState { get; set; }

        protected virtual ITaskbarButtonService TaskbarButtonService { get { return null; } }

        public MainViewModel() {
            Description = "Hello";
            ProgressValue = 50;
            IsNormalProgressState = true;
        }
        public void Init() {
            OnDescriptionChanged();
            OnProgressValueChanged();
            UpdateProgressState();
        }
        protected void OnDescriptionChanged() {
            if(TaskbarButtonService == null) return;
            TaskbarButtonService.Description = Description;
        }
        protected void OnProgressValueChanged() {
            if(TaskbarButtonService == null) return;
            TaskbarButtonService.ProgressValue = ProgressValue / 100;
        }
        protected void UpdateProgressState() {
            if(TaskbarButtonService == null) return;
            if(IsNormalProgressState)
                TaskbarButtonService.ProgressState = TaskbarItemProgressState.Normal;
            else if(IsPausedProgressState)
                TaskbarButtonService.ProgressState = TaskbarItemProgressState.Paused;
            else if(IsErrorProgressState)
                TaskbarButtonService.ProgressState = TaskbarItemProgressState.Error;
            else if(IsIndetermintateProgressState)
                TaskbarButtonService.ProgressState = TaskbarItemProgressState.Indeterminate;
            else if(IsNoneProgressState)
                TaskbarButtonService.ProgressState = TaskbarItemProgressState.None;
            else
                throw new NotImplementedException();
        }
    }
}