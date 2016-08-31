using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Savonia.Measurements.Models
{
    [DataContract]
    [Serializable]
    public class SensorModel
    {
        [IgnoreDataMember]
        [Display(Name = "Provider ID")]
        public int ProviderID { get; set; }

        [Required(ErrorMessage = "Tag field is required!")]
        [DataMember]
        [Display(Name = "Tag")]
        public string Tag { get; set; }

        [DataMember]
        [Display(Name = "Name")]
        public string Name { get; set; }

        [DataMember]
        [Display(Name = "Description")]
        public string Description { get; set; }

        [DataMember]
        [Display(Name = "Unit")]
        public string Unit { get; set; }

        [DataMember]
        [Display(Name = "Decimal Count")]
        public int? ValueDecimalCount { get; set; }

        [DataMember]
        [Display(Name = "Location")]
        public Location Location { get; set; }
    }
}
