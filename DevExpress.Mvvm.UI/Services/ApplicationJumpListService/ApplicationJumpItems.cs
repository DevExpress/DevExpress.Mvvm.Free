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
using DevExpress.Mvvm.UI.Native;

namespace DevExpress.Mvvm.UI {
    public abstract class ApplicationJumpItem : DependencyObjectExt, ISupportInitialize, ICloneable, IApplicationJumpItemInfoSource, IApplicationJumpItem {
        public static ApplicationJumpItemInfo GetItemInfo(ApplicationJumpItem item) { return item.ItemInfo; }
        public static ApplicationJumpItem GetItem(ApplicationJumpItemInfo itemInfo) {
            IApplicationJumpItemInfoInternal itemInfoInternal = itemInfo;
            ApplicationJumpItem item = (ApplicationJumpItem)itemInfoInternal.Source;
            if(item != null) return item;
            ApplicationJumpPathInfo jumpPathInfo = itemInfo as ApplicationJumpPathInfo;
            if(jumpPathInfo != null)
                return new ApplicationJumpPath(jumpPathInfo);
            ApplicationJumpTaskInfo jumpTaskInfo = itemInfo as ApplicationJumpTaskInfo;
            if(jumpTaskInfo != null)
                return new ApplicationJumpTask(jumpTaskInfo);
            throw new ArgumentException("itemInfo");
        }

        ApplicationJumpItemInfo itemInfo;

        public ApplicationJumpItem(ApplicationJumpItemInfo itemInfo) {
            if(itemInfo == null) throw new ArgumentNullException("itemInfo");
            ItemInfo = itemInfo;
        }
        public string CustomCategory {
            get { return ItemInfo.CustomCategory; }
            set { ItemInfo.CustomCategory = value; }
        }
        public new ApplicationJumpItem Clone() {
            ApplicationJumpItem item = CreateInstanceCore();
            CloneCore(item);
            return item;
        }
        protected ApplicationJumpItemInfo ItemInfo {
            get { return itemInfo; }
            set {
                ApplicationJumpItemInfo oldValue = ItemInfo;
                itemInfo = value;
                OnItemInfoChanged(oldValue);
            }
        }
        protected virtual void OnItemInfoChanged(ApplicationJumpItemInfo oldItemInfo) {
            IApplicationJumpItemInfoInternal oldItemInfoInternal = oldItemInfo;
            if(oldItemInfoInternal != null)
                oldItemInfoInternal.Source = null;
            IApplicationJumpItemInfoInternal itemInfoInternal = ItemInfo;
            if(itemInfoInternal != null)
                itemInfoInternal.Source = this;
        }
        protected new abstract ApplicationJumpItem CreateInstanceCore();
        protected virtual void CloneCore(ApplicationJumpItem clone) {
            clone.ItemInfo = ItemInfo.Clone();
        }
        void ISupportInitialize.BeginInit() {
            ISupportInitialize itemInfoInitialize = ItemInfo;
            itemInfoInitialize.BeginInit();
        }
        void ISupportInitialize.EndInit() {
            ISupportInitialize itemInfoInitialize = ItemInfo;
            itemInfoInitialize.EndInit();
        }
        object ICloneable.Clone() { return Clone(); }
    }
    public class ApplicationJumpPath : ApplicationJumpItem, IApplicationJumpPath {
        public ApplicationJumpPath() : this(new ApplicationJumpPathInfo()) { }
        public ApplicationJumpPath(ApplicationJumpPathInfo itemInfo) : base(itemInfo) { }
        public string Path {
            get { return ItemInfo.Path; }
            set { ItemInfo.Path = value; }
        }
        public new ApplicationJumpPath Clone() { return (ApplicationJumpPath)base.Clone(); }
        protected new ApplicationJumpPathInfo ItemInfo { get { return (ApplicationJumpPathInfo)base.ItemInfo; } }
        protected override ApplicationJumpItem CreateInstanceCore() { return new ApplicationJumpPath(); }
    }
    public class ApplicationJumpTask : ApplicationJumpItem, IClickable, IApplicationJumpTaskInfoSource, IApplicationJumpTask {
        #region Dependency Properties
        public static readonly DependencyProperty CommandProperty =
            DependencyProperty.Register("Command", typeof(ICommand), typeof(ApplicationJumpTask), new PropertyMetadata(null));
        public static readonly DependencyProperty CommandParameterProperty =
            DependencyProperty.Register("CommandParameter", typeof(object), typeof(ApplicationJumpTask), new PropertyMetadata(null));
        #endregion

        public ApplicationJumpTask() : this(new ApplicationJumpTaskInfo()) { }
        public ApplicationJumpTask(ApplicationJumpTaskInfo itemInfo) : base(itemInfo) { }
        public string Title {
            get { return ItemInfo.Title; }
            set { ItemInfo.Title = value; }
        }
        public ImageSource Icon {
            get { return ItemInfo.Icon; }
            set { ItemInfo.Icon = value; }
        }
        public string IconResourcePath {
            get { return ItemInfo.IconResourcePath; }
            set { ItemInfo.IconResourcePath = value; }
        }
        public int IconResourceIndex {
            get { return ItemInfo.IconResourceIndex; }
            set { ItemInfo.IconResourceIndex = value; }
        }
        public string Description {
            get { return ItemInfo.Description; }
            set { ItemInfo.Description = value; }
        }
        public string ApplicationPath {
            get { return ItemInfo.ApplicationPath; }
            set { ItemInfo.ApplicationPath = value; }
        }
        public string Arguments {
            get { return ItemInfo.Arguments; }
            set { ItemInfo.Arguments = value; }
        }
        public string WorkingDirectory {
            get { return ItemInfo.WorkingDirectory; }
            set { ItemInfo.WorkingDirectory = value; }
        }
        public string CommandId {
            get { return ItemInfo.CommandId; }
            set { ItemInfo.CommandId = value; }
        }
        public ICommand Command {
            get { return (ICommand)GetValue(CommandProperty); }
            set { SetValue(CommandProperty, value); }
        }
        public object CommandParameter {
            get { return GetValue(CommandParameterProperty); }
            set { SetValue(CommandParameterProperty, value); }
        }
        public event EventHandler Click;
        public new ApplicationJumpTask Clone() { return (ApplicationJumpTask)base.Clone(); }
        protected new ApplicationJumpTaskInfo ItemInfo { get { return (ApplicationJumpTaskInfo)base.ItemInfo; } }
        protected override ApplicationJumpItem CreateInstanceCore() { return new ApplicationJumpTask(); }
        protected override void CloneCore(ApplicationJumpItem clone) {
            base.CloneCore(clone);
            ApplicationJumpTask task = (ApplicationJumpTask)clone;
            task.Click = Click;
            task.Command = Command;
            task.CommandParameter = CommandParameter;
        }
        void RaiseClickAndExecuteCommand() {
            if(Dispatcher.CheckAccess())
                RaiseClickAndExecuteCommandCore();
            else
                Dispatcher.BeginInvoke((Action)RaiseClickAndExecuteCommandCore);
        }
        void RaiseClickAndExecuteCommandCore() {
            if(Click != null)
                Click(this, EventArgs.Empty);
            if(Command != null && Command.CanExecute(CommandParameter))
                Command.Execute(CommandParameter);
        }
        Action IApplicationJumpTaskInfoSource.Action { get { return RaiseClickAndExecuteCommand; } }
        Action IApplicationJumpTask.Action {
            get { return ItemInfo.Action; }
            set { ItemInfo.Action = value; }
        }
    }
}