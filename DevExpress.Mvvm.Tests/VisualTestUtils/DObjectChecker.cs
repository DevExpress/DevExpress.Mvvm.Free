using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Markup;
using System.Windows.Media;
using System.Text;

using DevExpress.Mvvm.UI.Native;
namespace DevExpress {
    [ContentProperty("Children")]
    public class DObjectChecker {
        public static void CheckNoSubTree(DependencyObject dObject, DObjectChecker checker) {
            try {
                CheckSubTree(dObject, checker);
            } catch {
                return;
            }
            Assert.Fail();
        }
        public static void CheckSubTree(DependencyObject dObject, DObjectChecker checker) {
            CheckSubTree(dObject, checker, true);
        }
        public static void CheckSubTree(DependencyObject dObject, DObjectChecker checker, bool failIfNothingFound) {
            VisualTreeEnumerator visualTreeEnumerator = new VisualTreeEnumerator(dObject);
            DependencyObjectCheckerEnumerator dObjectCheckerEnumerator = new DependencyObjectCheckerEnumerator(checker);
            dObjectCheckerEnumerator.MoveNext();
            while(visualTreeEnumerator.MoveNext()) {
                if(dObjectCheckerEnumerator.Current.CheckElement(visualTreeEnumerator.Current, visualTreeEnumerator.Level)) {
                    DObjectChecker parent = (DObjectChecker)dObjectCheckerEnumerator.CurrentParent;
                    if(parent != null)
                        Assert.IsTrue(IsParent(dObjectCheckerEnumerator.Current.Element, parent.Element));
                    if(!dObjectCheckerEnumerator.MoveNext()) {
                        checker.CheckLevels();
                        return;
                    }
                }
            }
            if(failIfNothingFound)
                Assert.Fail("Element not found: " + dObjectCheckerEnumerator.Current);
        }
        static bool IsParent(DependencyObject element, DependencyObject parent) {
            DependencyObject currentParent = element;
            while(currentParent != null) {
                if(object.ReferenceEquals(currentParent, parent))
                    return true;
                currentParent = VisualTreeHelper.GetParent(currentParent);
            }
            return false;
        }
        DependencyObject element;
        int visualTreeLevel;
        public Type Type { get; set; }
        public DObjectCheckerCollection Children { get; set; }
        public DependencyPropertyInfoCollection PropertyInfo { get; set; }
        public DependencyObject Element { get { return element; } }
        public DObjectChecker() {
            PropertyInfo = new DependencyPropertyInfoCollection();
            Children = new DObjectCheckerCollection();
        }
        public DObjectChecker(Type dObjectType)
            : this(dObjectType, null, null) {
        }
        public DObjectChecker(Type dObjectType, IList<DPropertyInfo> info)
            : this(dObjectType, info, null) {
        }
        public DObjectChecker(Type dObjectType, DObjectChecker child)
            : this(dObjectType, new DObjectChecker[] { child }) {
        }
        public DObjectChecker(Type dObjectType, DPropertyInfo info)
            : this(dObjectType, new DPropertyInfo[] { info }) {
        }
        public DObjectChecker(Type dObjectType, IList<DObjectChecker> children)
            : this(dObjectType, null, children) {
        }
        public DObjectChecker(Type dObjectType, IList<DPropertyInfo> info, IList<DObjectChecker> children)
            : this() {
            this.Type = dObjectType;
            if(info != null)
                PropertyInfo.AddRange(info);
            if(children != null)
                Children.AddRange(children);
        }

        void CheckLevels() {
            if(Children.Count == 0)
                return;
            int firstChildLevel = Children[0].visualTreeLevel;
            foreach(DObjectChecker child in Children) {
                Assert.AreEqual(firstChildLevel, child.visualTreeLevel);
                child.CheckLevels();
            }
        }
        protected virtual bool CheckElement(DependencyObject dObject, int level) {
            if(!CheckObjectType(dObject))
                return false;
            foreach(DPropertyInfo item in PropertyInfo) {
                if(!item.CompareWith(dObject))
                    return false;
            }
            element = dObject;
            visualTreeLevel = level;
            return true;
        }
        protected virtual bool CheckObjectType(DependencyObject dObject) {
            return Type == dObject.GetType();
        }
        public override string ToString() {
            StringBuilder builder = new StringBuilder(string.Format("Type: {0} PropertyInfo: ", Type.Name));
            foreach(DPropertyInfo item in PropertyInfo) {
                builder.Append(" " + item);
            }
            return builder.ToString();
        }
    }
    public class DPropertyInfo {
        [IgnoreDependencyPropertiesConsistencyChecker]
        DependencyProperty property;
        public DependencyProperty Property { get { return property; } set { property = value; } }
        public Type Type { get; set; }
        public object Value { get; set; }
        public DPropertyInfo() {
        }
        public DPropertyInfo(DependencyProperty property, object value) {
            Property = property;
            Value = value;
        }
        public DPropertyInfo(DependencyProperty property, Type type) {
            Property = property;
            Type = type;
        }

        public bool CompareWith(DependencyObject dObject) {
            object value = dObject.GetValue(Property);
            return Value != null ? object.Equals(Value, value) : Type.IsAssignableFrom(value.GetType());
        }
        public override string ToString() {
            return string.Format("Property: {0}, Value: {2}, Type: {1}", Property.Name, Type != null ? Type.Name : Value.GetType().Name, Value);
        }
    }
    public class DObjectCheckerCollection : List<DObjectChecker> {
    }
    public class DependencyPropertyInfoCollection : List<DPropertyInfo> {
    }

    [CLSCompliant(false)]
    public class DependencyObjectCheckerEnumerator : NestedObjectEnumeratorBase {
        public DObjectChecker Current { get { return (DObjectChecker)Enumerator.Current; } }
        public DependencyObjectCheckerEnumerator(DObjectChecker checker)
            : base(checker) {
        }
        protected override IEnumerator GetNestedObjects(object obj) {
            return ((DObjectChecker)obj).Children.GetEnumerator();
        }
    }
}