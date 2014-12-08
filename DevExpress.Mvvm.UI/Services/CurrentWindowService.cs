using DevExpress.Mvvm.UI.Native;
using DevExpress.Mvvm.UI.Interactivity;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System;
using DevExpress.Mvvm.Native;
using System.Windows.Input;
using System.ComponentModel;

namespace DevExpress.Mvvm.UI {
    [TargetTypeAttribute(typeof(UserControl))]
    [TargetTypeAttribute(typeof(Window))]
    public class CurrentWindowService : WindowAwareServiceBase, ICurrentWindowService {
        public ICommand ClosingCommand {
            get { return (ICommand)GetValue(ClosingCommandProperty); }
            set { SetValue(ClosingCommandProperty, value); }
        }

        public static readonly DependencyProperty ClosingCommandProperty =
            DependencyProperty.Register("ClosingCommand", typeof(ICommand), typeof(CurrentWindowService), new PropertyMetadata(null));


        void ICurrentWindowService.Close() {
            ActualWindow.Do(x => x.Close());
        }
        void ICurrentWindowService.Activate() {
            ActualWindow.Do(x => x.Activate());
        }
        void ICurrentWindowService.Hide() {
            ActualWindow.Do(x => x.Hide());
        }
        void ICurrentWindowService.SetWindowState(WindowState state) {
            ActualWindow.Do(x => x.WindowState = state);
        }
        void ICurrentWindowService.Show() {
            ActualWindow.Do(x => x.Show());
        }

        protected override void OnActualWindowChanged(Window oldWindow) {
            oldWindow.Do(x => x.Closing -= OnClosing);
            ActualWindow.Do(x => x.Closing += OnClosing);
        }
        void OnClosing(object sender, CancelEventArgs e) {
            ClosingCommand.Do(x => x.Execute(e));
        }
    }
}