using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;


namespace RemoteMXPlugin
{
    [DataContract]
    public class RemoteMXPluginSettings
    {
        [DataMember]
        public string SamiKey { get; set; }
        [DataMember]
        public string MeasurementObject { get; set; }
        [DataMember]
        public string MeasurementTag { get; set; }
        [DataMember]
        public string UserName { get; set; }
        [DataMember]
        public string Password { get; set; }
        [DataMember]
        public int SiteId { get; set; }
        [DataMember]
        public string ReadConfigPath { get; set; }
        [DataMember(Name="ChannelMap")]
        public Dictionary<string, string> ChannelMap { get; set; }
        [DataMember]
        public bool SaveMappedValuesOnly { get; set; }
        [DataMember]
        public TimeSpan MaxTimeToReadAtOnce { get; set; }
    }
}
