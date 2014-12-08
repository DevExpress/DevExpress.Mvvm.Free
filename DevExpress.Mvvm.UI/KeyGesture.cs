using System;
using System.ComponentModel;
using System.Globalization;
using System.Windows.Input;
#if NETFX_CORE
using DevExpress.Mvvm.UI.Native;
using DevExpress.Mvvm.Native;
using Windows.System;
using Key = Windows.System.VirtualKey;

#endif

namespace DevExpress.Mvvm.UI {
#if NETFX_CORE && !MVVM
    [DevExpress.Data.Native.TypeConverter(typeof(KeyGestureConverter))]
#else
    [TypeConverter(typeof(KeyGestureConverter))]
#endif
    public class KeyGesture {
        public Key Key { get; private set; }
        public ModifierKeys ModifierKeys { get; private set; }
        public KeyGesture(Key key)
            : this(key, ModifierKeys.None) {
        }
        public KeyGesture(Key key, ModifierKeys modifiers) {
            Key = key;
            ModifierKeys = modifiers;
        }
#if NETFX_CORE
        public static implicit operator KeyGesture(string value) {
            return ModifierKeysConverter.ConvertStringToKeyGesture(value, System.Globalization.CultureInfo.CurrentCulture);
        }
#endif
    }

    public class ModifierKeysConverter : TypeConverter {
        public const char Delimiter = '+';

        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType) {
            return sourceType == typeof(string);
        }
        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object source) {
            return ConvertStringToModifierKeys((string)source, culture);
        }
        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType) {
            if(destinationType != typeof(string)) return false;
#if !NETFX_CORE
            return context != null && context.Instance is ModifierKeys;
#else
            return true;
#endif

        }
        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType) {
            if(destinationType != typeof(string))
                throw new NotSupportedException("Cannot convert ModifierKeys to the defined type");
            ModifierKeys modifiers = (ModifierKeys)value;
            string res = string.Empty;
            if(IsFlagSet(modifiers, ModifierKeys.Control))
                res += "Ctrl";
            if(IsFlagSet(modifiers, ModifierKeys.Alt))
                res += Delimiter + "Alt";
            if(IsFlagSet(modifiers, ModifierKeys.Shift))
                res += Delimiter + "Shift";
            if(IsFlagSet(modifiers, ModifierKeys.Windows))
                res += Delimiter + "Windows";
            return res;
        }

        protected static internal ModifierKeys ConvertStringToModifierKeys(string modifiersString, CultureInfo culture) {
            ModifierKeys modifiers = ModifierKeys.None;
            foreach(var current in modifiersString.Split(Delimiter)) {
                var currentModifier = ConvertStringToModifierKey(current, culture);
                if(currentModifier == null)
                    throw new NotSupportedException("Not Supported ModifierKeys value");
                modifiers |= currentModifier.Value;
            }
            return modifiers;
        }
        protected static internal KeyGesture ConvertStringToKeyGesture(string keyGesture, CultureInfo culture) {
            ModifierKeys modifiers = ModifierKeys.None;
            Key key = Key.None;
            foreach(var current in keyGesture.Split(Delimiter)) {
                var currentModifier = ConvertStringToModifierKey(current, culture);
                if(currentModifier != null) {
                    modifiers |= currentModifier.Value;
                    continue;
                }
                var currentKey = ConvertStringToKey(current, culture);
                if(currentKey == null || key != Key.None)
                    throw new NotSupportedException("Cannot create KeyGesture from this string");
                key = currentKey.Value;
            }
            return new KeyGesture(key, modifiers);
        }
        protected static internal Key? ConvertStringToKey(string keyString, CultureInfo culture) {
            Key res = Key.None;
            if(Enum.TryParse<Key>(keyString, true, out res))
                return res;
            return null;
        }
        protected static internal ModifierKeys? ConvertStringToModifierKey(string modifierString, CultureInfo culture) {
            modifierString = modifierString.Trim();
#if !NETFX_CORE
            modifierString = culture != null ? modifierString.ToUpper(culture) : modifierString.ToUpper();
#else
            modifierString = modifierString.ToUpper();
#endif
            switch(modifierString) {
                case "CONTROL":
                case "CTRL":
                    return ModifierKeys.Control;
                case "SHIFT":
                    return ModifierKeys.Shift;
                case "ALT":
                    return ModifierKeys.Alt;
                case "WINDOWS":
                case "WIN":
                    return ModifierKeys.Windows;
                case "NONE":
                    return ModifierKeys.None;
                default:
                    return null;
            }
        }
        protected static internal bool IsFlagSet(ModifierKeys value, ModifierKeys flag) {
            return (value & flag) == flag;
        }
    }
    public class KeyGestureConverter : ModifierKeysConverter {
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType) {
            return sourceType == typeof(string);
        }
        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object source) {
            return ConvertStringToKeyGesture((string)source, culture);
        }
        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType) {
            if(destinationType != typeof(string)) return false;
#if !NETFX_CORE
            return context != null && context.Instance is KeyGesture;
#else
            return true;
#endif
        }
        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType) {
            if(destinationType != typeof(string))
                throw new NotSupportedException("Cannot convert ModifierKeys to the defined type");
            KeyGesture keyGesture = (KeyGesture)value;
            string res = string.Empty;
            if(IsFlagSet(keyGesture.ModifierKeys, ModifierKeys.Control))
                res += "Ctrl";
            if(IsFlagSet(keyGesture.ModifierKeys, ModifierKeys.Alt))
                res += Delimiter + "Alt";
            if(IsFlagSet(keyGesture.ModifierKeys, ModifierKeys.Shift))
                res += Delimiter + "Shift";
            if(IsFlagSet(keyGesture.ModifierKeys, ModifierKeys.Windows))
                res += Delimiter + "Windows";
            if(keyGesture.Key != Key.None)
                res += Delimiter + keyGesture.Key.ToString();
            return res;
        }

    }
}