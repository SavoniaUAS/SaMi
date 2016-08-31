using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Savonia.Measurements.Database;
using Savonia.Measurements.Providers.Models;
using Savonia.Measurements.Models;

namespace Savonia.Measurements.Providers.DatabaseRepository
{
    public class DBRepository : IMeasurementRepository
    {
        public string Name 
        {
            get { return "Database repository"; }
        }
        public Logger Log { private get; set; }

        public void Initialize(string config)
        {
            // do nothing
        }

        public MeasurementPersistResult Persist(MeasurementPackage package)
        {
            // persist directly to measurements database
            MeasurementPersistResult persisted;
            using (Savonia.Measurements.Database.Repository r = new Database.Repository())
            {
                var result = r.SaveMeasurementsAsync(package.Key, package.Measurements.ToArray());
                persisted = new MeasurementPersistResult(result.Result);
            }

            return persisted;
        }

        public void Dispose()
        {
            // do nothing
        }


    }
}
