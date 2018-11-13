using NUnit.Framework;
using System;
using NUnit.Framework.Constraints;
using System.Collections;

namespace DevExpress {
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public sealed class AsynchronousAttribute : Attribute {
    }
    public class BindingErrorsDetectionAttribute : Attribute {
        public bool IsBindingErrorsDetectionEnabled { get; set; }
        public BindingErrorsDetectionAttribute(bool isBindingErrorsDetectionEnabled) {
            this.IsBindingErrorsDetectionEnabled = isBindingErrorsDetectionEnabled;
        }
    }
    public class TestCategoryAttribute : NUnit.Framework.CategoryAttribute {
        public TestCategoryAttribute(string name) : base(name) { }
    }
}

namespace DevExpress.Tests {
    public class DevExpressTestsNamespace {

    }

    public class DXRequiresSTAAttribute : NUnit.Framework.PropertyAttribute {
        public DXRequiresSTAAttribute()
            : base("APARTMENT_STATE", 0) {
        }
    }
}
namespace NUnit.Framework.SyntaxHelpers {
    public class TestTuple { }
}