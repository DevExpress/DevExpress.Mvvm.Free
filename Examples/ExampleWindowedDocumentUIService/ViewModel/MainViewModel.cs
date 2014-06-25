using DevExpress.Mvvm;
using DevExpress.Mvvm.DataAnnotations;
using DevExpress.Mvvm.POCO;
using System.Linq;

namespace Example.ViewModel {
    [POCOViewModel]
    public class MainViewModel {

        protected virtual IDocumentManagerService DocumentManagerService { get { return null; } }

        public virtual int TotalNumberOfDocuments { get; set; }
        public virtual string ActiveDocumentTitle { get; set; }
        public virtual int ClosedDocumentIndex { get; set; }
        public virtual int ActivateDocumentIndex { get; set; }


        public void ShowWindowedDocument() {
            DocumentManagerService.ActiveDocumentChanged += OnActiveDocumentChanged;
            IDocument document = DocumentManagerService.CreateDocument("TestView", ViewModelSource.Create(() => new TestViewModel()));
            document.Title = "WindowedDocument " + (GetTotalNumberOfDocuments() - 1);
            document.Show();
            GetServiceInfo();
        }
        public void CloseDocument() {
            if(ClosedDocumentIndex < 0 || ClosedDocumentIndex >= GetTotalNumberOfDocuments()) return;
            Enumerable.ElementAt<IDocument>(DocumentManagerService.Documents, ClosedDocumentIndex).Close();
            GetServiceInfo();
        }

        public void ActivateDocument() {
            if(ActivateDocumentIndex < 0 || ActivateDocumentIndex >= GetTotalNumberOfDocuments()) return;
            DocumentManagerService.ActiveDocument = Enumerable.ElementAt<IDocument>(DocumentManagerService.Documents, ActivateDocumentIndex);
        }

        void OnActiveDocumentChanged(object sender, ActiveDocumentChangedEventArgs e) {
            GetServiceInfo();
        }

        void GetServiceInfo() {
            GetTotalNumberOfDocuments();
            foreach(IDocument item in DocumentManagerService.Documents) {
                if(item == DocumentManagerService.ActiveDocument) {
                    ActiveDocumentTitle = item.Title.ToString();
                    return;
                }
                ActiveDocumentTitle = string.Empty;
            }
        }
        int GetTotalNumberOfDocuments() {
            return TotalNumberOfDocuments = Enumerable.Count<IDocument>(DocumentManagerService.Documents);
        }
    }
}
