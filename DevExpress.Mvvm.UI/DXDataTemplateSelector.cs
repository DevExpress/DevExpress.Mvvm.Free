using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Markup;

namespace DevExpress.Xpf.DXBinding {
    [ContentProperty("Template")]
    public class DXDataTemplateTrigger : DXTriggerBase {
        DataTemplate template;
        public DataTemplate Template {
            get { return template; }
            set {
                WritePreamble();
                template = value;
            }
        }
    }
    public abstract class DXTriggerBase : Freezable {
        BindingBase binding;
        public BindingBase Binding {
            get { return binding; }
            set {
                WritePreamble();
                binding = value;
            }
        }

        public object Value {
            get { return (object)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }
        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register("Value", typeof(object), typeof(DXTriggerBase), new PropertyMetadata(null));

        protected override Freezable CreateInstanceCore() {
            throw new NotSupportedException();
        }
    }
    public class DXDataTemplateTriggerCollection : FreezableCollection<DXDataTemplateTrigger> { }
    [ContentProperty("Items")]
    public class DXDataTemplateSelector : DataTemplateSelector {
        public DXDataTemplateSelector() {
            Items = new DXDataTemplateTriggerCollection();
        }
        public DXDataTemplateTriggerCollection Items { get; private set; }


        Style style;
        public sealed override DataTemplate SelectTemplate(object item, DependencyObject container) {
            if(!DesignerProperties.GetIsInDesignMode(Items))
                Items.Freeze();
            if(style == null) {
                style = DXTriggerHelper.CreateStyle<DXDataTemplateTrigger>(Items, FrameworkElement.TagProperty, e => e.Template, null);
            }
            var element = new FrameworkElement();
            element.DataContext = item;
            element.Style = style;
            try {
                return (DataTemplate)element.Tag;
            } finally {
                element.DataContext = null;
            }
        }
    }
    public static class DXTriggerHelper {
        public static Style CreateStyle<T>(IEnumerable<T> items, DependencyProperty dependencyProperty, Func<T, object> getValue, object defaultValue) where T : DXTriggerBase {
            Style style = new Style();
            if(defaultValue != null)
                style.Setters.Add(new Setter(dependencyProperty, defaultValue));
            foreach(var conditionItem in items) {
                var setter = new Setter(dependencyProperty, getValue(conditionItem));
                if(conditionItem.Binding != null) {
                    var trigger = new DataTrigger() {
                        Binding = conditionItem.Binding,
                        Value = conditionItem.Value,
                    };
                    trigger.Setters.Add(setter);
                    style.Triggers.Add(trigger);
                } else {
                    style.Setters.Add(setter);
                }
            }
            style.Seal();
            return style;
        }
    }
}