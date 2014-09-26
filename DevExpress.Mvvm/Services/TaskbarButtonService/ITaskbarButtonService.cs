using DevExpress.Mvvm.Native;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shell;

namespace DevExpress.Mvvm {
    public interface ITaskbarButtonService {
        double ProgressValue { get; set; }
        TaskbarItemProgressState ProgressState { get; set; }
        ImageSource OverlayIcon { get; set; }
        string Description { get; set; }
        IList<TaskbarThumbButtonInfo> ThumbButtonInfos { get; }
        Thickness ThumbnailClipMargin { get; set; }
        Func<Size, Thickness> ThumbnailClipMarginCallback { get; set; }
        void UpdateThumbnailClipMargin();
    }
    public class TaskbarThumbButtonInfo : Freezable, ITaskbarThumbButtonInfo {
        #region Dependency Properties
        public static readonly DependencyProperty CommandParameterProperty =
            DependencyProperty.Register("CommandParameter", typeof(object), typeof(TaskbarThumbButtonInfo), new PropertyMetadata(null));
        public static readonly DependencyProperty CommandProperty =
            DependencyProperty.Register("Command", typeof(ICommand), typeof(TaskbarThumbButtonInfo), new PropertyMetadata(null));
        public static readonly DependencyProperty DescriptionProperty =
            DependencyProperty.Register("Description", typeof(string), typeof(TaskbarThumbButtonInfo), new PropertyMetadata(""));
        public static readonly DependencyProperty DismissWhenClickedProperty =
            DependencyProperty.Register("DismissWhenClicked", typeof(bool), typeof(TaskbarThumbButtonInfo), new PropertyMetadata(false));
        public static readonly DependencyProperty ImageSourceProperty =
            DependencyProperty.Register("ImageSource", typeof(ImageSource), typeof(TaskbarThumbButtonInfo), new PropertyMetadata(null));
        public static readonly DependencyProperty IsBackgroundVisibleProperty =
            DependencyProperty.Register("IsBackgroundVisible", typeof(bool), typeof(TaskbarThumbButtonInfo), new PropertyMetadata(true));
        public static readonly DependencyProperty IsEnabledProperty =
            DependencyProperty.Register("IsEnabled", typeof(bool), typeof(TaskbarThumbButtonInfo), new PropertyMetadata(true));
        public static readonly DependencyProperty IsInteractiveProperty =
            DependencyProperty.Register("IsInteractive", typeof(bool), typeof(TaskbarThumbButtonInfo), new PropertyMetadata(true));
        public static readonly DependencyProperty VisibilityProperty =
            DependencyProperty.Register("Visibility", typeof(Visibility), typeof(TaskbarThumbButtonInfo), new PropertyMetadata(Visibility.Visible));
        #endregion

        public Action Action { get; set; }
        public object CommandParameter { get { return (object)GetValue(CommandParameterProperty); } set { SetValue(CommandParameterProperty, value); } }
        public ICommand Command { get { return (ICommand)GetValue(CommandProperty); } set { SetValue(CommandProperty, value); } }
        public string Description { get { return (string)GetValue(DescriptionProperty); } set { SetValue(DescriptionProperty, value); } }
        public bool DismissWhenClicked { get { return (bool)GetValue(DismissWhenClickedProperty); } set { SetValue(DismissWhenClickedProperty, value); } }
        public ImageSource ImageSource { get { return (ImageSource)GetValue(ImageSourceProperty); } set { SetValue(ImageSourceProperty, value); } }
        public bool IsBackgroundVisible { get { return (bool)GetValue(IsBackgroundVisibleProperty); } set { SetValue(IsBackgroundVisibleProperty, value); } }
        public bool IsEnabled { get { return (bool)GetValue(IsEnabledProperty); } set { SetValue(IsEnabledProperty, value); } }
        public bool IsInteractive { get { return (bool)GetValue(IsInteractiveProperty); } set { SetValue(IsInteractiveProperty, value); } }
        public Visibility Visibility { get { return (Visibility)GetValue(VisibilityProperty); } set { SetValue(VisibilityProperty, value); } }
        public event EventHandler Click;
        protected override Freezable CreateInstanceCore() { return this; }
        EventHandler ITaskbarThumbButtonInfo.Click {
            get { return Click; }
            set { Click = value; }
        }
    }
}