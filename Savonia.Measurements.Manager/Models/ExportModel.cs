using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Globalization;
using System.Runtime.Serialization;
using System.ComponentModel.DataAnnotations;

namespace Savonia.Measurements.Manager.Models
{
    public class ExportModel
    {

        private string _datetimeformat;
        /// <summary>
        /// Datetime format 
        /// </summary>
        [DataMember]
        [Display(Name = "Datetime Format")]
        public string Datetimeformat
        {
            get { return _datetimeformat; }
            set { _datetimeformat = value; }
        }

        /// <summary>
        /// Delimeter for columns
        /// </summary>
        [DataMember]
        [Display(Name = "Delimeter")]
        public string ColumnDelimeter
        { get; set; }

        /// <summary>
        /// Show headers 
        /// </summary>
        [DataMember]
        [Display(Name = "Headers")]
        public bool ShowHeaders
        {
            get;
            set;
        }

        private string _fileFormat;
        /// <summary>
        /// Format for file
        /// </summary>
        [DataMember]
        [Display(Name = "Format")]
        public string FileFormat
        {

            get { return _fileFormat; }
            set { _fileFormat = value; }

        }
        [DataMember]
        [Display(Name = "Values")]
        public DataValueModel DataValues { get; set; }


        /// <summary>
        /// Initializes new instance of ExportModel class and sets default values for properties
        /// </summary>
        public ExportModel()
        {
            ShowHeaders = true;
            Datetimeformat = "yyyy-MM-ddTHH\\:mm\\:ss.zzz";
            ColumnDelimeter = ";";
            FileFormat = ".csv";
        }


        /// <summary>
        /// Set DateTimeformat for file (default format is yyyy-MM-ddTHH\\:mm\\:ss.zzz)
        /// </summary>
        /// <param name="format">Tries to set given string to models DatetimeFormat</param>
        /// <returns>Returns true if successfully set</returns>
        public bool SetDatetimeFormat(string format = "default")
        {

            bool returnValue = false;


            if (format != "default" && format != "yyyy-MM-ddTHH\\:mm\\:ss.zzz")
            {
                CultureInfo ci = CultureInfo.GetCultureInfo("en-US");


                string[] dtPatterns = ci.DateTimeFormat.GetAllDateTimePatterns();
                foreach (var Datetimeformat in dtPatterns)
                {
                    if (Datetimeformat == format)
                    {
                        returnValue = true;
                        _datetimeformat = format;
                        break;
                    }
                    else
                    {
                        returnValue = false;
                    }
                }

            }
            else
            {
                _datetimeformat = "yyyy-MM-ddTHH\\:mm\\:ss.zzz"; //"yyyy'-'MM'-'dd'T'HH':'mm':'ss"
                returnValue = true;
            }

            return returnValue;

        }

    }

    public class DataValueModel
    {
        [DataMember]
        public bool Value { get; set; }
        [DataMember]
        public bool LongValue { get; set; }
        [DataMember]
        public bool TextValue { get; set; }
        [DataMember]
        public bool BinaryValue { get; set; }
        [DataMember]
        public bool XmlValue { get; set; }

    }
}