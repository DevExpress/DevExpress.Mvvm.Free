using DevExpress.Mvvm.Native;
using System;
using System.Linq.Expressions;
using System.Windows.Input;

namespace DevExpress.Mvvm.DataAnnotations {
    public abstract class CommandMetadataBuilderBase<T, TBuilder> :
        MemberMetadataBuilderBase<T, TBuilder, ClassMetadataBuilder<T>>
        where TBuilder : CommandMetadataBuilderBase<T, TBuilder> {
        internal CommandMetadataBuilderBase(MemberMetadataStorage storage, ClassMetadataBuilder<T> parent)
            : base(storage, parent) {
        }
        public MetadataBuilder<T> EndCommand() {
            return (MetadataBuilder<T>)parent;
        }

    }
}