using DevExpress.Mvvm.Native;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace DevExpress.Mvvm.DataAnnotations {
    public interface IMetadataLocator {
        Type[] GetMetadataTypes(Type type);
    }
    public class MetadataLocator : IMetadataLocator {
        public static IMetadataLocator Default;

        public static MetadataLocator Create() {
            return new MetadataLocator(Enumerable.Empty<Tuple<Type, Type>>());
        }
        readonly IEnumerable<Tuple<Type, Type>> infoList;
        IDictionary<Type, Type[]> dictionary;
        IDictionary<Type, Type[]> Dictionary {
            get {
                return dictionary ?? (dictionary = MetadataHelper.InternalMetadataProviders.Union(infoList).GroupBy(x => x.Item1, x => x.Item2).ToDictionary(x => x.Key, x => x.ToArray()));
            }
        }
        MetadataLocator(IEnumerable<Tuple<Type, Type>> infoList) {
            this.infoList = infoList;
            MetadataHelper.RegisterLocator(this);
        }
        internal void Update() {
            dictionary = null;
        }


        Type[] IMetadataLocator.GetMetadataTypes(Type type) {
            Type[] result;
            Dictionary.TryGetValue(type, out result);
            return result;
        }
        public MetadataLocator AddMetadata(Type type, Type metadataType) {
            var tuple = new Tuple<Type, Type>(type, metadataType);
            return AddMetadata(new[] { tuple });
        }
        public MetadataLocator AddMetadata(Type metadataType) {
            return AddMetadata(MetadataHelper.GetMetadataInfoList(metadataType));
        }
        public MetadataLocator AddMetadata<T, TMetadata>() {
            return AddMetadata(typeof(T), typeof(TMetadata));
        }
        public MetadataLocator AddMetadata<TMetadata>() {
            return AddMetadata(typeof(TMetadata));
        }
        MetadataLocator AddMetadata(IEnumerable<Tuple<Type, Type>> newInfoList) {
            MetadataHelper.CheckMetadata(newInfoList);
            return new MetadataLocator(infoList.Union(newInfoList));
        }
    }
}