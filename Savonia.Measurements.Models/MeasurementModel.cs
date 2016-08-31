using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Xml.Serialization;

namespace Savonia.Measurements.Models
{
    [DataContract]
    [Serializable]
    public class MeasurementModel
    {
        // Key is only for OData key
        [IgnoreDataMember]
        [XmlIgnore]
        public string Key { get; set; }

        [IgnoreDataMember]
        [XmlIgnore]
        public long ID { get; set; }

        [IgnoreDataMember]
        [XmlIgnore]
        public long ProviderID { get; set; }
        
        [DataMember]
        public string Object { get; set; }
        [DataMember]
        public string Tag { get; set; }
        [DataMember]
        public DateTimeOffset Timestamp { get; set; }

        [DataMember(IsRequired=false)]
        public string TimestampISO8601 { get; set; }
        
        [DataMember]
        public string Note { get; set; }

        [DataMember]
        public Location Location { get; set; }

        [DataMember]
        [XmlElement(ElementName = "Data")]
        public List<DataModel> Data { get; set; }

        public override string ToString()
        {
            string display = string.Join(" - ", Object, Tag);
            return display;
        }
    }

    [DataContract]
    [Serializable]
    public class DataModel
    {
        [IgnoreDataMember]
        public long MeasurementID { get; set; }

        [DataMember]
        [XmlAttribute]
        public string Tag { get; set; }
        [DataMember]
        public double? Value { get; set; }
        [DataMember]
        public long? LongValue { get; set; }
        [DataMember]
        public string TextValue { get; set; }
        [DataMember]
        public byte[] BinaryValue { get; set; }
        [DataMember]
        public string XmlValue { get; set; }
    }

    [DataContract]
    [Serializable]
    public class Location
    {
        [DataMember]
        public double? Latitude { get; set; }
        [DataMember]
        public double? Longitude { get; set; }
        /// <summary>
        /// Expects WGS 84 type text for location info.
        /// </summary>
        [DataMember]
        public string WellKnownTextWGS84 { get; set; }

        [IgnoreDataMember]
        public bool IsValid
        {
            get { return (Latitude.HasValue && Longitude.HasValue) || !string.IsNullOrEmpty(WellKnownTextWGS84); }
        }

        public override string ToString()
        {
            if (Latitude.HasValue && Longitude.HasValue)
            {
                return string.Format("POINT({0} {1})", this.GetCoordinate(Longitude), this.GetCoordinate(Latitude));
            }
            return WellKnownTextWGS84;
        }

        public string GetCoordinate(double? coordinate)
        {
            if (coordinate.HasValue)
            {
                return coordinate.Value.ToString("G", CultureInfo.InvariantCulture);
            }
            return string.Empty;
        }
    }
}