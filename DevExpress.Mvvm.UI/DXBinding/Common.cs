using System;
using System.Linq;

namespace DevExpress.DXBinding.Native {
    interface IParserErrorHandler {
        bool HasError { get; }
        void Error(int pos, string msg);
    }
    class ParserErrorHandler : IParserErrorHandler {
        readonly ParserMode mode;
        public ParserErrorHandler(ParserMode mode) {
            this.mode = mode;
        }
        public bool HasError { get { return !string.IsNullOrEmpty(error); } }
        public string GetError() {
            return error;
        }
        string error = string.Empty;
        void IParserErrorHandler.Error(int pos, string msg) {
            if(!string.IsNullOrEmpty(error)) error += Environment.NewLine;
            error += ErrorHelper.ReportParserError(pos, msg, mode);
        }
    }

    public interface IErrorHandler {
        bool HasError { get; }
        bool CatchAllExceptions { get; set; }
        void ClearError();
        void SetError();
        void ReportOrThrow(Exception e);
        void Report(string msg, bool critical);
        void Throw(string msg, Exception innerException);
    }
    public abstract class ErrorHandlerBase : IErrorHandler {
        public bool CatchAllExceptions { get; set; }
        public bool HasError { get; protected set; }

        public ErrorHandlerBase() {
            CatchAllExceptions = true;
        }
        public void ClearError() {
            HasError = false;
        }
        public void SetError() {
            HasError = true;
        }
        public void ReportOrThrow(Exception e) {
            if (CatchAllExceptions)
                Report(e.Message, true);
            else throw e;
        }
        public void Report(string msg, bool critical) {
            if(critical) SetError();
            ReportCore(msg);
        }
        public void Throw(string msg, Exception innerException) {
            HasError = true;
            ThrowCore(msg, innerException);
        }
        protected abstract void ReportCore(string msg);
        protected abstract void ThrowCore(string msg, Exception innerException);
    }
    public interface ITypeResolver {
        Type ResolveType(string type);
    }

    public abstract class TreeInfoBase {
        public IErrorHandler ErrorHandler { get; private set; }
        readonly ExprInfo[] exprs;
        internal TreeInfoBase(ExprInfo[] exprs, IErrorHandler errorHandler) {
            this.exprs = exprs;
            this.ErrorHandler = errorHandler;
            for(int i = 0; i < exprs.Count(); i++)
                exprs[i].Init(Throw);
        }
        protected string GetExprString(int i) { return exprs.ElementAt(i).exprString; }
        protected virtual void Throw(string msg) {
            ErrorHandler.Throw(msg, null);
        }
        internal NRoot GetExpr(int i) { return exprs.ElementAt(i).Expr; }
        internal class ExprInfo {
            public readonly string exprString;
            public readonly string defaultExprString;
            readonly ParserMode parseMode;
            Action<string> throwEx;
            public ExprInfo(string exprString, string defaultExpr, ParserMode parseMode) {
                this.exprString = exprString;
                this.defaultExprString = defaultExpr;
                this.parseMode = parseMode;
            }
            public void Init(Action<string> throwEx) {
                this.throwEx = throwEx;
            }
            public NRoot Expr {
                get {
                    if(expr != null) return expr;
                    ParserErrorHandler errors = new ParserErrorHandler(parseMode);
                    var exprString = string.IsNullOrEmpty(this.exprString) ? defaultExprString : this.exprString;
                    if(string.IsNullOrEmpty(exprString)) return null;
                    expr = ParserHelper.GetSyntaxTree(exprString, parseMode, errors);
                    if(errors.HasError) throwEx(errors.GetError());
                    return expr;
                }
            }
            NRoot expr;
        }
    }
    public class BindingTreeInfo : TreeInfoBase {
        public string ExprString { get { return GetExprString(0); } }
        public string BackExprString { get { return GetExprString(1); } }
        internal NRoot Expr { get { return GetExpr(0); } }
        internal NRoot BackExpr { get { return GetExpr(1); } }
        public BindingTreeInfo(string expr, string backExpr, IErrorHandler errorHandler)
            : base(new[] {
                    new ExprInfo(expr, "@DataContext", ParserMode.BindingExpr),
                    new ExprInfo(backExpr, null, ParserMode.BindingBackExpr) },
                  errorHandler) {
        }
        public bool IsSimpleExpr() {
            if(Expr.Exprs.Count() != 1) return false;
            if(!(Expr.Expr is NIdentBase)) return false;
            NIdentBase rest;
            var op = VisitorOperand.ReduceIdent((NIdentBase)Expr.Expr, x => typeof(object), out rest);
            return op != null && rest == null;
        }
        public bool IsEmptyBackExpr() {
            return BackExpr == null;
        }
    }
    public class CommandTreeInfo : TreeInfoBase {
        public string ExecuteExprString { get { return GetExprString(0); } }
        public string CanExecuteExprString { get { return GetExprString(1); } }
        internal NRoot ExecuteExpr { get { return GetExpr(0); } }
        internal NRoot CanExecuteExpr { get { return GetExpr(1); } }
        public CommandTreeInfo(string executeExpr, string canExecuteExpr, IErrorHandler errorHandler)
            : base(new[] {
                new ExprInfo(executeExpr, null, ParserMode.CommandExecute),
                new ExprInfo(canExecuteExpr, "true", ParserMode.CommandCanExecute)
            }, errorHandler) {
        }
    }
    public class EventTreeInfo : TreeInfoBase {
        public string ExprString { get { return GetExprString(0); } }
        internal NRoot Expr { get { return GetExpr(0); } }
        public EventTreeInfo(string expr, IErrorHandler errorHandler)
            : base(new[] {
                new ExprInfo(expr, null, ParserMode.Event)
            }, errorHandler) {
        }
    }
}