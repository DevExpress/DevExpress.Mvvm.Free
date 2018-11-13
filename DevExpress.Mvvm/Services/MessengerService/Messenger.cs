using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows;
using DevExpress.Mvvm.Internal;
using DevExpress.Mvvm.Native;
using System.Windows.Threading;

namespace DevExpress.Mvvm {
    public class Messenger : IMessenger {
        #region Inner Classes
        public struct ActionInvokerTokenPair {
            public readonly IActionInvoker ActionInvoker;
            public readonly object Token;

            public ActionInvokerTokenPair(IActionInvoker actionInvoker, object token) {
                ActionInvoker = actionInvoker;
                Token = token;
            }
        }
        public class ActionInvokerCollection : FuzzyDictionary<Type, List<ActionInvokerTokenPair>> {
            public ActionInvokerCollection()
                : base(TypeInclude) {
            }
            static bool TypeInclude(Type baseType, Type type) {
                return type.IsSubclassOf(baseType) || baseType.IsAssignableFrom(type);
            }
            public void Register(object token, bool receiveInheritedMessagesToo, Type messageType, IActionInvoker actionInvoker) {
                List<ActionInvokerTokenPair> list;
                if(!TryGetValue(messageType, receiveInheritedMessagesToo, out list)) {
                    list = new List<ActionInvokerTokenPair>();
                    Add(messageType, list, receiveInheritedMessagesToo);
                }
                list.Add(new ActionInvokerTokenPair(actionInvoker, token));
            }
            public void CleanUp() {
                var removeLists = new List<FuzzyKeyValuePair<Type, List<ActionInvokerTokenPair>>>();
                foreach(var list in this) {
                    foreach(ActionInvokerTokenPair removeAction in new List<ActionInvokerTokenPair>(list.Value)) {
                        if(removeAction.ActionInvoker.Target != null) continue;
                        list.Value.Remove(removeAction);
                    }
                    if(list.Value.Count == 0)
                        removeLists.Add(list);
                }
                foreach(var pair in removeLists) {
                    Remove(pair.Key, pair.UseIncludeCondition);
                }
            }
            public void Unregister(object recipient, object token, Delegate action, Type messageType) {
                if(recipient == null) return;
                foreach(List<ActionInvokerTokenPair> list in GetValues(messageType)) {
                    foreach(ActionInvokerTokenPair item in list) {
                        if(token == null || token.Equals(item.Token)) {
                            item.ActionInvoker.ClearIfMatched(action, recipient);
                        }
                    }
                }
            }
            public void Send(object message, Type messageTargetType, object token, Type messageType) {
                foreach(List<ActionInvokerTokenPair> list in GetValues(messageType)) {
                    foreach(ActionInvokerTokenPair item in list.ToArray()) {
                        if(object.Equals(item.Token, token))
                            item.ActionInvoker.ExecuteIfMatched(messageTargetType, message);
                    }
                }
            }
        }
#endregion
        const DispatcherPriority CleanUpPriority = DispatcherPriority.ApplicationIdle;
        static readonly object defaultMessengerLock = new object();
        static IMessenger defaultMessenger;
        public static IMessenger Default {
            get {
                if(defaultMessenger == null) {
                    lock(defaultMessengerLock) {
                        if(defaultMessenger == null)
                            defaultMessenger = new Messenger();
                    }
                }
                return defaultMessenger;
            }
            set { defaultMessenger = value; }
        }

        bool isMultiThreadSafe;
        ActionInvokerCollection actionInvokers = new ActionInvokerCollection();
        IActionInvokerFactory actionInvokerFactory;
        bool cleanupScheduled;

        public Messenger() :
            this(false) { }
        public Messenger(bool isMultiThreadSafe,
            ActionReferenceType actionReferenceType = ActionReferenceType.WeakReference) :
            this(isMultiThreadSafe, CreateActionInvokerFactory(actionReferenceType)) { }

        public Messenger(bool isMultiThreadSafe, IActionInvokerFactory actionInvokerFactory) {
            this.actionInvokerFactory = actionInvokerFactory;
            this.isMultiThreadSafe = isMultiThreadSafe;
        }

        public virtual void Register<TMessage>(object recipient, object token, bool receiveInheritedMessages, Action<TMessage> action) {
            try {
                if(isMultiThreadSafe)
                    Monitor.Enter(actionInvokers);
                IActionInvoker actionInvoker = actionInvokerFactory.CreateActionInvoker<TMessage>(recipient, action);
                RegisterCore(token, receiveInheritedMessages, typeof(TMessage), actionInvoker);
            } finally {
                if(isMultiThreadSafe)
                    Monitor.Exit(actionInvokers);
            }
            RequestCleanup();
        }
        protected void RegisterCore(object token, bool receiveInheritedMessages, Type messageType, IActionInvoker actionInvoker) {
            actionInvokers.Register(token, receiveInheritedMessages, messageType, actionInvoker);
        }
        public virtual void Send<TMessage>(TMessage message, Type messageTargetType, object token) {
            try {
                if(isMultiThreadSafe)
                    Monitor.Enter(actionInvokers);
                actionInvokers.Send(message, messageTargetType, token, typeof(TMessage));
            } finally {
                if(isMultiThreadSafe)
                    Monitor.Exit(actionInvokers);
            }
            RequestCleanup();
        }
        public virtual void Unregister(object recipient) {
            UnregisterCore(recipient, null, null, null);
        }
        public virtual void Unregister<TMessage>(object recipient, object token, Action<TMessage> action) {
            UnregisterCore(recipient, token, action, typeof(TMessage));
        }
        protected void UnregisterCore(object recipient, object token, Delegate action, Type messageType) {
            try {
                if(isMultiThreadSafe)
                    Monitor.Enter(actionInvokers);
                actionInvokers.Unregister(recipient, token, action, messageType);
            } finally {
                if(isMultiThreadSafe)
                    Monitor.Exit(actionInvokers);
            }
            RequestCleanup();
        }
        public void Cleanup() {
            try {
                if(isMultiThreadSafe)
                    Monitor.Enter(actionInvokers);
                actionInvokers.CleanUp();
            } finally {
                if(isMultiThreadSafe)
                    Monitor.Exit(actionInvokers);
            }
            cleanupScheduled = false;
        }
        public void RequestCleanup() {
            if(cleanupScheduled) return;
            cleanupScheduled = true;
            Dispatcher.CurrentDispatcher.BeginInvoke(new Action(Cleanup), CleanUpPriority, null);
        }
        static IActionInvokerFactory CreateActionInvokerFactory(ActionReferenceType actionReferenceType) {
            return actionReferenceType == ActionReferenceType.WeakReference ?
                (IActionInvokerFactory)new WeakReferenceActionInvokerFactory() :
                (IActionInvokerFactory)new StrongReferenceActionInvokerFactory();
        }
    }
    public enum ActionReferenceType {
        WeakReference,
        StrongReference
    }
}