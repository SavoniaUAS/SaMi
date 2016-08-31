using Savonia.Measurements.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace Savonia.Measurements.Manager.Models
{
    /// <summary>
    /// This class is used as viewmodel for saved queries
    /// </summary>
    public class SavedQueryModel
    {
        [DataMember]
        [Display(Name = "ID")]
        public int ID { get; set; }

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
            get; set;
        }
        [DataMember]
        [Display(Name = "From")]
        public string From { get; set; }

        [DataMember]
        [Display(Name = "To")]
        public string To { get; set; }


        [DataMember]
        [Display(Name = "Data tags")]
        public string Sensors { get; set; }

        public SavedQueryModel()
        {

        }
        public SavedQueryModel(MeasurementQueryModel model)
        {
            this.Name = model.Name;
            this.Obj = model.Obj;
            this.Tag = model.Tag;
            this.Take = model.Take;
            if (model.From.HasValue)
            {
                var f = model.From.Value.ToString("u");
                var a = f.Split('Z');
                var from = a[0];
                this.From = from;
            }

            if (model.To.HasValue)
            {
                var f = model.To.Value.ToString("u");
                var a = f.Split('Z');
                var to = a[0];
                this.To = to;
            }
            this.Sensors = model.Sensors;

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