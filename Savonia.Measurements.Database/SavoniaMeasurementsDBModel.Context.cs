﻿//------------------------------------------------------------------------------
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
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    
    public partial class SavoniaMeasurementsV2Entities : DbContext
    {
        public SavoniaMeasurementsV2Entities()
            : base("name=SavoniaMeasurementsV2Entities")
        {
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public virtual DbSet<AccessKey> AccessKeys { get; set; }
        public virtual DbSet<Datum> Data { get; set; }
        public virtual DbSet<Measurement> Measurements { get; set; }
        public virtual DbSet<Sensor> Sensors { get; set; }
        public virtual DbSet<Meta> Metas { get; set; }
        public virtual DbSet<Provider> Providers { get; set; }
        public virtual DbSet<Query> Queries { get; set; }
    }
}
