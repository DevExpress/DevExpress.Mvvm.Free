using DevExpress.Mvvm.POCO;
using DevExpress.Mvvm.UI;
using NUnit.Framework;
using System;
using System.Linq;
using System.Windows.Controls;
using System.ComponentModel;
using System.Windows;
using System.Reflection;

namespace DevExpress.Xpf.DXBinding.Tests {
    [Platform("NET")]
    [TestFixture]
    public class CommandTests {
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
        [Test, Retry(3)]
        public virtual void OneExecute() {
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
        public virtual void TwoExecute() {
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
        public virtual void Arguments() {
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
        public virtual void CommandInStyleSetter() {
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
        public virtual void CommandInDataTemplate() {
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
        public virtual void StaticMethod() {
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

    [Platform("NET")]
    [TestFixture]
    public class CommandTests_Dynamic : CommandTests {
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
        public override void Arguments() {
            var vm = CommandTests_a.Create();
            var bt = BindingTestHelper.BindAssert<Button>("Button", "Command",
                "{b:DXCommand Execute='Do3(@s.Tag.Parameter, @parameter);', CanExecute='CanDo3(@s.Tag.CanDo)'}", null, vm);
            Assert.AreEqual(false, BindingTestHelper.CanDoCommand(bt));
            bt.Tag = new { CanDo = true }; BindingTestHelper.DoEvents(bt);
            Assert.AreEqual(true, BindingTestHelper.CanDoCommand(bt));
            bt.Tag = new { CanDo = true, Parameter = 1 }; BindingTestHelper.DoEvents(bt);
            Assert.AreEqual(true, BindingTestHelper.CanDoCommand(bt));
            bt.CommandParameter = 1; BindingTestHelper.DoEvents(bt);
            Assert.AreEqual(true, BindingTestHelper.CanDoCommand(bt));
            BindingTestHelper.DoCommand(bt);
            Assert.AreEqual(2, vm.Do3Value);
        }
        [Test]
        public void NewOperator() {
            var vm = new CommandTests_b();
            var bt = BindingTestHelper.BindAssert<Button>(
                "Button", "Command",
                @"{b:DXCommand 
                    Execute='Do(@s.Margin);', 
                    CanExecute='new $Thickness(@s.Margin.Bottom).Left == 1'}",
                null, vm);
            Assert.AreEqual(false, BindingTestHelper.CanDoCommand(bt));
            Assert.AreEqual(0, vm.DoubleProp);

            bt.Margin = new Thickness(1, 0, 0, 0); BindingTestHelper.DoEvents(bt);
            Assert.AreEqual(false, BindingTestHelper.CanDoCommand(bt));
            Assert.AreEqual(0, vm.DoubleProp);

            bt.Margin = new Thickness(1, 0, 0, 1); BindingTestHelper.DoEvents(bt);
            Assert.AreEqual(true, BindingTestHelper.CanDoCommand(bt));
            BindingTestHelper.DoCommand(bt); BindingTestHelper.DoEvents(bt);
            Assert.AreEqual(1, vm.DoubleProp);
        }
        [Test]
        public void AssignOperator() {
            var vm = new CommandTests_b() { IntProp = 0 };
            var bt = BindingTestHelper.BindAssert<Button>(
                "Button", "Command",
                @"{b:DXCommand 
                    Execute='IntProp = IntProp + 1'}",
                null, vm);
            Assert.AreEqual(0, vm.IntProp);
            BindingTestHelper.DoCommand(bt); BindingTestHelper.DoEvents(bt);
            Assert.AreEqual(1, vm.IntProp);

            vm.IntProp = 0;
            bt = BindingTestHelper.BindAssert<Button>(
                "Button", "Command",
                @"{b:DXCommand 
                    Execute='IntProp = IntProp + @parameter'}",
                null, vm);
            bt.CommandParameter = 1;
            Assert.AreEqual(0, vm.IntProp);
            BindingTestHelper.DoCommand(bt); BindingTestHelper.DoEvents(bt);
            Assert.AreEqual(1, vm.IntProp);
            bt.CommandParameter = 2;
            BindingTestHelper.DoCommand(bt); BindingTestHelper.DoEvents(bt);
            Assert.AreEqual(3, vm.IntProp);

            bt = BindingTestHelper.BindAssert<Button>(
             "Button", "Command",
             @"{b:DXCommand 
                    Execute='@s.Tag = @parameter'}",
             null, null);
            bt.CommandParameter = 1;
            BindingTestHelper.DoCommand(bt); BindingTestHelper.DoEvents(bt);
            Assert.AreEqual(1, bt.Tag);
            bt.CommandParameter = 2;
            BindingTestHelper.DoCommand(bt); BindingTestHelper.DoEvents(bt);
            Assert.AreEqual(2, bt.Tag);
        }
        [Test]
        public void AssignOperator_ElementName() {
            string xaml = @"
<Grid x:Name=""panel"" Tag=""{b:DXBinding '1'}"">
    <Button Command=""{b:DXCommand Execute='@e(panel).Tag = @e(panel).Tag + 1'}""/>
</Grid>
";
            var panel = BindingTestHelper.LoadXaml<Grid>(xaml);
            var bt = (Button)panel.Children[0];
            Assert.AreEqual(1, panel.Tag);
            BindingTestHelper.DoCommand(bt); BindingTestHelper.DoEvents(bt);
            Assert.AreEqual(2, panel.Tag);
        }
        [Test]
        public void AssignOperators() {
            var vm = new CommandTests_b() { IntProp = 0 };
            var bt = BindingTestHelper.BindAssert<Button>(
                "Button", "Command",
                @"{b:DXCommand 
                    Execute='IntProp = IntProp + 1; IntProp = IntProp + 1;;'}",
                null, vm);
            Assert.AreEqual(0, vm.IntProp);
            BindingTestHelper.DoCommand(bt); BindingTestHelper.DoEvents(bt);
            Assert.AreEqual(2, vm.IntProp);
        }
        [Test]
        public void AssignOperator_Static() {
            CommandTests_a.DoValue = 0;
            var bt = BindingTestHelper.BindAssert<Button>(
                "Button", "Command",
                @"{b:DXCommand 
                    Execute='$test:CommandTests_a.DoValue = $test:CommandTests_a.DoValue + 1'}",
                null, null);
            Assert.AreEqual(0, CommandTests_a.DoValue);
            BindingTestHelper.DoCommand(bt); BindingTestHelper.DoEvents(bt);
            Assert.AreEqual(1, CommandTests_a.DoValue);
        }

        [Test]
        public void AttachedPropertyTest() {
            var bt = BindingTestHelper.BindAssert<Button>(
               "Button", "Command",
               @"{b:DXCommand 
                    Execute='@s.($test:CommandTests_a.AttachedProp) = true'}",
               null, null, false);
            Assert.AreEqual(null, CommandTests_a.GetAttachedProp(bt));
            BindingTestHelper.DoCommand(bt); BindingTestHelper.DoEvents(bt);
            Assert.AreEqual(true, CommandTests_a.GetAttachedProp(bt));
        }

        [Test]
        public void T684511() {
            var vm = new CommandTests_b();
            var bt = BindingTestHelper.BindAssert<Button>(
                "Button", "Command",
                @"{b:DXCommand Execute='DoException()'}",
                null, vm);
            Assert.DoesNotThrow(() => BindingTestHelper.DoCommand(bt));

            bt = BindingTestHelper.BindAssert<Button>(
                "Button", "Command",
                @"{b:DXCommand Execute='DoException()', CatchExceptions=False}",
                null, vm);
            var e = Assert.Throws<TargetInvocationException>(() => BindingTestHelper.DoCommand(bt));
            Assert.AreEqual("DoException", e.InnerException.Message);
        }
    }

    public class CommandTests_a {
        public static readonly DependencyProperty AttachedPropProperty = DependencyProperty.RegisterAttached("AttachedProp", typeof(object), typeof(CommandTests_a), new PropertyMetadata(null));
        public static object GetAttachedProp(DependencyObject obj) { return (object)obj.GetValue(AttachedPropProperty); }
        public static void SetAttachedProp(DependencyObject obj, object value) { obj.SetValue(AttachedPropProperty, value); }

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
    public class CommandTests_b : INotifyPropertyChanged {
        public event PropertyChangedEventHandler PropertyChanged;
        int intProp;
        public int IntProp {
            get { return intProp; }
            set {
                if (intProp == value) return;
                intProp = value;
                if (PropertyChanged != null)
                    PropertyChanged(this, new PropertyChangedEventArgs("IntProp"));
            }
        }
        double doubleProp;
        public double DoubleProp {
            get { return doubleProp; }
            set {
                if (doubleProp == value) return;
                doubleProp = value;
                if (PropertyChanged != null)
                    PropertyChanged(this, new PropertyChangedEventArgs("DoubleProp"));
            }
        }
        string stringProp;
        public string StringProp {
            get { return stringProp; }
            set {
                if (stringProp == value) return;
                stringProp = value;
                if (PropertyChanged != null)
                    PropertyChanged(this, new PropertyChangedEventArgs("StringProp"));
            }
        }
        public CommandTests_b GetSelf() {
            return this;
        }
        public CommandTests_b() { }
        public CommandTests_b(double v) {
            DoubleProp = v;
        }
        public void Do(Thickness thickness) {
            DoubleProp = thickness.Left;
        }
        public void DoException() {
            throw new Exception("DoException");
        }
    }
}