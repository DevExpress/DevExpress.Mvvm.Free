#if DEBUGTEST || MVVM
#if !NETFX_CORE
using NUnit.Framework;
#endif
#if !NETFX_CORE && !FREE
using DevExpress.Xpf.Core.Tests;
#endif
using System;
using NUnit.Framework.Constraints;
using System.Collections;

#if MVVM
namespace DevExpress {
#else
namespace DevExpress.Xpf.Core.Tests {
#endif
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public sealed class AsynchronousAttribute : Attribute {
    }
    public class BindingErrorsDetectionAttribute : Attribute {
        public bool IsBindingErrorsDetectionEnabled { get; set; }
        public BindingErrorsDetectionAttribute(bool isBindingErrorsDetectionEnabled) {
            this.IsBindingErrorsDetectionEnabled = isBindingErrorsDetectionEnabled;
        }
    }
#if !NETFX_CORE
    public class TestCategoryAttribute : NUnit.Framework.CategoryAttribute {
        public TestCategoryAttribute(string name) : base(name) { }
    }
#endif
}
#endif

namespace DevExpress.Tests {
    public class DevExpressTestsNamespace {

    }

    public class DXRequiresSTAAttribute : NUnit.Framework.PropertyAttribute {
        public DXRequiresSTAAttribute()
            : base("APARTMENT_STATE", 0) { 
        }
    }
}
namespace NUnit.Framework.SyntaxHelpers {//Remove after migration on NUnit 2.6
    public class TestTuple { }
}