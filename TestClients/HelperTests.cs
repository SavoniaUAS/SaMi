using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Savonia.Measurements.Providers.Models;
using Savonia.Measurements.Database.Helpers;
using System.Diagnostics;
using Savonia.Measurements.Models;
using Savonia.Measurements.Models.Helpers;
using System.Data.Entity.Spatial;

namespace TestClients
{
    [TestClass]
    public class HelperTests
    {
        [TestMethod]
        public void SafeFileNameTest1()
        {
            string orig = "www.www:www123";
            string expected = "www.wwwwww123";
            string actual = orig.GetSafeFileName();
            Trace.WriteLine(string.Format("Actual: {0}", actual));

            Assert.AreEqual(actual, expected);
        }

        [TestMethod]
        public void SafeFileNameTest2()
        {
            string orig = "www.www:www123";
            string expected = "www.www-www123";
            string actual = orig.GetSafeFileName('-');
            Trace.WriteLine(string.Format("Actual: {0}", actual));

            Assert.AreEqual(actual, expected);
        }

        [TestMethod]
        public void LocationToDBGeography()
        {
            Location orig = new Location()
            {
                Latitude = 91,
                Longitude = 27
            };
            try
            {
                var actual = orig.ToGeography();
                Assert.AreEqual(actual.Latitude, orig.Latitude);
                Assert.AreEqual(actual.Longitude, orig.Longitude);
            }
            catch (Exception ex)
            {
                Trace.TraceError(ex.FriendlyException());
                Assert.Fail();
            }


        }

    }
}
