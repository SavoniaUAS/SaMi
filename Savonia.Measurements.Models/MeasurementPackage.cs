using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Savonia.Measurements.Models
{
    [DataContract]
    [Serializable]
    public class MeasurementPackage
    {
        [DataMember]
        [XmlAttribute]
        public string Key { get; set; }

        [DataMember]
        [XmlElement(ElementName="Measurement")]
        public List<MeasurementModel> Measurements { get; set; }
    }
}
