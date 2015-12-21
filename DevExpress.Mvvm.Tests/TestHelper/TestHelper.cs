using Microsoft.Silverlight.Testing;
using Microsoft.Silverlight.Testing.Client;
using Microsoft.Silverlight.Testing.Harness;
using Microsoft.Silverlight.Testing.UnitTesting.Metadata;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Windows;
using System.Windows.Browser;
using System.Windows.Controls;
using System.Linq;

namespace DevExpress {
    [CLSCompliant(false)]
    public static class TestHelper {

        static UnitTestHarness _TestHarness;
        static List<string> ignoredCategories = new List<string>();
        public static List<string> IgnoredCategories { get { return ignoredCategories; } }
        public static UnitTestHarness TestHarness {
            get {
                if(_TestHarness == null) {
                    TestPage testPage = Application.Current.RootVisual as TestPage;
                    if(testPage != null)
                        _TestHarness = testPage.UnitTestHarness;
                }
                return _TestHarness;
            }
            private set { _TestHarness = value; }
        }

        public static void RunTests(Application app) {
            UnitTestSettings settings = UnitTestSystem.CreateDefaultSettings();
            settings.StartRunImmediately = SuppressTagExpressionEditor();
            settings.ShowTagExpressionEditor = !SuppressTagExpressionEditor();
            settings.TestHarness = new AgTestHarness();
            TestHarness = (UnitTestHarness)settings.TestHarness;
            settings.TestAssemblies.Clear();
            settings.TestAssemblies.Add(Assembly.GetCallingAssembly());
            if(IsGUI()) {
                string categories = (string)HtmlPage.Window.Eval("window.external.GetIgnoreCategories();");
                IgnoredCategories.AddRange(categories.Split(';'));
                settings.LogProviders.Clear();
                settings.LogProviders.Add(new SilverlightTestGUILog());
                TestHarness.TestHarnessCompleted += TestHarnessCompleted;
            } else {
                IgnoredCategories.Add("TODO");
            }

#if DEBUG
            settings.LogProviders.Add(new DebugLogger());
#endif
            UnitTestProviders.Providers.Add(new BaseUnitTestProvider());
            app.RootVisual = CreateRootVisual(settings);
        }
        static UIElement CreateRootVisual(UnitTestSettings settings) {
            TestPage testPage = (TestPage)UnitTestSystem.CreateTestPage(settings);
            ComboBox comboBox = null;
            TestGrid grid = new TestGrid(comboBox, testPage);
            return grid;
        }
        static void OnThemeBoxSelectionChanged(object sender, SelectionChangedEventArgs e) {
            IgnoredCategories.Add(((ComboBox)sender).SelectedValue.ToString());
        }

        internal static bool SuppressTagExpressionEditor() {
            try {
                var value = HtmlPage.Window.GetProperty("dxSuppressTagExpEditor");
                return value is bool ? (bool)value : false;
            } catch {
                return false;
            }
        }
        internal static bool IsGUI() {
            if(Application.Current.IsRunningOutOfBrowser) return false;
            HtmlPage.Window.Eval("var dxSLIsGUI = typeof(window.external.Start) != 'undefined';");
            return (bool)HtmlPage.Window.GetProperty("dxSLIsGUI");
        }
        static void TestHarnessCompleted(object sender, TestHarnessCompletedEventArgs e) {
            ((SilverlightTestGUILog)TestHarness.Settings.LogProviders[0]).ProcessEndMessage();
        }
    }
}