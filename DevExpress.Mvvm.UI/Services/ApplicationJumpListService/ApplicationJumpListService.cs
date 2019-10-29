#if !DXCORE3

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Shell;
using DevExpress.Mvvm.Native;
using DevExpress.Mvvm.UI.Interactivity;
using DevExpress.Mvvm.UI.Interactivity.Internal;
using DevExpress.Mvvm.UI.Native;
using DevExpress.Utils;
using DevExpress.Internal;
using System.Windows.Markup;

namespace DevExpress.Mvvm.UI {
    [TargetTypeAttribute(typeof(UserControl))]
    [TargetTypeAttribute(typeof(Window))]
    [ContentProperty("Items")]
    public class ApplicationJumpListService : ServiceBase, IApplicationJumpListService, IApplicationJumpListImplementation {
        public const string CommandLineArgumentDefaultPrefix = "/APPLICATION_JUMP_TASK=";
        #region Dependency Properties
        public static readonly DependencyProperty CommandLineArgumentPrefixProperty =
            DependencyProperty.Register("CommandLineArgumentPrefix", typeof(string), typeof(ApplicationJumpListService), new PropertyMetadata(CommandLineArgumentDefaultPrefix));
        public static readonly DependencyProperty DefaultLauncherStorageFolderProperty =
            DependencyProperty.Register("DefaultLauncherStorageFolder", typeof(string), typeof(ApplicationJumpListService), new PropertyMetadata(NativeResourceManager.ResourcesFolder));
        public static readonly DependencyProperty CustomLauncherPathProperty =
            DependencyProperty.Register("CustomLauncherPath", typeof(string), typeof(ApplicationJumpListService), new PropertyMetadata(null));
        public static readonly DependencyProperty IconStorageFolderProperty =
            DependencyProperty.Register("IconStorageFolder", typeof(string), typeof(ApplicationJumpListService), new PropertyMetadata(NativeResourceManager.ResourcesFolder));
        public static readonly DependencyProperty IconStorageProperty =
            DependencyProperty.Register("IconStorage", typeof(IIconStorage), typeof(ApplicationJumpListService), new PropertyMetadata(null, null, (d, v) => v ?? ((ApplicationJumpListService)d).CreateDefaultIconStorage()));

        [IgnoreDependencyPropertiesConsistencyChecker]
        static readonly DependencyProperty InternalItemsProperty =
            DependencyProperty.RegisterAttached("InternalItems", typeof(FreezableCollection<ApplicationJumpItem>), typeof(ApplicationJumpListService), new PropertyMetadata(null));
        static FreezableCollection<ApplicationJumpItem> GetInternalItems(DependencyObject obj) {
            return (FreezableCollection<ApplicationJumpItem>)obj.GetValue(InternalItemsProperty);
        }
        static void SetInternalItems(DependencyObject obj, FreezableCollection<ApplicationJumpItem> value) {
            obj.SetValue(InternalItemsProperty, value);
        }
        #endregion

        static NativeJumpList currentNativeJumpList = new NativeJumpList(ApplicationJumpItemWrapper.GetJumpItemCommandId);
        INativeJumpList nativeJumpList;
        IJumpActionsManager jumpActionsManager;
        bool designModeShowFrequentCategory;
        bool designModeShowRecentCategory;
        List<ApplicationJumpItemInfo> designModeItems = new List<ApplicationJumpItemInfo>();
        ApplicationJumpList jumpList;

        public ApplicationJumpListService() : this(null, null) { }
        protected ApplicationJumpListService(INativeJumpList nativeJumpList, IJumpActionsManager jumpActionsManager) {
            IconStorage = CreateDefaultIconStorage();
            jumpList = new ApplicationJumpList(this);
            Items = new ApplicationJumpItemCollectionInternal(this);
            if(InteractionHelper.IsInDesignMode(this)) return;
            this.nativeJumpList = nativeJumpList ?? currentNativeJumpList;
            this.jumpActionsManager = jumpActionsManager ?? JumpActionsManager.Current;
        }
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public ApplicationJumpItemCollection Items { get; private set; }
        public string CommandLineArgumentPrefix { get { return (string)GetValue(CommandLineArgumentPrefixProperty); } set { SetValue(CommandLineArgumentPrefixProperty, value); } }
        public string DefaultLauncherStorageFolder { get { return (string)GetValue(DefaultLauncherStorageFolderProperty); } set { SetValue(DefaultLauncherStorageFolderProperty, value); } }
        public string CustomLauncherPath { get { return (string)GetValue(CustomLauncherPathProperty); } set { SetValue(CustomLauncherPathProperty, value); } }
        public IIconStorage IconStorage { get { return (IIconStorage)GetValue(IconStorageProperty); } set { SetValue(IconStorageProperty, value); } }
        public string IconStorageFolder { get { return (string)GetValue(IconStorageFolderProperty); } set { SetValue(IconStorageFolderProperty, value); } }
        public virtual bool ShowFrequentCategory {
            get {
                return InteractionHelper.IsInDesignMode(this) ? designModeShowFrequentCategory : nativeJumpList.ShowFrequentCategory;
            }
            set {
                if(InteractionHelper.IsInDesignMode(this))
                    designModeShowFrequentCategory = value;
                else
                    nativeJumpList.ShowFrequentCategory = value;
            }
        }
        public virtual bool ShowRecentCategory {
            get {
                return InteractionHelper.IsInDesignMode(this) ? designModeShowRecentCategory : nativeJumpList.ShowRecentCategory;
            }
            set {
                if(InteractionHelper.IsInDesignMode(this))
                    designModeShowRecentCategory = value;
                else
                    nativeJumpList.ShowRecentCategory = value;
            }
        }
        public virtual void AddToRecentCategory(string jumpPath) {
            if(InteractionHelper.IsInDesignMode(this)) return;
            nativeJumpList.AddToRecentCategory(jumpPath);
        }
        public virtual IEnumerable<RejectedApplicationJumpItem> Apply() {
            if(InteractionHelper.IsInDesignMode(this)) return new RejectedApplicationJumpItem[] { };
            jumpActionsManager.BeginUpdate();
            try {
                foreach(JumpItem jumpItem in nativeJumpList) {
                    ApplicationJumpItemWrapper.FillWrapProperties(jumpItem);
                    ApplicationJumpTaskWrap jumpTask = jumpItem as ApplicationJumpTaskWrap;
                    if(jumpTask != null)
                        jumpActionsManager.RegisterAction(jumpTask, CommandLineArgumentPrefix, GetLauncherPath);
                }
                IEnumerable<Tuple<JumpItem, JumpItemRejectionReason>> nativeRejectedItems = nativeJumpList.Apply() ?? new Tuple<JumpItem, JumpItemRejectionReason>[] { };
                return nativeRejectedItems.Select(i => new RejectedApplicationJumpItem(ApplicationJumpItemWrapper.Unwrap(i.Item1), i.Item2)).ToArray();
            } finally {
                jumpActionsManager.EndUpdate();
            }
        }
        protected virtual ApplicationJumpTaskInfo Find(string commandId) {
            if(InteractionHelper.IsInDesignMode(this)) return null;
            ApplicationJumpTaskWrap task = (ApplicationJumpTaskWrap)nativeJumpList.Find(commandId);
            return task == null ? null : task.ApplicationJumpTask;
        }
        public virtual bool AddOrReplace(ApplicationJumpTaskInfo jumpTask) {
            if(InteractionHelper.IsInDesignMode(this)) {
                designModeItems.Add(jumpTask);
                return true;
            }
            ApplicationJumpTaskWrap jumpTaskWrap = new ApplicationJumpTaskWrap(jumpTask);
            PrepareTask(jumpTaskWrap);
            ISupportInitialize itemInit = jumpTask;
            itemInit.EndInit();
            JumpItem existingItem = nativeJumpList.Find(jumpTask.CommandId);
            if(existingItem != null) {
                nativeJumpList[nativeJumpList.IndexOf(existingItem)] = jumpTaskWrap;
                return false;
            }
            nativeJumpList.Add(jumpTaskWrap);
            return true;
        }
        protected override void OnAttached() {
            if(Items.SourceItems.Any())
                Apply();
            base.OnAttached();
            AssociatedObject.SetValue(InternalItemsProperty, Items.SourceItems);
        }
        protected override void OnDetaching() {
            AssociatedObject.SetValue(InternalItemsProperty, null);
            base.OnDetaching();
        }
        protected virtual void AddItem(ApplicationJumpItemInfo item) {
            if(InteractionHelper.IsInDesignMode(this)) {
                designModeItems.Add(item);
                return;
            }
            nativeJumpList.Add(PrepareItem(item, null));
        }
        protected virtual void InsertItem(int index, ApplicationJumpItemInfo item) {
            if(InteractionHelper.IsInDesignMode(this)) {
                designModeItems.Insert(index, item);
                return;
            }
            nativeJumpList.Insert(index, PrepareItem(item, null));
        }
        protected virtual void SetItem(int index, ApplicationJumpItemInfo item) {
            if(InteractionHelper.IsInDesignMode(this)) {
                designModeItems[index] = item;
                return;
            }
            nativeJumpList[index] = PrepareItem(item, nativeJumpList[index]);
        }
        protected virtual bool RemoveItem(ApplicationJumpItemInfo item) {
            if(InteractionHelper.IsInDesignMode(this))
                return designModeItems.Remove(item);
            return nativeJumpList.Remove(ApplicationJumpItemWrapper.Wrap(item));
        }
        protected virtual void RemoveItemAt(int index) {
            if(InteractionHelper.IsInDesignMode(this)) {
                designModeItems.RemoveAt(index);
                return;
            }
            nativeJumpList.RemoveAt(index);
        }
        protected virtual void ClearItems() {
            if(InteractionHelper.IsInDesignMode(this)) {
                designModeItems.Clear();
                return;
            }
            nativeJumpList.Clear();
        }
        protected virtual ApplicationJumpItemInfo GetItem(int index) {
            if(InteractionHelper.IsInDesignMode(this))
                return designModeItems[index];
            return ApplicationJumpItemWrapper.Unwrap(nativeJumpList[index]);
        }
        protected virtual int IndexOfItem(ApplicationJumpItemInfo item) {
            if(InteractionHelper.IsInDesignMode(this))
                return designModeItems.IndexOf(item);
            return nativeJumpList.IndexOf(ApplicationJumpItemWrapper.Wrap(item));
        }
        protected virtual bool ContainsItem(ApplicationJumpItemInfo item) {
            if(InteractionHelper.IsInDesignMode(this))
                return designModeItems.Contains(item);
            return nativeJumpList.Contains(ApplicationJumpItemWrapper.Wrap(item));
        }
        protected virtual IEnumerable<ApplicationJumpItemInfo> GetItems() {
            if(InteractionHelper.IsInDesignMode(this))
                return designModeItems;
            return nativeJumpList.Select(i => ApplicationJumpItemWrapper.Unwrap(i));
        }
        protected virtual int ItemsCount() {
            if(InteractionHelper.IsInDesignMode(this))
                return designModeItems.Count;
            return nativeJumpList.Count;
        }
        protected virtual string GetLauncherPath() {
            if(!string.IsNullOrEmpty(CustomLauncherPath)) return NativeResourceManager.ExpandVariables(CustomLauncherPath);
            string filePath = Path.Combine(NativeResourceManager.ExpandVariables(DefaultLauncherStorageFolder), "DevExpress.Mvvm.UI.ApplicationJumpTaskLauncher" + AssemblyInfo.VSuffix + ".exe");
            if(File.Exists(filePath) && NativeResourceManager.GetFileTime(filePath) > NativeResourceManager.GetApplicationCreateTime()) return filePath;
            Directory.CreateDirectory(Path.GetDirectoryName(filePath));
            Stream stream = AssemblyHelper.GetEmbeddedResourceStream(typeof(JumpActionsManager).Assembly, "DevExpress.Mvvm.UI.ApplicationJumpTaskLauncher.exe", true);
            try {
                File.WriteAllBytes(filePath, StreamHelper.CopyAllBytes(stream));
            } catch(IOException) { } catch(UnauthorizedAccessException) { }
            return filePath;
        }
        JumpItem PrepareItem(ApplicationJumpItemInfo item, JumpItem itemToReplace) {
            JumpItem itemWrap = ApplicationJumpItemWrapper.Wrap(item);
            ApplicationJumpTaskWrap task = itemWrap as ApplicationJumpTaskWrap;
            if(task != null) {
                PrepareTask(task);
                JumpItem existingItem = nativeJumpList.Find(task.ApplicationJumpTask.CommandId);
                if(existingItem != null && existingItem != itemToReplace)
                    throw new ApplicationJumpTaskDuplicateCommandIdException();
            }
            ISupportInitialize itemInit = item;
            itemInit.EndInit();
            return itemWrap;
        }
        void PrepareTask(ApplicationJumpTaskWrap task) {
            IApplicationJumpTaskInfoInternal taskInternal = task.ApplicationJumpTask;
            if(taskInternal.IsInitialized) return;
            string iconResourceName;
            if(task.ApplicationJumpTask.Icon == null) {
                task.IconResourcePath = task.ApplicationJumpTask.IconResourcePath;
                task.IconResourceIndex = task.ApplicationJumpTask.IconResourceIndex;
                iconResourceName = string.Format("{0}_{1}", task.IconResourcePath, task.IconResourceIndex);
            } else {
                if(task.ApplicationJumpTask.IconResourcePath != null)
                    throw new ApplicationJumpTaskBothIconAndIconResourcePathSpecifiedException();
                string iconResourcePath;
                if(!IconStorage.TryStoreIconToFile(task.ApplicationJumpTask.Icon, NativeResourceManager.ExpandVariables(IconStorageFolder), out iconResourceName, out iconResourcePath))
                    throw new ApplicationJumpTaskInvalidIconException();
                task.IconResourcePath = iconResourcePath;
                task.IconResourceIndex = 0;
            }
            if(task.ApplicationJumpTask.CommandId == null)
                taskInternal.SetAutoGeneratedCommandId(string.Format("{0}${1}${2}", task.ApplicationJumpTask.CustomCategory, task.ApplicationJumpTask.Title, iconResourceName));
        }
        IIconStorage CreateDefaultIconStorage() {
            return new IconStorage(GetBaseUri);
        }
        #region IApplicationJumpListService
        IApplicationJumpList IApplicationJumpListService.Items { get { return jumpList; } }
        #endregion
        #region IApplicationJumpListImplementation
        ApplicationJumpTaskInfo IApplicationJumpListImplementation.Find(string commandId) { return Find(commandId); }
        bool IApplicationJumpListImplementation.AddOrReplace(ApplicationJumpTaskInfo jumpTask) { return AddOrReplace(jumpTask); }
        void IApplicationJumpListImplementation.AddItem(ApplicationJumpItemInfo item) { AddItem(item); }
        void IApplicationJumpListImplementation.InsertItem(int index, ApplicationJumpItemInfo item) { InsertItem(index, item); }
        void IApplicationJumpListImplementation.SetItem(int index, ApplicationJumpItemInfo item) { SetItem(index, item); }
        bool IApplicationJumpListImplementation.RemoveItem(ApplicationJumpItemInfo item) { return RemoveItem(item); }
        void IApplicationJumpListImplementation.RemoveItemAt(int index) { RemoveItemAt(index); }
        void IApplicationJumpListImplementation.ClearItems() { ClearItems(); }
        ApplicationJumpItemInfo IApplicationJumpListImplementation.GetItem(int index) { return GetItem(index); }
        int IApplicationJumpListImplementation.IndexOfItem(ApplicationJumpItemInfo item) { return IndexOfItem(item); }
        bool IApplicationJumpListImplementation.ContainsItem(ApplicationJumpItemInfo item) { return ContainsItem(item); }
        IEnumerable<ApplicationJumpItemInfo> IApplicationJumpListImplementation.GetItems() { return GetItems(); }
        int IApplicationJumpListImplementation.ItemsCount() { return ItemsCount(); }
        #endregion
    }
}
#endif