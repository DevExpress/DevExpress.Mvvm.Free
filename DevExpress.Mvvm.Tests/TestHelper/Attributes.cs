using System;

namespace DevExpress {
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public sealed class TestFixtureAttribute : Attribute { }

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public sealed class TestAttribute : Attribute { }

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public sealed class SetUpAttribute : Attribute { }

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public sealed class TearDownAttribute : Attribute { }

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public sealed class TestFixtureSetUpAttribute : Attribute { }

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public sealed class TestFixtureTearDownAttribute : Attribute { }

    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = false)]
    public sealed class IgnoreAttribute : Attribute {
        public IgnoreAttribute() { }
        public IgnoreAttribute(string reason) { }
    }

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Event | AttributeTargets.Field, AllowMultiple = false)]
    public class CategoryAttribute : Attribute {
        public CategoryAttribute(string category) { Category = category; }

        public string Category { get; private set; }
    }

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public sealed class RunFirstAttribute : Attribute {
        public RunFirstAttribute() { }
    }
}