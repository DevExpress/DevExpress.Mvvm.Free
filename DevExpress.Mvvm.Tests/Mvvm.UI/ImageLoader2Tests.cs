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
            TestUri(
                AssemblyHelper.GetResourceUri(typeof(ImageLoader2Tests).Assembly, "Services/ApplicationJumpListService/Code_Central.png")
            );
        }
        [Test, Explicit]
        public void LoadWebImage() {
            TestUri(new Uri("https://html5box.com/images/image.png"));
        }
        void TestUri(params Uri[] uris) {
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
                Assert.AreEqual(ImageFormat.Png, bitmap.RawFormat);
            }
        }
        DrawingImage Image {
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
        [Test]
        public void LoadDrawingImage() {
            var data = ImageLoader2.ImageToByteArray(Image);
            Assert.IsNotNull(data);
            Assert.AreEqual(3682, data.Length);
            Bitmap bitmap = new Bitmap(new MemoryStream(data));
            Assert.AreEqual(ImageFormat.Png, bitmap.RawFormat);
            Assert.AreEqual(System.Drawing.Imaging.PixelFormat.Format32bppArgb, bitmap.PixelFormat);
            Assert.AreEqual(299, bitmap.Width);
            Assert.AreEqual(86, bitmap.Height);
        }
        [Test]
        public void LoadDrawingImage_Resize() {
            var data = ImageLoader2.ImageToByteArray(Image, drawingImageSize: new System.Windows.Size(150, 45));
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