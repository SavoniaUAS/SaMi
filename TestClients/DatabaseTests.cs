using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TestClients
{
    [TestClass]
    public class DatabaseTests
    {
        [TestMethod]
        public void Save20Measurements()
        {

            SaveMeasurements(20);
        }

        
        public async void SaveMeasurements(int count)
        {
            DateTimeOffset dt = DateTimeOffset.Now;
            var measurements = dt.GetMeasurementMeasurements("testData", count, 10, true);
            var key = TestHelper.TestKey;
            Savonia.Measurements.Database.Repository r = new Savonia.Measurements.Database.Repository();
            
              await  r.SaveMeasurementsAsync(key, measurements.ToArray());

            
        }
        }
}
