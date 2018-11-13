using System.Collections.Generic;
using System.Linq;

namespace DevExpress.DXBinding.Native {
    class NRoot {
        public NBase Expr { get { return Exprs.First(); } }
        public List<NBase> Exprs { get; set; }
        public NRoot() {
            Exprs = new List<NBase>();
        }
    }

    abstract class NBase { }
    class NConstant : NBase {
        public enum NKind { Integer, Float, String, Boolean, Null }
        public object Value { get; set; }
        public NKind Kind { get; set; }
        public NConstant(NKind kind, object value) {
            Kind = kind;
            Value = value;
        }
    }
    class NUnaryBase : NBase {
        public NBase Value { get; set; }
        protected NUnaryBase(NBase value) {
            Value = value;
        }
    }
    class NUnary : NUnaryBase {
        public enum NKind { Plus, Minus, Not, NotBitwise }
        public NKind Kind { get; set; }
        public NUnary(NKind kind, NBase value) : base(value) {
            Kind = kind;
        }
    }
    class NCast : NUnaryBase {
        public enum NKind { Cast, Is, As, }
        public NKind Kind { get; set; }
        public NType Type { get; set; }
        public NCast(NKind kind, NBase value, NType type) : base(value) {
            Kind = kind;
            Type = type;
        }
    }
    class NBinary : NBase {
        public enum NKind {
            Mul, Div, Mod, Plus, Minus,
            ShiftLeft, ShiftRight,
            Less, Greater, LessOrEqual, GreaterOrEqual,

            And, Or, Xor,
            AndAlso, OrElse, Equal, NotEqual,
            Coalesce,
        }
        public NBase Left { get; set; }
        public NBase Right { get; set; }
        public NKind Kind { get; set; }
        public NBinary(NKind kind, NBase left, NBase right) {
            Kind = kind;
            Left = left;
            Right = right;
        }
    }
    class NTernary : NBase {
        public enum NKind { Condition, }
        public NBase First { get; set; }
        public NBase Second { get; set; }
        public NBase Third { get; set; }
        public NKind Kind { get; set; }
        public NTernary(NKind kind, NBase first, NBase second, NBase third) {
            Kind = kind;
            First = first;
            Second = second;
            Third = third;
        }
    }

    abstract class NIdentBase : NBase {
        public string Name { get; set; }
        public NIdentBase Next { get; set; }
        public NIdentBase(string name, NIdentBase next) {
            Name = name;
            Next = next;
        }
        public IEnumerable<NIdentBase> Unfold() {
            var res = new List<NIdentBase>();
            NIdentBase n = this;
            while (n != null) {
                res.Add(n);
                n = n.Next;
            }
            return res;
        }
    }
    class NExprIdent : NIdentBase {
        public NBase Expr { get; set; }
        public NExprIdent(NBase expr, NIdentBase next) : base("expr", next) {
            Expr = expr;
        }
    }
    class NIdent : NIdentBase {
        public NIdent(string name, NIdentBase next) : base(name, next) { }
    }
    class NMethod : NIdentBase {
        public NArgs Args { get; set; }
        public NMethod(string name, NIdentBase next, NArgs args = null) : base(name, next) {
            Args = args ?? new NArgs();
        }
    }
    class NType : NIdentBase {
        public static string[] PrimitiveTypes = new string[] { "sbyte", "byte", "short", "ushort", "int", "uint", "long", "ulong", "float", "double", "decimal", "bool", "object", "string" };
        public static bool IsPrimitiveType(string type) {
            return PrimitiveTypes.Contains(type);
        }

        public enum NKind { Type, TypeOf, Static, Attached }
        public NKind Kind { get; set; }
        public NIdentBase Ident { get; set; }
        public bool IsNullable { get; set; }
        public bool IsPrimitive { get { return IsPrimitiveType(Name); } }
        public NType(string name, NIdentBase next, NKind kind, NIdentBase ident) : base(name, next) {
            Kind = kind;
            Ident = ident;
        }
    }
    class NRelative : NIdentBase {
        public enum NKind {
            Context, Self, Parent, Element, Resource, Reference, Ancestor,
            Value,
            Parameter,
            Sender, Args
        }
        public NRelative(string name, NIdentBase next, NKind kind) : base(name, next) {
            Kind = kind;
        }
        public NKind Kind { get; set; }
        public string ElementName { get; set; }
        public string ResourceName { get; set; }
        public string ReferenceName { get; set; }
        public NType AncestorType { get; set; }
        public int? AncestorLevel { get; set; }
    }
    class NIndex : NIdentBase {
        public NArgs Args { get; set; }
        public NIndex(NIdentBase next, NArgs args = null) : base("Indexer", next) {
            Args = args ?? new NArgs();
        }
    }
    class NNew : NIdentBase {
        public NType Type { get; set; }
        public NArgs Args { get; set; }
        public NNew(NType type, NIdentBase next, NArgs args = null) : base(type.Name, next) {
            Type = type;
            Args = args ?? new NArgs();
        }
    }
    class NArgs : List<NBase> { }

    class NAssign : NBase {
        public NBase Left { get; set; }
        public NBase Expr { get; set; }
        public NAssign(NBase left, NBase expr) {
            Left = left;
            Expr = expr;
        }
    }
}