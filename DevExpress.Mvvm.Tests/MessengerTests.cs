using System;
using NUnit.Framework;

namespace DevExpress.Mvvm.Tests {
    [TestFixture]
    public class MessengerTests : BaseWpfFixture {
        public interface ITestMessage {
            string Message { get; }
        }
        public class TestMessageBase : ITestMessage {
            public string Message { get; private set; }
            public TestMessageBase(string message) {
                Message = message;
            }
        }
        public class TestMessage : TestMessageBase {
            public object Sender { get; private set; }
            public TestMessage(object sender)
                : base(null) {
                Sender = sender;
            }
            public TestMessage(object sender, string message)
                : base(message) {
                Sender = sender;
            }
        }
        public class GenericMessage<T> : TestMessageBase {
            public T Tag { get; private set; }
            public GenericMessage(T tag)
                : base(null) {
                Tag = tag;
            }
            public GenericMessage(T tag, string message)
                : base(message) {
                Tag = tag;
            }
        }
        public class TestRecipient {
            public string Message { get; private set; }
            public void OnMessage(TestMessageBase message) {
                Message = message.Message;
            }
            public TestRecipient() {
            }
            Action<string> CustomHandler;
            public TestRecipient(Action<string> customHandler) {
                CustomHandler = customHandler;
            }
            public void RunCustomHandler(string x) {
                CustomHandler(x);
            }
        }
        class TestRecipient2 : TestRecipient {
            public TestRecipient2() {
            }
        }
        public class TestRecipient3 {
            public string PublicMessage { get; private set; }
            private string PrivateMessage { get; set; }
            internal string InternalMessage { get; private set; }
            public static string StaticMessage { get; private set; }

            public TestRecipient3() { }
            public void CheckPublic(string expected) {
                Assert.AreEqual(expected, PublicMessage);
            }
            public void CheckPrivate(string expected) {
                Assert.AreEqual(expected, PrivateMessage);
            }
            public void CheckInternal(string expected) {
                Assert.AreEqual(expected, InternalMessage);
            }
            public void CheckStatic(string expected) {
                Assert.AreEqual(expected, StaticMessage);
            }

            public void RegisterPublic() {
                Messenger.Default.Register<string>(this, PublicMethod);
            }
            public void RegisterPrivate() {
                Messenger.Default.Register<string>(this, PrivateMethod);
            }
            public void RegisterInternal() {
                Messenger.Default.Register<string>(this, InternalMethod);
            }
            public void RegisterStatic() {
                Messenger.Default.Register<string>(this, StaticMethod);
            }
            public void RegisterAnonymousPublic() {
                Messenger.Default.Register<string>(this, x => PublicMessage = x);
            }
            public void RegisterAnonymousPrivate() {
                Messenger.Default.Register<string>(this, x => PrivateMessage = x);
            }
            public void RegisterAnonymousInternal() {
                Messenger.Default.Register<string>(this, x => InternalMessage = x);
            }
            public void RegisterAnonymousStatic() {
                Messenger.Default.Register<string>(this, x => StaticMessage = x);
            }

            public void PublicMethod(string message) {
                PublicMessage = message;
            }
            private void PrivateMethod(string message) {
                PrivateMessage = message;
            }
            internal void InternalMethod(string message) {
                InternalMessage = message;
            }
            public static void StaticMethod(string message) {
                StaticMessage = message;
            }
        }
        class TestMessenger : Messenger {
            public int Count { get; private set; }
            public override void Send<TMessage>(TMessage message, Type messageTargetType, object token) {
                Count++;
                base.Send<TMessage>(message, messageTargetType, token);
            }
        }

        protected override void SetUpCore() {
            base.SetUpCore();
            Messenger.Default = null;
        }

#region Send
        [Test]
        public void SendNullMessage() {
            bool ok = false;
            Messenger.Default.Register<string>(this, x => ok = x == null);
            Messenger.Default.Send<string>(null);
            Assert.IsTrue(ok);
        }
        [Test]
        public void Broadcast() {
            string message = null;
            Messenger.Default.Register<TestMessageBase>(this, x => message = x.Message);
            TestRecipient recipient = new TestRecipient();
            Messenger.Default.Register<TestMessageBase>(recipient, recipient.OnMessage);

            Messenger.Default.Send(new TestMessageBase("test"));
            Assert.AreEqual("test", message);
            Assert.AreEqual("test", recipient.Message);
        }
        [Test]
        public void BroadcastToOneType() {
            Messenger.Default = null; ;
            var recipient1 = new TestRecipient();
            var recipient2 = new TestRecipient();
            var recipient3 = new TestRecipient2();
            var recipient4 = this;
            string message1 = null;
            string message2 = null;
            string message3 = null;
            string message4 = null;
            Messenger.Default.Register<string>(recipient1, x => message1 = x);
            Messenger.Default.Register<string>(recipient2, x => message2 = x);
            Messenger.Default.Register<string>(recipient3, x => message3 = x);
            Messenger.Default.Register<string>(recipient3, x => message3 = x);

            Messenger.Default.Send<string, TestRecipient>("test1");
            Assert.AreEqual("test1", message1, "message1");
            Assert.AreEqual("test1", message2, "message2");
            Assert.AreEqual("test1", message3, "message3");
            Assert.IsNull(message4);

            Messenger.Default.Send<string, TestRecipient2>("test2");
            Assert.AreEqual("test1", message1, "message1");
            Assert.AreEqual("test1", message2, "message2");
            Assert.AreEqual("test2", message3, "message3");
            Assert.IsNull(message4);
        }
        [Test]
        public void SendToken() {
            object message1 = null;
            object message2 = null;
            object message3 = null;
            Messenger.Default.Register<Exception>(this, true, x => message1 = x);
            Messenger.Default.Register<Exception>(this, 1, true, x => message2 = x);
            Messenger.Default.Register<Exception>(this, 2, true, x => message3 = x);
            var v = new InvalidOperationException();
            Messenger.Default.Send(v, 1);
            Assert.IsNull(message1);
            Assert.AreSame(v, message2);
            Assert.IsNull(message3);
        }
#endregion
#region Message Inheritance
        [Test]
        public void MessageInheritance() {
            string message = null;
            object sender = null;
            Action<TestMessageBase> action = x => {
                if(x is TestMessage)
                    sender = ((TestMessage)x).Sender;
                message = x.Message;
            };
            Messenger.Default.Register<TestMessageBase>(this, action);
            Messenger.Default.Send(new TestMessageBase("test"));
            Assert.AreEqual("test", message);
            Assert.IsNull(sender);

            message = null;
            Messenger.Default.Send(new TestMessage(1, "test"));
            Assert.IsNull(message);
            Assert.IsNull(sender);

            Messenger.Default.Register<TestMessageBase>(this, true, action);
            Messenger.Default.Send(new TestMessage(1, "test"));
            Assert.AreEqual("test", message);
            Assert.AreEqual(1, sender);
        }
        [Test]
        public void GenericMessageInheritance() {
            int message1 = 0;
            string message2 = null;
            DateTime message3 = DateTime.MinValue;
            Exception message4 = null;

            Action<TestMessageBase> action = x => {
                if(x is GenericMessage<int>) {
                    message1 = ((GenericMessage<int>)x).Tag;
                    return;
                }
                if(x is GenericMessage<string>) {
                    message2 = ((GenericMessage<string>)x).Tag;
                    return;
                }
                if(x is GenericMessage<DateTime>) {
                    message3 = ((GenericMessage<DateTime>)x).Tag;
                    return;
                }
                if(x is GenericMessage<Exception>) {
                    message4 = ((GenericMessage<Exception>)x).Tag;
                    return;
                }
                throw new InvalidOperationException();
            };
            Messenger.Default.Register<TestMessageBase>(this, action);
            Messenger.Default.Send(new GenericMessage<int>(1));
            Messenger.Default.Send(new GenericMessage<string>("test"));
            Messenger.Default.Send(new GenericMessage<DateTime>(DateTime.MaxValue));
            Messenger.Default.Send(new GenericMessage<Exception>(new InvalidOperationException()));
            Assert.AreEqual(0, message1);
            Assert.IsNull(message2);
            Assert.AreEqual(DateTime.MinValue, message3);
            Assert.IsNull(message4);


            Messenger.Default.Register<TestMessageBase>(this, true, action);
            Messenger.Default.Send(new GenericMessage<int>(1));
            Assert.AreEqual(1, message1);
            Assert.IsNull(message2);
            Assert.AreEqual(DateTime.MinValue, message3);
            Assert.IsNull(message4);

            Messenger.Default.Send(new GenericMessage<string>("test"));
            Assert.AreEqual(1, message1);
            Assert.AreEqual("test", message2);
            Assert.AreEqual(DateTime.MinValue, message3);
            Assert.IsNull(message4);

            Messenger.Default.Send(new GenericMessage<DateTime>(DateTime.MaxValue));
            Assert.AreEqual(1, message1);
            Assert.AreEqual("test", message2);
            Assert.AreEqual(DateTime.MaxValue, message3);
            Assert.IsNull(message4);

            Exception v = new InvalidOperationException();
            Messenger.Default.Send(new GenericMessage<Exception>(v));
            Assert.AreEqual(1, message1);
            Assert.AreEqual("test", message2);
            Assert.AreEqual(DateTime.MaxValue, message3);
            Assert.AreSame(v, message4);
        }
#endregion
#region Register Unregister
        [Test]
        public void RegisterUnregister() {
            string message = null;
            Messenger.Default.Register<TestMessageBase>(this, x => message = x.Message);
            Messenger.Default.Send(new TestMessageBase("test"));
            Assert.AreEqual("test", message);
            message = null;
            Messenger.Default.Unregister<TestMessageBase>(this);
            Messenger.Default.Send(new TestMessageBase("test"));
            Assert.IsNull(message);
        }
        [Test]
        public void RegisterUnregisterToken() {
            string message1 = null;
            string message2 = null;

            Messenger.Default.Register<string>(this, 1, x => message1 = x);
            Messenger.Default.Register<string>(this, 2, x => message2 = x);
            Messenger.Default.Send("test1", 1);
            Assert.AreEqual("test1", message1);
            Assert.AreEqual(null, message2);
            Messenger.Default.Send("test2", 2);
            Assert.AreEqual("test1", message1);
            Assert.AreEqual("test2", message2);

            message1 = null;
            message2 = null;
            Messenger.Default.Unregister<string>(this, 1);
            Messenger.Default.Send("test1", 1);
            Messenger.Default.Send("test2", 2);
            Assert.IsNull(message1);
            Assert.AreEqual("test2", message2);

            message1 = null;
            message2 = null;
            Messenger.Default.Unregister<string>(this, 2);
            Messenger.Default.Send("test1", 1);
            Messenger.Default.Send("test2", 2);
            Assert.IsNull(message1);
            Assert.IsNull(message2);
        }
        [Test]
        public void RegisterUnregisterOneAction() {
            string message1 = null;
            string message2 = null;
            Action<string> action1 = x => message1 = x;
            Action<string> action2 = x => message2 = x;

            Messenger.Default.Register(this, action1);
            Messenger.Default.Register(this, action2);
            Messenger.Default.Send("test");
            Assert.AreEqual("test", message1);
            Assert.AreEqual("test", message2);

            message1 = null;
            message2 = null;
            Messenger.Default.Unregister(this, action2);
            Messenger.Default.Send("test");
            Assert.AreEqual("test", message1);
            Assert.IsNull(message2);
        }
        [Test]
        public void RegisterUnregisterOneActionToken() {
            string message1 = null;
            string message2 = null;
            string message3 = null;
            Action<string> action1 = x => message1 = x;
            Action<string> action2 = x => message2 = x;
            Action<string> action3 = x => message3 = x;

            Messenger.Default.Register(this, 1, action1);
            Messenger.Default.Register(this, 2, action2);
            Messenger.Default.Register(this, 2, action3);

            Messenger.Default.Send("test", 1);
            Assert.AreEqual("test", message1);
            Assert.IsNull(message2);
            Assert.IsNull(message3);

            message1 = null;
            Messenger.Default.Send("test", 2);
            Assert.IsNull(message1);
            Assert.AreEqual("test", message2);
            Assert.AreEqual("test", message3);

            message2 = null;
            message3 = null;
            Messenger.Default.Unregister(this, 2, action3);
            Messenger.Default.Send("test", 2);
            Assert.IsNull(message1);
            Assert.AreEqual("test", message2);
            Assert.IsNull(message3);

            Messenger.Default.Send("test", 1);
            Assert.AreEqual("test", message1);
            Assert.AreEqual("test", message2);
            Assert.IsNull(message3);
        }
        [Test]
        public void RegisterUnregisterGenericMessage() {
            int message1 = 0;
            string message2 = null;
            DateTime message3 = DateTime.MinValue;
            Exception message4 = null;

            Messenger.Default.Register<GenericMessage<int>>(this, x => message1 = x.Tag);
            Messenger.Default.Register<GenericMessage<string>>(this, x => message2 = x.Tag);
            Messenger.Default.Register<GenericMessage<DateTime>>(this, x => message3 = x.Tag);
            Messenger.Default.Register<GenericMessage<Exception>>(this, x => message4 = x.Tag);

            Messenger.Default.Send(new GenericMessage<double>(1));
            Assert.AreEqual(0, message1);
            Assert.IsNull(message2);
            Assert.AreEqual(DateTime.MinValue, message3);
            Assert.IsNull(message4);

            Messenger.Default.Send(new GenericMessage<int>(1));
            Assert.AreEqual(1, message1);
            Messenger.Default.Send(new GenericMessage<string>("test"));
            Assert.AreEqual("test", message2);
            Messenger.Default.Send(new GenericMessage<DateTime>(DateTime.MaxValue));
            Assert.AreEqual(DateTime.MaxValue, message3);
            Exception v = new InvalidOperationException();
            Messenger.Default.Send(new GenericMessage<Exception>(v));
            Assert.AreSame(v, message4);

            message1 = 0;
            message2 = null;
            message3 = DateTime.MinValue;
            message4 = null;
            Messenger.Default.Unregister<GenericMessage<int>>(this);
            Messenger.Default.Unregister<GenericMessage<string>>(this);
            Messenger.Default.Unregister<GenericMessage<DateTime>>(this);
            Messenger.Default.Unregister<GenericMessage<Exception>>(this);
            Messenger.Default.Send(new GenericMessage<int>(1));
            Messenger.Default.Send(new GenericMessage<string>("test"));
            Messenger.Default.Send(new GenericMessage<DateTime>(DateTime.MaxValue));
            Messenger.Default.Send(new GenericMessage<Exception>(new InvalidOperationException()));
            Assert.AreEqual(0, message1);
            Assert.IsNull(message2);
            Assert.AreEqual(DateTime.MinValue, message3);
            Assert.IsNull(message4);
        }
        [Test]
        public void RegisterUnregisterOneRecipient() {
            string message1 = null;
            string message2 = null;
            string message3 = null;
            Messenger.Default.Register<TestMessageBase>(this, x => message1 = x.Message);
            Messenger.Default.Register<TestMessageBase>(this, x => message2 = x.Message);
            Messenger.Default.Register<TestMessage>(this, x => message3 = (string)x.Sender);

            string externalMessage1 = null;
            string externalMessage2 = null;
            var externalRecipient = new TestRecipient();
            Messenger.Default.Register<TestMessageBase>(externalRecipient, x => externalMessage1 = x.Message);
            Messenger.Default.Register<TestMessage>(externalRecipient, x => externalMessage2 = (string)x.Sender);

            Messenger.Default.Send(new TestMessageBase("test1"));
            Messenger.Default.Send(new TestMessage("test2", "test2"));

            Assert.AreEqual("test1", message1);
            Assert.AreEqual("test1", message2);
            Assert.AreEqual("test2", message3);
            Assert.AreEqual("test1", externalMessage1);
            Assert.AreEqual("test2", externalMessage2);

            Messenger.Default.Unregister(this);

            Messenger.Default.Send(new TestMessageBase("newTest1"));
            Messenger.Default.Send(new TestMessage("newTest2", "newTest2"));

            Assert.AreEqual("test1", message1);
            Assert.AreEqual("test1", message2);
            Assert.AreEqual("test2", message3);
            Assert.AreEqual("newTest1", externalMessage1);
            Assert.AreEqual("newTest2", externalMessage2);
        }
        [Test]
        public void RegisterUnregisterInterfaceMessage() {
            string message = null;
            Messenger.Default.Register<ITestMessage>(this, true, x => message = x.Message);
            Messenger.Default.Send(new TestMessageBase("test"));
            Assert.AreEqual("test", message);
            Messenger.Default.Send(new GenericMessage<int>(1, "test2"));
            Assert.AreEqual("test2", message);

            message = null;
            Messenger.Default.Unregister<ITestMessage>(this);
            Messenger.Default.Send(new TestMessageBase("test"));
            Assert.IsNull(message);
        }
        [Test]
        public void RegisterTokenSendWithoutToken() {
            string message = null;
            Messenger.Default.Register<string>(this, 1, x => message = x);
            Messenger.Default.Send("test");
            Assert.IsNull(message);
        }

        [Test]
        public void RegisterWhileReceiving1() {
            string message = null;
            Messenger.Default.Register<string>(this, x => Messenger.Default.Register<string>(this, xx => message = xx));
            Messenger.Default.Send("test");
            Assert.IsNull(message);
            Messenger.Default.Send("test");
            Assert.AreEqual("test", message);
        }
        [Test]
        public void RegisterWhileReceiving2() {
            string message = null;
            Messenger.Default.Register<string>(this, x => {
                message = x;
                Messenger.Default.Register<TestMessageBase>(this, xx => message = xx.Message);
                Messenger.Default.Send(new TestMessageBase("test2"));
            });
            Messenger.Default.Send("test");
            Assert.AreEqual("test2", message);
        }
        [Test]
        public void UnregisterWhileReceiving1() {
            string message = null;
            Messenger.Default.Register<string>(this, x => {
                Messenger.Default.Unregister<string>(this);
                message = x;
            });
            Messenger.Default.Send("test");
            Assert.AreEqual("test", message);
            message = null;
            Messenger.Default.Send("test");
            Assert.IsNull(message);
        }
        [Test]
        public void UnregisterWhileReceiving2() {
            string message = null;
            TestRecipient recipient = null;
            Action<string> handler = x => {
                recipient.RunCustomHandler(x);
            };
            recipient = new TestRecipient(x => {
                Messenger.Default.Unregister<string>(recipient, handler);
                Messenger.Default.Send("new");
                message = x;
            });
            Messenger.Default.Register<string>(recipient, handler);
            Messenger.Default.Send("test");
            Assert.AreEqual("test", message);
        }
#endregion
#region Custom Messenger
        [Test]
        public void CustomMessenger() {
            string message1 = null;
            string message2 = null;
            Messenger.Default.Register<string>(this, x => message1 = x);
            Messenger.Default.Send("test1");
            Assert.AreEqual("test1", message1);
            Messenger.Default = new TestMessenger();
            Messenger.Default.Register<string>(this, x => message2 = x);
            Assert.AreEqual(0, ((TestMessenger)Messenger.Default).Count);
            Messenger.Default.Send("test2");
            Assert.AreEqual(1, ((TestMessenger)Messenger.Default).Count);
            Assert.AreEqual("test1", message1);
            Assert.AreEqual("test2", message2);
        }
        [Test]
        public void DeletingMessenger() {
            string message = null;
            Messenger.Default.Register<string>(this, x => message = x);
            Messenger.Default = null; ;
            Messenger.Default.Send("test");
            Assert.IsNull(message);
        }
        [Test]
        public void MultipleMessengers() {
            Messenger messenger1 = new Messenger();
            Messenger messenger2 = new Messenger();
            TestRecipient recipient1 = new TestRecipient();
            TestRecipient recipient2 = new TestRecipient();
            messenger1.Register<TestMessageBase>(recipient1, recipient1.OnMessage);
            messenger2.Register<TestMessageBase>(recipient2, recipient2.OnMessage);
            messenger1.Send(new TestMessageBase("test1"));
            Assert.AreEqual("test1", recipient1.Message);
            Assert.IsNull(null, recipient2.Message);
            messenger2.Send(new TestMessageBase("test2"));
            Assert.AreEqual("test1", recipient1.Message);
            Assert.AreEqual("test2", recipient2.Message);
        }
        #endregion
        #region Memory
        TestRecipient DeletingRecipientRecipient1;
        TestRecipient DeletingRecipientRecipient2;
        string DeletingRecipientMessage1;
        string DeletingRecipientMessage2;
        [Test]
        public void DeletingRecipient() {
            Action alloc = () => {
                DeletingRecipientRecipient1 = new TestRecipient();
                DeletingRecipientRecipient2 = new TestRecipient();
                Messenger.Default.Register<string>(DeletingRecipientRecipient1, x => DeletingRecipientMessage1 = x);
                Messenger.Default.Register<string>(DeletingRecipientRecipient2, x => DeletingRecipientMessage2 = x);
            };
            alloc();
            Messenger.Default.Send("test");
            Assert.AreEqual("test", DeletingRecipientMessage1);
            Assert.AreEqual("test", DeletingRecipientMessage2);
            DeletingRecipientRecipient1 = null;
            GC.Collect();
            Messenger.Default.Send("test2");
            Assert.AreEqual("test", DeletingRecipientMessage1);
            Assert.AreEqual("test2", DeletingRecipientMessage2);
            DeletingRecipientRecipient2 = null;
            GC.Collect();
            Messenger.Default.Send("test3");
            Assert.AreEqual("test", DeletingRecipientMessage1);
            Assert.AreEqual("test2", DeletingRecipientMessage2);
        }
        [Test]
        public void MemoryTest0() {
            Messenger.Default.Register<TestMessageBase>(this, x => { });
            WeakReference messenger1 = new WeakReference(Messenger.Default);
            GC.Collect();
            Messenger.Default.Register<TestMessageBase>(this, x => { });
            WeakReference messenger2 = new WeakReference(Messenger.Default);
            GC.Collect();
            Assert.AreSame(messenger1.Target, messenger2.Target);
        }
        [Test]
        public void MemoryTest1() {
            var recipient = new TestRecipient();
            Messenger.Default.Register<string>(recipient, x => recipient.OnMessage(new TestMessageBase(x)));
            GC.Collect();
            Messenger.Default.Send("test");
            Assert.AreEqual("test", recipient.Message);
        }

        [Test]
        public void CollectRecipient1() {
            CollectRecipientCore(x => x.RegisterPublic());
            CollectRecipientCore(x => x.RegisterPrivate());
            CollectRecipientCore(x => x.RegisterInternal());
            CollectRecipientCore(x => x.RegisterStatic());
            CollectRecipientCore(x => x.RegisterAnonymousPublic());
            CollectRecipientCore(x => x.RegisterAnonymousPrivate());
            CollectRecipientCore(x => x.RegisterAnonymousInternal());
            CollectRecipientCore(x => x.RegisterAnonymousStatic());
        }
        [Test]
        public void CollectRecipient2() {
            CollectRecipientCore(x => x.RegisterPublic(), x => x.CheckPublic("test"));
            CollectRecipientCore(x => x.RegisterPrivate(), x => x.CheckPrivate("test"));
            CollectRecipientCore(x => x.RegisterInternal(), x => x.CheckInternal("test"));
            CollectRecipientCore(x => x.RegisterStatic(), x => x.CheckStatic("test"));
            CollectRecipientCore(x => x.RegisterAnonymousPublic(), x => x.CheckPublic("test"));
            CollectRecipientCore(x => x.RegisterAnonymousPrivate(), x => x.CheckPrivate("test"));
            CollectRecipientCore(x => x.RegisterAnonymousInternal(), x => x.CheckInternal("test"));
            CollectRecipientCore(x => x.RegisterAnonymousStatic(), x => x.CheckStatic("test"));
        }
        void CollectRecipientCore(Action<TestRecipient3> registerMethod) {
            CollectRecipientCore(registerMethod, false, null);
        }
        void CollectRecipientCore(Action<TestRecipient3> registerMethod, bool unregister) {
            CollectRecipientCore(registerMethod, unregister, null);
        }
        void CollectRecipientCore(Action<TestRecipient3> registerMethod, Action<TestRecipient3> checkMessageMethod) {
            CollectRecipientCore(registerMethod, false, checkMessageMethod);
        }
        void CollectRecipientCore(Action<TestRecipient3> registerMethod, bool unregister, Action<TestRecipient3> checkMessageMethod) {
            WeakReference recipientReference = CollectRecipientCoreAlloc(registerMethod, unregister, checkMessageMethod);
            GC.Collect();
            Assert.IsFalse(recipientReference.IsAlive);
        }
        WeakReference CollectRecipientCoreAlloc(Action<TestRecipient3> registerMethod, bool unregister, Action<TestRecipient3> checkMessageMethod) {
            TestRecipient3 recipient = new TestRecipient3();
            WeakReference recipientReference = new WeakReference(recipient);
            registerMethod(recipient);
            if(checkMessageMethod != null) {
                Messenger.Default.Send<string>("test");
                checkMessageMethod(recipient);
            }
            if(unregister)
                Messenger.Default.Unregister(recipient);
            return recipientReference;
        }
        #endregion
        #region Extension Methods
        [Test]
        public void NullService() {
            IMessenger service = null;
            Assert.Throws<ArgumentNullException>(() => { service.Register<string>(new object(), x => { }); });
            Assert.Throws<ArgumentNullException>(() => { service.Register<string>(new object(), true, x => { }); });
            Assert.Throws<ArgumentNullException>(() => { service.Register<string>(new object(), new object(), x => { }); });
            Assert.Throws<ArgumentNullException>(() => { service.Unregister<string>(new object(), x => { }); });
            Assert.Throws<ArgumentNullException>(() => { service.Unregister<string>(new object()); });
            Assert.Throws<ArgumentNullException>(() => { service.Unregister<string>(new object(), new object()); });
            Assert.Throws<ArgumentNullException>(() => { service.Send(new object()); });
            Assert.Throws<ArgumentNullException>(() => { service.Send(new object(), new object()); });
            Assert.Throws<ArgumentNullException>(() => { service.Send<string, string>("test"); });
        }
#endregion
    }
}