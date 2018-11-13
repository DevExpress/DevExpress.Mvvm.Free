using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using DevExpress.Internal.WinApi;
using DevExpress.Internal.WinApi.Window.Data.Xml.Dom;
using DevExpress.Internal.WinApi.Windows.UI.Notifications;

namespace DevExpress.Internal {
    class WinRTToastNotificationContent : IPredefinedToastNotificationContent, IPredefinedToastNotificationContentGeneric {
        string[] lines = new string[3];
        ToastTemplateType type;
        string imagePath;
        internal string tempImagePath;
        string appLogoImagePath;
        string tempAppLogoImagePath;
        string heroImagePath;
        string tempHeroImagePath;
        ImageCropType appLogoImageCrop;
        PredefinedSound sound;
        NotificationDuration duration = NotificationDuration.Default;
        string attributionText = string.Empty;
        DateTimeOffset? displayTimestamp = null;
        WinRTToastNotificationContent() { }
        public void Dispose() {
            RemoveTempFile(tempImagePath);
            tempImagePath = null;
            RemoveTempFile(tempAppLogoImagePath);
            tempAppLogoImagePath = null;
            RemoveTempFile(tempHeroImagePath);
            tempHeroImagePath = null;
            updateToastContent = null;
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
        public static WinRTToastNotificationContent CreateToastGeneric(string headlineText, string bodyText1, string bodyText2) {
            return Create(ToastTemplateType.ToastGeneric, headlineText, bodyText1, bodyText2);
        }
        static WinRTToastNotificationContent Create(ToastTemplateType type, params string[] lines) {
            return new WinRTToastNotificationContent { type = type, lines = lines };
        }
        static void SetNodeValueString(string str, IXmlDocument xmldoc, IXmlNode node) {
            IXmlText textNode;
            int res = xmldoc.CreateTextNode(str, out textNode);
            ComFunctions.CheckHRESULT(res);
            AppendNode(node, (IXmlNode)textNode);
        }
        static void SetImageSrc(IXmlDocument xmldoc, string imagePath) {
            IXmlNode imageNode = GetNode(xmldoc, "image");
            IXmlNode srcAttribute;
            int res = imageNode.Attributes.GetNamedItem("src", out srcAttribute);
            ComFunctions.CheckHRESULT(res);

            SetNodeValueString(imagePath, xmldoc, srcAttribute);
        }
        public void SetAppLogoImageCrop(ImageCropType appLogoImageCrop) {
            this.appLogoImageCrop = appLogoImageCrop;
        }
        public void SetSound(PredefinedSound sound) {
            this.sound = sound;
        }
        public void SetDuration(NotificationDuration duration) {
            this.duration = duration;
        }
        public void SetAttributionText(string attributionText) {
            this.attributionText = attributionText;
        }
        public void SetDisplayTimestamp(DateTimeOffset? displayTimestamp) {
            this.displayTimestamp = displayTimestamp;
        }
        public void SetImage(string imagePath) {
            SetImage(imagePath, ImagePlacement.Inline);
        }
        public void SetImage(string imagePath, ImagePlacement placement) {
            CheckImagePath(imagePath);
            switch(placement) {
                case ImagePlacement.Inline:
                    UpdateTemplateType();
                    this.imagePath = imagePath;
                    break;
                case ImagePlacement.AppLogo:
                    this.appLogoImagePath = imagePath;
                    break;
                case ImagePlacement.Hero:
                    this.heroImagePath = imagePath;
                    break;
            }
        }
        void UpdateTemplateType() {
            if(type != ToastTemplateType.ToastGeneric) {
                if(type == ToastTemplateType.ToastText01)
                    type = ToastTemplateType.ToastImageAndText01;
                if(type == ToastTemplateType.ToastText02)
                    type = ToastTemplateType.ToastImageAndText02;
                if(type == ToastTemplateType.ToastText03)
                    type = ToastTemplateType.ToastImageAndText03;
                if(type == ToastTemplateType.ToastText04)
                    type = ToastTemplateType.ToastImageAndText04;
            }
            else if(!ToastNotificationManager.IsGenericTemplateSupported)
                type = ToastTemplateType.ToastImageAndText04;

        }
        public void SetImage(byte[] image) {
            SetImage(new MemoryStream(image));
        }
        public void SetImage(Stream stream) {
            SetImage(Image.FromStream(stream));
        }
        public void SetImage(Image image) {
            SetImage(image, ImagePlacement.Inline);
        }
        public void SetImage(Image image, ImagePlacement placement) {
            string imagePath = GetTempPath();
            switch(placement) {
                case ImagePlacement.Inline:
                    if(tempImagePath != null) {
                        RemoveTempFile(tempImagePath);
                        tempImagePath = null;
                    }
                    tempImagePath = imagePath;
                    break;
                case ImagePlacement.AppLogo:
                    if(tempAppLogoImagePath != null) {
                        RemoveTempFile(tempAppLogoImagePath);
                        tempAppLogoImagePath = null;
                    }
                    tempAppLogoImagePath = imagePath;
                    break;
                case ImagePlacement.Hero:
                    if(tempHeroImagePath != null) {
                        RemoveTempFile(tempHeroImagePath);
                        tempHeroImagePath = null;
                    }
                    tempHeroImagePath = imagePath;
                    break;
            }
            SaveImageToFile(image, imagePath);
            SetImage(imagePath, placement);
        }
        static string GetTempPath() {
            string tempPath = Path.GetTempPath();
            string result = string.Empty;
            do {
                result = string.Format("{0}{1}.png", tempPath, Guid.NewGuid());
            }
            while(File.Exists(result));
            return result;
        }
        void SaveImageToFile(Image image, string path) {
            image.Save(path, System.Drawing.Imaging.ImageFormat.Png);
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
        static bool IsLoopingSound(PredefinedSound sound) {
            return sound >= PredefinedSound.Notification_Looping_Alarm;
        }

        Action<System.Xml.XmlDocument> updateToastContent = null;
        public void SetUpdateToastContentAction(Action<System.Xml.XmlDocument> updateToastContentAction) {
            updateToastContent = updateToastContentAction;
        }
        public IPredefinedToastNotificationInfo Info {
            get {
                return new WinRTToastNotificationInfo() {
                    ToastTemplateType = type,
                    Lines = lines,
                    ImagePath = imagePath,
                    AppLogoImagePath = appLogoImagePath,
                    HeroImagePath = heroImagePath,
                    AppLogoImageCrop = appLogoImageCrop,
                    Duration = duration,
                    Sound = sound,
                    AttributionText = attributionText,
                    DisplayTimestamp = displayTimestamp,
                    UpdateToastContent = updateToastContent
                };
            }
        }
        class WinRTToastNotificationInfo : IPredefinedToastNotificationInfo, IPredefinedToastNotificationInfoGeneric {
            public ToastTemplateType ToastTemplateType { get; set; }
            public string[] Lines { get; set; }
            public string ImagePath { get; set; }
            public string AppLogoImagePath { get; set; }
            public string HeroImagePath { get; set; }
            public ImageCropType AppLogoImageCrop { get; set; }
            public NotificationDuration Duration { get; set; }
            public PredefinedSound Sound { get; set; }
            public string AttributionText { get; set; }
            public DateTimeOffset? DisplayTimestamp { get; set; }
            public Action<System.Xml.XmlDocument> UpdateToastContent { get; set; }
        }
        internal static IXmlDocument GetDocument(IToastNotificationManager manager, IPredefinedToastNotificationInfo info) {
            var content = manager.GetTemplateContent(GetToastTemplateType(info));
            if(ToastNotificationManager.IsGenericTemplateSupported) {
                UpdateTemplate(content, info);
                UpdateAttributionText(content, info);
                UpdateDisplayTimestamp(content, info);
                UpdateAppLogoImage(info, content);
                UpdateHeroImage(info, content);
            }
            UpdateText(content, info);
            UpdateInlineImage(info, content);
            UpdateSound(content, info);
            UpdateDuration(content, info);
            UpdateContent(content, info);
            return content;
        }
        static void UpdateContent(IXmlDocument xmldoc, IPredefinedToastNotificationInfo info) {
            string xml = ToastNotificationManager.GetXml((IXmlNodeSerializer)xmldoc);
            System.Xml.XmlDocument doc = new System.Xml.XmlDocument();
            doc.LoadXml(xml);
            IPredefinedToastNotificationInfoGeneric infoGeneric = info as IPredefinedToastNotificationInfoGeneric;
            if(infoGeneric.UpdateToastContent != null)
                infoGeneric.UpdateToastContent.Invoke(doc);
            infoGeneric.UpdateToastContent = null;
            ToastNotificationManager.LoadXml((IXmlDocumentIO)xmldoc, doc.OuterXml);
        }
        static ToastTemplateType GetToastTemplateType(IPredefinedToastNotificationInfo info) {
            if(info.ToastTemplateType != ToastTemplateType.ToastGeneric) return info.ToastTemplateType;
            return ToastTemplateType.ToastText04;
        }
        static void UpdateTemplate(IXmlDocument xmldoc, IPredefinedToastNotificationInfo info) {
            if(info.ToastTemplateType != ToastTemplateType.ToastGeneric) return;
            SetAttribute(xmldoc, "binding", "template", "ToastGeneric");
        }
        static void UpdateText(IXmlDocument xmldoc, IPredefinedToastNotificationInfo info) {
            IXmlNodeList nodes = GetNodes(xmldoc, "text");
            Debug.Assert(nodes.Length >= info.Lines.Length);
            for(uint i = 0; i < info.Lines.Length; i++)
                SetNodeValueString(info.Lines[i], xmldoc, GetNode(nodes, i));
        }
        static void UpdateAttributionText(IXmlDocument xmldoc, IPredefinedToastNotificationInfo info) {
            IPredefinedToastNotificationInfoGeneric infoGeneric = info as IPredefinedToastNotificationInfoGeneric;
            if(string.IsNullOrWhiteSpace(infoGeneric.AttributionText)) return;
            IXmlElement attributionTextElement = CreateElement(xmldoc, "text");
            IXmlNode bindingNode = GetNode(xmldoc, "binding");
            IXmlNode appendedChild = AppendNode(bindingNode, (IXmlNode)attributionTextElement);
            SetAttribute(appendedChild, "placement", "attribution");
            SetNodeValueString(infoGeneric.AttributionText, xmldoc, appendedChild);
        }
        static void UpdateDisplayTimestamp(IXmlDocument xmldoc, IPredefinedToastNotificationInfo info) {
            IPredefinedToastNotificationInfoGeneric infoGeneric = info as IPredefinedToastNotificationInfoGeneric;
            if(infoGeneric.DisplayTimestamp == null) return;
            IXmlNode toastNode = GetNode(xmldoc, "toast");
            string displayTimestamp = infoGeneric.DisplayTimestamp.Value.ToLocalTime().ToString("yyyy-MM-ddTHH:mm:ssZ");
            SetAttribute(toastNode, "displayTimestamp", displayTimestamp);
        }
        static void UpdateInlineImage(IPredefinedToastNotificationInfo info, IXmlDocument content) {
            if(!string.IsNullOrEmpty(info.ImagePath)) {
                string imagePath = new System.Uri(info.ImagePath).AbsoluteUri;
                if(info.ToastTemplateType == ToastTemplateType.ToastGeneric)
                    CreateInlineImageNode(content, imagePath);
                else
                    SetImageSrc(content, imagePath);
            }
        }
        static void UpdateAppLogoImage(IPredefinedToastNotificationInfo info, IXmlDocument content) {
            IPredefinedToastNotificationInfoGeneric infoGeneric = info as IPredefinedToastNotificationInfoGeneric;
            if(!string.IsNullOrEmpty(infoGeneric.AppLogoImagePath) && info.ToastTemplateType == ToastTemplateType.ToastGeneric) {
                string appLogoImagePath = new System.Uri(infoGeneric.AppLogoImagePath).AbsoluteUri;
                IXmlNode appLogoImageNode = CreateInlineImageNode(content, appLogoImagePath);
                SetAttribute(appLogoImageNode, "placement", "appLogoOverride");
                if(infoGeneric.AppLogoImageCrop == ImageCropType.Circle)
                    SetAttribute(appLogoImageNode, "hint-crop", "circle");
            }
        }
        static void UpdateHeroImage(IPredefinedToastNotificationInfo info, IXmlDocument content) {
            IPredefinedToastNotificationInfoGeneric infoGeneric = info as IPredefinedToastNotificationInfoGeneric;
            if(!string.IsNullOrEmpty(infoGeneric.HeroImagePath) && info.ToastTemplateType == ToastTemplateType.ToastGeneric) {
                string heroImagePath = new System.Uri(infoGeneric.HeroImagePath).AbsoluteUri;
                IXmlNode heroImageNode = CreateInlineImageNode(content, heroImagePath);
                SetAttribute(heroImageNode, "placement", "hero");
            }
        }
        static IXmlNode CreateInlineImageNode(IXmlDocument content, string imagePath) {
            IXmlNode bindingNode = GetNode(content, "binding");
            IXmlElement imageElement = CreateElement(content, "image");
            IXmlNode inlineImageNode = AppendNode(bindingNode, (IXmlNode)imageElement);
            SetAttribute(inlineImageNode, "src", imagePath);
            return inlineImageNode;
        }
        static void UpdateSound(IXmlDocument content, IPredefinedToastNotificationInfo info) {
            if(info.Sound != PredefinedSound.Notification_Default)
                SetSound(content, info.Sound);
        }
        static void UpdateDuration(IXmlDocument xmldoc, IPredefinedToastNotificationInfo info) {
            NotificationDuration duration = info.Duration;
            if(IsLoopingSound(info.Sound))
                duration = NotificationDuration.Long;
            if(duration != NotificationDuration.Default)
                SetAttribute(xmldoc, "toast", "duration", "long");
        }
        static void SetAttribute(IXmlDocument xmldoc, string tagName, string attributeName, string attributeValue) {
            IXmlNode node = GetNode(xmldoc, tagName);
            SetAttribute(node, attributeName, attributeValue);
        }
        static void SetAttribute(IXmlNode node, string attributeName, string attributeValue) {
            SetAttribute((IXmlElement)node, attributeName, attributeValue);
        }
        static void SetAttribute(IXmlElement node, string attributeName, string attributeValue) {
            ComFunctions.CheckHRESULT(node.SetAttribute(attributeName, attributeValue));
        }
        static void SetSound(IXmlDocument xmldoc, PredefinedSound sound) {
            string soundXml = "ms-winsoundevent:" + sound.ToString().Replace("_", ".");
            IXmlElement soundElement = CreateElement(xmldoc, "audio");
            if(sound == PredefinedSound.NoSound) {
                SetAttribute(soundElement, "silent", "true");
            }
            else {
                SetAttribute(soundElement, "src", soundXml);
                SetAttribute(soundElement, "loop", IsLoopingSound(sound).ToString().ToLower());
            }
            IXmlNode toastNode = GetNode(xmldoc, "toast");
            AppendNode(toastNode, (IXmlNode)soundElement);
        }
        static IXmlNode GetNode(IXmlDocument xmldoc, string tagName) {
            IXmlNodeList nodes = GetNodes(xmldoc, tagName);
            IXmlNode node = GetNode(nodes, 0);
            return node;
        }
        static IXmlNode GetNode(IXmlNodeList nodes, uint index) {
            IXmlNode node;
            ComFunctions.CheckHRESULT(nodes.Item(index, out node));
            return node;
        }
        static IXmlNodeList GetNodes(IXmlDocument xmldoc, string tagName) {
            IXmlNodeList nodes;
            ComFunctions.CheckHRESULT(xmldoc.GetElementsByTagName(tagName, out nodes));
            return nodes;
        }
        static IXmlElement CreateElement(IXmlDocument xmldoc, string elementName) {
            IXmlElement element;
            ComFunctions.CheckHRESULT(xmldoc.CreateElement(elementName, out element));
            return element;
        }
        static IXmlNode AppendNode(IXmlNode parentNode, IXmlNode childNode) {
            IXmlNode appendedChild;
            ComFunctions.CheckHRESULT(parentNode.AppendChild(childNode, out appendedChild));
            return appendedChild;
        }
        public bool IsAssigned { get; set; }
        void RemoveTempFile(string filePath) {
            try {
                if(!string.IsNullOrEmpty(filePath) && File.Exists(filePath))
                    File.Delete(filePath);
            }
            catch { }
        }
    }
}