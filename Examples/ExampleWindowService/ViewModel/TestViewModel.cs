using System;

namespace Example.ViewModel {
    public class TestViewModel {
        Random random = new Random();
        public virtual int RandomNumber { get; set; }
        public void CreateRandomNumber() {
            RandomNumber = random.Next();
        }
    }
}
