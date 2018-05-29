using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security;
using System.Windows;
using System.Windows.Shell;

namespace DevExpress.Mvvm.UI.Native {
    public static class TaskbarInfoApplicator {
        public static void SetTaskbarItemInfo(Window window, TaskbarItemInfo itemInfo) {
            Debug.Assert(window.TaskbarItemInfo == null, "SetTaskbarItemInfo.Assert");
            if(IsOSWindows7OrNewer && UserIsLoggedIn())
                window.TaskbarItemInfo = itemInfo;
        }
        [SecuritySafeCritical]
        static bool UserIsLoggedIn() {
            try {
                var taskbarList = (ITaskbarList)Activator.CreateInstance(Type.GetTypeFromCLSID(new Guid(TaskbarListObjectGuid)));
                try {
                    try {
                        taskbarList.HrInit();
                    } catch(NotImplementedException) {
                        return false;
                    }
                } finally {
                    Marshal.ReleaseComObject(taskbarList);
                }
            } catch(Exception e) {
                Assert(e, "SetTaskbarItemInfo.Exception");
                return false;
            }
            return true;
        }
        const string TaskbarListInterfaceGuid = "56FDF342-FD6D-11d0-958A-006097C9A090";
        const string TaskbarListObjectGuid = "56FDF344-FD6D-11d0-958A-006097C9A090";
        [SecurityCritical, SuppressUnmanagedCodeSecurity]
        [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid(TaskbarListInterfaceGuid)]
        interface ITaskbarList {
            void HrInit();
            void AddTab(IntPtr hwnd);
            void DeleteTab(IntPtr hwnd);
            void ActivateTab(IntPtr hwnd);
            void SetActiveAlt(IntPtr hwnd);
        }
        static readonly Version osVersion = Environment.OSVersion.Version;
        static bool IsOSWindows7OrNewer { get { return osVersion >= new Version(6, 1); } }
        [Conditional("DEBUG")]
        static void Assert(Exception exception, string message) {
            if(exception != null)
                throw new Exception(message, exception);
        }
    }
}