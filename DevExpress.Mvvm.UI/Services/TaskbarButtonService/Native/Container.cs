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
    public struct Container<T> {
        public Container(T content)
            : this() {
            Content = content;
        }
        public T Content { get; private set; }
        #region Equality
        public override int GetHashCode() {
            return Content == null ? 0 : Content.GetHashCode();
        }
        public static bool operator ==(Container<T> t1, Container<T> t2) {
            return object.Equals(t1.Content, t2.Content);
        }
        public static bool operator !=(Container<T> t1, Container<T> t2) {
            return !(t1 == t2);
        }
        public override bool Equals(object obj) {
            return obj is Container<T> && this == (Container<T>)obj;
        }
        #endregion
    }
}