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

        public static byte[] ImageToByteArray(ImageSource source, Func<Uri> baseUriProvider = null) {
            if(source == null) throw new ArgumentNullException("source");
            Stream stream;
            Uri uri;
            TryGetStreamAndUri(source, out uri, out stream);
            byte[] streamData;
            if(stream == null) {
                streamData = null;
            } else {
                try {
                    streamData = StreamHelper.CopyAllBytes(stream);
                } catch(ObjectDisposedException) {
                    streamData = null;
                }
            }
            if(streamData != null) {
                if(streamData.Length == 0)
                    throw new ArgumentException("EndOfStream", "source");
                return streamData;
            }
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
        static void TryGetStreamAndUri(ImageSource imageSource, out Uri uri, out Stream stream) {
            BitmapImage bitmapImage = imageSource as BitmapImage;
            if(bitmapImage != null) {
                TryGetStreamAndUri(bitmapImage, out uri, out stream);
                return;
            }
            BitmapFrame bitmapFrame = imageSource as BitmapFrame;
            if(bitmapFrame != null) {
                TryGetStreamAndUri(bitmapFrame, out uri, out stream);
                return;
            }
            uri = null;
            stream = null;
        }
        static void TryGetStreamAndUri(BitmapImage bitmapImage, out Uri uri, out Stream stream) {
            var bitmapStream = bitmapImage.StreamSource;
            if(bitmapStream != null && bitmapStream.CanRead)
                stream = bitmapStream;
            else
                stream = null;
            uri = bitmapImage.UriSource;
        }
        static void TryGetStreamAndUri(BitmapFrame bitmapFrame, out Uri uri, out Stream stream) {
            BitmapDecoder decoder = bitmapFrame.Decoder;
            FieldInfo streamField = GetDecoderField(decoder, "_stream");
            FieldInfo uriStreamField = GetDecoderField(decoder, "_uriStream");
            FieldInfo uriField = GetDecoderField(decoder, "_uri");
            var decoderStream = (Stream)streamField.GetValue(decoder);
            if(decoderStream == null || !decoderStream.CanRead)
                decoderStream = (Stream)uriStreamField.GetValue(decoder);
            if(decoderStream != null && decoderStream.CanRead)
                stream = decoderStream;
            else
                stream = null;
            var decoderUri = (Uri)uriField.GetValue(decoder);
            if(decoderUri != null && bitmapFrame.BaseUri != null)
                decoderUri = new Uri(bitmapFrame.BaseUri, decoderUri);
            uri = decoderUri;
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