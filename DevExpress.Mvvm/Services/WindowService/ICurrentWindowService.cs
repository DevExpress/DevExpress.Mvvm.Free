using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace DevExpress.Mvvm {
    public interface ICurrentWindowService {
        void Close();
        void SetWindowState(WindowState state);
        void Activate();
        void Hide();
        void Show();
    }
}