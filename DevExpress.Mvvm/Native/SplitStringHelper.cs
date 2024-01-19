using System.Text.RegularExpressions;

namespace DevExpress.Mvvm.Native {
    internal static class SplitStringHelper {
        static Regex reg1 = new Regex("(\\p{Ll})(\\p{Lu})", RegexOptions.Compiled);
        static Regex reg2 = new Regex("(\\p{Lu}{2})(\\p{Lu}\\p{Ll}{2})", RegexOptions.Compiled);
        public static string SplitPascalCaseString(string value) {
            if(value == null)
                return string.Empty;
            value = reg1.Replace(value, "$1 $2");
            value = reg2.Replace(value, "$1 $2");
            return value;
        }
    }
}