using DevExpress.Mvvm.Native;
using System;
using System.Linq.Expressions;
using System.Windows.Input;

namespace DevExpress.Mvvm.DataAnnotations {
    public abstract class CommandMetadataBuilderBase<T, TBuilder> : PropertyMetadataBuilderBase<T, ICommand, TBuilder> where TBuilder : PropertyMetadataBuilderBase<T, ICommand, TBuilder> {
        internal CommandMetadataBuilderBase(MemberMetadataStorage storage, MetadataBuilder<T> parent)
            : base(storage, parent) {
        }
        public MetadataBuilder<T> EndCommand() {
            return parent;
        }
    }
    public class CommandMethodMetadataBuilder<T> : CommandMetadataBuilderBase<T, CommandMethodMetadataBuilder<T>> {
        readonly string methodName;
        internal CommandMethodMetadataBuilder(MemberMetadataStorage storage, MetadataBuilder<T> parent, string methodName)
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
#if !SILVERLIGHT
        public CommandMethodMetadataBuilder<T> DoNotUseCommandManager() {
            return AddOrModifyAttribute<CommandAttribute>(x => x.UseCommandManager = false);
        }
#endif
        public CommandMethodMetadataBuilder<T> DoNotCreateCommand() {
            return AddOrReplaceAttribute(new CommandAttribute(false));
        }
    }

}