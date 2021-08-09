using DevExpress.Mvvm.Native;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Windows.Shell;

namespace DevExpress.Mvvm.UI.Native {
    public struct Container<T>  : IEquatable<Container<T>> {
        public Container(T content)
            : this() {
            Content = content;
        }
        public T Content { get; private set; }
        #region Equality
        public static bool operator ==(Container<T> t1, Container<T> t2) {
            return t1.Equals(t2);
        }
        public static bool operator !=(Container<T> t1, Container<T> t2) {
            return !t1.Equals(t2);
        }
        public bool Equals(Container<T> other) {
            return EqualityComparer<T>.Default.Equals(Content, other.Content);
        }
        public override bool Equals(object obj) {
            if (ReferenceEquals(null, obj)) return false;
            return obj is Container<T> && Equals((Container<T>)obj);
        }
        public override int GetHashCode() {
            return EqualityComparer<T>.Default.GetHashCode(Content);
        }
        #endregion
    }
}
