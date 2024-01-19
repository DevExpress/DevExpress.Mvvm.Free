using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Xml;
using System.Xml.Serialization;
using DevExpress.Mvvm.Internal;
using DevExpress.Mvvm.Native;

namespace DevExpress.Mvvm.ModuleInjection.Native {
    [Serializable]
    public class SerializableState {
        [XmlIgnore]
        public string State {
            get;
            private set;
        }
        [EditorBrowsable(EditorBrowsableState.Never)]
        public XmlCDataSection CDATAState {
            get {
                if(string.IsNullOrEmpty(State))
                    return null;
                var document = new XmlDocument();
                return document.CreateCDataSection(State);
            }
            set { State = value?.Value; }
        }
        public SerializableState() { }
        public SerializableState(string state) {
            State = state;
        }
    }

    [Serializable]
    public class RegionInfo {
        public string RegionName { get; set; }
        public string SelectedViewModelKey { get; set; }
        public List<RegionItemInfo> Items { get; set; }
        public RegionInfo() {
            Items = new List<RegionItemInfo>();
        }
    }
    [Serializable]
    public class RegionItemInfo {
        public string Key { get; set; }
        public string ViewModelName { get; set; }
        public string ViewName { get; set; }
        public string ViewModelStateType { get; set; }
        public bool IsInjected { get; set; }
        public SerializableState ViewModelState { get; set; }
        public RegionItemInfo() {
            IsInjected = true;
        }
    }
    [Serializable]
    public class RegionVisualInfo {
        public string RegionName { get; set; }
        public List<RegionItemVisualInfo> Items { get; set; }
        public RegionVisualInfo() {
            Items = new List<RegionItemVisualInfo>();
        }
    }
    [Serializable]
    public class RegionItemVisualInfo {
        public string Key { get; set; }
        public string ViewName { get; set; }
        public string ViewPart { get; set; }
        public SerializableState State { get; set; }
    }

    [Serializable]
    public class LogicalInfo {
        public static string Serialize(LogicalInfo logicalState) {
            return SerializationHelper.SerializeToString(x => XmlSerializerHelper.Serialize<LogicalInfo>(x,logicalState));
        }
        public static LogicalInfo Deserialize(string logicalState) {
            if(string.IsNullOrEmpty(logicalState))
                return null;
            return SerializationHelper.DeserializeFromString(logicalState, x => XmlSerializerHelper.Deserialize<LogicalInfo>(x));
        }
        public LogicalInfo() {
            Regions = new List<RegionInfo>();
        }
        public List<RegionInfo> Regions { get; set; }
    }
    [Serializable]
    public class VisualInfo {
        public static string Serialize(VisualInfo visualState) {
            return SerializationHelper.SerializeToString(x => XmlSerializerHelper.Serialize<VisualInfo>(x, visualState));
        }
        public static VisualInfo Deserialize(string visualState) {
            if(string.IsNullOrEmpty(visualState))
                return null;
            return SerializationHelper.DeserializeFromString(visualState, x => XmlSerializerHelper.Deserialize<VisualInfo>(x));
        }
        public VisualInfo() {
            Regions = new List<RegionVisualInfo>();
        }
        public List<RegionVisualInfo> Regions { get; set; }
    }
}