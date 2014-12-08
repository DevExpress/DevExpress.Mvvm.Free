using System;
using System.Collections;
using System.Windows;
using System.Collections.Generic;
using System.Linq;
#if !NETFX_CORE
using System.Windows.Media;
#else
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;
#endif
namespace DevExpress.Mvvm.UI.Native {

    public enum EnumeratorDirection { Forward, Backward }

    public abstract class NestedObjectEnumeratorBase : IEnumerator {
        #region inner class
        protected class EnumStack : Stack<IEnumerator> {
            public IEnumerator TopEnumerator {
                get { return (IsEmpty == false) ? Peek() : null; }
            }
            public bool IsEmpty {
                get { return Count == 0; }
            }

            public EnumStack() {
            }
        }
        #endregion
        protected static readonly IEnumerator EmptyEnumerator = (new object[0]).GetEnumerator();
        IEnumerator objects;
        protected EnumStack stack;

        protected IEnumerator Enumerator { get { return (IEnumerator)this; } }
        public object CurrentParent {
            get { return GetParents().FirstOrDefault(); }
        }
        public IEnumerable<object> GetParents() {
            IEnumerator<IEnumerator> en = stack.GetEnumerator();
            if(en.MoveNext()) {
                while(en.MoveNext()) {
                    yield return en.Current.Current;
                }
            }
        }
        public int Level { get { return stack.Count; } }

        protected NestedObjectEnumeratorBase(object obj) {
            this.objects = new object[] { obj }.GetEnumerator();
            stack = new EnumStack();
            Reset();
        }
        object IEnumerator.Current {
            get { return stack.TopEnumerator.Current; }
        }
        public virtual bool MoveNext() {
            if(stack.IsEmpty) {
                stack.Push(objects);
                return stack.TopEnumerator.MoveNext();
            }
            IEnumerator nestedObjects = GetNestedObjects(Enumerator.Current);
            if(nestedObjects.MoveNext()) {
                stack.Push(nestedObjects);
                return true;
            }
            while(stack.TopEnumerator.MoveNext() == false) {
                stack.Pop();
                if(stack.IsEmpty)
                    return false;
            }
            return true;
        }
        protected abstract IEnumerator GetNestedObjects(object obj);
        public virtual void Reset() {
            stack.Clear();
            objects.Reset();
        }
    }
    public class VisualTreeEnumerator : NestedObjectEnumeratorBase {
        static IEnumerator<object> GetDependencyObjectEnumerator(DependencyObject dObject, int startIndex, int endIndex, int step) {
            for(int i = startIndex; i != endIndex; i += step) {
                yield return VisualTreeHelper.GetChild(dObject, i);
            }
        }
        EnumeratorDirection direction = EnumeratorDirection.Forward;
        public DependencyObject Current {
            get { return (DependencyObject)Enumerator.Current; }
        }
        public VisualTreeEnumerator(DependencyObject dObject)
            : this(dObject, EnumeratorDirection.Forward) {
        }
        protected VisualTreeEnumerator(DependencyObject dObject, EnumeratorDirection direction)
            : base(dObject) {
            this.direction = direction;
        }
        protected virtual bool IsObjectVisual(DependencyObject d) {
#if !SILVERLIGHT && !NETFX_CORE || SLDESIGN
            return d is Visual;
#else
            return d is UIElement;
#endif
        }
        protected override IEnumerator GetNestedObjects(object obj) {
            DependencyObject dObject = (DependencyObject)obj;
            int count = IsObjectVisual(dObject) ? VisualTreeHelper.GetChildrenCount(dObject) : 0;
            return direction == EnumeratorDirection.Forward ?
                GetDependencyObjectEnumerator(dObject, 0, count, 1) :
                GetDependencyObjectEnumerator(dObject, count - 1, -1, -1);
        }
        public IEnumerable<DependencyObject> GetVisualParents() {
            return GetParents().Cast<DependencyObject>();
        }
    }
#if !SILVERLIGHT && !NETFX_CORE
    public class LogicalTreeEnumerator : VisualTreeEnumerator {
        static IEnumerator GetVisualAndLogicalChilder(object obj, IEnumerator visualChildren, bool dependencyObjectsOnly) {
            while(visualChildren.MoveNext())
                yield return visualChildren.Current;
            foreach(object logicalChild in LogicalTreeHelper.GetChildren((DependencyObject)obj)) {
                if(dependencyObjectsOnly && !(logicalChild is DependencyObject)) continue;
                yield return logicalChild;
            }
        }
        public LogicalTreeEnumerator(DependencyObject dObject)
            : base(dObject) {
        }
        protected virtual bool DependencyObjectsOnly { get { return false; } }
        protected override IEnumerator GetNestedObjects(object obj) {
            return GetVisualAndLogicalChilder(obj, base.GetNestedObjects(obj), DependencyObjectsOnly);
        }
    }
    public class SerializationEnumerator : LogicalTreeEnumerator {
        protected override bool DependencyObjectsOnly {
            get { return true; }
        }
        public SerializationEnumerator(DependencyObject dObject)
            : base(dObject) {
        }
    }
    public class VisualTreeEnumeratorWithConditionalStop : IEnumerator<DependencyObject> {
        DependencyObject Root;
        Stack<DependencyObject> Stack;
        Predicate<DependencyObject> TreeStop;
        public VisualTreeEnumeratorWithConditionalStop(DependencyObject root, Predicate<DependencyObject> treeStop) {
            Stack = new Stack<DependencyObject>();
            Root = root;
            TreeStop = treeStop;
        }
        public void Dispose() {
            Reset();
            Stack = null;
        }
        #region IEnumerator Members
        DependencyObject current;
        object System.Collections.IEnumerator.Current {
            get { return current; }
        }
        public DependencyObject Current {
            get { return current; }
        }
        public bool MoveNext() {
            if(current == null) {
                current = Root;
            }
            else {
                int count = VisualTreeHelper.GetChildrenCount(current);
                DependencyObject[] children = new DependencyObject[count];
                for(int i = 0; i < count; i++) {
                    children[i] = VisualTreeHelper.GetChild(current, i);
                }
                if(children.Length > 0) {
                    for(int i = 0; i < children.Length; i++) {
                        DependencyObject child = children[(children.Length - 1) - i];
                        if(TreeStop != null && TreeStop(child)) continue;
                        Stack.Push(child);
                    }
                }
                current = Stack.Count > 0 ? Stack.Pop() : null;
            }
            return current != null;
        }
        public void Reset() {
            if(Stack != null)
                Stack.Clear();
            current = null;
        }
        #endregion
    }
#endif
    public class SingleObjectEnumerator : VisualTreeEnumerator {
        public SingleObjectEnumerator(DependencyObject dObject)
            : base(dObject) {
        }
        protected override IEnumerator GetNestedObjects(object obj) {
            return EmptyEnumerator;
        }
    }
}