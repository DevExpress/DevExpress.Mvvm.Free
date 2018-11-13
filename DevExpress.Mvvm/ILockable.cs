
namespace DevExpress.Mvvm {
    public interface ILockable {
        void BeginUpdate();
        void EndUpdate();
        bool IsLockUpdate { get; }
    }
}