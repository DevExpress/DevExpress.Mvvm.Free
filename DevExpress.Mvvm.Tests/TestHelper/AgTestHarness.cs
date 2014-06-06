using Microsoft.Silverlight.Testing;
using Microsoft.Silverlight.Testing.Harness;

namespace DevExpress {
    class AgTestHarness : UnitTestHarness {
        protected override TestRunFilter CreateTestRunFilter(UnitTestSettings settings) {
            if(string.IsNullOrEmpty(settings.TagExpression))
                return new AgTestRunFilter(settings, this);
            else
                return new TagTestRunFilter(settings, this, settings.TagExpression);
        }
    }
}