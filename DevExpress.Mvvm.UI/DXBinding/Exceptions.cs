using System;
using System.Collections.Generic;
using System.Linq;

namespace DevExpress.DXBinding.Native {
    public static class ErrorHelper {
        static string NameDXBinding { get { return "DXBinding"; } }
        static string NameDXBindingExpr { get { return "Expr"; } }
        static string NameDXBindingBackExpr { get { return "BackExpr"; } }
        static string NameDXCommand { get { return "DXCommand"; } }
        static string NameDXCommandExecute { get { return "Execute"; } }
        static string NameDXCommandCanExecute { get { return "CanExecute"; } }
        static string NameDXEvent { get { return "DXEvent"; } }
        static string NameDXEventHandler { get { return "Handler"; } }
        static string GetMarkupTarget(Type markup) {
            if(GetMarkupName(markup) == "DXBinding" || GetMarkupName(markup) == "DXCommand")
                return "a DependencyProperty";
            else if(GetMarkupName(markup) == "DXEvent")
                return "an event";
            else throw new NotImplementedException();
        }
        static string GetMarkupName(Type markup) {
            return markup.Name.Replace("Extension", "");
        }

        const string err001 = "The {0} cannot resolve the IProvideValueTarget service.";
        public static string Err001(object markup) {
            return string.Format(err001, GetMarkupName(markup.GetType()));
        }
        const string err002 = "The {0} can only be set on {1} of a DependencyObject.";
        public static string Err002(object markup) {
            return string.Format(err002, GetMarkupName(markup.GetType()), GetMarkupTarget(markup.GetType()));
        }
        const string err003 = "The {0} cannot be used in styles.";
        public static string Err003(object markup) {
            return string.Format(err003, GetMarkupName(markup.GetType()));
        }
        const string err004 = "Cannot resolve the '{0}' type.";
        public static string Err004(string type) {
            return string.Format(err004, type);
        }

        const string Err101 = "The TwoWay or OneWayToSource binding mode requires the {0} property to be set in complex {1}s.";
        public static string Err101_TwoWay() {
            return string.Format(Err101, NameDXBinding + "." + NameDXBindingBackExpr, NameDXBinding);
        }
        const string err102 = "The {0} property is specified in the short form, but the {1} expression contains no binding operands.";
        public static string Err102() {
            return string.Format(err102, NameDXBinding + "." + NameDXBindingBackExpr, NameDXBinding + "." + NameDXBindingExpr);
        }
        const string err103 = "The {0} property is specified in the short form, but the {1} expression contains more than one binding operand.";
        public static string Err103() {
            return string.Format(err103, NameDXBinding + "." + NameDXBindingBackExpr, NameDXBinding + "." + NameDXBindingExpr);
        }
        const string err104 = "The {0} cannot resolve the target type during back conversion.";
        public static string Err104() {
            return string.Format(err104, NameDXBinding);
        }

        const string report001 = "The '{0}' property is not found on object '{1}'.";
        public static string Report001(string property, Type objectType) {
            return string.Format(report001, property, objectType.Name);
        }
        const string report002 = "The '{0}({1})' method is not found on object '{2}'.";
        public static string Report002(string method, Type[] methodArgs, Type objectType) {
            var methodArgsStr = methodArgs == null || !methodArgs.Any() ? string.Empty : methodArgs.Select(x => x != null ? x.Name : "null").Aggregate((x, y) => x + ", " + y);
            return string.Format(report002, method, methodArgsStr, objectType.Name);
        }

        public static string ReportBindingError(string baseMessage, string expr, string backExpr) {
            return ReportCore(NameDXBinding + " error:", baseMessage, new[] {
                new Tuple<string, string>(NameDXBindingExpr, expr),
                new Tuple<string, string>(NameDXBindingBackExpr, backExpr)
            });
        }
        public static string ReportCommandError(string baseMessage, string execute, string canExecute) {
            return ReportCore(NameDXCommand + " error:", baseMessage, new[] {
                new Tuple<string, string>(NameDXCommandExecute, execute),
                new Tuple<string, string>(NameDXCommandCanExecute, canExecute)
            });
        }
        public static string ReportEventError(string baseMessage, string expr) {
            return ReportCore(NameDXEvent + " error:", baseMessage, new[] {
                new Tuple<string, string>(NameDXEventHandler, expr)
            });
        }
        static string ReportCore(string prefix, string baseMessage, Tuple<string, string>[] exprs) {
            var _exprs = exprs.Select(x => {
                string propertyName = x.Item1;
                string propertyValue = x.Item2;
                string format = propertyName + "='{0}'";
                return !string.IsNullOrEmpty(propertyValue) ? string.Format(format, propertyValue) : null;
            });
            var _exprsString = _exprs.Where(x => !string.IsNullOrEmpty(x)).Aggregate((x, y) => string.Format("{0}, {1}", x, y));
            if(string.IsNullOrEmpty(_exprsString))
                return baseMessage;
            return string.Format("{0} {1} {2}.", prefix, baseMessage, _exprsString);
        }

        static Dictionary<string, string> parserErrorReplacementMapping =
          new Dictionary<string, string>() {
                { "Ident ", "identifier " },
                { "Int ", "integer " },
                { "Float ", "float " },
                { "String ", "string " },
                { "??? expected", "invalid expression" },
                { "invalid DXBinding", "invalid expression" },
                { "invalid Back_ExprRoot", "invalid expression" },
                { "invalid TypeExpr", "invalid type expression" },
                { "invalid AtomRootExpr", "invalid identifier expression" },
                { "invalid AtomExpr", "invalid identifier expression" },
                { "invalid Back_AtomExpr", "invalid identifier expression" },
                { "invalid Execute_AtomExpr", "invalid identifier expression" },
                { "invalid ConstExpr", "invalid constant expression" },
                { "invalid RelativeExpr", "invalid expression" },
                { "invalid TypeIdentExpr", "invalid identifier expression" },
                { "invalid NextIdentExpr", "invalid identifier expression" },
                { "invalid Back_AssignLeft", "invalid assignment expression" },
                { "invalid Back_RelativeValueExpr", "invalid expression" },
                { "invalid Execute_Ident", "invalid identifier expression" },
                { "invalid Execute_RelativeExpr", "invalid expression" },
                { "invalid Event_AtomExpr", "invalid identifier expression" },
                { "invalid Event_Ident", "invalid identifier expression" },
                { "invalid Event_RelativeExpr", "invalid expression" },
          };
        internal static string ReportParserError(int pos, string msg, ParserMode mode) {
            foreach(var r in parserErrorReplacementMapping)
                msg = msg.Replace(r.Key, r.Value);
            string caption = string.Empty;
            switch(mode) {
                case ParserMode.BindingExpr:
                case ParserMode.BindingBackExpr:
                    caption = NameDXBinding;
                    break;
                case ParserMode.CommandExecute:
                case ParserMode.CommandCanExecute:
                    caption = NameDXCommand;
                    break;
                case ParserMode.Event:
                    caption = NameDXEvent;
                    break;
                default: throw new NotImplementedException();
            }
            caption += " " + "error";
            if(pos != -1) caption += " " + string.Format("(position {0})", pos);
            return caption + ": " + msg + ".";
        }
    }
}