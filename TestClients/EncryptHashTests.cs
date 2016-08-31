using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Savonia.Measurements.Database.Helpers;
using System.Diagnostics;
using System.Collections.Generic;

namespace TestClients
{
    [TestClass]
    public class EncryptHashTests
    {
        [TestMethod]
        public void HashTimer()
        {
            string data = "testkey";
            Stopwatch sw = Stopwatch.StartNew();
            string hash = data.Hash();
            sw.Stop();
            Trace.WriteLine(string.Format("Hash of {0} is {1}. Hashing took {2} ms", data, hash, sw.Elapsed.TotalMilliseconds));
        }

        [TestMethod]
        public void HashTimer2()
        {
            string data = "fdsaflkj#3239d3324dsaASD";
            Stopwatch sw = Stopwatch.StartNew();
            string hash = data.Hash();
            sw.Stop();
            Trace.WriteLine(string.Format("Hash of {0} is {1}. Hashing took {2} ms", data, hash, sw.Elapsed.TotalMilliseconds));
        }

        [TestMethod]
        public void EncryptTimer()
        {
            string data = "weqr ewöildsfa iewqffsdafdsa";
            Stopwatch sw = Stopwatch.StartNew();
            string en = data.Encrypt();
            sw.Stop();
            Trace.WriteLine(string.Format("Encrypt of {0} is {1}. Encryption took {2} ms", data, en, sw.Elapsed.TotalMilliseconds));
        }

        [TestMethod]
        public void EncryptTexts()
        {
            string[] source = new string[] 
            { 
                "20148D1AAC8E097A55C09-30",
                "jam",
                "test",
                "2014323DA4B15287C10-09",
                "test",
                "201428A241EBA7E8110-16",
                "20145F112A2CA65DA11-26",
                "20145F91EF01B892711-26",
                "2014242964B5D7ED311-27"
            };
            

            foreach (var item in source)
            {
                Trace.WriteLine(string.Format("{0} = {1}", item, item.Encrypt()));
            }
        }

    }
}
