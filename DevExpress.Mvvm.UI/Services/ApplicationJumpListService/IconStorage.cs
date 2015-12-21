using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Cryptography;
using System.Windows.Media;
using DevExpress.Mvvm.UI.Native;
using DevExpress.Utils;
using DevExpress.Mvvm.Native;
using System.Windows;

namespace DevExpress.Mvvm.UI {
    public interface IIconStorage {
        bool TryStoreIconToFile(ImageSource icon, string storageFolder, out string iconID, out string iconPath);
    }
    public class IconStorage : IIconStorage {
        static readonly byte[] PngIconHeaderBytes = new byte[] { 0, 0, 1, 0, 1, 0, 0, 0, 0, 0, 1, 0, 24, 0, 0, 0, 0, 0, 0, 0, 0, 0 };

        Func<Uri> baseUri;

        public IconStorage(Func<Uri> baseUri) {
            this.baseUri = baseUri;
        }
        public bool TryStoreIconToFile(ImageSource icon, string storageFolder, out string iconID, out string iconPath) {
            GuardHelper.ArgumentNotNull(icon, "icon");
            byte[] iconBytes = null;
            try {
                iconBytes = ImageLoader2.ImageToByteArray(icon, baseUri);
            } catch(Exception) {
                iconID = null;
                iconPath = null;
                return false;
            }
            iconID = NativeResourceManager.CreateFileName(GetImageHash(iconBytes)) + ".ico";
            iconPath = UnpackIcon(Path.Combine(storageFolder, iconID), iconBytes);
            return iconPath != null;
        }
        static string UnpackIcon(string imagePath, byte[] imageBytes) {
            if(File.Exists(imagePath)) return imagePath;
            Directory.CreateDirectory(Path.GetDirectoryName(imagePath));
            System.Drawing.Icon icon = TryCreateIcon(imageBytes);
            if(icon != null) {
                using(icon) {
                    SaveIcon(imagePath, imageBytes, icon);
                    return imagePath;
                }
            }
            System.Drawing.Image image = TryCreateImage(imageBytes);
            if(image != null) {
                using(image) {
                    SaveImageAsIcon(imagePath, imageBytes, image);
                    return imagePath;
                }
            }
            return null;
        }
        static System.Drawing.Icon TryCreateIcon(byte[] imageBytes) {
            using(MemoryStream imageStream = new MemoryStream(imageBytes)) {
                try {
                    return new System.Drawing.Icon(imageStream);
                } catch(ArgumentException) {
                    return null;
                }
            }
        }
        static System.Drawing.Image TryCreateImage(byte[] imageBytes) {
            using(MemoryStream imageStream = new MemoryStream(imageBytes)) {
                try {
                    return System.Drawing.Image.FromStream(imageStream);
                } catch(ArgumentException) {
                    return null;
                }
            }
        }
        static void SaveIcon(string imagePath, byte[] imageBytes, System.Drawing.Icon icon) {
            WriteIconToFile(imagePath, f => f.Write(imageBytes, 0, imageBytes.Length));
        }
        static void SaveImageAsIcon(string imagePath, byte[] imageBytes, System.Drawing.Image image) {
            if(!SavePngImageAsIcon(imagePath, imageBytes, image))
                SaveBmpImageAsIcon(imagePath, imageBytes, image);
        }
        [SecuritySafeCritical]
        static void SaveBmpImageAsIcon(string imagePath, byte[] imageBytes, System.Drawing.Image image) {
            using(System.Drawing.Bitmap bitmap = new System.Drawing.Bitmap(image.Width, image.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb)) {
                using(System.Drawing.Graphics graphics = System.Drawing.Graphics.FromImage(bitmap))
                    graphics.DrawImage(image, 0, 0, image.Width, image.Height);
                IntPtr iconHandle = bitmap.GetHicon();
                try {
                    using(System.Drawing.Icon icon = System.Drawing.Icon.FromHandle(iconHandle)) {
                        WriteIconToFile(imagePath, f => icon.Save(f));
                    }
                } finally {
                    DestroyIcon(iconHandle);
                }
            }
        }
        static bool SavePngImageAsIcon(string imagePath, byte[] imageBytes, System.Drawing.Image image) {
            if(image.Width > byte.MaxValue || image.Height > byte.MaxValue || imageBytes.Length > ushort.MaxValue || !object.Equals(image.RawFormat, System.Drawing.Imaging.ImageFormat.Png)) return false;
            byte[] header = new byte[PngIconHeaderBytes.Length];
            PngIconHeaderBytes.CopyTo(header, 0);
            header[6] = (byte)image.Width;
            header[7] = (byte)image.Height;
            header[14] = (byte)(imageBytes.Length % 256);
            header[15] = (byte)(imageBytes.Length / 256);
            header[18] = (byte)header.Length;
            WriteIconToFile(imagePath, f => {
                f.Write(header, 0, header.Length);
                f.Write(imageBytes, 0, imageBytes.Length);
            });
            return true;
        }
        static void WriteIconToFile(string imagePath, Action<Stream> writeAction) {
            try {
                using(FileStream file = new FileStream(imagePath, FileMode.Create, FileAccess.Write, FileShare.Read)) {
                    writeAction(file);
                }
            } catch(IOException) { }
        }
        static string GetImageHash(byte[] image) {
            using(SHA1 sha1 = SHA1.Create()) {
                return Convert.ToBase64String(sha1.ComputeHash(image));
            }
        }
        [DllImport("user32.dll")]
        static extern bool DestroyIcon(IntPtr handle);
    }
}