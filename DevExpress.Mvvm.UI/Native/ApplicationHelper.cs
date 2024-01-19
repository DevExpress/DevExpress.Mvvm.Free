using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Resources;
using System.Xml.Linq;
using System.Windows.Interop;

namespace DevExpress.Mvvm.UI.Native {
    public static class StreamHelper {
        public static string ToStringWithDispose(this Stream stream) {
            using(stream) {
                using(StreamReader reader = new StreamReader(stream)) {
                    return reader.ReadToEnd();
                }
            }
        }
        public static byte[] CopyAllBytes(this Stream stream) {
            if (!stream.CanRead)
                return null;
            if (stream.CanSeek)
                stream.Seek(0L, SeekOrigin.Begin);
            List<byte> list = new List<byte>();
            byte[] buffer = new byte[1024];
            int num;
            while ((num = stream.Read(buffer, 0, 1024)) > 0) {
                for (int index = 0; index < num; ++index)
                    list.Add(buffer[index]);
            }
            return list.ToArray();
        }
    }
}