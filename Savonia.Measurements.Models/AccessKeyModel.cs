using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace Savonia.Measurements.Models
{
    [DataContract]
    [Serializable]
    public class AccessKeyModel
    {
        [DataMember]
        public int ProviderID { get; set; }

        [DataMember]
        [MaxLength(150)]
        [Display(Name ="Key")]
        [Required(ErrorMessage ="Key field is required!")]
        public string Key { get; set; }

       
        [DataMember]
        [Display(Name = "Access")]
        [Required(ErrorMessage = "Access field is required!")]
        public AccessControl AccessControl { get; set; }

        [DataMember]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        [Display(Name = "Valid From")]
        public DateTimeOffset? ValidFrom { get; set; }
        [DataMember]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        [Display(Name = "Valid To")]
        public DateTimeOffset? ValidTo { get; set; }

        [DataMember]
        [Display(Name = "Key ID")]
        public short? KeyId { get; set; }

        [DataMember]
        [DataType(DataType.MultilineText)]
        [Display(Name = "Info")]
        public string Info { get; set; }

        [IgnoreDataMember]
        [Display(Name = "Key")]
        public string CombinedKey
        {
            get
            {
                return this.ToString();
            }
        }

        /// <summary>
        /// AccessKey is valid when current time is in between ValidFrom and ValidTo values and ProviderID is greater than 0.
        /// </summary>
        public bool IsValid
        {
            get
            {
                if (ProviderID <= 0)
                {
                    return false;
                }
                bool isValid = true;
                var now = DateTimeOffset.Now;
                if (ValidFrom.HasValue)
                {
                    isValid = isValid & (ValidFrom.Value <= now);
                }
                if (ValidTo.HasValue)
                {
                    isValid = isValid & (ValidTo.Value >= now);
                }

                return isValid;
            }
        }

        public static AccessKeyModel FromKey(string key)
        {
            AccessKeyModel model = new AccessKeyModel();

            if (null == key)
            {
                return model;    
            }

            var data = key.Split('-');
            if (data.Length <= 1)
            {
                model.Key = key;
            }
            else
            {
                int i;
                if (data[0].StartsWith("SK"))
                {
                    if (int.TryParse(data[0].Substring(2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out i))
                    {
                        model.ProviderID = i;
                    }
                    model.Key = string.Join("-", data.Skip(1));
                }
                else
                {
                    model.Key = key;
                }
                
            }

            return model;
        }

        public override string ToString()
        {
            return string.Format("SK{0:X}-{1}", ProviderID, Key);
        }
    }
}
