#if !NETFX_CORE
using NUnit.Framework;
#endif
using System;

namespace DevExpress {
#if !NETFX_CORE
    public class DXExpectedExceptionAttribute : ExpectedExceptionAttribute {
        public DXExpectedExceptionAttribute(Type exceptionType, string message = null)
            : base(exceptionType) {
            ExpectedMessage = message;
        }
    }
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

namespace DevExpress.Tests {
    public class DevExpressTestsNamespace {

    }
}