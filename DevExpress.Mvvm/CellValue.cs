using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DevExpress.Mvvm {
    public class CellValue {
        public CellValue(object row, string property, object value) {
            Value = value;
            Property = property;
            Row = row;
        }
        public object Row { get; private set; }
        public string Property { get; private set; }
        public object Value { get; private set; }
    }
}