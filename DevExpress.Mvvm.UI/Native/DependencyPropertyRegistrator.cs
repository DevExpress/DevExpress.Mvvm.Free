using System;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Windows;
using System.Windows.Controls;
using DevExpress.Mvvm.Native;

namespace DevExpress.Mvvm.UI.Native {

    interface IDPValueStorage {
        void SetValue(object value);
    }
    public sealed class DPValueStorage<T> : IDPValueStorage {
        bool isInitialized;
        T value;

        public T GetValue(DependencyObject o, DependencyProperty p) {
            if (isInitialized) {
                return value;
            }
            value = (T)o.GetValue(p);
            isInitialized = true;
            return value;
        }
        void IDPValueStorage.SetValue(object v) {
            value = (T)v;
            isInitialized = true;
        }
    }
    public static class DPValueStorageExtentions {
        public static T GetAttachedValue<T>(this DPValueStorage<T> storage, DependencyObject o, DependencyProperty p) {
            if (storage != null)
                return storage.GetValue(o, p);
            return (T)o.GetValue(p);
        }
    }
    public class DependencyPropertyRegistrator<T> where T : class {
        protected DependencyPropertyRegistrator() { }
        public static DependencyPropertyRegistrator<T> New() {
            return new DependencyPropertyRegistrator<T>();
        }


        public DependencyPropertyRegistrator<T> RegisterFast<TProperty>(string property, Func<T, DPValueStorage<TProperty>> getStorage, out DependencyProperty propertyField, TProperty defaultValue, FrameworkPropertyMetadataOptions? frameworkOptions = null) {
            return RegisterInternal<TProperty>(property, out propertyField, CreateMetadata(defaultValue, getStorage, (Action<T>)null, null, frameworkOptions));
        }
        public DependencyPropertyRegistrator<T> RegisterFast<TProperty>(string property, Func<T, DPValueStorage<TProperty>> getStorage, out DependencyProperty propertyField, TProperty defaultValue, Func<T, TProperty, TProperty> coerceCallback, FrameworkPropertyMetadataOptions? frameworkOptions = null) {
            return RegisterInternal<TProperty>(property, out propertyField, CreateMetadata(defaultValue, getStorage, (Action<T>)null, coerceCallback, frameworkOptions));
        }
        public DependencyPropertyRegistrator<T> RegisterFast<TProperty>(string property, Func<T, DPValueStorage<TProperty>> getStorage, out DependencyProperty propertyField, TProperty defaultValue, Action<T> changedCallback, FrameworkPropertyMetadataOptions? frameworkOptions = null) {
            return RegisterInternal<TProperty>(property, out propertyField, CreateMetadata(defaultValue, getStorage, changedCallback, null, frameworkOptions));
        }
        public DependencyPropertyRegistrator<T> RegisterFast<TProperty>(string property, Func<T, DPValueStorage<TProperty>> getStorage, out DependencyProperty propertyField, TProperty defaultValue, Action<T, DependencyPropertyChangedEventArgs> changedCallback, FrameworkPropertyMetadataOptions? frameworkOptions = null) {
            return RegisterInternal<TProperty>(property, out propertyField, CreateMetadata(defaultValue, getStorage, changedCallback, null, frameworkOptions));
        }
        public DependencyPropertyRegistrator<T> RegisterFast<TProperty>(string property, Func<T, DPValueStorage<TProperty>> getStorage, out DependencyProperty propertyField, TProperty defaultValue, Action<T, TProperty, TProperty> changedCallback, FrameworkPropertyMetadataOptions? frameworkOptions = null) {
            return RegisterInternal<TProperty>(property, out propertyField, CreateMetadata(defaultValue, getStorage, changedCallback, null, frameworkOptions));
        }
        public DependencyPropertyRegistrator<T> RegisterFast<TProperty>(string property, Func<T, DPValueStorage<TProperty>> getStorage, out DependencyProperty propertyField, TProperty defaultValue, Action<T> changedCallback, Func<T, TProperty, TProperty> coerceCallback, FrameworkPropertyMetadataOptions? frameworkOptions = null) {
            return RegisterInternal<TProperty>(property, out propertyField, CreateMetadata(defaultValue, getStorage, changedCallback, coerceCallback, frameworkOptions));
        }
        public DependencyPropertyRegistrator<T> RegisterFast<TProperty>(string property, Func<T, DPValueStorage<TProperty>> getStorage, out DependencyProperty propertyField, TProperty defaultValue, Action<T, DependencyPropertyChangedEventArgs> changedCallback, Func<T, TProperty, TProperty> coerceCallback, FrameworkPropertyMetadataOptions? frameworkOptions = null) {
            return RegisterInternal<TProperty>(property, out propertyField, CreateMetadata(defaultValue, getStorage, changedCallback, coerceCallback, frameworkOptions));
        }
        public DependencyPropertyRegistrator<T> RegisterFast<TProperty>(string property, Func<T, DPValueStorage<TProperty>> getStorage, out DependencyProperty propertyField, TProperty defaultValue, Action<T, TProperty, TProperty> changedCallback, Func<T, TProperty, TProperty> coerceCallback, FrameworkPropertyMetadataOptions? frameworkOptions = null) {
            return RegisterInternal<TProperty>(property, out propertyField, CreateMetadata(defaultValue, getStorage, changedCallback, coerceCallback, frameworkOptions));
        }

        public DependencyPropertyRegistrator<T> RegisterFastReadOnly<TProperty>(string property, Func<T, DPValueStorage<TProperty>> getStorage, out DependencyPropertyKey propertyFieldKey, out DependencyProperty propertyField, TProperty defaultValue, FrameworkPropertyMetadataOptions? frameworkOptions = null) {
            return RegisterReadOnlyInternal<TProperty>(property, out propertyFieldKey, out propertyField, CreateMetadata(defaultValue, getStorage, (Action<T>)null, null, frameworkOptions));
        }
        public DependencyPropertyRegistrator<T> RegisterFastReadOnly<TProperty>(string property, Func<T, DPValueStorage<TProperty>> getStorage, out DependencyPropertyKey propertyFieldKey, out DependencyProperty propertyField, TProperty defaultValue, Func<T, TProperty, TProperty> coerceCallback, FrameworkPropertyMetadataOptions? frameworkOptions = null) {
            return RegisterReadOnlyInternal<TProperty>(property, out propertyFieldKey, out propertyField, CreateMetadata(defaultValue, getStorage, (Action<T>)null, coerceCallback, frameworkOptions));
        }
        public DependencyPropertyRegistrator<T> RegisterFastReadOnly<TProperty>(string property, Func<T, DPValueStorage<TProperty>> getStorage, out DependencyPropertyKey propertyFieldKey, out DependencyProperty propertyField, TProperty defaultValue, Action<T> changedCallback, FrameworkPropertyMetadataOptions? frameworkOptions = null) {
            return RegisterReadOnlyInternal<TProperty>(property, out propertyFieldKey, out propertyField, CreateMetadata(defaultValue, getStorage, changedCallback, null, frameworkOptions));
        }
        public DependencyPropertyRegistrator<T> RegisterFastReadOnly<TProperty>(string property, Func<T, DPValueStorage<TProperty>> getStorage, out DependencyPropertyKey propertyFieldKey, out DependencyProperty propertyField, TProperty defaultValue, Action<T, DependencyPropertyChangedEventArgs> changedCallback, FrameworkPropertyMetadataOptions? frameworkOptions = null) {
            return RegisterReadOnlyInternal<TProperty>(property, out propertyFieldKey, out propertyField, CreateMetadata(defaultValue, getStorage, changedCallback, null, frameworkOptions));
        }
        public DependencyPropertyRegistrator<T> RegisterFastReadOnly<TProperty>(string property, Func<T, DPValueStorage<TProperty>> getStorage, out DependencyPropertyKey propertyFieldKey, out DependencyProperty propertyField, TProperty defaultValue, Action<T, TProperty, TProperty> changedCallback, FrameworkPropertyMetadataOptions? frameworkOptions = null) {
            return RegisterReadOnlyInternal<TProperty>(property, out propertyFieldKey, out propertyField, CreateMetadata(defaultValue, getStorage, changedCallback, null, frameworkOptions));
        }
        public DependencyPropertyRegistrator<T> RegisterFastReadOnly<TProperty>(string property, Func<T, DPValueStorage<TProperty>> getStorage, out DependencyPropertyKey propertyFieldKey, out DependencyProperty propertyField, TProperty defaultValue, Action<T> changedCallback, Func<T, TProperty, TProperty> coerceCallback, FrameworkPropertyMetadataOptions? frameworkOptions = null) {
            return RegisterReadOnlyInternal<TProperty>(property, out propertyFieldKey, out propertyField, CreateMetadata(defaultValue, getStorage, changedCallback, coerceCallback, frameworkOptions));
        }
        public DependencyPropertyRegistrator<T> RegisterFastReadOnly<TProperty>(string property, Func<T, DPValueStorage<TProperty>> getStorage, out DependencyPropertyKey propertyFieldKey, out DependencyProperty propertyField, TProperty defaultValue, Action<T, DependencyPropertyChangedEventArgs> changedCallback, Func<T, TProperty, TProperty> coerceCallback, FrameworkPropertyMetadataOptions? frameworkOptions = null) {
            return RegisterReadOnlyInternal<TProperty>(property, out propertyFieldKey, out propertyField, CreateMetadata(defaultValue, getStorage, changedCallback, coerceCallback, frameworkOptions));
        }
        public DependencyPropertyRegistrator<T> RegisterFastReadOnly<TProperty>(string property, Func<T, DPValueStorage<TProperty>> getStorage, out DependencyPropertyKey propertyFieldKey, out DependencyProperty propertyField, TProperty defaultValue, Action<T, TProperty, TProperty> changedCallback, Func<T, TProperty, TProperty> coerceCallback, FrameworkPropertyMetadataOptions? frameworkOptions = null) {
            return RegisterReadOnlyInternal<TProperty>(property, out propertyFieldKey, out propertyField, CreateMetadata(defaultValue, getStorage, changedCallback, coerceCallback, frameworkOptions));
        }

        public DependencyPropertyRegistrator<T> RegisterFastAttached<TOwner, TProperty>(string getMethodName, Func<TOwner, DPValueStorage<TProperty>> getStorage, out DependencyProperty propertyField, TProperty defaultValue, FrameworkPropertyMetadataOptions? frameworkOptions = null) where TOwner : class {
            return RegisterAttachedInternal<TOwner, TProperty>(getMethodName, out propertyField, CreateMetadata(defaultValue, getStorage, (Action<TOwner>)null, null, frameworkOptions));
        }
        public DependencyPropertyRegistrator<T> RegisterFastAttached<TOwner, TProperty>(string getMethodName, Func<TOwner, DPValueStorage<TProperty>> getStorage, out DependencyProperty propertyField, TProperty defaultValue, Func<TOwner, TProperty, TProperty> coerceCallback, FrameworkPropertyMetadataOptions? frameworkOptions = null) where TOwner : class {
            return RegisterAttachedInternal<TOwner, TProperty>(getMethodName, out propertyField, CreateMetadata(defaultValue, getStorage, (Action<TOwner>)null, coerceCallback, frameworkOptions));
        }
        public DependencyPropertyRegistrator<T> RegisterFastAttached<TOwner, TProperty>(string getMethodName, Func<TOwner, DPValueStorage<TProperty>> getStorage, out DependencyProperty propertyField, TProperty defaultValue, Action<TOwner> changedCallback, FrameworkPropertyMetadataOptions? frameworkOptions = null) where TOwner : class {
            return RegisterAttachedInternal<TOwner, TProperty>(getMethodName, out propertyField, CreateMetadata(defaultValue, getStorage, changedCallback, null, frameworkOptions));
        }
        public DependencyPropertyRegistrator<T> RegisterFastAttached<TOwner, TProperty>(string getMethodName, Func<TOwner, DPValueStorage<TProperty>> getStorage, out DependencyProperty propertyField, TProperty defaultValue, Action<TOwner, DependencyPropertyChangedEventArgs> changedCallback, FrameworkPropertyMetadataOptions? frameworkOptions = null) where TOwner : class {
            return RegisterAttachedInternal<TOwner, TProperty>(getMethodName, out propertyField, CreateMetadata(defaultValue, getStorage, changedCallback, null, frameworkOptions));
        }
        public DependencyPropertyRegistrator<T> RegisterFastAttached<TOwner, TProperty>(string getMethodName, Func<TOwner, DPValueStorage<TProperty>> getStorage, out DependencyProperty propertyField, TProperty defaultValue, Action<TOwner, TProperty, TProperty> changedCallback, FrameworkPropertyMetadataOptions? frameworkOptions = null) where TOwner : class {
            return RegisterAttachedInternal<TOwner, TProperty>(getMethodName, out propertyField, CreateMetadata(defaultValue, getStorage, changedCallback, null, frameworkOptions));
        }
        public DependencyPropertyRegistrator<T> RegisterFastAttached<TOwner, TProperty>(string getMethodName, Func<TOwner, DPValueStorage<TProperty>> getStorage, out DependencyProperty propertyField, TProperty defaultValue, Action<TOwner> changedCallback, Func<TOwner, TProperty, TProperty> coerceCallback, FrameworkPropertyMetadataOptions? frameworkOptions = null) where TOwner : class {
            return RegisterAttachedInternal<TOwner, TProperty>(getMethodName, out propertyField, CreateMetadata(defaultValue, getStorage, changedCallback, coerceCallback, frameworkOptions));
        }
        public DependencyPropertyRegistrator<T> RegisterFastAttached<TOwner, TProperty>(string getMethodName, Func<TOwner, DPValueStorage<TProperty>> getStorage, out DependencyProperty propertyField, TProperty defaultValue, Action<TOwner, DependencyPropertyChangedEventArgs> changedCallback, Func<TOwner, TProperty, TProperty> coerceCallback, FrameworkPropertyMetadataOptions? frameworkOptions = null) where TOwner : class {
            return RegisterAttachedInternal<TOwner, TProperty>(getMethodName, out propertyField, CreateMetadata(defaultValue, getStorage, changedCallback, coerceCallback, frameworkOptions));
        }
        public DependencyPropertyRegistrator<T> RegisterFastAttached<TOwner, TProperty>(string getMethodName, Func<TOwner, DPValueStorage<TProperty>> getStorage, out DependencyProperty propertyField, TProperty defaultValue, Action<TOwner, TProperty, TProperty> changedCallback, Func<TOwner, TProperty, TProperty> coerceCallback, FrameworkPropertyMetadataOptions? frameworkOptions = null) where TOwner : class {
            return RegisterAttachedInternal<TOwner, TProperty>(getMethodName, out propertyField, CreateMetadata(defaultValue, getStorage, changedCallback, coerceCallback, frameworkOptions));
        }

        public DependencyPropertyRegistrator<T> RegisterFastAttachedReadOnly<TOwner, TProperty>(string getMethodName, Func<TOwner, DPValueStorage<TProperty>> getStorage, out DependencyPropertyKey propertyFieldKey, out DependencyProperty propertyField, TProperty defaultValue, FrameworkPropertyMetadataOptions? frameworkOptions = null) where TOwner : class {
            return RegisterAttachedReadOnlyInternal<TOwner, TProperty>(getMethodName, out propertyFieldKey, out propertyField, CreateMetadata(defaultValue, getStorage, (Action<TOwner>)null, null, frameworkOptions));
        }
        public DependencyPropertyRegistrator<T> RegisterFastAttachedReadOnly<TOwner, TProperty>(string getMethodName, Func<TOwner, DPValueStorage<TProperty>> getStorage, out DependencyPropertyKey propertyFieldKey, out DependencyProperty propertyField, TProperty defaultValue, Func<TOwner, TProperty, TProperty> coerceCallback, FrameworkPropertyMetadataOptions? frameworkOptions = null) where TOwner : class {
            return RegisterAttachedReadOnlyInternal<TOwner, TProperty>(getMethodName, out propertyFieldKey, out propertyField, CreateMetadata(defaultValue, getStorage, (Action<TOwner>)null, coerceCallback, frameworkOptions));
        }
        public DependencyPropertyRegistrator<T> RegisterFastAttachedReadOnly<TOwner, TProperty>(string getMethodName, Func<TOwner, DPValueStorage<TProperty>> getStorage, out DependencyPropertyKey propertyFieldKey, out DependencyProperty propertyField, TProperty defaultValue, Action<TOwner> changedCallback, FrameworkPropertyMetadataOptions? frameworkOptions = null) where TOwner : class {
            return RegisterAttachedReadOnlyInternal<TOwner, TProperty>(getMethodName, out propertyFieldKey, out propertyField, CreateMetadata(defaultValue, getStorage, changedCallback, null, frameworkOptions));
        }
        public DependencyPropertyRegistrator<T> RegisterFastAttachedReadOnly<TOwner, TProperty>(string getMethodName, Func<TOwner, DPValueStorage<TProperty>> getStorage, out DependencyPropertyKey propertyFieldKey, out DependencyProperty propertyField, TProperty defaultValue, Action<TOwner, DependencyPropertyChangedEventArgs> changedCallback, FrameworkPropertyMetadataOptions? frameworkOptions = null) where TOwner : class {
            return RegisterAttachedReadOnlyInternal<TOwner, TProperty>(getMethodName, out propertyFieldKey, out propertyField, CreateMetadata(defaultValue, getStorage, changedCallback, null, frameworkOptions));
        }
        public DependencyPropertyRegistrator<T> RegisterFastAttachedReadOnly<TOwner, TProperty>(string getMethodName, Func<TOwner, DPValueStorage<TProperty>> getStorage, out DependencyPropertyKey propertyFieldKey, out DependencyProperty propertyField, TProperty defaultValue, Action<TOwner, TProperty, TProperty> changedCallback, FrameworkPropertyMetadataOptions? frameworkOptions = null) where TOwner : class {
            return RegisterAttachedReadOnlyInternal<TOwner, TProperty>(getMethodName, out propertyFieldKey, out propertyField, CreateMetadata(defaultValue, getStorage, changedCallback, null, frameworkOptions));
        }
        public DependencyPropertyRegistrator<T> RegisterFastAttachedReadOnly<TOwner, TProperty>(string getMethodName, Func<TOwner, DPValueStorage<TProperty>> getStorage, out DependencyPropertyKey propertyFieldKey, out DependencyProperty propertyField, TProperty defaultValue, Action<TOwner> changedCallback, Func<TOwner, TProperty, TProperty> coerceCallback, FrameworkPropertyMetadataOptions? frameworkOptions = null) where TOwner : class {
            return RegisterAttachedReadOnlyInternal<TOwner, TProperty>(getMethodName, out propertyFieldKey, out propertyField, CreateMetadata(defaultValue, getStorage, changedCallback, coerceCallback, frameworkOptions));
        }
        public DependencyPropertyRegistrator<T> RegisterFastAttachedReadOnly<TOwner, TProperty>(string getMethodName, Func<TOwner, DPValueStorage<TProperty>> getStorage, out DependencyPropertyKey propertyFieldKey, out DependencyProperty propertyField, TProperty defaultValue, Action<TOwner, DependencyPropertyChangedEventArgs> changedCallback, Func<TOwner, TProperty, TProperty> coerceCallback, FrameworkPropertyMetadataOptions? frameworkOptions = null) where TOwner : class {
            return RegisterAttachedReadOnlyInternal<TOwner, TProperty>(getMethodName, out propertyFieldKey, out propertyField, CreateMetadata(defaultValue, getStorage, changedCallback, coerceCallback, frameworkOptions));
        }
        public DependencyPropertyRegistrator<T> RegisterFastAttachedReadOnly<TOwner, TProperty>(string getMethodName, Func<TOwner, DPValueStorage<TProperty>> getStorage, out DependencyPropertyKey propertyFieldKey, out DependencyProperty propertyField, TProperty defaultValue, Action<TOwner, TProperty, TProperty> changedCallback, Func<TOwner, TProperty, TProperty> coerceCallback, FrameworkPropertyMetadataOptions? frameworkOptions = null) where TOwner : class {
            return RegisterAttachedReadOnlyInternal<TOwner, TProperty>(getMethodName, out propertyFieldKey, out propertyField, CreateMetadata(defaultValue, getStorage, changedCallback, coerceCallback, frameworkOptions));
        }

        public DependencyPropertyRegistrator<T> AddOwnerFast<TProperty>(string property, Func<T, DPValueStorage<TProperty>> getStorage, out DependencyProperty propertyField, DependencyProperty sourceProperty, TProperty defaultValue, Action<T> changedCallback = null) {
            return AddOwnerInternal<TProperty>(out propertyField, sourceProperty, CreateFrameworkMetadata(defaultValue, getStorage, changedCallback));
        }
        public DependencyPropertyRegistrator<T> AddOwnerFast<TProperty>(string property, Func<T, DPValueStorage<TProperty>> getStorage, out DependencyProperty propertyField, DependencyProperty sourceProperty, TProperty defaultValue, Action<T, DependencyPropertyChangedEventArgs> changedCallback) {
            return AddOwnerInternal<TProperty>(out propertyField, sourceProperty, CreateFrameworkMetadata(defaultValue, getStorage, changedCallback));
        }
        public DependencyPropertyRegistrator<T> AddOwnerFast<TProperty>(string property, Func<T, DPValueStorage<TProperty>> getStorage, out DependencyProperty propertyField, DependencyProperty sourceProperty, TProperty defaultValue, Action<T, TProperty, TProperty> changedCallback) {
            return AddOwnerInternal<TProperty>(out propertyField, sourceProperty, CreateFrameworkMetadata(defaultValue, getStorage, changedCallback));
        }
        public DependencyPropertyRegistrator<T> AddOwnerFast<TProperty>(string property, Func<T, DPValueStorage<TProperty>> getStorage, out DependencyProperty propertyField, DependencyProperty sourceProperty, TProperty defaultValue, Action<T, TProperty, TProperty> changedCallback, Func<T, TProperty, TProperty> coerceCallback, FrameworkPropertyMetadataOptions frameworkOptions) {
            return AddOwnerInternal<TProperty>(out propertyField, sourceProperty, CreateMetadata(defaultValue, getStorage, changedCallback, coerceCallback, frameworkOptions));
        }
        public DependencyPropertyRegistrator<T> AddOwnerFast<TProperty>(string property, Func<T, DPValueStorage<TProperty>> getStorage, out DependencyProperty propertyField, DependencyProperty sourceProperty, TProperty defaultValue, FrameworkPropertyMetadataOptions frameworkOptions) {
            return AddOwnerInternal<TProperty>(out propertyField, sourceProperty, CreateMetadata(defaultValue, getStorage, (Action<T>)null, null, frameworkOptions));
        }

        public DependencyPropertyRegistrator<T> OverrideMetadataFast<TProperty>(DependencyProperty propertyField, Func<T, DPValueStorage<TProperty>> getStorage, Action<T> changedCallback = null) {
            propertyField.OverrideMetadata(typeof(T), new FrameworkPropertyMetadata(ToHandler(getStorage, changedCallback)));
            return this;
        }
        public DependencyPropertyRegistrator<T> OverrideMetadataFast<TProperty>(DependencyProperty propertyField, Func<T, DPValueStorage<TProperty>> getStorage, object defaultValue, Action<T> changedCallback = null, FrameworkPropertyMetadataOptions frameworkOptions = FrameworkPropertyMetadataOptions.None) {
            propertyField.OverrideMetadata(typeof(T), new FrameworkPropertyMetadata(defaultValue, frameworkOptions, ToHandler(getStorage, changedCallback)));
            return this;
        }
        public DependencyPropertyRegistrator<T> OverrideMetadataFast<TProperty>(DependencyProperty propertyField, Func<T, DPValueStorage<TProperty>> getStorage, Action<T> changedCallback, Func<T, TProperty, TProperty> coerceCallback) {
            propertyField.OverrideMetadata(typeof(T), new FrameworkPropertyMetadata(ToHandler(getStorage, changedCallback), ToCoerce(coerceCallback)));
            return this;
        }
        public DependencyPropertyRegistrator<T> OverrideMetadataFast<TProperty>(DependencyProperty propertyField, Func<T, DPValueStorage<TProperty>> getStorage, Action<T, TProperty, TProperty> changedCallback) {
            propertyField.OverrideMetadata(typeof(T), new FrameworkPropertyMetadata(ToHandler(getStorage, changedCallback)));
            return this;
        }
        public DependencyPropertyRegistrator<T> OverrideMetadataFast<TProperty>(string getMethodName, Func<T, DPValueStorage<TProperty>> getStorage, DependencyProperty propertyField, Action<T, TProperty, TProperty> changedCallback) {
            propertyField.OverrideMetadata(typeof(T), new FrameworkPropertyMetadata(ToHandler(getStorage, changedCallback)));
            return this;
        }
        public DependencyPropertyRegistrator<T> OverrideMetadataFast<TProperty>(DependencyProperty propertyField, Func<T, DPValueStorage<TProperty>> getStorage, TProperty defaultValue, Action<T, TProperty, TProperty> changedCallback, FrameworkPropertyMetadataOptions frameworkOptions = FrameworkPropertyMetadataOptions.None) {
            propertyField.OverrideMetadata(typeof(T), new FrameworkPropertyMetadata(defaultValue, frameworkOptions, ToHandler(getStorage, changedCallback)));
            return this;
        }
        public DependencyPropertyRegistrator<T> OverrideMetadataFast<TProperty>(DependencyProperty propertyField, Func<T, DPValueStorage<TProperty>> getStorage, Action<T, TProperty, TProperty> changedCallback, Func<T, TProperty, TProperty> coerceCallback) {
            propertyField.OverrideMetadata(typeof(T), new FrameworkPropertyMetadata(ToHandler(getStorage, changedCallback), ToCoerce(coerceCallback)));
            return this;
        }

        public DependencyPropertyRegistrator<T> AddOwner<TProperty>(out DependencyProperty propertyField, DependencyProperty sourceProperty) {
            return AddOwnerInternal<TProperty>(out propertyField, sourceProperty, null);
        }
        public DependencyPropertyRegistrator<T> AddOwner<TProperty>(out DependencyProperty propertyField, DependencyProperty sourceProperty, TProperty defaultValue, Action<T> changedCallback = null) {
            return AddOwnerInternal<TProperty>(out propertyField, sourceProperty, CreateFrameworkMetadata(defaultValue, changedCallback));
        }
        public DependencyPropertyRegistrator<T> AddOwner<TProperty>(out DependencyProperty propertyField, DependencyProperty sourceProperty, TProperty defaultValue, Action<T, DependencyPropertyChangedEventArgs> changedCallback) {
            return AddOwnerInternal<TProperty>(out propertyField, sourceProperty, CreateFrameworkMetadata(defaultValue, changedCallback));
        }
        public DependencyPropertyRegistrator<T> AddOwner<TProperty>(out DependencyProperty propertyField, DependencyProperty sourceProperty, TProperty defaultValue, Action<T, TProperty, TProperty> changedCallback) {
            return AddOwnerInternal<TProperty>(out propertyField, sourceProperty, CreateFrameworkMetadata(defaultValue, changedCallback));
        }
        public DependencyPropertyRegistrator<T> AddOwner<TProperty>(out DependencyProperty propertyField, DependencyProperty sourceProperty, TProperty defaultValue, Action<T, TProperty, TProperty> changedCallback, Func<T, TProperty, TProperty> coerceCallback, FrameworkPropertyMetadataOptions frameworkOptions) {
            return AddOwnerInternal<TProperty>(out propertyField, sourceProperty, CreateMetadata(defaultValue, changedCallback, coerceCallback, frameworkOptions));
        }
        public DependencyPropertyRegistrator<T> AddOwner<TProperty>(out DependencyProperty propertyField, DependencyProperty sourceProperty, TProperty defaultValue, FrameworkPropertyMetadataOptions frameworkOptions) {
            return AddOwnerInternal<TProperty>(out propertyField, sourceProperty, CreateMetadata(defaultValue, (Action<T>)null, null, frameworkOptions));
        }
        DependencyPropertyRegistrator<T> AddOwnerInternal<TProperty>(out DependencyProperty propertyField, DependencyProperty sourceProperty, PropertyMetadata metadata) {
            propertyField = sourceProperty.AddOwner(typeof(T), metadata);
            return this;
        }

        public DependencyPropertyRegistrator<T> Register<TProperty>(string propertyName, out DependencyProperty propertyField, TProperty defaultValue, FrameworkPropertyMetadataOptions? frameworkOptions = null) {
            return RegisterInternal<TProperty>(propertyName, out propertyField, CreateMetadata(defaultValue, (Action<T>)null, null, frameworkOptions));
        }
        public DependencyPropertyRegistrator<T> Register<TProperty>(string propertyName, out DependencyProperty propertyField, TProperty defaultValue, Func<T, TProperty, TProperty> coerceCallback, FrameworkPropertyMetadataOptions? frameworkOptions = null) {
            return RegisterInternal<TProperty>(propertyName, out propertyField, CreateMetadata(defaultValue, (Action<T>)null, coerceCallback, frameworkOptions));
        }
        public DependencyPropertyRegistrator<T> Register<TProperty>(string propertyName, out DependencyProperty propertyField, TProperty defaultValue, Action<T> changedCallback, FrameworkPropertyMetadataOptions? frameworkOptions = null) {
            return RegisterInternal<TProperty>(propertyName, out propertyField, CreateMetadata(defaultValue, changedCallback, null, frameworkOptions));
        }
        public DependencyPropertyRegistrator<T> Register<TProperty>(string propertyName, out DependencyProperty propertyField, TProperty defaultValue, Action<T, DependencyPropertyChangedEventArgs> changedCallback, FrameworkPropertyMetadataOptions? frameworkOptions = null) {
            return RegisterInternal<TProperty>(propertyName, out propertyField, CreateMetadata(defaultValue, changedCallback, null, frameworkOptions));
        }
        public DependencyPropertyRegistrator<T> Register<TProperty>(string propertyName, out DependencyProperty propertyField, TProperty defaultValue, Action<T, TProperty, TProperty> changedCallback, FrameworkPropertyMetadataOptions? frameworkOptions = null) {
            return RegisterInternal<TProperty>(propertyName, out propertyField, CreateMetadata(defaultValue, changedCallback, null, frameworkOptions));
        }
        public DependencyPropertyRegistrator<T> Register<TProperty>(string propertyName, out DependencyProperty propertyField, TProperty defaultValue, Action<T> changedCallback, Func<T, TProperty, TProperty> coerceCallback, FrameworkPropertyMetadataOptions? frameworkOptions = null) {
            return RegisterInternal<TProperty>(propertyName, out propertyField, CreateMetadata(defaultValue, changedCallback, coerceCallback, frameworkOptions));
        }
        public DependencyPropertyRegistrator<T> Register<TProperty>(string propertyName, out DependencyProperty propertyField, TProperty defaultValue, Action<T, DependencyPropertyChangedEventArgs> changedCallback, Func<T, TProperty, TProperty> coerceCallback, FrameworkPropertyMetadataOptions? frameworkOptions = null) {
            return RegisterInternal<TProperty>(propertyName, out propertyField, CreateMetadata(defaultValue, changedCallback, coerceCallback, frameworkOptions));
        }
        public DependencyPropertyRegistrator<T> Register<TProperty>(string propertyName, out DependencyProperty propertyField, TProperty defaultValue, Action<T, TProperty, TProperty> changedCallback, Func<T, TProperty, TProperty> coerceCallback, FrameworkPropertyMetadataOptions? frameworkOptions = null) {
            return RegisterInternal<TProperty>(propertyName, out propertyField, CreateMetadata(defaultValue, changedCallback, coerceCallback, frameworkOptions));
        }
        DependencyPropertyRegistrator<T> RegisterInternal<TProperty>(string propertyName, out DependencyProperty propertyField, PropertyMetadata metadata) {
            propertyField = DependencyProperty.Register(propertyName, typeof(TProperty), typeof(T), metadata);
            return this;
        }

        public DependencyPropertyRegistrator<T> RegisterReadOnly<TProperty>(string propertyName, out DependencyPropertyKey propertyFieldKey, out DependencyProperty propertyField, TProperty defaultValue, FrameworkPropertyMetadataOptions? frameworkOptions = null) {
            return RegisterReadOnlyInternal<TProperty>(propertyName, out propertyFieldKey, out propertyField, CreateMetadata(defaultValue, (Action<T>)null, null, frameworkOptions));
        }
        public DependencyPropertyRegistrator<T> RegisterReadOnly<TProperty>(string propertyName, out DependencyPropertyKey propertyFieldKey, out DependencyProperty propertyField, TProperty defaultValue, Func<T, TProperty, TProperty> coerceCallback, FrameworkPropertyMetadataOptions? frameworkOptions = null) {
            return RegisterReadOnlyInternal<TProperty>(propertyName, out propertyFieldKey, out propertyField, CreateMetadata(defaultValue, (Action<T>)null, coerceCallback, frameworkOptions));
        }
        public DependencyPropertyRegistrator<T> RegisterReadOnly<TProperty>(string propertyName, out DependencyPropertyKey propertyFieldKey, out DependencyProperty propertyField, TProperty defaultValue, Action<T> changedCallback, FrameworkPropertyMetadataOptions? frameworkOptions = null) {
            return RegisterReadOnlyInternal<TProperty>(propertyName, out propertyFieldKey, out propertyField, CreateMetadata(defaultValue, changedCallback, null, frameworkOptions));
        }
        public DependencyPropertyRegistrator<T> RegisterReadOnly<TProperty>(string propertyName, out DependencyPropertyKey propertyFieldKey, out DependencyProperty propertyField, TProperty defaultValue, Action<T, DependencyPropertyChangedEventArgs> changedCallback, FrameworkPropertyMetadataOptions? frameworkOptions = null) {
            return RegisterReadOnlyInternal<TProperty>(propertyName, out propertyFieldKey, out propertyField, CreateMetadata(defaultValue, changedCallback, null, frameworkOptions));
        }
        public DependencyPropertyRegistrator<T> RegisterReadOnly<TProperty>(string propertyName, out DependencyPropertyKey propertyFieldKey, out DependencyProperty propertyField, TProperty defaultValue, Action<T, TProperty, TProperty> changedCallback, FrameworkPropertyMetadataOptions? frameworkOptions = null) {
            return RegisterReadOnlyInternal<TProperty>(propertyName, out propertyFieldKey, out propertyField, CreateMetadata(defaultValue, changedCallback, null, frameworkOptions));
        }
        public DependencyPropertyRegistrator<T> RegisterReadOnly<TProperty>(string propertyName, out DependencyPropertyKey propertyFieldKey, out DependencyProperty propertyField, TProperty defaultValue, Action<T> changedCallback, Func<T, TProperty, TProperty> coerceCallback, FrameworkPropertyMetadataOptions? frameworkOptions = null) {
            return RegisterReadOnlyInternal<TProperty>(propertyName, out propertyFieldKey, out propertyField, CreateMetadata(defaultValue, changedCallback, coerceCallback, frameworkOptions));
        }
        public DependencyPropertyRegistrator<T> RegisterReadOnly<TProperty>(string propertyName, out DependencyPropertyKey propertyFieldKey, out DependencyProperty propertyField, TProperty defaultValue, Action<T, DependencyPropertyChangedEventArgs> changedCallback, Func<T, TProperty, TProperty> coerceCallback, FrameworkPropertyMetadataOptions? frameworkOptions = null) {
            return RegisterReadOnlyInternal<TProperty>(propertyName, out propertyFieldKey, out propertyField, CreateMetadata(defaultValue, changedCallback, coerceCallback, frameworkOptions));
        }
        public DependencyPropertyRegistrator<T> RegisterReadOnly<TProperty>(string propertyName, out DependencyPropertyKey propertyFieldKey, out DependencyProperty propertyField, TProperty defaultValue, Action<T, TProperty, TProperty> changedCallback, Func<T, TProperty, TProperty> coerceCallback, FrameworkPropertyMetadataOptions? frameworkOptions = null) {
            return RegisterReadOnlyInternal<TProperty>(propertyName, out propertyFieldKey, out propertyField, CreateMetadata(defaultValue, changedCallback, coerceCallback, frameworkOptions));
        }
        DependencyPropertyRegistrator<T> RegisterReadOnlyInternal<TProperty>(string propertyName, out DependencyPropertyKey propertyFieldKey, out DependencyProperty propertyField, PropertyMetadata metadata) {
            propertyFieldKey = DependencyProperty.RegisterReadOnly(propertyName, typeof(TProperty), typeof(T), metadata);
            propertyField = propertyFieldKey.DependencyProperty;
            return this;
        }

        public DependencyPropertyRegistrator<T> RegisterAttached<TOwner, TProperty>(string methodName, out DependencyProperty propertyField, TProperty defaultValue, FrameworkPropertyMetadataOptions? frameworkOptions = null) where TOwner : class {
            return RegisterAttachedInternal<TOwner, TProperty>(methodName, out propertyField, CreateMetadata(defaultValue, (Action<TOwner>)null, null, frameworkOptions));
        }
        public DependencyPropertyRegistrator<T> RegisterAttached<TOwner, TProperty>(string methodName, out DependencyProperty propertyField, TProperty defaultValue, Func<TOwner, TProperty, TProperty> coerceCallback, FrameworkPropertyMetadataOptions? frameworkOptions = null) where TOwner : class {
            return RegisterAttachedInternal<TOwner, TProperty>(methodName, out propertyField, CreateMetadata(defaultValue, (Action<TOwner>)null, coerceCallback, frameworkOptions));
        }
        public DependencyPropertyRegistrator<T> RegisterAttached<TOwner, TProperty>(string methodName, out DependencyProperty propertyField, TProperty defaultValue, Action<TOwner> changedCallback, FrameworkPropertyMetadataOptions? frameworkOptions = null) where TOwner : class {
            return RegisterAttachedInternal<TOwner, TProperty>(methodName, out propertyField, CreateMetadata(defaultValue, changedCallback, null, frameworkOptions));
        }
        public DependencyPropertyRegistrator<T> RegisterAttached<TOwner, TProperty>(string methodName, out DependencyProperty propertyField, TProperty defaultValue, Action<TOwner, DependencyPropertyChangedEventArgs> changedCallback, FrameworkPropertyMetadataOptions? frameworkOptions = null) where TOwner : class {
            return RegisterAttachedInternal<TOwner, TProperty>(methodName, out propertyField, CreateMetadata(defaultValue, changedCallback, null, frameworkOptions));
        }
        public DependencyPropertyRegistrator<T> RegisterAttached<TOwner, TProperty>(string methodName, out DependencyProperty propertyField, TProperty defaultValue, Action<TOwner, TProperty, TProperty> changedCallback, FrameworkPropertyMetadataOptions? frameworkOptions = null) where TOwner : class {
            return RegisterAttachedInternal<TOwner, TProperty>(methodName, out propertyField, CreateMetadata(defaultValue, changedCallback, null, frameworkOptions));
        }
        public DependencyPropertyRegistrator<T> RegisterAttached<TOwner, TProperty>(string methodName, out DependencyProperty propertyField, TProperty defaultValue, Action<TOwner> changedCallback, Func<TOwner, TProperty, TProperty> coerceCallback, FrameworkPropertyMetadataOptions? frameworkOptions = null) where TOwner : class {
            return RegisterAttachedInternal<TOwner, TProperty>(methodName, out propertyField, CreateMetadata(defaultValue, changedCallback, coerceCallback, frameworkOptions));
        }
        public DependencyPropertyRegistrator<T> RegisterAttached<TOwner, TProperty>(string methodName, out DependencyProperty propertyField, TProperty defaultValue, Action<TOwner, DependencyPropertyChangedEventArgs> changedCallback, Func<TOwner, TProperty, TProperty> coerceCallback, FrameworkPropertyMetadataOptions? frameworkOptions = null) where TOwner : class {
            return RegisterAttachedInternal<TOwner, TProperty>(methodName, out propertyField, CreateMetadata(defaultValue, changedCallback, coerceCallback, frameworkOptions));
        }
        public DependencyPropertyRegistrator<T> RegisterAttached<TOwner, TProperty>(string methodName, out DependencyProperty propertyField, TProperty defaultValue, Action<TOwner, TProperty, TProperty> changedCallback, Func<TOwner, TProperty, TProperty> coerceCallback, FrameworkPropertyMetadataOptions? frameworkOptions = null) where TOwner : class {
            return RegisterAttachedInternal<TOwner, TProperty>(methodName, out propertyField, CreateMetadata(defaultValue, changedCallback, coerceCallback, frameworkOptions));
        }
        DependencyPropertyRegistrator<T> RegisterAttachedInternal<TOwner, TProperty>(string methodName, out DependencyProperty propertyField, PropertyMetadata metadata) where TOwner : class {
            propertyField = DependencyProperty.RegisterAttached(GetPropertyNameFromMethod<TOwner, TProperty>(methodName), typeof(TProperty), typeof(T), metadata);
            return this;
        }

        public DependencyPropertyRegistrator<T> RegisterAttachedReadOnly<TOwner, TProperty>(string getMethodName, out DependencyPropertyKey propertyFieldKey, out DependencyProperty propertyField, TProperty defaultValue, FrameworkPropertyMetadataOptions? frameworkOptions = null) where TOwner : class {
            return RegisterAttachedReadOnlyInternal<TOwner, TProperty>(getMethodName, out propertyFieldKey, out propertyField, CreateMetadata(defaultValue, (Action<TOwner>)null, null, frameworkOptions));
        }
        public DependencyPropertyRegistrator<T> RegisterAttachedReadOnly<TOwner, TProperty>(string getMethodName, out DependencyPropertyKey propertyFieldKey, out DependencyProperty propertyField, TProperty defaultValue, Func<TOwner, TProperty, TProperty> coerceCallback, FrameworkPropertyMetadataOptions? frameworkOptions = null) where TOwner : class {
            return RegisterAttachedReadOnlyInternal<TOwner, TProperty>(getMethodName, out propertyFieldKey, out propertyField, CreateMetadata(defaultValue, (Action<TOwner>)null, coerceCallback, frameworkOptions));
        }
        public DependencyPropertyRegistrator<T> RegisterAttachedReadOnly<TOwner, TProperty>(string getMethodName, out DependencyPropertyKey propertyFieldKey, out DependencyProperty propertyField, TProperty defaultValue, Action<TOwner> changedCallback, FrameworkPropertyMetadataOptions? frameworkOptions = null) where TOwner : class {
            return RegisterAttachedReadOnlyInternal<TOwner, TProperty>(getMethodName, out propertyFieldKey, out propertyField, CreateMetadata(defaultValue, changedCallback, null, frameworkOptions));
        }
        public DependencyPropertyRegistrator<T> RegisterAttachedReadOnly<TOwner, TProperty>(string getMethodName, out DependencyPropertyKey propertyFieldKey, out DependencyProperty propertyField, TProperty defaultValue, Action<TOwner, DependencyPropertyChangedEventArgs> changedCallback, FrameworkPropertyMetadataOptions? frameworkOptions = null) where TOwner : class {
            return RegisterAttachedReadOnlyInternal<TOwner, TProperty>(getMethodName, out propertyFieldKey, out propertyField, CreateMetadata(defaultValue, changedCallback, null, frameworkOptions));
        }
        public DependencyPropertyRegistrator<T> RegisterAttachedReadOnly<TOwner, TProperty>(string getMethodName, out DependencyPropertyKey propertyFieldKey, out DependencyProperty propertyField, TProperty defaultValue, Action<TOwner, TProperty, TProperty> changedCallback, FrameworkPropertyMetadataOptions? frameworkOptions = null) where TOwner : class {
            return RegisterAttachedReadOnlyInternal<TOwner, TProperty>(getMethodName, out propertyFieldKey, out propertyField, CreateMetadata(defaultValue, changedCallback, null, frameworkOptions));
        }
        public DependencyPropertyRegistrator<T> RegisterAttachedReadOnly<TOwner, TProperty>(string getMethodName, out DependencyPropertyKey propertyFieldKey, out DependencyProperty propertyField, TProperty defaultValue, Action<TOwner> changedCallback, Func<TOwner, TProperty, TProperty> coerceCallback, FrameworkPropertyMetadataOptions? frameworkOptions = null) where TOwner : class {
            return RegisterAttachedReadOnlyInternal<TOwner, TProperty>(getMethodName, out propertyFieldKey, out propertyField, CreateMetadata(defaultValue, changedCallback, coerceCallback, frameworkOptions));
        }
        public DependencyPropertyRegistrator<T> RegisterAttachedReadOnly<TOwner, TProperty>(string getMethodName, out DependencyPropertyKey propertyFieldKey, out DependencyProperty propertyField, TProperty defaultValue, Action<TOwner, DependencyPropertyChangedEventArgs> changedCallback, Func<TOwner, TProperty, TProperty> coerceCallback, FrameworkPropertyMetadataOptions? frameworkOptions = null) where TOwner : class {
            return RegisterAttachedReadOnlyInternal<TOwner, TProperty>(getMethodName, out propertyFieldKey, out propertyField, CreateMetadata(defaultValue, changedCallback, coerceCallback, frameworkOptions));
        }
        public DependencyPropertyRegistrator<T> RegisterAttachedReadOnly<TOwner, TProperty>(string getMethodName, out DependencyPropertyKey propertyFieldKey, out DependencyProperty propertyField, TProperty defaultValue, Action<TOwner, TProperty, TProperty> changedCallback, Func<TOwner, TProperty, TProperty> coerceCallback, FrameworkPropertyMetadataOptions? frameworkOptions = null) where TOwner : class {
            return RegisterAttachedReadOnlyInternal<TOwner, TProperty>(getMethodName, out propertyFieldKey, out propertyField, CreateMetadata(defaultValue, changedCallback, coerceCallback, frameworkOptions));
        }
        DependencyPropertyRegistrator<T> RegisterAttachedReadOnlyInternal<TOwner, TProperty>(string getMethodName, out DependencyPropertyKey propertyFieldKey, out DependencyProperty propertyField, PropertyMetadata metadata) where TOwner : class {
            propertyFieldKey = DependencyProperty.RegisterAttachedReadOnly(GetPropertyNameFromMethod<TOwner, TProperty>(getMethodName), typeof(TProperty), typeof(T), metadata);
            propertyField = propertyFieldKey.DependencyProperty;
            return this;
        }

        public DependencyPropertyRegistrator<T> OverrideDefaultStyleKey() {
            DefaultStyleKeyHelper.DefaultStyleKeyProperty.OverrideMetadata(typeof(T), new FrameworkPropertyMetadata(typeof(T)));
            return this;
        }
        public DependencyPropertyRegistrator<T> OverrideMetadata(DependencyProperty propertyField, Action<T> changedCallback = null) {
            propertyField.OverrideMetadata(typeof(T), new FrameworkPropertyMetadata(ToHandler(changedCallback)));
            return this;
        }
        public DependencyPropertyRegistrator<T> OverrideMetadata(DependencyProperty propertyField, object defaultValue, Action<T> changedCallback = null, FrameworkPropertyMetadataOptions frameworkOptions = FrameworkPropertyMetadataOptions.None) {
            propertyField.OverrideMetadata(typeof(T), new FrameworkPropertyMetadata(defaultValue, frameworkOptions, ToHandler(changedCallback)));
            return this;
        }
        public DependencyPropertyRegistrator<T> OverrideMetadata<TProperty>(DependencyProperty propertyField, Action<T> changedCallback, Func<T, TProperty, TProperty> coerceCallback) {
            propertyField.OverrideMetadata(typeof(T), new FrameworkPropertyMetadata(ToHandler(changedCallback), ToCoerce(coerceCallback)));
            return this;
        }

        public DependencyPropertyRegistrator<T> OverrideMetadata<TProperty>(DependencyProperty propertyField, Action<T, TProperty, TProperty> changedCallback) {
            propertyField.OverrideMetadata(typeof(T), new FrameworkPropertyMetadata(ToHandler(changedCallback)));
            return this;
        }
        public DependencyPropertyRegistrator<T> OverrideMetadata<TProperty>(DependencyProperty propertyField, TProperty defaultValue, Action<T, TProperty, TProperty> changedCallback, FrameworkPropertyMetadataOptions frameworkOptions = FrameworkPropertyMetadataOptions.None) {
            propertyField.OverrideMetadata(typeof(T), new FrameworkPropertyMetadata(defaultValue, frameworkOptions, ToHandler(changedCallback)));
            return this;
        }
        public DependencyPropertyRegistrator<T> OverrideMetadata<TProperty>(DependencyProperty propertyField, Action<T, TProperty, TProperty> changedCallback, Func<T, TProperty, TProperty> coerceCallback) {
            propertyField.OverrideMetadata(typeof(T), new FrameworkPropertyMetadata(ToHandler(changedCallback), ToCoerce(coerceCallback)));
            return this;
        }

        public DependencyPropertyRegistrator<T> FixPropertyValue(DependencyProperty propertyField, object value) {
            propertyField.OverrideMetadata(typeof(T), new FrameworkPropertyMetadata(value, null, (d, o) => value));
            return this;
        }

        static PropertyMetadata CreateFrameworkMetadata<TOwner, TProperty>(TProperty defaultValue, Func<TOwner, DPValueStorage<TProperty>> getStorage, Action<TOwner> changedCallback) where TOwner : class {
            var handler = ToHandler<TOwner, TProperty>(getStorage, changedCallback);
            return new FrameworkPropertyMetadata(defaultValue, handler);
        }
        static PropertyMetadata CreateFrameworkMetadata<TOwner, TProperty>(TProperty defaultValue, Func<TOwner, DPValueStorage<TProperty>> getStorage, Action<TOwner, DependencyPropertyChangedEventArgs> changedCallback) where TOwner : class {
            var handler = ToHandler<TOwner, TProperty>(getStorage, changedCallback);
            return new FrameworkPropertyMetadata(defaultValue, handler);
        }
        static PropertyMetadata CreateFrameworkMetadata<TOwner, TProperty>(TProperty defaultValue, Func<TOwner, DPValueStorage<TProperty>> getStorage, Action<TOwner, TProperty, TProperty> changedCallback) where TOwner : class {
            var handler = ToHandler<TOwner, TProperty>(getStorage, changedCallback);
            return new FrameworkPropertyMetadata(defaultValue, handler);
        }
        static PropertyMetadata CreateFrameworkMetadata<TOwner, TProperty>(TProperty defaultValue, Action<TOwner> changedCallback) where TOwner : class {
            var handler = ToHandler(changedCallback);
            return new FrameworkPropertyMetadata(defaultValue, handler);
        }
        static PropertyMetadata CreateFrameworkMetadata<TOwner, TProperty>(TProperty defaultValue, Action<TOwner, DependencyPropertyChangedEventArgs> changedCallback) where TOwner : class {
            var handler = ToHandler(changedCallback);
            return new FrameworkPropertyMetadata(defaultValue, handler);
        }
        static PropertyMetadata CreateFrameworkMetadata<TOwner, TProperty>(TProperty defaultValue, Action<TOwner, TProperty, TProperty> changedCallback) where TOwner : class {
            var handler = ToHandler(changedCallback);
            return new FrameworkPropertyMetadata(defaultValue, handler);
        }
        static PropertyMetadata CreateMetadata<TOwner, TProperty>(TProperty defaultValue, Func<TOwner, DPValueStorage<TProperty>> getStorage, Action<TOwner> changedCallback, Func<TOwner, TProperty, TProperty> coerceCallback, FrameworkPropertyMetadataOptions? frameworkOptions) where TOwner : class {
            var handler = ToHandler<TOwner, TProperty>(getStorage, changedCallback);
            var coerce = ToCoerce(coerceCallback);
            return CreateMetadataCore<TOwner, TProperty>(defaultValue, handler, coerce, frameworkOptions);
        }
        static PropertyMetadata CreateMetadata<TOwner, TProperty>(TProperty defaultValue, Func<TOwner, DPValueStorage<TProperty>> getStorage, Action<TOwner, DependencyPropertyChangedEventArgs> changedCallback, Func<TOwner, TProperty, TProperty> coerceCallback, FrameworkPropertyMetadataOptions? frameworkOptions) where TOwner : class {
            var handler = ToHandler<TOwner, TProperty>(getStorage, changedCallback);
            var coerce = ToCoerce(coerceCallback);
            return CreateMetadataCore<TOwner, TProperty>(defaultValue, handler, coerce, frameworkOptions);
        }
        static PropertyMetadata CreateMetadata<TOwner, TProperty>(TProperty defaultValue, Func<TOwner, DPValueStorage<TProperty>> getStorage, Action<TOwner, TProperty, TProperty> changedCallback, Func<TOwner, TProperty, TProperty> coerceCallback, FrameworkPropertyMetadataOptions? frameworkOptions) where TOwner : class {
            var handler = ToHandler<TOwner, TProperty>(getStorage, changedCallback);
            var coerce = ToCoerce(coerceCallback);
            return CreateMetadataCore<TOwner, TProperty>(defaultValue, handler, coerce, frameworkOptions);
        }
        static PropertyMetadata CreateMetadata<TOwner, TProperty>(TProperty defaultValue, Action<TOwner> changedCallback, Func<TOwner, TProperty, TProperty> coerceCallback, FrameworkPropertyMetadataOptions? frameworkOptions) where TOwner : class {
            var handler = ToHandler(changedCallback);
            var coerce = ToCoerce(coerceCallback);
            return CreateMetadataCore<TOwner, TProperty>(defaultValue, handler, coerce, frameworkOptions);
        }
        static PropertyMetadata CreateMetadata<TOwner, TProperty>(TProperty defaultValue, Action<TOwner, DependencyPropertyChangedEventArgs> changedCallback, Func<TOwner, TProperty, TProperty> coerceCallback, FrameworkPropertyMetadataOptions? frameworkOptions) where TOwner : class {
            var handler = ToHandler(changedCallback);
            var coerce = ToCoerce(coerceCallback);
            return CreateMetadataCore<TOwner, TProperty>(defaultValue, handler, coerce, frameworkOptions);
        }
        static PropertyMetadata CreateMetadata<TOwner, TProperty>(TProperty defaultValue, Action<TOwner, TProperty, TProperty> changedCallback, Func<TOwner, TProperty, TProperty> coerceCallback, FrameworkPropertyMetadataOptions? frameworkOptions) where TOwner : class {
            var handler = ToHandler(changedCallback);
            var coerce = ToCoerce(coerceCallback);
            return CreateMetadataCore<TOwner, TProperty>(defaultValue, handler, coerce, frameworkOptions);
        }
        static PropertyMetadata CreateMetadataCore<TOwner, TProperty>(TProperty defaultValue, PropertyChangedCallback handler, CoerceValueCallback coerce, FrameworkPropertyMetadataOptions? frameworkOptions) {
            return frameworkOptions == null ? new PropertyMetadata(defaultValue, handler, coerce) : new FrameworkPropertyMetadata(defaultValue, frameworkOptions.Value, handler, coerce);
        }
        static PropertyChangedCallback ToHandler<TOwner, TProperty>(Func<TOwner, DPValueStorage<TProperty>> getStorage, Action<TOwner> changedCallback) where TOwner : class {
            if (changedCallback == null)
                return (d, e) => SetStorageValue(d, e.NewValue, getStorage);
            return (d, e) => {
                SetStorageValue(d, e.NewValue, getStorage);
                changedCallback((TOwner)(object)d);
            };
        }
        static PropertyChangedCallback ToHandler<TOwner, TProperty>(Func<TOwner, DPValueStorage<TProperty>> getStorage, Action<TOwner, TProperty, TProperty> changedCallback) where TOwner : class {
            if (changedCallback == null)
                return (d, e) => SetStorageValue(d, e.NewValue, getStorage);
            return (d, e) => {
                SetStorageValue(d, e.NewValue, getStorage);
                changedCallback((TOwner)(object)d, (TProperty)e.OldValue, (TProperty)e.NewValue);
            };
        }
        static PropertyChangedCallback ToHandler<TOwner, TProperty>(Func<TOwner, DPValueStorage<TProperty>> getStorage, Action<TOwner, DependencyPropertyChangedEventArgs> changedCallback) where TOwner : class {
            if (changedCallback == null)
                return (d, e) => SetStorageValue(d, e.NewValue, getStorage);
            return (d, e) => {
                SetStorageValue(d, e.NewValue, getStorage);
                changedCallback((TOwner)(object)d, e);
            };
        }
        static void SetStorageValue<TOwner, TProperty>(DependencyObject d, object newValue, Func<TOwner, DPValueStorage<TProperty>> getStorage) where TOwner : class {
            var storage = getStorage((TOwner)(object)d);
            if (storage != null)
                ((IDPValueStorage)storage).SetValue(newValue);
        }
        static PropertyChangedCallback ToHandler<TOwner>(Action<TOwner> changedCallback) where TOwner : class {
            if(changedCallback == null)
                return null;
            return (d, e) => changedCallback((TOwner)(object)d);
        }
        static PropertyChangedCallback ToHandler<TOwner, TProperty>(Action<TOwner, TProperty, TProperty> changedCallback) where TOwner : class {
            if(changedCallback == null)
                return null;
            return (d, e) => changedCallback((TOwner)(object)d, (TProperty)e.OldValue, (TProperty)e.NewValue);
        }
        static PropertyChangedCallback ToHandler<TOwner>(Action<TOwner, DependencyPropertyChangedEventArgs> changedCallback) where TOwner : class {
            if(changedCallback == null)
                return null;
            return (d, e) => changedCallback((TOwner)(object)d, e);
        }
        static CoerceValueCallback ToCoerce<TOwner, TProperty>(Func<TOwner, TProperty, TProperty> coerceCallback) where TOwner : class {
            if(coerceCallback == null)
                return null;
            return (d, o) => coerceCallback((TOwner)(object)d, (TProperty)o);
        }
        internal static string GetPropertyName<TProperty>(Expression<Func<T, TProperty>> property) {
            return (property.Body as MemberExpression).Member.Name;
        }
        static string GetPropertyNameFromMethod<TOwner, TProperty>(string methodName) {
            if(!methodName.StartsWith("Get"))
                throw new ArgumentException();
            return methodName.Substring(3);
        }

        class DefaultStyleKeyHelper : FrameworkElement {
            public static new DependencyProperty DefaultStyleKeyProperty { get { return FrameworkElement.DefaultStyleKeyProperty; } }
        }
    }
}
