namespace DevExpress.Mvvm.DataAnnotations {
    public interface IEnumMetadataProvider<T> where T : struct {
        void BuildMetadata(EnumMetadataBuilder<T> builder);
    }
    public class EnumMetadataBuilder<T> : MetadataBuilderBase<T, EnumMetadataBuilder<T>> where T : struct {
        public EnumMetadataBuilder() { }
        public EnumMemberMetadataBuilder<T> Member(T member) {
            return GetBuilder(member.ToString(), x => new EnumMemberMetadataBuilder<T>(x, this));
        }
    }
}