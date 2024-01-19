using System;

namespace DevExpress.Mvvm.UI.Native {
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class DPAccessModifierAttribute : Attribute {
        public DPAccessModifierAttribute(MemberVisibility setterVisibility = MemberVisibility.Public, MemberVisibility getterVisibility = MemberVisibility.Public, bool nonBrowsable = false) {
            SetterVisibility = setterVisibility;
            GetterVisibility = getterVisibility;
            NonBrowsable = nonBrowsable;
        }
        public MemberVisibility SetterVisibility { get; set; }
        public MemberVisibility GetterVisibility { get; set; }
        public bool NonBrowsable { get; set; }
    }
}