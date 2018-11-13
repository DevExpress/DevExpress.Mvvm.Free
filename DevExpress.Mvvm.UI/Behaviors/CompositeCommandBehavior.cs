using DevExpress.Mvvm.Native;
using DevExpress.Mvvm.UI.Interactivity;
using DevExpress.Mvvm.UI.Native;
using System;
using System.Collections.Specialized;
using System.Reflection;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Markup;
using System.Linq;

namespace DevExpress.Mvvm.UI {
    [ContentProperty("Commands")]
    public class CompositeCommandBehavior : Behavior<DependencyObject> {
        #region Inner classes

        class CompositeCommandCanExecuteConverter : IMultiValueConverter {
            readonly CompositeCommandExecuteCondition canExecuteCondition;

            public CompositeCommandCanExecuteConverter(CompositeCommandExecuteCondition canExecuteCondition) {
                this.canExecuteCondition = canExecuteCondition;
            }

            public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
                if(values == null || values.Length == 0)
                    return false;
                if(canExecuteCondition == CompositeCommandExecuteCondition.AllCommandsCanBeExecuted)
                    return values.Cast<bool>().All(x => x);
                if(canExecuteCondition == CompositeCommandExecuteCondition.AnyCommandCanBeExecuted)
                    return values.Cast<bool>().Any(x => x);
                throw new InvalidOperationException();
            }
            public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture) {
                throw new NotImplementedException();
            }
        }

        #endregion
        #region Static
        public static readonly DependencyProperty CommandPropertyNameProperty;
        public static readonly DependencyProperty CanExecuteConditionProperty;
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Code Defects", "DXCA001")]
        [IgnoreDependencyPropertiesConsistencyChecker]
        static readonly DependencyProperty CanExecuteProperty;
        [IgnoreDependencyPropertiesConsistencyChecker]
        static readonly DependencyProperty InternalItemsProperty;

        static CompositeCommandBehavior() {
            Type owner = typeof(CompositeCommandBehavior);
            CommandPropertyNameProperty = DependencyProperty.Register("CommandPropertyName", typeof(string), owner,
                new PropertyMetadata("Command", (d, e) => ((CompositeCommandBehavior)d).OnCommandPropertyNameChanged(e)));
            CanExecuteProperty = DependencyProperty.Register("CanExecute", typeof(bool), owner,
                new PropertyMetadata(false, (d, e) => ((CompositeCommandBehavior)d).OnCanExecuteChanged(e)));
            InternalItemsProperty = DependencyProperty.RegisterAttached("InternalItems", typeof(CommandsCollection), owner,
                new PropertyMetadata(null));
            CanExecuteConditionProperty = DependencyProperty.Register("CanExecuteCondition", typeof(CompositeCommandExecuteCondition), owner,
                new PropertyMetadata(CompositeCommandExecuteCondition.AllCommandsCanBeExecuted, (d, e) => ((CompositeCommandBehavior)d).UpdateCanExecuteBinding()));
        }
        #endregion

        #region Dependency Properties
        public string CommandPropertyName {
            get { return (string)GetValue(CommandPropertyNameProperty); }
            set { SetValue(CommandPropertyNameProperty, value); }
        }
        bool CanExecute {
            get { return (bool)GetValue(CanExecuteProperty); }
            set { SetValue(CanExecuteProperty, value); }
        }
        public CompositeCommandExecuteCondition CanExecuteCondition {
            get { return (CompositeCommandExecuteCondition)GetValue(CanExecuteConditionProperty); }
            set { SetValue(CanExecuteConditionProperty, value); }
        }

        #endregion

        #region Props
        public DelegateCommand<object> CompositeCommand { get; private set; }
        public CommandsCollection Commands { get; private set; }
        #endregion

        public CompositeCommandBehavior() {
            CompositeCommand = new DelegateCommand<object>(CompositeCommandExecute, CompositeCommandCanExecute, false);
            Commands = new CommandsCollection();
            ((INotifyCollectionChanged)Commands).CollectionChanged += OnCommandsChanged;
        }

        #region Commands
        void OnCommandsChanged(object sender, NotifyCollectionChangedEventArgs e) {
            UpdateCanExecuteBinding();
        }
        void UpdateCanExecuteBinding() {
            MultiBinding multiBinding = new MultiBinding() {
                Converter = new CompositeCommandCanExecuteConverter(CanExecuteCondition)
            };

            for(int i = 0; i < Commands.Count; i++) {
                multiBinding.Bindings.Add(new Binding("CanExecute") {
                    Mode = BindingMode.OneWay,
                    Source = Commands[i]
                });
            }

            BindingOperations.SetBinding(this, CanExecuteProperty, multiBinding);
        }
        void CompositeCommandExecute(object parameter) {
            if(!CanExecute)
                return;

            foreach(CommandItem item in Commands)
                item.ExecuteCommand();
        }
        bool CompositeCommandCanExecute(object parameter) {
            return CanExecute;
        }
        void RaiseCompositeCommandCanExecuteChanged() {
            CompositeCommand.RaiseCanExecuteChanged();
        }
        void OnCanExecuteChanged(DependencyPropertyChangedEventArgs e) {
            RaiseCompositeCommandCanExecuteChanged();
        }
        #endregion

        #region Attach|Detach command
        protected override void OnAttached() {
            base.OnAttached();
            SetAssociatedObjectCommandProperty(CommandPropertyName);
            AssociatedObject.SetValue(InternalItemsProperty, Commands);

        }
        protected override void OnDetaching() {
            ReleaseAssociatedObjectCommandProperty(CommandPropertyName);
            AssociatedObject.SetValue(InternalItemsProperty, null);
            base.OnDetaching();
        }
        void OnCommandPropertyNameChanged(DependencyPropertyChangedEventArgs e) {
            if(!IsAttached)
                return;

            ReleaseAssociatedObjectCommandProperty(e.OldValue as string);
            SetAssociatedObjectCommandProperty(e.NewValue as string);
        }
        PropertyInfo GetCommandProperty(string propName) {
            return ObjectPropertyHelper.GetPropertyInfoSetter(AssociatedObject, propName);
        }
        DependencyProperty GetCommandDependencyProperty(string propName) {
            return ObjectPropertyHelper.GetDependencyProperty(AssociatedObject, propName);
        }
        void SetAssociatedObjectCommandProperty(string propName) {
            var pi = GetCommandProperty(propName);
            if(pi != null)
                pi.SetValue(AssociatedObject, CompositeCommand, null);
            else
                GetCommandDependencyProperty(propName).Do(x => AssociatedObject.SetValue(x, CompositeCommand));
        }
        void ReleaseAssociatedObjectCommandProperty(string propName) {
            var pi = GetCommandProperty(propName);
            if(pi != null) {
                if(pi.GetValue(AssociatedObject, null) == CompositeCommand)
                    pi.SetValue(AssociatedObject, null, null);
            } else {
                DependencyProperty commandProperty = GetCommandDependencyProperty(propName);
                if(commandProperty != null && AssociatedObject.GetValue(commandProperty) == CompositeCommand)
                    AssociatedObject.SetValue(commandProperty, null);
            }
        }
        #endregion
    }

    public class CommandsCollection : FreezableCollection<CommandItem> { }

    public class CommandItem : DependencyObjectExt {
        #region Static
        public static readonly DependencyProperty CommandProperty;
        public static readonly DependencyProperty CommandParameterProperty;
        public static readonly DependencyProperty CheckCanExecuteProperty;
        public static readonly DependencyProperty CanExecuteProperty;
        static readonly DependencyPropertyKey CanExecutePropertyKey;

        static CommandItem() {
            Type owner = typeof(CommandItem);
            CommandProperty = DependencyProperty.Register("Command", typeof(ICommand), owner,
                new PropertyMetadata(null, (d, e) => ((CommandItem)d).OnCommandChanged(e)));
            CommandParameterProperty = DependencyProperty.Register("CommandParameter", typeof(object), owner,
                new PropertyMetadata(null, (d, e) => ((CommandItem)d).UpdateCanExecute()));
            CheckCanExecuteProperty = DependencyProperty.Register("CheckCanExecute", typeof(bool), owner,
                new PropertyMetadata(true, (d, e) => ((CommandItem)d).UpdateCanExecute()));
            CanExecutePropertyKey = DependencyProperty.RegisterReadOnly("CanExecute", typeof(bool), owner,
                new PropertyMetadata(false));
            CanExecuteProperty = CanExecutePropertyKey.DependencyProperty;
        }
        #endregion

        #region Dependency Properties
        public ICommand Command {
            get { return (ICommand)GetValue(CommandProperty); }
            set { SetValue(CommandProperty, value); }
        }
        public object CommandParameter {
            get { return GetValue(CommandParameterProperty); }
            set { SetValue(CommandParameterProperty, value); }
        }
        public bool CheckCanExecute {
            get { return (bool)GetValue(CheckCanExecuteProperty); }
            set { SetValue(CheckCanExecuteProperty, value); }
        }
        public bool CanExecute {
            get { return (bool)GetValue(CanExecuteProperty); }
            private set { SetValue(CanExecutePropertyKey, value); }
        }
        #endregion

        CanExecuteChangedEventHandler<CommandItem> commandCanExecuteChangedHandler;

        public CommandItem() {
            this.commandCanExecuteChangedHandler = new CanExecuteChangedEventHandler<CommandItem>(this, (owner, o, e) => owner.OnCommandCanExecuteChanged(o, e));
        }

        public bool ExecuteCommand() {
            if(!CanExecute)
                return false;

            Command.Execute(CommandParameter);
            return true;
        }
        void OnCommandChanged(DependencyPropertyChangedEventArgs e) {
            e.OldValue.With(x => x as ICommand).Do(o => o.CanExecuteChanged -= this.commandCanExecuteChangedHandler.Handler);
            e.NewValue.With(x => x as ICommand).Do(o => o.CanExecuteChanged += this.commandCanExecuteChangedHandler.Handler);
            UpdateCanExecute();
        }
        void OnCommandCanExecuteChanged(object sender, EventArgs e) {
            UpdateCanExecute();
        }
        void UpdateCanExecute() {
            CanExecute = Command != null && (!CheckCanExecute || Command.CanExecute(CommandParameter));
        }
    }
    public enum CompositeCommandExecuteCondition { AllCommandsCanBeExecuted, AnyCommandCanBeExecuted }
}