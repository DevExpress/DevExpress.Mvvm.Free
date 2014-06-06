using System;

namespace DevExpress.Mvvm {
    public interface IDispatcherService {
        void BeginInvoke(Action action);
    }
}