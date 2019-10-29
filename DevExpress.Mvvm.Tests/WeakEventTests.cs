using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace DevExpress.Mvvm.Tests {
    [TestFixture]
    public class WeakEventTests {
        class EventOwner {
            WeakEvent<EventHandler, EventArgs> myEvent = new WeakEvent<EventHandler, EventArgs>();
            public event EventHandler MyEvent { add { myEvent.Add(value); } remove { myEvent.Remove(value); } }
            public void RaiseMyEvent(EventArgs e) {
                myEvent.Raise(this, e);
            }
            public void RaiseMyEvent(object sender, EventArgs e) {
                myEvent.Raise(sender, e);
            }

            static WeakEvent<EventHandler, EventArgs> myStaticEvent = new WeakEvent<EventHandler, EventArgs>();
            public static event EventHandler MyStaticEvent { add { myStaticEvent.Add(value); } remove { myStaticEvent.Remove(value); } }
            public void RaiseMyStaticEvent(EventArgs e) {
                myStaticEvent.Raise(this, e);
            }
            public void RaiseMyStaticEvent(object sender, EventArgs e) {
                myStaticEvent.Raise(sender, e);
            }

            WeakEvent<Action<object, string>, string> myEvent2 = new WeakEvent<Action<object, string>, string>();
            public event Action<object, string> MyEvent2 { add { myEvent2.Add(value); } remove { myEvent2.Remove(value); } }
            public void RaiseMyEvent2(string args) {
                myEvent2.Raise(this, args);
            }
        }
        class Subscriber {
            public object Sender { get; private set; }
            public EventArgs E { get; private set; }
            public int Count { get; private set; }

            public void Clear() {
                Sender = null;
                E = null;
                Count = 0;
            }

            public void Subscribe(EventOwner eventOwner) {
                eventOwner.MyEvent += OnMyEvent;
            }
            public void SubscribeTwice(EventOwner eventOwner) {
                EventHandler h = null;
                h += OnMyEvent;
                h += OnMyEvent;
                eventOwner.MyEvent += h;
            }
            public void Unsubscribe(EventOwner eventOwner) {
                eventOwner.MyEvent -= OnMyEvent;
            }
            void OnMyEvent(object sender, EventArgs e) {
                Sender = sender;
                E = e;
                Count++;
            }

            public void SubscribeStaticEvent(EventOwner eventOwner) {
                EventOwner.MyStaticEvent += OnMyEvent;
            }
            public void SubscribeStaticEventTwice(EventOwner eventOwner) {
                EventHandler h = null;
                h += OnMyEvent;
                h += OnMyEvent;
                EventOwner.MyStaticEvent += h;
            }
            public void UnsubscribeStaticEvent(EventOwner eventOwner) {
                EventOwner.MyStaticEvent -= OnMyEvent;
            }
            void OnMyStaticEvent(object sender, EventArgs e) {
                Sender = sender;
                E = e;
                Count++;
            }
        }
        class Subscriber2 {
            public bool _PublicOnEvent { get; private set; }
            bool _PrivateOnEvent { get; set; }
            internal bool _InternalOnEvent { get; set; }
            static bool _StaticOnEvent { get; set; }

            public void PublicOnEvent(object sender, EventArgs e) { _PublicOnEvent = true; }
            void PrivateOnEvent(object sender, EventArgs e) { _PrivateOnEvent = true; }
            internal void InternalOnEvent(object sender, EventArgs e) { _InternalOnEvent = true; }
            static void StaticOnEvent(object sender, EventArgs e) { _StaticOnEvent = true; }

            public void SubscribePublic(EventOwner e, bool stat) {
                if(stat) EventOwner.MyStaticEvent += PublicOnEvent;
                else e.MyEvent += PublicOnEvent;
            }
            public void SubscribePrivate(EventOwner e, bool stat) {
                if(stat) EventOwner.MyStaticEvent += PrivateOnEvent;
                else e.MyEvent += PrivateOnEvent;
            }
            public void SubscribeInternal(EventOwner e, bool stat) {
                if(stat) EventOwner.MyStaticEvent += InternalOnEvent;
                else e.MyEvent += InternalOnEvent;
            }
            public void SibscribeStatic(EventOwner e, bool stat) {
                if(stat) EventOwner.MyStaticEvent += StaticOnEvent;
                else e.MyEvent += StaticOnEvent;
            }
            public void SibscribeAnonymousPublic(EventOwner e, bool stat) {
                if(stat)
                    EventOwner.MyStaticEvent += (s, args) => _PublicOnEvent = args != null;
                else e.MyEvent += (s, args) => _PublicOnEvent = args != null;
            }
            public void SibscribeAnonymousPrivate(EventOwner e, bool stat) {
                if(stat)
                    EventOwner.MyStaticEvent += (s, args) => _PrivateOnEvent = args != null;
                else e.MyEvent += (s, args) => _PrivateOnEvent = args != null;
            }
            public void SibscribeAnonymousInternal(EventOwner e, bool stat) {
                if(stat)
                    EventOwner.MyStaticEvent += (s, args) => _InternalOnEvent = args != null;
                else e.MyEvent += (s, args) => _InternalOnEvent = args != null;
            }
            public void SibscribeAnonymousStatic(EventOwner e, bool stat) {
                if(stat)
                    EventOwner.MyStaticEvent += (s, args) => _StaticOnEvent = args != null;
                else e.MyEvent += (s, args) => _StaticOnEvent = args != null;
            }

            public void Clear() {
                _PublicOnEvent = false;
                _PrivateOnEvent = false;
                _InternalOnEvent = false;
                _StaticOnEvent = false;
            }
            public void CheckPublic() { Assert.AreEqual(true, _PublicOnEvent); }
            public void CheckPrivate() { Assert.AreEqual(true, _PrivateOnEvent); }
            public void CheckInternal() { Assert.AreEqual(true, _InternalOnEvent); }
            public void CheckStatic() { Assert.AreEqual(true, _StaticOnEvent); }
        }

        [Test]
        public void SubscribeUnsubscribe() {
            SubscribeUnsubsribeCore(false, (e, args) => e.RaiseMyEvent(args), (e, sender, args) => e.RaiseMyEvent(sender, args),
                (s, e) => s.Subscribe(e), (s, e) => s.SubscribeTwice(e), (s, e) => s.Unsubscribe(e));
            SubscribeUnsubsribeCore(true, (e, args) => e.RaiseMyEvent(args), (e, sender, args) => e.RaiseMyEvent(sender, args),
                (s, e) => s.Subscribe(e), (s, e) => s.SubscribeTwice(e), (s, e) => s.Unsubscribe(e));

            SubscribeUnsubsribeCore(false, (e, args) => e.RaiseMyStaticEvent(args), (e, sender, args) => e.RaiseMyStaticEvent(sender, args),
                (s, e) => s.SubscribeStaticEvent(e), (s, e) => s.SubscribeStaticEventTwice(e), (s, e) => s.UnsubscribeStaticEvent(e));
            SubscribeUnsubsribeCore(true, (e, args) => e.RaiseMyStaticEvent(args), (e, sender, args) => e.RaiseMyStaticEvent(sender, args),
                (s, e) => s.SubscribeStaticEvent(e), (s, e) => s.SubscribeStaticEventTwice(e), (s, e) => s.UnsubscribeStaticEvent(e));
        }
        void SubscribeUnsubsribeCore(bool passSender, Action<EventOwner, EventArgs> raiseMyEvent1, Action<EventOwner, object, EventArgs> raiseMyEvent2,
            Action<Subscriber, EventOwner> subscribe, Action<Subscriber, EventOwner> subscribeTwice, Action<Subscriber, EventOwner> unsubscribe) {
            var e = new EventOwner();
            var sender = passSender ? new object() : e;
            var args = new PropertyChangedEventArgs("Test");
            Action raise = () => {
                if(passSender) raiseMyEvent2(e, sender, args);
                else raiseMyEvent1(e, args);
            };

            WeakReference sRef = SubscribeUnsubsribeCoreAlloc(e, sender, args, raise, raiseMyEvent1, raiseMyEvent2, subscribe, subscribeTwice, unsubscribe);
            MemoryLeaksHelper.CollectOptional(sRef);
            MemoryLeaksHelper.EnsureCollected(sRef);
            raise();
        }
        WeakReference SubscribeUnsubsribeCoreAlloc(EventOwner e, object sender, PropertyChangedEventArgs args, Action raise,
            Action<EventOwner, EventArgs> raiseMyEvent1, Action<EventOwner, object, EventArgs> raiseMyEvent2,
            Action<Subscriber, EventOwner> subscribe, Action<Subscriber, EventOwner> subscribeTwice, Action<Subscriber, EventOwner> unsubscribe) {
            var s = new Subscriber();

            subscribe(s, e);
            raise();
            Assert.AreEqual(sender, s.Sender);
            Assert.AreEqual(args, s.E);
            Assert.AreEqual(1, s.Count);
            raise();
            Assert.AreEqual(sender, s.Sender);
            Assert.AreEqual(args, s.E);
            Assert.AreEqual(2, s.Count);

            s.Clear();
            subscribe(s, e);
            raise();
            Assert.AreEqual(sender, s.Sender);
            Assert.AreEqual(args, s.E);
            Assert.AreEqual(2, s.Count);

            s.Clear();
            unsubscribe(s, e);
            raise();
            Assert.AreEqual(sender, s.Sender);
            Assert.AreEqual(args, s.E);
            Assert.AreEqual(1, s.Count);

            s.Clear();
            unsubscribe(s, e);
            raise();
            Assert.AreEqual(null, s.Sender);
            Assert.AreEqual(null, s.E);
            Assert.AreEqual(0, s.Count);

            s.Clear();
            subscribeTwice(s, e);
            raise();
            Assert.AreEqual(sender, s.Sender);
            Assert.AreEqual(args, s.E);
            Assert.AreEqual(2, s.Count);
            unsubscribe(s, e);
            unsubscribe(s, e);

            unsubscribe(s, e);
            subscribe(s, e);
            return new WeakReference(s);
        }

        [Test]
        public void Memory() {
            MemoryCore((s, e) => s.SubscribePublic(e, false), (s) => s.CheckPublic(), (e) => e.RaiseMyEvent(EventArgs.Empty));
            MemoryCore((s, e) => s.SubscribePrivate(e, false), (s) => s.CheckPrivate(), (e) => e.RaiseMyEvent(EventArgs.Empty));
            MemoryCore((s, e) => s.SubscribeInternal(e, false), (s) => s.CheckInternal(), (e) => e.RaiseMyEvent(EventArgs.Empty));
            MemoryCore((s, e) => s.SibscribeStatic(e, false), (s) => s.CheckStatic(), (e) => e.RaiseMyEvent(EventArgs.Empty));

            MemoryCore((s, e) => s.SibscribeAnonymousPublic(e, false), (s) => s.CheckPublic(), (e) => e.RaiseMyEvent(EventArgs.Empty));
            MemoryCore((s, e) => s.SibscribeAnonymousPrivate(e, false), (s) => s.CheckPrivate(), (e) => e.RaiseMyEvent(EventArgs.Empty));
            MemoryCore((s, e) => s.SibscribeAnonymousInternal(e, false), (s) => s.CheckInternal(), (e) => e.RaiseMyEvent(EventArgs.Empty));
            MemoryCore((s, e) => s.SibscribeAnonymousStatic(e, false), (s) => s.CheckStatic(), (e) => e.RaiseMyEvent(EventArgs.Empty));

            MemoryCore((s, e) => s.SubscribePublic(e, true), (s) => s.CheckPublic(), (e) => e.RaiseMyStaticEvent(EventArgs.Empty));
            MemoryCore((s, e) => s.SubscribePrivate(e, true), (s) => s.CheckPrivate(), (e) => e.RaiseMyStaticEvent(EventArgs.Empty));
            MemoryCore((s, e) => s.SubscribeInternal(e, true), (s) => s.CheckInternal(), (e) => e.RaiseMyStaticEvent(EventArgs.Empty));
            MemoryCore((s, e) => s.SibscribeStatic(e, true), (s) => s.CheckStatic(), (e) => e.RaiseMyStaticEvent(EventArgs.Empty));

            MemoryCore((s, e) => s.SibscribeAnonymousPublic(e, true), (s) => s.CheckPublic(), (e) => e.RaiseMyStaticEvent(EventArgs.Empty));
            MemoryCore((s, e) => s.SibscribeAnonymousPrivate(e, true), (s) => s.CheckPrivate(), (e) => e.RaiseMyStaticEvent(EventArgs.Empty));
            MemoryCore((s, e) => s.SibscribeAnonymousInternal(e, true), (s) => s.CheckInternal(), (e) => e.RaiseMyStaticEvent(EventArgs.Empty));
            MemoryCore((s, e) => s.SibscribeAnonymousStatic(e, true), (s) => s.CheckStatic(), (e) => e.RaiseMyStaticEvent(EventArgs.Empty));
        }
        void MemoryCore(Action<Subscriber2, EventOwner> subscribe, Action<Subscriber2> check, Action<EventOwner> raise) {
            WeakReference sRef = MemoryCoreAlloc(subscribe, check, raise);
            Collect();
            Assert.AreEqual(false, sRef.IsAlive);
        }
        WeakReference MemoryCoreAlloc(Action<Subscriber2, EventOwner> subscribe, Action<Subscriber2> check, Action<EventOwner> raise) {
            EventOwner e = new EventOwner();
            Subscriber2 s = new Subscriber2();
            subscribe(s, e);
            Collect();
            raise(e);
            check(s);
            s.Clear();
            return new WeakReference(s);
        }

        [Test]
        public void Order() {
            var e = new EventOwner();
            List<string> args = new List<string>();
            e.MyEvent2 += (s, x) => args.Add("first");
            e.MyEvent2 += (s, x) => args.Add("second");

            e.RaiseMyEvent2("test");
            Assert.AreEqual("first", args[0]);
            Assert.AreEqual("second", args[1]);
        }

        void Collect() {
            GC.GetTotalMemory(true);
            GC.WaitForPendingFinalizers();
            GC.GetTotalMemory(true);
        }
    }
}