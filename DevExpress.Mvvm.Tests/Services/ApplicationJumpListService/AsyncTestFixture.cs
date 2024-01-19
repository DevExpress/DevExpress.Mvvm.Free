using System;
using System.Diagnostics;
using System.Threading;

namespace DevExpress.Mvvm.UI.Tests {
    public class AsyncTestFixture {
        public virtual int SecondsTimeout { get { return Debugger.IsAttached ? 180 : 25; } }
        public int MillisecondsTimeout { get { return SecondsTimeout * 1000; } }
        public TimeSpan Timeout { get { return new TimeSpan(0, 0, SecondsTimeout); } }
        public virtual int SecondsTestTimeout { get { return Debugger.IsAttached ? 240 : 120; } }
        public int MillisecondsTestTimeout { get { return SecondsTestTimeout * 1000; } }
        public TimeSpan TestTimeout { get { return new TimeSpan(0, 0, SecondsTestTimeout); } }
        public void WaitOne(WaitHandle waitHandle, int millisecondsTimeout = 0) {
            if(millisecondsTimeout == 0)
                millisecondsTimeout = MillisecondsTimeout;
            if(!waitHandle.WaitOne(millisecondsTimeout))
                throw new TimeoutException();
        }
        public void Join(Thread thread, int millisecondsTimeout = 0) {
            if(millisecondsTimeout == 0)
                millisecondsTimeout = MillisecondsTimeout;
            if(!thread.Join(millisecondsTimeout))
                throw new TimeoutException();
        }
    }
    public abstract class AsyncTestObjectBase {
        public AsyncTestObjectBase(AsyncTestFixture fixture) {
            Fixture = fixture;
        }
        protected AsyncTestFixture Fixture { get; private set; }
    }
}