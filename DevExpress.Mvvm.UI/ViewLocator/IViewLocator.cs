namespace DevExpress.Mvvm.UI {
    public interface IViewLocator {
        object ResolveView(string name);
    }
}