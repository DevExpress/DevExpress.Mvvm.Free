using Microsoft.Silverlight.Testing.Harness;
using Microsoft.Silverlight.Testing.UnitTesting.Metadata;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace DevExpress {
    class BaseUnitTestProvider : IUnitTestProvider {
        private Dictionary<Assembly, IAssembly> _assemblyCache = new Dictionary<Assembly, IAssembly>(2);
        private const UnitTestProviderCapabilities MyCapabilities = UnitTestProviderCapabilities.MethodCanCategorize;
        private const string ProviderName = "VSTT";

        public BaseUnitTestProvider() {
            UnitTestProviders.Providers.Clear();
        }
        public IAssembly GetUnitTestAssembly(UnitTestHarness testHarness, Assembly assemblyReference) {
            if(!this._assemblyCache.ContainsKey(assemblyReference)) {
                this._assemblyCache[assemblyReference] = new BaseTestAssembly(this, testHarness, assemblyReference);
            }
            return this._assemblyCache[assemblyReference];
        }

        public bool HasCapability(UnitTestProviderCapabilities capability) {
            return ((capability & UnitTestProviderCapabilities.MethodCanCategorize) == capability);
        }

        public bool IsFailedAssert(Exception exception) {
            Type type = exception.GetType();
            Type c = typeof(AssertFailedException);
            if(type != c) {
                return type.IsSubclassOf(c);
            }
            return true;
        }
        public UnitTestProviderCapabilities Capabilities {
            get {
                return UnitTestProviderCapabilities.MethodCanCategorize;
            }
        }

        public string Name {
            get {
                return "VSTT";
            }
        }
    }
}