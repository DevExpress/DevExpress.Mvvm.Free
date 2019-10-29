using DevExpress.Mvvm.POCO;
using DevExpress.Mvvm.UI;
using NUnit.Framework;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace DevExpress.Xpf.DXBinding.Tests {
    [Platform("NET")]
    [TestFixture]
    public class EventTests {
        [SetUp]
        public virtual void Init() {
            BindingListener.Enable();
            BindingTestHelper.TestsSetUp();
            BindingTestHelper.SetResolvingMode(DXBindingResolvingMode.LegacyStaticTyping);
        }
        [TearDown]
        public virtual void TearDown() {
            BindingTestHelper.TestsTearDown();
            BindingListener.Disable();
            BindingTestHelper.ClearResolvingMode();
        }
        [Test]
        public virtual void OneMethod() {
            var vm = EventTests_a.Create();
            var bt = BindingTestHelper.BindAssert<EventTests_visual>("test:EventTests_visual", "CustomRoutedEvent", "{b:DXEvent Do1()}", null, vm);
            Assert.AreEqual(0, vm.Do1Counter);
            bt.RaiseCustomRoutedEvent();
            Assert.AreEqual(1, vm.Do1Counter);

            bt = BindingTestHelper.BindAssert<EventTests_visual>("test:EventTests_visual", "CustomEvent", "{b:DXEvent Do1()}", null, vm);
            bt.RaiseCustomEvent();
            Assert.AreEqual(2, vm.Do1Counter);
        }
        [Test]
        public virtual void TwoMethods() {
            var vm = EventTests_a.Create();
            var bt = BindingTestHelper.BindAssert<EventTests_visual>("test:EventTests_visual", "CustomEvent", "{b:DXEvent 'Do1(); Do2()'}", null, vm);
            bt.RaiseCustomEvent();
            Assert.AreEqual(1, vm.Do1Counter);
            Assert.AreEqual(1, vm.Do2Counter);
        }
        [Test]
        public virtual void Parameters() {
            string xaml = @"
<Grid x:Name=""root"">
    <Grid.Resources>
        <Style x:Key=""st"" TargetType=""TextBox"">
            <Setter Property=""Text"" Value=""{b:DXBinding IntProp+$test:BindingTests_a.StaticIntProp,
                                        BackExpr='IntProp=int.Parse(@v)-$test:BindingTests_a.StaticIntProp'}""/>
        </Style>
    </Grid.Resources>
    <test:EventTests_visual x:Name=""bt1"" CustomEvent=""{b:DXEvent 'Do3(@args, @e(bt2), @sender)'}""/>
    <test:EventTests_visual x:Name=""bt2"" CustomEvent=""{b:DXEvent 'Do3(@args, @e(bt1), @sender)'}""/>
</Grid>";

            var vm = EventTests_a.Create();
            Grid panel = BindingTestHelper.LoadXaml<Grid>(xaml);
            var bt1 = (EventTests_visual)panel.Children[0];
            var bt2 = (EventTests_visual)panel.Children[1];
            panel.DataContext = vm;
            BindingTestHelper.DoEvents(panel);
            bt1.RaiseCustomEvent();
            Assert.AreEqual(bt2, vm.Do3Parameter);
            Assert.AreEqual(EventArgs.Empty, vm.Do3EventArgs);
            Assert.AreEqual(bt1, vm.Do3Sender);
            bt2.RaiseCustomEvent();
            Assert.AreEqual(bt1, vm.Do3Parameter);
            Assert.AreEqual(EventArgs.Empty, vm.Do3EventArgs);
            Assert.AreEqual(bt2, vm.Do3Sender);
        }
        [Test]
        public virtual void EventInDataTemplate() {
            string xaml = @"
<Grid>
    <Grid.Resources>
        <DataTemplate x:Key=""temp"">
            <test:EventTests_visual CustomEvent=""{b:DXEvent 'Do4($test:BindingTests_a.StaticIntProp)'}""/>
        </DataTemplate>
    </Grid.Resources>
    <ContentControl Content=""{b:DXBinding}"" ContentTemplate=""{StaticResource temp}""/>
    <ContentControl Content=""{b:DXBinding}"" ContentTemplate=""{StaticResource temp}""/>
</Grid>
";
            var panel = BindingTestHelper.LoadXaml<Grid>(xaml);
            BindingTestHelper.VisualTest(panel, () => {
                BindingTests_a.Static(2);
                var vm = EventTests_a.Create();
                var tb1 = LayoutTreeHelper.GetVisualChildren(panel.Children[0]).OfType<EventTests_visual>().First();
                var tb2 = LayoutTreeHelper.GetVisualChildren(panel.Children[1]).OfType<EventTests_visual>().First();
                panel.DataContext = vm;
                BindingTestHelper.DoEvents(panel);
                tb1.RaiseCustomEvent();
                tb2.RaiseCustomEvent();
                Assert.AreEqual(2, vm.Do4Counter);
            });
        }
        [Test]
        public virtual void StaticMethod() {
            EventTests_a.DoValue = 0;
            var bt = BindingTestHelper.BindAssert<EventTests_visual>("test:EventTests_visual", "CustomEvent", "{b:DXEvent '$test:EventTests_a.DoStatic()'}", null, null);
            bt.RaiseCustomEvent();
            Assert.AreEqual(1, EventTests_a.DoValue);

            EventTests_a.DoValue = 0;
            bt = BindingTestHelper.BindAssert<EventTests_visual>("test:EventTests_visual", "CustomEvent", "{b:DXEvent '$test:EventTests_a.DoStatic()'}", null, new object());
            bt.RaiseCustomEvent();
            Assert.AreEqual(1, EventTests_a.DoValue);
        }
        [Test]
        public virtual void NoDataContextTest_T543513() {
            var bt = BindingTestHelper.BindAssert<EventTests_visual>("test:EventTests_visual", "CustomEvent", "{b:DXEvent Do1()}", null, null);
            bt.RaiseCustomEvent();

            bt = BindingTestHelper.BindAssert<EventTests_visual>("test:EventTests_visual", "CustomEvent", "{b:DXEvent 'Do1(@sender, @args)'}", null, null);
            bt.RaiseCustomEvent();
        }
    }

    [Platform("NET")]
    [TestFixture]
    public class EventTests_Dynamic : EventTests {
        [SetUp]
        public override void Init() {
            base.Init();
            BindingTestHelper.SetResolvingMode(DXBindingResolvingMode.DynamicTyping);
        }
        [TearDown]
        public override void TearDown() {
            base.TearDown();
            BindingTestHelper.ClearResolvingMode();
        }
        [Test]
        public void NewOperator() {
            var vm = EventTests_a.Create();
            var bt = BindingTestHelper.BindAssert<EventTests_visual>(
                "test:EventTests_visual",
                "CustomRoutedEvent",
                "{b:DXEvent Do5(new $Thickness(@s.Margin.Left))}", null, vm);
            bt.RaiseCustomRoutedEvent();
            Assert.AreEqual(new Thickness(), vm.Do5V);
            bt.Margin = new Thickness(1);
            bt.RaiseCustomRoutedEvent();
            Assert.AreEqual(new Thickness(1), vm.Do5V);
        }
        [Test]
        public void AssignOperator_ElementName() {
            string xaml = @"
<Grid x:Name=""panel"" Tag=""{b:DXBinding '1'}"">
    <test:EventTests_visual CustomRoutedEvent=""{b:DXEvent Handler='@e(panel).Tag = @e(panel).Tag + 1'}""/>
</Grid>
";
            var panel = BindingTestHelper.LoadXaml<Grid>(xaml);
            var visual = (EventTests_visual)panel.Children[0];
            Assert.AreEqual(1, panel.Tag);
            visual.RaiseCustomRoutedEvent();
            Assert.AreEqual(2, panel.Tag);
        }

        [Test]
        public void T709285() {
            var vm = new T709285_VM();
            ((T709285_VM_Base)vm).Prop = "1";
            var bt = BindingTestHelper.BindAssert<EventTests_visual>(
                "test:EventTests_visual",
                "CustomRoutedEvent",
                "{b:DXEvent Do(Prop)}", null, vm);
            Assert.AreEqual(null, vm.ResProp);
            bt.RaiseCustomRoutedEvent();
            Assert.AreEqual("1", vm.ResProp);
        }
        public class T709285_VM_Base {
            public object Prop { get; set; }
        }
        public class T709285_VM : T709285_VM_Base {
            public new string Prop { get { return (string)base.Prop; } }
            public string ResProp { get; private set; }
            public void Do(string prop) {
                ResProp = prop;
            }
        }
    }

    public class EventTests_visual : Button {
        public static readonly RoutedEvent CustomRoutedEventProperty = EventManager.RegisterRoutedEvent("CustomRoutedEvent", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(EventTests_visual));
        public event RoutedEventHandler CustomRoutedEvent { add { AddHandler(CustomRoutedEventProperty, value); } remove { RemoveHandler(CustomRoutedEventProperty, value); } }
        public event EventHandler CustomEvent;
        public void RaiseCustomRoutedEvent() {
            RoutedEventArgs newEventArgs = new RoutedEventArgs(CustomRoutedEventProperty);
            RaiseEvent(newEventArgs);
            BindingTestHelper.DoEvents(this);
        }
        public void RaiseCustomEvent() {
            if(CustomEvent != null) CustomEvent(this, EventArgs.Empty);
        }
    }
    public class EventTests_a {
        public static int DoValue { get; set; }
        public static void DoStatic() { DoValue++; }
        public static EventTests_a Create() {
            return ViewModelSource.Create(() => new EventTests_a());
        }
        protected EventTests_a() { }
        public int Do1Counter { get; set; }
        public void Do1() { Do1Counter++; }
        public int Do2Counter { get; set; }
        public void Do2() { Do2Counter++; }

        public object Do3EventArgs { get; set; }
        public object Do3Parameter { get; set; }
        public object Do3Sender { get; set; }
        public void Do3(object eventArgs, object parameter, object sender) {
            Do3EventArgs = eventArgs;
            Do3Parameter = parameter;
            Do3Sender = sender;
        }

        public int Do4Counter { get; set; }
        public int Do4(int v) {
            Do4Counter++;
            return v;
        }

        public Thickness Do5V { get; set; }
        public void Do5(Thickness thickness) {
            Do5V = thickness;
        }
    }
}