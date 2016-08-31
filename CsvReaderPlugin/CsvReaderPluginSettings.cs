using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;


namespace CsvReaderPlugin
{
    [DataContract]
    public class CsvReaderPluginSettings
    {
        [DataMember]
        public string CsvUri { get; set; }
        [DataMember]
        public string ContentEncoding { get; set; }
        [DataMember]
        public bool IsWebResource { get; set; }
        [DataMember]
        public List<string> AdditionalHttpHeaders { get; set; }

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
        public bool HasHeaders { get; set; }
        [DataMember]
        public string DelimeterChar { get; set; }
        [DataMember]
        public char QuotationChar { get; set; }
        [DataMember]
        public string ReadConfigPath { get; set; }
        [DataMember(Name="ItemMap")]
        public List<CsvItem> ItemMap { get; set; }
        [DataMember]
        public TimeSpan MaxTimeToReadAtOnce { get; set; }
        [DataMember]
        public string MeasurementTimeFieldHeader { get; set; }
        [DataMember]
        public int MeasurementTimeFieldIndex { get; set; }
    }

    [DataContract]
    public class CsvItem
    {
        [DataMember]
        public string Header { get; set; }
        [DataMember]
        public int Index { get; set; }
        [DataMember]
        public string SamiTag { get; set; }
        [DataMember]
        public ValueLinearScale Scale { get; set; }
        /// <summary>
        /// Returns item header.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Format("{0}", Header);
        }
    }

    [DataContract]
    public class ValueLinearScale
    {
        [DataMember]
        public LinearScaleLimit Source { get; set; }
        [DataMember]
        public LinearScaleLimit Destination { get; set; }
        /// <summary>
        /// Scale given value to scaled value. Assumes linear scaling.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public double ScaleValue(double value)
        {
            if (null == Source || null == Destination)
            {
                // cannot scale --> return value as is.
                return value;
            }
            var sourceRange = Source.Range;
            if (0m == sourceRange)
            {
                // prevent division by zero --> return value as is.
                return value;
            }
            var destinationRange = Destination.Range;
            if (0m == destinationRange)
            {
                return value;
            }
            var multiplier = destinationRange / sourceRange;
            return (value - (double)Source.From) * (double)multiplier + (double)Destination.From;
        }
    }

    [DataContract]
    public class LinearScaleLimit
    {
        [DataMember]
        public decimal From { get; set; }
        [DataMember]
        public decimal To { get; set; }
        /// <summary>
        /// Get Scale limit range. Range is To - From.
        /// </summary>
        public decimal Range
        {
            get { return To - From; }
        }
    }
}
