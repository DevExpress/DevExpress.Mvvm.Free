using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using DevExpress.Mvvm.UI.Native;
using DevExpress.Utils;
using NUnit.Framework;
using DevExpress.Mvvm.Native;

namespace DevExpress.Mvvm.UI.Tests {
    [TestFixture]
    public class ImageLoader2Tests {
        [SetUp]
        public void SetUp() {
            ApplicationJumpListServiceTestsImageSourceHelper.RegisterPackScheme();
        }
        [Test]
        public void LoadLocalImages2() {
            TestUri(ImageFormat.Png,
                AssemblyHelper.GetResourceUri(typeof(ImageLoader2Tests).Assembly, "Services/ApplicationJumpListService/Code_Central.png")
            );
        }
        [Test]
        public void LoadLocalImages3() {
            TestUri(ImageFormat.Icon,
                AssemblyHelper.GetResourceUri(typeof(ImageLoader2Tests).Assembly, "Services/ApplicationJumpListService/demoicon.ico")
            );
        }
        [Test, Explicit]
        public void LoadWebImage() {
            TestUri(ImageFormat.Png, new Uri("https://html5box.com/images/image.png"));
        }
        void TestUri(ImageFormat format, params Uri[] uris) {
            List<ImageSource> sources = new List<ImageSource>();
            foreach(Uri uri in uris) {
                sources.Add(new BitmapImage(uri));
                sources.Add(ApplicationJumpListServiceTestsImageSourceHelper.GetImageSource(uri));
            }
            foreach(ImageSource source in sources) {
                byte[] data = ImageLoader2.ImageToByteArray(source);
                Assert.IsNotNull(data);
                Assert.AreNotEqual(0, data.Length);
                Bitmap bitmap = new Bitmap(new MemoryStream(data));
                Assert.AreEqual(format, bitmap.RawFormat);
            }
        }
        DrawingImage DrawingImage {
            get {
                var pen = new System.Windows.Media.Pen() { Thickness = 3, Brush = System.Windows.Media.Brushes.Green };
                var geometry = new GeometryDrawing() { Geometry = Geometry.Parse("M 105,195 C 105,25 355,350 400,170 H 270"), Pen = pen };
                var drawingGroup = new DrawingGroup();
                drawingGroup.Children.Add(geometry);
                var image = new DrawingImage();
                image.Drawing = drawingGroup;
                return image;
            }
        }
        RenderTargetBitmap RenderTargetBitmap {
            get {
                var pen = new System.Windows.Media.Pen() { Thickness = 3, Brush = System.Windows.Media.Brushes.Green };
                var image = new RenderTargetBitmap(100, 100, 96, 96, System.Windows.Media.PixelFormats.Pbgra32);
                var drawingVisual = new DrawingVisual();
                using(var drawingContext = drawingVisual.RenderOpen())
                    drawingContext.DrawRectangle(System.Windows.Media.Brushes.Red, pen, new System.Windows.Rect(new System.Windows.Point(30, 30), new System.Windows.Size(30, 20)));
                image.Render(drawingVisual);
                return image;
            }
        }
        System.Windows.Interop.InteropBitmap InteropBitmap {
            get {
                var icon = new Icon(AssemblyHelper.GetResourceStream(typeof(ImageLoader2Tests).Assembly, "Services/ApplicationJumpListService/demoicon.ico", true));
                return (System.Windows.Interop.InteropBitmap)System.Windows.Interop.Imaging.CreateBitmapSourceFromHIcon(icon.Handle, System.Windows.Int32Rect.Empty, BitmapSizeOptions.FromWidthAndHeight(icon.Width, icon.Height));
            }
        }

        [Test]
        public void LoadDrawingImage() {
            var data = ImageLoader2.ImageToByteArray(DrawingImage);
            Assert.IsNotNull(data);
            Assert.AreEqual(3682, data.Length);
            Bitmap bitmap = new Bitmap(new MemoryStream(data));
            Assert.AreEqual(ImageFormat.Png, bitmap.RawFormat);
            Assert.AreEqual(System.Drawing.Imaging.PixelFormat.Format32bppArgb, bitmap.PixelFormat);
            Assert.AreEqual(299, bitmap.Width);
            Assert.AreEqual(86, bitmap.Height);
        }
        [Test]
        public void LoadInteropBitmap() {
            var data = ImageLoader2.ImageToByteArray(InteropBitmap);
            Assert.IsNotNull(data);
            Assert.AreEqual(1164, data.Length);
            Bitmap bitmap = new Bitmap(new MemoryStream(data));
            Assert.AreEqual(ImageFormat.Png, bitmap.RawFormat);
            Assert.AreEqual(System.Drawing.Imaging.PixelFormat.Format32bppArgb, bitmap.PixelFormat);
            Assert.AreEqual(32, bitmap.Width);
            Assert.AreEqual(32, bitmap.Height);
        }
        [Test]
        public void LoadGenericBitmapSource() {
            var data = ImageLoader2.ImageToByteArray(RenderTargetBitmap);
            Assert.IsNotNull(data);
            Assert.AreEqual(304, data.Length);
            Bitmap bitmap = new Bitmap(new MemoryStream(data));
            Assert.AreEqual(ImageFormat.Png, bitmap.RawFormat);
            Assert.AreEqual(System.Drawing.Imaging.PixelFormat.Format32bppArgb, bitmap.PixelFormat);
            Assert.AreEqual(100, bitmap.Width);
            Assert.AreEqual(100, bitmap.Height);
        }
        [Test]
        public void LoadDrawingImage_Resize() {
            var data = ImageLoader2.ImageToByteArray(DrawingImage, drawingImageSize: new System.Windows.Size(150, 45));
            Assert.IsNotNull(data);
            Assert.AreEqual(1821, data.Length);
            Bitmap bitmap = new Bitmap(new MemoryStream(data));
            Assert.AreEqual(ImageFormat.Png, bitmap.RawFormat);
            Assert.AreEqual(System.Drawing.Imaging.PixelFormat.Format32bppArgb, bitmap.PixelFormat);
            Assert.AreEqual(150, bitmap.Width);
            Assert.AreEqual(45, bitmap.Height);
        }
    }
}