using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shell;
using DevExpress.Mvvm.Native;
using System.Windows;
using System.ComponentModel;

namespace DevExpress.Mvvm {
    public interface IApplicationJumpListService {
        bool ShowFrequentCategory { get; set; }
        bool ShowRecentCategory { get; set; }
        void AddToRecentCategory(string jumpPath);
        IApplicationJumpList Items { get; }
        IEnumerable<RejectedApplicationJumpItem> Apply();
    }
    public interface IApplicationJumpList : IList<ApplicationJumpItemInfo> {
        ApplicationJumpTaskInfo Find(string commandId);
        bool AddOrReplace(ApplicationJumpTaskInfo jumpTask);
    }
    public sealed class RejectedApplicationJumpItem {
        public RejectedApplicationJumpItem(ApplicationJumpItemInfo jumpItem, JumpItemRejectionReason reason) {
            JumpItem = jumpItem;
            Reason = reason;
        }
        public ApplicationJumpItemInfo JumpItem { get; private set; }
        public JumpItemRejectionReason Reason { get; private set; }
    }
    public abstract class InvalidApplicationJumpItemException : Exception { }
    public class ApplicationJumpTaskInvalidIconException : InvalidApplicationJumpItemException { }
    public class ApplicationJumpTaskBothIconAndIconResourcePathSpecifiedException : InvalidApplicationJumpItemException { }
    public class ApplicationJumpTaskDuplicateCommandIdException : InvalidApplicationJumpItemException { }
    public static class ApplicationJumpListExtensions {
        public static void AddRange(this IApplicationJumpList jumpList, IEnumerable<ApplicationJumpItemInfo> jumpItems) {
            if(jumpList == null) throw new ArgumentNullException("jumpList");
            List<Exception> exceptions = null;
            foreach(ApplicationJumpItemInfo jumpItem in jumpItems) {
                try {
                    jumpList.Add(jumpItem);
                } catch(InvalidApplicationJumpItemException e) {
                    if(exceptions == null)
                        exceptions = new List<Exception>();
                    exceptions.Add(e);
                }
            }
            if(exceptions != null)
                throw new InvalidOperationException("", new AggregateException(exceptions.ToArray()));
        }
        public static ApplicationJumpPathInfo Add(this IApplicationJumpList jumpList, string jumpPath) {
            try {
                return jumpList.Add(null, jumpPath);
            } catch(InvalidApplicationJumpItemException e) {
                throw new InvalidOperationException("", e);
            }
        }
        public static ApplicationJumpPathInfo Add(this IApplicationJumpList jumpList, string customCategory, string jumpPath) {
            if(jumpList == null) throw new ArgumentNullException("jumpList");
            ApplicationJumpPathInfo jumpItem = new ApplicationJumpPathInfo() { Path = jumpPath, CustomCategory = customCategory };
            try {
                jumpList.Add(jumpItem);
            } catch(InvalidApplicationJumpItemException e) {
                throw new InvalidOperationException("", e);
            }
            return jumpItem;
        }
        public static ApplicationJumpTaskInfo Add(this IApplicationJumpList jumpList, string title, ImageSource icon, Action action, string commandId = null) {
            return jumpList.Add(null, title, icon, null, action, commandId);
        }
        public static ApplicationJumpTaskInfo Add(this IApplicationJumpList jumpList, string title, ImageSource icon, string description, Action action, string commandId = null) {
            return jumpList.Add(null, title, icon, description, action, commandId);
        }
        public static ApplicationJumpTaskInfo Add(this IApplicationJumpList jumpList, string customCategory, string title, ImageSource icon, Action action, string commandId = null) {
            return jumpList.Add(customCategory, title, icon, null, action, commandId);
        }
        public static ApplicationJumpTaskInfo Add(this IApplicationJumpList jumpList, string customCategory, string title, ImageSource icon, string description, Action action, string commandId = null) {
            if(jumpList == null) throw new ArgumentNullException("jumpList");
            ApplicationJumpTaskInfo jumpItem = new ApplicationJumpTaskInfo() { CustomCategory = customCategory, Title = title, Icon = icon, Description = description, CommandId = commandId, Action = action };
            try {
                jumpList.Add(jumpItem);
            } catch(ApplicationJumpTaskInvalidIconException e) {
                throw new ArgumentException("", "icon", e);
            } catch(InvalidApplicationJumpItemException e) {
                throw new InvalidOperationException("", e);
            }
            return jumpItem;
        }
        public static ApplicationJumpTaskInfo AddOrReplace(this IApplicationJumpList jumpList, string title, ImageSource icon, Action action, string commandId = null) {
            return jumpList.AddOrReplace(null, title, icon, null, action, commandId);
        }
        public static ApplicationJumpTaskInfo AddOrReplace(this IApplicationJumpList jumpList, string title, ImageSource icon, string description, Action action, string commandId = null) {
            return jumpList.AddOrReplace(null, title, icon, description, action, commandId);
        }
        public static ApplicationJumpTaskInfo AddOrReplace(this IApplicationJumpList jumpList, string customCategory, string title, ImageSource icon, Action action, string commandId = null) {
            return jumpList.AddOrReplace(customCategory, title, icon, null, action, commandId);
        }
        public static ApplicationJumpTaskInfo AddOrReplace(this IApplicationJumpList jumpList, string customCategory, string title, ImageSource icon, string description, Action action, string commandId = null) {
            if(jumpList == null) throw new ArgumentNullException("jumpList");
            ApplicationJumpTaskInfo jumpItem = new ApplicationJumpTaskInfo() { CustomCategory = customCategory, Title = title, Icon = icon, Description = description, CommandId = commandId, Action = action };
            try {
                jumpList.AddOrReplace(jumpItem);
            } catch(ApplicationJumpTaskInvalidIconException e) {
                throw new ArgumentException("", "icon", e);
            } catch(InvalidApplicationJumpItemException e) {
                throw new InvalidOperationException("", e);
            }
            return jumpItem;
        }
    }
}