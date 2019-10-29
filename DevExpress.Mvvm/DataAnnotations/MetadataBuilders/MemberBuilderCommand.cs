using DevExpress.Mvvm.Native;
using System;
using System.Linq.Expressions;

namespace DevExpress.Mvvm.DataAnnotations {
    public abstract class CommandMethodMetadataBuilderBase<T, TBuilder> :
        CommandMetadataBuilderBase<T, TBuilder>
        where TBuilder : CommandMethodMetadataBuilderBase<T, TBuilder> {
        readonly string methodName;
        internal CommandMethodMetadataBuilderBase(MemberMetadataStorage storage, ClassMetadataBuilder<T> parent, string methodName)
            : base(storage, parent) {
            this.methodName = methodName;
        }
        public TBuilder CanExecuteMethod(Expression<Func<T, bool>> canExecuteMethodExpression) {
            return AddOrModifyAttribute<CommandAttribute>(x => x.CanExecuteMethod = ExpressionHelper.GetArgumentFunctionStrict(canExecuteMethodExpression));
        }
        public TBuilder CommandName(string commandName) {
            return AddOrModifyAttribute<CommandAttribute>(x => x.Name = commandName);
        }
        public TBuilder UseMethodNameAsCommandName() {
            return CommandName(methodName);
        }
        public TBuilder DoNotUseCommandManager() {
            return AddOrModifyAttribute<CommandAttribute>(x => x.UseCommandManager = false);
        }
        public TBuilder DoNotCreateCommand() {
            return AddOrReplaceAttribute(new CommandAttribute(false));
        }
    }
    public class CommandMethodMetadataBuilder<T> :
        CommandMethodMetadataBuilderBase<T, CommandMethodMetadataBuilder<T>> {
        internal CommandMethodMetadataBuilder(MemberMetadataStorage storage, ClassMetadataBuilder<T> parent, string methodName)
            : base(storage, parent, methodName) {
        }
    }
    public class AsyncCommandMethodMetadataBuilder<T> :
        CommandMethodMetadataBuilderBase<T, AsyncCommandMethodMetadataBuilder<T>> {
        internal AsyncCommandMethodMetadataBuilder(MemberMetadataStorage storage, ClassMetadataBuilder<T> parent, string methodName)
            : base(storage, parent, methodName) {
        }
        public AsyncCommandMethodMetadataBuilder<T> AllowMultipleExecution() {
            return AddOrModifyAttribute<CommandAttribute>(x => x.AllowMultipleExecutionCore = true);
        }
    }

}