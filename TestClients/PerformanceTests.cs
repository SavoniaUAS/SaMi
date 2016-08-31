using Microsoft.VisualStudio.TestTools.UnitTesting;
using Savonia.Measurements.Database;
using Savonia.Measurements.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;

namespace TestClients
{
    [TestClass]
    public class PerformanceTests
    {
        private const string readKey = "Your-Key-Here";

        [TestMethod]
        public async Task ReadMeasurementsAsync()
        {
            using (Repository r = new Repository())
            {
                var query = new MeasurementQueryModel()
                {
                    Key = readKey,
                    From = new DateTimeOffset(2015, 10, 25, 0, 0, 0, TimeSpan.FromHours(3)),
                    To = new DateTimeOffset(2015, 10, 26, 0, 0, 0, TimeSpan.FromHours(2)),
                    Take = null
                };
                Stopwatch sw = Stopwatch.StartNew();
                var result = await r.GetMeasurementsAsync(query);
                sw.Stop();

                int expected = 79365;

                Trace.WriteLine(string.Format("Got {0} results in {1}.", result.Count, sw.Elapsed));
                var f = result.First();
                var l = result.Last();
                Trace.WriteLine(string.Format("First {0} and last {1}.", f.Timestamp, l.Timestamp));
                Assert.IsNotNull(result);
                Assert.AreEqual<int>(expected, result.Count);
                Assert.AreEqual<string>("25.10.2015 23:59:59 +02:00", f.Timestamp.ToString());
                Assert.AreEqual<string>("25.10.2015 0:00:01 +03:00", l.Timestamp.ToString());
            }
        }

        [TestMethod]
        public async Task ReadMeasurementsWithDataAsync()
        {
            using (Repository r = new Repository())
            {
                var query = new MeasurementQueryModel()
                {
                    Key = readKey,
                    From = new DateTimeOffset(2015, 10, 25, 0, 0, 0, TimeSpan.FromHours(3)),
                    To = new DateTimeOffset(2015, 10, 26, 0, 0, 0, TimeSpan.FromHours(2)),
                    Take = null
                };
                Stopwatch sw = Stopwatch.StartNew();
                var result = await r.GetMeasurementsAsync(query, true);
                sw.Stop();

                int expected = 79365;

                Trace.WriteLine(string.Format("Got {0} results with data in {1}.", result.Count, sw.Elapsed));
                var f = result.First();
                var tenth = result[9];
                var l = result.Last();
                var fV = f.Data.Single(d => d.Tag == "CU2_F1_PILOT");
                var tV = tenth.Data.Single(d => d.Tag == "CU2_T2_PILOT");
                var lV = l.Data.Single(d => d.Tag == "L2_T2_PILOT");
                Trace.WriteLine(string.Format("First ({0}) {1}, 10th ({2}) {3} and last ({4}) {5}.", f.ID, f.Timestamp, tenth.ID, tenth.Timestamp, l.ID, l.Timestamp));
                Trace.WriteLine(string.Format("1st: {0} = {1}, 10th: {2} = {3}, last: {4} = {5}", fV.Tag, fV.Value, tV.Tag, tV.Value, lV.Tag, lV.Value));
                Assert.IsNotNull(result);
                Assert.AreEqual<int>(expected, result.Count);
                Assert.AreEqual<string>("25.10.2015 23:59:59 +02:00", f.Timestamp.ToString());
                Assert.AreEqual<string>("25.10.2015 0:00:01 +03:00", l.Timestamp.ToString());
                Assert.AreEqual<string>("25.10.2015 23:59:50 +02:00", tenth.Timestamp.ToString());
                Assert.AreEqual<double>(4.118750095367, fV.Value.Value);
                Assert.AreEqual<double>(17.1015625, tV.Value.Value);
                Assert.AreEqual<double>(15.53515625, lV.Value.Value);
                Assert.IsNull(fV.XmlValue);
                Assert.IsNull(tV.BinaryValue);
                Assert.IsNull(lV.TextValue);
            }
        }

        [TestMethod]
        public async Task ReadMeasurementsWithDataTake200Async()
        {
            using (Repository r = new Repository())
            {
                var query = new MeasurementQueryModel()
                {
                    Key = readKey,
                    From = new DateTimeOffset(2015, 10, 25, 0, 0, 0, TimeSpan.FromHours(3)),
                    To = new DateTimeOffset(2015, 10, 26, 0, 0, 0, TimeSpan.FromHours(2)),
                    Take = 200
                };
                Stopwatch sw = Stopwatch.StartNew();
                var result = await r.GetMeasurementsAsync(query, true);
                sw.Stop();

                int expected = 200;

                Trace.WriteLine(string.Format("Got {0} results with data in {1}.", result.Count, sw.Elapsed));
                var f = result.First();
                var tenth = result[9];
                var l = result.Last();
                var fV = f.Data.Single(d => d.Tag == "CU2_F1_PILOT");
                var tV = tenth.Data.Single(d => d.Tag == "CU2_T2_PILOT");
                var lV = l.Data.Single(d => d.Tag == "L2_T2_PILOT");
                Trace.WriteLine(string.Format("First ({0}) {1}, 10th ({2}) {3} and last ({4}) {5}.", f.ID, f.Timestamp, tenth.ID, tenth.Timestamp, l.ID, l.Timestamp));
                Trace.WriteLine(string.Format("1st: {0} = {1}, 10th: {2} = {3}, last: {4} = {5}", fV.Tag, fV.Value, tV.Tag, tV.Value, lV.Tag, lV.Value));
                Assert.IsNotNull(result);
                Assert.AreEqual<int>(expected, result.Count);
                Assert.AreEqual<string>("25.10.2015 23:59:59 +02:00", f.Timestamp.ToString());
                Assert.AreEqual<string>("25.10.2015 23:56:40 +02:00", l.Timestamp.ToString());
                Assert.AreEqual<string>("25.10.2015 23:59:50 +02:00", tenth.Timestamp.ToString());
                Assert.AreEqual<double>(4.118750095367, fV.Value.Value);
                Assert.AreEqual<double>(17.1015625, tV.Value.Value);
                Assert.AreEqual<double>(16.29296875, lV.Value.Value);
                Assert.IsNull(fV.XmlValue);
                Assert.IsNull(tV.BinaryValue);
                Assert.IsNull(lV.TextValue);
            }
        }

        [TestMethod]
        public async Task ReadMeasurementsWithBinaryDataAsync()
        {
            using (Repository r = new Repository())
            {
                var query = new MeasurementQueryModel()
                {
                    Key = "Your-Key-here"
                };
                Stopwatch sw = Stopwatch.StartNew();
                var result = await r.GetMeasurementsAsync(query, true);
                sw.Stop();

                int expected = 20;

                Trace.WriteLine(string.Format("Got {0} results with data in {1}.", result.Count, sw.Elapsed));
                var f = result.First();
                var wanted = result.Single(d => d.ID == 18);
                var im = wanted.Data.Single(d => d.Tag == "Image");
                var l = result.Last();
                Trace.WriteLine(string.Format("First ({0}) {1}, 10th ({2}) {3} and last ({4}) {5}.", f.ID, f.Timestamp, wanted.ID, wanted.Timestamp, l.ID, l.Timestamp));
                wanted.Data.ForEach(d =>
                {
                    Trace.WriteLine(string.Format("{0}: Value = {1}, Text = {2}, Has binary = {3}", d.Tag, d.Value, d.TextValue, null != d.BinaryValue));
                });
                Assert.IsNotNull(result);
                Assert.AreEqual<int>(expected, result.Count);
                //Assert.AreEqual<string>("25.10.2015 23:59:59 +02:00", f.Timestamp.ToString());
                //Assert.AreEqual<string>("25.10.2015 0:00:01 +03:00", l.Timestamp.ToString());
                //Assert.AreEqual<string>("25.10.2015 23:59:50 +02:00", tenth.Timestamp.ToString());
                Assert.AreEqual<string>("image/jpeg", im.TextValue);
                Assert.IsNotNull(im.BinaryValue);
                //if (null != im.BinaryValue)
                //{
                //    System.IO.File.WriteAllBytes(@"c:\temp\img.jpg", im.BinaryValue);
                //}

            }
        }

        [TestMethod]
        public void JSONReadMeasurementsTake20()
        {
            string key = readKey;
            string from = "2015-10-25T00:00:00";
            string to = "2015-10-26T00:00:00";
            int? take = 20;
            var uri = string.Format("Your-Measurement-service-uri", key, from, to, take);
            Trace.WriteLine(string.Format("Requested: {0}", uri));
            var httpWebRequest = (HttpWebRequest)WebRequest.Create(uri);
            httpWebRequest.ContentType = "text/json";
            httpWebRequest.Method = "GET";

            try
            {
                var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                var ser = new DataContractJsonSerializer(typeof(List<MeasurementModel>));
                List<MeasurementModel> result = (List<MeasurementModel>)ser.ReadObject(httpResponse.GetResponseStream());

                Assert.IsNotNull(result);
                result.ForEach(r =>
                {
                    Trace.WriteLine(string.Format("ID: {0} @ {1} with {2} data.", r.ID, r.Timestamp, r.Data.Count));
                });
                //Assert.IsTrue(result.Success);
                //Assert.IsNull(result.Failures);
            }
            catch (System.Net.WebException ex)
            {
                string content = string.Empty;
                if (null != ex.Response && ex.Response.ContentLength > 0)
                {
                    using (StreamReader sr = new StreamReader(ex.Response.GetResponseStream()))
                    {
                        content = sr.ReadToEnd();
                    }
                }

                Trace.TraceError("{1}{0}{0}{2}", Environment.NewLine, ex.Message, content);

                Assert.Fail();
            }
        }

        [TestMethod]
        public void JSONReadMeasurementsTake2000()
        {
            string key = readKey;
            string from = "2015-10-25T00:00:00";
            string to = "2015-10-26T00:00:00";
            int take = 2000;
            var uri = string.Format("Your-Measurement-service-uri", key, from, to, take);
            Trace.WriteLine(string.Format("Requested: {0}", uri));
            var httpWebRequest = (HttpWebRequest)WebRequest.Create(uri);
            httpWebRequest.ContentType = "text/json";
            httpWebRequest.Method = "GET";

            try
            {
                var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                var ser = new DataContractJsonSerializer(typeof(List<MeasurementModel>));
                List<MeasurementModel> result = (List<MeasurementModel>)ser.ReadObject(httpResponse.GetResponseStream());

                Assert.IsNotNull(result);
                //result.ForEach(r =>
                //{
                //    Trace.WriteLine(string.Format("ID: {0} @ {1} with {2} data.", r.ID, r.Timestamp, r.Data.Count));
                //});
                Trace.WriteLine(string.Format("Results count {0}.", result.Count));
                Assert.AreEqual<int>(result.Count, take);
                var f = result.First();
                Assert.IsNotNull(f);
                Assert.AreEqual<int>(43, f.Data.Count);
            }
            catch (System.Net.WebException ex)
            {
                string content = string.Empty;
                if (null != ex.Response && ex.Response.ContentLength > 0)
                {
                    using (StreamReader sr = new StreamReader(ex.Response.GetResponseStream()))
                    {
                        content = sr.ReadToEnd();
                    }
                }

                Trace.TraceError("{1}{0}{0}{2}", Environment.NewLine, ex.Message, content);

                Assert.Fail();
            }
        }
    }
}
