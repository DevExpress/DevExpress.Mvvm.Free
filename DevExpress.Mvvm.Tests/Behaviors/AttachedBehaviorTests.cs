#if SILVERLIGHT
using Microsoft.Silverlight.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
#elif NETFX_CORE
using DevExpress.TestFramework.NUnit;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
#else
using NUnit.Framework;
#endif
#if !FREE && !NETFX_CORE
using DevExpress.Xpf.Core.Tests;
#endif
using System.Windows;
using DevExpress.Mvvm.UI.Interactivity;
using DevExpress.Mvvm.Native;
using DevExpress.Mvvm.UI.Interactivity.Internal;
using System.Threading.Tasks;
#if !NETFX_CORE
using System.Windows.Data;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
#else
#endif

namespace DevExpress.Mvvm.UI.Tests {
#if !NETFX_CORE
    public class FakeTrigger : TriggerBase<DependencyObject> {
    }
#else
 public class FakeTrigger : TriggerBase<FrameworkElement> {
    }
#endif
    public class FakeBehavior : Behavior<FrameworkElement> {
    }

    public class TestBehavior : Behavior<Button> {
        internal int attachedFireCount;
        internal int detachingFireCount;
        protected override void OnAttached() {
            base.OnAttached();
            attachedFireCount++;
        }
        protected override void OnDetaching() {
            base.OnDetaching();
            detachingFireCount++;
        }
    }
    public class TestBehaviorWithBindableProperty : TestBehavior {
        public object MyProperty {
            get { return (object)GetValue(MyPropertyProperty); }
            set { SetValue(MyPropertyProperty, value); }
        }
        public static readonly DependencyProperty MyPropertyProperty =
            DependencyProperty.Register("MyProperty", typeof(object), typeof(TestBehaviorWithBindableProperty), new PropertyMetadata(null));
    }
    [TestFixture]
    public class AttachedBehaviorTests : BaseWpfFixture {
        TestBehavior behavior;
        Button button;

        protected override void SetUpCore() {
            base.SetUpCore();
            behavior = new TestBehavior();
            button = new Button();
        }
        protected override void TearDownCore() {
            base.TearDownCore();
            behavior = null;
            button = null;
        }

#if !NETFX_CORE
        [Test, Asynchronous]
        public void Q458047_BindAttachedBehaviorInPopup() {
            Popup popup = new Popup();
            var behavior = new TestBehaviorWithBindableProperty();
            BindingOperations.SetBinding(behavior, TestBehaviorWithBindableProperty.MyPropertyProperty, new Binding());
            Interaction.GetBehaviors(button).Add(behavior);
            popup.Child = button;
            button.DataContext = "test";
            EnqueueShowWindow();
            EnqueueCallback(() => {
                popup.IsOpen = true;
            });
            EnqueueWindowUpdateLayout();
            EnqueueCallback(() => {
                Assert.AreEqual("test", behavior.MyProperty);
                popup.IsOpen = false;
            });
            EnqueueTestComplete();
        }
#endif
        [Test, Asynchronous]
#if !NETFX_CORE
        public void InheritDataContextWhenElementInTree() {
#else
        public async Task InheritDataContextWhenElementInTree() {
#endif
            button.DataContext = "test";
            Window.Content = button;
#if NETFX_CORE
            await
#endif
            EnqueueShowWindow();
            EnqueueCallback(() => {
                Assert.AreEqual(0, behavior.attachedFireCount);
                Interaction.GetBehaviors(button).Add(behavior);
                Assert.AreEqual(1, behavior.attachedFireCount);
                Assert.AreEqual(1, behavior.attachedFireCount);
                Assert.AreEqual(0, behavior.detachingFireCount);
            });
            EnqueueTestComplete();
        }
        [Test, Asynchronous]
#if !NETFX_CORE
        public void InheritDataContextWhenElementInTree2() {
#else
        public async Task InheritDataContextWhenElementInTree2() {
#endif
            Border border = new Border() { DataContext = "test", Child = button };
            Window.Content = border;
#if NETFX_CORE
            await
#endif
            EnqueueShowWindow();
            EnqueueCallback(() => {
                Assert.AreEqual(0, behavior.attachedFireCount);
                Interaction.GetBehaviors(button).Add(behavior);
                Assert.AreEqual(1, behavior.attachedFireCount);
                Assert.AreEqual(1, behavior.attachedFireCount);
                Assert.AreEqual(0, behavior.detachingFireCount);
            });
            EnqueueTestComplete();
        }
        [Test]
        public void InheritDataContextWhenElementNotInTree() {
            Interaction.GetBehaviors(button).Add(behavior);
            button.DataContext = "test";
            Assert.AreEqual(1, behavior.attachedFireCount);
            Assert.AreEqual(0, behavior.detachingFireCount);
        }
        [Test]
        public void InheritDataContextWhenElementNotInTree3() {
            Interaction.GetBehaviors(button).Add(behavior);
            Assert.AreEqual(1, behavior.attachedFireCount);
            Assert.AreEqual(0, behavior.detachingFireCount);
            Border border = new Border() { DataContext = "test", Child = button };
            Interaction.GetBehaviors(button).Remove(behavior);
            Assert.AreEqual(1, behavior.attachedFireCount);
            Assert.AreEqual(1, behavior.detachingFireCount);
        }
    }

    [TestFixture]
    public class AttachedBehaviorDesignModeTests : BaseWpfFixture {
        public class TestBehaviorRegular : Behavior<FrameworkElement> { }
        public class TestBehaviorAllowAttachInDesignMode : Behavior<FrameworkElement> {
            protected override bool AllowAttachInDesignMode {
                get { return true; }
            }
        }
        public class TestBehaviorNotAllowAttachInDesignMode : Behavior<FrameworkElement> {
            protected override bool AllowAttachInDesignMode {
                get { return false; }
            }
        }

        protected override void TearDownCore() {
            base.TearDownCore();
            ViewModelDesignHelper.IsInDesignModeOverride = false;
        }

        #region RunTime
        [Test]
        public void TestBehaviorRegular_AttachInRunTime() {
            CheckAttach(new TestBehaviorRegular());
        }
        [Test]
        public void TestBehaviorAllowAttachInDesignTime_AttachInRunTime() {
            CheckAttach(new TestBehaviorAllowAttachInDesignMode());
        }
        [Test]
        public void TestBehaviorNotAllowAttachInDesignMode_AttachInRunTime() {
            CheckAttach(new TestBehaviorNotAllowAttachInDesignMode());
        }

        [Test]
        public void TestBehaviorRegular_AttachInRunTime_InteractionHelperOnBehavior() {
            var b = new TestBehaviorRegular();
            InteractionHelper.SetBehaviorInDesignMode(b, InteractionBehaviorInDesignMode.AsWellAsNotInDesignMode);
            CheckAttach(b);
        }
        [Test]
        public void TestBehaviorAllowAttachInDesignMode_AttachInRunTime_InteractionHelperOnBehavior() {
            var b = new TestBehaviorAllowAttachInDesignMode();
            InteractionHelper.SetBehaviorInDesignMode(b, InteractionBehaviorInDesignMode.AsWellAsNotInDesignMode);
            CheckAttach(b);
        }
        [Test]
        public void TestBehaviorNotAllowAttachInDesignMode_AttachInRunTime_InteractionHelperOnBehavior() {
            var b = new TestBehaviorNotAllowAttachInDesignMode();
            InteractionHelper.SetBehaviorInDesignMode(b, InteractionBehaviorInDesignMode.AsWellAsNotInDesignMode);
            CheckAttach(b);
        }

        [Test]
        public void TestBehaviorRegular_AttachInRunTime_InteractionHelperOnAssociatedObject() {
            Grid element = new Grid();
            InteractionHelper.SetBehaviorInDesignMode(element, InteractionBehaviorInDesignMode.AsWellAsNotInDesignMode);
            CheckAttach(new TestBehaviorRegular(), element);
        }
        [Test]
        public void TestBehaviorAllowAttachInDesignMode_AttachInRunTime_InteractionHelperOnAssociatedObject() {
            Grid element = new Grid();
            InteractionHelper.SetBehaviorInDesignMode(element, InteractionBehaviorInDesignMode.AsWellAsNotInDesignMode);
            CheckAttach(new TestBehaviorAllowAttachInDesignMode(), element);
        }
        [Test]
        public void TestBehaviorNotAllowAttachInDesignMode_AttachInRunTime_InteractionHelperOnAssociatedObject() {
            Grid element = new Grid();
            InteractionHelper.SetBehaviorInDesignMode(element, InteractionBehaviorInDesignMode.AsWellAsNotInDesignMode);
            CheckAttach(new TestBehaviorNotAllowAttachInDesignMode(), element);
        }
        #endregion


        #region DesignTime
        [Test]
        public void TestBehaviorRegular_NotAttachInDesignTime() {
            ViewModelDesignHelper.IsInDesignModeOverride = true;
            CheckNotAttach(new TestBehaviorRegular());
        }
        [Test]
        public void TestBehaviorAllowAttachInDesignTime_AttachInDesignTime() {
            ViewModelDesignHelper.IsInDesignModeOverride = true;
            CheckAttach(new TestBehaviorAllowAttachInDesignMode());
        }
        [Test]
        public void TestBehaviorNotAllowAttachInDesignMode_AttachInDesignTime() {
            ViewModelDesignHelper.IsInDesignModeOverride = true;
            CheckNotAttach(new TestBehaviorNotAllowAttachInDesignMode());
        }

        [Test]
        public void TestBehaviorRegular_AttachInDesignTime_InteractionHelperOnBehavior() {
            ViewModelDesignHelper.IsInDesignModeOverride = true;
            var b = new TestBehaviorRegular();
            InteractionHelper.SetBehaviorInDesignMode(b, InteractionBehaviorInDesignMode.AsWellAsNotInDesignMode);
            CheckAttach(b);
        }
        [Test]
        public void TestBehaviorAllowAttachInDesignMode_AttachInDesignTime_InteractionHelperOnBehavior() {
            ViewModelDesignHelper.IsInDesignModeOverride = true;
            var b = new TestBehaviorAllowAttachInDesignMode();
            InteractionHelper.SetBehaviorInDesignMode(b, InteractionBehaviorInDesignMode.AsWellAsNotInDesignMode);
            CheckAttach(b);
        }
        [Test]
        public void TestBehaviorNotAllowAttachInDesignMode_AttachInDesignTime_InteractionHelperOnBehavior() {
            ViewModelDesignHelper.IsInDesignModeOverride = true;
            var b = new TestBehaviorNotAllowAttachInDesignMode();
            InteractionHelper.SetBehaviorInDesignMode(b, InteractionBehaviorInDesignMode.AsWellAsNotInDesignMode);
            CheckNotAttach(b);
        }

        [Test]
        public void TestBehaviorRegular_AttachInDesignTime_InteractionHelperOnAssociatedObject() {
            ViewModelDesignHelper.IsInDesignModeOverride = true;
            Grid element = new Grid();
            InteractionHelper.SetBehaviorInDesignMode(element, InteractionBehaviorInDesignMode.AsWellAsNotInDesignMode);
            CheckAttach(new TestBehaviorRegular(), element);
        }
        [Test]
        public void TestBehaviorAllowAttachInDesignMode_AttachInDesignTime_InteractionHelperOnAssociatedObject() {
            ViewModelDesignHelper.IsInDesignModeOverride = true;
            Grid element = new Grid();
            InteractionHelper.SetBehaviorInDesignMode(element, InteractionBehaviorInDesignMode.AsWellAsNotInDesignMode);
            CheckAttach(new TestBehaviorAllowAttachInDesignMode(), element);
        }
        [Test]
        public void TestBehaviorNotAllowAttachInDesignMode_AttachInDesignTime_InteractionHelperOnAssociatedObject() {
            ViewModelDesignHelper.IsInDesignModeOverride = true;
            Grid element = new Grid();
            InteractionHelper.SetBehaviorInDesignMode(element, InteractionBehaviorInDesignMode.AsWellAsNotInDesignMode);
            CheckAttach(new TestBehaviorAllowAttachInDesignMode(), element);
        }
        #endregion

        void CheckAttach(Behavior behavior, FrameworkElement container = null) {
            FrameworkElement element = container ?? new Grid();
            Interaction.GetBehaviors(element).Add(behavior);
            Assert.IsTrue(behavior.IsAttached);
            Assert.AreSame(element, behavior.AssociatedObject);
        }
        void CheckNotAttach(Behavior behavior, FrameworkElement container = null) {
            FrameworkElement element = container ?? new Grid();
            Interaction.GetBehaviors(element).Add(behavior);
            Assert.IsFalse(behavior.IsAttached);
            Assert.IsNull(behavior.AssociatedObject);
        }
    }
}