using System;
using System.Reflection;
using DevExpress.Mvvm.Native;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Windows;

namespace DevExpress.Mvvm.UI.Interactivity.Internal {
    public static class InteractionHelper {
        public static readonly DependencyProperty EnableBehaviorsInDesignTimeProperty =
            DependencyProperty.RegisterAttached("EnableBehaviorsInDesignTime", typeof(bool), typeof(InteractionHelper),
            new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.Inherits));
        public static bool GetEnableBehaviorsInDesignTime(DependencyObject d) {
   if(d == null) return false;
            return (bool)d.GetValue(EnableBehaviorsInDesignTimeProperty);
        }
        public static void SetEnableBehaviorsInDesignTime(DependencyObject d, bool value) {
            d.SetValue(EnableBehaviorsInDesignTimeProperty, value);
        }
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
    public class EventTriggerEventSubscriber {
        Action<object, object> EventHandler;
        Delegate subscribedEventHandler;
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
            this.subscribedEventHandler = CreateEventHandler(eventInfo.EventHandlerType);
            if(this.subscribedEventHandler == null) return;
            eventInfo.AddEventHandler(obj, this.subscribedEventHandler);
        }
        public void SubscribeToEvent(object obj, RoutedEvent routedEvent) {
            UIElement eventSource = obj as UIElement;
            if(eventSource == null || routedEvent == null) return;
            this.subscribedEventHandler = CreateEventHandler(routedEvent.HandlerType);
            if(this.subscribedEventHandler == null) return;
            eventSource.AddHandler(routedEvent, this.subscribedEventHandler);
        }
        public void UnsubscribeFromEvent(object obj, string eventName) {
            if(obj == null || string.IsNullOrEmpty(eventName)) return;
            if(this.subscribedEventHandler == null) return;
            Type type = obj.GetType();
            EventInfo info = type.GetEvent(eventName);
            info.RemoveEventHandler(obj, this.subscribedEventHandler);
            this.subscribedEventHandler = null;
        }
        public void UnsubscribeFromEvent(object obj, RoutedEvent routedEvent) {
            UIElement eventSource = obj as UIElement;
            if(eventSource == null || routedEvent == null) return;
            if(this.subscribedEventHandler == null) return;
            eventSource.RemoveHandler(routedEvent, this.subscribedEventHandler);
            this.subscribedEventHandler = null;
        }
        public Delegate CreateEventHandler(Type eventHandlerType) {
            if(!IsEventCorrect(eventHandlerType)) return null;
            ParameterInfo[] parameters = GetParameters(eventHandlerType);
            Type handlerType = typeof(EventTriggerGenericHandler<,>).MakeGenericType(parameters[0].ParameterType, parameters[1].ParameterType);
            object instance = Activator.CreateInstance(handlerType, new object[] { EventHandler });
            return Delegate.CreateDelegate(eventHandlerType, instance, instance.GetType().GetMethod("Handler"));
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