using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DevExpress.Mvvm.UI.Interactivity {
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
    public sealed class TargetTypeAttribute : Attribute {
        public TargetTypeAttribute(Type targetType) : this(true, targetType) { }
        public TargetTypeAttribute(bool isTargetType, Type targetType) {
            IsTargetType = isTargetType;
            TargetType = targetType;
        }
        public bool IsTargetType { get; private set; }
        public Type TargetType { get; private set; }
    }
    namespace Internal {
        [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
        public sealed class UniqueBehaviorTypeAttribute : Attribute {
            public UniqueBehaviorTypeAttribute() { }

            public static Type GetDeclaredType(Type type) {
                var baseType = type;
                if(!baseType.GetCustomAttributes(typeof(UniqueBehaviorTypeAttribute), true).Any())
                    return null;
                while(baseType.BaseType != null) {
                    var attributes = baseType.BaseType.GetCustomAttributes(true).OfType<UniqueBehaviorTypeAttribute>();
                    if(attributes.Any())
                        baseType = baseType.BaseType;
                    else
                        break;
                }
                return baseType;
            }


        }
    }
}