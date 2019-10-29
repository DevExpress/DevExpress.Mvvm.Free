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
    public class DependencyPropertyRegistrator<T> {
        protected DependencyPropertyRegistrator() { }
        public static DependencyPropertyRegistrator<T> New() {
            return new DependencyPropertyRegistrator<T>();
        }


        public DependencyPropertyRegistrator<T> RegisterFast<TProperty>(Expression<Func<T, TProperty>> property, Func<T, DPValueStorage<TProperty>> getStorage, out DependencyProperty propertyField, TProperty defaultValue, FrameworkPropertyMetadataOptions? frameworkOptions = null) {
            return Register(property, out propertyField, CreateMetadata(defaultValue, getStorage, (Action<T>)null, null, frameworkOptions));
        }
        public DependencyPropertyRegistrator<T> RegisterFast<TProperty>(Expression<Func<T, TProperty>> property, Func<T, DPValueStorage<TProperty>> getStorage, out DependencyProperty propertyField, TProperty defaultValue, Func<T, TProperty, TProperty> coerceCallback, FrameworkPropertyMetadataOptions? frameworkOptions = null) {
            return Register(property, out propertyField, CreateMetadata(defaultValue, getStorage, (Action<T>)null, coerceCallback, frameworkOptions));
        }
        public DependencyPropertyRegistrator<T> RegisterFast<TProperty>(Expression<Func<T, TProperty>> property, Func<T, DPValueStorage<TProperty>> getStorage, out DependencyProperty propertyField, TProperty defaultValue, Action<T> changedCallback, FrameworkPropertyMetadataOptions? frameworkOptions = null) {
            return Register(property, out propertyField, CreateMetadata(defaultValue, getStorage, changedCallback, null, frameworkOptions));
        }
        public DependencyPropertyRegistrator<T> RegisterFast<TProperty>(Expression<Func<T, TProperty>> property, Func<T, DPValueStorage<TProperty>> getStorage, out DependencyProperty propertyField, TProperty defaultValue, Action<T, DependencyPropertyChangedEventArgs> changedCallback, FrameworkPropertyMetadataOptions? frameworkOptions = null) {
            return Register(property, out propertyField, CreateMetadata(defaultValue, getStorage, changedCallback, null, frameworkOptions));
        }
        public DependencyPropertyRegistrator<T> RegisterFast<TProperty>(Expression<Func<T, TProperty>> property, Func<T, DPValueStorage<TProperty>> getStorage, out DependencyProperty propertyField, TProperty defaultValue, Action<T, TProperty, TProperty> changedCallback, FrameworkPropertyMetadataOptions? frameworkOptions = null) {
            return Register(property, out propertyField, CreateMetadata(defaultValue, getStorage, changedCallback, null, frameworkOptions));
        }
        public DependencyPropertyRegistrator<T> RegisterFast<TProperty>(Expression<Func<T, TProperty>> property, Func<T, DPValueStorage<TProperty>> getStorage, out DependencyProperty propertyField, TProperty defaultValue, Action<T> changedCallback, Func<T, TProperty, TProperty> coerceCallback, FrameworkPropertyMetadataOptions? frameworkOptions = null) {
            return Register(property, out propertyField, CreateMetadata(defaultValue, getStorage, changedCallback, coerceCallback, frameworkOptions));
        }
        public DependencyPropertyRegistrator<T> RegisterFast<TProperty>(Expression<Func<T, TProperty>> property, Func<T, DPValueStorage<TProperty>> getStorage, out DependencyProperty propertyField, TProperty defaultValue, Action<T, DependencyPropertyChangedEventArgs> changedCallback, Func<T, TProperty, TProperty> coerceCallback, FrameworkPropertyMetadataOptions? frameworkOptions = null) {
            return Register(property, out propertyField, CreateMetadata(defaultValue, getStorage, changedCallback, coerceCallback, frameworkOptions));
        }
        public DependencyPropertyRegistrator<T> RegisterFast<TProperty>(Expression<Func<T, TProperty>> property, Func<T, DPValueStorage<TProperty>> getStorage, out DependencyProperty propertyField, TProperty defaultValue, Action<T, TProperty, TProperty> changedCallback, Func<T, TProperty, TProperty> coerceCallback, FrameworkPropertyMetadataOptions? frameworkOptions = null) {
            return Register(property, out propertyField, CreateMetadata(defaultValue, getStorage, changedCallback, coerceCallback, frameworkOptions));
        }

        public DependencyPropertyRegistrator<T> RegisterFastReadOnly<TProperty>(Expression<Func<T, TProperty>> property, Func<T, DPValueStorage<TProperty>> getStorage, out DependencyPropertyKey propertyFieldKey, out DependencyProperty propertyField, TProperty defaultValue, FrameworkPropertyMetadataOptions? frameworkOptions = null) {
            return RegisterReadOnly(property, out propertyFieldKey, out propertyField, CreateMetadata(defaultValue, getStorage, (Action<T>)null, null, frameworkOptions));
        }
        public DependencyPropertyRegistrator<T> RegisterFastReadOnly<TProperty>(Expression<Func<T, TProperty>> property, Func<T, DPValueStorage<TProperty>> getStorage, out DependencyPropertyKey propertyFieldKey, out DependencyProperty propertyField, TProperty defaultValue, Func<T, TProperty, TProperty> coerceCallback, FrameworkPropertyMetadataOptions? frameworkOptions = null) {
            return RegisterReadOnly(property, out propertyFieldKey, out propertyField, CreateMetadata(defaultValue, getStorage, (Action<T>)null, coerceCallback, frameworkOptions));
        }
        public DependencyPropertyRegistrator<T> RegisterFastReadOnly<TProperty>(Expression<Func<T, TProperty>> property, Func<T, DPValueStorage<TProperty>> getStorage, out DependencyPropertyKey propertyFieldKey, out DependencyProperty propertyField, TProperty defaultValue, Action<T> changedCallback, FrameworkPropertyMetadataOptions? frameworkOptions = null) {
            return RegisterReadOnly(property, out propertyFieldKey, out propertyField, CreateMetadata(defaultValue, getStorage, changedCallback, null, frameworkOptions));
        }
        public DependencyPropertyRegistrator<T> RegisterFastReadOnly<TProperty>(Expression<Func<T, TProperty>> property, Func<T, DPValueStorage<TProperty>> getStorage, out DependencyPropertyKey propertyFieldKey, out DependencyProperty propertyField, TProperty defaultValue, Action<T, DependencyPropertyChangedEventArgs> changedCallback, FrameworkPropertyMetadataOptions? frameworkOptions = null) {
            return RegisterReadOnly(property, out propertyFieldKey, out propertyField, CreateMetadata(defaultValue, getStorage, changedCallback, null, frameworkOptions));
        }
        public DependencyPropertyRegistrator<T> RegisterFastReadOnly<TProperty>(Expression<Func<T, TProperty>> property, Func<T, DPValueStorage<TProperty>> getStorage, out DependencyPropertyKey propertyFieldKey, out DependencyProperty propertyField, TProperty defaultValue, Action<T, TProperty, TProperty> changedCallback, FrameworkPropertyMetadataOptions? frameworkOptions = null) {
            return RegisterReadOnly(property, out propertyFieldKey, out propertyField, CreateMetadata(defaultValue, getStorage, changedCallback, null, frameworkOptions));
        }
        public DependencyPropertyRegistrator<T> RegisterFastReadOnly<TProperty>(Expression<Func<T, TProperty>> property, Func<T, DPValueStorage<TProperty>> getStorage, out DependencyPropertyKey propertyFieldKey, out DependencyProperty propertyField, TProperty defaultValue, Action<T> changedCallback, Func<T, TProperty, TProperty> coerceCallback, FrameworkPropertyMetadataOptions? frameworkOptions = null) {
            return RegisterReadOnly(property, out propertyFieldKey, out propertyField, CreateMetadata(defaultValue, getStorage, changedCallback, coerceCallback, frameworkOptions));
        }
        public DependencyPropertyRegistrator<T> RegisterFastReadOnly<TProperty>(Expression<Func<T, TProperty>> property, Func<T, DPValueStorage<TProperty>> getStorage, out DependencyPropertyKey propertyFieldKey, out DependencyProperty propertyField, TProperty defaultValue, Action<T, DependencyPropertyChangedEventArgs> changedCallback, Func<T, TProperty, TProperty> coerceCallback, FrameworkPropertyMetadataOptions? frameworkOptions = null) {
            return RegisterReadOnly(property, out propertyFieldKey, out propertyField, CreateMetadata(defaultValue, getStorage, changedCallback, coerceCallback, frameworkOptions));
        }
        public DependencyPropertyRegistrator<T> RegisterFastReadOnly<TProperty>(Expression<Func<T, TProperty>> property, Func<T, DPValueStorage<TProperty>> getStorage, out DependencyPropertyKey propertyFieldKey, out DependencyProperty propertyField, TProperty defaultValue, Action<T, TProperty, TProperty> changedCallback, Func<T, TProperty, TProperty> coerceCallback, FrameworkPropertyMetadataOptions? frameworkOptions = null) {
            return RegisterReadOnly(property, out propertyFieldKey, out propertyField, CreateMetadata(defaultValue, getStorage, changedCallback, coerceCallback, frameworkOptions));
        }

        public DependencyPropertyRegistrator<T> RegisterFastAttached<TOwner, TProperty>(Expression<Func<TOwner, TProperty>> getMethodName, Func<TOwner, DPValueStorage<TProperty>> getStorage, out DependencyProperty propertyField, TProperty defaultValue, FrameworkPropertyMetadataOptions? frameworkOptions = null) where TOwner : DependencyObject {
            return RegisterAttached(getMethodName, out propertyField, CreateMetadata(defaultValue, getStorage, (Action<TOwner>)null, null, frameworkOptions));
        }
        public DependencyPropertyRegistrator<T> RegisterFastAttached<TOwner, TProperty>(Expression<Func<TOwner, TProperty>> getMethodName, Func<TOwner, DPValueStorage<TProperty>> getStorage, out DependencyProperty propertyField, TProperty defaultValue, Func<TOwner, TProperty, TProperty> coerceCallback, FrameworkPropertyMetadataOptions? frameworkOptions = null) where TOwner : DependencyObject {
            return RegisterAttached(getMethodName, out propertyField, CreateMetadata(defaultValue, getStorage, (Action<TOwner>)null, coerceCallback, frameworkOptions));
        }
        public DependencyPropertyRegistrator<T> RegisterFastAttached<TOwner, TProperty>(Expression<Func<TOwner, TProperty>> getMethodName, Func<TOwner, DPValueStorage<TProperty>> getStorage, out DependencyProperty propertyField, TProperty defaultValue, Action<TOwner> changedCallback, FrameworkPropertyMetadataOptions? frameworkOptions = null) where TOwner : DependencyObject {
            return RegisterAttached(getMethodName, out propertyField, CreateMetadata(defaultValue, getStorage, changedCallback, null, frameworkOptions));
        }
        public DependencyPropertyRegistrator<T> RegisterFastAttached<TOwner, TProperty>(Expression<Func<TOwner, TProperty>> getMethodName, Func<TOwner, DPValueStorage<TProperty>> getStorage, out DependencyProperty propertyField, TProperty defaultValue, Action<TOwner, DependencyPropertyChangedEventArgs> changedCallback, FrameworkPropertyMetadataOptions? frameworkOptions = null) where TOwner : DependencyObject {
            return RegisterAttached(getMethodName, out propertyField, CreateMetadata(defaultValue, getStorage, changedCallback, null, frameworkOptions));
        }
        public DependencyPropertyRegistrator<T> RegisterFastAttached<TOwner, TProperty>(Expression<Func<TOwner, TProperty>> getMethodName, Func<TOwner, DPValueStorage<TProperty>> getStorage, out DependencyProperty propertyField, TProperty defaultValue, Action<TOwner, TProperty, TProperty> changedCallback, FrameworkPropertyMetadataOptions? frameworkOptions = null) where TOwner : DependencyObject {
            return RegisterAttached(getMethodName, out propertyField, CreateMetadata(defaultValue, getStorage, changedCallback, null, frameworkOptions));
        }
        public DependencyPropertyRegistrator<T> RegisterFastAttached<TOwner, TProperty>(Expression<Func<TOwner, TProperty>> getMethodName, Func<TOwner, DPValueStorage<TProperty>> getStorage, out DependencyProperty propertyField, TProperty defaultValue, Action<TOwner> changedCallback, Func<TOwner, TProperty, TProperty> coerceCallback, FrameworkPropertyMetadataOptions? frameworkOptions = null) where TOwner : DependencyObject {
            return RegisterAttached(getMethodName, out propertyField, CreateMetadata(defaultValue, getStorage, changedCallback, coerceCallback, frameworkOptions));
        }
        public DependencyPropertyRegistrator<T> RegisterFastAttached<TOwner, TProperty>(Expression<Func<TOwner, TProperty>> getMethodName, Func<TOwner, DPValueStorage<TProperty>> getStorage, out DependencyProperty propertyField, TProperty defaultValue, Action<TOwner, DependencyPropertyChangedEventArgs> changedCallback, Func<TOwner, TProperty, TProperty> coerceCallback, FrameworkPropertyMetadataOptions? frameworkOptions = null) where TOwner : DependencyObject {
            return RegisterAttached(getMethodName, out propertyField, CreateMetadata(defaultValue, getStorage, changedCallback, coerceCallback, frameworkOptions));
        }
        public DependencyPropertyRegistrator<T> RegisterFastAttached<TOwner, TProperty>(Expression<Func<TOwner, TProperty>> getMethodName, Func<TOwner, DPValueStorage<TProperty>> getStorage, out DependencyProperty propertyField, TProperty defaultValue, Action<TOwner, TProperty, TProperty> changedCallback, Func<TOwner, TProperty, TProperty> coerceCallback, FrameworkPropertyMetadataOptions? frameworkOptions = null) where TOwner : DependencyObject {
            return RegisterAttached(getMethodName, out propertyField, CreateMetadata(defaultValue, getStorage, changedCallback, coerceCallback, frameworkOptions));
        }

        public DependencyPropertyRegistrator<T> RegisterFastAttachedReadOnly<TOwner, TProperty>(Expression<Func<TOwner, TProperty>> getMethodName, Func<TOwner, DPValueStorage<TProperty>> getStorage, out DependencyPropertyKey propertyFieldKey, out DependencyProperty propertyField, TProperty defaultValue, FrameworkPropertyMetadataOptions? frameworkOptions = null) where TOwner : DependencyObject {
            return RegisterAttachedReadOnly(getMethodName, out propertyFieldKey, out propertyField, CreateMetadata(defaultValue, getStorage, (Action<TOwner>)null, null, frameworkOptions));
        }
        public DependencyPropertyRegistrator<T> RegisterFastAttachedReadOnly<TOwner, TProperty>(Expression<Func<TOwner, TProperty>> getMethodName, Func<TOwner, DPValueStorage<TProperty>> getStorage, out DependencyPropertyKey propertyFieldKey, out DependencyProperty propertyField, TProperty defaultValue, Func<TOwner, TProperty, TProperty> coerceCallback, FrameworkPropertyMetadataOptions? frameworkOptions = null) where TOwner : DependencyObject {
            return RegisterAttachedReadOnly(getMethodName, out propertyFieldKey, out propertyField, CreateMetadata(defaultValue, getStorage, (Action<TOwner>)null, coerceCallback, frameworkOptions));
        }
        public DependencyPropertyRegistrator<T> RegisterFastAttachedReadOnly<TOwner, TProperty>(Expression<Func<TOwner, TProperty>> getMethodName, Func<TOwner, DPValueStorage<TProperty>> getStorage, out DependencyPropertyKey propertyFieldKey, out DependencyProperty propertyField, TProperty defaultValue, Action<TOwner> changedCallback, FrameworkPropertyMetadataOptions? frameworkOptions = null) where TOwner : DependencyObject {
            return RegisterAttachedReadOnly(getMethodName, out propertyFieldKey, out propertyField, CreateMetadata(defaultValue, getStorage, changedCallback, null, frameworkOptions));
        }
        public DependencyPropertyRegistrator<T> RegisterFastAttachedReadOnly<TOwner, TProperty>(Expression<Func<TOwner, TProperty>> getMethodName, Func<TOwner, DPValueStorage<TProperty>> getStorage, out DependencyPropertyKey propertyFieldKey, out DependencyProperty propertyField, TProperty defaultValue, Action<TOwner, DependencyPropertyChangedEventArgs> changedCallback, FrameworkPropertyMetadataOptions? frameworkOptions = null) where TOwner : DependencyObject {
            return RegisterAttachedReadOnly(getMethodName, out propertyFieldKey, out propertyField, CreateMetadata(defaultValue, getStorage, changedCallback, null, frameworkOptions));
        }
        public DependencyPropertyRegistrator<T> RegisterFastAttachedReadOnly<TOwner, TProperty>(Expression<Func<TOwner, TProperty>> getMethodName, Func<TOwner, DPValueStorage<TProperty>> getStorage, out DependencyPropertyKey propertyFieldKey, out DependencyProperty propertyField, TProperty defaultValue, Action<TOwner, TProperty, TProperty> changedCallback, FrameworkPropertyMetadataOptions? frameworkOptions = null) where TOwner : DependencyObject {
            return RegisterAttachedReadOnly(getMethodName, out propertyFieldKey, out propertyField, CreateMetadata(defaultValue, getStorage, changedCallback, null, frameworkOptions));
        }
        public DependencyPropertyRegistrator<T> RegisterFastAttachedReadOnly<TOwner, TProperty>(Expression<Func<TOwner, TProperty>> getMethodName, Func<TOwner, DPValueStorage<TProperty>> getStorage, out DependencyPropertyKey propertyFieldKey, out DependencyProperty propertyField, TProperty defaultValue, Action<TOwner> changedCallback, Func<TOwner, TProperty, TProperty> coerceCallback, FrameworkPropertyMetadataOptions? frameworkOptions = null) where TOwner : DependencyObject {
            return RegisterAttachedReadOnly(getMethodName, out propertyFieldKey, out propertyField, CreateMetadata(defaultValue, getStorage, changedCallback, coerceCallback, frameworkOptions));
        }
        public DependencyPropertyRegistrator<T> RegisterFastAttachedReadOnly<TOwner, TProperty>(Expression<Func<TOwner, TProperty>> getMethodName, Func<TOwner, DPValueStorage<TProperty>> getStorage, out DependencyPropertyKey propertyFieldKey, out DependencyProperty propertyField, TProperty defaultValue, Action<TOwner, DependencyPropertyChangedEventArgs> changedCallback, Func<TOwner, TProperty, TProperty> coerceCallback, FrameworkPropertyMetadataOptions? frameworkOptions = null) where TOwner : DependencyObject {
            return RegisterAttachedReadOnly(getMethodName, out propertyFieldKey, out propertyField, CreateMetadata(defaultValue, getStorage, changedCallback, coerceCallback, frameworkOptions));
        }
        public DependencyPropertyRegistrator<T> RegisterFastAttachedReadOnly<TOwner, TProperty>(Expression<Func<TOwner, TProperty>> getMethodName, Func<TOwner, DPValueStorage<TProperty>> getStorage, out DependencyPropertyKey propertyFieldKey, out DependencyProperty propertyField, TProperty defaultValue, Action<TOwner, TProperty, TProperty> changedCallback, Func<TOwner, TProperty, TProperty> coerceCallback, FrameworkPropertyMetadataOptions? frameworkOptions = null) where TOwner : DependencyObject {
            return RegisterAttachedReadOnly(getMethodName, out propertyFieldKey, out propertyField, CreateMetadata(defaultValue, getStorage, changedCallback, coerceCallback, frameworkOptions));
        }

        public DependencyPropertyRegistrator<T> AddOwnerFast<TProperty>(Expression<Func<T, TProperty>> property, Func<T, DPValueStorage<TProperty>> getStorage, out DependencyProperty propertyField, DependencyProperty sourceProperty, TProperty defaultValue, Action<T> changedCallback = null) {
            return AddOwner(property, out propertyField, sourceProperty, CreateFrameworkMetadata(defaultValue, getStorage, changedCallback));
        }
        public DependencyPropertyRegistrator<T> AddOwnerFast<TProperty>(Expression<Func<T, TProperty>> property, Func<T, DPValueStorage<TProperty>> getStorage, out DependencyProperty propertyField, DependencyProperty sourceProperty, TProperty defaultValue, Action<T, DependencyPropertyChangedEventArgs> changedCallback) {
            return AddOwner(property, out propertyField, sourceProperty, CreateFrameworkMetadata(defaultValue, getStorage, changedCallback));
        }
        public DependencyPropertyRegistrator<T> AddOwnerFast<TProperty>(Expression<Func<T, TProperty>> property, Func<T, DPValueStorage<TProperty>> getStorage, out DependencyProperty propertyField, DependencyProperty sourceProperty, TProperty defaultValue, Action<T, TProperty, TProperty> changedCallback) {
            return AddOwner(property, out propertyField, sourceProperty, CreateFrameworkMetadata(defaultValue, getStorage, changedCallback));
        }
        public DependencyPropertyRegistrator<T> AddOwnerFast<TProperty>(Expression<Func<T, TProperty>> property, Func<T, DPValueStorage<TProperty>> getStorage, out DependencyProperty propertyField, DependencyProperty sourceProperty, TProperty defaultValue, Action<T, TProperty, TProperty> changedCallback, Func<T, TProperty, TProperty> coerceCallback, FrameworkPropertyMetadataOptions frameworkOptions) {
            return AddOwner(property, out propertyField, sourceProperty, CreateMetadata(defaultValue, getStorage, changedCallback, coerceCallback, frameworkOptions));
        }
        public DependencyPropertyRegistrator<T> AddOwnerFast<TProperty>(Expression<Func<T, TProperty>> property, Func<T, DPValueStorage<TProperty>> getStorage, out DependencyProperty propertyField, DependencyProperty sourceProperty, TProperty defaultValue, FrameworkPropertyMetadataOptions frameworkOptions) {
            return AddOwner(property, out propertyField, sourceProperty, CreateMetadata(defaultValue, getStorage, (Action<T>)null, null, frameworkOptions));
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
        public DependencyPropertyRegistrator<T> OverrideMetadataFast<TProperty>(Expression<Func<T, TProperty>> getMethodName, Func<T, DPValueStorage<TProperty>> getStorage, DependencyProperty propertyField, Action<T, TProperty, TProperty> changedCallback) {
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




        public DependencyPropertyRegistrator<T> AddOwner<TProperty>(Expression<Func<T, TProperty>> property, out DependencyProperty propertyField, DependencyProperty sourceProperty) {
            return AddOwner(property, out propertyField, sourceProperty, null);
        }
        public DependencyPropertyRegistrator<T> AddOwner<TProperty>(Expression<Func<T, TProperty>> property, out DependencyProperty propertyField, DependencyProperty sourceProperty, TProperty defaultValue, Action<T> changedCallback = null) {
            return AddOwner(property, out propertyField, sourceProperty, CreateFrameworkMetadata(defaultValue, changedCallback));
        }
        public DependencyPropertyRegistrator<T> AddOwner<TProperty>(Expression<Func<T, TProperty>> property, out DependencyProperty propertyField, DependencyProperty sourceProperty, TProperty defaultValue, Action<T, DependencyPropertyChangedEventArgs> changedCallback) {
            return AddOwner(property, out propertyField, sourceProperty, CreateFrameworkMetadata(defaultValue, changedCallback));
        }
        public DependencyPropertyRegistrator<T> AddOwner<TProperty>(Expression<Func<T, TProperty>> property, out DependencyProperty propertyField, DependencyProperty sourceProperty, TProperty defaultValue, Action<T, TProperty, TProperty> changedCallback) {
            return AddOwner(property, out propertyField, sourceProperty, CreateFrameworkMetadata(defaultValue, changedCallback));
        }
        public DependencyPropertyRegistrator<T> AddOwner<TProperty>(Expression<Func<T, TProperty>> property, out DependencyProperty propertyField, DependencyProperty sourceProperty, TProperty defaultValue, Action<T, TProperty, TProperty> changedCallback, Func<T, TProperty, TProperty> coerceCallback, FrameworkPropertyMetadataOptions frameworkOptions) {
            return AddOwner(property, out propertyField, sourceProperty, CreateMetadata(defaultValue, changedCallback, coerceCallback, frameworkOptions));
        }
        public DependencyPropertyRegistrator<T> AddOwner<TProperty>(Expression<Func<T, TProperty>> property, out DependencyProperty propertyField, DependencyProperty sourceProperty, TProperty defaultValue, FrameworkPropertyMetadataOptions frameworkOptions) {
            return AddOwner(property, out propertyField, sourceProperty, CreateMetadata(defaultValue, (Action<T>)null, null, frameworkOptions));
        }
        DependencyPropertyRegistrator<T> AddOwner<TProperty>(Expression<Func<T, TProperty>> property, out DependencyProperty propertyField, DependencyProperty sourceProperty, PropertyMetadata metadata) {
            propertyField = sourceProperty.AddOwner(typeof(T), metadata);
            return this;
        }

        public DependencyPropertyRegistrator<T> Register<TProperty>(Expression<Func<T, TProperty>> property, out DependencyProperty propertyField, TProperty defaultValue, FrameworkPropertyMetadataOptions? frameworkOptions = null) {
            return Register(property, out propertyField, CreateMetadata(defaultValue, (Action<T>)null, null, frameworkOptions));
        }
        public DependencyPropertyRegistrator<T> Register<TProperty>(Expression<Func<T, TProperty>> property, out DependencyProperty propertyField, TProperty defaultValue, Func<T, TProperty, TProperty> coerceCallback, FrameworkPropertyMetadataOptions? frameworkOptions = null) {
            return Register(property, out propertyField, CreateMetadata(defaultValue, (Action<T>)null, coerceCallback, frameworkOptions));
        }
        public DependencyPropertyRegistrator<T> Register<TProperty>(Expression<Func<T, TProperty>> property, out DependencyProperty propertyField, TProperty defaultValue, Action<T> changedCallback, FrameworkPropertyMetadataOptions? frameworkOptions = null) {
            return Register(property, out propertyField, CreateMetadata(defaultValue, changedCallback, null, frameworkOptions));
        }
        public DependencyPropertyRegistrator<T> Register<TProperty>(Expression<Func<T, TProperty>> property, out DependencyProperty propertyField, TProperty defaultValue, Action<T, DependencyPropertyChangedEventArgs> changedCallback, FrameworkPropertyMetadataOptions? frameworkOptions = null) {
            return Register(property, out propertyField, CreateMetadata(defaultValue, changedCallback, null, frameworkOptions));
        }
        public DependencyPropertyRegistrator<T> Register<TProperty>(Expression<Func<T, TProperty>> property, out DependencyProperty propertyField, TProperty defaultValue, Action<T, TProperty, TProperty> changedCallback, FrameworkPropertyMetadataOptions? frameworkOptions = null) {
            return Register(property, out propertyField, CreateMetadata(defaultValue, changedCallback, null, frameworkOptions));
        }
        public DependencyPropertyRegistrator<T> Register<TProperty>(Expression<Func<T, TProperty>> property, out DependencyProperty propertyField, TProperty defaultValue, Action<T> changedCallback, Func<T, TProperty, TProperty> coerceCallback, FrameworkPropertyMetadataOptions? frameworkOptions = null) {
            return Register(property, out propertyField, CreateMetadata(defaultValue, changedCallback, coerceCallback, frameworkOptions));
        }
        public DependencyPropertyRegistrator<T> Register<TProperty>(Expression<Func<T, TProperty>> property, out DependencyProperty propertyField, TProperty defaultValue, Action<T, DependencyPropertyChangedEventArgs> changedCallback, Func<T, TProperty, TProperty> coerceCallback, FrameworkPropertyMetadataOptions? frameworkOptions = null) {
            return Register(property, out propertyField, CreateMetadata(defaultValue, changedCallback, coerceCallback, frameworkOptions));
        }
        public DependencyPropertyRegistrator<T> Register<TProperty>(Expression<Func<T, TProperty>> property, out DependencyProperty propertyField, TProperty defaultValue, Action<T, TProperty, TProperty> changedCallback, Func<T, TProperty, TProperty> coerceCallback, FrameworkPropertyMetadataOptions? frameworkOptions = null) {
            return Register(property, out propertyField, CreateMetadata(defaultValue, changedCallback, coerceCallback, frameworkOptions));
        }
        DependencyPropertyRegistrator<T> Register<TProperty>(Expression<Func<T, TProperty>> property, out DependencyProperty propertyField, PropertyMetadata metadata) {
            propertyField = DependencyProperty.Register(GetPropertyName(property), typeof(TProperty), typeof(T), metadata);
            return this;
        }

        public DependencyPropertyRegistrator<T> RegisterReadOnly<TProperty>(Expression<Func<T, TProperty>> property, out DependencyPropertyKey propertyFieldKey, out DependencyProperty propertyField, TProperty defaultValue, FrameworkPropertyMetadataOptions? frameworkOptions = null) {
            return RegisterReadOnly(property, out propertyFieldKey, out propertyField, CreateMetadata(defaultValue, (Action<T>)null, null, frameworkOptions));
        }
        public DependencyPropertyRegistrator<T> RegisterReadOnly<TProperty>(Expression<Func<T, TProperty>> property, out DependencyPropertyKey propertyFieldKey, out DependencyProperty propertyField, TProperty defaultValue, Func<T, TProperty, TProperty> coerceCallback, FrameworkPropertyMetadataOptions? frameworkOptions = null) {
            return RegisterReadOnly(property, out propertyFieldKey, out propertyField, CreateMetadata(defaultValue, (Action<T>)null, coerceCallback, frameworkOptions));
        }
        public DependencyPropertyRegistrator<T> RegisterReadOnly<TProperty>(Expression<Func<T, TProperty>> property, out DependencyPropertyKey propertyFieldKey, out DependencyProperty propertyField, TProperty defaultValue, Action<T> changedCallback, FrameworkPropertyMetadataOptions? frameworkOptions = null) {
            return RegisterReadOnly(property, out propertyFieldKey, out propertyField, CreateMetadata(defaultValue, changedCallback, null, frameworkOptions));
        }
        public DependencyPropertyRegistrator<T> RegisterReadOnly<TProperty>(Expression<Func<T, TProperty>> property, out DependencyPropertyKey propertyFieldKey, out DependencyProperty propertyField, TProperty defaultValue, Action<T, DependencyPropertyChangedEventArgs> changedCallback, FrameworkPropertyMetadataOptions? frameworkOptions = null) {
            return RegisterReadOnly(property, out propertyFieldKey, out propertyField, CreateMetadata(defaultValue, changedCallback, null, frameworkOptions));
        }
        public DependencyPropertyRegistrator<T> RegisterReadOnly<TProperty>(Expression<Func<T, TProperty>> property, out DependencyPropertyKey propertyFieldKey, out DependencyProperty propertyField, TProperty defaultValue, Action<T, TProperty, TProperty> changedCallback, FrameworkPropertyMetadataOptions? frameworkOptions = null) {
            return RegisterReadOnly(property, out propertyFieldKey, out propertyField, CreateMetadata(defaultValue, changedCallback, null, frameworkOptions));
        }
        public DependencyPropertyRegistrator<T> RegisterReadOnly<TProperty>(Expression<Func<T, TProperty>> property, out DependencyPropertyKey propertyFieldKey, out DependencyProperty propertyField, TProperty defaultValue, Action<T> changedCallback, Func<T, TProperty, TProperty> coerceCallback, FrameworkPropertyMetadataOptions? frameworkOptions = null) {
            return RegisterReadOnly(property, out propertyFieldKey, out propertyField, CreateMetadata(defaultValue, changedCallback, coerceCallback, frameworkOptions));
        }
        public DependencyPropertyRegistrator<T> RegisterReadOnly<TProperty>(Expression<Func<T, TProperty>> property, out DependencyPropertyKey propertyFieldKey, out DependencyProperty propertyField, TProperty defaultValue, Action<T, DependencyPropertyChangedEventArgs> changedCallback, Func<T, TProperty, TProperty> coerceCallback, FrameworkPropertyMetadataOptions? frameworkOptions = null) {
            return RegisterReadOnly(property, out propertyFieldKey, out propertyField, CreateMetadata(defaultValue, changedCallback, coerceCallback, frameworkOptions));
        }
        public DependencyPropertyRegistrator<T> RegisterReadOnly<TProperty>(Expression<Func<T, TProperty>> property, out DependencyPropertyKey propertyFieldKey, out DependencyProperty propertyField, TProperty defaultValue, Action<T, TProperty, TProperty> changedCallback, Func<T, TProperty, TProperty> coerceCallback, FrameworkPropertyMetadataOptions? frameworkOptions = null) {
            return RegisterReadOnly(property, out propertyFieldKey, out propertyField, CreateMetadata(defaultValue, changedCallback, coerceCallback, frameworkOptions));
        }
        DependencyPropertyRegistrator<T> RegisterReadOnly<TProperty>(Expression<Func<T, TProperty>> property, out DependencyPropertyKey propertyFieldKey, out DependencyProperty propertyField, PropertyMetadata metadata) {
            propertyFieldKey = DependencyProperty.RegisterReadOnly(GetPropertyName(property), typeof(TProperty), typeof(T), metadata);
            propertyField = propertyFieldKey.DependencyProperty;
            return this;
        }

        public DependencyPropertyRegistrator<T> RegisterAttached<TOwner, TProperty>(Expression<Func<TOwner, TProperty>> getMethodName, out DependencyProperty propertyField, TProperty defaultValue, FrameworkPropertyMetadataOptions? frameworkOptions = null) where TOwner : DependencyObject {
            return RegisterAttached(getMethodName, out propertyField, CreateMetadata(defaultValue, (Action<TOwner>)null, null, frameworkOptions));
        }
        public DependencyPropertyRegistrator<T> RegisterAttached<TOwner, TProperty>(Expression<Func<TOwner, TProperty>> getMethodName, out DependencyProperty propertyField, TProperty defaultValue, Func<TOwner, TProperty, TProperty> coerceCallback, FrameworkPropertyMetadataOptions? frameworkOptions = null) where TOwner : DependencyObject {
            return RegisterAttached(getMethodName, out propertyField, CreateMetadata(defaultValue, (Action<TOwner>)null, coerceCallback, frameworkOptions));
        }
        public DependencyPropertyRegistrator<T> RegisterAttached<TOwner, TProperty>(Expression<Func<TOwner, TProperty>> getMethodName, out DependencyProperty propertyField, TProperty defaultValue, Action<TOwner> changedCallback, FrameworkPropertyMetadataOptions? frameworkOptions = null) where TOwner : DependencyObject {
            return RegisterAttached(getMethodName, out propertyField, CreateMetadata(defaultValue, changedCallback, null, frameworkOptions));
        }
        public DependencyPropertyRegistrator<T> RegisterAttached<TOwner, TProperty>(Expression<Func<TOwner, TProperty>> getMethodName, out DependencyProperty propertyField, TProperty defaultValue, Action<TOwner, DependencyPropertyChangedEventArgs> changedCallback, FrameworkPropertyMetadataOptions? frameworkOptions = null) where TOwner : DependencyObject {
            return RegisterAttached(getMethodName, out propertyField, CreateMetadata(defaultValue, changedCallback, null, frameworkOptions));
        }
        public DependencyPropertyRegistrator<T> RegisterAttached<TOwner, TProperty>(Expression<Func<TOwner, TProperty>> getMethodName, out DependencyProperty propertyField, TProperty defaultValue, Action<TOwner, TProperty, TProperty> changedCallback, FrameworkPropertyMetadataOptions? frameworkOptions = null) where TOwner : DependencyObject {
            return RegisterAttached(getMethodName, out propertyField, CreateMetadata(defaultValue, changedCallback, null, frameworkOptions));
        }
        public DependencyPropertyRegistrator<T> RegisterAttached<TOwner, TProperty>(Expression<Func<TOwner, TProperty>> getMethodName, out DependencyProperty propertyField, TProperty defaultValue, Action<TOwner> changedCallback, Func<TOwner, TProperty, TProperty> coerceCallback, FrameworkPropertyMetadataOptions? frameworkOptions = null) where TOwner : DependencyObject {
            return RegisterAttached(getMethodName, out propertyField, CreateMetadata(defaultValue, changedCallback, coerceCallback, frameworkOptions));
        }
        public DependencyPropertyRegistrator<T> RegisterAttached<TOwner, TProperty>(Expression<Func<TOwner, TProperty>> getMethodName, out DependencyProperty propertyField, TProperty defaultValue, Action<TOwner, DependencyPropertyChangedEventArgs> changedCallback, Func<TOwner, TProperty, TProperty> coerceCallback, FrameworkPropertyMetadataOptions? frameworkOptions = null) where TOwner : DependencyObject {
            return RegisterAttached(getMethodName, out propertyField, CreateMetadata(defaultValue, changedCallback, coerceCallback, frameworkOptions));
        }
        public DependencyPropertyRegistrator<T> RegisterAttached<TOwner, TProperty>(Expression<Func<TOwner, TProperty>> getMethodName, out DependencyProperty propertyField, TProperty defaultValue, Action<TOwner, TProperty, TProperty> changedCallback, Func<TOwner, TProperty, TProperty> coerceCallback, FrameworkPropertyMetadataOptions? frameworkOptions = null) where TOwner : DependencyObject {
            return RegisterAttached(getMethodName, out propertyField, CreateMetadata(defaultValue, changedCallback, coerceCallback, frameworkOptions));
        }
        DependencyPropertyRegistrator<T> RegisterAttached<TOwner, TProperty>(Expression<Func<TOwner, TProperty>> getMethodName, out DependencyProperty propertyField, PropertyMetadata metadata) where TOwner : DependencyObject {
            propertyField = DependencyProperty.RegisterAttached(GetPropertyNameFromMethod(getMethodName), typeof(TProperty), typeof(T), metadata);
            return this;
        }


        public DependencyPropertyRegistrator<T> RegisterAttachedReadOnly<TOwner, TProperty>(Expression<Func<TOwner, TProperty>> getMethodName, out DependencyPropertyKey propertyFieldKey, out DependencyProperty propertyField, TProperty defaultValue, FrameworkPropertyMetadataOptions? frameworkOptions = null) where TOwner : DependencyObject {
            return RegisterAttachedReadOnly(getMethodName, out propertyFieldKey, out propertyField, CreateMetadata(defaultValue, (Action<TOwner>)null, null, frameworkOptions));
        }
        public DependencyPropertyRegistrator<T> RegisterAttachedReadOnly<TOwner, TProperty>(Expression<Func<TOwner, TProperty>> getMethodName, out DependencyPropertyKey propertyFieldKey, out DependencyProperty propertyField, TProperty defaultValue, Func<TOwner, TProperty, TProperty> coerceCallback, FrameworkPropertyMetadataOptions? frameworkOptions = null) where TOwner : DependencyObject {
            return RegisterAttachedReadOnly(getMethodName, out propertyFieldKey, out propertyField, CreateMetadata(defaultValue, (Action<TOwner>)null, coerceCallback, frameworkOptions));
        }
        public DependencyPropertyRegistrator<T> RegisterAttachedReadOnly<TOwner, TProperty>(Expression<Func<TOwner, TProperty>> getMethodName, out DependencyPropertyKey propertyFieldKey, out DependencyProperty propertyField, TProperty defaultValue, Action<TOwner> changedCallback, FrameworkPropertyMetadataOptions? frameworkOptions = null) where TOwner : DependencyObject {
            return RegisterAttachedReadOnly(getMethodName, out propertyFieldKey, out propertyField, CreateMetadata(defaultValue, changedCallback, null, frameworkOptions));
        }
        public DependencyPropertyRegistrator<T> RegisterAttachedReadOnly<TOwner, TProperty>(Expression<Func<TOwner, TProperty>> getMethodName, out DependencyPropertyKey propertyFieldKey, out DependencyProperty propertyField, TProperty defaultValue, Action<TOwner, DependencyPropertyChangedEventArgs> changedCallback, FrameworkPropertyMetadataOptions? frameworkOptions = null) where TOwner : DependencyObject {
            return RegisterAttachedReadOnly(getMethodName, out propertyFieldKey, out propertyField, CreateMetadata(defaultValue, changedCallback, null, frameworkOptions));
        }
        public DependencyPropertyRegistrator<T> RegisterAttachedReadOnly<TOwner, TProperty>(Expression<Func<TOwner, TProperty>> getMethodName, out DependencyPropertyKey propertyFieldKey, out DependencyProperty propertyField, TProperty defaultValue, Action<TOwner, TProperty, TProperty> changedCallback, FrameworkPropertyMetadataOptions? frameworkOptions = null) where TOwner : DependencyObject {
            return RegisterAttachedReadOnly(getMethodName, out propertyFieldKey, out propertyField, CreateMetadata(defaultValue, changedCallback, null, frameworkOptions));
        }
        public DependencyPropertyRegistrator<T> RegisterAttachedReadOnly<TOwner, TProperty>(Expression<Func<TOwner, TProperty>> getMethodName, out DependencyPropertyKey propertyFieldKey, out DependencyProperty propertyField, TProperty defaultValue, Action<TOwner> changedCallback, Func<TOwner, TProperty, TProperty> coerceCallback, FrameworkPropertyMetadataOptions? frameworkOptions = null) where TOwner : DependencyObject {
            return RegisterAttachedReadOnly(getMethodName, out propertyFieldKey, out propertyField, CreateMetadata(defaultValue, changedCallback, coerceCallback, frameworkOptions));
        }
        public DependencyPropertyRegistrator<T> RegisterAttachedReadOnly<TOwner, TProperty>(Expression<Func<TOwner, TProperty>> getMethodName, out DependencyPropertyKey propertyFieldKey, out DependencyProperty propertyField, TProperty defaultValue, Action<TOwner, DependencyPropertyChangedEventArgs> changedCallback, Func<TOwner, TProperty, TProperty> coerceCallback, FrameworkPropertyMetadataOptions? frameworkOptions = null) where TOwner : DependencyObject {
            return RegisterAttachedReadOnly(getMethodName, out propertyFieldKey, out propertyField, CreateMetadata(defaultValue, changedCallback, coerceCallback, frameworkOptions));
        }
        public DependencyPropertyRegistrator<T> RegisterAttachedReadOnly<TOwner, TProperty>(Expression<Func<TOwner, TProperty>> getMethodName, out DependencyPropertyKey propertyFieldKey, out DependencyProperty propertyField, TProperty defaultValue, Action<TOwner, TProperty, TProperty> changedCallback, Func<TOwner, TProperty, TProperty> coerceCallback, FrameworkPropertyMetadataOptions? frameworkOptions = null) where TOwner : DependencyObject {
            return RegisterAttachedReadOnly(getMethodName, out propertyFieldKey, out propertyField, CreateMetadata(defaultValue, changedCallback, coerceCallback, frameworkOptions));
        }
        DependencyPropertyRegistrator<T> RegisterAttachedReadOnly<TOwner, TProperty>(Expression<Func<TOwner, TProperty>> getMethodName, out DependencyPropertyKey propertyFieldKey, out DependencyProperty propertyField, PropertyMetadata metadata) where TOwner : DependencyObject {
            propertyFieldKey = DependencyProperty.RegisterAttachedReadOnly(GetPropertyNameFromMethod(getMethodName), typeof(TProperty), typeof(T), metadata);
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
        public DependencyPropertyRegistrator<T> OverrideMetadata<TProperty>(Expression<Func<T, TProperty>> getMethodName, DependencyProperty propertyField, Action<T, TProperty, TProperty> changedCallback) {
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

        static PropertyMetadata CreateFrameworkMetadata<TOwner, TProperty>(TProperty defaultValue, Func<TOwner, DPValueStorage<TProperty>> getStorage, Action<TOwner> changedCallback) {
            var handler = ToHandler<TOwner, TProperty>(getStorage, changedCallback);
            return new FrameworkPropertyMetadata(defaultValue, handler);
        }
        static PropertyMetadata CreateFrameworkMetadata<TOwner, TProperty>(TProperty defaultValue, Func<TOwner, DPValueStorage<TProperty>> getStorage, Action<TOwner, DependencyPropertyChangedEventArgs> changedCallback) {
            var handler = ToHandler<TOwner, TProperty>(getStorage, changedCallback);
            return new FrameworkPropertyMetadata(defaultValue, handler);
        }
        static PropertyMetadata CreateFrameworkMetadata<TOwner, TProperty>(TProperty defaultValue, Func<TOwner, DPValueStorage<TProperty>> getStorage, Action<TOwner, TProperty, TProperty> changedCallback) {
            var handler = ToHandler<TOwner, TProperty>(getStorage, changedCallback);
            return new FrameworkPropertyMetadata(defaultValue, handler);
        }
        static PropertyMetadata CreateFrameworkMetadata<TOwner, TProperty>(TProperty defaultValue, Action<TOwner> changedCallback) {
            var handler = ToHandler(changedCallback);
            return new FrameworkPropertyMetadata(defaultValue, handler);
        }
        static PropertyMetadata CreateFrameworkMetadata<TOwner, TProperty>(TProperty defaultValue, Action<TOwner, DependencyPropertyChangedEventArgs> changedCallback) {
            var handler = ToHandler(changedCallback);
            return new FrameworkPropertyMetadata(defaultValue, handler);
        }
        static PropertyMetadata CreateFrameworkMetadata<TOwner, TProperty>(TProperty defaultValue, Action<TOwner, TProperty, TProperty> changedCallback) {
            var handler = ToHandler(changedCallback);
            return new FrameworkPropertyMetadata(defaultValue, handler);
        }
        static PropertyMetadata CreateMetadata<TOwner, TProperty>(TProperty defaultValue, Func<TOwner, DPValueStorage<TProperty>> getStorage, Action<TOwner> changedCallback, Func<TOwner, TProperty, TProperty> coerceCallback, FrameworkPropertyMetadataOptions? frameworkOptions) {
            var handler = ToHandler<TOwner, TProperty>(getStorage, changedCallback);
            var coerce = ToCoerce(coerceCallback);
            return CreateMetadataCore<TOwner, TProperty>(defaultValue, handler, coerce, frameworkOptions);
        }
        static PropertyMetadata CreateMetadata<TOwner, TProperty>(TProperty defaultValue, Func<TOwner, DPValueStorage<TProperty>> getStorage, Action<TOwner, DependencyPropertyChangedEventArgs> changedCallback, Func<TOwner, TProperty, TProperty> coerceCallback, FrameworkPropertyMetadataOptions? frameworkOptions) {
            var handler = ToHandler<TOwner, TProperty>(getStorage, changedCallback);
            var coerce = ToCoerce(coerceCallback);
            return CreateMetadataCore<TOwner, TProperty>(defaultValue, handler, coerce, frameworkOptions);
        }
        static PropertyMetadata CreateMetadata<TOwner, TProperty>(TProperty defaultValue, Func<TOwner, DPValueStorage<TProperty>> getStorage, Action<TOwner, TProperty, TProperty> changedCallback, Func<TOwner, TProperty, TProperty> coerceCallback, FrameworkPropertyMetadataOptions? frameworkOptions) {
            var handler = ToHandler<TOwner, TProperty>(getStorage, changedCallback);
            var coerce = ToCoerce(coerceCallback);
            return CreateMetadataCore<TOwner, TProperty>(defaultValue, handler, coerce, frameworkOptions);
        }
        static PropertyMetadata CreateMetadata<TOwner, TProperty>(TProperty defaultValue, Action<TOwner> changedCallback, Func<TOwner, TProperty, TProperty> coerceCallback, FrameworkPropertyMetadataOptions? frameworkOptions) {
            var handler = ToHandler(changedCallback);
            var coerce = ToCoerce(coerceCallback);
            return CreateMetadataCore<TOwner, TProperty>(defaultValue, handler, coerce, frameworkOptions);
        }
        static PropertyMetadata CreateMetadata<TOwner, TProperty>(TProperty defaultValue, Action<TOwner, DependencyPropertyChangedEventArgs> changedCallback, Func<TOwner, TProperty, TProperty> coerceCallback, FrameworkPropertyMetadataOptions? frameworkOptions) {
            var handler = ToHandler(changedCallback);
            var coerce = ToCoerce(coerceCallback);
            return CreateMetadataCore<TOwner, TProperty>(defaultValue, handler, coerce, frameworkOptions);
        }
        static PropertyMetadata CreateMetadata<TOwner, TProperty>(TProperty defaultValue, Action<TOwner, TProperty, TProperty> changedCallback, Func<TOwner, TProperty, TProperty> coerceCallback, FrameworkPropertyMetadataOptions? frameworkOptions) {
            var handler = ToHandler(changedCallback);
            var coerce = ToCoerce(coerceCallback);
            return CreateMetadataCore<TOwner, TProperty>(defaultValue, handler, coerce, frameworkOptions);
        }
        static PropertyMetadata CreateMetadataCore<TOwner, TProperty>(TProperty defaultValue, PropertyChangedCallback handler, CoerceValueCallback coerce, FrameworkPropertyMetadataOptions? frameworkOptions) {
            return frameworkOptions == null ? new PropertyMetadata(defaultValue, handler, coerce) : new FrameworkPropertyMetadata(defaultValue, frameworkOptions.Value, handler, coerce);
        }
        static PropertyChangedCallback ToHandler<TOwner, TProperty>(Func<TOwner, DPValueStorage<TProperty>> getStorage, Action<TOwner> changedCallback) {
            if (changedCallback == null)
                return (d, e) => SetStorageValue(d, e.NewValue, getStorage);
            return (d, e) => {
                SetStorageValue(d, e.NewValue, getStorage);
                changedCallback((TOwner)(object)d);
            };
        }
        static PropertyChangedCallback ToHandler<TOwner, TProperty>(Func<TOwner, DPValueStorage<TProperty>> getStorage, Action<TOwner, TProperty, TProperty> changedCallback) {
            if (changedCallback == null)
                return (d, e) => SetStorageValue(d, e.NewValue, getStorage);
            return (d, e) => {
                SetStorageValue(d, e.NewValue, getStorage);
                changedCallback((TOwner)(object)d, (TProperty)e.OldValue, (TProperty)e.NewValue);
            };
        }
        static PropertyChangedCallback ToHandler<TOwner, TProperty>(Func<TOwner, DPValueStorage<TProperty>> getStorage, Action<TOwner, DependencyPropertyChangedEventArgs> changedCallback) {
            if (changedCallback == null)
                return (d, e) => SetStorageValue(d, e.NewValue, getStorage);
            return (d, e) => {
                SetStorageValue(d, e.NewValue, getStorage);
                changedCallback((TOwner)(object)d, e);
            };
        }
        static void SetStorageValue<TOwner, TProperty>(DependencyObject d, object newValue, Func<TOwner, DPValueStorage<TProperty>> getStorage) {
            var storage = getStorage((TOwner)(object)d);
            if (storage != null)
                ((IDPValueStorage)storage).SetValue(newValue);
        }
        static PropertyChangedCallback ToHandler<TOwner>(Action<TOwner> changedCallback) {
            return (d, e) => changedCallback.Do(x => x((TOwner)(object)d));
        }
        static PropertyChangedCallback ToHandler<TOwner, TProperty>(Action<TOwner, TProperty, TProperty> changedCallback) {
            return (d, e) => changedCallback.Do(x => x((TOwner)(object)d, (TProperty)e.OldValue, (TProperty)e.NewValue));
        }
        static PropertyChangedCallback ToHandler<TOwner>(Action<TOwner, DependencyPropertyChangedEventArgs> changedCallback) {
            return (d, e) => changedCallback.Do(x => x((TOwner)(object)d, e));
        }
        static CoerceValueCallback ToCoerce<TOwner, TProperty>(Func<TOwner, TProperty, TProperty> coerceCallback) {
            if(coerceCallback == null)
                return null;
            return (d, o) => coerceCallback((TOwner)(object)d, (TProperty)o);
        }
        internal static string GetPropertyName<TProperty>(Expression<Func<T, TProperty>> property) {
            return (property.Body as MemberExpression).Member.Name;
        }
        static string GetPropertyNameFromMethod<TOwner, TProperty>(Expression<Func<TOwner, TProperty>> method) {
            string name = (method.Body as MethodCallExpression).Method.Name;
            if(!name.StartsWith("Get"))
                throw new ArgumentException();
            return name.Substring(3);
        }

        class DefaultStyleKeyHelper : FrameworkElement {
            public static new DependencyProperty DefaultStyleKeyProperty { get { return FrameworkElement.DefaultStyleKeyProperty; } }
        }
    }
    public static class BindableReadOnlyPropertyRegistrator {
        public static DependencyPropertyRegistrator<T> RegisterBindableReadOnly<T, TProperty>(this DependencyPropertyRegistrator<T> registrator, Expression<Func<T, TProperty>> property, out Action<T, TProperty> propertySeter, out DependencyProperty propertyField, TProperty defaultValue, FrameworkPropertyMetadataOptions frameworkOptions = FrameworkPropertyMetadataOptions.None) where T : DependencyObject {
            int locked = 0;
            PropertyChangedCallback changedCallback = (d, e) => {
                if(locked != 0) return;
                ++locked;
                try {
                    d.SetCurrentValue(e.Property, e.OldValue);
                } finally {
                    --locked;
                }
            };
            var registeredProperty = DependencyProperty.Register(DependencyPropertyRegistrator<T>.GetPropertyName(property), typeof(TProperty), typeof(T), new FrameworkPropertyMetadata(defaultValue, frameworkOptions | FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, changedCallback));
            propertySeter = (d, v) => {
                ++locked;
                try {
                    d.SetCurrentValue(registeredProperty, v);
                } finally {
                    --locked;
                }
            };
            propertyField = registeredProperty;
            return registrator;
        }
    }
}