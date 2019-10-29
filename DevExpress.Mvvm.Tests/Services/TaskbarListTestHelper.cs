using System;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security;
using System.Windows;
using System.Windows.Interop;
using DevExpress.Mvvm.Native;

namespace DevExpress.Mvvm.UI.Tests {
    static class TaskbarListTestHelper {
        public static void SendTaskBarButtonCreated(Window window) {
            var handle = new WindowInteropHelper(window).Handle;
            var msg = (int)typeof(Window).GetField("WM_TASKBARBUTTONCREATED", BindingFlags.NonPublic | BindingFlags.Static).GetValue(null);
            SendMessage(handle, msg, IntPtr.Zero, IntPtr.Zero);
        }

        [DllImport("user32.dll")]
        static extern int SendMessage(IntPtr hWnd, int wMsg, IntPtr wParam, IntPtr lParam);

#if !DXCORE3
        public static void DoWithNotImplementedHrInit(Action action) {
            var iface = (ITaskbarList)Activator.CreateInstance(Type.GetTypeFromCLSID(new Guid("56FDF344-FD6D-11d0-958A-006097C9A090")));
            DoWithPatchedVtable(iface, x => x.HrInit(), new HrInitCallback(HrInitNotImplemented), action);
        }

        static void DoWithPatchedVtable<T>(T comInterface, Expression<Action<T>> method, Delegate func, Action action) {
            var methodInfo = ExpressionHelper.GetMethod(method);
            var funcPtr = Marshal.GetFunctionPointerForDelegate(func);
            var interfacePtr = Marshal.GetComInterfaceForObject(comInterface, typeof(T));
            var vtablePtr = Marshal.ReadIntPtr(interfacePtr);
            var slot = Marshal.GetComSlotForMethodInfo(methodInfo);
            var offset = slot * Marshal.SizeOf<IntPtr>();
            var originalFuncPtr = PatchVtable(vtablePtr, offset, funcPtr);
            try {
                action();
            } finally {
                PatchVtable(vtablePtr, offset, originalFuncPtr);
            }
            GC.KeepAlive(func);
        }
#endif

        static IntPtr PatchVtable(IntPtr vtable, int offset, IntPtr funcPtr) {
            var p = VirtualProtect(vtable, new UIntPtr((uint)Marshal.SizeOf<IntPtr>()), 0x40);
            try {
                var oldValue = Marshal.ReadIntPtr(vtable, offset);
                Marshal.WriteIntPtr(vtable, offset, funcPtr);
                return oldValue;
            } finally {
                VirtualProtect(vtable, new UIntPtr((uint)Marshal.SizeOf<IntPtr>()), p);
            }
        }

        static UInt32 HrInitNotImplemented(ITaskbarList _) {
            return E_NOTIMPL;
        }

        delegate UInt32 HrInitCallback([MarshalAs(UnmanagedType.Interface)] ITaskbarList a);

        const UInt32 E_NOTIMPL = 0x80004001;

        static uint VirtualProtect(IntPtr lpAddress, UIntPtr dwSize, uint flNewProtect) {
            uint res;
            if(!VirtualProtect(lpAddress, dwSize, flNewProtect, out res))
                throw new InvalidOperationException();
            return res;
        }

        [DllImport("kernel32.dll")]
        static extern bool VirtualProtect(IntPtr lpAddress, UIntPtr dwSize, uint flNewProtect, out uint lpflOldProtect);

        [Guid("56FDF342-FD6D-11d0-958A-006097C9A090"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown), SecurityCritical, SuppressUnmanagedCodeSecurity]
        [ComImport]
        interface ITaskbarList {
            void HrInit();
            void AddTab(IntPtr hwnd);
            void DeleteTab(IntPtr hwnd);
            void ActivateTab(IntPtr hwnd);
            void SetActiveAlt(IntPtr hwnd);
        }
    }
}