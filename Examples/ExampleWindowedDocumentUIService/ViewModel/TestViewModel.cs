using System;

namespace Example.ViewModel {
    public class TestViewModel {
        public TestViewModel() {
            Number = 9;
        }
        public virtual int Number { get; set; }
        public void Multiply() {
            Number *= 3;
        }
    }
}
