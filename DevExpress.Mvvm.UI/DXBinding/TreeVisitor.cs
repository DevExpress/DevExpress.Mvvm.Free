using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace DevExpress.DXBinding.Native {
    public class Operand {
        public enum RelativeSource { Context, Self, Parent, Element, Resource, Reference, Ancestor }
        public string Path { get; private set; }
        public RelativeSource Source { get; private set; }
        public string ElementName { get; private set; }
        public string ResourceName { get; private set; }
        public string ReferenceName { get; private set; }
        public Type AncestorType { get; private set; }
        public int AncestorLevel { get; private set; }

        public bool IsTwoWay { get; private set; }
        public Func<object[], object> BackConverter { get; private set; }

        public Operand(string path, RelativeSource source, string elementName, string resourceName, string referenceName, Type ancestorType, int ancestorLevel) {
            Path = path;
            Source = source;
            ElementName = elementName;
            ResourceName = resourceName;
            ReferenceName = referenceName;
            AncestorType = ancestorType;
            AncestorLevel = ancestorLevel;
            IsTwoWay = false;
        }
        internal static Operand CreateOperand(string path, NRelative n, Func<NType, Type> resolveType) {
            if(path == null && n == null) return null;
            return new Operand(
                path: path == null && n != null ? string.Empty : path,
                source: GetRelativeSource(n as NRelative),
                elementName: n != null ? n.ElementName : null,
                resourceName: n != null ? n.ResourceName : null,
                referenceName: n != null ? n.ReferenceName : null,
                ancestorType: GetAncestorType(n, resolveType),
                ancestorLevel: GetAncestorLevel(n));
        }
        static RelativeSource GetRelativeSource(NRelative n) {
            if(n == null) return RelativeSource.Context;
            switch(n.Kind) {
                case NRelative.NKind.Ancestor: return RelativeSource.Ancestor;
                case NRelative.NKind.Element: return RelativeSource.Element;
                case NRelative.NKind.Parent: return RelativeSource.Parent;
                case NRelative.NKind.Resource: return RelativeSource.Resource;
                case NRelative.NKind.Reference: return RelativeSource.Reference;
                case NRelative.NKind.Self: return RelativeSource.Self;
                case NRelative.NKind.Context: return RelativeSource.Context;
                default: throw new NotImplementedException();
            }
        }
        static Type GetAncestorType(NRelative n, Func<NType, Type> resolveType) {
            if(n == null || n.Kind != NRelative.NKind.Ancestor) return null;
            return resolveType(n.AncestorType);
        }
        static int GetAncestorLevel(NRelative n) {
            if(n == null || n.Kind != NRelative.NKind.Ancestor) return 0;
            return n.AncestorLevel ?? 1;
        }

        public override bool Equals(object obj) {
            Operand v = obj as Operand;
            if(v == null) return false;
            return Path == v.Path
                && Source == v.Source
                && ElementName == v.ElementName
                && ResourceName == v.ResourceName
                && ReferenceName == v.ReferenceName
                && AncestorType == v.AncestorType
                && AncestorLevel == v.AncestorLevel;
        }
        public override int GetHashCode() {
            return (Path != null ? Path.GetHashCode() : 7)
                ^ (ElementName != null ? ElementName.GetHashCode() : 13)
                ^ (ResourceName != null ? ResourceName.GetHashCode() : 17)
                ^ (ReferenceName != null ? ReferenceName.GetHashCode() : 23)
                ^ (AncestorType != null ? AncestorType.GetHashCode() : 27)
                ^ AncestorLevel.GetHashCode()
                ^ Source.GetHashCode();
        }
        public void SetMode(bool isTwoWay) {
            IsTwoWay = isTwoWay;
        }
        internal void SetBackConverter(Func<object[], object> backConverter) {
            BackConverter = backConverter;
        }
    }
    static class DynamicHelper {
        [ThreadStatic]
        static Dictionary<Type, Func<object, bool>> isOperators;
        static Dictionary<Type, Func<object, bool>> IsOperators { get { return isOperators ?? (isOperators = new Dictionary<Type, Func<object, bool>>()); } }
        [ThreadStatic]
        static Dictionary<Type, Func<object, object>> asOperators;
        static Dictionary<Type, Func<object, object>> AsOperators { get { return asOperators ?? (asOperators = new Dictionary<Type, Func<object, object>>()); } }
        [ThreadStatic]
        static Dictionary<Type, Func<object, object>> castOperators;
        static Dictionary<Type, Func<object, object>> CastOperators { get { return castOperators ?? (castOperators = new Dictionary<Type, Func<object, object>>()); } }

        static MethodInfo castMethod;
        static MethodInfo CastMethod { get { return castMethod ?? (castMethod = typeof(DynamicHelper).GetMethod("Cast", BindingFlags.Static | BindingFlags.NonPublic)); } }
        static object Cast<T>(object value) {
            if (value == null)
                return null;
            Type t = typeof(T);
            if (t.IsValueType) {
                Type underlyingType = Nullable.GetUnderlyingType(t);
                bool isNullable = underlyingType != null;
                if (isNullable)
                    return (T)Convert.ChangeType(value, underlyingType);
                return Convert.ChangeType(value, t);
            }
            return (T)value;
        }

        public static object Binary(NBinary.NKind kind, dynamic left, Func<dynamic> right) {
            switch (kind) {
                case NBinary.NKind.And:             return left & right();
                case NBinary.NKind.AndAlso:         return left && right();
                case NBinary.NKind.Coalesce:        return left ?? right();
                case NBinary.NKind.Div:             return left / right();
                case NBinary.NKind.Equal:           return Equal(left, right());
                case NBinary.NKind.Greater:         return left > right();
                case NBinary.NKind.GreaterOrEqual:  return left >= right();
                case NBinary.NKind.Less:            return left < right();
                case NBinary.NKind.LessOrEqual:     return left <= right();
                case NBinary.NKind.Minus:           return left - right();
                case NBinary.NKind.Mod:             return left % right();
                case NBinary.NKind.Mul:             return left * right();
                case NBinary.NKind.NotEqual:        return !Equal(left, right());
                case NBinary.NKind.Or:              return left | right();
                case NBinary.NKind.OrElse:          return left || right();
                case NBinary.NKind.Plus:            return left + right();
                case NBinary.NKind.ShiftLeft:       return left << right();
                case NBinary.NKind.ShiftRight:      return left >> right();
                case NBinary.NKind.Xor:             return left ^ right();
            }
            throw new NotImplementedException();
        }
        public static object Unary(NUnary.NKind kind, dynamic value) {
            switch (kind) {
                case NUnary.NKind.Minus: return -value;
                case NUnary.NKind.Plus: return +value;
                case NUnary.NKind.Not: return !value;
                case NUnary.NKind.NotBitwise: return ~value;
            }
            throw new NotImplementedException();
        }
        public static object Cast(NCast.NKind kind, object value, Type type) {
            if (kind == NCast.NKind.Is) {
                if (!IsOperators.ContainsKey(type)) {
                    var p = Expression.Parameter(typeof(object));
                    var expr = Expression.TypeIs(p, type);
                    var func = Expression.Lambda<Func<object, bool>>(expr, p).Compile();
                    IsOperators.Add(type, func);
                }
                return IsOperators[type](value);
            }
            if (kind == NCast.NKind.As) {
                if (!AsOperators.ContainsKey(type)) {
                    var p = Expression.Parameter(typeof(object));
                    var expr = Expression.TypeAs(p, type);
                    expr = Expression.Convert(expr, typeof(object));
                    var func = Expression.Lambda<Func<object, object>>(expr, p).Compile();
                    AsOperators.Add(type, func);
                }
                return AsOperators[type](value);
            }
            if (kind == NCast.NKind.Cast) {
                if (!CastOperators.ContainsKey(type)) {
                    var p = Expression.Parameter(typeof(object));
                    var expr = Expression.Call(null, CastMethod.MakeGenericMethod(type), p);
                    var func = Expression.Lambda<Func<object, object>>(expr, p).Compile();
                    CastOperators.Add(type, func);
                }
                return CastOperators[type](value);
            }
            throw new NotImplementedException();
        }

        static bool Equal(dynamic left, dynamic right) {
            if (left == null && right == null)
                return true;
            if (left == null || right == null)
                return false;
            Type leftT = left.GetType();
            Type rightT = right.GetType();
            if(ParserHelper.CastNumericTypes(ref leftT, ref rightT)) {
                object _left = Cast(NCast.NKind.Cast, left, leftT);
                object _right = Cast(NCast.NKind.Cast, right, rightT);
                return object.Equals(_left, _right);
            }
            return object.Equals(left, right);
        }
    }
    class MemberSearcher {
        public static readonly BindingFlags StaticBindingFlags = BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy;
        public static readonly BindingFlags InstanceBindingFlags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy;

        public static PropertyInfo FindInstanceProperty(Type instanceType, string propertyName) {
            Type t = instanceType;
            PropertyInfo res;
            while (t != null) {
                res = t.GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
                if (res != null)
                    return res;
                t = t.BaseType;
            }
            return null;
        }
        public static FieldInfo FindInstanceField(Type instanceType, string fieldName) {
            Type t = instanceType;
            FieldInfo res;
            while (t != null) {
                res = t.GetField(fieldName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
                if (res != null)
                    return res;
                t = t.BaseType;
            }
            return null;
        }
        public static PropertyInfo FindStaticProperty(Type instanceType, string propertyName) {
            return instanceType.GetProperty(propertyName, StaticBindingFlags);
        }
        public static FieldInfo FindStaticField(Type instanceType, string fieldName) {
            return instanceType.GetField(fieldName, StaticBindingFlags);
        }

        internal static PropertyInfo FindPropertyCore(Type instanceType, string propertyName, BindingFlags flags) {
            return instanceType.GetProperty(propertyName, flags);
        }
        internal static FieldInfo FindFieldCore(Type instanceType, string fieldName, BindingFlags flags) {
            return instanceType.GetField(fieldName, flags);
        }
        internal static bool IsImplicitConversion(Type from, Type to) {
            if (from == to)
                return true;
            if (from == null && !to.IsValueType)
                return true;
            var isShort = to == typeof(short);
            var isInt = to == typeof(int);
            var isLong = to == typeof(long);
            var isFloat = to == typeof(float);
            var isDouble = to == typeof(double);
            var isDecimal = to == typeof(decimal);
            var isUShort = to == typeof(ushort);
            var isUInt = to == typeof(uint);
            var isULong = to == typeof(ulong);
            if (from == typeof(sbyte) && (isShort || isInt || isLong || isFloat || isDouble || isDecimal))
                return true;
            if (from == typeof(byte) && (isShort || isUShort || isInt || isUInt || isLong || isULong || isFloat || isDouble || isDecimal))
                return true;
            if (from == typeof(short) && (isInt || isLong || isFloat || isDouble || isDecimal))
                return true;
            if (from == typeof(ushort) && (isInt || isUInt || isLong || isULong || isFloat || isDouble || isDecimal))
                return true;
            if (from == typeof(int) && (isLong || isFloat || isDouble || isDecimal))
                return true;
            if (from == typeof(uint) && (isLong || isULong || isFloat || isDouble || isDecimal))
                return true;
            if (from == typeof(long) && (isFloat || isDouble || isDecimal))
                return true;
            if (from == typeof(ulong) && (isFloat || isDouble || isDecimal))
                return true;
            if (from == typeof(char) && (isUShort || isInt || isUInt || isLong || isULong || isFloat || isDouble || isDecimal))
                return true;
            if (from == typeof(float) && isDouble)
                return true;
            return to.IsAssignableFrom(from);
        }
        static bool IsApplicableInNormalForm(Type[] args, ParameterInfo[] parameters) {
            if (args.Length > parameters.Length)
                return false;
            if (parameters.Length - args.Length > parameters.Count(x => x.IsOptional))
                return false;
            for (int i = 0; i < args.Length; ++i) {
                if (!IsImplicitConversion(args[i], parameters[i].ParameterType))
                    return false;
            }
            return true;
        }
        public static bool IsParams(ParameterInfo parameter) {
            return parameter.ParameterType.IsArray && parameter.GetCustomAttributes(true).Any(a => a is ParamArrayAttribute);
        }
        class Class<T> { public void m(T arg) { } };
        static ParameterInfo StripArray(ParameterInfo parameter) {
            var generic = typeof(Class<>).MakeGenericType(parameter.ParameterType.GetElementType());
            return generic.GetMethod("m").GetParameters().Single();
        }
        static ParameterInfo[] GetExpandedForm(Type[] args, ParameterInfo[] parameters) {
            if (!parameters.Any() || !IsParams(parameters.Last()))
                return null;
            return parameters.Take(parameters.Length - 1)
                .Concat(Enumerable.Range(0, Math.Max(0, args.Length - parameters.Length + 1)).Select(_ => StripArray(parameters.Last())))
                .ToArray();
        }
        static bool IsApplicableInExtendedForm(Type[] args, ParameterInfo[] parameters) {
            var expanded = GetExpandedForm(args, parameters) ?? parameters;
            return IsApplicableInNormalForm(args, expanded);
        }
        public static MethodBase[] GetApplicableFunctionMembers(IEnumerable<MethodBase> allMethods, Type[] args) {
            allMethods = allMethods.Where(x => !x.IsGenericMethodDefinition);
            allMethods = allMethods.Where(x => !x.GetParameters().Any(p => p.IsOut || p.IsRetval));
            allMethods = allMethods.Where(x => IsApplicableInNormalForm(args, x.GetParameters())
                                            || IsApplicableInExtendedForm(args, x.GetParameters()));
            return allMethods.ToArray();
        }
        static ParameterInfo[] GetPreferredForm(Type[] args, ParameterInfo[] parameters) {
            parameters = GetExpandedForm(args, parameters) ?? parameters;
            return parameters.Take(args.Length).ToArray();
        }
        static bool IsLeftBetterByTieBreakingRules(
            Type[] args,
            MethodBase leftMethod,
            ParameterInfo[] left,
            MethodBase rightMethod,
            ParameterInfo[] right) {
            if (!leftMethod.IsGenericMethod && rightMethod.IsGenericMethod)
                return true;
            if (GetExpandedForm(args, leftMethod.GetParameters()) == null
                && GetExpandedForm(args, rightMethod.GetParameters()) != null)
                return true;
            if (leftMethod.GetParameters().Length > rightMethod.GetParameters().Length)
                return true;
            if (leftMethod.GetParameters().Length == left.Length
                && rightMethod.GetParameters().Length > args.Length)
                return true;
            return false;
        }
        static bool IsLeftConversionBetter(Type arg, ParameterInfo left, ParameterInfo right) {
            if (arg == null)
                return false;
            if (arg == left.ParameterType && arg != right.ParameterType)
                return true;
            if (IsImplicitConversion(left.ParameterType, right.ParameterType)
                && !IsImplicitConversion(right.ParameterType, left.ParameterType))
                return true;
            if (left.ParameterType == typeof(sbyte)
                && new[] { typeof(byte), typeof(ushort), typeof(uint), typeof(ulong) }.Contains(right.ParameterType))
                return true;
            if (left.ParameterType == typeof(short)
                && new[] { typeof(ushort), typeof(uint), typeof(ulong) }.Contains(right.ParameterType))
                return true;
            if (left.ParameterType == typeof(int)
                && new[] { typeof(uint), typeof(ulong) }.Contains(right.ParameterType))
                return true;
            if (left.ParameterType == typeof(long) && right.ParameterType == typeof(ulong))
                return true;
            return false;
        }
        static bool IsLeftBetter(Type[] args, MethodBase leftMethod, MethodBase rightMethod) {
            var left = GetPreferredForm(args, leftMethod.GetParameters());
            var right = GetPreferredForm(args, rightMethod.GetParameters());
            if (left.Select(x => x.ParameterType).SequenceEqual(right.Select(x => x.ParameterType)))
                return IsLeftBetterByTieBreakingRules(args, leftMethod, left, rightMethod, right);
            for (int i = 0; i < args.Length; ++i) {
                if (IsLeftConversionBetter(args[i], right[i], left[i]))
                    return false;
            }
            for (int i = 0; i < args.Length; ++i) {
                if (IsLeftConversionBetter(args[i], left[i], right[i]))
                    return true;
            }
            return false;
        }
        public static MethodInfo FindMethod(IEnumerable<MethodInfo> allMethods, Type[] args, out Type[] outArgs) {
            return (MethodInfo)FindMethodBase(allMethods, args, out outArgs);
        }
        public static ConstructorInfo FindCtor(IEnumerable<ConstructorInfo> allMethods, Type[] args, out Type[] outArgs) {
            return (ConstructorInfo)FindMethodBase(allMethods, args, out outArgs);
        }
        public static MethodBase FindMethodBase(IEnumerable<MethodBase> allMethods, Type[] args, out Type[] outArgs) {
            var applicable = GetApplicableFunctionMembers(allMethods, args).ToList();
            outArgs = null;
            if (!applicable.Any()) {
                return null;
            }
            MethodBase best = null;
            if (applicable.Count() == 1) {
                best = applicable.First();
            } else {
                best = applicable.FirstOrDefault(x => applicable.Where(a => a != x).All(a => IsLeftBetter(args, x, a)));
                if (best == null)
                    return null;
            }
            outArgs = GetPreferredForm(args, best.GetParameters()).Select(x => x.ParameterType).ToArray();
            return best;
        }
    }

    abstract class VisitorBase<T> {
        protected IEnumerable<T> RootVisit(NRoot n) {
            return n.Exprs.Select(Visit).ToList();
        }
        protected virtual T Visit(NBase n) {
            if(!CanContinue(n)) return default(T);
            if(n is NIdentBase) return RootIdent((NIdentBase)n);
            if(n is NConstant) return Constant((NConstant)n);
            if(n is NBinary) return VisitBinary((NBinary)n);
            if(n is NUnary) return VisitUnary((NUnary)n);
            if(n is NCast) return VisitCast((NCast)n);
            if(n is NTernary) return VisitTernary((NTernary)n);
            if(n is NAssign) return VisitAssign((NAssign)n);
            throw new NotImplementedException();
        }
        protected virtual T VisitUnary(NUnary n) {
            T value = Visit(n.Value);
            if (!CanContinue(n)) return default(T);
            return Unary(n, value);
        }
        protected virtual T VisitBinary(NBinary n) {
            T left = Visit(n.Left);
            T right = Visit(n.Right);
            if (!CanContinue(n)) return default(T);
            return Binary(n, left, right);
        }
        protected virtual T VisitTernary(NTernary n) {
            T first = Visit(n.First);
            T second = Visit(n.Second);
            T third = Visit(n.Third);
            if (!CanContinue(n)) return default(T);
            return Ternary(n, first, second, third);
        }
        protected virtual T VisitCast(NCast n) {
            T value = Visit(n.Value);
            if (!CanContinue(n)) return default(T);
            return Cast(n, value);
        }
        protected virtual T VisitAssign(NAssign n) {
            T value = Visit(n.Expr);
            if (!CanContinue(n)) return default(T);
            return Assign(n, value);
        }

        protected virtual bool CanContinue(NBase n) {
            return true;
        }
        protected abstract T RootIdent(NIdentBase n);
        protected abstract T Constant(NConstant n);
        protected abstract T Binary(NBinary n, T left, T right);
        protected abstract T Unary(NUnary n, T value);
        protected abstract T Cast(NCast n, T value);
        protected abstract T Ternary(NTernary n, T first, T second, T third);
        protected abstract T Assign(NAssign n, T value);

        protected T RootIdentCore(T from, NIdentBase n) {
            if (!CanContinue(n)) return default(T);
            if (n is NIdent) return Ident(from, (NIdent)n);
            else if (n is NRelative) return Relative(from, (NRelative)n);
            else if (n is NMethod) {
                var args = Args(((NMethod)n).Args);
                if (!CanContinue(n)) return default(T);
                return Method(from, (NMethod)n, args);
            } else if (n is NType) return Type(from, (NType)n);
            else if (n is NExprIdent) return ExprIdent(from, (NExprIdent)n);
            else if (n is NIndex) {
                var args = Args(((NIndex)n).Args);
                if (!CanContinue(n)) return default(T);
                return Index(from, ((NIndex)n), args);
            } else if (n is NNew) {
                var args = Args(((NNew)n).Args);
                if (!CanContinue(n)) return default(T);
                return Type_New(from, ((NNew)n), args);
            } else throw new NotImplementedException();
        }
        protected virtual T ExprIdent(T from, NExprIdent n) {
            return Visit(n.Expr);
        }
        protected virtual T Index(T from, NIndex n, IEnumerable<T> indexArgs) { throw new NotImplementedException(); }
        protected virtual T Relative(T from, NRelative n) { throw new NotImplementedException(); }
        protected virtual T Ident(T from, NIdent n) { throw new NotImplementedException(); }
        protected virtual T Method(T from, NMethod n, IEnumerable<T> methodArgs) { throw new NotImplementedException(); }
        IEnumerable<T> Args(IEnumerable<NBase> args) {
            List<T> res = new List<T>();
            foreach (var arg in args)
                res.Add(Visit(arg));
            return res;
        }

        T Type(T from, NType n) {
            switch (n.Kind) {
                case NType.NKind.Type: return Type_Type(from, n);
                case NType.NKind.TypeOf: return Type_TypeOf(from, n);
                case NType.NKind.Static: return Type_Static(from, n);
                case NType.NKind.Attached: return Type_Attached(from, n);
                default: throw new NotImplementedException();
            }
        }
        protected virtual T Type_Type(T from, NType n) { throw new NotImplementedException(); }
        protected virtual T Type_TypeOf(T from, NType n) { throw new NotImplementedException(); }
        T Type_Static(T from, NType n) {
            if (n.Ident is NIdent) return Type_StaticIdent(from, n);
            else if (n.Ident is NMethod) return Type_StaticMethod(from, n, Args(((NMethod)n.Ident).Args));
            else throw new NotImplementedException();
        }
        protected virtual T Type_StaticIdent(T from, NType n) { throw new NotImplementedException(); }
        protected virtual T Type_StaticMethod(T from, NType n, IEnumerable<T> methodArgs) { throw new NotImplementedException(); }
        protected virtual T Type_Attached(T from, NType n) { throw new NotImplementedException(); }
        protected virtual T Type_New(T from, NNew n, IEnumerable<T> args) { throw new NotImplementedException(); }

        protected static IEnumerable<TEl> MakePlain<TEl>(IEnumerable<IEnumerable<TEl>> list) {
            if(list == null || list.Count() == 0 || list.Contains(null)) return new TEl[] { };
            return list.Aggregate((x, y) => x.Union(y)).ToList();
        }
    }
    class VisitorString : VisitorBase<string> {
        #region mapping
        static readonly Dictionary<NConstant.NKind, string> constantKindToStringMapping =
            new Dictionary<NConstant.NKind, string>() {
            { NConstant.NKind.Null,       "null" },
            { NConstant.NKind.Integer,    "{0}" },
            { NConstant.NKind.Float,      "{0}" },
            { NConstant.NKind.Boolean,    "{0}" },
            { NConstant.NKind.String,     "\"{0}\"" },
        };
        static readonly Dictionary<NBinary.NKind, string> binaryKindToStringMapping =
            new Dictionary<NBinary.NKind, string>() {
            { NBinary.NKind.Mul,          "{0}*{1}" },
            { NBinary.NKind.Div,          "{0}/{1}" },
            { NBinary.NKind.Mod,          "{0}%{1}" },
            { NBinary.NKind.Plus,         "{0}+{1}" },
            { NBinary.NKind.Minus,        "{0}-{1}" },

            { NBinary.NKind.And,          "{0}&{1}" },
            { NBinary.NKind.Or,           "{0}|{1}" },
            { NBinary.NKind.Xor,          "{0}^{1}" },
            { NBinary.NKind.ShiftLeft,    "{0}<<{1}" },
            { NBinary.NKind.ShiftRight,   "{0}>>{1}" },

            { NBinary.NKind.AndAlso,      "{0}&&{1}" },
            { NBinary.NKind.OrElse,       "{0}||{1}" },
            { NBinary.NKind.Equal,        "{0}=={1}" },
            { NBinary.NKind.NotEqual,     "{0}!={1}" },

            { NBinary.NKind.Coalesce,     "{0}??{1}" },
        };
        static readonly Dictionary<NUnary.NKind, string> unaryKindToStringMapping =
            new Dictionary<NUnary.NKind, string>() {
            { NUnary.NKind.Plus,          "{0}" },
            { NUnary.NKind.Minus,         "-{0}" },
            { NUnary.NKind.Not,           "!{0}" },
        };
        static readonly Dictionary<NCast.NKind, string> castKindToStringMapping =
            new Dictionary<NCast.NKind, string>() {
            { NCast.NKind.Cast,           "({1}){0}" },
            { NCast.NKind.Is,             "({0} is {1})" },
            { NCast.NKind.As,             "({0} as {1})" },
        };
        static readonly Dictionary<NTernary.NKind, string> ternaryKindToStringMapping =
            new Dictionary<NTernary.NKind, string>() {
            { NTernary.NKind.Condition,   "{0}?{1}:{2}" },
        };
        static readonly string identToString = "{0}";
        static readonly string nextIdentToString = ".{0}";
        static readonly string methodToString = "{0}({1})";
        static readonly string newToString = "new {0}({1})";
        static readonly string nextMethodArgToString = ",{0}";
        static readonly string exprIdentToString = "({0})";
        static readonly string indexToString = "[{0}]";
        static readonly Dictionary<NType.NKind, string> typeKindToStringMapping =
            new Dictionary<NType.NKind, string>() {
            { NType.NKind.Type,           "{0}" },
            { NType.NKind.TypeOf,         "typeof({0})" },
            { NType.NKind.Static,         "{0}.{1}" },
            { NType.NKind.Attached,       "({0}.{1})" },
            };
        static readonly string typeToString = "{0}";
        static readonly string typeNullableToString = "{0}?";
        static readonly Dictionary<NRelative.NKind, string> relativeKindToStringMapping =
            new Dictionary<NRelative.NKind, string>() {
            { NRelative.NKind.Context,    "" },
            { NRelative.NKind.Self,       "{0}" },
            { NRelative.NKind.Parent,     "{0}" },
            { NRelative.NKind.Resource,   "{0}({1})" },
            { NRelative.NKind.Element,    "{0}({1})" },
            { NRelative.NKind.Ancestor,   "{0}({1})" },
            { NRelative.NKind.Value,      "{0}" },
            { NRelative.NKind.Parameter,  "{0}" },
            { NRelative.NKind.Sender,     "{0}" },
            { NRelative.NKind.Args,       "{0}" },
        };
        static readonly string assignToString = "{0}={1}";
        static readonly string nextExprToString = ";{0}";
        #endregion
        #region Visitor
        protected override string RootIdent(NIdentBase n) {
            return CombineIdents(n.Unfold().Select(x => RootIdentCore(null, x)));
        }
        protected override string ExprIdent(string from, NExprIdent n) {
            return string.Format(exprIdentToString, base.ExprIdent(from, n));
        }
        protected override string Index(string from, NIndex n, IEnumerable<string> indexArgs) {
            return string.Format(indexToString, CombineArgs(indexArgs));
        }
        protected override string Relative(string from, NRelative n) {
            if(n.Kind == NRelative.NKind.Resource)
                return string.Format(relativeKindToStringMapping[n.Kind], n.Name, n.ResourceName);
            if(n.Kind == NRelative.NKind.Element)
                return string.Format(relativeKindToStringMapping[n.Kind], n.Name, n.ElementName);
            if(n.Kind == NRelative.NKind.Ancestor) {
                var args = n.AncestorLevel != null ?
                    new[] { Type(n.AncestorType), n.AncestorLevel.ToString() } :
                    new[] { Type(n.AncestorType) };
                return string.Format(relativeKindToStringMapping[n.Kind], n.Name, CombineArgs(args));
            }
            return string.Format(relativeKindToStringMapping[n.Kind], n.Name);
        }
        protected override string Ident(string from, NIdent n) {
            return string.Format(identToString, n.Name);
        }
        protected override string Method(string from, NMethod n, IEnumerable<string> methodArgs) {
            return string.Format(methodToString, n.Name, CombineArgs(methodArgs));
        }
        string Type(NType n) {
            string typeIdent = string.Empty;
            if(n.Ident != null) {
                typeIdent = RootIdentCore(null, n.Ident);
            }
            string type = n.IsNullable
                ? string.Format(typeNullableToString, n.Name)
                : string.Format(typeToString, n.Name);
            return string.Format(typeKindToStringMapping[n.Kind], type, typeIdent);
        }
        protected override string Type_Type(string from, NType n) {
            return Type(n);
        }
        protected override string Type_TypeOf(string from, NType n) {
            return Type(n);
        }
        protected override string Type_StaticIdent(string from, NType n) {
            return Type(n);
        }
        protected override string Type_StaticMethod(string from, NType n, IEnumerable<string> methodArgs) {
            return Type(n);
        }
        protected override string Type_Attached(string from, NType n) {
            return Type(n);
        }
        protected override string Type_New(string from, NNew n, IEnumerable<string> args) {
            return string.Format(newToString, n.Type.IsNullable ? n.Name + "?" : n.Name, CombineArgs(args));
        }

        protected override string Constant(NConstant n) {
            return string.Format(constantKindToStringMapping[n.Kind], n.Value != null ? n.Value.ToString() : null);
        }
        protected override string Binary(NBinary n, string left, string right) {
            return string.Format(binaryKindToStringMapping[n.Kind], left, right);
        }
        protected override string Unary(NUnary n, string value) {
            return string.Format(unaryKindToStringMapping[n.Kind], value);
        }
        protected override string Cast(NCast n, string value) {
            return string.Format(castKindToStringMapping[n.Kind], value, Visit(n.Type));
        }
        protected override string Ternary(NTernary n, string first, string second, string third) {
            return string.Format(ternaryKindToStringMapping[n.Kind], first, second, third);
        }

        protected override string Assign(NAssign n, string value) {
            return string.Format(assignToString, Visit(n.Left), Visit(n.Expr));
        }
        #endregion

        protected VisitorString() { }
        public static string Resolve(NRoot n) {
            return new VisitorString().RootVisit(n).Aggregate((x, y) => x + string.Format(nextExprToString, y));
        }
        public static string ResolveIdent(NIdentBase n, bool recursive) {
            VisitorString visitor = new VisitorString();
            return recursive ? visitor.RootIdent(n) : visitor.RootIdentCore(null, n);
        }
        public static string CombineIdents(IEnumerable<string> idents) {
            if(idents == null || idents.Count() == 0) return null;
            return idents.Aggregate((x, y) => x + string.Format(nextIdentToString, y));
        }
        public static string CombineArgs(IEnumerable<string> args) {
            if(args == null || args.Count() == 0) return null;
            return args.Aggregate((x, y) => x + string.Format(nextMethodArgToString, y));
        }
    }
    class VisitorType : VisitorBase<IEnumerable<VisitorType.TypeInfo>> {
        #region mapping
        static readonly Dictionary<string, Type> primitiveTypeMapping =
            new Dictionary<string, Type>() {
            { "sbyte",                    typeof(sbyte) },
            { "byte",                     typeof(byte) },
            { "short",                    typeof(short) },
            { "ushort",                   typeof(ushort) },
            { "int",                      typeof(int) },
            { "uint",                     typeof(uint) },
            { "long",                     typeof(long) },
            { "ulong",                    typeof(ulong) },
            { "float",                    typeof(float) },
            { "double",                   typeof(double) },
            { "decimal",                  typeof(decimal) },
            { "bool",                     typeof(bool) },
            { "object",                   typeof(object) },
            { "string",                   typeof(string) },
        };
        #endregion
        #region TypeInfo
        public struct TypeInfo {
            public readonly string Name;
            public readonly bool IsNullable;
            public TypeInfo(string name, bool isNullable) {
                Name = name;
                IsNullable = isNullable;
            }
            public TypeInfo(NType type) {
                Name = type.Name;
                IsNullable = type.IsNullable;
            }
        }
        #endregion
        #region Visitor
        protected override IEnumerable<TypeInfo> RootIdent(NIdentBase n) {
            IEnumerable<TypeInfo> res = new TypeInfo[] { };
            NIdentBase rest;
            VisitorOperand.ReduceIdent(n, x => {
                res = res.Union(Type_Type(null, x));
                return typeof(object);
            }, out rest, fullPack);
            n = rest;
            while(n != null) {
                if(!CanContinue(n)) return new TypeInfo[] { };
                res = res.Union(RootIdentCore(res, n));
                n = n.Next;
            }
            return res;
        }
        protected override IEnumerable<TypeInfo> Index(IEnumerable<TypeInfo> from, NIndex n, IEnumerable<IEnumerable<TypeInfo>> indexArgs) {
            return MakePlain(indexArgs);
        }
        protected override IEnumerable<TypeInfo> Relative(IEnumerable<TypeInfo> from, NRelative n) {
            if(n.Kind == NRelative.NKind.Ancestor)
                return Type_Type(from, n.AncestorType);
            return new TypeInfo[] { };
        }
        protected override IEnumerable<TypeInfo> Ident(IEnumerable<TypeInfo> from, NIdent n) {
            return new TypeInfo[] { };
        }
        protected override IEnumerable<TypeInfo> Method(IEnumerable<TypeInfo> from, NMethod n, IEnumerable<IEnumerable<TypeInfo>> methodArgs) {
            return MakePlain(methodArgs);
        }
        protected override IEnumerable<TypeInfo> Type_Type(IEnumerable<TypeInfo> from, NType n) {
            return new TypeInfo[] { new TypeInfo(n) };
        }
        protected override IEnumerable<TypeInfo> Type_TypeOf(IEnumerable<TypeInfo> from, NType n) {
            return Type_Type(from, n);
        }
        protected override IEnumerable<TypeInfo> Type_StaticIdent(IEnumerable<TypeInfo> from, NType n) {
            return Type_Type(from, n);
        }
        protected override IEnumerable<TypeInfo> Type_StaticMethod(IEnumerable<TypeInfo> from, NType n, IEnumerable<IEnumerable<TypeInfo>> methodArgs) {
            return Type_Type(from, n).Union(MakePlain(methodArgs));
        }
        protected override IEnumerable<TypeInfo> Type_Attached(IEnumerable<TypeInfo> from, NType n) {
            return Type_Type(from, n);
        }
        protected override IEnumerable<TypeInfo> Type_New(IEnumerable<TypeInfo> from, NNew n, IEnumerable<IEnumerable<TypeInfo>> args) {
            return Type_Type(from, n.Type).Union(MakePlain(args));
        }

        protected override IEnumerable<TypeInfo> Constant(NConstant n) {
            return new TypeInfo[] { };
        }
        protected override IEnumerable<TypeInfo> Binary(NBinary n, IEnumerable<TypeInfo> left, IEnumerable<TypeInfo> right) {
            return left.Union(right);
        }
        protected override IEnumerable<TypeInfo> Unary(NUnary n, IEnumerable<TypeInfo> value) {
            return value;
        }
        protected override IEnumerable<TypeInfo> Cast(NCast n, IEnumerable<TypeInfo> value) {
            return value.Union(Type_Type(null, n.Type));
        }
        protected override IEnumerable<TypeInfo> Ternary(NTernary n, IEnumerable<TypeInfo> first, IEnumerable<TypeInfo> second, IEnumerable<TypeInfo> third) {
            return first.Union(second).Union(third);
        }

        protected override IEnumerable<TypeInfo> Assign(NAssign n, IEnumerable<TypeInfo> value) {
            return Visit(n.Left).Union(value);
        }
        #endregion

        bool fullPack;
        public static IEnumerable<TypeInfo> Resolve(NRoot expr, NRoot expr2, bool fullPack = true) {
            VisitorType me = new VisitorType();
            me.fullPack = fullPack;
            var res = MakePlain(me.RootVisit(expr));
            if(expr2 != null) {
                me.fullPack = true;
                var backRes = MakePlain(me.RootVisit(expr2));
                res = res.Union(backRes);
            }
            return res;
        }
        public static Type ResolveType(TypeInfo type, ITypeResolver typeResolver, IErrorHandler errorHandler) {
            Type res = NType.IsPrimitiveType(type.Name) ? primitiveTypeMapping[type.Name] : typeResolver.ResolveType(type.Name);
            if(!type.IsNullable) return res;
            if(Nullable.GetUnderlyingType(res) != null) {
                errorHandler.Throw(ErrorHelper.Err004(type.Name), null);
                return null;
            }
            return typeof(Nullable<>).MakeGenericType(res);
        }
    }
    class VisitorOperand : VisitorBase<IEnumerable<Operand>> {
        protected override IEnumerable<Operand> Constant(NConstant n) {
            return new Operand[] { };
        }
        protected override IEnumerable<Operand> Binary(NBinary n, IEnumerable<Operand> left, IEnumerable<Operand> right) {
            return left.Union(right);
        }
        protected override IEnumerable<Operand> Unary(NUnary n, IEnumerable<Operand> value) {
            return value;
        }
        protected override IEnumerable<Operand> Cast(NCast n, IEnumerable<Operand> value) {
            return value;
        }
        protected override IEnumerable<Operand> Ternary(NTernary n, IEnumerable<Operand> first, IEnumerable<Operand> second, IEnumerable<Operand> third) {
            return first.Union(second).Union(third);
        }
        protected override IEnumerable<Operand> Assign(NAssign n, IEnumerable<Operand> value) {
            return Visit(n.Left).Union(value);
        }
        protected override IEnumerable<Operand> RootIdent(NIdentBase n) {
            if (this.fullPack)
                return FullPack_RootIdent(n);
            else return NotFullPack_RootIdent(n);
        }
        IEnumerable<Operand> FullPack_RootIdent(NIdentBase n) {
            IEnumerable<Operand> res = new Operand[] { };
            NIdentBase rest;
            Operand op = ReduceIdent(n, typeResolver, out rest);
            if (op != null) res = new[] { op };
            if (rest == null) return res;
            rest.Unfold().OfType<NIdentBase>()
                .Where(x => x is NExprIdent || x is NMethod || x is NIndex || x is NType || x is NNew)
                .Select<NIdentBase, IEnumerable<NBase>>(x => {
                    if (x is NExprIdent) return new NBase[] { ((NExprIdent)x).Expr };
                    else if (x is NMethod) return ((NMethod)x).Args;
                    else if (x is NIndex) return ((NIndex)x).Args;
                    else if (x is NType) {
                        if (((NType)x).Ident is NMethod)
                            return ((NMethod)((NType)x).Ident).Args;
                        else return new NBase[] { };
                    } else if (x is NNew) {
                        return ((NNew)x).Args;
                    }
                    throw new NotImplementedException();
                }).SelectMany(x => x).ToList().ForEach(x => res = res.Union(Visit(x)));
            return res.ToList();
        }
        IEnumerable<Operand> NotFullPack_RootIdent(NIdentBase n) {
            IEnumerable<Operand> res = new Operand[] { };
            NRelative nr = n as NRelative;
            if (nr != null) {
                if (!(nr.Kind == NRelative.NKind.Value
                    || nr.Kind == NRelative.NKind.Parameter
                    || nr.Kind == NRelative.NKind.Sender
                    || nr.Kind == NRelative.NKind.Args)) {
                    res = new[] { Operand.CreateOperand(string.Empty, nr, typeResolver) };
                }
            } else {
                if (n is NIdent || n is NMethod || n is NIndex) {
                    res = new[] { Operand.CreateOperand(string.Empty, null, typeResolver) };
                }
            }

            n.Unfold().OfType<NIdentBase>()
                   .Where(x => x is NExprIdent || x is NMethod || x is NIndex || x is NType || x is NNew)
                   .Select<NIdentBase, IEnumerable<NBase>>(x => {
                       if (x is NExprIdent) return new NBase[] { ((NExprIdent)x).Expr };
                       else if (x is NMethod) return ((NMethod)x).Args;
                       else if (x is NIndex) return ((NIndex)x).Args;
                       else if (x is NType) {
                           if (((NType)x).Ident is NMethod)
                               return ((NMethod)((NType)x).Ident).Args;
                           else return new NBase[] { };
                       } else if (x is NNew) {
                           return ((NNew)x).Args;
                       }
                       throw new NotImplementedException();
                   }).SelectMany(x => x).ToList().ForEach(x => res = res.Union(Visit(x)));
            return res.ToList();
        }

        void BackExpr(NBase n) {
            if(operands.Count() == 0) {
                errorHandler.Throw(ErrorHelper.Err102(), null);
                return;
            }
            if(operands.Count() > 1) {
                errorHandler.Throw(ErrorHelper.Err103(), null);
                return;
            }
            operands[0].SetMode(true);
            return;
        }
        void BackAssigns(IEnumerable<NAssign> assigns) {
            IEnumerable<Operand> backOperands = new Operand[] { };
            assigns
                .Select(x => x.Left).Select(Visit)
                .ToList()
                .ForEach(x => backOperands = backOperands.Union(x));
            backOperands.ToList().ForEach(x => x.SetMode(true));
            foreach(var op in backOperands) {
                if(operands.Contains(op))
                    operands[operands.IndexOf(op)].SetMode(true);
                else operands.Add(op);
            }
        }
        void RootVisitBack(NRoot backExpr) {
            if(errorHandler.HasError) return;
            if(backExpr.Exprs.Count() == 1 && !(backExpr.Expr is NAssign)) {
                BackExpr(backExpr.Expr);
                return;
            }
            BackAssigns(backExpr.Exprs.Cast<NAssign>());
        }
        protected override bool CanContinue(NBase n) {
            return !errorHandler.HasError;
        }

        readonly bool fullPack;
        readonly IErrorHandler errorHandler;
        readonly Func<NType, Type> typeResolver;
        List<Operand> operands;
        protected VisitorOperand(Func<NType, Type> typeResolver, IErrorHandler errorHandler, bool fullPack) {
            this.fullPack = fullPack;
            this.typeResolver = typeResolver;
            this.errorHandler = errorHandler;
            this.operands = new List<Operand>();
        }
        public static IEnumerable<Operand> Resolve(IEnumerable<NRoot> exprs, NRoot backExpr, Func<NType, Type> typeResolver, IErrorHandler errorHandler, bool fullPack = true) {
            VisitorOperand me = new VisitorOperand(typeResolver, errorHandler, fullPack);
            var res = MakePlain(exprs.Select(x => MakePlain(me.RootVisit(x))));
            me.operands = new List<Operand>(res);
            if (backExpr != null)
                me.RootVisitBack(backExpr);
            return me.operands;
        }

        public static Operand ReduceIdent(NIdentBase n, Func<NType, Type> typeResolver, out NIdentBase rest, bool fullPack = true) {
            if (fullPack)
                return FullPack_ReduceIdent(n, typeResolver, out rest);
            else return NotFullPack_ReduceIdent(n, typeResolver, out rest);
        }
        static Operand FullPack_ReduceIdent(NIdentBase n, Func<NType, Type> typeResolver, out NIdentBase rest) {
            if ((n is NNew) || (n is NExprIdent) || (n is NRelative &&
                (((NRelative)n).Kind == NRelative.NKind.Value
                || ((NRelative)n).Kind == NRelative.NKind.Parameter
                || ((NRelative)n).Kind == NRelative.NKind.Sender
                || ((NRelative)n).Kind == NRelative.NKind.Args))) {
                rest = n;
                return null;
            }
            rest = n is NRelative ? n.Next : n;
            List<string> idents = new List<string>();
            while (rest != null) {
                if (rest is NIdent || (rest is NType && ((NType)rest).Kind == NType.NKind.Attached))
                    idents.Add(VisitorString.ResolveIdent(rest, false));
                else break;
                rest = rest.Next;
            }
            var path = VisitorString.CombineIdents(idents);
            if (path == null && n is NMethod) path = string.Empty;
            return Operand.CreateOperand(path, n as NRelative, typeResolver);
        }
        static Operand NotFullPack_ReduceIdent(NIdentBase n, Func<NType, Type> typeResolver, out NIdentBase rest) {
            if ((n is NNew) || (n is NExprIdent) || (n is NRelative &&
                (((NRelative)n).Kind == NRelative.NKind.Value
                || ((NRelative)n).Kind == NRelative.NKind.Parameter
                || ((NRelative)n).Kind == NRelative.NKind.Sender
                || ((NRelative)n).Kind == NRelative.NKind.Args))) {
                rest = n;
                return null;
            }
            rest = n is NRelative ? n.Next : n;
            if (n is NType || n is NNew || n is NExprIdent)
                return Operand.CreateOperand(null, null, typeResolver);
            return Operand.CreateOperand(string.Empty, n as NRelative, typeResolver);
        }
    }

    abstract class VisitorEvaluatorBase : VisitorBase<object> {
        protected object LastMember { get; private set; }
        protected object LastMemberOwner { get; private set; }
        protected bool IsLastMember { get; private set; }
        void SetLastMember(object lastMember, object lastMemberOwner) {
            this.LastMember = lastMember;
            this.LastMemberOwner = lastMemberOwner;
            IsLastMember = true;
        }

        protected override object Visit(NBase n) {
            IsLastMember = false;
            return base.Visit(n);
        }
        protected override object VisitTernary(NTernary n) {
            object first = Visit(n.First);
            if(object.Equals(first, true))
                return Visit(n.Second);
            if(object.Equals(first, false))
                return Visit(n.Third);
            errorHandler.Report(string.Format("Cannot convert '{0}' to boolean.", first), true);
            return null;
        }
        protected override object VisitBinary(NBinary n) {
            return Try(() => DynamicHelper.Binary(n.Kind, Visit(n.Left), () => Visit(n.Right)));
        }
        protected override object Ternary(NTernary n, object first, object second, object third) {
            throw new NotImplementedException();
        }
        protected override object Binary(NBinary n, object left, object right) {
            throw new NotImplementedException();
        }
        protected override object Unary(NUnary n, object value) {
            return Try(() => DynamicHelper.Unary(n.Kind, value));
        }
        protected override object Cast(NCast n, object value) {
            return Try(() => DynamicHelper.Cast(n.Kind, value, typeResolver(n.Type)));
        }
        protected override object Constant(NConstant n) {
            return n.Value;
        }

        protected override object RootIdent(NIdentBase n) {
            object res;
            NIdentBase rest;
            Operand operand = VisitorOperand.ReduceIdent(n, x => typeResolver(x), out rest, fullPack);
            if (operand == null && rest is NRelative) {
                res = Relative(null, (NRelative)rest);
                rest = rest.Next;
            } else
                res = GetOperandValue(operand, null);
            n = rest;
            while (n != null) {
                if (!CanContinue(n)) return null;
                res = RootIdentCore(res, n);
                operand = null;
                n = n.Next;
            }
            return res;
        }
        protected override object Ident(object from, NIdent n) {
            if (IsNull(from))
                return null;
            var fromT = from.GetType();
            var prop = MemberSearcher.FindInstanceProperty(fromT, n.Name);
            if (prop != null) {
                SetLastMember(prop, from);
                return prop.GetValue(from, null);
            }
            var field = MemberSearcher.FindInstanceField(fromT, n.Name);
            if (field != null) {
                SetLastMember(field, from);
                return field.GetValue(from);
            }
            errorHandler.Report(ErrorHelper.Report001(n.Name, from.GetType()), true);
            return null;
        }
        protected override object Method(object from, NMethod n, IEnumerable<object> methodArgs) {
            if (IsNull(from)) return null;
            var fromT = from.GetType();
            string methodName = n.Name;
            var allMethods = fromT.GetMethods(MemberSearcher.InstanceBindingFlags)
                .Where(x => x.Name == methodName)
                .ToArray();
            return InvokeMethod(allMethods, methodArgs, methodName, from, fromT);
        }
        protected override object Index(object from, NIndex n, IEnumerable<object> indexArgs) {
            if (IsNull(from)) return null;
            var fromT = from.GetType();
            if (fromT.IsArray)
                return Try(() => ((Array)from).GetValue(indexArgs.Cast<int>().ToArray()));
            var allMethods = fromT.GetProperties(MemberSearcher.InstanceBindingFlags)
                .Where(x => x.GetIndexParameters().Any())
                .Select(x => x.GetGetMethod())
                .ToArray();
            return InvokeMethod(allMethods, indexArgs, "Indexer", from, fromT);
        }
        protected override object Relative(object from, NRelative n) {
            switch (n.Kind) {
                case NRelative.NKind.Parameter:
                case NRelative.NKind.Value:
                case NRelative.NKind.Sender:
                case NRelative.NKind.Args: return GetOperandValue(null, n.Kind);
                default: throw new NotImplementedException();
            }
        }

        protected override object Type_Type(object from, NType n) {
            throw new NotImplementedException();
        }
        protected override object Type_TypeOf(object from, NType n) {
            return typeResolver(n);
        }
        protected override object Type_StaticIdent(object from, NType n) {
            Type t = typeResolver(n);
            var prop = MemberSearcher.FindStaticProperty(t, n.Ident.Name);
            if (prop != null) {
                SetLastMember(prop, null);
                return prop.GetValue(null, null);
            }
            var field = MemberSearcher.FindStaticField(t, n.Ident.Name);
            if (field != null) {
                SetLastMember(field, null);
                return field.GetValue(null);
            }
            errorHandler.Report(ErrorHelper.Report001(n.Ident.Name, t), true);
            return null;
        }
        protected override object Type_StaticMethod(object from, NType n, IEnumerable<object> methodArgs) {
            Type t = typeResolver(n);
            string methodName = n.Ident.Name;
            var allMethods = t.GetMethods(MemberSearcher.StaticBindingFlags)
              .Where(x => x.Name == methodName)
              .ToArray();
            return InvokeMethod(allMethods, methodArgs, methodName, null, t);
        }
        protected override object Type_Attached(object from, NType n) {
            if (IsNull(from)) return null;
            Type t = typeResolver(n);
            var field = MemberSearcher.FindStaticField(t, n.Ident.Name + "Property");
            if (field == null) {
                errorHandler.Report(ErrorHelper.Report001(n.Ident.Name + "Property", t), true);
                return null;
            }
            var dp = (System.Windows.DependencyProperty)field.GetValue(null);
            if (dp == null) {
                errorHandler.Report(ErrorHelper.Report001(n.Ident.Name + "Property", t), true);
                return null;
            }
            SetLastMember(dp, from);
            return Try(() => ((System.Windows.DependencyObject)from).GetValue(dp));
        }
        protected override object Type_New(object from, NNew n, IEnumerable<object> args) {
            Type t = typeResolver(n.Type);
            if (t.IsPrimitive)
                return Try(() => Activator.CreateInstance(t, args.ToArray()));
            var ctors = t.GetConstructors();
            ConstructorInfo method;
            IEnumerable<object> outMethodArgs;
            GetInvocation<ConstructorInfo>(ctors, args, out method, out outMethodArgs);
            if (method == null) {
                errorHandler.Report(ErrorHelper.Report002("Ctor", GetArgsTypes(args), t), true);
                return null;
            }
            return Try(() => method.Invoke(outMethodArgs.ToArray()));
        }
        object InvokeMethod(IEnumerable<MethodInfo> methods, IEnumerable<object> methodArgs, string methodName, object from, Type fromT) {
            MethodInfo method;
            IEnumerable<object> outMethodArgs;
            GetInvocation<MethodInfo>(methods, methodArgs, out method, out outMethodArgs);
            if (method == null) {
                errorHandler.Report(ErrorHelper.Report002(methodName, GetArgsTypes(methodArgs), fromT), true);
                return null;
            }
            return Try(() => method.Invoke(from, outMethodArgs.ToArray()));
        }

        static Type[] GetArgsTypes(IEnumerable<object> args) {
            return args.Select(x => {
                if (x == null) return null;
                return x.GetType();
            }).ToArray();
        }
        static void GetInvocation<T>(IEnumerable<T> methods, IEnumerable<object> args, out T method, out IEnumerable<object> outArgs)
            where T : MethodBase {

            var argsTypes = GetArgsTypes(args);
            Type[] outArgsTypes;
            outArgs = new List<object>(args);
            method = (T)MemberSearcher.FindMethodBase(methods, argsTypes, out outArgsTypes);
            if (method == null)
                return;

            var parameters = method.GetParameters();
            if (parameters.Any() && MemberSearcher.IsParams(parameters.Last())) {
                int toPack = args.Count() - parameters.Length + 1;
                var paramsArray = outArgs.Skip(outArgs.Count() - toPack).ToArray();
                outArgs = outArgs.Take(args.Count() - toPack).ToList();
                Type arrayType = parameters.Last().ParameterType.GetElementType();
                paramsArray = paramsArray.Select(x => DynamicHelper.Cast(NCast.NKind.Cast, x, arrayType)).ToArray();
                Array typedParamsArray = Array.CreateInstance(arrayType, paramsArray.Length);
                Array.Copy(paramsArray, typedParamsArray, paramsArray.Length);
                ((List<object>)outArgs).Add(typedParamsArray);
            }
            if (parameters.Length > args.Count()) {
                for (int i = args.Count(); i < parameters.Length; ++i) {
                    ((List<object>)outArgs).Add(parameters[i].DefaultValue);
                }
            }
        }

        bool IsNull(object obj) {
            if (obj == null) {
                errorHandler.SetError();
                return true;
            }
            return false;
        }
        object Try(Func<object> func) {
            try {
                return func();
            } catch (Exception e) {
                errorHandler.ReportOrThrow(e);
                return null;
            }
        }
        protected override bool CanContinue(NBase n) {
            if (errorHandler.HasError)
                return false;
            return true;
        }

        readonly bool fullPack;
        protected readonly Func<NType, Type> typeResolver;
        protected readonly IErrorHandler errorHandler;
        protected VisitorEvaluatorBase(Func<NType, Type> typeResolver, IErrorHandler errorHandler, bool fullPack) {
            this.fullPack = fullPack;
            this.typeResolver = typeResolver;
            this.errorHandler = errorHandler;
        }

        protected abstract object GetOperandValue(Operand operand, NRelative.NKind? relativeSource);
    }
    class VisitorEvaluator : VisitorEvaluatorBase {
        readonly IDictionary<Operand, object> operandValues;
        readonly bool executeAssign;
        object backParam;
        object parameterParam;
        object senderParam;
        object argsParam;
        protected VisitorEvaluator(IDictionary<Operand, object> operandValues, Func<NType, Type> typeResolver, IErrorHandler errorHandler, bool fullPack, bool executeAssign)
            : base(typeResolver, errorHandler, fullPack) {
            this.executeAssign = executeAssign;
            this.operandValues = operandValues;
            this.assigns = new Dictionary<Operand, object>();
        }
        protected override object GetOperandValue(Operand operand, NRelative.NKind? relativeSource) {
            if (relativeSource.HasValue) {
                switch (relativeSource.Value) {
                    case NRelative.NKind.Value: return backParam;
                    case NRelative.NKind.Parameter: return parameterParam;
                    case NRelative.NKind.Sender: return senderParam;
                    case NRelative.NKind.Args: return argsParam;
                    default: throw new NotImplementedException();
                }
            }
            if (operand != null) {
                if (operandValues.ContainsKey(operand))
                    return operandValues[operand];
                errorHandler.SetError();
            }
            return null;
        }

        Dictionary<Operand, object> assigns;
        protected override object Assign(NAssign n, object value) {
            return executeAssign ? Assign_Execute(n, value) : Assign_NotExecute(n, value);
        }
        object Assign_NotExecute(NAssign n, object value) {
            NIdentBase rest;
            if (!(n.Left is NIdentBase))
                throw new InvalidOperationException();
            var op = VisitorOperand.ReduceIdent((NIdentBase)n.Left, x => typeResolver(x), out rest);
            assigns.Add(op, value);
            return null;
        }
        object Assign_Execute(NAssign n, object value) {
            Visit(n.Left);
            if(!IsLastMember) {
                errorHandler.Report("Assign Expression is invalid.", true);
                return null;
            }
            object lastMember = LastMember;
            object lastMemberOwner = LastMemberOwner;
            if(lastMember is System.Windows.DependencyProperty) {
                ((System.Windows.DependencyObject)LastMemberOwner).SetCurrentValue((System.Windows.DependencyProperty)lastMember, value);
                return null;
            }
            if(lastMember is PropertyInfo) {
                ((PropertyInfo)lastMember).SetValue(LastMemberOwner, value, null);
                return null;
            }
            if(lastMember is FieldInfo) {
                ((FieldInfo)lastMember).SetValue(LastMemberOwner, value);
                return null;
            }
            errorHandler.Report("Assign Expression is invalid.", true);
            return null;
        }

        public static IEnumerable<object> Resolve(NRoot expr, IDictionary<Operand, object> operandValues, Func<NType, Type> typeResolver, IErrorHandler errorHandler) {
            VisitorEvaluator me = new VisitorEvaluator(operandValues, typeResolver, errorHandler, true, false);
            return me.RootVisit(expr);
        }
        public static IEnumerable<object> ResolveBack(NRoot expr, object backParam, List<Operand> operands, IDictionary<Operand, object> operandValues, Func<NType, Type> typeResolver, IErrorHandler errorHandler) {
            VisitorEvaluator me = new VisitorEvaluator(operandValues, typeResolver, errorHandler, true, false);
            me.backParam = backParam;
            var res = me.RootVisit(expr);
            if (res.Count() == 1 && res.First() != null)
                return res;
            var result =  operands.Select(x => {
                object xx;
                me.assigns.TryGetValue(x, out xx);
                return xx;
            }).ToArray();
            return result;
        }
        public static void ResolveExecute(NRoot expr, object parameterParam, IDictionary<Operand, object> operandValues, Func<NType, Type> typeResolver, IErrorHandler errorHandler) {
            VisitorEvaluator me = new VisitorEvaluator(operandValues, typeResolver, errorHandler, false, true);
            me.parameterParam = parameterParam;
            me.RootVisit(expr);
        }
        public static bool ResolveCanExecute(NRoot expr, object parameterParam, IDictionary<Operand, object> operandValues, Func<NType, Type> typeResolver, IErrorHandler errorHandler) {
            VisitorEvaluator me = new VisitorEvaluator(operandValues, typeResolver, errorHandler, true, false);
            me.parameterParam = parameterParam;
            var res = me.RootVisit(expr);
            return object.Equals(res.FirstOrDefault(), true);
        }
        public static void ResolveEvent(NRoot expr, object sender, object args, IDictionary<Operand, object> operandValues, Func<NType, Type> typeResolver, IErrorHandler errorHandler) {
            VisitorEvaluator me = new VisitorEvaluator(operandValues, typeResolver, errorHandler, false, true);
            me.senderParam = sender;
            me.argsParam = args;
            me.RootVisit(expr);
        }
    }

    public interface IBindingCalculator {
        IEnumerable<Operand> Operands { get; }
        void Init(ITypeResolver typeResolver);
        object Resolve(object[] values);
    }
    public interface ICommandCalculator {
        IEnumerable<Operand> Operands { get; }
        void Init(ITypeResolver typeResolver);
        void Execute(object[] opValues, object parameter);
        bool CanExecute(object[] opValues, object parameter);
    }
    public interface IEventCalculator {
        IEnumerable<Operand> Operands { get; }
        void Init(ITypeResolver typeResolver);
        void Event(object[] opValues, object sender, object args);
    }

    public abstract class CalculatorDynamicBase {
        public IEnumerable<Operand> Operands { get; private set; }
        internal Dictionary<VisitorType.TypeInfo, Type> TypeInfos { get; private set; }
        internal IErrorHandler ErrorHandler { get; private set; }
        internal ITypeResolver TypeResolver { get; private set; }

        internal CalculatorDynamicBase(IErrorHandler errorHandler) {
            ErrorHandler = errorHandler;
            Operands = new List<Operand>();
        }
        public virtual void Init(ITypeResolver typeResolver) {
            TypeResolver = typeResolver;
            TypeInfos = new Dictionary<VisitorType.TypeInfo, Type>();
            foreach (var typeInfo in GetTypeInfos())
                TypeInfos.Add(typeInfo, VisitorType.ResolveType(typeInfo, TypeResolver, ErrorHandler));
            Operands = GetOperands().ToList();
        }
        internal abstract IEnumerable<VisitorType.TypeInfo> GetTypeInfos();
        internal abstract IEnumerable<Operand> GetOperands();
        internal Type GetResolvedType(NType n) {
            return TypeInfos[new VisitorType.TypeInfo(n)];
        }
        internal IDictionary<Operand, object> GetOperandsValues(object[] values) {
            var _values = values != null ? values.ToList() : new List<object>();
            return Operands
                .Select((x, i) => new { x = x, i = i })
                .ToDictionary(x => x.x, x => _values.ElementAtOrDefault(x.i));
        }
    }
    public class BindingCalculatorDynamic : CalculatorDynamicBase, IBindingCalculator {
        readonly NRoot expr;
        readonly NRoot backExpr;
        readonly object fallbackValue;

        public BindingCalculatorDynamic(BindingTreeInfo treeInfo, object fallbackValue)
            : this(treeInfo.Expr, treeInfo.BackExpr, fallbackValue, treeInfo.ErrorHandler) { }
        internal BindingCalculatorDynamic(NRoot expr, NRoot backExpr, object fallbackValue, IErrorHandler errorHandler)
            : base(errorHandler) {
            this.expr = expr;
            this.backExpr = backExpr;
            this.fallbackValue = fallbackValue;
        }

        public object Resolve(object[] values) {
            return VisitorEvaluator.Resolve(expr, GetOperandsValues(values), x => GetResolvedType(x), ErrorHandler)
                .FirstOrDefault();
        }
        public IEnumerable<object> ResolveBack(object[] values, object backParam) {
            return VisitorEvaluator.ResolveBack(backExpr, backParam, Operands.ToList(), GetOperandsValues(values), x => GetResolvedType(x), ErrorHandler);
        }
        internal override IEnumerable<VisitorType.TypeInfo> GetTypeInfos() {
            return VisitorType.Resolve(expr, backExpr);
        }
        internal override IEnumerable<Operand> GetOperands() {
            return VisitorOperand.Resolve(new[] { expr }, backExpr, x => GetResolvedType(x), ErrorHandler).ToList();
        }
    }
    public class CommandCalculatorDynamic : CalculatorDynamicBase, ICommandCalculator {
        readonly NRoot executeExpr;
        readonly NRoot canExecuteExpr;
        readonly bool fallbackCanExecute;

        public CommandCalculatorDynamic(CommandTreeInfo treeInfo, bool fallbackCanExecute)
            : this(treeInfo.ExecuteExpr, treeInfo.CanExecuteExpr, fallbackCanExecute, treeInfo.ErrorHandler) { }
        internal CommandCalculatorDynamic(NRoot executeExpr, NRoot canExecuteExpr, bool fallbackCanExecute, IErrorHandler errorHandler)
            : base(errorHandler) {
            this.executeExpr = executeExpr;
            this.canExecuteExpr = canExecuteExpr;
            this.fallbackCanExecute = fallbackCanExecute;
        }

        public void Execute(object[] opValues, object parameter) {
            VisitorEvaluator.ResolveExecute(executeExpr, parameter, GetOperandsValues(opValues), x => GetResolvedType(x), ErrorHandler);
        }
        public bool CanExecute(object[] opValues, object parameter) {
            return VisitorEvaluator.ResolveCanExecute(canExecuteExpr, parameter, GetOperandsValues(opValues), x => GetResolvedType(x), ErrorHandler);
        }
        internal override IEnumerable<VisitorType.TypeInfo> GetTypeInfos() {
            return VisitorType.Resolve(executeExpr, canExecuteExpr, false);
        }
        internal override IEnumerable<Operand> GetOperands() {
            return VisitorOperand.Resolve(new[] { executeExpr }, null, GetResolvedType, ErrorHandler, false)
                .Union(VisitorOperand.Resolve(new[] { canExecuteExpr }, null, GetResolvedType, ErrorHandler, true));
        }
    }
    public class EventCalculatorDynamic : CalculatorDynamicBase, IEventCalculator {
        readonly NRoot expr;
        public EventCalculatorDynamic(EventTreeInfo treeInfo)
            : this(treeInfo.Expr, treeInfo.ErrorHandler) { }
        internal EventCalculatorDynamic(NRoot expr, IErrorHandler errorHandler)
            : base(errorHandler) {
            this.expr = expr;
        }

        public void Event(object[] opValues, object sender, object args) {
            VisitorEvaluator.ResolveEvent(expr, sender, args, GetOperandsValues(opValues), x => GetResolvedType(x), ErrorHandler);
        }
        internal override IEnumerable<VisitorType.TypeInfo> GetTypeInfos() {
            return VisitorType.Resolve(expr, null);
        }
        internal override IEnumerable<Operand> GetOperands() {
            return VisitorOperand.Resolve(new[] { expr }, null, GetResolvedType, ErrorHandler, false);
        }
    }

    #region OLD
    abstract class VisitorExpressionBase : VisitorBase<Expression> {
        #region mapping
        static readonly MethodInfo StringConcatMethod =
            typeof(string).GetMethod("Concat", MemberSearcher.StaticBindingFlags, System.Type.DefaultBinder, new[] { typeof(object), typeof(object) }, null);
        static readonly Func<Expression, Expression, Expression> StringConcatExpression =
            (x, y) => Expression.Call(null, StringConcatMethod, x, y);
        static readonly MethodInfo DependencyObject_GetValueMethod =
            typeof(System.Windows.DependencyObject).GetMethod("GetValue", BindingFlags.Public | BindingFlags.Instance, System.Type.DefaultBinder, new[] { typeof(System.Windows.DependencyProperty) }, null);
        static readonly Func<Expression, Expression, Expression> DependencyObject_GetValueExpression =
            (x, y) => Expression.Call(x, DependencyObject_GetValueMethod, y);
        static readonly Dictionary<NBinary.NKind, Func<Expression, Expression, Expression>> binaryKindToExpressionMapping =
            new Dictionary<NBinary.NKind, Func<Expression, Expression, Expression>>() {
            { NBinary.NKind.Mul,          Expression.Multiply },
            { NBinary.NKind.Div,          Expression.Divide },
            { NBinary.NKind.Mod,          Expression.Modulo },
            { NBinary.NKind.Plus,         Expression.Add },
            { NBinary.NKind.Minus,        Expression.Subtract },
            { NBinary.NKind.ShiftLeft,    Expression.LeftShift },
            { NBinary.NKind.ShiftRight,   Expression.RightShift },

            { NBinary.NKind.Less,         Expression.LessThan },
            { NBinary.NKind.Greater,      Expression.GreaterThan },
            { NBinary.NKind.LessOrEqual,  Expression.LessThanOrEqual },
            { NBinary.NKind.GreaterOrEqual,Expression.GreaterThanOrEqual },
            { NBinary.NKind.Equal,        Expression.Equal },
            { NBinary.NKind.NotEqual,     Expression.NotEqual },

            { NBinary.NKind.And,          Expression.And },
            { NBinary.NKind.Or,           Expression.Or },
            { NBinary.NKind.Xor,          Expression.ExclusiveOr },
            { NBinary.NKind.AndAlso,      Expression.AndAlso },
            { NBinary.NKind.OrElse,       Expression.OrElse },

            { NBinary.NKind.Coalesce,     Expression.Coalesce },
        };
        static readonly Dictionary<NUnary.NKind, Func<Expression, Expression>> unaryKindToExpressionMapping =
            new Dictionary<NUnary.NKind, Func<Expression, Expression>>() {
            { NUnary.NKind.Plus,          x => x },
            { NUnary.NKind.Minus,         Expression.Negate },
            { NUnary.NKind.Not,           Expression.Not },
            { NUnary.NKind.NotBitwise,    Expression.Not },
        };
        static readonly Dictionary<NCast.NKind, Func<Expression, Type, Expression>> castKindToExpressionMapping =
            new Dictionary<NCast.NKind, Func<Expression, Type, Expression>>() {
            { NCast.NKind.Cast,           Expression.Convert },
            { NCast.NKind.Is,             Expression.TypeIs },
            { NCast.NKind.As,             Expression.TypeAs },
        };
        static readonly Dictionary<NTernary.NKind, Func<Expression, Expression, Expression, Expression>> ternaryKindToExpressionMapping =
            new Dictionary<NTernary.NKind, Func<Expression, Expression, Expression, Expression>>() {
            { NTernary.NKind.Condition,   Expression.Condition },
        };
        #endregion
        #region Visitor
        protected override Expression RootIdent(NIdentBase n) {
            Expression res;
            NIdentBase rest;
            Operand operand = VisitorOperand.ReduceIdent(n, typeResolver, out rest);
            if(operand == null && rest is NRelative) {
                res = Relative(null, (NRelative)rest);
                rest = rest.Next;
            } else
                res = GetOperandParameter(operand, null);
            n = rest;
            while(n != null) {
                if(!CanContinue(n, operand)) return null;
                res = RootIdentCore(res, n);
                operand = null;
                n = n.Next;
            }
            return res;
        }
        protected override Expression Index(Expression from, NIndex n, IEnumerable<Expression> indexArgs) {
            if(from.Type.IsArray)
                return Expression.ArrayIndex(from, indexArgs);
            var allMethods = from.Type.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static)
                .Where(x => x.GetIndexParameters().Any())
                .Select(x => x.GetGetMethod())
                .ToArray();
            return GetInvocationExpression(from, from.Type, allMethods, n.Name, indexArgs);
        }
        protected override Expression Relative(Expression from, NRelative n) {
            switch(n.Kind) {
                case NRelative.NKind.Parameter:
                case NRelative.NKind.Value:
                case NRelative.NKind.Sender:
                case NRelative.NKind.Args: return GetOperandParameter(null, n.Kind);
                default: throw new NotImplementedException();
            }
        }
        protected override Expression Ident(Expression from, NIdent n) {
            var prop = MemberSearcher.FindPropertyCore(from.Type, n.Name, BindingFlags.Public | BindingFlags.Instance);
            if(prop == null) {
                var field = MemberSearcher.FindFieldCore(from.Type, n.Name, BindingFlags.Public | BindingFlags.Instance);
                if(field == null) {
                    errorHandler.Report(ErrorHelper.Report001(n.Name, from.Type), true);
                    return null;
                }
                return Expression.Field(from, field);
            }
            return Expression.Property(from, prop);
        }
        Expression GetInvocationExpression(Expression from, Type fromType, MethodInfo[] possibleMethods, string methodName, IEnumerable<Expression> methodArgs) {
            Type[] args;
            var method = MemberSearcher.FindMethod(possibleMethods, methodArgs.Select(x => x.Type).ToArray(), out args);
            if(method == null) {
                errorHandler.Report(ErrorHelper.Report002(methodName, methodArgs.Select(x => x.Type).ToArray(), fromType), true);
                return null;
            }
            var _methodArgs = new List<Expression>();
            if(args != null) {
                for(int i = 0; i < args.Count(); i++)
                    _methodArgs.Add(Expression.Convert(methodArgs.ElementAt(i), args[i]));
            }
            var parameters = method.GetParameters();
            if(parameters.Any() && MemberSearcher.IsParams(parameters.Last())) {
                int toPack = args.Length - parameters.Length + 1;
                var paramsArray = _methodArgs.Skip(_methodArgs.Count - toPack).ToArray();
                _methodArgs = _methodArgs.Take(methodArgs.Count() - toPack)
                    .Concat(new[] { Expression.NewArrayInit(args.Last(), paramsArray) })
                    .ToList();
            }
            if(parameters.Length > args.Length) {
                for(int i = args.Length; i < parameters.Length; ++i) {
                    _methodArgs.Add(Expression.Constant(parameters[i].DefaultValue, parameters[i].ParameterType));
                }
            }
            return Expression.Call(from, method, _methodArgs);
        }
        protected override Expression Method(Expression from, NMethod n, IEnumerable<Expression> methodArgs) {
            var allMethods = from.Type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static)
                    .Where(x => x.Name == n.Name)
                    .ToArray();
            return GetInvocationExpression(from, from.Type, allMethods, n.Name, methodArgs);
        }
        protected override Expression Type_Type(Expression from, NType n) {
            throw new NotImplementedException();
        }
        protected override Expression Type_TypeOf(Expression from, NType n) {
            Type type = typeResolver(n);
            return Expression.Constant(type, typeof(Type));
        }
        protected override Expression Type_StaticIdent(Expression from, NType n) {
            Type t = typeResolver(n);
            return Type_StaticIdentCore(t, n.Ident.Name);
        }
        protected override Expression Type_StaticMethod(Expression from, NType n, IEnumerable<Expression> methodArgs) {
            Type t = typeResolver(n);
            var allMethods = t.GetMethods(MemberSearcher.StaticBindingFlags)
                .Where(x => x.Name == n.Ident.Name)
                .ToArray();
            return GetInvocationExpression(null, t, allMethods, n.Ident.Name, methodArgs);
        }
        Expression Type_StaticIdentCore(Type t, string nName) {
            object member = (object)t.GetProperty(nName, MemberSearcher.StaticBindingFlags)
                ?? (object)t.GetField(nName, MemberSearcher.StaticBindingFlags);
            if(member is PropertyInfo) {
                return Expression.Property(null, (PropertyInfo)member);
            } else if(member is FieldInfo) {
                return Expression.Field(null, (FieldInfo)member);
            } else {
                errorHandler.Throw(ErrorHelper.Report001(nName, t), null);
                return Expression.Constant(null);
            }
        }
        protected override Expression Type_Attached(Expression from, NType n) {
            Expression attachedProperty = Type_StaticIdentCore(typeResolver(n), n.Ident.Name + "Property");
            return DependencyObject_GetValueExpression(from, attachedProperty);
        }
        protected override Expression Type_New(Expression from, NNew n, IEnumerable<Expression> methodArgs) {
            throw new NotSupportedException();
        }

        protected override Expression Constant(NConstant n) {
            return Expression.Constant(n.Value);
        }
        protected override Expression Binary(NBinary n, Expression left, Expression right) {
            if(n.Kind == NBinary.NKind.Plus && (left.Type == typeof(string) || right.Type == typeof(string)))
                return StringConcatExpression(Expression.Convert(left, typeof(object)), Expression.Convert(right, typeof(object)));
            ParserHelper.CastNumericTypes(ref left, ref right);
            return binaryKindToExpressionMapping[n.Kind](left, right);
        }
        protected override Expression Unary(NUnary n, Expression value) {
            return unaryKindToExpressionMapping[n.Kind](value);
        }
        protected override Expression Cast(NCast n, Expression value) {
            return castKindToExpressionMapping[n.Kind](value, typeResolver(n.Type));
        }
        protected override Expression Ternary(NTernary n, Expression first, Expression second, Expression third) {
            ParserHelper.CastNumericTypes(ref second, ref third);
            if(second.Type != third.Type) {
                bool secondToThird = MemberSearcher.IsImplicitConversion(second.Type, third.Type);
                bool thirdToSecond = MemberSearcher.IsImplicitConversion(third.Type, second.Type);
                if(secondToThird && !thirdToSecond) {
                    second = Expression.Convert(second, third.Type);
                }
                if (thirdToSecond && !secondToThird) {
                    third = Expression.Convert(third, second.Type);
                }
            }
            return ternaryKindToExpressionMapping[n.Kind](first, second, third);
        }

        protected override bool CanContinue(NBase n) {
            return CanContinue(n, null);
        }
        protected abstract bool CanContinue(NBase n, Operand operand);

        List<Tuple<Operand, Expression>> assigns;
        protected override Expression Assign(NAssign n, Expression value) {
            NIdentBase rest;
            if (!(n.Left is NIdentBase))
                throw new InvalidOperationException();
            var op = VisitorOperand.ReduceIdent((NIdentBase)n.Left, x => typeResolver(x), out rest);
            var expr = Visit(n.Expr);
            assigns.Add(new Tuple<Operand, Expression>(op, expr));
            return null;
        }
        #endregion

        IErrorHandler errorHandler;
        Func<NType, Type> typeResolver;
        protected IEnumerable<Expression> Resolve(NRoot expr, Func<NType, Type> typeResolver, IErrorHandler errorHandler) {
            try {
                this.errorHandler = errorHandler;
                this.typeResolver = typeResolver;
                return RootVisit(expr);
            } finally {
                this.errorHandler = null;
                this.typeResolver = null;
            }
        }
        protected IEnumerable<Tuple<Operand, Expression>> ResolveBackCore(NRoot expr, Func<NType, Type> typeResolver, IErrorHandler errorHandler) {
            assigns = new List<Tuple<Operand, Expression>>();
            var res = Resolve(expr, typeResolver, errorHandler);
            if(res.Count() == 1 && res.First() != null)
                return new[] { new Tuple<Operand, Expression>(null, res.First()) };
            var resAssigns = assigns;
            assigns = null;
            return resAssigns;
        }

        protected abstract ParameterExpression GetOperandParameter(Operand operand, NRelative.NKind? relativeSource);
    }
    class VisitorExpression : VisitorExpressionBase {
        #region OperandInfo
        public class OperandInfo {
            public Type OperandType { get; private set; }
            public object OperandValue { get; private set; }
            public ParameterExpression Parameter { get; private set; }
            public void Init(object opValue) {
                OperandValue = opValue;
                OperandType = opValue != null ? opValue.GetType() : null;
            }
            public void CreateParameter() {
                Parameter = Expression.Parameter(OperandType ?? typeof(object));
            }
            public void Clear() {
                OperandValue = null;
            }
            public void ClearParameter() {
                Parameter = null;
            }
        }
        #endregion

        readonly IErrorHandler errorHandler;
        readonly Dictionary<Operand, OperandInfo> operandInfos;
        readonly Func<NType, Type> typeResolver;
        public VisitorExpression(Dictionary<Operand, OperandInfo> operandInfos, Func<NType, Type> typeResolver, IErrorHandler errorHandler) {
            this.operandInfos = operandInfos;
            this.errorHandler = errorHandler;
            this.typeResolver = typeResolver;
        }

        List<ParameterExpression> currentParameters;
        ParameterExpression backParam;
        ParameterExpression parameterParam;
        ParameterExpression senderParam;
        ParameterExpression argsParam;
        public Func<object[], object> Resolve(NRoot expr) {
            currentParameters = new List<ParameterExpression>();
            var _expr = Resolve(expr, typeResolver, errorHandler).FirstOrDefault();
            var res = Compile(_expr);
            Clean();
            return res;
        }
        public void ResolveBack(NRoot backExpr, Type backExprType) {
            if(backExprType == null) errorHandler.Throw(ErrorHelper.Err104(), null);
            currentParameters = new List<ParameterExpression>();
            backParam = Expression.Parameter(backExprType, "$value");
            var res = ResolveBackCore(backExpr, typeResolver, errorHandler);
            currentParameters.Add(backParam);
            foreach(var tuple in res) {
                var converter = Compile(tuple.Item2);
                if(tuple.Item1 == null) {
                    operandInfos.Keys.First()
                        .SetBackConverter(converter);
                } else {
                    operandInfos.Keys.First(x => x.Equals(tuple.Item1))
                          .SetBackConverter(converter);
                }
            }
            Clean();
        }
        public void ResolveExecute(NRoot executeExpr, NRoot canExecuteExpr, Type parameterType,
            out Func<object[], object> execute, out Func<object[], object> canExecute) {
            execute = null;
            canExecute = null;
            currentParameters = new List<ParameterExpression>();
            parameterParam = Expression.Parameter(parameterType ?? typeof(object));
            var _ex = Resolve(executeExpr, typeResolver, errorHandler);
            var _canEx = Resolve(canExecuteExpr, typeResolver, errorHandler);
            currentParameters.Add(parameterParam);
            if(!errorHandler.HasError) {
                execute = Compile(Expression.Block(_ex));
                canExecute = Compile(Expression.Convert(_canEx.First(), typeof(bool)));
            }
            Clean();
        }
        public Func<object[], object> ResolveEvent(NRoot expr, Type senderType, Type argsType) {
            currentParameters = new List<ParameterExpression>();
            senderParam = Expression.Parameter(senderType ?? typeof(object));
            argsParam = Expression.Parameter(argsType ?? typeof(object));
            var _expr = Resolve(expr, typeResolver, errorHandler);
            currentParameters.Add(senderParam);
            currentParameters.Add(argsParam);
            var res = errorHandler.HasError ? null : Compile(Expression.Block(_expr));
            Clean();
            return res;
        }
        void Clean() {
            currentParameters = null;
            backParam = null;
            parameterParam = null;
        }

        protected override ParameterExpression GetOperandParameter(Operand operand, NRelative.NKind? relativeSource) {
            if(relativeSource.HasValue) {
                switch(relativeSource.Value) {
                    case NRelative.NKind.Value: return backParam;
                    case NRelative.NKind.Parameter: return parameterParam;
                    case NRelative.NKind.Sender: return senderParam;
                    case NRelative.NKind.Args: return argsParam;
                    default: throw new NotImplementedException();
                }
            }
            if(operand != null) {
                if(operandInfos[operand].Parameter == null)
                    operandInfos[operand].CreateParameter();
                var res = operandInfos[operand].Parameter;
                if(!currentParameters.Contains(res))
                    currentParameters.Add(res);
                return res;
            }
            return null;
        }
        protected override bool CanContinue(NBase n, Operand operand) {
            if(errorHandler.HasError) return false;
            if(operand != null) {
                if(n is NIdent || n is NMethod) {
                    var operandValue = operandInfos[operand].OperandValue;
                    if(operandValue == null) {
                        errorHandler.Report(null, true);
                        return false;
                    }
                }
            }
            return true;
        }
        Func<object[], object> Compile(Expression expr) {
            if(expr == null) return null;
            return Expression.Lambda(expr, currentParameters).Compile().DynamicInvoke;
        }
    }

    public abstract class CalculatorBase {
        public IEnumerable<Operand> Operands { get { return OperandInfos.Keys; } }
        internal VisitorExpression VisitorExpression { get; private set; }
        internal Dictionary<VisitorType.TypeInfo, Type> TypeInfos { get; private set; }
        internal IErrorHandler ErrorHandler { get; private set; }
        internal ITypeResolver TypeResolver { get; private set; }
        internal Dictionary<Operand, VisitorExpression.OperandInfo> OperandInfos { get; set; }

        public CalculatorBase(IErrorHandler errorHandler) {
            ErrorHandler = errorHandler;

            this.TypeInfos = new Dictionary<VisitorType.TypeInfo, Type>();
            this.OperandInfos = new Dictionary<Operand, VisitorExpression.OperandInfo>();
        }
        public virtual void Init(ITypeResolver typeResolver) {
            TypeResolver = typeResolver;
            foreach (var typeInfo in TypeInfos.Keys.ToList())
                TypeInfos[typeInfo] = VisitorType.ResolveType(typeInfo, typeResolver, ErrorHandler);
        }
        internal void InitTypeInfos(IEnumerable<VisitorType.TypeInfo> typeInfos) {
            foreach (var typeInfo in typeInfos)
                TypeInfos.Add(typeInfo, null);
        }
        internal void InitOperands(IEnumerable<Operand> operands) {
            foreach (var operand in operands)
                OperandInfos.Add(operand, new VisitorExpression.OperandInfo());
            VisitorExpression = new VisitorExpression(OperandInfos, GetResolvedType, ErrorHandler);
        }
        internal Type GetResolvedType(NType n) {
            return TypeInfos[new VisitorType.TypeInfo(n)];
        }
        internal void ResolveCore(object[] opValues, bool forceRecompile, Action recompile, Action calculate) {
            if (!CheckOperandsCount(opValues)) return;
            bool needToRecompile = NeedToRecompile(opValues);
            InitOperandInfos(opValues);
            if (forceRecompile || needToRecompile) {
                ClearOperandParameters();
                recompile();
            }
            CalculateCore(calculate);
            ClearOperandInfos();
        }

        bool CheckOperandsCount(object[] opValues) {
            if (Operands.Count() == 0 && opValues == null)
                return true;
            if (opValues == null)
                return false;
            if (Operands.Count() != opValues.Count())
                throw new InvalidOperationException();
            return true;
        }
        bool NeedToRecompile(object[] opValues) {
            if (opValues == null) return false;
            var opTypes = OperandInfos.Values.Select(x => x.OperandType);
            var actualOpTypes = opValues.Select(x => x != null ? x.GetType() : null);
            return !opTypes.SequenceEqual(actualOpTypes);
        }
        void InitOperandInfos(object[] opValues) {
            if (opValues == null) return;
            int i = 0;
            var opList = opValues.ToList();
            foreach (var op in Operands) {
                OperandInfos[op].Init(opList[i]);
                i++;
            }
        }
        void ClearOperandInfos() {
            foreach (var opInfo in OperandInfos.Values)
                opInfo.Clear();
        }
        void ClearOperandParameters() {
            foreach (var opInfo in OperandInfos.Values)
                opInfo.ClearParameter();
        }
        void CalculateCore(Action calculate) {
            if (ErrorHandler.HasError) return;
            try {
                calculate();
            } catch (Exception e) {
                ErrorHandler.Report(e.Message, true);
            }
        }
    }
    public class BindingCalculator : CalculatorBase, IBindingCalculator {
        readonly NRoot expr;
        readonly NRoot backExpr;
        readonly object fallbackValue;
        public BindingCalculator(BindingTreeInfo treeInfo, object fallbackValue)
            : this(treeInfo.Expr, treeInfo.BackExpr, fallbackValue, treeInfo.ErrorHandler) { }
        internal BindingCalculator(NRoot expr, NRoot backExpr, object fallbackValue, IErrorHandler errorHandler)
            : base(errorHandler) {
            this.expr = expr;
            this.backExpr = backExpr;
            this.fallbackValue = fallbackValue;
            InitTypeInfos(VisitorType.Resolve(expr, backExpr));
        }
        public override void Init(ITypeResolver typeResolver) {
            base.Init(typeResolver);
            InitOperands(VisitorOperand.Resolve(new[] { expr }, backExpr, GetResolvedType, ErrorHandler));
        }
        public void InitBack(Type backExprType) {
            if (backExpr != null) VisitorExpression.ResolveBack(backExpr, backExprType);
        }

        Func<object[], object> calc;
        public object Resolve(object[] opValues) {
            if (ErrorHandler.HasError) return fallbackValue;
            object res = fallbackValue;
            ResolveCore(opValues, calc == null,
                () => { calc = VisitorExpression.Resolve(expr); },
                () => { res = calc != null ? calc(CollectParameterValues()) : fallbackValue; });
            return res;
        }
        object[] CollectParameterValues() {
            List<object> res = new List<object>();
            foreach (var opInfo in OperandInfos.Values) {
                if (opInfo.Parameter != null)
                    res.Add(opInfo.OperandValue);
            }
            return res.ToArray();
        }
    }
    public class CommandCalculator : CalculatorBase, ICommandCalculator {
        readonly NRoot executeExpr;
        readonly NRoot canExecuteExpr;
        readonly bool fallbackCanExecute;
        public CommandCalculator(CommandTreeInfo treeInfo, bool fallbackCanExecute)
            : this(treeInfo.ExecuteExpr, treeInfo.CanExecuteExpr, fallbackCanExecute, treeInfo.ErrorHandler) { }
        internal CommandCalculator(NRoot executeExpr, NRoot canExecuteExpr, bool fallbackCanExecute, IErrorHandler errorHandler)
            : base(errorHandler) {
            this.executeExpr = executeExpr;
            this.canExecuteExpr = canExecuteExpr;
            this.fallbackCanExecute = fallbackCanExecute;
            InitTypeInfos(VisitorType.Resolve(executeExpr, canExecuteExpr));
        }
        public override void Init(ITypeResolver typeResolver) {
            base.Init(typeResolver);
            InitOperands(VisitorOperand.Resolve(new[] { executeExpr, canExecuteExpr }, null, GetResolvedType, ErrorHandler));
        }

        Type parameterType;
        Func<object[], object> execute;
        Func<object[], object> canExecute;

        public void Execute(object[] opValues, object parameter) {
            var input = CollectParameterValues(opValues, parameter);
            Resolve(opValues, parameter, () => { execute(input); });
        }
        public bool CanExecute(object[] opValues, object parameter) {
            bool res = fallbackCanExecute;
            var input = CollectParameterValues(opValues, parameter);
            Resolve(opValues, parameter, () => { res = (bool)canExecute(input); });
            return res;
        }
        void Resolve(object[] opValues, object parameter, Action calculate) {
            if (ErrorHandler.HasError) return;
            var actualParameterType = parameter != null ? parameter.GetType() : typeof(object);
            ResolveCore(
                opValues,
                execute == null || canExecute == null || parameterType != actualParameterType,
                () => {
                    this.parameterType = actualParameterType;
                    VisitorExpression.ResolveExecute(
                        executeExpr,
                        canExecuteExpr,
                        parameterType,
                        out execute,
                        out canExecute);
                },
                calculate);
        }
        object[] CollectParameterValues(object[] opValues, object parameter) {
            List<object> res = new List<object>(opValues ?? new object[] { });
            res.Add(parameter);
            return res.ToArray();
        }
    }
    public class EventCalculator : CalculatorBase, IEventCalculator {
        readonly NRoot expr;
        public EventCalculator(EventTreeInfo treeInfo)
            : this(treeInfo.Expr, treeInfo.ErrorHandler) { }
        internal EventCalculator(NRoot expr, IErrorHandler errorHandler)
            : base(errorHandler) {
            this.expr = expr;
            InitTypeInfos(VisitorType.Resolve(expr, null));
        }
        public override void Init(ITypeResolver typeResolver) {
            base.Init(typeResolver);
            InitOperands(VisitorOperand.Resolve(new[] { expr }, null, GetResolvedType, ErrorHandler));
        }

        Type senderType;
        Type argsType;
        Func<object[], object> eventFunc;
        public void Event(object[] opValues, object sender, object args) {
            if (ErrorHandler.HasError) return;
            var input = CollectParameterValues(opValues, sender, args);
            ResolveCore(opValues, eventFunc == null || senderType == null || argsType == null,
                () => {
                    if (senderType == null) senderType = sender != null ? sender.GetType() : null;
                    if (argsType == null) argsType = args != null ? args.GetType() : null;
                    eventFunc = VisitorExpression.ResolveEvent(expr, senderType, argsType);
                },
                () => { eventFunc(input); });
        }
        object[] CollectParameterValues(object[] opValues, object sender, object args) {
            List<object> res = new List<object>(opValues ?? new object[] { });
            res.Add(sender);
            res.Add(args);
            return res.ToArray();
        }
    }
    #endregion OLD
}