using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace Savonia.Measurements.Models
{
    [DataContract]
    public class MeasurementQueryModel
    {
        /// <summary>
        /// ID is only for Saved Queries/ Queries-table
        /// </summary>
        [IgnoreDataMember]
        [Display(Name = "ID")]
        public int ID { get; set; }

        [DataMember]
        [Required]
        [DataType(DataType.Password)]
        [Display(Name="Key")]
        public string Key { get; set; }

        [DataMember]
        [Display(Name = "Name")]
        public string Name { get; set; }

        [DataMember]
        [Display(Name = "Object")]
        public string Obj { get; set; }
        
        [DataMember]
        [Display(Name = "Tag")]
        public string Tag { get; set; }

        private int? take;

        [DataMember]
        [Display(Name = "Take")]
        public int? Take 
        {
            get
            {
                if (!this.take.HasValue && (!From.HasValue || !To.HasValue))
                {
                    return Constants.DefaultTakeValue;
                }
                return this.take;
            }
            set { this.take = value; } 
        }
        [DataMember]
        [Display(Name = "From")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString = Constants.DateTimeFormat)]
        public DateTimeOffset? From { get; set; }

        [DataMember]
        [Display(Name = "To")]
        [DataType(DataType.DateTime)]
        [DisplayFormat(DataFormatString=Constants.DateTimeFormat)]
        public DateTimeOffset? To { get; set; }

        [DataMember]
        [Display(Name = "Include From value to search results")]
        public bool InclusiveFrom { get; set; }

        [DataMember]
        [Display(Name = "Include To value to search results")]
        public bool InclusiveTo { get; set; }

        [DataMember]
        [Display(Name = "Data tags")]
        public string Sensors { get; set; }

        /// <summary>
        /// Create measurement query model with default Take value of 20 (<see cref="Savonia.Measurements.Models.Constants.DefaultTakeValue"/>).
        /// </summary>
        public MeasurementQueryModel()
        { 
            // set default Take count
            Take = Constants.DefaultTakeValue;
        }

        public List<string> DataTags()
        {
            if (string.IsNullOrWhiteSpace(Sensors))
            {
                return null;
            }
            var t = Sensors.Split(",;".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            return t.Distinct().ToList();
        }
    }
}