using System;
using System.Threading.Tasks;

namespace DevExpress.Mvvm {
    public interface IDispatcherService {
        Task BeginInvoke(Action action);
        void Invoke(Action action);
    }
}