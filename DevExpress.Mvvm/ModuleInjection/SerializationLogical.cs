using System;
using System.IO;
using System.Linq;
using System.Text;
using DevExpress.Mvvm.Internal;
using DevExpress.Mvvm.Native;

namespace DevExpress.Mvvm {
    public interface ISupportState<T> {
        T SaveState();
        void RestoreState(T state);
    }
    public interface IStateSerializer {
        string SerializeState(object state, Type stateType);
        object DeserializeState(string state, Type stateType);
    }
    public class StateSerializer : IStateSerializer {
        static IStateSerializer _defaultInstance = new StateSerializer();
        static IStateSerializer _default;
        public static IStateSerializer Default { get { return _default ?? _defaultInstance; } set { _default = value; } }

        public string SerializeState(object state, Type stateType) {
            string res = null;
            if(state == null) return res;
            return SerializationHelper.SerializeToString(x => XmlSerializerHelper.Serialize(x, state, state.GetType()));
        }
        public object DeserializeState(string state, Type stateType) {
            object res = null;
            SerializationHelper.DeserializeFromString(state, x => res = XmlSerializerHelper.Deserialize(x, stateType));
            return res;
        }
    }
}

namespace DevExpress.Mvvm.Native {
    public static class ISupportStateHelper {
        public static Type GetStateType(Type vmType) {
            Type iSupportSerialization = GetISupportStateImplementation(vmType);
            if(iSupportSerialization == null) return null;
            return iSupportSerialization.GetGenericArguments().First();
        }
        public static object GetState(object vm) {
            if(vm == null) return null;
            Type vmType = vm.GetType();
            Type iSupportSerialization = GetISupportStateImplementation(vmType);
            if(iSupportSerialization == null) return null;
            var saveStateMethod = iSupportSerialization.GetMethod("SaveState");
            return saveStateMethod.Invoke(vm, null);
        }
        public static void RestoreState(object vm, object state) {
            if(vm == null || state == null) return;
            Type vmType = vm.GetType();
            Type iSupportSerialization = GetISupportStateImplementation(vmType);
            if(iSupportSerialization == null) return;
            var restoreStateMethod = iSupportSerialization.GetMethod("RestoreState");
            restoreStateMethod.Invoke(vm, new object[] { state });
        }
        static Type GetISupportStateImplementation(Type vmType) {
            return vmType.GetInterfaces()
                .Where(x => x.IsGenericType)
                .Where(x => x.GetGenericTypeDefinition() == typeof(ISupportState<>))
                .FirstOrDefault();
        }
    }
    public static class SerializationHelper {
        public static string SerializeToString(Action<Stream> serializationMethod) {
            string res = null;
            using(var ms = new MemoryStream()) {
                serializationMethod(ms);
                ms.Seek(0, SeekOrigin.Begin);
                using(var reader = new StreamReader(ms))
                    res = reader.ReadToEnd();
            }
            return res;
        }
        public static void DeserializeFromString(string state, Action<Stream> deserializationMethod) {
            using(var ms = new MemoryStream(Encoding.UTF8.GetBytes(state)))
                deserializationMethod(ms);
        }
        public static T DeserializeFromString<T>(string state, Func<Stream,T> deserializationMethod) {
            using(var ms = new MemoryStream(Encoding.UTF8.GetBytes(state)))
                return deserializationMethod(ms);
        }
    }
}