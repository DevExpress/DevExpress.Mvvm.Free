using System;
using System.Windows.Input;
using System.Windows;
using System.Globalization;
using System.ComponentModel;
using Windows.System;
using Windows.UI.Core;
using Windows.UI.Xaml;

namespace DevExpress.Mvvm.UI.Native {
    [Flags]
    public enum ModifierKeys {
        None = 0,
        Alt = 1,
        Control = 2,
        Shift = 4,
        Windows = 8,
        Apple = 8,
    }
    public class SkipPropertyAssertionAttribute : Attribute {
        public SkipPropertyAssertionAttribute() {
        }
    }
    public static class ModifierKeysHelper {
        public static bool IsShiftPressed() {
            return IsShiftPressed(GetKeyboardModifiers());
        }
        public static bool IsControlPressed() {
            return IsCtrlPressed(GetKeyboardModifiers());
        }

        public static bool IsKeyPressed(VirtualKey key) {
            CoreVirtualKeyStates controlKeyState = Window.Current.CoreWindow.GetKeyState(key);
            return(controlKeyState & CoreVirtualKeyStates.Down) == CoreVirtualKeyStates.Down;
        }
        public static ModifierKeys GetKeyboardModifiers() {
            ModifierKeys mc = ModifierKeys.None;
            mc |= IsKeyPressed(VirtualKey.Shift) ? ModifierKeys.Shift : ModifierKeys.None;
            mc |= IsKeyPressed(VirtualKey.LeftControl) || IsKeyPressed(VirtualKey.RightControl) ? ModifierKeys.Control : ModifierKeys.None;
            mc |= IsKeyPressed(VirtualKey.LeftWindows) || IsKeyPressed(VirtualKey.RightWindows) ? ModifierKeys.Windows : ModifierKeys.None;
            mc |= IsKeyPressed(VirtualKey.LeftMenu) || IsKeyPressed(VirtualKey.RightMenu) ? ModifierKeys.Alt : ModifierKeys.None;
            return mc;
        }
        public static bool IsAltPressed(ModifierKeys modifiers) {
            return (modifiers & ModifierKeys.Alt) != ModifierKeys.None;
        }

        public static bool IsShiftPressed(ModifierKeys modifiers) {
            return (modifiers & ModifierKeys.Shift) != ModifierKeys.None;
        }

        public static bool IsCtrlPressed(ModifierKeys modifiers) {
            return (modifiers & (ModifierKeys.Control | ModifierKeys.Apple)) != ModifierKeys.None;
        }

        public static bool IsWinPressed(ModifierKeys modifiers) {
            return (modifiers & ModifierKeys.Windows) != ModifierKeys.None;
        }
        public static bool NoModifiers(ModifierKeys modifiers) {
            return modifiers == ModifierKeys.None;
        }
    }
    public class Counter {
        int innerCounter;
        public void Increment() {
            innerCounter++;
        }
        public int Value { get { return innerCounter; } }
        public void Reset() {
            innerCounter = 0;
        }
        public Counter() {
            Reset();
        }
        public bool IsClear {
            get { return innerCounter == 0; }
        }
    }
}