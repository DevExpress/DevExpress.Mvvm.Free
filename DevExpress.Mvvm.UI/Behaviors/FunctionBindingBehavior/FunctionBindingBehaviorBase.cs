using DevExpress.Mvvm.UI.Interactivity;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using DevExpress.Mvvm.Native;
using System.Collections.Generic;
using System.Reflection;
using System.Text.RegularExpressions;
using DevExpress.Mvvm.UI.Native;
using System.ComponentModel;

namespace DevExpress.Mvvm.UI {
    public abstract class FunctionBindingBehaviorBase : Behavior<DependencyObject> {
        #region error messages
        protected const string Error_PropertyNotFound = "Cannot find property with name {1} in the {0} class.";
        protected const string Error_SourceMethodNotFound = "FunctionBindingBehaviorBase error: Cannot find function with name {1} in the {0} class.";
        protected const string Error_SourceMethodReturnVoid = "The return value of the '{0}.{1}' function can't be void.";
        #endregion
        #region Dependency Properties
        public static readonly DependencyProperty SourceProperty =
            DependencyProperty.Register("Source", typeof(object), typeof(FunctionBindingBehaviorBase),
            new PropertyMetadata(null, (d, e) => ((FunctionBindingBehaviorBase)d).OnSourceChanged()));
        public static readonly DependencyProperty TargetProperty =
            DependencyProperty.Register("Target", typeof(object), typeof(FunctionBindingBehaviorBase),
            new PropertyMetadata(null, (d, e) => ((FunctionBindingBehaviorBase)d).OnTargetChanged(e)));
        public static readonly DependencyProperty Arg1Property =
            DependencyProperty.Register("Arg1", typeof(object), typeof(FunctionBindingBehaviorBase),
            new PropertyMetadata(null, (d, e) => ((FunctionBindingBehaviorBase)d).OnResultAffectedPropertyChanged()));
        public static readonly DependencyProperty Arg2Property =
            DependencyProperty.Register("Arg2", typeof(object), typeof(FunctionBindingBehaviorBase),
            new PropertyMetadata(null, (d, e) => ((FunctionBindingBehaviorBase)d).OnResultAffectedPropertyChanged()));
        public static readonly DependencyProperty Arg3Property =
            DependencyProperty.Register("Arg3", typeof(object), typeof(FunctionBindingBehaviorBase),
            new PropertyMetadata(null, (d, e) => ((FunctionBindingBehaviorBase)d).OnResultAffectedPropertyChanged()));
        public static readonly DependencyProperty Arg4Property =
            DependencyProperty.Register("Arg4", typeof(object), typeof(FunctionBindingBehaviorBase),
            new PropertyMetadata(null, (d, e) => ((FunctionBindingBehaviorBase)d).OnResultAffectedPropertyChanged()));
        public static readonly DependencyProperty Arg5Property =
            DependencyProperty.Register("Arg5", typeof(object), typeof(FunctionBindingBehaviorBase),
            new PropertyMetadata(null, (d, e) => ((FunctionBindingBehaviorBase)d).OnResultAffectedPropertyChanged()));
        public static readonly DependencyProperty Arg6Property =
            DependencyProperty.Register("Arg6", typeof(object), typeof(FunctionBindingBehaviorBase),
            new PropertyMetadata(null, (d, e) => ((FunctionBindingBehaviorBase)d).OnResultAffectedPropertyChanged()));
        public static readonly DependencyProperty Arg7Property =
            DependencyProperty.Register("Arg7", typeof(object), typeof(FunctionBindingBehaviorBase),
            new PropertyMetadata(null, (d, e) => ((FunctionBindingBehaviorBase)d).OnResultAffectedPropertyChanged()));
        public static readonly DependencyProperty Arg8Property =
            DependencyProperty.Register("Arg8", typeof(object), typeof(FunctionBindingBehaviorBase),
            new PropertyMetadata(null, (d, e) => ((FunctionBindingBehaviorBase)d).OnResultAffectedPropertyChanged()));
        public static readonly DependencyProperty Arg9Property =
            DependencyProperty.Register("Arg9", typeof(object), typeof(FunctionBindingBehaviorBase),
            new PropertyMetadata(null, (d, e) => ((FunctionBindingBehaviorBase)d).OnResultAffectedPropertyChanged()));
        public static readonly DependencyProperty Arg10Property =
            DependencyProperty.Register("Arg10", typeof(object), typeof(FunctionBindingBehaviorBase),
            new PropertyMetadata(null, (d, e) => ((FunctionBindingBehaviorBase)d).OnResultAffectedPropertyChanged()));
        public static readonly DependencyProperty Arg11Property =
            DependencyProperty.Register("Arg11", typeof(object), typeof(FunctionBindingBehaviorBase),
            new PropertyMetadata(null, (d, e) => ((FunctionBindingBehaviorBase)d).OnResultAffectedPropertyChanged()));
        public static readonly DependencyProperty Arg12Property =
            DependencyProperty.Register("Arg12", typeof(object), typeof(FunctionBindingBehaviorBase),
            new PropertyMetadata(null, (d, e) => ((FunctionBindingBehaviorBase)d).OnResultAffectedPropertyChanged()));
        public static readonly DependencyProperty Arg13Property =
            DependencyProperty.Register("Arg13", typeof(object), typeof(FunctionBindingBehaviorBase),
            new PropertyMetadata(null, (d, e) => ((FunctionBindingBehaviorBase)d).OnResultAffectedPropertyChanged()));
        public static readonly DependencyProperty Arg14Property =
            DependencyProperty.Register("Arg14", typeof(object), typeof(FunctionBindingBehaviorBase),
            new PropertyMetadata(null, (d, e) => ((FunctionBindingBehaviorBase)d).OnResultAffectedPropertyChanged()));
        public static readonly DependencyProperty Arg15Property =
            DependencyProperty.Register("Arg15", typeof(object), typeof(FunctionBindingBehaviorBase),
            new PropertyMetadata(null, (d, e) => ((FunctionBindingBehaviorBase)d).OnResultAffectedPropertyChanged()));

        public object Source {
            get { return GetValue(SourceProperty); }
            set { SetValue(SourceProperty, value); }
        }
        public object Target {
            get { return GetValue(TargetProperty); }
            set { SetValue(TargetProperty, value); }
        }
        public object Arg1 {
            get { return GetValue(Arg1Property); }
            set { SetValue(Arg1Property, value); }
        }
        public object Arg2 {
            get { return GetValue(Arg2Property); }
            set { SetValue(Arg2Property, value); }
        }
        public object Arg3 {
            get { return GetValue(Arg3Property); }
            set { SetValue(Arg3Property, value); }
        }
        public object Arg4 {
            get { return GetValue(Arg4Property); }
            set { SetValue(Arg4Property, value); }
        }
        public object Arg5 {
            get { return GetValue(Arg5Property); }
            set { SetValue(Arg5Property, value); }
        }
        public object Arg6 {
            get { return GetValue(Arg6Property); }
            set { SetValue(Arg6Property, value); }
        }
        public object Arg7 {
            get { return GetValue(Arg7Property); }
            set { SetValue(Arg7Property, value); }
        }
        public object Arg8 {
            get { return GetValue(Arg8Property); }
            set { SetValue(Arg8Property, value); }
        }
        public object Arg9 {
            get { return GetValue(Arg9Property); }
            set { SetValue(Arg9Property, value); }
        }
        public object Arg10 {
            get { return GetValue(Arg10Property); }
            set { SetValue(Arg10Property, value); }
        }
        public object Arg11 {
            get { return GetValue(Arg11Property); }
            set { SetValue(Arg11Property, value); }
        }
        public object Arg12 {
            get { return GetValue(Arg12Property); }
            set { SetValue(Arg12Property, value); }
        }
        public object Arg13 {
            get { return GetValue(Arg13Property); }
            set { SetValue(Arg13Property, value); }
        }
        public object Arg14 {
            get { return GetValue(Arg14Property); }
            set { SetValue(Arg14Property, value); }
        }
        public object Arg15 {
            get { return GetValue(Arg15Property); }
            set { SetValue(Arg15Property, value); }
        }
        #endregion

        static readonly Regex argNameRegExp = new Regex(@"^Arg\d", RegexOptions.Compiled);
        static readonly Regex argOrderRegExp = new Regex(@"\d+", RegexOptions.Compiled);
        protected object ActualSource { get; private set; }
        protected abstract string ActualFunction { get; }
        protected object ActualTarget { get { return Target ?? AssociatedObject; } }

        protected override void OnAttached() {
            base.OnAttached();
            (AssociatedObject as FrameworkElement).Do(x => x.DataContextChanged += OnAssociatedObjectDataContextChanged);
            (AssociatedObject as FrameworkContentElement).Do(x => x.DataContextChanged += OnAssociatedObjectDataContextChanged);
            UpdateActualSource();
        }
        protected override void OnDetaching() {
            (AssociatedObject as FrameworkElement).Do(x => x.DataContextChanged -= OnAssociatedObjectDataContextChanged);
            (AssociatedObject as FrameworkContentElement).Do(x => x.DataContextChanged -= OnAssociatedObjectDataContextChanged);
            (ActualSource as INotifyPropertyChanged).Do(x => x.PropertyChanged -= OnSourceObjectPropertyChanged);
            ActualSource = null;
            base.OnDetaching();
        }
        protected virtual void UpdateActualSource() {
            (ActualSource as INotifyPropertyChanged).Do(x => x.PropertyChanged -= OnSourceObjectPropertyChanged);
            ActualSource = Source ?? GetAssociatedObjectDataContext();
            (ActualSource as INotifyPropertyChanged).Do(x => x.PropertyChanged += OnSourceObjectPropertyChanged);
            OnResultAffectedPropertyChanged();
        }
        protected virtual void OnResultAffectedPropertyChanged() { }

        void OnAssociatedObjectDataContextChanged(object sender, DependencyPropertyChangedEventArgs e) {
            if(Source == null)
                UpdateActualSource();
        }
        void OnSourceChanged() {
            UpdateActualSource();
        }
        protected virtual void OnTargetChanged(DependencyPropertyChangedEventArgs e) {
            if(e.NewValue == e.OldValue)
                return;
            OnResultAffectedPropertyChanged();
        }
        object GetAssociatedObjectDataContext() {
            return (AssociatedObject as FrameworkElement).With(x => x.DataContext) ?? (AssociatedObject as FrameworkContentElement).With(x => x.DataContext);
        }
        void OnSourceObjectPropertyChanged(object sender, PropertyChangedEventArgs e) {
            if(e.PropertyName == ActualFunction)
                OnResultAffectedPropertyChanged();
        }

        #region get method value
        protected class ArgInfo {
            public object Value { get; private set; }
            public Type Type { get; private set; }
            public bool IsUnsetValue { get; private set; }

            public ArgInfo(DependencyObject obj, DependencyProperty prop) {
                object value = obj.ReadLocalValue(prop);
                IsUnsetValue = value == DependencyProperty.UnsetValue;
                if(!IsUnsetValue) {
                    Value = value is BindingExpression ? obj.GetValue(prop) : value;
                    Type = Value.With(x => x.GetType());
                }
            }
            public ArgInfo(object value) {
                IsUnsetValue = value == DependencyProperty.UnsetValue;
                if(!IsUnsetValue) {
                    Value = value;
                    Type = Value.With(x => x.GetType());
                }
            }
        }
        protected static List<ArgInfo> GetArgsInfo(FunctionBindingBehaviorBase instance) {
            return typeof(FunctionBindingBehaviorBase).GetFields(BindingFlags.Public | BindingFlags.Static)
                .Where(x => argNameRegExp.IsMatch(x.Name)).OrderBy(x => int.Parse(argOrderRegExp.Match(x.Name).Value))
                .Select(x => new ArgInfo(instance, (DependencyProperty)x.GetValue(instance))).ToList();
        }
        protected static object InvokeSourceFunction(object source, string functionName, List<ArgInfo> argsInfo, Func<MethodInfo, Type, string, bool> functionChecker) {
            if(argsInfo == null)
                argsInfo = new List<ArgInfo>();
            while(argsInfo.Count < 15)
                argsInfo.Add(new ArgInfo(DependencyProperty.UnsetValue));
            int argsCount = argsInfo.FindLastIndex(x => !x.IsUnsetValue) + 1;
            Type targetType = (source as Type).Return(x => x.IsAbstract && x.IsSealed, () => false) ? source as Type : source.GetType();
            MethodInfo info = GetMethodInfo(targetType, functionName, argsCount, argsInfo.Take(argsCount).Select(x => x.Type).ToArray());
            if(!functionChecker.Invoke(info, targetType, functionName))
                return DependencyProperty.UnsetValue;

            ParameterInfo[] paramsInfo = info.GetParameters();
            argsCount = paramsInfo.Length;
            int count = 0;
            object[] param = argsInfo.Take(argsCount).Select(x => {
                ParameterInfo parInfo = paramsInfo[count++];
                if(x.IsUnsetValue && parInfo.IsOptional && parInfo.Attributes.HasFlag(ParameterAttributes.HasDefault))
                    return parInfo.RawDefaultValue;
                else
                    return TypeCastHelper.TryCast(x.Value, parInfo.ParameterType);
            }).ToArray();

            return info.Invoke(source, param);
        }
        protected static bool DefaultMethodInfoChecker(MethodInfo info, Type targetType, string functionName) {
            if(info == null) {
                System.Diagnostics.Trace.WriteLine(string.Format(Error_SourceMethodNotFound, targetType.Name, functionName));
                return false;
            }
            if(info.ReturnType == typeof(void))
                throw new ArgumentException(string.Format(Error_SourceMethodReturnVoid, targetType.Name, info.Name));

            return true;
        }
        protected static Action<object> GetObjectPropertySetter(object target, string property, bool? throwExcepctionOnNotFound = null) {
            PropertyInfo targetPropertyInfo = ObjectPropertyHelper.GetPropertyInfoSetter(target, property);
            targetPropertyInfo = targetPropertyInfo ?? ObjectPropertyHelper.GetPropertyInfo(target, property,
                BindingFlags.Public | BindingFlags.SetProperty | BindingFlags.NonPublic | BindingFlags.Static);
            if(targetPropertyInfo != null)
                return x => targetPropertyInfo.SetValue(target, x, null);

            DependencyProperty targetDependencyProperty = ObjectPropertyHelper.GetDependencyProperty(target, property);
            if(targetDependencyProperty != null)
                return x => ((DependencyObject)target).SetValue(targetDependencyProperty, x);

            if(throwExcepctionOnNotFound.HasValue) {
                string message = string.Format(Error_PropertyNotFound, target.GetType().Name, property);
                if(throwExcepctionOnNotFound.Value)
                    throw new ArgumentException(message);
                else
                    System.Diagnostics.Trace.WriteLine(string.Format("MethodBindingBehaviorBase error: {0}", message));
            }

            return null;
        }
        static MethodInfo GetMethodInfo(Type objType, string methodName, int argsCount = -1, Type[] argsType = null) {
            if(string.IsNullOrWhiteSpace(methodName) || objType == null)
                return null;

            MethodInfo result = null;
            BindingFlags flags = BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static;
            if(!argsType.Return(x => x.Length == 0 || x.Any(e => e == null), () => false))
                result = objType.GetMethod(methodName, flags, Type.DefaultBinder, argsType, null);

            return result ?? TryFindAmongMethods(objType, methodName, argsCount, argsType);
        }
        static MethodInfo TryFindAmongMethods(Type objType, string methodName, int argsCount, Type[] argsType) {
            var appropriateMethods = objType.GetMethods(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static)
                .Where(e => e.Name == methodName).OrderBy(o => o.GetParameters().Length);

            if(argsType != null && argsType.Length > 0 && appropriateMethods.Count() > 1) {
                foreach(MethodInfo info in appropriateMethods) {
                    if(argsCount > -1 && info.GetParameters().Length < argsCount)
                        continue;

                    int count = 0;
                    if(argsType.All(x => x == null || info.GetParameters()[count++].ParameterType.Equals(x)))
                        return info;
                }
            }

            return (argsCount > -1 ? appropriateMethods.FirstOrDefault(x => x.GetParameters().Length >= argsCount) : null) ?? appropriateMethods.FirstOrDefault();
        }
        #endregion
    }
}