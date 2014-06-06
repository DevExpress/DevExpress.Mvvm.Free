using DevExpress.Mvvm.UI.Native;
using System;
using System.Windows;

namespace DevExpress.Mvvm.UI.Interactivity {
    public static class Interaction {
        #region
#if SILVERLIGHT
        const string BehaviorsPropertyName = "Behaviors";
        const string TriggersPropertyName = "Triggers";
#else
        const string BehaviorsPropertyName = "BehaviorsInternal";
        const string TriggersPropertyName = "TriggersInternal";
#endif
        [IgnoreDependencyPropertiesConsistencyChecker]
        public static readonly DependencyProperty BehaviorsProperty =
            DependencyProperty.RegisterAttached(BehaviorsPropertyName, typeof(BehaviorCollection), typeof(Interaction), new PropertyMetadata(null, OnCollectionChanged));
        [IgnoreDependencyPropertiesConsistencyChecker]
        [Obsolete("This property is obsolete. Use the Behaviors property instead.")]
        public static readonly DependencyProperty TriggersProperty =
            DependencyProperty.RegisterAttached(TriggersPropertyName, typeof(TriggerCollection), typeof(Interaction), new PropertyMetadata(null, OnCollectionChanged));
        #endregion

        public static BehaviorCollection GetBehaviors(DependencyObject d) {
            BehaviorCollection behaviors = (BehaviorCollection)d.GetValue(BehaviorsProperty);
            if(behaviors == null) {
                behaviors = new BehaviorCollection();
                d.SetValue(BehaviorsProperty, behaviors);
            }
            return behaviors;
        }
        [Obsolete("This method is obsolete. Use the GetBehaviors method instead.")]
        public static TriggerCollection GetTriggers(DependencyObject d) {
            TriggerCollection triggers = (TriggerCollection)d.GetValue(TriggersProperty);
            if(triggers == null) {
                triggers = new TriggerCollection();
                d.SetValue(TriggersProperty, triggers);
            }
            return triggers;
        }
        static void OnCollectionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            IAttachableObject oldValue = (IAttachableObject)e.OldValue;
            IAttachableObject newValue = (IAttachableObject)e.NewValue;
            if(object.ReferenceEquals(oldValue, newValue)) return;
            if(oldValue != null && oldValue.AssociatedObject != null)
                oldValue.Detach();
            if(newValue != null && d != null) {
                if(newValue.AssociatedObject != null)
                    throw new InvalidOperationException();
                newValue.Attach(d);
            }
        }
    }
}