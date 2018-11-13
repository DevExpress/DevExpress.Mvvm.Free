using System;
using System.Diagnostics;
using System.IO;
using Microsoft.Win32;
using DevExpress.Internal.WinApi;

namespace DevExpress.Data {
    public static partial class ShellHelper {
        public static void TryCreateShortcut(string applicationId, string name, string iconPath = null) {
            string shortcutPath = GetShortcutPath(name);
            if(!File.Exists(shortcutPath))
                InstallShortcut(shortcutPath, applicationId, iconPath);
        }
        public static void TryCreateShortcut(string exePath, string applicationId, string name, string iconPath = null) {
            string shortcutPath = GetShortcutPath(name);
            if(!File.Exists(shortcutPath))
                InstallShortcut(exePath, shortcutPath, applicationId, iconPath);
        }
        public static void TryCreateShortcut(string applicationId, string name, string iconPath, Type activatorType) {
            TryCreateShortcut(null, applicationId, iconPath, activatorType);
        }
        public static void TryCreateShortcut(string exePath, string applicationId, string name, string iconPath, Type activatorType) {
            exePath = PatchExePath(exePath);
            string shortcutPath = GetShortcutPath(name);
            if(!File.Exists(shortcutPath))
                InstallShortcut(exePath, shortcutPath, applicationId, iconPath, activatorType);
        }
        public static void TryRemoveShortcut(string name) {
            string shortcutPath = GetShortcutPath(name);
            if(File.Exists(shortcutPath))
                File.Delete(shortcutPath);
        }
        public static bool IsApplicationShortcutExist(string name) {
            string shortcutPath = GetShortcutPath(name);
            return File.Exists(shortcutPath);
        }
        static string GetShortcutPath(string name) {
            return Path.Combine(GetProgramsFolder(), name + ".lnk");
        }
        static void InstallShortcut(string shortcutPath, string applicationId, string iconPath) {
            InstallShortcut(GetSourcePath(), shortcutPath, applicationId, iconPath);
        }
        static void InstallShortcut(string exePath, string shortcutPath, string applicationId, string iconPath) {
            InstallShortcut(exePath, shortcutPath, applicationId, iconPath, null);
        }
        static void InstallShortcut(string exePath, string shortcutPath, string applicationId, string iconPath, Type activatorType) {
            IShellLinkW newShortcut = (IShellLinkW)new CShellLink();
            SetExePath(exePath, newShortcut);
            SetArguments(newShortcut);
            SetIconLocation(iconPath, newShortcut);
            IPropertyStore newShortcutProperties = (IPropertyStore)newShortcut;
            SetAppId(applicationId, newShortcutProperties);
            SetActivatorId(activatorType, newShortcutProperties);
            SaveShortcut(shortcutPath, newShortcut);
        }
        static void SaveShortcut(string shortcutPath, IShellLinkW newShortcut) {
            IPersistFile newShortcutSave = (IPersistFile)newShortcut;
            ErrorHelper.VerifySucceeded(newShortcutSave.Save(shortcutPath, true));
        }
        static void SetArguments(IShellLinkW newShortcut) {
            ErrorHelper.VerifySucceeded(newShortcut.SetArguments(""));
        }
        static void SetExePath(string exePath, IShellLinkW newShortcut) {
            ErrorHelper.VerifySucceeded(newShortcut.SetPath(exePath));
        }
        static void SetIconLocation(string iconPath, IShellLinkW newShortcut) {
            if(iconPath != null)
                newShortcut.SetIconLocation(iconPath, 0);
        }
        static void SetActivatorId(Type activatorType, IPropertyStore newShortcutProperties) {
            if(activatorType == null) return;
            using(PropVariant activatorID = new PropVariant(activatorType.GUID)) {
                PropertyKey toastActivatorCLS_ID = new PropertyKey(new Guid("{9F4C2855-9F79-4B39-A8D0-E1D42DE1D5F3}"), 26);
                SetShortcutPropertiesValue(newShortcutProperties, toastActivatorCLS_ID, activatorID);
            }
        }
        static void SetAppId(string applicationId, IPropertyStore newShortcutProperties) {
            using(PropVariant appId = new PropVariant(applicationId)) {
                PropertyKey appUserModel_ID = new PropertyKey(new Guid("{9F4C2855-9F79-4B39-A8D0-E1D42DE1D5F3}"), 5);
                SetShortcutPropertiesValue(newShortcutProperties, appUserModel_ID, appId);
            }
        }
        static void SetShortcutPropertiesValue(IPropertyStore newShortcutProperties, PropertyKey key, PropVariant pv) {
            ErrorHelper.VerifySucceeded(newShortcutProperties.SetValue(key, pv));
            CommitShortcutProperties(newShortcutProperties);
        }
        static void CommitShortcutProperties(IPropertyStore newShortcutProperties) {
            ErrorHelper.VerifySucceeded(newShortcutProperties.Commit());
        }
        static string GetProgramsFolder() {
            return Environment.GetFolderPath(Environment.SpecialFolder.Programs);
        }
        static string GetSourcePath() {
            return Process.GetCurrentProcess().MainModule.FileName;
        }
        static string PatchExePath(string exePath) {
            if(string.IsNullOrEmpty(exePath))
                exePath = GetSourcePath();
            return exePath;
        }
        static string GetRegistryKeyName(Type activatorType) {
            return string.Format("SOFTWARE\\Classes\\CLSID\\{{{0}}}\\LocalServer32", activatorType.GUID);
        }
        public static void UnregisterComServer(Type activatorType) {
            string keyName = GetRegistryKeyName(activatorType);
            using(var key = Registry.CurrentUser.OpenSubKey(keyName)) {
                if(key == null)
                    return;
            }
            Registry.CurrentUser.DeleteSubKeyTree(keyName);
        }
    }
}