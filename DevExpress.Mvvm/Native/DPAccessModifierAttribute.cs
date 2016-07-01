using System;

namespace DevExpress.Mvvm.UI.Native {
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class DPAccessModifierAttribute : Attribute {
        public DPAccessModifierAttribute(MemberVisibility setterVisibility, MemberVisibility getterVisibility = MemberVisibility.Public) {
            SetterVisibility = setterVisibility;
            GetterVisibility = getterVisibility;
        }
        public MemberVisibility SetterVisibility { get; set; }
        public MemberVisibility GetterVisibility { get; set; }
    }
}