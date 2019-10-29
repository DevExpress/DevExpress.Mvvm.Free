using NUnit.Framework;
using System;
using System.Linq.Expressions;
using DevExpress.Mvvm.DataAnnotations;
using System.Windows.Input;
using DevExpress.Mvvm.Native;
using System.ComponentModel.DataAnnotations;
using System.Windows.Controls;
using System.Windows.Data;
using System.ComponentModel;
using System.Threading.Tasks;

namespace DevExpress.Mvvm.Tests {
    [TestFixture]
    public class ViewModelBaseTests {
        public interface IService1 { }
        public interface IService2 { }
        public class TestService1 : IService1 { }
        public class TestService2 : IService2 { }
        [Test]
        public void Interfaces() {
            var viewModel = new TestViewModel();
            var parentViewModel = new TestViewModel();
            Assert.IsNull(viewModel.ParentViewModelChangedValue);
            ((ISupportParentViewModel)viewModel).ParentViewModel = parentViewModel;
            Assert.AreEqual(parentViewModel, viewModel.ParentViewModelChangedValue);

            parentViewModel.ServiceContainer.RegisterService(new TestService1());
            Assert.IsNotNull(parentViewModel.ServiceContainer.GetService<IService1>());
            Assert.IsNotNull(viewModel.ServiceContainer.GetService<IService1>());
            Assert.IsNotNull(viewModel.GetService<IService1>());
            Assert.IsNull(viewModel.GetService<IService1>(ServiceSearchMode.LocalOnly));

            Assert.IsNull(viewModel.NavigatedToParameter);
            ((ISupportParameter)viewModel).Parameter = "test";
            Assert.AreEqual("test", viewModel.NavigatedToParameter);
            Assert.AreEqual("test", ((ISupportParameter)viewModel).Parameter);
        }
        [Test]
        public void NullParameterCausesOnParameterChanged() {
            var viewModel = new TestViewModel();
            Assert.IsNull(((ISupportParameter)viewModel).Parameter);
            Assert.AreEqual(0, viewModel.ParameterChangedCount);
            ((ISupportParameter)viewModel).Parameter = null;
            Assert.IsNull(((ISupportParameter)viewModel).Parameter);
            Assert.AreEqual(1, viewModel.ParameterChangedCount);
        }
        [Test]
        public void InitializeInDesignMode() {
            var viewModel = new TestViewModel();
            Assert.IsNull(((ISupportParameter)viewModel).Parameter);
            Assert.AreEqual(0, viewModel.ParameterChangedCount);
            viewModel.ForceInitializeInDesignMode();
            Assert.IsNull(((ISupportParameter)viewModel).Parameter);
            Assert.AreEqual(1, viewModel.ParameterChangedCount);
        }
        [Test]
        public void InitializeInRuntime() {
            ViewModelDesignHelper.IsInDesignModeOverride = true;
            var viewModel = new TestViewModel();
            Assert.AreEqual(0, viewModel.InitializeInRuntimeCount);

            ViewModelDesignHelper.IsInDesignModeOverride = false;
            var viewModel2 = new TestViewModel();
            Assert.AreEqual(1, viewModel2.InitializeInRuntimeCount);
        }
        #region command attrbute
        public abstract class CommandAttributeViewModelBaseCounters : ViewModelBase {
            public int BaseClassCommandCallCount;
            public int SimpleMethodCallCount;
            public int MethodWithCommandCallCount;
            public int CustomNameCommandCallCount;
            public bool MethodWithCanExecuteCanExcute = false;
            public int MethodWithReturnTypeCallCount;
            public int MethodWithParameterCallCount;
            public int MethodWithParameterLastParameter;
            public bool MethodWithCustomCanExecuteCanExcute = false;

            internal volatile bool SimpleAsyncMethodExecuting = false;
            internal volatile bool MethodWithCanExecuteAsyncExecuting = false;
            internal bool MethodWithCanExecuteAsyncCanExecute = false;
            internal volatile bool MethodWithParameterAsyncMethodExecuting = false;
            public int MethodWithParameterAsyncLastParameter;
            internal volatile bool MultiExecuteAsyncMethodExecuting = false;
        }
        public abstract class CommandAttributeViewModelBase : CommandAttributeViewModelBaseCounters {
            [Command]
            public void BaseClassCommand() { BaseClassCommandCallCount++; }
        }
        public class CommandAttributeViewModel : CommandAttributeViewModelBase {
            [Command]
            public void Simple() { SimpleMethodCallCount++; }
            [Command]
            public void MethodWithCommand() { MethodWithCommandCallCount++; }

            public void NoAttribute() { }

            [Command(Name = "MyCommand")]
            public void CustomName() { CustomNameCommandCallCount++; }

            [Command]
            public void MethodWithCanExecute() { }
            public bool CanMethodWithCanExecute() { return MethodWithCanExecuteCanExcute; }

            [Command]
            public int MethodWithReturnType() { MethodWithReturnTypeCallCount++; return 0; }

            [Command]
            public void MethodWithParameter(int parameter) { MethodWithParameterCallCount++; MethodWithParameterLastParameter = parameter; }
            public bool CanMethodWithParameter(int parameter) { return parameter != 13; }

            [Command(CanExecuteMethodName = "CanMethodWithCustomCanExecute_", UseCommandManager = false)]
            public void MethodWithCustomCanExecute() { }
            public bool CanMethodWithCustomCanExecute_() { return MethodWithCustomCanExecuteCanExcute; }

            [Command]
            public async Task SimpleAsync() {
                SimpleAsyncMethodExecuting = true;
                await Task.Run(() => { while(SimpleAsyncMethodExecuting) ; });
            }
            [Command]
            public async Task MethodWithCanExecuteAsync() {
                MethodWithCanExecuteAsyncExecuting = true;
                await Task.Run(() => { while(MethodWithCanExecuteAsyncExecuting) ; });
            }
            public bool CanMethodWithCanExecuteAsync() { return MethodWithCanExecuteAsyncCanExecute; }

            [Command(UseCommandManager = false)]
            public async Task MethodWithParameterAsync(int parameter) {
                MethodWithParameterAsyncLastParameter = parameter;
                await Task.Run(() => { while(MethodWithParameterAsyncMethodExecuting) ; });
            }
            public bool CanMethodWithParameterAsync(int parameter) { return parameter != 13; }


            [AsyncCommand(AllowMultipleExecution = true)]
            public async Task MultiExecuteAsync() {
                MultiExecuteAsyncMethodExecuting = true;
                await Task.Run(() => { while(MultiExecuteAsyncMethodExecuting) ; });
            }
        }
        [MetadataType(typeof(CommandAttributeViewModelMetadata))]
        public class CommandAttributeViewModel_FluentAPI : CommandAttributeViewModelBaseCounters {
            public void BaseClassCommand() { BaseClassCommandCallCount++; }
            public void Simple() { SimpleMethodCallCount++; }
            public void MethodWithCommand() { MethodWithCommandCallCount++; }
            public void NoAttribute() { }
            public void CustomName() { CustomNameCommandCallCount++; }
            public void MethodWithCanExecute() { }
            public bool CanMethodWithCanExecute() { return MethodWithCanExecuteCanExcute; }
            public int MethodWithReturnType() { MethodWithReturnTypeCallCount++; return 0; }
            public void MethodWithParameter(int parameter) { MethodWithParameterCallCount++; MethodWithParameterLastParameter = parameter; }
            public bool CanMethodWithParameter(int parameter) { return parameter != 13; }
            public void MethodWithCustomCanExecute() { }

            public bool CanMethodWithCustomCanExecute_(int x) { throw new InvalidOperationException(); }
            public bool CanMethodWithCustomCanExecute_() { return MethodWithCustomCanExecuteCanExcute; }

            public async Task SimpleAsync() {
                SimpleAsyncMethodExecuting = true;
                await Task.Run(() => { while(SimpleAsyncMethodExecuting) ; });
            }

            public async Task MethodWithCanExecuteAsync() {
                MethodWithCanExecuteAsyncExecuting = true;
                await Task.Run(() => { while(MethodWithCanExecuteAsyncExecuting) ; });
            }
            public bool CanMethodWithCanExecuteAsync() { return MethodWithCanExecuteAsyncCanExecute; }

            public async Task MethodWithParameterAsync(int parameter) {
                MethodWithParameterAsyncLastParameter = parameter;
                await Task.Run(() => { while(MethodWithParameterAsyncMethodExecuting) ; });
            }
            public bool CanMethodWithParameterAsync(int parameter) { return parameter != 13; }

            public async Task MultiExecuteAsync() {
                MultiExecuteAsyncMethodExecuting = true;
                await Task.Run(() => { while(MultiExecuteAsyncMethodExecuting) ; });
            }
        }
        public class CommandAttributeViewModelMetadata : IMetadataProvider<CommandAttributeViewModel_FluentAPI> {
            void IMetadataProvider<CommandAttributeViewModel_FluentAPI>.BuildMetadata(MetadataBuilder<CommandAttributeViewModel_FluentAPI> builder) {
                builder.CommandFromMethod(x => x.BaseClassCommand());
                builder.CommandFromMethod(x => x.Simple());
                builder.CommandFromMethod(x => x.MethodWithCommand());
                builder.CommandFromMethod(x => x.CustomName()).CommandName("MyCommand");
                builder.CommandFromMethod(x => x.MethodWithCanExecute());
                builder.CommandFromMethod(x => x.MethodWithReturnType());
                builder.CommandFromMethod(x => x.MethodWithParameter(default(int)));
                builder.CommandFromMethod(x => x.MethodWithCustomCanExecute())
                    .DoNotUseCommandManager()
                    .CanExecuteMethod(x => x.CanMethodWithCustomCanExecute_())
                    .DoNotUseCommandManager();

                builder.CommandFromMethod(x => x.SimpleAsync());
                builder.CommandFromMethod(x => x.MethodWithCanExecuteAsync());
                builder.CommandFromMethod(x => x.MethodWithParameterAsync(default(int)))
                    .DoNotUseCommandManager();
                builder.CommandFromMethod(x => x.MultiExecuteAsync())
                    .AllowMultipleExecution();
            }
        }
        [Test]
        public void CommandAttribute_ViewModelTest() {
            var viewModel = new CommandAttributeViewModel();
            CommandAttribute_ViewModelTestCore(viewModel, () => viewModel.MethodWithCanExecute(), () => viewModel.MethodWithCustomCanExecute(), () => viewModel.MethodWithCanExecuteAsync());
            viewModel = new CommandAttributeViewModel();
            CommandAttribute_ViewModelTestCore(viewModel, () => viewModel.MethodWithCanExecute(), () => viewModel.MethodWithCustomCanExecute(), () => viewModel.MethodWithCanExecuteAsync());
        }
        [Test]
        public void CommandAttribute_ViewModelTest_FluentAPI() {
            var viewModel = new CommandAttributeViewModel_FluentAPI();
            CommandAttribute_ViewModelTestCore(viewModel, () => viewModel.MethodWithCanExecute(), () => viewModel.MethodWithCustomCanExecute(), () => viewModel.MethodWithCanExecuteAsync());
            Assert.AreSame(((ICustomTypeDescriptor)viewModel).GetProperties(), ((ICustomTypeDescriptor)viewModel).GetProperties());
        }
        void CommandAttribute_ViewModelTestCore(CommandAttributeViewModelBaseCounters viewModel, Expression<Action> methodWithCanExecuteExpression, Expression<Action> methodWithCustomCanExecuteExpression, Expression<Func<Task>> methodWithCanExecuteAsyncExpression) {
            var button = new Button() { DataContext = viewModel };

            button.SetBinding(Button.CommandProperty, new Binding("SimpleCommand"));
            button.Command.Execute(null);
            Assert.AreEqual(1, viewModel.SimpleMethodCallCount);

            button.SetBinding(Button.CommandProperty, new Binding("NoAttributeCommand"));
            Assert.IsNull(button.Command);

            button.SetBinding(Button.CommandProperty, new Binding("MethodWithCommand"));
            button.Command.Execute(null);
            Assert.AreEqual(1, viewModel.MethodWithCommandCallCount);

            button.SetBinding(Button.CommandProperty, new Binding("MyCommand"));
            button.Command.Execute(null);
            Assert.AreEqual(1, viewModel.CustomNameCommandCallCount);

            button.SetBinding(Button.CommandProperty, new Binding("BaseClassCommand"));
            button.Command.Execute(null);
            Assert.AreEqual(1, viewModel.BaseClassCommandCallCount);
            Assert.IsTrue(button.IsEnabled);

            button.SetBinding(Button.CommandProperty, new Binding("MethodWithCanExecuteCommand"));
            Assert.IsFalse(button.IsEnabled);
            viewModel.MethodWithCanExecuteCanExcute = true;
            DispatcherHelper.DoEvents();
            Assert.IsFalse(button.IsEnabled);
            viewModel.RaiseCanExecuteChanged(methodWithCanExecuteExpression);

            Assert.IsFalse(button.IsEnabled);
            DispatcherHelper.DoEvents();
            Assert.IsTrue(button.IsEnabled);

            button.SetBinding(Button.CommandProperty, new Binding("MethodWithReturnTypeCommand"));
            button.Command.Execute(null);
            Assert.AreEqual(1, viewModel.MethodWithReturnTypeCallCount);

            button.SetBinding(Button.CommandProperty, new Binding("MethodWithParameterCommand"));
            button.Command.Execute(9);
            Assert.AreEqual(1, viewModel.MethodWithParameterCallCount);
            Assert.AreEqual(9, viewModel.MethodWithParameterLastParameter);
            Assert.IsTrue(button.Command.CanExecute(9));
            Assert.IsFalse(button.Command.CanExecute(13));
            button.Command.Execute("10");
            Assert.AreEqual(2, viewModel.MethodWithParameterCallCount);
            Assert.AreEqual(10, viewModel.MethodWithParameterLastParameter);

            button.SetBinding(Button.CommandProperty, new Binding("MethodWithCustomCanExecuteCommand"));
            Assert.IsFalse(button.IsEnabled);
            viewModel.MethodWithCustomCanExecuteCanExcute = true;
            Assert.IsFalse(button.IsEnabled);
            viewModel.RaiseCanExecuteChanged(methodWithCustomCanExecuteExpression);
            Assert.IsTrue(button.IsEnabled);

            button.SetBinding(Button.CommandProperty, new Binding("SimpleAsyncCommand"));
            Assert.True(button.IsEnabled);
            button.Command.Execute(null);
            Assert.True(button.IsEnabled);
            DispatcherHelper.DoEvents();
            Assert.False(button.IsEnabled);
            Assert.True(viewModel.SimpleAsyncMethodExecuting);
            viewModel.SimpleAsyncMethodExecuting = false;
            while(((IAsyncCommand)button.Command).IsExecuting)
                DispatcherHelper.DoEvents();
            DispatcherHelper.DoEvents();
            Assert.True(button.IsEnabled);

            button.SetBinding(Button.CommandProperty, new Binding("MethodWithCanExecuteAsyncCommand"));
            Assert.IsFalse(button.IsEnabled);
            viewModel.MethodWithCanExecuteAsyncCanExecute = true;
            viewModel.RaiseCanExecuteChanged(methodWithCanExecuteAsyncExpression);
            Assert.IsFalse(button.IsEnabled);
            DispatcherHelper.DoEvents();
            Assert.IsTrue(button.IsEnabled);
            viewModel.MethodWithCanExecuteAsyncCanExecute = false;
            viewModel.RaiseCanExecuteChanged(methodWithCanExecuteAsyncExpression);
            Assert.IsTrue(button.IsEnabled);
            DispatcherHelper.DoEvents();
            Assert.IsFalse(button.IsEnabled);

            button.SetBinding(Button.CommandProperty, new Binding("MethodWithParameterAsyncCommand"));
            Assert.IsTrue(button.Command.CanExecute(9));
            Assert.IsFalse(button.Command.CanExecute(13));
            button.Command.Execute(9);
            Assert.AreEqual(9, viewModel.MethodWithParameterAsyncLastParameter);
            Assert.False(button.IsEnabled);
            viewModel.MethodWithParameterAsyncMethodExecuting = false;
            while(((IAsyncCommand)button.Command).IsExecuting)
                DispatcherHelper.DoEvents();
            Assert.True(button.IsEnabled);

            button.SetBinding(Button.CommandProperty, new Binding("MultiExecuteAsyncCommand"));
            Assert.True(button.IsEnabled);
            button.Command.Execute(null);
            Assert.True(button.IsEnabled);
            DispatcherHelper.DoEvents();
            Assert.True(button.IsEnabled);
            Assert.True(viewModel.MultiExecuteAsyncMethodExecuting);
            viewModel.MultiExecuteAsyncMethodExecuting = false;
            while(((IAsyncCommand)button.Command).IsExecuting)
                DispatcherHelper.DoEvents();
            DispatcherHelper.DoEvents();
            Assert.True(button.IsEnabled);

            button.Command = null;
        }
        #region exceptions
#pragma warning disable 0618
        public class NameConflictViewModel : ViewModelBase {
            [Command]
            public void Simple() { }
            public ICommand SimpleCommand { get; private set; }
        }
        [Test]
        public void CommandAttribute_NameConflictTest() {
            AssertHelper.AssertThrows<CommandAttributeException>(() => {
                new NameConflictViewModel();
            }, x => Assert.AreEqual("Property with the same name already exists: SimpleCommand.", x.Message));
        }

        public class DuplicateNamesViewModel : ViewModelBase {
            [Command(Name = "MyCommand")]
            public void Method1() { }
            [Command(Name = "MyCommand")]
            public void Method2() { }
        }
        [Test]
        public void CommandAttribute_DuplicateNamesTest() {
            AssertHelper.AssertThrows<CommandAttributeException>(() => {
                new DuplicateNamesViewModel();
            }, x => Assert.AreEqual("Property with the same name already exists: MyCommand.", x.Message));
        }

        public class NotPublicMethodViewModel : ViewModelBase {
            [Command]
            protected internal void NotPublicMethod() { }
        }
        [Test]
        public void CommandAttribute_NotPublicMethodTest() {
            AssertHelper.AssertThrows<CommandAttributeException>(() => {
                new NotPublicMethodViewModel();
            }, x => Assert.AreEqual("Method should be public: NotPublicMethod.", x.Message));
        }

        public class TooMuchArgumentsMethodViewModel : ViewModelBase {
            [Command]
            public void TooMuchArgumentsMethod(int a, int b) { }
        }
        [Test]
        public void CommandAttribute_TooMuchArgumentsMethodTest() {
            AssertHelper.AssertThrows<CommandAttributeException>(() => {
                new TooMuchArgumentsMethodViewModel();
            }, x => Assert.AreEqual("Method cannot have more than one parameter: TooMuchArgumentsMethod.", x.Message));
        }

        public class OutParameterMethodViewModel : ViewModelBase {
            [Command]
            public void OutParameterMethod(out int a) { a = 0; }
        }
        [Test]
        public void CommandAttribute_OutParameterMethodTest() {
            AssertHelper.AssertThrows<CommandAttributeException>(() => {
                new OutParameterMethodViewModel();
            }, x => Assert.AreEqual("Method cannot have out or reference parameter: OutParameterMethod.", x.Message));
        }

        public class RefParameterMethodViewModel : ViewModelBase {
            [Command]
            public void RefParameterMethod(ref int a) { a = 0; }
        }
        [Test]
        public void CommandAttribute_RefParameterMethodTest() {
            AssertHelper.AssertThrows<CommandAttributeException>(() => {
                new RefParameterMethodViewModel();
            }, x => Assert.AreEqual("Method cannot have out or reference parameter: RefParameterMethod.", x.Message));
        }

        public class CanExecuteParameterCountMismatchViewModel : ViewModelBase {
            [Command]
            public void Method() { }
            public bool CanMethod(int a) { return true; }
        }
        [Test]
        public void CommandAttribute_CanExecuteParameterCountMismatchTest() {
            AssertHelper.AssertThrows<CommandAttributeException>(() => {
                new CanExecuteParameterCountMismatchViewModel();
            }, x => Assert.AreEqual("Can execute method has incorrect parameters: CanMethod.", x.Message));
        }

        public class CanExecuteParametersMismatchViewModel : ViewModelBase {
            [Command]
            public void Method(long a) { }
            public bool CanMethod(int a) { return true; }
        }
        [Test]
        public void CommandAttribute_CanExecuteParametersMismatchTest() {
            AssertHelper.AssertThrows<CommandAttributeException>(() => {
                new CanExecuteParametersMismatchViewModel();
            }, x => Assert.AreEqual("Can execute method has incorrect parameters: CanMethod.", x.Message));
        }

        public class CanExecuteParametersMismatchViewModel2 : ViewModelBase {
            [Command]
            public void Method(int a) { }
            public bool CanMethod(out int a) { a = 0; return true; }
        }
        [Test]
        public void CommandAttribute_CanExecuteParametersMismatchTest2() {
            AssertHelper.AssertThrows<CommandAttributeException>(() => {
                new CanExecuteParametersMismatchViewModel2();
            }, x => Assert.AreEqual("Can execute method has incorrect parameters: CanMethod.", x.Message));
        }

        public class NotPublicCanExecuteViewModel : ViewModelBase {
            [Command]
            public void Method() { }
            bool CanMethod() { return true; }
        }
        [Test]
        public void CommandAttribute_NotPublicCanExecuteTest() {
            AssertHelper.AssertThrows<CommandAttributeException>(() => {
                new NotPublicCanExecuteViewModel();
            }, x => Assert.AreEqual("Method should be public: CanMethod.", x.Message));
        }

        public class InvalidCanExecuteMethodNameViewModel : ViewModelBase {
            [Command(CanExecuteMethodName = "CanMethod_")]
            public void Method() { }
        }
        [Test]
        public void CommandAttribute_InvalidCanExecuteMethodNameTest() {
            AssertHelper.AssertThrows<CommandAttributeException>(() => {
                new InvalidCanExecuteMethodNameViewModel();
            }, x => Assert.AreEqual("Method not found: CanMethod_.", x.Message));
        }
#pragma warning restore 0618
        #endregion
        #endregion

        [Test]
        public void T370425() {
            var vm = new TestViewModel();
            AssertHelper.AssertThrows<InvalidOperationException>(() => ((ISupportParentViewModel)vm).ParentViewModel = vm,
                e => Assert.AreEqual("ViewModel cannot be parent of itself.", e.Message));
        }
    }
    public class TestViewModel : ViewModelBase {
        public new IServiceContainer ServiceContainer { get { return base.ServiceContainer; } }

        public object ParentViewModelChangedValue { get; private set; }
        protected override void OnParentViewModelChanged(object parentViewModel) {
            ParentViewModelChangedValue = parentViewModel;
            base.OnParentViewModelChanged(parentViewModel);
        }

        public object NavigatedToParameter { get; private set; }
        protected override void OnParameterChanged(object parameter) {
            NavigatedToParameter = parameter;
            ParameterChangedCount++;
            base.OnParameterChanged(parameter);
        }
        public new T GetService<T>(ServiceSearchMode searchMode = ServiceSearchMode.PreferLocal) where T : class {
            return base.GetService<T>(searchMode);
        }

        public int ParameterChangedCount { get; private set; }

        public void ForceInitializeInDesignMode() {
            OnInitializeInDesignMode();
        }

        public int InitializeInRuntimeCount { get; private set; }
        protected override void OnInitializeInRuntime() {
            base.OnInitializeInRuntime();
            InitializeInRuntimeCount++;
        }
    }
}