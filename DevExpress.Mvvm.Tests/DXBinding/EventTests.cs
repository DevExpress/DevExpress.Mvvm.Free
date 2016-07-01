using DevExpress.Mvvm.POCO;
using DevExpress.Mvvm.UI;
using NUnit.Framework;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace DevExpress.Xpf.DXBinding.Tests {
    [TestFixture]
    public class EventTests {
        [SetUp]
        public void Init() {
            BindingListener.Enable();
            BindingTestHelper.TestsSetUp();
        }
        [TearDown]
        public void TearDown() {
            BindingTestHelper.TestsTearDown();
            BindingListener.Disable();
        }
        [Test]
        public void OneMethod() {
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
        public void TwoMethods() {
            var vm = EventTests_a.Create();
            var bt = BindingTestHelper.BindAssert<EventTests_visual>("test:EventTests_visual", "CustomEvent", "{b:DXEvent 'Do1(); Do2()'}", null, vm);
            bt.RaiseCustomEvent();
            Assert.AreEqual(1, vm.Do1Counter);
            Assert.AreEqual(1, vm.Do2Counter);
        }
        [Test]
        public void Parameters() {
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
        public void EventInDataTemplate() {
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
        public void StaticMethod() {
            EventTests_a.DoValue = 0;
            var bt = BindingTestHelper.BindAssert<EventTests_visual>("test:EventTests_visual", "CustomEvent", "{b:DXEvent '$test:EventTests_a.DoStatic()'}", null, null);
            bt.RaiseCustomEvent();
            Assert.AreEqual(1, EventTests_a.DoValue);

            EventTests_a.DoValue = 0;
            bt = BindingTestHelper.BindAssert<EventTests_visual>("test:EventTests_visual", "CustomEvent", "{b:DXEvent '$test:EventTests_a.DoStatic()'}", null, new object());
            bt.RaiseCustomEvent();
            Assert.AreEqual(1, EventTests_a.DoValue);
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
    }
}