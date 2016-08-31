using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Savonia.Measurements.Models
{
    [DataContract]
    public class ProviderModel
    {
        [IgnoreDataMember]
        [Display(Name = "ID")]
        public int ID { get; set; }

        [Required(ErrorMessage = "Key field is required!")]
        [MaxLength(150)]
        [DataMember]
        [Display(Name = "Key")]
        public string Key { get; set; }
        [DataMember]
        [DataType(DataType.MultilineText)]
        [Display(Name = "Info")]
        public string Info { get; set; }
        [MaxLength(250)]
        [DataMember]
        [Display(Name = "Name")]
        public string Name { get; set; }
        [MaxLength(250)]
        [DataMember]
        [Display(Name = "Owner")]
        public string Owner { get; set; }

        [DataMember]
        [Display(Name="Is public")]
        public bool IsPublic { get; set; }

        [DataMember]
        [Display(Name = "Created")]
        public DateTime Created { get; set; }

        [MaxLength(50)]
        [DataMember]
        [Display(Name = "Tag")]
        public string Tag { get; set; }

        [DataMember]
        [Display(Name = "Location")]
        public Location Location { get; set; }

        [DataMember]
        [DataType(DataType.EmailAddress)]
        [Display(Name = "Contact email")]
        public string ContactEmail { get; set; }

        [DataMember]
        [DataType(DataType.Date)]
        [Display(Name = "Active from")]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime? ActiveFrom { get; set; }

        [DataMember]
        [DataType(DataType.Date)]
        [Display(Name = "Active to")]
        [DisplayFormat(DataFormatString="{0:yyyy-MM-dd}", ApplyFormatInEditMode=true)]
        public DateTime? ActiveTo { get; set; }

        [DataMember]
        [DataType(DataType.Date)]
        [Display(Name = "Data stored until")]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime? DataStorageUntil { get; set; }

        [MaxLength(250)]
        [DataMember]
        [Display(Name = "Created By")]
        public string CreatedBy { get; set; }


        [DataMember]
        [XmlElement(ElementName = "AccessKey")]
        public List<AccessKeyModel> Keys { get; set; }
    }
}
