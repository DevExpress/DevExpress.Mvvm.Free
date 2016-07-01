using System;
using System.Linq;

namespace DevExpress.Xpf.DXBinding.Native {
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
    interface IErrorHandler {
        bool HasError { get; }
        void ClearError();
        void SetError();
        void Report(string msg, bool critical);
        void Throw(string msg, Exception innerException);
    }
    abstract class ErrorHandlerBase : IErrorHandler {
        public bool HasError { get; protected set; }
        public void ClearError() {
            HasError = false;
        }
        public void SetError() {
            HasError = true;
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
    interface ITypeResolver {
        Type ResolveType(string type);
    }

    abstract class TreeInfoBase {
        protected readonly IErrorHandler errorHandler;
        readonly ExprInfo[] exprs;
        protected TreeInfoBase(ExprInfo[] exprs, IErrorHandler errorHandler) {
            this.exprs = exprs;
            this.errorHandler = errorHandler;
            for(int i = 0; i < exprs.Count(); i++)
                exprs[i].Init(Throw);
        }
        protected string GetExprString(int i) { return exprs.ElementAt(i).exprString; }
        protected NRoot GetExpr(int i) { return exprs.ElementAt(i).Expr; }
        protected virtual void Throw(string msg) {
            errorHandler.Throw(msg, null);
        }
        protected class ExprInfo {
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
    class BindingTreeInfo : TreeInfoBase {
        public string ExprString { get { return GetExprString(0); } }
        public NRoot Expr { get { return GetExpr(0); } }
        public string BackExprString { get { return GetExprString(1); } }
        public NRoot BackExpr { get { return GetExpr(1); } }
        public BindingTreeInfo(string expr, string backExpr, IErrorHandler errorHandler)
            : base(new[] {
                    new ExprInfo(expr, "@DataContext", ParserMode.BindingExpr),
                    new ExprInfo(backExpr, null, ParserMode.BindingBackExpr) },
                  errorHandler) {
        }
    }
    class CommandTreeInfo : TreeInfoBase {
        public string ExecuteExprString { get { return GetExprString(0); } }
        public NRoot ExecuteExpr { get { return GetExpr(0); } }
        public string CanExecuteExprString { get { return GetExprString(1); } }
        public NRoot CanExecuteExpr { get { return GetExpr(1); } }
        public CommandTreeInfo(string executeExpr, string canExecuteExpr, IErrorHandler errorHandler)
            : base(new[] {
                new ExprInfo(executeExpr, null, ParserMode.CommandExecute),
                new ExprInfo(canExecuteExpr, "true", ParserMode.CommandCanExecute)
            }, errorHandler) {
        }
    }
    class EventTreeInfo : TreeInfoBase {
        public string ExprString { get { return GetExprString(0); } }
        public NRoot Expr { get { return GetExpr(0); } }
        public EventTreeInfo(string expr, IErrorHandler errorHandler)
            : base(new[] {
                new ExprInfo(expr, null, ParserMode.Event)
            }, errorHandler) {
        }
    }
}