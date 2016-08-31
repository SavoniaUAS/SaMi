using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestClients.MeasurementsService;
using System.Collections.Generic;
using System.Diagnostics;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Threading.Tasks;

namespace TestClients
{
    [TestClass]
    public class WCFClientTest
    {
        [TestMethod]
        public void WCFSendMeasurementPackage()
        {
            MeasurementsServiceClient client = new MeasurementsServiceClient();

            DateTimeOffset measTime = DateTimeOffset.Now;

            var package = measTime.GetMeasurementPackage("WCF client");
            try
            {
                var result = client.SaveMeasurementPackage(package);
                Assert.IsNotNull(result);
                Assert.IsTrue(result.Success);
                Assert.IsNull(result.Failures);
            }
            catch (FaultException fe)
            {
                Trace.TraceError(fe.ToString());
                Assert.Fail();
            }
        }

        [TestMethod]
        public void WCFSendMeasurementPackageFaultyLocation()
        {
            MeasurementsServiceClient client = new MeasurementsServiceClient();

            DateTimeOffset measTime = DateTimeOffset.Now;

            var package = measTime.GetMeasurementPackage("WCF client");
            package.Measurements[0].Location.Latitude = 91; // set invalid latitude
            try
            {
                var result = client.SaveMeasurementPackage(package);
                Assert.Fail(); // service should throw
            }
            catch (FaultException fe)
            {
                Trace.TraceError(fe.ToString());
                Assert.IsTrue(fe.Message.Contains("24201: Latitude values must be between -90 and 90 degrees."));
            }
        }

        [TestMethod]
        public void WCFSendMeasurementPackage5Measurements()
        {
            this.WCFSendMeasurements(5, 10, false, false);
        }

        [TestMethod]
        public void WCFReadMeasurementsFromTo()
        {
            MeasurementQueryModel q = new MeasurementQueryModel()
            {
                Key = TestHelper.TestKey,
                From = new DateTimeOffset(2014, 11, 26, 0, 0, 0, TimeSpan.FromHours(2)),
                To = new DateTimeOffset(2014, 11, 27, 0, 0, 0, TimeSpan.FromHours(2))
            };

            this.WCFReadMeasurements(q, 13451);
        }

        [TestMethod]
        public async Task WCFReadMeasurementsFromToAsync()
        {
            MeasurementQueryModel q = new MeasurementQueryModel()
            {
                Key = TestHelper.TestKey,
                From = new DateTimeOffset(2014, 11, 26, 0, 0, 0, TimeSpan.FromHours(2)),
                To = new DateTimeOffset(2014, 11, 27, 0, 0, 0, TimeSpan.FromHours(2))
            };

            await this.WCFReadMeasurementsAsync(q, 13451);
        }

        [TestMethod]
        public void WCFReadMeasurementsTop100()
        {
            MeasurementQueryModel q = new MeasurementQueryModel()
            {
                Key = TestHelper.TestKey,
                Take = 100
            };

            this.WCFReadMeasurements(q, 100);
        }

        [TestMethod]
        public async Task WCFReadMeasurementsTop100Async()
        {
            int expected = 100;
            MeasurementQueryModel q = new MeasurementQueryModel()
            {
                Key = TestHelper.TestKey,
                Take = expected
            };

            MeasurementsServiceClient client = new MeasurementsServiceClient();

            Stopwatch sw = Stopwatch.StartNew();
            var result = await client.GetMeasurementsAsync(q);
            sw.Stop();
            client.Close();

            Trace.WriteLine(string.Format("GetMeasurements (async) with query {0} took {1} ms and returned {2} results.", GetQueryString(q), sw.ElapsedMilliseconds, result.Length));
            Assert.IsNotNull(result);
            Assert.AreEqual(expected, result.Length);
        }


        [TestMethod]
        public void WCFReadMeasurementsTop1000()
        {
            MeasurementQueryModel q = new MeasurementQueryModel()
            {
                Key = TestHelper.TestKey,
                Take = 1000
            };

            this.WCFReadMeasurements(q, 1000);
        }

        private void WCFReadMeasurements(MeasurementQueryModel q, int expectedCount)
        {
            MeasurementsServiceClient client = new MeasurementsServiceClient();

            Stopwatch sw = Stopwatch.StartNew();
            var result = client.GetMeasurements(q);
            sw.Stop();
            client.Close();

            Trace.WriteLine(string.Format("GetMeasurements with query {0} took {1} ms and returned {2} results.", GetQueryString(q), sw.ElapsedMilliseconds, result.Length));
            Assert.IsNotNull(result);
            Assert.AreEqual(expectedCount, result.Length);
        }

        private async Task WCFReadMeasurementsAsync(MeasurementQueryModel q, int expectedCount)
        {
            MeasurementsServiceClient client = new MeasurementsServiceClient();

            Stopwatch sw = Stopwatch.StartNew();
            var result = await client.GetMeasurementsAsync(q);
            sw.Stop();
            client.Close();

            Trace.WriteLine(string.Format("GetMeasurements with query {0} took {1} ms and returned {2} results.", GetQueryString(q), sw.ElapsedMilliseconds, result.Length));
            Assert.IsNotNull(result);
            Assert.AreEqual(expectedCount, result.Length);
        }


        private string GetQueryString(MeasurementQueryModel q)
        {
            return string.Format("\"From {0}, To {1}, Take {2}, Obj {3}, Tag {4}\"", q.From, q.To, q.Take, q.Obj, q.Tag);
        }

        [TestMethod]
        public void WCFSendMeasurementPackage100Measurements()
        {
            this.WCFSendMeasurements(100, 10, false, false);
        }

        [TestMethod]
        public void WCFSendMeasurementPackage500Measurements()
        {
            this.WCFSendMeasurements(500, 10, false, false);
        }

        [TestMethod]
        public void WCFSendMeasurementPackage200MeasurementsWithLocation()
        {
            this.WCFSendMeasurements(200, 10, true, false);
        }


        [TestMethod]
        public void WCFSendMeasurementPackage5MeasurementsWithFailure()
        {
            this.WCFSendMeasurements(5, 10, false, true);
        }

        [TestMethod]
        public void WCFSendMeasurementPackage100MeasurementsWithFailure()
        {
            this.WCFSendMeasurements(100, 10, false, true);
        }


      private void WCFSendMeasurements(int measurementsCount, int dataCount, bool useLocation, bool createFaultyMeasurements)
        {
            MeasurementsServiceClient client = new MeasurementsServiceClient();

            DateTimeOffset measTime = DateTimeOffset.Now;

            var package = measTime.GetMeasurementPackageWithXMeasurementsAndYData("WCF client", measurementsCount, dataCount, useLocation);

            if (createFaultyMeasurements)
            {
                var lastMeasurement = package.Measurements[package.Measurements.Length - 1];
                if (lastMeasurement.Data.Length > 1)
                {
                    lastMeasurement.Data[0].Tag = lastMeasurement.Data[1].Tag;
                }
            }

            Stopwatch sw = Stopwatch.StartNew();
            var result = client.SaveMeasurementPackage(package);
            sw.Stop();
            client.Close();
            Trace.WriteLine(string.Format("SaveMeasurementPackage with {0} measurement took {1} ms", package.Measurements.Length, sw.ElapsedMilliseconds));
            Assert.IsNotNull(result);
            if (createFaultyMeasurements)
            {
                Assert.IsFalse(result.Success);
                Assert.IsNotNull(result.Failures);
                foreach (var f in result.Failures)
                {
                    Trace.WriteLine(string.Format("{0}, {1}, {2}", f.Index, f.Code, f.Reason));
                }
            }
            else
            {
                Assert.IsTrue(result.Success);
                Assert.IsNull(result.Failures);
            }
            //Assert.AreEqual<int>(result.Statuses.Length, measurementsCount);
            //var item = result.Statuses[0];
            //Assert.IsTrue(item.IsSaved);
            //Assert.AreEqual<DateTimeOffset>(item.MeasurementTimeStamp, measTime);

        }

        public void SendMeasurementPackage()
        {
            // init the client
            MeasurementsServiceClient client = new MeasurementsServiceClient();
            // create the measurement package and populate it
            MeasurementPackage package = new MeasurementPackage();
            package.Key = "your-key-goes-here";

            // create a collection of measurements
            List<MeasurementModel> measurements = new List<MeasurementModel>();
            // create a measurement
            MeasurementModel m = new MeasurementModel()
            {
                Object = "your-measurement-object",
                Tag = "your-measurement-tag",
                Timestamp = DateTimeOffset.Now,
                Location = new Location() 
                {
                    Latitude = 62.8989,
                    Longitude = 27.6630
                }
            };
            // add some data to your measurement
            DataModel d = new DataModel()
            {
                Tag = "your-data-tag",
                Value = 3.14
            };
            // add data to measurement
            m.Data = new DataModel[1];
            m.Data[0] = d;
            // add measurement to measurements collection
            measurements.Add(m);
            // add measurements collection to measurement package
            package.Measurements = measurements.ToArray();
            // save the measurement package and check the result
            SaveResult result = client.SaveMeasurementPackage(package);
        }

    }
}
