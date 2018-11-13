using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace DevExpress.Mvvm {
    public class WeakEventAttribute : Attribute { }
    public class WeakEvent<TEventHandler, TEventArgs> {
        delegate void WeakEventHandler(object target, object sender, TEventArgs e);
        readonly List<WeakHandler> weakHandlers = new List<WeakHandler>();

        [DebuggerStepThrough]
        public void Add(TEventHandler handler) {
            List<WeakHandler> whs = new List<WeakHandler>();
            foreach(var d in ((Delegate)(object)handler).GetInvocationList()) {
                whs.Add(new WeakHandler(d));
            }
            lock(weakHandlers) weakHandlers.AddRange(whs);
        }
        public void Remove(TEventHandler handler) {
            Delegate d = (Delegate)(object)handler;
            lock(weakHandlers) {
                var wh = weakHandlers.LastOrDefault(x => Match(x, d));
                if(wh != null) weakHandlers.Remove(wh);
            }
        }
        public void Raise(object sender, TEventArgs e) {
            lock(weakHandlers) {
                weakHandlers.RemoveAll(x => !Invoke(x, sender, e));
            }
        }

        static bool Invoke(WeakHandler wh, object sender, TEventArgs e) {
            if(wh.IsStatic) {
                wh.Handler(null, sender, e);
                return true;
            }
            var target = wh.Target;
            if(target == null) return false;
            wh.Handler(target, sender, e);
            return true;
        }
        static bool Match(WeakHandler wh, Delegate d) {
            return ReferenceEquals(wh.Target, d.Target)
                && wh.Method.Equals(d.Method);
        }

        [ThreadStatic]
        static Dictionary<MethodInfo, WeakEventHandler> weakEventHandlerCache;
        static Dictionary<MethodInfo, WeakEventHandler> WeakEventHandlerCache {
            get { return weakEventHandlerCache ?? (weakEventHandlerCache = new Dictionary<MethodInfo, WeakEventHandler>()); }
        }
        static WeakEventHandler CreateWeakEventHandler(MethodInfo method) {
            var target = Expression.Parameter(typeof(object), "target");
            var sender = Expression.Parameter(typeof(object), "sender");
            var e = Expression.Parameter(typeof(TEventArgs), "e");
            var methodExpr = method.IsStatic
                ? Expression.Call(method, sender, e)
                : Expression.Call(Expression.Convert(target, method.DeclaringType), method, sender, e);
            return Expression.Lambda<WeakEventHandler>(methodExpr, target, sender, e).Compile();
        }
        class WeakHandler {
            readonly WeakReference target;
            public bool IsStatic { get { return target == null; } }
            public object Target { get { return target != null ? target.Target : null; } }
            public MethodInfo Method { get; private set; }
            public WeakEventHandler Handler { get; private set; }

            public WeakHandler(Delegate handler) {
                target = handler.Target != null ? new WeakReference(handler.Target) : null;
                Method = handler.Method;
                WeakEventHandler wh;
                if(!WeakEventHandlerCache.TryGetValue(Method, out wh))
                    WeakEventHandlerCache.Add(Method, wh = CreateWeakEventHandler(Method));
                Handler = wh;
            }
        }
    }
}