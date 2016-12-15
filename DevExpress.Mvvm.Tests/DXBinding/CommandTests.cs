using DevExpress.Mvvm.POCO;
using DevExpress.Mvvm.UI;
using NUnit.Framework;
using System;
using System.Linq;
using System.Windows.Controls;

namespace DevExpress.Xpf.DXBinding.Tests {
    [TestFixture]
    public class CommandTests {
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
        public void OneExecute() {
            var vm = CommandTests_a.Create();
            var bt = BindingTestHelper.BindAssert<Button>("Button", "Command", "{b:DXCommand Do1()}", null, vm);
            BindingTestHelper.DoCommand(bt);
            Assert.AreEqual(1, vm.Do1Counter);
            BindingTestHelper.DoCommand(bt);
            Assert.AreEqual(2, vm.Do1Counter);

            vm = CommandTests_a.Create();
            vm.CanDo1Value = true;
            bt = BindingTestHelper.BindAssert<Button>("Button", "Command", "{b:DXCommand Do1(), CanExecute='CanDo1()'}", null, vm);
            Assert.AreEqual(1, vm.CanDo1Counter);

            vm = CommandTests_a.Create();
            bt = BindingTestHelper.BindAssert<Button>("Button", "Command", "{b:DXCommand Do1(), CanExecute='CanDo1()'}", null, vm);
            Assert.AreEqual(1, vm.CanDo1Counter);
            Assert.AreEqual(false, BindingTestHelper.CanDoCommand(bt));
            Assert.AreEqual(2, vm.CanDo1Counter);
            Assert.AreEqual(0, vm.Do1Counter);
            vm.CanDo1Value = true;
            Assert.AreEqual(true, BindingTestHelper.CanDoCommand(bt));
            Assert.AreEqual(3, vm.CanDo1Counter);
            BindingTestHelper.DoCommand(bt);
            Assert.AreEqual(1, vm.Do1Counter);
        }
        [Test]
        public void TwoExecute() {
            var vm = CommandTests_a.Create();
            var bt = BindingTestHelper.BindAssert<Button>("Button", "Command", "{b:DXCommand Execute='Do1(); Do2()'}", null, vm);
            BindingTestHelper.DoCommand(bt);
            Assert.AreEqual(1, vm.Do1Counter);
            Assert.AreEqual(1, vm.Do2Counter);
            BindingTestHelper.DoCommand(bt);
            Assert.AreEqual(2, vm.Do1Counter);
            Assert.AreEqual(2, vm.Do2Counter);

            vm = CommandTests_a.Create();
            bt = BindingTestHelper.BindAssert<Button>("Button", "Command", "{b:DXCommand Execute='Do1(); Do2()', CanExecute='CanDo1() &amp;&amp; CanDo2()'}", null, vm);
            Assert.AreEqual(false, BindingTestHelper.CanDoCommand(bt));
            Assert.AreEqual(2, vm.CanDo1Counter);
            Assert.AreEqual(0, vm.CanDo2Counter);
            vm.CanDo1Value = true;
            Assert.AreEqual(false, BindingTestHelper.CanDoCommand(bt));
            Assert.AreEqual(3, vm.CanDo1Counter);
            Assert.AreEqual(1, vm.CanDo2Counter);
            vm.CanDo2Value = true;
            Assert.AreEqual(true, BindingTestHelper.CanDoCommand(bt));
            Assert.AreEqual(4, vm.CanDo1Counter);
            Assert.AreEqual(2, vm.CanDo2Counter);
            BindingTestHelper.DoCommand(bt);
            Assert.AreEqual(1, vm.Do1Counter);
            Assert.AreEqual(1, vm.Do2Counter);
        }
        [Test]
        public void Arguments() {
            var vm = CommandTests_a.Create();
            var bt = BindingTestHelper.BindAssert<Button>("Button", "Command",
                "{b:DXCommand Execute='Do3(@s.Tag.Parameter, @parameter);', CanExecute='CanDo3(@s.Tag.CanDo)'}", null, vm);
            Assert.AreEqual(false, BindingTestHelper.CanDoCommand(bt));
            bt.Tag = new { CanDo = true }; BindingTestHelper.DoEvents(bt);
            Assert.AreEqual(false, BindingTestHelper.CanDoCommand(bt));
            bt.Tag = new { CanDo = true, Parameter = 1 }; BindingTestHelper.DoEvents(bt);
            Assert.AreEqual(false, BindingTestHelper.CanDoCommand(bt));
            bt.CommandParameter = 1; BindingTestHelper.DoEvents(bt);
            Assert.AreEqual(true, BindingTestHelper.CanDoCommand(bt));
            BindingTestHelper.DoCommand(bt);
            Assert.AreEqual(2, vm.Do3Value);
        }
        [Test]
        public void CommandInStyleSetter() {
            string xaml1 = @"
<Grid>
    <Grid.Resources>
        <Style x:Key=""st"" TargetType=""Button"">
            <Setter Property=""Command"" Value=""{b:DXCommand Method($test:BindingTests_a.StaticIntProp)}""/>
        </Style>
    </Grid.Resources>
    <Button Style=""{StaticResource st}""/>
    <Button Style=""{StaticResource st}""/>
</Grid>
";
            string xaml2 = @"
<Grid>
    <Grid.Resources>
        <Style TargetType=""Button"">
            <Setter Property=""Command"" Value=""{b:DXCommand Method($test:BindingTests_a.StaticIntProp)}""/>
        </Style>
    </Grid.Resources>
    <Button/>
    <Button/>
</Grid>
";

            Action<string> test = xamlStr => {
                BindingTests_a.Static(2);
                var panel = BindingTestHelper.LoadXaml<Grid>(xamlStr);
                var tb1 = (Button)panel.Children[0];
                var tb2 = (Button)panel.Children[1];
                var vm = CommandTests_a.Create();
                panel.DataContext = vm;
                BindingTestHelper.DoEvents(panel);
                BindingTestHelper.DoCommand(tb1);
                Assert.AreEqual(2, vm.MethodValue);
                BindingTests_a.Static(3);
                BindingTestHelper.DoCommand(tb2);
                Assert.AreEqual(3, vm.MethodValue);
            };
            test(xaml1);
            test(xaml2);
        }
        [Test]
        public void CommandInDataTemplate() {
            string xaml = @"
<Grid>
    <Grid.Resources>
        <DataTemplate x:Key=""temp"">
            <Button Command=""{b:DXCommand 'Method($test:BindingTests_a.StaticIntProp)'}""/>
        </DataTemplate>
    </Grid.Resources>
    <ContentControl Content=""{b:DXBinding}"" ContentTemplate=""{StaticResource temp}""/>
    <ContentControl Content=""{b:DXBinding}"" ContentTemplate=""{StaticResource temp}""/>
</Grid>
";
            var panel = BindingTestHelper.LoadXaml<Grid>(xaml);
            BindingTestHelper.VisualTest(panel, () => {
                BindingTests_a.Static(2);
                var vm =  CommandTests_a.Create();
                var tb1 = LayoutTreeHelper.GetVisualChildren(panel.Children[0]).OfType<Button>().First();
                var tb2 = LayoutTreeHelper.GetVisualChildren(panel.Children[1]).OfType<Button>().First();
                panel.DataContext = vm;
                BindingTestHelper.DoEvents(panel);
                BindingTestHelper.DoCommand(tb1);
                Assert.AreEqual(1, vm.MethodCounter);
                BindingTestHelper.DoCommand(tb2);
                Assert.AreEqual(2, vm.MethodCounter);
            });
        }
        [Test]
        public void StaticMethod() {
            CommandTests_a.DoValue = 0;
            var bt = BindingTestHelper.BindAssert<Button>("Button", "Command", "{b:DXCommand '$test:CommandTests_a.DoStatic()'}", null, null);
            BindingTestHelper.DoCommand(bt);
            Assert.AreEqual(1, CommandTests_a.DoValue);

            CommandTests_a.DoValue = 0;
            bt = BindingTestHelper.BindAssert<Button>("Button", "Command", "{b:DXCommand '$test:CommandTests_a.DoStatic()'}", null, new object());
            BindingTestHelper.DoCommand(bt);
            Assert.AreEqual(1, CommandTests_a.DoValue);
        }
    }
    public class CommandTests_a {
        public static int DoValue { get; set; }
        public static void DoStatic() { DoValue++; }
        public static CommandTests_a Create() {
            return ViewModelSource.Create(() => new CommandTests_a());
        }
        protected CommandTests_a() { }
        public virtual int Do1Counter { get; set; }
        public virtual int Do2Counter { get; set; }
        public virtual int CanDo1Counter { get; set; }
        public virtual int CanDo2Counter { get; set; }
        public virtual bool CanDo1Value { get; set; }
        public virtual bool CanDo2Value { get; set; }
        public void Do1() { Do1Counter++; }
        public void Do2() { Do2Counter++; }
        public bool CanDo1() {
            CanDo1Counter++;
            return CanDo1Value;
        }
        public bool CanDo2() {
            CanDo2Counter++;
            return CanDo2Value;
        }

        public virtual int Do3Value { get; set; }
        public void Do3(int p1, int p2) { Do3Value = p1 + p2; }
        public bool CanDo3(bool p) { return p; }

        public virtual int MethodValue { get; set; }
        public virtual int MethodCounter { get; set; }
        public void Method(int p) {
            MethodValue = p;
            MethodCounter++;
        }
    }
}