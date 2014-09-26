using DevExpress.Mvvm;
using DevExpress.Mvvm.DataAnnotations;
using DevExpress.Mvvm.POCO;
using System.Collections.ObjectModel;

namespace Example.ViewModel {
    [POCOViewModel]
    public class MainViewModel {
        public virtual ObservableCollection<Person> Persons { get; protected set; }
        protected virtual IMessageBoxService MessageBoxService { get { return null; } }
        public void Initialize() {
            Persons = new ObservableCollection<Person>();
            Persons.Add(ViewModelSource.Create(() => new Person() { FirstName = "John", LastName = "Smith" }));
            Persons.Add(ViewModelSource.Create(() => new Person() { FirstName = "Alex", LastName = "Carter" }));
        }
        public void Edit(Person person) {
            MessageBoxService.Show(string.Format("{0} {1}", person.FirstName, person.LastName));
        }
        public bool CanEdit(Person person) {
            return person != null;
        }
    }
    public class Person {
        public string FirstName { get; set; }
        public string LastName { get; set; }
    }
}
