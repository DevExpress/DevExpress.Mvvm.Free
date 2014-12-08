using System;
using System.Reflection;
using DevExpress.Mvvm.Native;
#if !NETFX_CORE
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Windows;
#else
using Windows.UI.Xaml;
#endif

namespace DevExpress.Mvvm.UI.Interactivity.Internal {
    public static class InteractionHelper {
        public static readonly DependencyProperty BehaviorInDesignModeProperty =
            DependencyProperty.RegisterAttached("BehaviorInDesignMode", typeof(InteractionBehaviorInDesignMode), typeof(InteractionHelper),
            new PropertyMetadata(InteractionBehaviorInDesignMode.Default));

        public static InteractionBehaviorInDesignMode GetBehaviorInDesignMode(DependencyObject d) {
            return (InteractionBehaviorInDesignMode)d.GetValue(BehaviorInDesignModeProperty);
        }
        public static void SetBehaviorInDesignMode(DependencyObject d, InteractionBehaviorInDesignMode behavior) {
            d.SetValue(BehaviorInDesignModeProperty, behavior);
        }
        public static bool IsInDesignMode(DependencyObject obj) {
            bool res = ViewModelBase.IsInDesignMode;
            if(obj is AttachableObjectBase) {
                res = res && !((AttachableObjectBase)obj)._AllowAttachInDesignMode;
                return res;
            }
            if(obj != null) {
                res = res && GetBehaviorInDesignMode(obj) != InteractionBehaviorInDesignMode.AsWellAsNotInDesignMode;
                return res;
            }
            return res;
        }
    }
    public enum InteractionBehaviorInDesignMode { Default, AsWellAsNotInDesignMode }

    class EventTriggerGenericHandler<TSender, TArgs> {
        readonly Action<object, object> action;
        public EventTriggerGenericHandler(Action<object, object> action) {
            this.action = action;
        }
        public void Handler(TSender sender, TArgs args) {
            action(sender, args);
        }
    }
    class EventTriggerEventSubscriber {
        Action<object, object> EventHandler;
        Delegate subscribedEventHandler;
#if NETFX_CORE
        object handlerRegistrationToken;
#endif
        public EventTriggerEventSubscriber(Action<object, object> eventHandler) {
            EventHandler = eventHandler;
        }
        public void SubscribeToEvent(object obj, string eventName) {
            if(obj == null || string.IsNullOrEmpty(eventName)) return;
            Type objType = obj.GetType();
            EventInfo eventInfo = objType.GetEvent(eventName);
            if(eventInfo == null) {
                return;
            }
            this.subscribedEventHandler = GetEventHandlerToSubscrive(eventInfo.EventHandlerType);
            if(this.subscribedEventHandler == null) return;
#if NETFX_CORE
            this.handlerRegistrationToken = eventInfo.AddEventHandlerEx(obj, this.subscribedEventHandler);
#else
            eventInfo.AddEventHandler(obj, this.subscribedEventHandler);
#endif
        }
#if !SILVERLIGHT && !NETFX_CORE
        public void SubscribeToEvent(object obj, RoutedEvent routedEvent) {
            UIElement eventSource = obj as UIElement;
            if(eventSource == null || routedEvent == null) return;
            this.subscribedEventHandler = GetEventHandlerToSubscrive(routedEvent.HandlerType);
            if(this.subscribedEventHandler == null) return;
            eventSource.AddHandler(routedEvent, this.subscribedEventHandler);
        }
#endif
        public void UnsubscribeFromEvent(object obj, string eventName) {
            if(obj == null || string.IsNullOrEmpty(eventName)) return;
            if(this.subscribedEventHandler == null) return;
            Type type = obj.GetType();
            EventInfo info = type.GetEvent(eventName);
#if NETFX_CORE
            if (this.handlerRegistrationToken is Delegate)
                info.RemoveEventHandlerEx(obj, handlerRegistrationToken as Delegate);
            else
                info.RemoveEventHandlerEx(obj, handlerRegistrationToken);
#else
            info.RemoveEventHandler(obj, this.subscribedEventHandler);
#endif
            this.subscribedEventHandler = null;
#if NETFX_CORE
            this.handlerRegistrationToken = null;
#endif
        }
#if !SILVERLIGHT && !NETFX_CORE
        public void UnsubscribeFromEvent(object obj, RoutedEvent routedEvent) {
            UIElement eventSource = obj as UIElement;
            if(eventSource == null || routedEvent == null) return;
            if(this.subscribedEventHandler == null) return;
            eventSource.RemoveHandler(routedEvent, this.subscribedEventHandler);
            this.subscribedEventHandler = null;
        }
#endif
        Delegate GetEventHandlerToSubscrive(Type eventHandlerType) {
            if(!IsEventCorrect(eventHandlerType)) return null;
            ParameterInfo[] parameters = GetParameters(eventHandlerType);
            Type handlerType = typeof(EventTriggerGenericHandler<,>).MakeGenericType(parameters[0].ParameterType, parameters[1].ParameterType);
            object instance = Activator.CreateInstance(handlerType, new object[] { EventHandler });
#if !NETFX_CORE
            return Delegate.CreateDelegate(eventHandlerType, instance, instance.GetType().GetMethod("Handler"));
#else
            return instance.GetType().GetMethod("Handler").CreateDelegate(eventHandlerType, instance);
#endif

        }
        bool IsEventCorrect(Type eventHandlerType) {
            if(!typeof(Delegate).IsAssignableFrom(eventHandlerType)) return false;
            ParameterInfo[] parameters = GetParameters(eventHandlerType);
            if(parameters.Length != 2) return false;
            if(!typeof(object).IsAssignableFrom(parameters[0].ParameterType)) return false;
            if(!typeof(object).IsAssignableFrom(parameters[1].ParameterType)) return false;
            return true;
        }
        ParameterInfo[] GetParameters(Type eventHandlerType) {
            return eventHandlerType.GetMethod("Invoke").GetParameters();
        }
    }
}