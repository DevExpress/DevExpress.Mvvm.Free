using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Controls;
using System.Windows.Data;
using DevExpress.Mvvm.UI.Interactivity;
using NUnit.Framework;

namespace DevExpress.Mvvm.UI.Tests {
    [TestFixture]
    public class ValidationErrorsHostBehaviorTests : BaseWpfFixture {
        protected override void TearDownCore() {
            Interaction.GetBehaviors(Window).Clear();
            base.TearDownCore();
        }
        [Test]
        public void ErrorsPropertyCoerce() {
            ValidationErrorsHostBehavior host = new ValidationErrorsHostBehavior();
            Interaction.GetBehaviors(Window).Add(host);
            EnqueueShowWindow();
            host.Errors = null;
            Assert.IsNotNull(host.Errors);
        }
        [Test]
        public void HookEvents() {
            ValidationErrorsHostBehavior host = new ValidationErrorsHostBehavior();
            host.Errors = new List<ValidationError>();
            Interaction.GetBehaviors(Window).Add(host);
            EnqueueShowWindow();
            Assert.IsFalse(host.HasErrors);
            ValidationErrorEventArgs eventArgs = CreateValidationErrorEventArgs(new ValidationError(new ExceptionValidationRule(), new Binding()), ValidationErrorEventAction.Added);
            Window.RaiseEvent(eventArgs);
            Assert.IsTrue(eventArgs.Handled);
            Assert.IsTrue(host.HasErrors);
            host.Errors = new List<ValidationError>();
            Assert.IsFalse(host.HasErrors);
            host.Errors = new ObservableCollection<ValidationError>();
            Assert.IsFalse(host.HasErrors);
            host.Errors.Add(new ValidationError(new ExceptionValidationRule(), new Binding()));
            Assert.IsTrue(host.HasErrors);
        }
        [Test]
        public void BindHasErrors() {
            ValidationErrorsHostBehavior host = new ValidationErrorsHostBehavior();
            BindingOperations.SetBinding(host, ValidationErrorsHostBehavior.HasErrorsProperty, new Binding("ValidationHasErrors") { Source = this, Mode = BindingMode.OneWayToSource, UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged });
            host.Errors.Add(new ValidationError(new ExceptionValidationRule(), new Binding()));
            Assert.IsTrue(host.HasErrors);
            Assert.IsTrue(ValidationHasErrors);
        }
        public bool ValidationHasErrors { get; set; }
        ValidationErrorEventArgs CreateValidationErrorEventArgs(ValidationError error, ValidationErrorEventAction action) {
            ConstructorInfo constructor = typeof(ValidationErrorEventArgs).GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance, null, new Type[] { typeof(ValidationError), typeof(ValidationErrorEventAction) }, null);
            return (ValidationErrorEventArgs)constructor.Invoke(new object[] { error, action });
        }
    }
}