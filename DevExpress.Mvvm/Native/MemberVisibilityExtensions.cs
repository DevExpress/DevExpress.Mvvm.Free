using System;

namespace DevExpress.Mvvm.UI.Native {
    public static class MemberVisibilityExtensions {
        public static bool IsStrongerThen(this MemberVisibility visibility, MemberVisibility value) {
            switch(visibility) {
            case MemberVisibility.Internal:
                switch(value) {
                case MemberVisibility.Internal: return false;
                case MemberVisibility.Private: return false;
                case MemberVisibility.Protected: return true;
                case MemberVisibility.ProtectedInternal: return false;
                case MemberVisibility.Public: return true;
                default: throw new InvalidOperationException();
                }
            case MemberVisibility.Private:
                switch(value) {
                case MemberVisibility.Internal: return true;
                case MemberVisibility.Private: return false;
                case MemberVisibility.Protected: return true;
                case MemberVisibility.ProtectedInternal: return true;
                case MemberVisibility.Public: return true;
                default: throw new InvalidOperationException();
                }
            case MemberVisibility.Protected:
                switch(value) {
                case MemberVisibility.Internal: return false;
                case MemberVisibility.Private: return false;
                case MemberVisibility.Protected: return false;
                case MemberVisibility.ProtectedInternal: return true;
                case MemberVisibility.Public: return false;
                default: throw new InvalidOperationException();
                }
            case MemberVisibility.ProtectedInternal:
                switch(value) {
                case MemberVisibility.Internal: return false;
                case MemberVisibility.Private: return false;
                case MemberVisibility.Protected: return false;
                case MemberVisibility.ProtectedInternal: return false;
                case MemberVisibility.Public: return true;
                default: throw new InvalidOperationException();
                }
            case MemberVisibility.Public:
                switch(value) {
                case MemberVisibility.Internal: return false;
                case MemberVisibility.Private: return false;
                case MemberVisibility.Protected: return false;
                case MemberVisibility.ProtectedInternal: return false;
                case MemberVisibility.Public: return false;
                default: throw new InvalidOperationException();
                }
            default:
                throw new InvalidOperationException();
            }
        }
    }
}