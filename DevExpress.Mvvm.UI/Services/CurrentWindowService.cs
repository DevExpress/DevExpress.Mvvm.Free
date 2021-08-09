using DevExpress.Mvvm.UI.Native;
using DevExpress.Mvvm.UI.Interactivity;
using System.Windows;
using System.Windows.Controls;
using DevExpress.Mvvm.Native;
using System.Windows.Input;
using System.ComponentModel;
using DevExpress.Mvvm.UI;
using DevExpress.Mvvm;
using System.Collections.Generic;
using System.Linq;

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

        protected Window GetActualWindow() {
            if (ActualWindow == null)
                UpdateActualWindow();
            return ActualWindow;
        }

        DXWindowState ICurrentWindowService.WindowState {
            get { return DXWindowStateConverter.ToDXWindowState(GetActualWindow()?.WindowState ?? WindowState.Normal); }
            set { 
                var w = GetActualWindow();
                if(w != null) 
                    w.WindowState = DXWindowStateConverter.ToWindowState(value);
            }
        }
        void ICurrentWindowService.Close() {
            GetActualWindow().Do(x => x.Close());
        }
        void ICurrentWindowService.Activate() {
            GetActualWindow().Do(x => x.Activate());
        }
        void ICurrentWindowService.Hide() {
            GetActualWindow().Do(x => x.Hide());
        }
        void ICurrentWindowService.Show() {
            GetActualWindow().Do(x => x.Show());
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
