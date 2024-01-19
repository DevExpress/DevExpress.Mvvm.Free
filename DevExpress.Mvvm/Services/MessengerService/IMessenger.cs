using System;

namespace DevExpress.Mvvm {
    public interface IMessenger {
        void Register<TMessage>(object recipient, object token, bool receiveInheritedMessages, Action<TMessage> action);
        void Send<TMessage>(TMessage message, Type messageTargetType, object token);
        void Unregister(object recipient);
        void Unregister<TMessage>(object recipient, object token, Action<TMessage> action);
    }
}