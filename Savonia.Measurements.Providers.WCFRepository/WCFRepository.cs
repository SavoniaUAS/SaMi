using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Savonia.Measurements.Models;
using Savonia.Measurements.Providers.Models;

namespace Savonia.Measurements.Providers.WCFRepository
{
    public class WCFRepository : IMeasurementRepository
    {
        public Logger Log { private get; set; }
        public string Name 
        {
            get { return "WCF repository"; }
        }

        public void Initialize(string config)
        {
            // do nothing
        }

        public MeasurementPersistResult Persist(MeasurementPackage package)
        {
            MeasurementPersistResult persisted = new MeasurementPersistResult();
            MeasurementsServiceV1.MeasurementsServiceClient client = new MeasurementsServiceV1.MeasurementsServiceClient();
            try
            {
                var result = client.SaveMeasurementPackage(package);
                persisted.SaveResult = result;
                client.Close();
            }
            catch (Exception ex)
            {
                persisted.Exception = ex;
                if (client.State != System.ServiceModel.CommunicationState.Closed)
                {
                    client.Abort();
                }
            }
            client = null;
            return persisted;
        }

        public void Dispose()
        {
            // do nothing
        }
    }
}
