using NUnit.Framework;
using System;

namespace DevExpress.Mvvm.Tests {
    [TestFixture]
    public class MessageBoxServiceTests {
        public class TestMessabeBoxService : IMessageBoxService {
            public string MessageBoxText { get; private set; }
            public string Caption { get; private set; }
            public MessageButton Button { get; private set; }
            public MessageIcon Icon { get; private set; }
            public MessageResult DefaultResult { get; private set; }
            public MessageResult Result { get; set; }
            MessageResult IMessageBoxService.Show(string messageBoxText, string caption, MessageButton button, MessageIcon icon, MessageResult defaultResult) {
                MessageBoxText = messageBoxText;
                Caption = caption;
                Button = button;
                Icon = icon;
                DefaultResult = defaultResult;
                return Result;
            }
        }

        [Test]
        public void MessageBoxExtensionsTest() {
            TestMessabeBoxService s;
            s = new TestMessabeBoxService() { Result = MessageResult.OK } ;
            Assert.AreEqual(true, s.ShowMessage("Test"));
            Assert.AreEqual(true, s.ShowMessage("Test", "Caption"));
            s = new TestMessabeBoxService() { Result = MessageResult.Cancel };
            Assert.AreEqual(MessageResult.Cancel, s.ShowMessage("Test", "Caption", MessageButton.OKCancel));
            s = new TestMessabeBoxService() { Result = MessageResult.No };
            Assert.AreEqual(MessageResult.No, s.ShowMessage("Test", "Caption", MessageButton.YesNoCancel, MessageIcon.Warning));
            s = new TestMessabeBoxService() { Result = MessageResult.Yes };
            Assert.AreEqual(MessageResult.Yes, s.ShowMessage("Test", "Caption", MessageButton.YesNoCancel, MessageIcon.Warning, MessageResult.Yes));
        }
        [Test]
        public void NullService1() {
            IMessageBoxService service = null;
            Assert.Throws<ArgumentNullException>(() => { service.Show(""); });
            Assert.Throws<ArgumentNullException>(() => { service.Show("", ""); });
            Assert.Throws<ArgumentNullException>(() => { service.ShowMessage("", "", MessageButton.OKCancel); });
            Assert.Throws<ArgumentNullException>(() => { service.ShowMessage("", "", MessageButton.OKCancel, MessageIcon.Warning); });
            Assert.Throws<ArgumentNullException>(() => { service.ShowMessage(""); });
            Assert.Throws<ArgumentNullException>(() => { service.ShowMessage("", ""); });
            Assert.Throws<ArgumentNullException>(() => { service.ShowMessage("", "", MessageButton.OKCancel); });
            Assert.Throws<ArgumentNullException>(() => { service.ShowMessage("", "", MessageButton.OKCancel, MessageIcon.Warning); });
        }
    }
}