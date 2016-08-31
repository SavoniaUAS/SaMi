using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Savonia.Measurements.Manager.Models
{
    public class ParsedDataModel : RawDataModel
    {
        public const string MeasurementTimeField = "mtime";
        public const string MeasurementObjectField = "mobject";
        public const string MeasurementTagField = "mtag";

        [Display(Name = "Field map")]
        public List<string> FieldMap { get; set; }
        public List<List<string>> Data { get; set; }
        [Display(Name = "Data field type")]
        public DataField DataFieldType { get; set; }

        public ParsedDataModel()
        {
            FieldMap = new List<string>();
            FieldMap.Add(MeasurementTimeField);
            FieldMap.Add(MeasurementObjectField);
            FieldMap.Add(MeasurementTagField);
        }
    }

    public enum DataField
    { 
        Value,
        LongValue,
        Text
    }
}