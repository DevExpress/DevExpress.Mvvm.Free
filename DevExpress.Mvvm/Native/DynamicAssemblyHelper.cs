using DevExpress.Internal;
using System;
using System.Collections;
using System.Reflection;

namespace DevExpress.Mvvm.Native {
    static class DynamicAssemblyHelper {
        static Lazy<Assembly> mvvmUIAssembly = new Lazy<Assembly>(() => ResolveAssembly(MvvmAssemblyHelper.MvvmUIAssemblyName));
        public static Assembly MvvmUIAssembly { get { return mvvmUIAssembly.Value; } }

        static Assembly ResolveAssembly(string asmName) {
            IEnumerable assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach(Assembly asm in assemblies) {
                if(PartialNameEquals(asm.FullName, asmName))
                    return asm;
            }
#pragma warning disable DX0010
            return Assembly.Load(asmName);
#pragma warning restore DX0010
        }
        static bool PartialNameEquals(string asmName0, string asmName1) {
            return string.Equals(GetPartialName(asmName0), GetPartialName(asmName1),
                StringComparison.InvariantCultureIgnoreCase);
        }
        static string GetPartialName(string asmName) {
            int nameEnd = asmName.IndexOf(',');
            return nameEnd < 0 ? asmName : asmName.Remove(nameEnd);
        }
    }
}