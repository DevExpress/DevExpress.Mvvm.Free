using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using DevExpress.Internal.WinApi;
using DevExpress.Internal.WinApi.Window.Data.Xml.Dom;
using DevExpress.Internal.WinApi.Windows.UI.Notifications;

namespace DevExpress.Internal {
    class WinRTToastNotificationContent : IPredefinedToastNotificationContent {
        string[] lines = new string[3];
        ToastTemplateType type;
        string imagePath;
        PredefinedSound sound;
        NotificationDuration duration = NotificationDuration.Default;
        internal string tempFilePath;
        WinRTToastNotificationContent() { }
        public void Dispose() {
            RemoveTempFile();
        }
        public static WinRTToastNotificationContent Create(string bodyText) {
            return Create(ToastTemplateType.ToastText01, bodyText);
        }
        public static WinRTToastNotificationContent CreateOneLineHeader(string headlineText, string bodyText) {
            return Create(ToastTemplateType.ToastText02, headlineText, bodyText);
        }
        public static WinRTToastNotificationContent CreateTwoLineHeader(string headlineText, string bodyText) {
            return Create(ToastTemplateType.ToastText03, headlineText, bodyText);
        }
        public static WinRTToastNotificationContent CreateOneLineHeader(string headlineText, string bodyText1, string bodyText2) {
            return Create(ToastTemplateType.ToastText04, headlineText, bodyText1, bodyText2);
        }
        static WinRTToastNotificationContent Create(ToastTemplateType type, params string[] lines) {
            return new WinRTToastNotificationContent { type = type, lines = lines };
        }
        static void SetNodeValueString(string str, IXmlDocument xmldoc, IXmlNode node) {
            IXmlText textNode;
            int res = xmldoc.CreateTextNode(str, out textNode);
            ComFunctions.CheckHRESULT(res);

            IXmlNode textNodeAsNode = (IXmlNode)textNode;
            IXmlNode appendedChild;
            res = node.AppendChild(textNodeAsNode, out appendedChild);
            ComFunctions.CheckHRESULT(res);
        }
        static void SetImageSrc(IXmlDocument xmldoc, string imagePath) {
            IXmlNodeList nodes;
            int res = xmldoc.GetElementsByTagName("image", out nodes);
            ComFunctions.CheckHRESULT(res);

            IXmlNode imageNode;
            res = nodes.Item(0, out imageNode);
            ComFunctions.CheckHRESULT(res);

            IXmlNode srcAttribute;
            res = imageNode.Attributes.GetNamedItem("src", out srcAttribute);
            ComFunctions.CheckHRESULT(res);

            SetNodeValueString(imagePath, xmldoc, srcAttribute);
        }
        public void SetSound(PredefinedSound sound) {
            this.sound = sound;
        }
        public void SetDuration(NotificationDuration duration) {
            this.duration = duration;
        }
        public void SetImage(string imagePath) {
            if(type == ToastTemplateType.ToastText01)
                type = ToastTemplateType.ToastImageAndText01;
            if(type == ToastTemplateType.ToastText02)
                type = ToastTemplateType.ToastImageAndText02;
            if(type == ToastTemplateType.ToastText03)
                type = ToastTemplateType.ToastImageAndText03;
            if(type == ToastTemplateType.ToastText04)
                type = ToastTemplateType.ToastImageAndText04;
            CheckImagePath(imagePath);
            this.imagePath = imagePath;
        }
        public void SetImage(byte[] image) {
            SetImage(new MemoryStream(image));
        }
        public void SetImage(Stream stream) {
            SetImage(Image.FromStream(stream));
        }
        public void SetImage(Image image) {
            if(tempFilePath != null) {
                RemoveTempFile();
            }
            tempFilePath = Path.GetTempFileName() + Guid.NewGuid() + ".png";
            image.Save(tempFilePath, System.Drawing.Imaging.ImageFormat.Png);
            SetImage(tempFilePath);
        }
        bool IsSupportedExtension(string imagePath) {
            var extensions = new string[] { ".png", ".jpg", ".jpeg", ".gif" };
            string fileExt = Path.GetExtension(imagePath);
            foreach(string goodExt in extensions) {
                if(fileExt.Equals(goodExt, StringComparison.OrdinalIgnoreCase)) {
                    return true;
                }
            }
            return false;
        }
        void CheckImagePath(string imagePath) {
            FileInfo info = new FileInfo(imagePath);
            if(!info.Exists)
                return;
            if(!IsSupportedExtension(info.Extension))
                throw new ArgumentException("Unsupported file type");
            if(info.Length > 1024 * 200)
                throw new ArgumentException("File must have size less or equal to 200 KB");
        }
        static void SetTextLine(IXmlDocument xmldoc, uint index, string text) {
            IXmlNodeList nodes;
            ComFunctions.CheckHRESULT(xmldoc.GetElementsByTagName("text", out nodes));
            Debug.Assert(nodes.Length >= index + 1);
            IXmlNode node;
            ComFunctions.CheckHRESULT(nodes.Item(index, out node));
            SetNodeValueString(text, xmldoc, node);
        }
        static bool IsLoopingSound(PredefinedSound sound) {
            return sound >= PredefinedSound.Notification_Looping_Alarm;
        }
        public IPredefinedToastNotificationInfo Info {
            get {
                return new WinRTToastNotificationInfo()
                {
                    ToastTemplateType = type,
                    Lines = lines,
                    ImagePath = imagePath,
                    Duration = duration,
                    Sound = sound
                };
            }
        }
        class WinRTToastNotificationInfo : IPredefinedToastNotificationInfo {
            public ToastTemplateType ToastTemplateType { get; set; }
            public string[] Lines { get; set; }
            public string ImagePath { get; set; }
            public NotificationDuration Duration { get; set; }
            public PredefinedSound Sound { get; set; }
        }
        internal static IXmlDocument GetDocument(IToastNotificationManager manager, IPredefinedToastNotificationInfo info) {
            var content = manager.GetTemplateContent(info.ToastTemplateType);
            for(uint i = 0; i < info.Lines.Length; i++) {
                SetTextLine(content, i, info.Lines[i]);
            }
            if(!string.IsNullOrEmpty(info.ImagePath)) {
                SetImageSrc(content, "file:///" + info.ImagePath);
            }
            NotificationDuration actualDuration = info.Duration;
            if(info.Sound != PredefinedSound.Notification_Default) {
                SetSound(content, info.Sound);
                if(IsLoopingSound(info.Sound)) {
                    actualDuration = NotificationDuration.Long;
                }
            }
            if(actualDuration != NotificationDuration.Default) {
                SetDuration(content, actualDuration);
            }
            return content;
        }
        static void SetDuration(IXmlDocument xmldoc, NotificationDuration duration) {
            IXmlNodeList nodes;
            ComFunctions.CheckHRESULT(xmldoc.GetElementsByTagName("toast", out nodes));
            IXmlNode toastNode;
            ComFunctions.CheckHRESULT(nodes.Item(0, out toastNode));
            ((IXmlElement)toastNode).SetAttribute("duration", "long");
        }
        static void SetSound(IXmlDocument xmldoc, PredefinedSound sound) {
            string soundXml = "ms-winsoundevent:" + sound.ToString().Replace("_", ".");
            IXmlElement soundElement;
            ComFunctions.CheckHRESULT(xmldoc.CreateElement("audio", out soundElement));
            if(sound == PredefinedSound.NoSound) {
                ComFunctions.CheckHRESULT(soundElement.SetAttribute("silent", "true"));
            }
            else {
                ComFunctions.CheckHRESULT(soundElement.SetAttribute("src", soundXml));
                ComFunctions.CheckHRESULT(soundElement.SetAttribute("loop", IsLoopingSound(sound).ToString().ToLower()));
            }
            var asNode = (IXmlNode)xmldoc;
            IXmlNode appendedChild;
            IXmlNodeList nodes;
            ComFunctions.CheckHRESULT(xmldoc.GetElementsByTagName("toast", out nodes));
            IXmlNode toastNode;
            ComFunctions.CheckHRESULT(nodes.Item(0, out toastNode));
            ComFunctions.CheckHRESULT(toastNode.AppendChild((IXmlNode)soundElement, out appendedChild));
        }
        public bool IsAssigned { get; set; }
        void RemoveTempFile() {
            try {
                if(!string.IsNullOrEmpty(tempFilePath) && File.Exists(tempFilePath))
                    File.Delete(tempFilePath);
                tempFilePath = null;
            }
            catch { }
        }
    }
}