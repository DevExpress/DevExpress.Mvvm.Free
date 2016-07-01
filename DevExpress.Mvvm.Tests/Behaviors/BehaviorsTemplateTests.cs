using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using System;
using DevExpress.Mvvm.UI.Interactivity;
using System.Linq;
using System.Collections.Generic;
using DevExpress.Mvvm.UI;
using System.Windows.Input;
using System.Reflection;
using NUnit.Framework;

namespace DevExpress.Mvvm.Tests.Behaviors {
    [TestFixture]
    public class BehaviorsTemplateTests : BaseWpfFixture {
        Button TestControl { get; set; }
        IList<Behavior> TestBehaviors { get { return Interaction.GetBehaviors(TestControl); } }

        #region Initialize
        protected override void SetUpCore() {
            TestControl = new Button() {
                Content = "TestContent"
            };
            base.SetUpCore();
        }

        protected override void TearDownCore() {
            TestControl = null;
            base.TearDownCore();
        }
        StackPanel CreateComplexView(string behaviorsDefinition, bool? useItemsControl = true) {
            string dcBinding = @"DataContext='{Binding RelativeSource={RelativeSource Mode=Self}}'";
            string view = string.Format(
                @"<StackPanel {0}>
                    <StackPanel.Resources>
                        <Style TargetType='Border'>
                            <Setter Property='dxmvvm:Interaction.BehaviorsTemplate'>
                                <Setter.Value>
                                    {1}
                                </Setter.Value>
                            </Setter>
                        </Style>
                    </StackPanel.Resources>
                    <Border Background='Red' {2}/>
                    <Border Background='Blue' {2}/>
                    <Border Background='Green' {2}/>
                </StackPanel>", GetXamlNamespacesDefinition(), GetTemplateString(behaviorsDefinition, useItemsControl, false), dcBinding);

            return ParseXaml<StackPanel>(view);
        }
        Style CreateStyle(string behaviorsDefinition, string targetType, bool? useItemsControl = true) {
            string style = behaviorsDefinition == null
                ? string.Format(@"<Style {0} TargetType='{1}'>
                    <Setter Property='dxmvvm:Interaction.BehaviorsTemplate' Value='{2}'/>
                </Style>", GetXamlNamespacesDefinition(), targetType, "{x:Null}")
                : string.Format(@"<Style {0} TargetType='{1}'>
                    <Setter Property='dxmvvm:Interaction.BehaviorsTemplate'>
                        <Setter.Value>
                            {2}
                        </Setter.Value>
                    </Setter>
                </Style>", GetXamlNamespacesDefinition(), targetType , GetTemplateString(behaviorsDefinition, useItemsControl, false));

            return ParseXaml<Style>(style);
        }
        DataTemplate CreateDataTemplate(string behaviorsDefinition, bool? useItemsControl) {
            if(behaviorsDefinition == null)
                return null;
            DataTemplate template = ParseXaml<DataTemplate>(GetTemplateString(behaviorsDefinition, useItemsControl, true));
            return template;
        }
        string GetTemplateString(string behaviorsDefinition, bool? useItemsControl, bool defineNamespaces) {
            string control;
            if(!useItemsControl.HasValue) {
                behaviorsDefinition = "";
                control = "Border";
            } else
                control = useItemsControl.Value ? "ItemsControl" : "ContentControl";

            return string.Format(
                @"<DataTemplate {0}>
                    <{1}>
                        {2}
                    </{1}>
                </DataTemplate>",
                defineNamespaces ? GetXamlNamespacesDefinition() : "", control, behaviorsDefinition);
        }
        string GetXamlNamespacesDefinition() {
            return string.Format(@"xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation'
                xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml'
                xmlns:local='clr-namespace:DevExpress.Mvvm.Tests.Behaviors;assembly={0}'
                xmlns:dxmvvm='clr-namespace:DevExpress.Mvvm.UI.Interactivity;assembly={1}'",
                    GetAssemblyName(Assembly.GetExecutingAssembly()), GetAssemblyName(typeof(BehaviorCollection).Assembly));
        }

        string GetAssemblyName(Assembly assembly) {
            return assembly.GetName().Name;
        }

        T ParseXaml<T>(string xaml) where T : class {
            return XamlReader.Parse(xaml) as T;
        }
        void LoadControlTemplate(string behaviorsDefinition, bool? useItemsControl = true, DependencyObject control = null) {
            control = control ?? TestControl;
            Interaction.SetBehaviorsTemplate(control, CreateDataTemplate(behaviorsDefinition, useItemsControl));
        }
        #endregion

        [Test]
        public void SetBehaviors_Test00() {
            LoadControlTemplate("<local:TestService/>");
            Assert.AreEqual(1, TestBehaviors.Count(x => x is TestService));
        }
        [Test]
        public void SetBehaviors_Test01() {
            LoadControlTemplate("<local:TestBehavior/><local:TestService/>");
            Assert.AreEqual(2, TestBehaviors.Count(x => x is Behavior));
        }
        [Test]
        public void SetBehaviors_Test02() {
            LoadControlTemplate("");
            Assert.AreEqual(0, TestBehaviors.Count);
        }
        [Test]
        public void SetBehaviors_Test03() {
            LoadControlTemplate("<local:TestService/>", false);
            Assert.AreEqual(1, TestBehaviors.Count(x => x is TestService));
        }
        [Test]
        public void SetBehaviors_Test04() {
            LoadControlTemplate("", false);
            Assert.AreEqual(0, TestBehaviors.Count);
        }
        [Test]
        public void SetBehaviors_Test05() {
            LoadControlTemplate("<Border/><local:TestBehavior/><Border/>");
            Assert.AreEqual(1, TestBehaviors.Count);
        }
        [Test]
        public void SetBehaviors_Test06() {
            LoadControlTemplate("<Border/>", false);
            Assert.AreEqual(0, TestBehaviors.Count);
        }
        [Test]
        public void SetBehaviors_Test07() {
            LoadControlTemplate("<local:TestBehavior/>", false);
            Assert.AreEqual(1, TestBehaviors.Count);
            LoadControlTemplate("", false);
            Assert.AreEqual(0, TestBehaviors.Count);
        }
        [Test]
        public void SetBehaviors_Test08() {
            LoadControlTemplate("<local:TestBehavior/>", false);
            Assert.AreEqual(1, TestBehaviors.Count);
            var testService = new TestService();
            TestBehaviors.Add(testService);
            Assert.AreEqual(2, TestBehaviors.Count);
            Assert.IsTrue(TestBehaviors.Contains(testService));
            LoadControlTemplate(null);
            Assert.AreEqual(1, TestBehaviors.Count);
            Assert.AreEqual(testService, TestBehaviors[0]);
        }
        [Test]
        public void SetBehaviors_Test09() {
            LoadControlTemplate("<local:TestBehavior/>", false);
            Assert.AreEqual(1, TestBehaviors.Count);
            var behavior = TestBehaviors[0];
            LoadControlTemplate("<local:TestService/>", false);
            Assert.AreEqual(1, TestBehaviors.Count);
            Assert.IsFalse(TestBehaviors.Contains(behavior));
            Assert.IsFalse(behavior.IsAttached);
            Assert.AreEqual(1, TestBehaviors.OfType<TestService>().Count());
        }
        [Test]
        public void SetBehaviors_Test10() {
            LoadControlTemplate("<local:TestBehavior/>");
            Assert.AreEqual(1, TestBehaviors.Count);
            LoadControlTemplate("");
            Assert.AreEqual(0, TestBehaviors.Count);
        }
        [Test]
        public void SetBehaviors_Test11() {
            LoadControlTemplate("<local:TestBehavior/>");
            Assert.AreEqual(1, TestBehaviors.Count);
            var testService = new TestService();
            TestBehaviors.Add(testService);
            Assert.AreEqual(2, TestBehaviors.Count);
            Assert.IsTrue(TestBehaviors.Contains(testService));
            LoadControlTemplate(null);
            Assert.AreEqual(1, TestBehaviors.Count);
            Assert.AreEqual(testService, TestBehaviors[0]);
        }
        [Test]
        public void SetBehaviors_Test12() {
            LoadControlTemplate("<local:TestBehavior/>");
            Assert.AreEqual(1, TestBehaviors.Count);
            var behavior = TestBehaviors[0];
            LoadControlTemplate("<local:TestBehavior/><local:TestService/>");
            Assert.AreEqual(2, TestBehaviors.Count);
            Assert.IsFalse(TestBehaviors.Contains(behavior));
            Assert.IsFalse(behavior.IsAttached);
            Assert.AreEqual(1, TestBehaviors.OfType<TestBehavior>().Count());
            Assert.AreEqual(1, TestBehaviors.OfType<TestService>().Count());
        }
        [Test]
        public void SetBehaviorsViaStyle_Test00() {
            Style style = CreateStyle("<local:TestService/>", "Button");
            TestControl.SetValue(FrameworkElement.StyleProperty, style);
            Assert.AreEqual(1, TestBehaviors.Count);
        }
        [Test]
        public void SetBehaviorsViaStyle_Test01() {
            Style style = CreateStyle("<local:TestService/>", "Button");
            Button control = new Button();
            TestControl.SetValue(FrameworkElement.StyleProperty, style);
            control.SetValue(FrameworkElement.StyleProperty, style);
            Assert.AreEqual(1, TestBehaviors.Count);
            Assert.AreEqual(1, Interaction.GetBehaviors(control).Count);
            Assert.AreNotEqual(TestBehaviors[0], Interaction.GetBehaviors(control)[0]);
        }
        [Test]
        public void SetBehaviorsViaStyle_Test02() {
            Style style = CreateStyle("<local:TestService/>", "Button", false);
            TestControl.SetValue(FrameworkElement.StyleProperty, style);
            Assert.AreEqual(1, TestBehaviors.Count);
        }
        [Test]
        public void SetBehaviorsViaStyle_Test03() {
            Style style = CreateStyle("<local:TestService/>", "Button", false);
            Button control = new Button();
            TestControl.SetValue(FrameworkElement.StyleProperty, style);
            control.SetValue(FrameworkElement.StyleProperty, style);
            Assert.AreEqual(1, TestBehaviors.Count);
            Assert.AreEqual(1, Interaction.GetBehaviors(control).Count);
            Assert.AreNotEqual(TestBehaviors[0], Interaction.GetBehaviors(control)[0]);
        }
        [Test]
        public void SetBehaviorsViaStyle_Test04() {
            Style style = CreateStyle("<local:TestService/>", "Button");
            TestControl.SetValue(FrameworkElement.StyleProperty, style);
            Assert.AreEqual(1, TestBehaviors.Count);
            style = CreateStyle(null, "Button");
            TestControl.SetValue(FrameworkElement.StyleProperty, style);
            Assert.AreEqual(0, TestBehaviors.Count);
        }
        [Test, Asynchronous]
        public void SetBehaviorsViaImplicitStyle_Test00() {
            SetBehaviorsViaImplicitStyleCore(CreateComplexView("<local:TestBehavior Brush='{Binding Background}'/>", true));
        }
        [Test, Asynchronous]
        public void SetBehaviorsViaImplicitStyle_Test01() {
            SetBehaviorsViaImplicitStyleCore(CreateComplexView("<local:TestBehavior Brush='{Binding Background}'/>", false));
        }
        void SetBehaviorsViaImplicitStyleCore(StackPanel panel) {
            Window.Content = panel;
            EnqueueShowWindow();
            EnqueueCallback(() => {
                Dictionary<Color, bool> dic = new Dictionary<Color, bool>() {
                    { Colors.Red, false },
                    { Colors.Blue, false },
                    { Colors.Green, false }
                };

                foreach(Border border in panel.Children) {
                    TestBehavior behavior = Interaction.GetBehaviors(border)[0] as TestBehavior;
                    dic[behavior.Brush.Color] = true;
                }

                Assert.IsTrue(dic.Values.All(x => x));
            });
            EnqueueTestComplete();
        }
        [Test]
        public void AddToExistedBehaviors_Test() {
            Interaction.GetBehaviors(TestControl).Add(new TestBehavior());
            LoadControlTemplate("<local:TestBehavior/><local:TestService/>");
            Assert.AreEqual(2, TestBehaviors.Count(x => x is TestBehavior));
            Assert.AreEqual(1, TestBehaviors.Count(x => x is TestService));
        }
        [Test]
        public void ThrowExceptionOnIncorrectTemplate_Test() {
            AssertHelper.AssertThrows<InvalidOperationException>(() => LoadControlTemplate("<local:TestBehavior/>", null),
                x => Assert.IsTrue(x.Message.IndexOf("Use ContentControl or ItemsControl") > -1));
        }
        [Test]
        public void BehaviorsRetainBindings_Test00() {
            var model = new TestViewModel();
            TestControl.DataContext = model;
            LoadControlTemplate("<local:TestService Command='{Binding EmptyCommand}'/>");
            Assert.IsNotNull(GetBindingExpression(TestBehaviors[0], TestService.CommandProperty));
            Assert.AreEqual(model.EmptyCommand, TestBehaviors.OfType<TestService>().First().Command);
        }
        [Test]
        public void BehaviorsRetainBindings_Test01() {
            var model = new TestViewModel();
            LoadControlTemplate("<local:TestService Command='{Binding EmptyCommand}'/>");
            TestControl.DataContext = model;
            WaitEvents();
            Assert.IsNotNull(GetBindingExpression(TestBehaviors[0], TestService.CommandProperty));
            Assert.AreEqual(model.EmptyCommand, TestBehaviors.OfType<TestService>().First().Command);
        }
        [Test]
        public void BehaviorsRetainBindings_Test02() {
            LoadControlTemplate("<local:TestBehavior TestName='TestName' Data='{Binding TestName, RelativeSource={RelativeSource Mode=Self}}'/>");
            WaitEvents();
            Assert.AreEqual("TestName", TestBehaviors.OfType<TestBehavior>().First().Data);
        }
        [Test]
        public void BehaviorsRetainBindings_Test03() {
            var model = new TestViewModel();
            TestControl.DataContext = model;
            LoadControlTemplate("<local:TestService Command='{Binding EmptyCommand}'/>", false);
            Assert.AreEqual(model.EmptyCommand, TestBehaviors.OfType<TestService>().First().Command);
        }
        [Test]
        public void BehaviorsRetainBindings_Test04() {
            var model = new TestViewModel();
            LoadControlTemplate("<local:TestService Command='{Binding EmptyCommand}'/>", false);
            TestControl.DataContext = model;
            WaitEvents();
            Assert.AreEqual(model.EmptyCommand, TestBehaviors.OfType<TestService>().First().Command);
            Assert.IsNotNull(GetBindingExpression(TestBehaviors[0], TestService.CommandProperty));
        }
        [Test]
        public void BehaviorsRetainBindings_Test05() {
            LoadControlTemplate("<local:TestBehavior TestName='TestName' Data='{Binding TestName, RelativeSource={RelativeSource Mode=Self}}'/>", false);
            WaitEvents();
            Assert.AreEqual("TestName", TestBehaviors.OfType<TestBehavior>().First().Data);
        }
        [Test]
        public void BehaviorsRetainBindings_Test06() {
            var container = new Border();
            container.Name = "container";
            container.Child = TestControl;
            LoadControlTemplate("<local:TestBehavior TestName='{Binding Name, RelativeSource={RelativeSource Mode=FindAncestor, AncestorType=Border}}'/>");
            WaitEvents();
            Assert.AreEqual(container.Name, TestBehaviors.OfType<TestBehavior>().First().TestName);
        }
        [Test]
        public void BehaviorsRetainBindings_Test07() {
            var container = new Border();
            container.Name = "container";
            container.Child = TestControl;
            LoadControlTemplate("<local:TestBehavior TestName='{Binding Name, RelativeSource={RelativeSource Mode=FindAncestor, AncestorType=Border}}'/>", false);
            WaitEvents();
            Assert.AreEqual(container.Name, TestBehaviors.OfType<TestBehavior>().First().TestName);
        }

        void WaitEvents() {
            DispatcherHelper.DoEvents();
        }

        BindingExpression GetBindingExpression(DependencyObject ob, DependencyProperty prop) {
            return BindingOperations.GetBindingExpression(ob, prop);
        }
    }
    public class TestBehavior : Behavior<DependencyObject> {
        public static readonly DependencyProperty TestNameProperty =
            DependencyProperty.Register("TestName", typeof(string), typeof(TestBehavior), new PropertyMetadata(null));
        public static readonly DependencyProperty DataProperty =
            DependencyProperty.Register("Data", typeof(string), typeof(TestBehavior), new PropertyMetadata(null));
        public static readonly DependencyProperty BrushProperty =
            DependencyProperty.Register("Brush", typeof(SolidColorBrush), typeof(TestBehavior), new PropertyMetadata(null));


        public SolidColorBrush Brush {
            get { return (SolidColorBrush)GetValue(BrushProperty); }
            set { SetValue(BrushProperty, value); }
        }
        public string TestName {
            get { return (string)GetValue(TestNameProperty); }
            set { SetValue(TestNameProperty, value); }
        }
        public string Data {
            get { return (string)GetValue(DataProperty); }
            set { SetValue(DataProperty, value); }
        }
    }
    public class TestService : ServiceBase {
        public static readonly DependencyProperty CommandProperty =
            DependencyProperty.Register("Command", typeof(ICommand), typeof(TestService), new PropertyMetadata(null));
        public ICommand Command {
            get { return (ICommand)GetValue(CommandProperty); }
            set { SetValue(CommandProperty, value); }
        }
    }
    class TestViewModel {
        public ICommand EmptyCommand { get; private set; }

        public TestViewModel() {
            EmptyCommand = new DelegateCommand(OnEmptyExecute);
        }

        void OnEmptyExecute() {
            throw new NotImplementedException();
        }
    }
}