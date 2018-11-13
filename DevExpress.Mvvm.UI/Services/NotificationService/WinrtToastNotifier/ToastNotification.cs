using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DevExpress.Data.Controls.WinrtToastNotifier.WinApi;
using DevExpress.Internal.WinApi;
using DevExpress.Internal.WinApi.Windows.UI.Notifications;

namespace DevExpress.Internal {
    public class WinRTToastNotificationFactory : IPredefinedToastNotificationFactory {
        readonly string appId;
        public WinRTToastNotificationFactory(string appId) {
            this.appId = appId;
        }
        public IPredefinedToastNotification CreateToastNotification(IPredefinedToastNotificationContent content) {
            return new WinRTToastNotification(content, () => ToastNotificationManager.CreateToastNotificationAdapter(appId));
        }
        public IPredefinedToastNotification CreateToastNotification(string bodyText) {
            return CreateToastNotification(DefaultFactory.CreateContent(bodyText));
        }
        public IPredefinedToastNotification CreateToastNotificationOneLineHeaderContent(string headlineText, string bodyText) {
            return CreateToastNotification(DefaultFactory.CreateOneLineHeaderContent(headlineText, bodyText));
        }
        public IPredefinedToastNotification CreateToastNotificationOneLineHeaderContent(string headlineText, string bodyText1, string bodyText2) {
            return CreateToastNotification(DefaultFactory.CreateOneLineHeaderContent(headlineText, bodyText1, bodyText2));
        }
        public IPredefinedToastNotification CreateToastNotificationTwoLineHeader(string headlineText, string bodyText) {
            return CreateToastNotification(DefaultFactory.CreateTwoLineHeaderContent(headlineText, bodyText));
        }
        #region IPredefinedToastNotificationContentFactory
        IPredefinedToastNotificationContentFactory factoryCore;
        IPredefinedToastNotificationContentFactory DefaultFactory {
            get {
                if(factoryCore == null)
                    factoryCore = CreateContentFactory();
                return factoryCore;
            }
        }
        public double ImageSize {
            get { return WindowsVersion.IsWin10AnniversaryOrNewer ? 48 : 90; }
        }
        public virtual IPredefinedToastNotificationContentFactory CreateContentFactory() {
            return new WinRTToastNotificationContentFactory();
        }
        class WinRTToastNotificationContentFactory : IPredefinedToastNotificationContentFactory, IPredefinedToastNotificationContentFactoryGeneric {
            public IPredefinedToastNotificationContent CreateContent(string bodyText) {
                return WinRTToastNotificationContent.Create(bodyText);
            }
            public IPredefinedToastNotificationContent CreateOneLineHeaderContent(string headlineText, string bodyText) {
                return WinRTToastNotificationContent.CreateOneLineHeader(headlineText, bodyText);
            }
            public IPredefinedToastNotificationContent CreateTwoLineHeaderContent(string headlineText, string bodyText) {
                return WinRTToastNotificationContent.CreateTwoLineHeader(headlineText, bodyText);
            }
            public IPredefinedToastNotificationContent CreateOneLineHeaderContent(string headlineText, string bodyText1, string bodyText2) {
                return WinRTToastNotificationContent.CreateOneLineHeader(headlineText, bodyText1, bodyText2);
            }
            public IPredefinedToastNotificationContent CreateToastGeneric(string headlineText, string bodyText1, string bodyText2) {
                return WinRTToastNotificationContent.CreateToastGeneric(headlineText, bodyText1, bodyText2);
            }
        }
        #endregion IPredefinedToastNotificationContentFactory
    }
    public class WinRTToastNotification : IPredefinedToastNotification {
        ToastNotificationHandlerInfo<HandlerDismissed> handlerDismissed;
        ToastNotificationHandlerInfo<HandlerActivated> handlerActivated;
        ToastNotificationHandlerInfo<HandlerFailed> handlerFailed;
        IPredefinedToastNotificationContent contentCore;
        Lazy<IToastNotificationAdapter> adapterCore;
        internal WinRTToastNotification(IPredefinedToastNotificationContent content, Func<IToastNotificationAdapter> adapterRoutine) {
            if(content == null)
                throw new ArgumentNullException("content");
            this.adapterCore = new Lazy<IToastNotificationAdapter>(adapterRoutine);
            this.contentCore = content;
            this.handlerDismissed = new ToastNotificationHandlerInfo<HandlerDismissed>(() => new HandlerDismissed(this));
            this.handlerActivated = new ToastNotificationHandlerInfo<HandlerActivated>(() => new HandlerActivated(this));
            this.handlerFailed = new ToastNotificationHandlerInfo<HandlerFailed>(() => new HandlerFailed(this));
        }
        public IPredefinedToastNotificationContent Content {
            get { return contentCore; }
        }
        internal IToastNotificationAdapter Adapter {
            get { return adapterCore.Value; }
        }
        #region Events
        internal delegate void TypedEventHandler<TSender, TResult>(TSender sender, TResult args);
        internal event TypedEventHandler<IPredefinedToastNotification, ToastDismissalReason> Dismissed {
            add { handlerDismissed.Handler.Subscribe(value); }
            remove { handlerDismissed.Handler.Unsubscribe(value); }
        }
        internal event TypedEventHandler<IPredefinedToastNotification, string> Activated {
            add { handlerActivated.Handler.Subscribe(value); }
            remove { handlerActivated.Handler.Unsubscribe(value); }
        }
        internal event TypedEventHandler<IPredefinedToastNotification, ToastNotificationFailedException> Failed {
            add { handlerFailed.Handler.Subscribe(value); }
            remove { handlerFailed.Handler.Unsubscribe(value); }
        }
        #endregion Events
        #region Handlers
        class ToastNotificationHandlerInfo<THandler> {
            public ToastNotificationHandlerInfo(Func<THandler> initializer) {
                Handler = initializer();
            }
            public EventRegistrationToken Token;
            public THandler Handler { get; private set; }
        }
        class ToastNotificationHandler<THandler> {
            List<THandler> handlers;
            WinRTToastNotification toast;
            public ToastNotificationHandler(WinRTToastNotification toast) {
                this.handlers = new List<THandler>();
                this.toast = toast;
            }
            public void Subscribe(THandler handler) {
                handlers.Add(handler);
            }
            public void Unsubscribe(THandler handler) {
                handlers.Remove(handler);
            }
            protected int InvokeCore<TArgs>(IToastNotification sender, TArgs args, Action<THandler, WinRTToastNotification, TArgs> action) {
                lock(this) {
                    using(toast.Content) {
                        handlers.ForEach((h) => action(h, toast, args));
                        handlers.Clear();
                    }
                }
                return 0;
            }
        }
        sealed class HandlerDismissed : ToastNotificationHandler<TypedEventHandler<IPredefinedToastNotification, ToastDismissalReason>>, ITypedEventHandler_IToastNotification_Dismissed {
            public HandlerDismissed(WinRTToastNotification sender) : base(sender) { }
            public int Invoke(IToastNotification sender, IToastDismissedEventArgs args) {
                return InvokeCore(sender, args, (h, s, a) => h(s, a.Reason));
            }
        }
        sealed class HandlerActivated : ToastNotificationHandler<TypedEventHandler<IPredefinedToastNotification, string>>, ITypedEventHandler_IToastNotification_Activated {
            public HandlerActivated(WinRTToastNotification sender) : base(sender) { }
            public int Invoke(IToastNotification sender, IInspectable args) {
                return InvokeCore(sender, GetActivationString(args as IToastActivatedEventArgs), (h, s, a) => h(s, a));
            }
            string GetActivationString(IToastActivatedEventArgs args) {
                string activationString = null;
                if(args != null)
                    args.GetArguments(out activationString);
                return activationString;
            }
        }
        sealed class HandlerFailed : ToastNotificationHandler<TypedEventHandler<IPredefinedToastNotification, ToastNotificationFailedException>>, ITypedEventHandler_IToastNotification_Failed {
            public HandlerFailed(WinRTToastNotification sender) : base(sender) { }
            public int Invoke(IToastNotification sender, IToastFailedEventArgs args) {
                return InvokeCore(sender, args, (h, s, a) => h(s, ToastNotificationFailedException.ToException(a.Error)));
            }
        }
        #endregion Handlers
        IToastNotification Notification;
        const uint WPN_E_TOAST_NOTIFICATION_DROPPED = 0x803E0207;
        public Task<ToastNotificationResultInternal> ShowAsync() {
            if(Notification == null)
                Notification = Adapter.Create(Content.Info);
            Subscribe();
            var state = ToastActivationAsyncState.Create();
            var source = new TaskCompletionSource<ToastNotificationResultInternal>(state);
            Activated += (s, args) => {
                Unsubscribe();
                source.Task.SetActivationArgs(args);
                source.SetResult(ToastNotificationResultInternal.Activated);
            };
            Dismissed += (s, reason) => {
                Unsubscribe();
                switch(reason) {
                    case ToastDismissalReason.ApplicationHidden:
                        source.SetResult(ToastNotificationResultInternal.ApplicationHidden);
                        break;
                    case ToastDismissalReason.TimedOut:
                        source.SetResult(ToastNotificationResultInternal.TimedOut);
                        break;
                    case ToastDismissalReason.UserCanceled:
                        source.SetResult(ToastNotificationResultInternal.UserCanceled);
                        break;
                }
            };
            Failed += (s, exception) => {
                Unsubscribe();
                if((UInt32)exception.ErrorCode == WPN_E_TOAST_NOTIFICATION_DROPPED) {
                    source.SetResult(ToastNotificationResultInternal.Dropped);
                }
                else source.SetException(exception);
            };
            ComFunctions.Safe(() => Adapter.Show(Notification),
                ce => {
                    Unsubscribe();
                    source.SetException(new ToastNotificationFailedException(ce, ce.ErrorCode));
                });
            return source.Task;
        }
        public void Hide() {
            if(Notification == null) return;
            Adapter.Hide(Notification);
        }
        void Subscribe() {
            if(Notification == null) return;
            Notification.AddDismissed(handlerDismissed.Handler, out handlerDismissed.Token);
            Notification.AddActivated(handlerActivated.Handler, out handlerActivated.Token);
            Notification.AddFailed(handlerFailed.Handler, out handlerFailed.Token);
        }
        void Unsubscribe() {
            if(Notification == null) return;
            Notification.RemoveDismissed(handlerDismissed.Token);
            Notification.RemoveActivated(handlerActivated.Token);
            Notification.RemoveFailed(handlerFailed.Token);
        }
    }
    #region State
    public static class ToastActivationAsyncState {
        sealed class State {
            public string Arguments;
        }
        public static object Create() {
            return new State();
        }
        public static string ActivationArgs(this Task<ToastNotificationResultInternal> task) {
            return ((State)task.AsyncState).Arguments;
        }
        public static void SetActivationArgs(this Task<ToastNotificationResultInternal> task, string args) {
            ((State)task.AsyncState).Arguments = args;
        }
    }
    #endregion State
}