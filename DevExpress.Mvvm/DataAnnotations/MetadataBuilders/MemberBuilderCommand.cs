using DevExpress.Mvvm.Native;
using System;
using System.Linq.Expressions;

namespace DevExpress.Mvvm.DataAnnotations {
    public class CommandMethodMetadataBuilder<T> : 
        CommandMetadataBuilderBase<T, CommandMethodMetadataBuilder<T>> {
        readonly string methodName;
        internal CommandMethodMetadataBuilder(MemberMetadataStorage storage, ClassMetadataBuilder<T> parent, string methodName)
            : base(storage, parent) {
            this.methodName = methodName;
        }
        public CommandMethodMetadataBuilder<T> CanExecuteMethod(Expression<Func<T, bool>> canExecuteMethodExpression) {
            return AddOrModifyAttribute<CommandAttribute>(x => x.CanExecuteMethod = ExpressionHelper.GetArgumentFunctionStrict(canExecuteMethodExpression));
        }
        public CommandMethodMetadataBuilder<T> CommandName(string commandName) {
            return AddOrModifyAttribute<CommandAttribute>(x => x.Name = commandName);
        }
        public CommandMethodMetadataBuilder<T> UseMethodNameAsCommandName() {
            return CommandName(methodName);
        }
        public CommandMethodMetadataBuilder<T> DoNotUseCommandManager() {
            return AddOrModifyAttribute<CommandAttribute>(x => x.UseCommandManager = false);
        }
        public CommandMethodMetadataBuilder<T> DoNotCreateCommand() {
            return AddOrReplaceAttribute(new CommandAttribute(false));
        }
    }

#if !FREE
    public class CommandMetadataBuilder<T> : 
        CommandMetadataBuilderBase<T, CommandMetadataBuilder<T>> {
        internal CommandMetadataBuilder(MemberMetadataStorage storage, ClassMetadataBuilder<T> parent)
            : base(storage, parent) {
        }
    }
#endif
}
