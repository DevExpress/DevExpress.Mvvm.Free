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
        [Test, Category("TODO")]
        public void LoadWebImage() {
            TestUri(new Uri("http://ph.blackberry.com/content/dam/blackBerry/images/icon/icon-blackBerryTravel.png.original.png"));
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
    }
}