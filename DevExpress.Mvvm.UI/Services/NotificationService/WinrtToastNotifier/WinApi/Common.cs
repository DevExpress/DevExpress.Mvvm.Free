using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Security;
using DevExpress.Internal.WinApi.Windows.UI.Notifications;

namespace DevExpress.Internal.WinApi {
#pragma warning disable 169
    [CLSCompliant(false)]
    public struct EventRegistrationToken : IEquatable<EventRegistrationToken> {
        readonly ulong value;
        public EventRegistrationToken(ulong value) {
            this.value = value;
        }
        public bool Equals(EventRegistrationToken other) {
            return (value == other.value);
        }
        public override bool Equals(object obj) {
            return (obj is EventRegistrationToken) && Equals((EventRegistrationToken)obj);
        }
        public override int GetHashCode() {
            return value.GetHashCode();
        }
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
        public static void Safe(Action action, Action<COMException> onError = null) {
            try {
                action();
            }
            catch(COMException ce) {
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
                using(var hString_classId = HSTRING.FromString(classId))
                    return Unsafe.RoGetActivationFactory(hString_classId, guid, out iface);
            }
            internal static int RoInitialize(RO_INIT_TYPE initType) {
                return Unsafe.RoInitialize(initType);
            }
            #region SecurityCritical
            static class Unsafe {
                [DllImport("Combase.dll")]
                internal static extern Int32 RoGetActivationFactory(
                    HSTRING classId,
                    [In, MarshalAs(UnmanagedType.LPStruct)] Guid guid,
                    [Out, MarshalAs(UnmanagedType.IUnknown)] out object iface);
                [DllImport("Combase.dll")]
                internal static extern int RoInitialize(RO_INIT_TYPE initType);
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

    [StructLayout(LayoutKind.Sequential), SecuritySafeCritical]
    public struct HSTRING : IDisposable {
        readonly IntPtr handle;
        public static HSTRING FromString(string str) {
            IntPtr hMem = Marshal.AllocHGlobal(IntPtr.Size);
            try {
                Marshal.ThrowExceptionForHR(WindowsCreateString(str, str.Length, hMem));
                return Marshal.PtrToStructure<HSTRING>(hMem);
            }
            finally { Marshal.FreeHGlobal(hMem); }
        }
        public void Dispose() {
            if(handle == IntPtr.Zero)
                return;
            Marshal.ThrowExceptionForHR(WindowsDeleteString(handle));
        }
        public string GetString() {
            if(handle == IntPtr.Zero)
                return string.Empty;
            uint len = WindowsGetStringLen(handle);
            if(len > 0) {
                uint actualLen;
                return Marshal.PtrToStringUni(WindowsGetStringRawBuffer(handle, out actualLen));
            }
            return string.Empty;
        }
        [DllImport("api-ms-win-core-winrt-string-l1-1-0.dll", CallingConvention = CallingConvention.StdCall)]
        static extern int WindowsCreateString(
            [MarshalAs(UnmanagedType.LPWStr)] string sourceString, int length, [Out] IntPtr hstring);
        [DllImport("api-ms-win-core-winrt-string-l1-1-0.dll", CallingConvention = CallingConvention.StdCall)]
        static extern int WindowsDeleteString([In] IntPtr hstring);
        [DllImport("api-ms-win-core-winrt-string-l1-1-0.dll", CallingConvention = CallingConvention.StdCall)]
        static extern uint WindowsGetStringLen([In] IntPtr hstring);
        [DllImport("api-ms-win-core-winrt-string-l1-1-0.dll", CallingConvention = CallingConvention.StdCall)]
        static extern IntPtr WindowsGetStringRawBuffer([In] IntPtr hstring, [Out] out uint length);
    }
}
