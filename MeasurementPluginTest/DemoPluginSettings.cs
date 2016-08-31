using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;


namespace MeasurementPluginSample
{
    [Serializable]
    public class DemoPluginSettings
    {
        public string Key { get; set; }
        public string Object { get; set; }
        public string Tag { get; set; }
        [XmlElement(ElementName="SensorTag")]
        public List<string> SensorTags { get; set; }
    }
}
