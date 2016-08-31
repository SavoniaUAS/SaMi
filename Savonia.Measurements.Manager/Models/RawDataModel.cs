using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Savonia.Measurements.Manager.Models
{
    public class RawDataModel
    {
        public const string DefaultSeparators = ";\t";

        [DataType(DataType.Password)]
        public string Key { get; set; }
        [DataType(DataType.MultilineText)]
        [Display(Name= "Raw data")]
        public string RawData { get; set; }
        public string Separators { get; set; }
    }
}