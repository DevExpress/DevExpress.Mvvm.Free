using DevExpress.Data;
using DevExpress.Internal.WinApi;
using DevExpress.Internal.WinApi.Window.Data.Xml.Dom;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace DevExpress.Internal.WinApi.Windows.UI.Notifications {
    public enum HResult {
        NoInterface = -2147467262,
        Fail = -2147467259,
        TypeElementNotFound = -2147319765,
        AccessDenied = -2147287035,
        NoObject = -2147221019,
        OutOfMemory = -2147024882,
        InvalidArguments = -2147024809,
        ResourceInUse = -2147024726,
        ElementNotFound = -2147023728,
        Canceled = -2147023673,
        Ok = 0,
        False = 1,
        Win32ErrorCanceled = 1223,
    }

    public enum ToastTemplateType {
        ToastImageAndText01 = 0,
        ToastImageAndText02 = 1,
        ToastImageAndText03 = 2,
        ToastImageAndText04 = 3,
        ToastText01 = 4,
        ToastText02 = 5,
        ToastText03 = 6,
        ToastText04 = 7,
        ToastGeneric = 8
    }

    [ComImport]
    [Guid("3F89D935-D9CB-4538-A0F0-FFE7659938F8")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IToastDismissedEventArgs : IInspectable {
        void IInspectableStub1();
        void IInspectableStub2();
        void IInspectableStub3();
        ToastDismissalReason Reason { get; }
    }

    [ComImport]
    [Guid("35176862-CFD4-44F8-AD64-F500FD896C3B")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IToastFailedEventArgs : IInspectable {
        void IInspectableStub1();
        void IInspectableStub2();
        void IInspectableStub3();
        int Error { get; }
    }

    [CLSCompliant(false)]
    [ComImport]
    [Guid("997E2675-059E-4E60-8B06-1760917C8B80")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IToastNotification : IInspectable {
        void IInspectableStub1();
        void IInspectableStub2();
        void IInspectableStub3();
        IXmlDocument Content { get; }
        DateTime ExpirationTime { get; set; }
        [MethodImpl(MethodImplOptions.InternalCall)]
        int AddDismissed([MarshalAs(UnmanagedType.Interface)] [In] ITypedEventHandler_IToastNotification_Dismissed handler,
                         [Out] out EventRegistrationToken token);
        int RemoveDismissed([In] EventRegistrationToken token);
        int AddActivated([MarshalAs(UnmanagedType.Interface)] [In] ITypedEventHandler_IToastNotification_Activated handler,
                         [Out] out EventRegistrationToken token);
        int RemoveActivated([In] EventRegistrationToken token);
        int AddFailed([MarshalAs(UnmanagedType.Interface)] [In] ITypedEventHandler_IToastNotification_Failed handler,
                       [Out] out EventRegistrationToken token);
        int RemoveFailed([In] EventRegistrationToken token);
    }

    [ComImport]
    [Guid("75927B93-03F3-41EC-91D3-6E5BAC1B38E7")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    interface IToastNotifier : IInspectable {
        void IInspectableStub1();
        void IInspectableStub2();
        void IInspectableStub3();
        int Show(IToastNotification notification);
        void Hide(IToastNotification notification);
        NotificationSetting Setting { get; }
        int AddToSchedule(IToastNotification scheduledToast);
        int RemoveFromSchedule(IToastNotification scheduledToast);
        HResult GetScheduledToastNotifications([Out] out IVectorView_ToastNotification toasts);
    }

    [ComImport]
    [Guid("ba0aff1f-6a8a-5a7e-a9f7-505b6266a436")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    interface IVectorView_ToastNotification {
        void IInspectableStub1();
        void IInspectableStub2();
        void IInspectableStub3();
        HResult GetAt(uint index, [Out] out IToastNotification value);
        uint Size { get; }
        HResult IndexOf(IToastNotification value, [Out] out uint index, [Out] out bool found);
        HResult GetMany();
    }

    [ComImport]
    [Guid("50AC103F-D235-4598-BBEF-98FE4D1A3AD4")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    interface IToastNotificationManager : IInspectable {
        void IInspectableStub1();
        void IInspectableStub2();
        void IInspectableStub3();
        void f6();
        int CreateToastNotifierWithId([In, MarshalAs(47)] string applicationId, out IToastNotifier notifier);
        IXmlDocument GetTemplateContent(ToastTemplateType type);
    }

    [ComImport]
    [Guid("04124B20-82C6-4229-B109-FD9ED4662B53")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    interface IToastNotificationFactory : IInspectable {
        void IInspectableStub1();
        void IInspectableStub2();
        void IInspectableStub3();
        int CreateToastNotification(IXmlDocument content, out IToastNotification notification);
    }

    [CLSCompliant(false)]
    [ComImport]
    [Guid("61c2402f-0ed0-5a18-ab69-59f4aa99a368")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface ITypedEventHandler_IToastNotification_Dismissed {
        [MethodImpl(MethodImplOptions.PreserveSig)]
        int Invoke(IToastNotification sender, IToastDismissedEventArgs args);
    }

    [CLSCompliant(false)]
    [ComImport]
    [Guid("ab54de2d-97d9-5528-b6ad-105afe156530")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface ITypedEventHandler_IToastNotification_Activated {
        [MethodImpl(MethodImplOptions.PreserveSig)]
        int Invoke(IToastNotification sender, IInspectable args);
    }

    [CLSCompliant(false)]
    [ComImport]
    [Guid("95e3e803-c969-5e3a-9753-ea2ad22a9a33")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface ITypedEventHandler_IToastNotification_Failed {
        [MethodImpl(MethodImplOptions.PreserveSig)]
        int Invoke(IToastNotification sender, IToastFailedEventArgs args);
    }
    [ComImport]
    [Guid("e3bf92f3-c197-436f-8265-0625824f8dac")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IToastActivatedEventArgs : IInspectable {
        void IInspectableStub1();
        void IInspectableStub2();
        void IInspectableStub3();
        HResult GetArguments([Out, MarshalAs(47)] out string args);
    }
    [ComImport]
    [Guid("53E31837-6600-4A81-9395-75CFFE746F94")]
    [ComVisible(true)]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [CLSCompliant(false)]
    public interface INotificationActivationCallback {
        void Activate(
            [In, MarshalAs(UnmanagedType.LPWStr)] string appUserModelId,
            [In, MarshalAs(UnmanagedType.LPWStr)] string invokedArgs,
            [In, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 3)] NOTIFICATION_USER_INPUT_DATA[] data,
            [In, MarshalAs(UnmanagedType.U4)] uint count);
    }
    [StructLayout(LayoutKind.Sequential), Serializable]
    public struct NOTIFICATION_USER_INPUT_DATA {
        [MarshalAs(UnmanagedType.LPWStr)]
        public string Key;
        [MarshalAs(UnmanagedType.LPWStr)]
        public string Value;
    }
}