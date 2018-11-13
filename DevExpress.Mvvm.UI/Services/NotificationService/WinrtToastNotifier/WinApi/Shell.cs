using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;

namespace DevExpress.Internal.WinApi {
    enum STGM : long {
        STGM_READ = 0x00000000L,
        STGM_WRITE = 0x00000001L,
        STGM_READWRITE = 0x00000002L,
        STGM_SHARE_DENY_NONE = 0x00000040L,
        STGM_SHARE_DENY_READ = 0x00000030L,
        STGM_SHARE_DENY_WRITE = 0x00000020L,
        STGM_SHARE_EXCLUSIVE = 0x00000010L,
        STGM_PRIORITY = 0x00040000L,
        STGM_CREATE = 0x00001000L,
        STGM_CONVERT = 0x00020000L,
        STGM_FAILIFTHERE = 0x00000000L,
        STGM_DIRECT = 0x00000000L,
        STGM_TRANSACTED = 0x00010000L,
        STGM_NOSCRATCH = 0x00100000L,
        STGM_NOSNAPSHOT = 0x00200000L,
        STGM_SIMPLE = 0x08000000L,
        STGM_DIRECT_SWMR = 0x00400000L,
        STGM_DELETEONRELEASE = 0x04000000L,
    }

    static class ShellIIDGuid {
        internal const string IShellLinkW = "000214F9-0000-0000-C000-000000000046";
        internal const string CShellLink = "00021401-0000-0000-C000-000000000046";
        internal const string IPersistFile = "0000010b-0000-0000-C000-000000000046";
        internal const string IPropertyStore = "886D8EEB-8CF2-4446-8D02-CDBA1DBDCF99";
    }

    [ComImport,
    Guid(ShellIIDGuid.IShellLinkW),
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    interface IShellLinkW {
        UInt32 GetPath(
            [Out(), MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszFile,
            int cchMaxPath,
            IntPtr pfd,
            uint fFlags);
        UInt32 GetIDList(out IntPtr ppidl);
        UInt32 SetIDList(IntPtr pidl);
        UInt32 GetDescription(
            [Out(), MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszFile,
            int cchMaxName);
        UInt32 SetDescription(
            [MarshalAs(UnmanagedType.LPWStr)] string pszName);
        UInt32 GetWorkingDirectory(
            [Out(), MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszDir,
            int cchMaxPath
            );
        UInt32 SetWorkingDirectory(
            [MarshalAs(UnmanagedType.LPWStr)] string pszDir);
        UInt32 GetArguments(
            [Out(), MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszArgs,
            int cchMaxPath);
        UInt32 SetArguments(
            [MarshalAs(UnmanagedType.LPWStr)] string pszArgs);
        UInt32 GetHotKey(out short wHotKey);
        UInt32 SetHotKey(short wHotKey);
        UInt32 GetShowCmd(out uint iShowCmd);
        UInt32 SetShowCmd(uint iShowCmd);
        UInt32 GetIconLocation(
            [Out(), MarshalAs(UnmanagedType.LPWStr)] out StringBuilder pszIconPath,
            int cchIconPath,
            out int iIcon);
        UInt32 SetIconLocation(
            [MarshalAs(UnmanagedType.LPWStr)] string pszIconPath,
            int iIcon);
        UInt32 SetRelativePath(
            [MarshalAs(UnmanagedType.LPWStr)] string pszPathRel,
            uint dwReserved);
        UInt32 Resolve(IntPtr hwnd, uint fFlags);
        UInt32 SetPath(
            [MarshalAs(UnmanagedType.LPWStr)] string pszFile);
    }

    [ComImport,
    Guid(ShellIIDGuid.IPersistFile),
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    interface IPersistFile {
        UInt32 GetCurFile(
            [Out(), MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszFile
        );
        UInt32 IsDirty();
        UInt32 Load(
            [MarshalAs(UnmanagedType.LPWStr)] string pszFileName,
            [MarshalAs(UnmanagedType.U4)] STGM dwMode);
        UInt32 Save(
            [MarshalAs(UnmanagedType.LPWStr)] string pszFileName,
            bool fRemember);
        UInt32 SaveCompleted(
            [MarshalAs(UnmanagedType.LPWStr)] string pszFileName);
    }

    [ComImport]
    [Guid(ShellIIDGuid.IPropertyStore)]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    interface IPropertyStore {
        UInt32 GetCount([Out] out uint propertyCount);
        UInt32 GetAt([In] uint propertyIndex, out PropertyKey key);
        UInt32 GetValue([In] ref PropertyKey key, [Out] PropVariant pv);
        UInt32 SetValue([In] ref PropertyKey key, [In] PropVariant pv);
        UInt32 Commit();
    }

    [ComImport,
    Guid(ShellIIDGuid.CShellLink),
    ClassInterface(ClassInterfaceType.None)]
    class CShellLink { }

    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    struct PropertyKey : IEquatable<PropertyKey> {
        Guid formatId;
        Int32 propertyId;
        public Guid FormatId { get { return formatId; } }
        public Int32 PropertyId { get { return propertyId; } }
        public PropertyKey(Guid formatId, Int32 propertyId) {
            this.formatId = formatId;
            this.propertyId = propertyId;
        }
        public PropertyKey(string formatId, Int32 propertyId) {
            this.formatId = new Guid(formatId);
            this.propertyId = propertyId;
        }
        public bool Equals(PropertyKey other) {
            return other.Equals((object)this);
        }
        public override int GetHashCode() {
            return unchecked(propertyId * 5381 + formatId.GetHashCode());
        }
        public override bool Equals(object obj) {
            if(obj == null)
                return false;
            if(!(obj is PropertyKey))
                return false;
            PropertyKey other = (PropertyKey)obj;
            return other.formatId.Equals(formatId) && (other.propertyId == propertyId);
        }
        public static bool operator ==(PropertyKey propKey1, PropertyKey propKey2) {
            return propKey1.Equals(propKey2);
        }
        public static bool operator !=(PropertyKey propKey1, PropertyKey propKey2) {
            return !propKey1.Equals(propKey2);
        }
    }

    [StructLayout(LayoutKind.Explicit)]
    sealed class PropVariant : IDisposable {
        private static Dictionary<Type, Action<PropVariant, Array, uint>> _vectorActions = null;
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        private static Dictionary<Type, Action<PropVariant, Array, uint>> GenerateVectorActions() {
            Dictionary<Type, Action<PropVariant, Array, uint>> cache = new Dictionary<Type, Action<PropVariant, Array, uint>>();

            cache.Add(typeof(Int16), (pv, array, i) => {
                short val;
                PropVariantNativeMethods.PropVariantGetInt16Elem(pv, i, out val);
                array.SetValue(val, i);
            });

            cache.Add(typeof(UInt16), (pv, array, i) => {
                ushort val;
                PropVariantNativeMethods.PropVariantGetUInt16Elem(pv, i, out val);
                array.SetValue(val, i);
            });

            cache.Add(typeof(Int32), (pv, array, i) => {
                int val;
                PropVariantNativeMethods.PropVariantGetInt32Elem(pv, i, out val);
                array.SetValue(val, i);
            });

            cache.Add(typeof(UInt32), (pv, array, i) => {
                uint val;
                PropVariantNativeMethods.PropVariantGetUInt32Elem(pv, i, out val);
                array.SetValue(val, i);
            });

            cache.Add(typeof(Int64), (pv, array, i) => {
                long val;
                PropVariantNativeMethods.PropVariantGetInt64Elem(pv, i, out val);
                array.SetValue(val, i);
            });

            cache.Add(typeof(UInt64), (pv, array, i) => {
                ulong val;
                PropVariantNativeMethods.PropVariantGetUInt64Elem(pv, i, out val);
                array.SetValue(val, i);
            });

            cache.Add(typeof(DateTime), (pv, array, i) => {
                System.Runtime.InteropServices.ComTypes.FILETIME val;
                PropVariantNativeMethods.PropVariantGetFileTimeElem(pv, i, out val);

                long fileTime = GetFileTimeAsLong(ref val);

                array.SetValue(DateTime.FromFileTime(fileTime), i);
            });

            cache.Add(typeof(Boolean), (pv, array, i) => {
                bool val;
                PropVariantNativeMethods.PropVariantGetBooleanElem(pv, i, out val);
                array.SetValue(val, i);
            });

            cache.Add(typeof(Double), (pv, array, i) => {
                double val;
                PropVariantNativeMethods.PropVariantGetDoubleElem(pv, i, out val);
                array.SetValue(val, i);
            });

            cache.Add(typeof(Single), (pv, array, i) =>
            {
                float[] val = new float[1];
                Marshal.Copy(pv._ptr2, val, (int)i, 1);
                array.SetValue(val[0], (int)i);
            });

            cache.Add(typeof(Decimal), (pv, array, i) => {
                int[] val = new int[4];
                for(int a = 0; a < val.Length; a++) {
                    val[a] = Marshal.ReadInt32(pv._ptr2,
                        (int)i * sizeof(decimal) + a * sizeof(int));
                }
                array.SetValue(new decimal(val), i);
            });

            cache.Add(typeof(String), (pv, array, i) => {
                string val = string.Empty;
                PropVariantNativeMethods.PropVariantGetStringElem(pv, i, ref val);
                array.SetValue(val, i);
            });
            cache.Add(typeof(Guid), (pv, array, i) => {
                byte[] guid = new byte[16];
                Marshal.Copy(pv._ptr, guid, 0, 16);
                array.SetValue(new Guid(guid), i);
            });
            return cache;
        }
        public static PropVariant FromObject(object value) {
            if(value == null) {
                return new PropVariant();
            } else {
                var func = GetDynamicConstructor(value.GetType());
                return func(value);
            }
        }
        private static Dictionary<Type, Func<object, PropVariant>> _cache = new Dictionary<Type, Func<object, PropVariant>>();
        private static object _padlock = new object();
        private static Func<object, PropVariant> GetDynamicConstructor(Type type) {
            lock(_padlock) {
                Func<object, PropVariant> action;
                if(!_cache.TryGetValue(type, out action)) {
                    ConstructorInfo constructor = typeof(PropVariant)
                        .GetConstructor(new Type[] { type });
                    if(constructor == null) {
                        throw new NotSupportedException();
                    } else {
                        var arg = Expression.Parameter(typeof(object), "arg");
                        var create = Expression.New(constructor, Expression.Convert(arg, type));
                        action = Expression.Lambda<Func<object, PropVariant>>(create, arg).Compile();
                        _cache.Add(type, action);
                    }
                }
                return action;
            }
        }
        [FieldOffset(0)]
        decimal _decimal;
        [FieldOffset(0)]
        ushort _valueType;
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2006:UseSafeHandleToEncapsulateNativeResources")]
        [FieldOffset(12)]
        IntPtr _ptr2;
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2006:UseSafeHandleToEncapsulateNativeResources")]
        [FieldOffset(8)]
        IntPtr _ptr;
        [FieldOffset(8)]
        Int32 _int32;
        [FieldOffset(8)]
        UInt32 _uint32;
        [FieldOffset(8)]
        byte _byte;
        [FieldOffset(8)]
        sbyte _sbyte;
        [FieldOffset(8)]
        short _short;
        [FieldOffset(8)]
        ushort _ushort;
        [FieldOffset(8)]
        long _long;
        [FieldOffset(8)]
        ulong _ulong;
        [FieldOffset(8)]
        double _double;
        [FieldOffset(8)]
        float _float;

        public PropVariant() { }
        [SecuritySafeCritical]
        public PropVariant(string value) {
            if(value == null) {
                throw new ArgumentNullException("value");
            }
            _valueType = (ushort)VarEnum.VT_LPWSTR;
            _ptr = Marshal.StringToCoTaskMemUni(value);
        }
        public PropVariant(string[] value) {
            if(value == null) { throw new ArgumentNullException("value"); }

            PropVariantNativeMethods.InitPropVariantFromStringVector(value, (uint)value.Length, this);
        }
        public PropVariant(bool[] value) {
            if(value == null) { throw new ArgumentNullException("value"); }

            PropVariantNativeMethods.InitPropVariantFromBooleanVector(value, (uint)value.Length, this);
        }
        public PropVariant(short[] value) {
            if(value == null) { throw new ArgumentNullException("value"); }

            PropVariantNativeMethods.InitPropVariantFromInt16Vector(value, (uint)value.Length, this);
        }
        public PropVariant(ushort[] value) {
            if(value == null) { throw new ArgumentNullException("value"); }

            PropVariantNativeMethods.InitPropVariantFromUInt16Vector(value, (uint)value.Length, this);
        }
        public PropVariant(int[] value) {
            if(value == null) { throw new ArgumentNullException("value"); }

            PropVariantNativeMethods.InitPropVariantFromInt32Vector(value, (uint)value.Length, this);
        }
        public PropVariant(uint[] value) {
            if(value == null) { throw new ArgumentNullException("value"); }

            PropVariantNativeMethods.InitPropVariantFromUInt32Vector(value, (uint)value.Length, this);
        }
        public PropVariant(long[] value) {
            if(value == null) { throw new ArgumentNullException("value"); }

            PropVariantNativeMethods.InitPropVariantFromInt64Vector(value, (uint)value.Length, this);
        }
        public PropVariant(ulong[] value) {
            if(value == null) { throw new ArgumentNullException("value"); }

            PropVariantNativeMethods.InitPropVariantFromUInt64Vector(value, (uint)value.Length, this);
        }
        public PropVariant(double[] value) {
            if(value == null) { throw new ArgumentNullException("value"); }

            PropVariantNativeMethods.InitPropVariantFromDoubleVector(value, (uint)value.Length, this);
        }
        public PropVariant(DateTime[] value) {
            if(value == null) { throw new ArgumentNullException("value"); }
            System.Runtime.InteropServices.ComTypes.FILETIME[] fileTimeArr =
                new System.Runtime.InteropServices.ComTypes.FILETIME[value.Length];

            for(int i = 0; i < value.Length; i++) {
                fileTimeArr[i] = DateTimeToFileTime(value[i]);
            }

            PropVariantNativeMethods.InitPropVariantFromFileTimeVector(fileTimeArr, (uint)fileTimeArr.Length, this);
        }
        public PropVariant(bool value) {
            _valueType = (ushort)VarEnum.VT_BOOL;
            _int32 = (value == true) ? -1 : 0;
        }
        public PropVariant(DateTime value) {
            _valueType = (ushort)VarEnum.VT_FILETIME;

            System.Runtime.InteropServices.ComTypes.FILETIME ft = DateTimeToFileTime(value);
            PropVariantNativeMethods.InitPropVariantFromFileTime(ref ft, this);
        }
        public PropVariant(byte value) {
            _valueType = (ushort)VarEnum.VT_UI1;
            _byte = value;
        }
        public PropVariant(sbyte value) {
            _valueType = (ushort)VarEnum.VT_I1;
            _sbyte = value;
        }
        public PropVariant(short value) {
            _valueType = (ushort)VarEnum.VT_I2;
            _short = value;
        }
        public PropVariant(ushort value) {
            _valueType = (ushort)VarEnum.VT_UI2;
            _ushort = value;
        }
        public PropVariant(int value) {
            _valueType = (ushort)VarEnum.VT_I4;
            _int32 = value;
        }
        public PropVariant(uint value) {
            _valueType = (ushort)VarEnum.VT_UI4;
            _uint32 = value;
        }
        public PropVariant(decimal value) {
            _decimal = value;
            _valueType = (ushort)VarEnum.VT_DECIMAL;
        }
        public PropVariant(decimal[] value) {
            if(value == null) { throw new ArgumentNullException("value"); }

            _valueType = (ushort)(VarEnum.VT_DECIMAL | VarEnum.VT_VECTOR);
            _int32 = value.Length;
            _ptr2 = Marshal.AllocCoTaskMem(value.Length * sizeof(decimal));
            for(int i = 0; i < value.Length; i++) {
                int[] bits = decimal.GetBits(value[i]);
                Marshal.Copy(bits, 0, _ptr2, bits.Length);
            }
        }
        public PropVariant(float value) {
            _valueType = (ushort)VarEnum.VT_R4;

            _float = value;
        }
        public PropVariant(float[] value) {
            if(value == null) { throw new ArgumentNullException("value"); }

            _valueType = (ushort)(VarEnum.VT_R4 | VarEnum.VT_VECTOR);
            _int32 = value.Length;

            _ptr2 = Marshal.AllocCoTaskMem(value.Length * sizeof(float));

            Marshal.Copy(value, 0, _ptr2, value.Length);
        }
        public PropVariant(long value) {
            _long = value;
            _valueType = (ushort)VarEnum.VT_I8;
        }
        public PropVariant(ulong value) {
            _valueType = (ushort)VarEnum.VT_UI8;
            _ulong = value;
        }
        public PropVariant(double value) {
            _valueType = (ushort)VarEnum.VT_R8;
            _double = value;
        }
        [System.Security.SecuritySafeCritical]
        public PropVariant(Guid value) {
            _valueType = (ushort)VarEnum.VT_CLSID;
            byte[] guid = ((Guid)value).ToByteArray();
            _ptr = Marshal.AllocCoTaskMem(guid.Length);
            Marshal.Copy(guid, 0, _ptr, guid.Length);
        }
        internal void SetIUnknown(object value) {
            _valueType = (ushort)VarEnum.VT_UNKNOWN;
            _ptr = Marshal.GetIUnknownForObject(value);
        }
        internal void SetSafeArray(Array array) {
            if(array == null) { throw new ArgumentNullException("array"); }
            const ushort vtUnknown = 13;
            IntPtr psa = PropVariantNativeMethods.SafeArrayCreateVector(vtUnknown, 0, (uint)array.Length);

            IntPtr pvData = PropVariantNativeMethods.SafeArrayAccessData(psa);
            try
            {
                for(int i = 0; i < array.Length; ++i) {
                    object obj = array.GetValue(i);
                    IntPtr punk = (obj != null) ? Marshal.GetIUnknownForObject(obj) : IntPtr.Zero;
                    Marshal.WriteIntPtr(pvData, i * IntPtr.Size, punk);
                }
            } finally {
                PropVariantNativeMethods.SafeArrayUnaccessData(psa);
            }

            _valueType = (ushort)VarEnum.VT_ARRAY | (ushort)VarEnum.VT_UNKNOWN;
            _ptr = psa;
        }
        public VarEnum VarType {
            get { return (VarEnum)_valueType; }
            set { _valueType = (ushort)value; }
        }
        public bool IsNullOrEmpty {
            get {
                return (_valueType == (ushort)VarEnum.VT_EMPTY || _valueType == (ushort)VarEnum.VT_NULL);
            }
        }
        public object Value {
            get {
                switch((VarEnum)_valueType) {
                    case VarEnum.VT_I1:
                        return _sbyte;
                    case VarEnum.VT_UI1:
                        return _byte;
                    case VarEnum.VT_I2:
                        return _short;
                    case VarEnum.VT_UI2:
                        return _ushort;
                    case VarEnum.VT_I4:
                    case VarEnum.VT_INT:
                        return _int32;
                    case VarEnum.VT_UI4:
                    case VarEnum.VT_UINT:
                        return _uint32;
                    case VarEnum.VT_I8:
                        return _long;
                    case VarEnum.VT_UI8:
                        return _ulong;
                    case VarEnum.VT_R4:
                        return _float;
                    case VarEnum.VT_R8:
                        return _double;
                    case VarEnum.VT_BOOL:
                        return _int32 == -1;
                    case VarEnum.VT_ERROR:
                        return _long;
                    case VarEnum.VT_CY:
                        return _decimal;
                    case VarEnum.VT_DATE:
                        return DateTime.FromOADate(_double);
                    case VarEnum.VT_FILETIME:
                        return DateTime.FromFileTime(_long);
                    case VarEnum.VT_BSTR:
                        return Marshal.PtrToStringBSTR(_ptr);
                    case VarEnum.VT_BLOB:
                        return GetBlobData();
                    case VarEnum.VT_LPSTR:
                        return Marshal.PtrToStringAnsi(_ptr);
                    case VarEnum.VT_LPWSTR:
                        return Marshal.PtrToStringUni(_ptr);
                    case VarEnum.VT_UNKNOWN:
                        return Marshal.GetObjectForIUnknown(_ptr);
                    case VarEnum.VT_DISPATCH:
                        return Marshal.GetObjectForIUnknown(_ptr);
                    case VarEnum.VT_DECIMAL:
                        return _decimal;
                    case VarEnum.VT_ARRAY | VarEnum.VT_UNKNOWN:
                        return CrackSingleDimSafeArray(_ptr);
                    case (VarEnum.VT_VECTOR | VarEnum.VT_LPWSTR):
                        return GetVector<string>();
                    case (VarEnum.VT_VECTOR | VarEnum.VT_I2):
                        return GetVector<Int16>();
                    case (VarEnum.VT_VECTOR | VarEnum.VT_UI2):
                        return GetVector<UInt16>();
                    case (VarEnum.VT_VECTOR | VarEnum.VT_I4):
                        return GetVector<Int32>();
                    case (VarEnum.VT_VECTOR | VarEnum.VT_UI4):
                        return GetVector<UInt32>();
                    case (VarEnum.VT_VECTOR | VarEnum.VT_I8):
                        return GetVector<Int64>();
                    case (VarEnum.VT_VECTOR | VarEnum.VT_UI8):
                        return GetVector<UInt64>();
                    case (VarEnum.VT_VECTOR | VarEnum.VT_R4):
                        return GetVector<float>();
                    case (VarEnum.VT_VECTOR | VarEnum.VT_R8):
                        return GetVector<Double>();
                    case (VarEnum.VT_VECTOR | VarEnum.VT_BOOL):
                        return GetVector<Boolean>();
                    case (VarEnum.VT_VECTOR | VarEnum.VT_FILETIME):
                        return GetVector<DateTime>();
                    case (VarEnum.VT_VECTOR | VarEnum.VT_DECIMAL):
                        return GetVector<Decimal>();
                    case VarEnum.VT_CLSID:
                        return GetVector<Guid>();
                    default:
                        return null;
                }
            }
        }
        private static long GetFileTimeAsLong(ref System.Runtime.InteropServices.ComTypes.FILETIME val) {
            return (((long)val.dwHighDateTime) << 32) + val.dwLowDateTime;
        }
        private static System.Runtime.InteropServices.ComTypes.FILETIME DateTimeToFileTime(DateTime value) {
            long hFT = value.ToFileTime();
            System.Runtime.InteropServices.ComTypes.FILETIME ft =
                new System.Runtime.InteropServices.ComTypes.FILETIME();
            ft.dwLowDateTime = (int)(hFT & 0xFFFFFFFF);
            ft.dwHighDateTime = (int)(hFT >> 32);
            return ft;
        }
        private object GetBlobData() {
            byte[] blobData = new byte[_int32];

            IntPtr pBlobData = _ptr2;
            Marshal.Copy(pBlobData, blobData, 0, _int32);

            return blobData;
        }
        private Array GetVector<T>() {
            int count = PropVariantNativeMethods.PropVariantGetElementCount(this);
            if(count <= 0) { return null; }

            lock(_padlock) {
                if(_vectorActions == null) {
                    _vectorActions = GenerateVectorActions();
                }
            }

            Action<PropVariant, Array, uint> action;
            if(!_vectorActions.TryGetValue(typeof(T), out action)) {
                throw new InvalidCastException();
            }

            Array array = new T[count];
            for(uint i = 0; i < count; i++) {
                action(this, array, i);
            }

            return array;
        }
        private static Array CrackSingleDimSafeArray(IntPtr psa) {
            uint cDims = PropVariantNativeMethods.SafeArrayGetDim(psa);
            if(cDims != 1)
                throw new ArgumentException("psa");

            int lBound = PropVariantNativeMethods.SafeArrayGetLBound(psa, 1U);
            int uBound = PropVariantNativeMethods.SafeArrayGetUBound(psa, 1U);

            int n = uBound - lBound + 1;

            object[] array = new object[n];
            for(int i = lBound; i <= uBound; ++i) {
                array[i] = PropVariantNativeMethods.SafeArrayGetElement(psa, ref i);
            }

            return array;
        }
        [SecuritySafeCritical]
        public void Dispose() {
            PropVariantNativeMethods.PropVariantClear(this);

            GC.SuppressFinalize(this);
        }
        ~PropVariant() {
            Dispose();
        }
    }

    static class PropVariantNativeMethods {
        [DllImport("Ole32.dll", PreserveSig = false)]
        internal extern static void PropVariantClear([In, Out] PropVariant pvar);

        [DllImport("OleAut32.dll", PreserveSig = true)]
        internal extern static IntPtr SafeArrayCreateVector(ushort vt, int lowerBound, uint cElems);

        [DllImport("OleAut32.dll", PreserveSig = false)]
        internal extern static IntPtr SafeArrayAccessData(IntPtr psa);

        [DllImport("OleAut32.dll", PreserveSig = false)]
        internal extern static void SafeArrayUnaccessData(IntPtr psa);

        [DllImport("OleAut32.dll", PreserveSig = true)]
        internal extern static uint SafeArrayGetDim(IntPtr psa);

        [DllImport("OleAut32.dll", PreserveSig = false)]
        internal extern static int SafeArrayGetLBound(IntPtr psa, uint nDim);

        [DllImport("OleAut32.dll", PreserveSig = false)]
        internal extern static int SafeArrayGetUBound(IntPtr psa, uint nDim);
        [DllImport("OleAut32.dll", PreserveSig = false)]
        [return: MarshalAs(UnmanagedType.IUnknown)]
        internal extern static object SafeArrayGetElement(IntPtr psa, ref int rgIndices);

        [DllImport("propsys.dll", CharSet = CharSet.Unicode, SetLastError = true, PreserveSig = false)]
        internal static extern void InitPropVariantFromPropVariantVectorElem([In] PropVariant propvarIn, uint iElem, [Out] PropVariant ppropvar);

        [DllImport("propsys.dll", CharSet = CharSet.Unicode, SetLastError = true, PreserveSig = false)]
        internal static extern void InitPropVariantFromFileTime([In] ref System.Runtime.InteropServices.ComTypes.FILETIME pftIn, [Out] PropVariant ppropvar);

        [DllImport("propsys.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.I4)]
        internal static extern int PropVariantGetElementCount([In] PropVariant propVar);

        [DllImport("propsys.dll", CharSet = CharSet.Unicode, SetLastError = true, PreserveSig = false)]
        internal static extern void PropVariantGetBooleanElem([In] PropVariant propVar, [In]uint iElem, [Out, MarshalAs(UnmanagedType.Bool)] out bool pfVal);

        [DllImport("propsys.dll", CharSet = CharSet.Unicode, SetLastError = true, PreserveSig = false)]
        internal static extern void PropVariantGetInt16Elem([In] PropVariant propVar, [In] uint iElem, [Out] out short pnVal);

        [DllImport("propsys.dll", CharSet = CharSet.Unicode, SetLastError = true, PreserveSig = false)]
        internal static extern void PropVariantGetUInt16Elem([In] PropVariant propVar, [In] uint iElem, [Out] out ushort pnVal);

        [DllImport("propsys.dll", CharSet = CharSet.Unicode, SetLastError = true, PreserveSig = false)]
        internal static extern void PropVariantGetInt32Elem([In] PropVariant propVar, [In] uint iElem, [Out] out int pnVal);

        [DllImport("propsys.dll", CharSet = CharSet.Unicode, SetLastError = true, PreserveSig = false)]
        internal static extern void PropVariantGetUInt32Elem([In] PropVariant propVar, [In] uint iElem, [Out] out uint pnVal);

        [DllImport("propsys.dll", CharSet = CharSet.Unicode, SetLastError = true, PreserveSig = false)]
        internal static extern void PropVariantGetInt64Elem([In] PropVariant propVar, [In] uint iElem, [Out] out Int64 pnVal);

        [DllImport("propsys.dll", CharSet = CharSet.Unicode, SetLastError = true, PreserveSig = false)]
        internal static extern void PropVariantGetUInt64Elem([In] PropVariant propVar, [In] uint iElem, [Out] out UInt64 pnVal);

        [DllImport("propsys.dll", CharSet = CharSet.Unicode, SetLastError = true, PreserveSig = false)]
        internal static extern void PropVariantGetDoubleElem([In] PropVariant propVar, [In] uint iElem, [Out] out double pnVal);

        [DllImport("propsys.dll", CharSet = CharSet.Unicode, SetLastError = true, PreserveSig = false)]
        internal static extern void PropVariantGetFileTimeElem([In] PropVariant propVar, [In] uint iElem, [Out, MarshalAs(UnmanagedType.Struct)] out System.Runtime.InteropServices.ComTypes.FILETIME pftVal);

        [DllImport("propsys.dll", CharSet = CharSet.Unicode, SetLastError = true, PreserveSig = false)]
        internal static extern void PropVariantGetStringElem([In] PropVariant propVar, [In]  uint iElem, [MarshalAs(UnmanagedType.LPWStr)] ref string ppszVal);

        [DllImport("propsys.dll", CharSet = CharSet.Unicode, SetLastError = true, PreserveSig = false)]
        internal static extern void InitPropVariantFromBooleanVector([In, MarshalAs(UnmanagedType.LPArray)] bool[] prgf, uint cElems, [Out] PropVariant ppropvar);

        [DllImport("propsys.dll", CharSet = CharSet.Unicode, SetLastError = true, PreserveSig = false)]
        internal static extern void InitPropVariantFromInt16Vector([In, Out] Int16[] prgn, uint cElems, [Out] PropVariant ppropvar);

        [DllImport("propsys.dll", CharSet = CharSet.Unicode, SetLastError = true, PreserveSig = false)]
        internal static extern void InitPropVariantFromUInt16Vector([In, Out] UInt16[] prgn, uint cElems, [Out] PropVariant ppropvar);

        [DllImport("propsys.dll", CharSet = CharSet.Unicode, SetLastError = true, PreserveSig = false)]
        internal static extern void InitPropVariantFromInt32Vector([In, Out] Int32[] prgn, uint cElems, [Out] PropVariant propVar);

        [DllImport("propsys.dll", CharSet = CharSet.Unicode, SetLastError = true, PreserveSig = false)]
        internal static extern void InitPropVariantFromUInt32Vector([In, Out] UInt32[] prgn, uint cElems, [Out] PropVariant ppropvar);

        [DllImport("propsys.dll", CharSet = CharSet.Unicode, SetLastError = true, PreserveSig = false)]
        internal static extern void InitPropVariantFromInt64Vector([In, Out] Int64[] prgn, uint cElems, [Out] PropVariant ppropvar);

        [DllImport("propsys.dll", CharSet = CharSet.Unicode, SetLastError = true, PreserveSig = false)]
        internal static extern void InitPropVariantFromUInt64Vector([In, Out] UInt64[] prgn, uint cElems, [Out] PropVariant ppropvar);

        [DllImport("propsys.dll", CharSet = CharSet.Unicode, SetLastError = true, PreserveSig = false)]
        internal static extern void InitPropVariantFromDoubleVector([In, Out] double[] prgn, uint cElems, [Out] PropVariant propvar);

        [DllImport("propsys.dll", CharSet = CharSet.Unicode, SetLastError = true, PreserveSig = false)]
        internal static extern void InitPropVariantFromFileTimeVector([In, Out] System.Runtime.InteropServices.ComTypes.FILETIME[] prgft, uint cElems, [Out] PropVariant ppropvar);

        [DllImport("propsys.dll", CharSet = CharSet.Unicode, SetLastError = true, PreserveSig = false)]
        internal static extern void InitPropVariantFromStringVector([In, Out] string[] prgsz, uint cElems, [Out] PropVariant ppropvar);
    }
}