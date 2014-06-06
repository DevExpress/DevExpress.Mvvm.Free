using Microsoft.Silverlight.Testing;
using Microsoft.Silverlight.Testing.Harness;
using Microsoft.Silverlight.Testing.UnitTesting.Metadata;
using System.Collections.Generic;

namespace DevExpress {
    class AgTestRunFilter : TestRunFilter {
        public AgTestRunFilter(UnitTestSettings settings, UnitTestHarness harness) : base(settings, harness) { }
        public override List<ITestClass> GetTestClasses(IAssembly assembly, TestClassInstanceDictionary instances) {
            List<ITestClass> result = base.GetTestClasses(assembly, instances);
            int i = 0, movedClassCount = 0;
            while(i < result.Count) {
                ITestClass testClass = result[i];
                if(testClass.Type.GetCustomAttributes(typeof(RunFirstAttribute), true).Length > 0 && result.IndexOf(testClass) != movedClassCount && !TestHelper.IsGUI()) {
                    result.Remove(testClass);
                    result.Insert(movedClassCount++, testClass);
                }
                i++;
            }
            return result;
        }
    }
}