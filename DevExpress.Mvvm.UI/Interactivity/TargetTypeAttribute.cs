using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DevExpress.Mvvm.UI.Interactivity {
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
    public sealed class TargetTypeAttribute : Attribute {
        public TargetTypeAttribute(Type targetType) : this(true, targetType) { }
        public TargetTypeAttribute(bool isTargeType, Type targetType) {
            IsTargetType = isTargeType;
            TargetType = targetType;
        }
        public bool IsTargetType { get; private set; }
        public Type TargetType { get; private set; }
    }
}