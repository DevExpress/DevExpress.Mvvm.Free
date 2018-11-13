using System;

namespace DevExpress.Mvvm {
    public static class MessengerExtensions {
        public static void Register<TMessage>(this IMessenger messenger, object recipient, Action<TMessage> action) {
            VerifyMessenger(messenger);
            messenger.Register(recipient, null, false, action);
        }
        public static void Register<TMessage>(this IMessenger messenger, object recipient, bool receiveInheritedMessagesToo, Action<TMessage> action) {
            VerifyMessenger(messenger);
            messenger.Register(recipient, null, receiveInheritedMessagesToo, action);
        }
        public static void Register<TMessage>(this IMessenger messenger, object recipient, object token, Action<TMessage> action) {
            VerifyMessenger(messenger);
            messenger.Register(recipient, token, false, action);
        }

        public static void Send<TMessage>(this IMessenger messenger, TMessage message) {
            VerifyMessenger(messenger);
            messenger.Send(message, null, null);
        }
        public static void Send<TMessage, TTarget>(this IMessenger messenger, TMessage message) {
            VerifyMessenger(messenger);
            messenger.Send(message, typeof(TTarget), null);
        }
        public static void Send<TMessage>(this IMessenger messenger, TMessage message, object token) {
            VerifyMessenger(messenger);
            messenger.Send(message, null, token);
        }

        public static void Unregister<TMessage>(this IMessenger messenger, object recipient) {
            VerifyMessenger(messenger);
            messenger.Unregister<TMessage>(recipient, null, null);
        }
        public static void Unregister<TMessage>(this IMessenger messenger, object recipient, object token) {
            VerifyMessenger(messenger);
            messenger.Unregister<TMessage>(recipient, token, null);
        }
        public static void Unregister<TMessage>(this IMessenger messenger, object recipient, Action<TMessage> action) {
            VerifyMessenger(messenger);
            messenger.Unregister(recipient, null, action);
        }

        static void VerifyMessenger(IMessenger messenger) {
            if(messenger == null)
                throw new ArgumentNullException("messenger");
        }
    }
}