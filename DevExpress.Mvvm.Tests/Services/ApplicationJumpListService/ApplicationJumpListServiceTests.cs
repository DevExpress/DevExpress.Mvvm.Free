using DevExpress.Internal;
using DevExpress.Mvvm.Native;
using DevExpress.Mvvm.UI.Interactivity;
using DevExpress.Mvvm.UI.Native;
using DevExpress.Utils;
using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shell;
using System.Reflection;

namespace DevExpress.Mvvm.UI.Tests {
    public static class ApplicationJumpListServiceTestsImageSourceHelper {
        public static void RegisterPackScheme() {
            new System.Windows.Documents.FlowDocument();
        }
        public static ImageSource GetImageSource(Uri uri) {
            if(uri == null) return null;
            try {
#if SILVERLIGHT
                return new BitmapImage(uri);
#else
                return BitmapFrame.Create(uri);
#endif
            } catch {
                return null;
            }
        }
        public static ImageSource GetImageSource(Stream stream) {
            if(stream == null) return null;
            try {
#if SILVERLIGHT
                BitmapImage bi = new BitmapImage();
                bi.SetSource(stream);
                return bi;
#else
                return BitmapFrame.Create(stream);
#endif
            } catch {
                return null;
            }
        }
    }
    [TestFixture]
    public class ApplicationJumpListServiceTests : BaseWpfFixture {
        TestJumpActionsManager jumpActionsManager;
        TestNativeJumpList nativeJumpList;
        IApplicationJumpListService applicationJumpListService;

        protected override void SetUpCore() {
            base.SetUpCore();
            ApplicationJumpListServiceTestsImageSourceHelper.RegisterPackScheme();
            NativeResourceManager.CompanyNameOverride = "DevExpress Tests";
            NativeResourceManager.ProductNameOverride = "DevExpress.Xpf.Core Tests";
            NativeResourceManager.VersionOverride = AssemblyInfo.Version;
            nativeJumpList = new TestNativeJumpList();
            jumpActionsManager = new TestJumpActionsManager();
            applicationJumpListService = new TestApplicationJumpListService(nativeJumpList, jumpActionsManager);
            Assert.IsNotNull(applicationJumpListService);
            applicationJumpListService.Items.Clear();
        }
        protected override void TearDownCore() {
            string resourcesFolder = NativeResourceManager.ExpandVariables(NativeResourceManager.ResourcesFolder);
            if(Directory.Exists(resourcesFolder))
                Directory.Delete(resourcesFolder, true);
            NativeResourceManager.ProductNameOverride = null;
            NativeResourceManager.CompanyNameOverride = null;
            NativeResourceManager.VersionOverride = null;
            base.TearDownCore();
        }
        [Test]
        public void FillJumpListInXaml_AttachToWindow_ShowWindow_CheckApplied() {
            applicationJumpListService.Items.Add(new ApplicationJumpPathInfo() { Path = "1.txt" });
            Interaction.GetBehaviors(Window).Add((ServiceBase)applicationJumpListService);
            EnqueueShowWindow();
            Assert.IsTrue(Window.IsLoaded);
            AssertHelper.AssertEnumerablesAreEqual(new JumpItem[] { new JumpPath() { Path = "1.txt" } }, nativeJumpList.AppliedList.JumpItems, true, false);
        }
        [Test]
        public void ShowFrequentCategory_ShowRecentCategory() {
            applicationJumpListService.ShowFrequentCategory = true;
            applicationJumpListService.ShowRecentCategory = true;
            applicationJumpListService.Apply();
            Assert.AreEqual(true, nativeJumpList.AppliedList.ShowFrequentCategory);
            Assert.AreEqual(true, nativeJumpList.AppliedList.ShowRecentCategory);
        }
        [Test]
        public void AddToRecentCategory() {
            applicationJumpListService.AddToRecentCategory("1");
            AssertHelper.AssertEnumerablesAreEqual(new string[] { "1" }, nativeJumpList.RecentCategory);
        }
        [Test]
        public void AddJumpPath_CheckRejectedItemsListIsEmpty() {
            applicationJumpListService.Items.Add("4");
            AssertHelper.AssertEnumerablesAreEqual(new RejectedApplicationJumpItem[] { }, applicationJumpListService.Apply());
        }
        [Test]
        public void AddJumpPath() {
            applicationJumpListService.Items.Add("1");
            applicationJumpListService.Items.Add("category", "2");
            applicationJumpListService.Apply();
            AssertHelper.AssertEnumerablesAreEqual(
                new JumpItem[] {
                    new JumpPath() { Path = "1" },
                    new JumpPath() { Path = "2", CustomCategory = "category" }
                }, nativeJumpList.AppliedList.JumpItems, true, false
            );
        }
        [Test]
        public void ClearJumpPath() {
            applicationJumpListService.Items.Add("1");
            applicationJumpListService.Items.Add("category", "2");
            applicationJumpListService.Items.Clear();
            applicationJumpListService.Apply();
            AssertHelper.AssertEnumerablesAreEqual(new JumpItem[] { }, nativeJumpList.AppliedList.JumpItems, true, false);
        }
        [Test]
        public void ReplaceJumpPath() {
            applicationJumpListService.Items.Add("1");
            applicationJumpListService.Items.Add("category", "2");
            applicationJumpListService.Items.Add("category5", "3");
            applicationJumpListService.Items[1] = applicationJumpListService.Items[0];
            applicationJumpListService.Apply();
            AssertHelper.AssertEnumerablesAreEqual(
                new JumpItem[] {
                    new JumpPath() { Path = "1" },
                    new JumpPath() { Path = "1" },
                    new JumpPath() { Path = "3", CustomCategory = "category5" },
                }, nativeJumpList.AppliedList.JumpItems, true, false
            );
        }
        [Test]
        public void RemoveJumpPath() {
            applicationJumpListService.Items.Add("1");
            applicationJumpListService.Items.Add("category", "2");
            applicationJumpListService.Items.Add("category5", "3");
            applicationJumpListService.Items.RemoveAt(1);
            applicationJumpListService.Apply();
            AssertHelper.AssertEnumerablesAreEqual(
                new JumpItem[] {
                    new JumpPath() { Path = "1" },
                    new JumpPath() { Path = "3", CustomCategory = "category5" },
                }, nativeJumpList.AppliedList.JumpItems, true, false
            );
        }
        [Test]
        public void AddApplicationJumpTask_CheckProperties() {
            Action action = () => { };
            applicationJumpListService.Items.Add("category", "1", null, "desc", action);
            applicationJumpListService.Items.Add(new ApplicationJumpTaskInfo() { Title = "2", IconResourcePath = "D:\\1.ico", IconResourceIndex = 3 });
            applicationJumpListService.Apply();
            AssertHelper.AssertEnumerablesAreEqual(
                new JumpItem[] {
                    new JumpTask() { Title = "1", IconResourcePath = null, Description = "desc", CustomCategory = "category" },
                    new JumpTask() { Title = "2", IconResourcePath = "D:\\1.ico", IconResourceIndex = 3 }
                }, nativeJumpList.AppliedList.JumpItems, true, false
            );
        }
        [Test]
        public void AddApplicationJumpTask() {
            Action action = () => { };
            ApplicationJumpTaskInfo jumpTask = applicationJumpListService.Items.Add("category", "1", null, "desc", action);
            applicationJumpListService.Apply();
            AssertHelper.AssertEnumerablesAreEqual(
                new ApplicationJumpTaskInfo[] {
                    jumpTask
                }, jumpActionsManager.RegisteredActions, true
            );
        }
        [Test]
        public void AddApplicationJumpTaskWithoutAction_AssignAction_CheckActionRegistered() {
            ApplicationJumpTaskInfo jumpTask = new ApplicationJumpTaskInfo() { Title = "1" };
            applicationJumpListService.Items.Add(jumpTask);
            applicationJumpListService.Apply();
            jumpTask.Action = () => { };
            AssertHelper.AssertEnumerablesAreEqual(
                new ApplicationJumpTaskInfo[] {
                    jumpTask
                }, jumpActionsManager.RegisteredActions, true
            );
        }
        [Test]
        public void AddApplicationJumpTaskWithIcon_CheckIconJumpTaskIconResourcePath() {
            ImageSource icon = ApplicationJumpListServiceTestsImageSourceHelper.GetImageSource(
                AssemblyHelper.GetResourceUri(typeof(ApplicationJumpListServiceTests).Assembly, "Services/ApplicationJumpListService/demoicon.ico"));
            ApplicationJumpTaskInfo applicationJumpTask = applicationJumpListService.Items.Add("title", icon, () => { });
            applicationJumpListService.Apply();
            ApplicationJumpTaskWrap jumpTask = (ApplicationJumpTaskWrap)nativeJumpList.AppliedList.JumpItems.Single();
            Assert.IsNotNull(jumpTask.IconResourcePath);
            byte[] expectedIcon = StreamHelper.CopyAllBytes(
                AssemblyHelper.GetResourceStream(typeof(ApplicationJumpListServiceTests).Assembly, "Services/ApplicationJumpListService/demoicon.ico", true));
            byte[] actualIcon = File.ReadAllBytes(jumpTask.IconResourcePath);
            AssertHelper.AssertEnumerablesAreEqual(expectedIcon, actualIcon);
        }
        [Test]
        public void RejectedReasons() {
            ImageSource invalidIcon = new BitmapImage() { UriSource = AssemblyHelper.GetResourceUri(typeof(ApplicationJumpListServiceTests).Assembly, "INVALID") };
            ImageSource icon = ApplicationJumpListServiceTestsImageSourceHelper.GetImageSource(
                AssemblyHelper.GetResourceUri(typeof(ApplicationJumpListServiceTests).Assembly, "Services/ApplicationJumpListService/demoicon.ico"));
            applicationJumpListService.Items.Add("a:None");
            applicationJumpListService.Items.Add("b:InvalidItem");
            applicationJumpListService.Items.Add("c:NoRegisteredHandler");
            applicationJumpListService.Items.Add("d:RemovedByUser");
            AssertHelper.AssertThrows<ArgumentException>(() => {
                applicationJumpListService.Items.Add("e", invalidIcon, () => { }, "e");
            }, e => {
                Assert.AreEqual("icon", e.ParamName);
                Assert.IsTrue(e.InnerException is ApplicationJumpTaskInvalidIconException);
            });
            AssertHelper.AssertThrows<ApplicationJumpTaskBothIconAndIconResourcePathSpecifiedException>(() => {
                applicationJumpListService.Items.Add(new ApplicationJumpTaskInfo() { Title = "g", Icon = icon, IconResourcePath = "C:\\1.ico", Action = () => { }, CommandId = "g" });
            });
            IEnumerable<RejectedApplicationJumpItem> rejection = applicationJumpListService.Apply();
            AssertHelper.AssertEnumerablesAreEqual(new JumpItemRejectionReason[] {
                JumpItemRejectionReason.None,
                JumpItemRejectionReason.InvalidItem,
                JumpItemRejectionReason.NoRegisteredHandler,
                JumpItemRejectionReason.RemovedByUser,
            }, rejection.Select(r => r.Reason));
        }
        [Test]
        public void AddApplicationJumpTaskTwice() {
            applicationJumpListService.Items.Add("category", "1", null, "desc", () => { });
            AssertHelper.AssertThrows<InvalidOperationException>(() => {
                applicationJumpListService.Items.Add("category", "1", null, "desc", () => { });
            });
            applicationJumpListService.Items.Add("another category", "1", null, "desc", () => { });
            IEnumerable<RejectedApplicationJumpItem> rejection = applicationJumpListService.Apply();
            AssertHelper.AssertEnumerablesAreEqual(new JumpItemRejectionReason[] { }, rejection.Select(r => r.Reason));
        }
        [Test]
        public void AddApplicationJumpTaskWithNullTitleAndNullCategory() {
            applicationJumpListService.Items.Add(null, null, null, "desc", () => { });
            IEnumerable<RejectedApplicationJumpItem> rejection = applicationJumpListService.Apply();
            Assert.IsFalse(rejection.Any());
        }
        [Test]
        public void JumpListCanBeConstructedFromXaml() {
            Assert.IsNotNull(typeof(ApplicationJumpListService).GetConstructor(new Type[] { }));
            Type itemsCollectionType = typeof(ApplicationJumpListService).GetProperty("Items").PropertyType;
            Assert.IsTrue(itemsCollectionType.GetInterfaces().Contains(typeof(IList)));
            Type[] itemTypes = itemsCollectionType.GetMethods().Where(m => m.Name == "Add").Select(m => m.GetParameters().SingleOrDefault()).Where(x => x != null).Select(p => p.ParameterType).ToArray();
            AssertHelper.AssertSetsAreEqual(new Type[] { typeof(ApplicationJumpItem) }, itemTypes);
        }
        [Test]
        public void AddPath_CastListToObjectArray() {
            ApplicationJumpPathInfo jumpPathInfo = applicationJumpListService.Items.Add("1");
            object[] a = applicationJumpListService.Items.Cast<object>().ToArray();
            Assert.AreEqual(1, a.Length);
            Assert.AreEqual(jumpPathInfo, a[0]);
            IEnumerable items = applicationJumpListService.Items;
            object[] b = items.Cast<object>().ToArray();
            Assert.AreEqual(1, b.Length);
            Assert.AreEqual(jumpPathInfo, b[0]);
            object[] c = ((ApplicationJumpListService)applicationJumpListService).Items.Cast<object>().ToArray();
            Assert.AreEqual(1, c.Length);
            ApplicationJumpPath jumpPath = (ApplicationJumpPath)c[0];
            Assert.AreEqual(jumpPathInfo, ApplicationJumpItem.GetItemInfo(jumpPath));
            Assert.AreEqual(jumpPath, ApplicationJumpItem.GetItem(jumpPathInfo));
        }
        [Test]
        public void SeveralJumpList() {
            IApplicationJumpListService jumpList_2 = new TestApplicationJumpListService(nativeJumpList, jumpActionsManager);
            Assert.IsFalse(jumpList_2.ShowFrequentCategory);
            Assert.IsFalse(jumpList_2.ShowRecentCategory);
            applicationJumpListService.ShowFrequentCategory = true;
            applicationJumpListService.ShowRecentCategory = true;
            applicationJumpListService.Items.Add("1");
            applicationJumpListService.Apply();
            AssertHelper.AssertEnumerablesAreEqual(new ApplicationJumpItemInfo[] { new ApplicationJumpPathInfo() { Path = "1" } }, jumpList_2.Items, true);
            Assert.IsTrue(jumpList_2.ShowFrequentCategory);
            Assert.IsTrue(jumpList_2.ShowRecentCategory);
        }
        [Test]
        public void TryChangeTitleAfterAddingToList() {
            ApplicationJumpTaskInfo task = applicationJumpListService.Items.Add("title", null, () => { });
            AssertHelper.AssertThrows<InvalidOperationException>(() => task.Title = "new title");
        }
        [Test]
        public void TryRegisterTwoSimilarTasks_SetCommandIdManual_Register() {
            ApplicationJumpTaskInfo task = applicationJumpListService.Items.Add("title", null, () => { }).Clone();
            AssertHelper.AssertThrows<ApplicationJumpTaskDuplicateCommandIdException>(() => applicationJumpListService.Items.Add(task));
            task.CommandId = "xxx";
            applicationJumpListService.Items.Add(task);
        }
        [Test]
        public void ReplaceTaskByTaskWithSimilarCommand() {
            ApplicationJumpTaskInfo task = applicationJumpListService.Items.Add("title", null, () => { }, "xxx").Clone();
            task.Title = "another";
            applicationJumpListService.Items[0] = task;
        }
        [Test]
        public void AddOrReplace() {
            ApplicationJumpTaskInfo task = applicationJumpListService.Items.Add("title", null, () => { });
            task = task.Clone();
            Assert.IsFalse(applicationJumpListService.Items.AddOrReplace(task));
            Assert.AreEqual(1, applicationJumpListService.Items.Count);
            task = task.Clone();
            task.Title = "new title";
            Assert.IsTrue(applicationJumpListService.Items.AddOrReplace(task));
            Assert.AreEqual(2, applicationJumpListService.Items.Count);
        }
    }
    public class TestNativeJumpList : NativeJumpList {
        public TestNativeJumpList()
            : base(ApplicationJumpItemWrapper.GetJumpItemCommandId) {
            AppliedList = new JumpList();
            RecentCategory = new List<string>();
        }
        public JumpList AppliedList { get; private set; }
        public List<string> RecentCategory { get; private set; }
        protected override IEnumerable<Tuple<JumpItem, JumpItemRejectionReason>> ApplyOverride(JumpList list) {
            AppliedList.ShowFrequentCategory = list.ShowFrequentCategory;
            AppliedList.ShowRecentCategory = list.ShowRecentCategory;
            AppliedList.JumpItems.Clear();
            AppliedList.JumpItems.AddRange(list.JumpItems);
            List<Tuple<JumpItem, JumpItemRejectionReason>> rejection = new List<Tuple<JumpItem, JumpItemRejectionReason>>();
            foreach(JumpItem item in list.JumpItems) {
                string title = (item as JumpPath).Return(p => p.Path, () => ((JumpTask)item).Title) ?? string.Empty;
                title.Split(':')
                    .Skip(1).SingleOrDefault()
                    .Do(s => rejection.Add(new Tuple<JumpItem, JumpItemRejectionReason>(item, (JumpItemRejectionReason)Enum.Parse(typeof(JumpItemRejectionReason), s))));
            }
            foreach(JumpItem item in rejection.Select(r => r.Item1))
                list.JumpItems.Remove(item);
            return rejection;
        }
        protected override void AddToRecentCategoryOverride(string path) {
            RecentCategory.Add(path);
        }
    }
    public class TestJumpActionsManager : IJumpActionsManager {
        List<ApplicationJumpTaskInfo> registeredActions = new List<ApplicationJumpTaskInfo>();
        bool updating = false;

        public List<ApplicationJumpTaskInfo> RegisteredActions { get { return registeredActions; } }
        public void BeginUpdate() {
            if(updating)
                throw new InvalidOperationException();
            updating = true;
        }
        public void EndUpdate() {
            updating = false;
        }
        public void RegisterAction(IJumpAction jumpAction, string commandLineArgumentPrefix, Func<string> launcherPath) {
            if(!updating)
                throw new InvalidOperationException();
            Assert.IsNotNull(commandLineArgumentPrefix);
            Assert.IsNotNull(launcherPath);
            Assert.IsTrue(File.Exists(launcherPath()));
            Assert.IsNotNull(jumpAction.CommandId);
            ApplicationJumpTaskWrap applicationJumpTaskWrap = (ApplicationJumpTaskWrap)jumpAction;
            RegisteredActions.Add(applicationJumpTaskWrap.ApplicationJumpTask);
        }
    }
    public class TestApplicationJumpListService : ApplicationJumpListService {
        public TestApplicationJumpListService(INativeJumpList nativeJumpList, IJumpActionsManager jumpActionsManager) : base(nativeJumpList, jumpActionsManager) { }
    }
}