//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Savonia.Measurements.Database
{
    using System;
    using System.Collections.Generic;
    
    public partial class Measurement
    {
        public Measurement()
        {
            this.Data = new HashSet<Datum>();
        }
    
        public long ID { get; set; }
        public int ProviderID { get; set; }
        public string Object { get; set; }
        public string Tag { get; set; }
        public System.DateTimeOffset Timestamp { get; set; }
        public string Note { get; set; }
        public System.Data.Entity.Spatial.DbGeography Location { get; set; }
        public Nullable<System.DateTimeOffset> RowCreatedTimestamp { get; set; }
        public Nullable<short> KeyId { get; set; }
    
        public virtual ICollection<Datum> Data { get; set; }
        public virtual Provider Provider { get; set; }
    }
}
