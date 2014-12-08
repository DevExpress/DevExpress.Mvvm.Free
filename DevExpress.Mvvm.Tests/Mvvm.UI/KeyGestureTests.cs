#if SILVERLIGHT
using Microsoft.VisualStudio.TestTools.UnitTesting;
#elif NETFX_CORE
using DevExpress.Mvvm.UI.Native;
using TestFixture = DevExpress.TestFramework.TestClassAttribute;
using Test = DevExpress.TestFramework.TestMethodAttribute;
using DevExpress.TestFramework;
using Key = Windows.System.VirtualKey;
#else
using NUnit.Framework;
#endif
using System;
using System.Windows.Input;

namespace DevExpress.Mvvm.UI.Tests {
#if SILVERLIGHT || NETFX_CORE
    [TestFixture]
    public class ModifierKeysConverterTests {
        protected virtual ModifierKeys? ConvertStringToModifierKey(string value) {
            return ModifierKeysConverter.ConvertStringToModifierKey(value, null);
        }
        protected virtual ModifierKeys? ConvertStringToModifierKeys(string value) {
            return ModifierKeysConverter.ConvertStringToModifierKeys(value, null);
        }
        protected virtual ModifierKeys ConvertFrom(string value) {
            ModifierKeysConverter c = new ModifierKeysConverter();
            return (ModifierKeys)c.ConvertFrom(value);
        }
        protected virtual string ConvertTo(ModifierKeys value, Type type = null) {
            ModifierKeysConverter c = new ModifierKeysConverter();
            return (string)c.ConvertTo(value, type ?? typeof(string));
        }

        [Test]
        public void ConvertStringToModifierKeyTest() {
            Assert.AreEqual(ModifierKeys.Control, ConvertStringToModifierKey("CTRL"));
            Assert.AreEqual(ModifierKeys.Control, ConvertStringToModifierKey(" CTRL"));
            Assert.AreEqual(ModifierKeys.Control, ConvertStringToModifierKey("ctrl "));
            Assert.AreEqual(ModifierKeys.Control, ConvertStringToModifierKey("Control"));

            Assert.AreEqual(ModifierKeys.Alt, ConvertStringToModifierKey("ALT"));
            Assert.AreEqual(ModifierKeys.Alt, ConvertStringToModifierKey("alt  "));

            Assert.AreEqual(ModifierKeys.Shift, ConvertStringToModifierKey("   SHIFT"));
            Assert.AreEqual(ModifierKeys.Shift, ConvertStringToModifierKey("shift"));

            Assert.AreEqual(ModifierKeys.Windows, ConvertStringToModifierKey("WINDOWS"));
            Assert.AreEqual(ModifierKeys.Windows, ConvertStringToModifierKey("windows"));
            Assert.AreEqual(ModifierKeys.Windows, ConvertStringToModifierKey("WIN"));
            Assert.AreEqual(ModifierKeys.Windows, ConvertStringToModifierKey("win"));
        }
        [Test]
        public void ConvertStringToModifierKeysTest() {
            Assert.AreEqual(ModifierKeys.Control | ModifierKeys.Alt, ConvertStringToModifierKeys("CTRL+alt"));
            Assert.AreEqual(ModifierKeys.Control | ModifierKeys.Alt | ModifierKeys.Shift, ConvertStringToModifierKeys("CTRL+alt + Shift"));
            Assert.AreEqual(ModifierKeys.Control | ModifierKeys.Alt | ModifierKeys.Shift, ConvertStringToModifierKeys("CTRL+alt + Shift"));
            AssertHelper.AssertThrows<NotSupportedException>(() => {
                ConvertStringToModifierKeys("bla");
            });
        }

        [Test]
        public void ConverterTest() {
            Assert.AreEqual(ModifierKeys.Control | ModifierKeys.Alt | ModifierKeys.Windows, ConvertFrom(" CTRL +  alt + wIN"));
            Assert.AreEqual("Ctrl+Alt+Windows", ConvertTo(ModifierKeys.Control | ModifierKeys.Alt | ModifierKeys.Windows));

            AssertHelper.AssertThrows<NotSupportedException>(() => {
                ConvertFrom("as");
            });
            AssertHelper.AssertThrows<NotSupportedException>(() => {
                ConvertTo(ModifierKeys.Control, typeof(int));
            });
        }
    }
    [TestFixture]
    public class KeyGestureConverterTests : ModifierKeysConverterTests {
        protected override ModifierKeys? ConvertStringToModifierKey(string value) {
            return KeyGestureConverter.ConvertStringToKeyGesture(value, null).ModifierKeys;
        }
        protected override ModifierKeys? ConvertStringToModifierKeys(string value) {
            return KeyGestureConverter.ConvertStringToKeyGesture(value, null).ModifierKeys;
        }
        protected override ModifierKeys ConvertFrom(string value) {
            KeyGestureConverter c = new KeyGestureConverter();
            return ((KeyGesture)c.ConvertFrom(value)).ModifierKeys;
        }
        protected override string ConvertTo(ModifierKeys value, Type type = null) {
            KeyGestureConverter c = new KeyGestureConverter();
            return (string)c.ConvertTo(new KeyGesture(Key.None, value), type ?? typeof(string));
        }

        [Test]
        public void ConvertStringToKeyTest() {
            Assert.AreEqual(Key.A, KeyGestureConverter.ConvertStringToKey("A", null));
            Assert.AreEqual(Key.A, KeyGestureConverter.ConvertStringToKey("  A ", null));
            Assert.AreEqual(Key.A, KeyGestureConverter.ConvertStringToKey("  A ", null));
            Assert.AreEqual(Key.A, KeyGestureConverter.ConvertStringToKey("  a ", null));

            Assert.AreEqual(Key.F10, KeyGestureConverter.ConvertStringToKey("  f10 ", null));
#if !NETFX_CORE
            Assert.AreEqual(Key.D0, KeyGestureConverter.ConvertStringToKey("  D0 ", null));
            Assert.AreEqual(Key.D0, KeyGestureConverter.ConvertStringToKey("  d0 ", null));
#endif

            Assert.IsNull(KeyGestureConverter.ConvertStringToKey("bla", null));
        }
        [Test]
        public void ConverterTest2() {
            KeyGesture keyGesture;
            KeyGestureConverter c = new KeyGestureConverter();
            keyGesture = (KeyGesture)c.ConvertFrom(" CTRL +  alt + wIN + a");
            Assert.AreEqual(ModifierKeys.Control | ModifierKeys.Alt | ModifierKeys.Windows, keyGesture.ModifierKeys);
            Assert.AreEqual(Key.A, keyGesture.Key);

            keyGesture = (KeyGesture)c.ConvertFrom(" CTRL +  wIN + f10 + alt");
            Assert.AreEqual(ModifierKeys.Control | ModifierKeys.Alt | ModifierKeys.Windows, keyGesture.ModifierKeys);
            Assert.AreEqual(Key.F10, keyGesture.Key);

            keyGesture = (KeyGesture)c.ConvertFrom("Ctrl+Enter");
            Assert.AreEqual(ModifierKeys.Control, keyGesture.ModifierKeys);
            Assert.AreEqual(Key.Enter, keyGesture.Key);

            Assert.AreEqual("Ctrl+A", c.ConvertTo(new KeyGesture(Key.A, ModifierKeys.Control), typeof(string)));

            AssertHelper.AssertThrows<NotSupportedException>(() => {
                c.ConvertFrom(" CTRL +  wIN + f10 + alt + b");
            });
            AssertHelper.AssertThrows<NotSupportedException>(() => {
                c.ConvertTo(new KeyGesture(Key.A, ModifierKeys.Control), typeof(int));
            });
        }
    }
#endif
}