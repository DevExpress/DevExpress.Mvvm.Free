using NUnit.Framework;
using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Text;
using System.Windows;

using DevExpress.Mvvm.UI.Native;
namespace DevExpress {
    [TestFixture]
    public class BaseWpfFixture : WpfTestWindow {
        protected class BindingErrorsTraceListener : TraceListener {
            public static readonly BindingErrorsTraceListener Intance = new BindingErrorsTraceListener();
            StringBuilder builder = new StringBuilder();
            bool isEnabled;
            Assembly lastAssembly;
            int failedCount;
            SourceLevels oldLevel;
            BindingErrorsTraceListener()
                : base() {
            }
            public override void Write(string message) {
                if(isEnabled)
                    builder.AppendLine(message);
            }
            public override void WriteLine(string message) {
                Write(message);
            }
            public void Reset(BaseWpfFixture fixture, bool? enable = null) {
                builder.Clear();
                Assembly currentAssembly = fixture.GetType().Assembly;
                if(lastAssembly != currentAssembly) {
                    failedCount = 0;
                    lastAssembly = currentAssembly;
                }
                SetEnabled(enable ?? GetIsEnabled(fixture));
                if(isEnabled) {
                    oldLevel = PresentationTraceSources.DataBindingSource.Switch.Level;
                    PresentationTraceSources.Refresh();
                    PresentationTraceSources.DataBindingSource.Listeners.Add(BindingErrorsTraceListener.Intance);
                    PresentationTraceSources.DataBindingSource.Switch.Level = SourceLevels.Error;
                }
            }
            public void DisableBindingErrorsDetection() {
                SetEnabled(false);
            }
            public void EnableBindingErrorsDetection() {
                SetEnabled(true);
            }
            void SetEnabled(bool isEnabled) {
                this.isEnabled = isEnabled;
            }
            bool GetIsEnabled(BaseWpfFixture fixture) {
                if(failedCount > 42)
                    return false;
                BindingErrorsDetectionAttribute bindingErrorsDetectionAttribute = TypeDescriptor.GetAttributes(fixture)[typeof(BindingErrorsDetectionAttribute)] as BindingErrorsDetectionAttribute;
                if(bindingErrorsDetectionAttribute == null || !bindingErrorsDetectionAttribute.IsBindingErrorsDetectionEnabled)
                    return false;
                return true;
            }

            public void Report(BaseWpfFixture fixture) {
                if(!isEnabled)
                    return;
                Flush();
                if(builder.Length > 0) {
                    failedCount++;
                    builder.Insert(0, "Binding errors occured while testing: \r\n");
                    Assert.Fail(builder.ToString());
                }
                isEnabled = false;
                PresentationTraceSources.DataBindingSource.Listeners.Remove(BindingErrorsTraceListener.Intance);
                PresentationTraceSources.DataBindingSource.Switch.Level = oldLevel;
            }
        }
        protected override void SetUpCore() {
            BindingErrorsTraceListener.Intance.Reset(this);
            base.SetUpCore();
        }
        protected override void TearDownCore() {
            BindingErrorsTraceListener.Intance.Report(this);
            base.TearDownCore();
        }
    }

    public class BaseWpfFixtureEx : BaseWpfFixture {
        public static T FindGridElement<T>(DependencyObject obj, bool failIfNothingFound = true) where T : DependencyObject {
            ObjectChecker<T> checker = new ObjectChecker<T>();
            DObjectChecker.CheckSubTree(obj, checker, failIfNothingFound);
            return (T)checker.Element;
        }
        public static List<T> FindAllGridElements<T>(DependencyObject obj) where T : DependencyObject {
            ObjectsChecker<T> checker = new ObjectsChecker<T>();
            checker.CheckSubTree(obj);
            return checker.Elements;
        }
        public static T FindGridElement<T>(DependencyObject obj, DependencyProperty property, object value,
                    bool exactType = false, bool onlyVisibleChildren = false, bool failIfNothingFound = true)
                where T : DependencyObject {
            ObjectChecker<T> checker
                = new ObjectChecker<T>(new DPropertyInfo(property, value), exactType, onlyVisibleChildren);
            DObjectChecker.CheckSubTree(obj, checker, failIfNothingFound);
            return (T)checker.Element;
        }
        class ObjectChecker<T> : DObjectChecker where T : DependencyObject {
            readonly bool exactType;
            readonly bool onlyVisibleChildren;

            public ObjectChecker() : base(typeof(T)) { }
            public ObjectChecker(Type type)
                : base(typeof(T)) {

            }
            public ObjectChecker(DPropertyInfo info)
                : this(info, false) { }
            public ObjectChecker(DPropertyInfo info, bool exactType)
                : this(info, exactType, false) { }
            public ObjectChecker(DPropertyInfo info, bool exactType, bool onlyVisibleChildren)
                : base(typeof(T), info) {
                this.exactType = exactType;
                this.onlyVisibleChildren = onlyVisibleChildren;
            }

            protected override bool CheckObjectType(DependencyObject dObject) {
                Type t = dObject.GetType();
                bool res = Type == t || (!exactType && t.IsSubclassOf(typeof(T)));
                if(res) {
                    UIElement elem = dObject as UIElement;
                    if(onlyVisibleChildren && !LayoutHelper.IsVisibleInTree(elem))
                        res = false;
                }
                return res;
            }
        }
        class ObjectsChecker<T> : ObjectChecker<T> where T : DependencyObject {
            List<T> elements;
            public ObjectsChecker()
                : base(typeof(T)) {
                this.elements = new List<T>();
            }

            public List<T> Elements {
                get { return elements; }
            }
            public void CheckSubTree(DependencyObject dObject) {
                VisualTreeEnumerator visualTreeEnumerator = new VisualTreeEnumerator(dObject);
                DependencyObjectCheckerEnumerator dObjectCheckerEnumerator = new DependencyObjectCheckerEnumerator(this);
                dObjectCheckerEnumerator.MoveNext();
                while(visualTreeEnumerator.MoveNext()) {
                    this.CheckElement(visualTreeEnumerator.Current, visualTreeEnumerator.Level);
                }
            }
            protected override bool CheckElement(DependencyObject dObject, int level) {
                if(!CheckObjectType(dObject))
                    return false;
                foreach(DPropertyInfo item in PropertyInfo) {
                    if(!item.CompareWith(dObject))
                        return false;
                }
                elements.Add((T)dObject);
                return false;
            }
        }
    }
}
namespace DevExpress {
    using System.Windows.Threading;

    [SetUpFixture]
    class WpfSetupFixtureBase {
        Dispatcher dispatcher;
        [OneTimeSetUp]
        public void RunBeforeAnyTests() {
            dispatcher = Dispatcher.CurrentDispatcher;
            DispatcherHelper.ForceIncreasePriorityContextIdleMessages();
        }
        [OneTimeTearDown]
        public void RunAfterAnyTests() {
            if(dispatcher != null) {
                dispatcher.InvokeShutdown();
                dispatcher = null;
            }
        }
    }
}