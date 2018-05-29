using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Security;
using DevExpress.Internal.WinApi.Windows.UI.Notifications;

namespace DevExpress.Internal.WinApi {
#pragma warning disable 169
    [CLSCompliant(false)]
    public struct EventRegistrationToken {
        UInt64 value;
    }

    [Guid("AF86E2E0-B12D-4c6a-9C5A-D7AA65101E90")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IInspectable {
        void GetIids();
        int GetRuntimeClassName([Out, MarshalAs(ComFunctions.UnmanagedType_HString)] out string name);
        void GetTrustLevel();
    }

    public static class ComFunctions {
        #region ctor
        static Dictionary<Type, Tuple<string, Guid>> knownTypes;
        static ComFunctions() {
            knownTypes = new Dictionary<Type, Tuple<string, Guid>>();
            knownTypes[typeof(IToastNotificationFactory)] =
                Tuple.Create("Windows.UI.Notifications.ToastNotification", new Guid("04124B20-82C6-4229-B109-FD9ED4662B53"));
            knownTypes[typeof(IToastNotificationManager)] =
                Tuple.Create("Windows.UI.Notifications.ToastNotificationManager", new Guid("50AC103F-D235-4598-BBEF-98FE4D1A3AD4"));
        }
        #endregion ctor
        public const short
            UnmanagedType_HString = 47;
        public enum RO_INIT_TYPE {
            RO_INIT_MULTITHREADED = 1
        };
        public static bool Initialize() {
            try {
                ComBaseImport.RoInitialize(RO_INIT_TYPE.RO_INIT_MULTITHREADED);
                return true;
            }
            catch { return false; }
        }
        public static void CheckHRESULT(int hResult) {
            if(hResult < 0) throw new Exception("Failed with HRESULT: " + hResult.ToString("X"));
        }
        public static void Safe(Action action, Action<System.Runtime.InteropServices.COMException> onError = null) {
            try {
                action();
            }
            catch(System.Runtime.InteropServices.COMException ce) {
                if(onError != null) onError(ce);
            }
        }
        public static T RoGetActivationFactory<T>() {
            Tuple<string, Guid> tuple = knownTypes[typeof(T)];
            object iface;
            int res = ComBaseImport.RoGetActivationFactory(tuple.Item1, tuple.Item2, out iface);
            CheckHRESULT(res);
            return (T)iface;
        }
        [SecuritySafeCritical]
        static class ComBaseImport {
            internal static int RoGetActivationFactory(string classId, Guid guid, out object iface) {
                return Unsafe.RoGetActivationFactory(classId, guid, out iface);
            }
            internal static int RoInitialize(RO_INIT_TYPE initType) {
                return Unsafe.RoInitialize(initType);
            }
            #region SecurityCritical
            static class Unsafe {
                [DllImport("Combase.dll")]
                internal static extern Int32 RoGetActivationFactory(
                    [MarshalAs(ComFunctions.UnmanagedType_HString)] string classId,
                    [In, MarshalAs(UnmanagedType.LPStruct)] Guid guid,
                    [Out, MarshalAs(UnmanagedType.IUnknown)] out object iface);

                [DllImport("Combase.dll")]
                internal static extern Int32 RoInitialize(RO_INIT_TYPE initType);
            }
            #endregion SecurityCritical
        }
    }

    [CLSCompliant(false)]
    public static class ErrorHelper {
        public static void VerifySucceeded(UInt32 hResult) {
            if(hResult > 1) throw new Exception("Failed with HRESULT: " + hResult.ToString("X"));
        }
    }
}