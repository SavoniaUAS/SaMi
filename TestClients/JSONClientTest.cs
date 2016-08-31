using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Net;
using System.IO;
using System.Runtime.Serialization.Json;
using TestClients.MeasurementsService;
using System.Diagnostics;

namespace TestClients
{
    [TestClass]
    public class JSONClientTest
    {
        [TestMethod]
        public void JSONSendMeasurementPackage()
        {
            DateTimeOffset measTime = DateTimeOffset.Now;
            var package = measTime.GetMeasurementPackage("JSON client");

            
            var httpWebRequest = (HttpWebRequest)WebRequest.Create(TestHelper.JSONSaveMeasurementPackageUri);
            httpWebRequest.ContentType = "text/json";
            httpWebRequest.Method = "POST";

            DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(MeasurementPackage));
            ser.WriteObject(httpWebRequest.GetRequestStream(), package);

            try
            {
                var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                ser = new DataContractJsonSerializer(typeof(SaveResult));
                SaveResult result = (SaveResult)ser.ReadObject(httpResponse.GetResponseStream());

                Assert.IsNotNull(result);
                Assert.IsTrue(result.Success);
                Assert.IsNull(result.Failures);
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
        public void JSONSendMeasurementPackageWithISO()
        {
            DateTime dt = DateTime.Now;
            var package = dt.GetMeasurementPackageWithISO("JSON Client ISO");


            var httpWebRequest = (HttpWebRequest)WebRequest.Create(TestHelper.JSONSaveMeasurementPackageUri);
            httpWebRequest.ContentType = "text/json";
            httpWebRequest.Method = "POST";

            DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(MeasurementPackage));
            ser.WriteObject(httpWebRequest.GetRequestStream(), package);

            try
            {
                var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                ser = new DataContractJsonSerializer(typeof(SaveResult));
                SaveResult result = (SaveResult)ser.ReadObject(httpResponse.GetResponseStream());

                Assert.IsNotNull(result);
                Assert.IsTrue(result.Success);
                Assert.IsNull(result.Failures);
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

        /// <summary>
        /// not working
        /// </summary>
        public void JSONSendMeasurementPackageWithIsoAndLongTextBinaryXmlValues()
        {
            DateTimeOffset dt = DateTimeOffset.Now;
            var package = dt.GetMeasurementPackageWithXMeasurementsAndYDataWithTextValue("testData", 20, 10, true);


            var httpWebRequest = (HttpWebRequest)WebRequest.Create(TestHelper.JSONSaveMeasurementPackageUri);
            httpWebRequest.ContentType = "text/json";
            httpWebRequest.Method = "POST";

            DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(MeasurementPackage));
            ser.WriteObject(httpWebRequest.GetRequestStream(), package);

            try
            {
                var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                ser = new DataContractJsonSerializer(typeof(SaveResult));
                SaveResult result = (SaveResult)ser.ReadObject(httpResponse.GetResponseStream());

                Assert.IsNotNull(result);
                Assert.IsTrue(result.Success);
                Assert.IsNull(result.Failures);
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
        public void JSONSendMeasurementPackageWith20Measurements()
        {
            DateTimeOffset dt = DateTimeOffset.Now;
            var package = dt.GetMeasurementPackageWithXMeasurementsAndYData("testData", 20, 10, true);


            var httpWebRequest = (HttpWebRequest)WebRequest.Create(TestHelper.JSONSaveMeasurementPackageUri);
            httpWebRequest.ContentType = "text/json";
            httpWebRequest.Method = "POST";

            DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(MeasurementPackage));
            ser.WriteObject(httpWebRequest.GetRequestStream(), package);

            try
            {
                var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                ser = new DataContractJsonSerializer(typeof(SaveResult));
                SaveResult result = (SaveResult)ser.ReadObject(httpResponse.GetResponseStream());

                Assert.IsNotNull(result);
                Assert.IsTrue(result.Success);
                Assert.IsNull(result.Failures);
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
        public void JSONSendMeasurementPackageFaultyLocation()
        {
            DateTimeOffset measTime = DateTimeOffset.Now;
            var package = measTime.GetMeasurementPackage("JSON client");
            package.Measurements[0].Location.Latitude = 91; // set invalid latitude

            var httpWebRequest = (HttpWebRequest)WebRequest.Create(TestHelper.JSONSaveMeasurementPackageUri);
            httpWebRequest.ContentType = "text/json";
            httpWebRequest.Method = "POST";

            DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(MeasurementPackage));
            ser.WriteObject(httpWebRequest.GetRequestStream(), package);

            try
            {
                var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                ser = new DataContractJsonSerializer(typeof(SaveResult));
                SaveResult result = (SaveResult)ser.ReadObject(httpResponse.GetResponseStream());
                Assert.Fail(); // service should throw
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
                Assert.IsTrue(content.Contains("24201: Latitude values must be between -90 and 90 degrees."));
            }
        }
    }
}
