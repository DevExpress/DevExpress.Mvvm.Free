using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Packaging;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;

namespace DevExpress.Mvvm.UI.Native {
    public static class ImageLoader2 {
        static Dictionary<Uri, WeakReference> cache = new Dictionary<Uri, WeakReference>();
        const int MaxCacheSize = 320;
        static Type wpfWebRequestHelper;
        static MethodInfo createRequestAndGetResponseStreamMethod;
        static Type securityHelper;
        static MethodInfo mapUrlToZoneWrapperMethod;

        public static byte[] ImageToByteArray(ImageSource source, Func<Uri> baseUriProvider = null, Size? drawingImageSize = null) {
            if(source == null) throw new ArgumentNullException("source");
            var streamData = TryGetStreamData(source, drawingImageSize);
            if(streamData != null) {
                if(streamData.Length == 0)
                    throw new ArgumentException("EndOfStream", "source");
                return streamData;
            }
            var uri = TryGetUri(source);
            if(uri != null) {
                Uri baseUri = baseUriProvider == null ? null : baseUriProvider();
                byte[] array = ImageToByteArray(baseUri == null ? uri : new Uri(baseUri, uri));
                if(array == null)
                    throw new ArgumentException("Uri:Stream.CanRead", "source");
                if(array.Length == 0)
                    throw new ArgumentException("Uri:EndOfStream", "source");
                return array;
            }
            throw new ArgumentException("ImageSource", "source");
        }
        public static string TryGetImageUriOriginalString(ImageSource source) {
            if(source == null) throw new ArgumentNullException("source");
            var uri = TryGetUri(source);
            return uri == null ? null : uri.OriginalString;
        }
        static byte[] ImageToByteArray(Uri uri) {
            if(uri == null) throw new ArgumentNullException("uri");
            byte[] data = CheckCache(uri);
            if(data != null) return data;
            Stream stream = null;
            if(uri.IsAbsoluteUri && string.Equals(uri.Scheme, PackUriHelper.UriSchemePack, StringComparison.OrdinalIgnoreCase))
                stream = CreateRequestAndGetResponseStream(uri);
            if(stream == null) {
                if(!uri.IsAbsoluteUri)
                    stream = new FileStream(uri.OriginalString, FileMode.Open, FileAccess.Read, FileShare.Read);
                else if(uri.IsFile && (uri.IsUnc || MapUrlToZoneWrapper(uri) == 0))
                    stream = new FileStream(uri.LocalPath, FileMode.Open, FileAccess.Read, FileShare.Read);
                else
                    stream = CreateRequestAndGetResponseStream(uri);
            }
            if(stream == null) return null;
            data = StreamHelper.CopyAllBytes(stream);
            AddToCache(uri, data);
            return data;
        }
        static byte[] TryCopyAllBytes(Stream stream) {
            try {
                return StreamHelper.CopyAllBytes(stream);
            } catch(ObjectDisposedException) {
                return null;
            }
        }
        static Uri TryGetUri(ImageSource imageSource) {
            return TryGetImageData(imageSource, TryGetUri, TryGetUri, _ => null, _ => null, () => null);
        }
        static byte[] TryGetStreamData(ImageSource imageSource, Size? drawingImageSize) {
            return TryGetImageData(imageSource, TryGetStreamData, TryGetStreamData, x => TryGetStreamData(x, drawingImageSize), SaveAsPng, () => null);
        }
        static T TryGetImageData<T>(ImageSource imageSource, Func<BitmapImage, T> fromBitmapImage, Func<BitmapFrame, T> fromBitmapFrame, Func<DrawingImage, T> fromDrawingImage, Func<BitmapSource, T> fromGenericBitmapSource, Func<T> fallback) {
            var bitmapImage = imageSource as BitmapImage;
            if(bitmapImage != null)
                return fromBitmapImage(bitmapImage);
            var bitmapFrame = imageSource as BitmapFrame;
            if(bitmapFrame != null)
                return fromBitmapFrame(bitmapFrame);
            var drawingImage = imageSource as DrawingImage;
            if(drawingImage != null)
                return fromDrawingImage(drawingImage);
            var genericBitmapSource = imageSource as BitmapSource;
            if(genericBitmapSource != null)
                return fromGenericBitmapSource(genericBitmapSource);
            return fallback();
        }
        static byte[] TryGetStreamData(DrawingImage drawingImage, Size? drawingImageSize) {
            var width = drawingImageSize == null ? drawingImage.Width : drawingImageSize.Value.Width;
            var height = drawingImageSize == null ? drawingImage.Height : drawingImageSize.Value.Height;
            var renderBitmap = new RenderTargetBitmap((int)Math.Ceiling(width), (int)Math.Ceiling(height), 96, 96, PixelFormats.Pbgra32);
            var drawingVisual = new DrawingVisual();
            using(var drawingContext = drawingVisual.RenderOpen())
                drawingContext.DrawImage(drawingImage, new Rect(new Point(), new Size(width, height)));
            renderBitmap.Render(drawingVisual);
            return SaveAsPng(renderBitmap);
        }
        static byte[] SaveAsPng(BitmapSource bitmapSource) {
            var encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(bitmapSource));
            using(var stream = new MemoryStream()) {
                encoder.Save(stream);
                return stream.ToArray();
            }
        }
        static byte[] TryGetStreamData(BitmapImage bitmapImage) {
            var bitmapStream = bitmapImage.StreamSource;
            if(bitmapStream != null && bitmapStream.CanRead)
                return TryCopyAllBytes(bitmapStream);
            return null;
        }
        static Uri TryGetUri(BitmapImage bitmapImage) {
            return bitmapImage.UriSource;
        }
        static byte[] TryGetStreamData(BitmapFrame bitmapFrame) {
            BitmapDecoder decoder = bitmapFrame.Decoder;
            FieldInfo streamField = GetDecoderField(decoder, "_stream");
            FieldInfo uriStreamField = GetDecoderField(decoder, "_uriStream");
            FieldInfo uriField = GetDecoderField(decoder, "_uri");
            var decoderStream = (Stream)streamField.GetValue(decoder);
            if(decoderStream == null || !decoderStream.CanRead)
                decoderStream = (Stream)uriStreamField.GetValue(decoder);
            if(decoderStream != null && decoderStream.CanRead)
                return TryCopyAllBytes(decoderStream);
            return null;
        }
        static Uri TryGetUri(BitmapFrame bitmapFrame) {
            BitmapDecoder decoder = bitmapFrame.Decoder;
            FieldInfo uriField = GetDecoderField(decoder, "_uri");
            var decoderUri = (Uri)uriField.GetValue(decoder);
            if(decoderUri != null && bitmapFrame.BaseUri != null)
                decoderUri = new Uri(bitmapFrame.BaseUri, decoderUri);
            return decoderUri;
        }
        static FieldInfo GetDecoderField(BitmapDecoder decoder, string fieldName) {
            FieldInfo streamField = decoder.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
            if(streamField == null)
                throw new Exception(string.Format("{0}.{1} ({2})", decoder.GetType().FullName, fieldName, decoder.GetType().AssemblyQualifiedName));
            return streamField;
        }

        static Stream CreateRequestAndGetResponseStream(Uri uri) {
            try {
                return (Stream)CreateRequestAndGetResponseStreamMethod.Invoke(null, new object[] { uri });
            } catch(Exception e) {
                throw new InvalidOperationException("", e);
            }
        }
        static int MapUrlToZoneWrapper(Uri url) {
            try {
                return (int)MapUrlToZoneWrapperMethod.Invoke(null, new object[] { url });
            } catch(Exception e) {
                throw new InvalidOperationException("", e);
            }
        }
        static MethodInfo CreateRequestAndGetResponseStreamMethod {
            get {
                if(createRequestAndGetResponseStreamMethod == null)
                    createRequestAndGetResponseStreamMethod = WpfWebRequestHelper.GetMethod("CreateRequestAndGetResponseStream", BindingFlags.Static | BindingFlags.NonPublic, null, new Type[] { typeof(Uri) }, null);
                return createRequestAndGetResponseStreamMethod;
            }
        }
        static Type WpfWebRequestHelper {
            get {
                if(wpfWebRequestHelper == null)
                    wpfWebRequestHelper = typeof(ImageSource).Assembly.GetType("MS.Internal.WpfWebRequestHelper");
                return wpfWebRequestHelper;
            }
        }
        static MethodInfo MapUrlToZoneWrapperMethod {
            get {
                if(mapUrlToZoneWrapperMethod == null)
                    mapUrlToZoneWrapperMethod = SecurityHelper.GetMethod("MapUrlToZoneWrapper", BindingFlags.Static | BindingFlags.NonPublic, null, new Type[] { typeof(Uri) }, null);
                return mapUrlToZoneWrapperMethod;
            }
        }
        static Type SecurityHelper {
            get {
                if(securityHelper == null)
                    securityHelper = typeof(ImageSource).Assembly.GetType("MS.Internal.SecurityHelper");
                return securityHelper;
            }
        }

        static void AddToCache(Uri uri, byte[] data) {
            lock(cache) {
                if(cache.ContainsKey(uri)) return;
                if(cache.Count == MaxCacheSize) {
                    foreach(Uri current in cache.Where(r => r.Value != null && r.Value.Target == null).Select(r => r.Key).ToList())
                        cache.Remove(current);
                }
                if(cache.Count != MaxCacheSize)
                    cache[uri] = new WeakReference(data);
            }
        }
        static void RemoveFromCache(Uri uri) {
            lock(cache) {
                if(cache.ContainsKey(uri))
                    cache.Remove(uri);
            }
        }
        static byte[] CheckCache(Uri uri) {
            lock(cache) {
                WeakReference reference;
                return cache.TryGetValue(uri, out reference) ? (byte[])reference.Target : null;
            }
        }
    }
}