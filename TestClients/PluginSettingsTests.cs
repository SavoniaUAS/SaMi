using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Savonia.Measurements.Providers.MeasurerService.Models;
using Savonia.Measurements.Providers.Models;
using System.Diagnostics;
using MeasurementPluginSample;
using Savonia.Measurements.Models;
using RemoteMXPlugin;
using OPCUAPlugin;
using CsvReaderPlugin;

namespace TestClients
{
    [TestClass]
    public class PluginSettingsTests
    {
        [TestMethod]
        public void CreatePluginSettingsTemplate()
        {
            PluginSettings ps = new PluginSettings()
            {
                MinimumExecutionInterval = 10,
                Plugins = new List<MeasurerProcessPlugin>()
            };

            MeasurerProcessPlugin mpp = new MeasurerProcessPlugin()
            {
                ConfigFile = "config_file_path",
                Description = "description of plugin",
                Enabled = true,
                ExecuteInterval = 20,
                ExecuteOnServiceStart = false,
                Name = "plugin name",
                PluginAssembly = "plugin_dll_path"
            };

            ps.Plugins.Add(mpp);

            mpp = new MeasurerProcessPlugin()
            {
                ConfigFile = "config_file_path_2",
                Description = "description of 2nd plugin",
                Enabled = true,
                ExecuteInterval = 110,
                ExecuteOnServiceStart = false,
                Name = "plugin 2 name",
                PluginAssembly = "plugin2_dll_path"
            };

            ps.Plugins.Add(mpp);


            string xml = SerializeHelper.SerializeToString<PluginSettings>(ps);
            Trace.WriteLine(xml);

            Assert.IsNotNull(xml);

            PluginSettings dps = SerializeHelper.Deserialize<PluginSettings>(xml);

            Assert.IsNotNull(dps);
            Assert.AreEqual(dps.MinimumExecutionInterval, ps.MinimumExecutionInterval);
            Assert.AreEqual(dps.Plugins.Count, ps.Plugins.Count);

        }

        [TestMethod]
        public void CreateDemoMeasurerPluginSettingsTemplate()
        {
            DemoPluginSettings ps = new DemoPluginSettings()
            {
                Key = "key here",
                SensorTags = new List<string>()
            };

            ps.SensorTags.Add("tag1");
            ps.SensorTags.Add("tag2");


            string xml = SerializeHelper.SerializeToString<DemoPluginSettings>(ps);
            Trace.WriteLine(xml);

            Assert.IsNotNull(xml);

            DemoPluginSettings dps = SerializeHelper.Deserialize<DemoPluginSettings>(xml);

            Assert.IsNotNull(dps);
            Assert.AreEqual(dps.Key, ps.Key);
            Assert.AreEqual(dps.SensorTags.Count, ps.SensorTags.Count);

        }

        [TestMethod]
        public void CreateRemoteMXPluginSettingsTemplate()
        {
            RemoteMXPluginSettings ps = new RemoteMXPluginSettings()
            {
                SamiKey = "key here",
                ChannelMap = new Dictionary<string, string>(),
                MeasurementObject = "measurement object here if needed",
                MeasurementTag = "measurement tag here if needed",
                Password = "Remote mx service password",
                UserName = "Remote mx service username",
                SiteId = 0,
                ReadConfigPath = "path to read config file, which contains newest read measurement time",
                SaveMappedValuesOnly = true,
                MaxTimeToReadAtOnce = TimeSpan.FromHours(26.5)
            };

            ps.ChannelMap.Add("rmx-channel-1", "sami-tag1");
            ps.ChannelMap.Add("rmx-channel-2", "sami-tag2");


            string xml = SerializeHelper.SerializeToStringWithDCS<RemoteMXPluginSettings>(ps);
            Trace.WriteLine(xml);

            Assert.IsNotNull(xml);

            RemoteMXPluginSettings dps = SerializeHelper.DeserializeWithDCS<RemoteMXPluginSettings>(xml);

            Assert.IsNotNull(dps);
            Assert.AreEqual(dps.SamiKey, ps.SamiKey);
            Assert.AreEqual(dps.ChannelMap.Count, ps.ChannelMap.Count);
        }

        [TestMethod]
        public void CreateRemoteMXReadConfigTemplate()
        {
            var d = new DateTime(2015, 11, 10, 16, 0, 0);
            RemoteMXPlugin.ReadConfig rc = new RemoteMXPlugin.ReadConfig()
            {
                LatestValueRead = d
            };


            string xml = SerializeHelper.SerializeToStringWithDCS<RemoteMXPlugin.ReadConfig>(rc);
            Trace.WriteLine(xml);

            Assert.IsNotNull(xml);

            RemoteMXPlugin.ReadConfig dps = SerializeHelper.DeserializeWithDCS<RemoteMXPlugin.ReadConfig>(xml);

            Assert.IsNotNull(dps);
            Assert.AreEqual(dps.LatestValueRead, rc.LatestValueRead);
        }

        [TestMethod]
        public void ReadAndDeserializeRemoteMXPluginSettingsFile()
        {
            var xml = System.IO.File.ReadAllText(@"Path to settings file");
            Trace.WriteLine(xml);

            Assert.IsNotNull(xml);

            RemoteMXPluginSettings dps = SerializeHelper.DeserializeWithDCS<RemoteMXPluginSettings>(xml);

            Assert.IsNotNull(dps);
            Assert.AreEqual(dps.SamiKey, "key here");
            Assert.AreEqual(dps.ChannelMap.Count, 2);
            Assert.AreEqual<TimeSpan>(TimeSpan.FromDays(1), dps.MaxTimeToReadAtOnce);

        }

        [TestMethod]
        public void CreateOpcUAPluginSettingsTemplate()
        {
            OpcUAPluginSettings ps = new OpcUAPluginSettings()
            {
                SamiKey = "key here",
                MeasurementObject = "measurement object here if needed",
                MeasurementTag = "measurement tag here if needed",
                Password = "Opc XML DA service password",
                UserName = "Opc XML DA service username",
                NamespaceUri = "S7COM:",
                OpcUAServiceUrl = "opc.tcp://opc-server-here:4845",
                OPCUAItemMap = new List<OPCUAItem>()
            };

            ps.OPCUAItemMap.Add(new OPCUAItem() { Identifier = "opc-item-name 1", SamiTag = "sami-tag 1", Scale = new OPCUAPlugin.ValueLinearScale() { Destination = new OPCUAPlugin.LinearScaleLimit() { From = 0m, To = 10m }, Source = new OPCUAPlugin.LinearScaleLimit() { From = 0m, To = 4095m } } });
            ps.OPCUAItemMap.Add(new OPCUAItem() { Identifier = "opc-item-name 2", SamiTag = "sami-tag 2", Scale = new OPCUAPlugin.ValueLinearScale() { Destination = new OPCUAPlugin.LinearScaleLimit() { From = 0m, To = 30m }, Source = new OPCUAPlugin.LinearScaleLimit() { From = 0m, To = 10000m } } });

            string xml = SerializeHelper.SerializeToStringWithDCS<OpcUAPluginSettings>(ps);
            Trace.WriteLine(xml);

            Assert.IsNotNull(xml);

            OpcUAPluginSettings dps = SerializeHelper.DeserializeWithDCS<OpcUAPluginSettings>(xml);

            Assert.IsNotNull(dps);
            Assert.AreEqual(dps.SamiKey, ps.SamiKey);
            Assert.AreEqual(dps.OPCUAItemMap.Count, ps.OPCUAItemMap.Count);
        }

        [TestMethod]
        public void ReadAndDeserializeOpcUAPluginSettingsFile()
        {
            var xml = System.IO.File.ReadAllText(@"Path to settings file");
            Trace.WriteLine(xml);

            Assert.IsNotNull(xml);

            OpcUAPluginSettings dps = SerializeHelper.DeserializeWithDCS<OpcUAPluginSettings>(xml);

            Assert.IsNotNull(dps);
            Assert.AreEqual(dps.SamiKey, "key here");
            Assert.AreEqual(dps.OPCUAItemMap.Count, 2);
            var firstItemMap = dps.OPCUAItemMap.First();
            var lastItemMap = dps.OPCUAItemMap.Last();
            Assert.IsNotNull(firstItemMap.Scale);
            Assert.IsNull(lastItemMap.Scale);
            Assert.AreEqual<double>(firstItemMap.Scale.ScaleValue(1), 0.1);

        }



        [TestMethod]
        public void CreateCsvPluginSettingsTemplate()
        {
            CsvReaderPluginSettings ps = new CsvReaderPluginSettings()
            {
                SamiKey = "key here",
                MeasurementObject = "measurement object here if needed",
                MeasurementTag = "measurement tag here if needed",
                Password = "csv web client password",
                UserName = "csv web client username",
                ContentEncoding = "UTF-8",
                HasHeaders = true,
                CsvUri = "https://testi.com",
                DelimeterChar = ";",
                IsWebResource = true,
                MaxTimeToReadAtOnce = TimeSpan.FromHours(1.5),
                MeasurementTimeFieldHeader = "time",
                QuotationChar = '"',
                ReadConfigPath = "[path to read config]",
                AdditionalHttpHeaders = new List<string>(),
                ItemMap = new List<CsvItem>()
            };

            ps.AdditionalHttpHeaders.Add("header 1");
            ps.AdditionalHttpHeaders.Add("header 2");
            ps.ItemMap.Add(new CsvItem() { Header = "item 1", SamiTag = "sami tag 1", Scale = new CsvReaderPlugin.ValueLinearScale() { Source = new CsvReaderPlugin.LinearScaleLimit() { From = 0, To = 10 }, Destination = new CsvReaderPlugin.LinearScaleLimit() { From = 0, To = 100 } } });
            ps.ItemMap.Add(new CsvItem() { Header = "item 2", SamiTag = "sami tag 2" });

            string xml = SerializeHelper.SerializeToStringWithDCS<CsvReaderPluginSettings>(ps);
            Trace.WriteLine(xml);

            Assert.IsNotNull(xml);

            CsvReaderPluginSettings dps = SerializeHelper.DeserializeWithDCS<CsvReaderPluginSettings>(xml);

            Assert.IsNotNull(dps);
            Assert.AreEqual(dps.SamiKey, ps.SamiKey);
            Assert.AreEqual(dps.ItemMap.Count, ps.ItemMap.Count);
        }

        [TestMethod]
        public void ReadAndDeserializeCsvPluginSettingsFile()
        {
            var xml = System.IO.File.ReadAllText(@"Path to settings file");
            Trace.WriteLine(xml);

            Assert.IsNotNull(xml);

            CsvReaderPluginSettings dps = SerializeHelper.DeserializeWithDCS<CsvReaderPluginSettings>(xml);

            Assert.IsNotNull(dps);
            Assert.AreEqual(@"known csv uri", dps.CsvUri);
            Assert.AreEqual(dps.SamiKey, "key here");
            Assert.AreEqual(dps.ItemMap.Count, 2);
            var firstItemMap = dps.ItemMap.First();
            var lastItemMap = dps.ItemMap.Last();
            Assert.IsNotNull(firstItemMap.Scale);
            Assert.IsNull(lastItemMap.Scale);
            Assert.AreEqual<double>(firstItemMap.Scale.ScaleValue(10), 100.0);

        }


        [TestMethod]
        public void LocalStoreSerializationTest()
        {
            Random rnd = new Random();
            MeasurementPackage p = new MeasurementPackage()
            {
                Key = "testing...",
                Measurements = new List<MeasurementModel>()
            };

            MeasurementModel mm = new MeasurementModel()
            {
                Object = "testing",
                Timestamp = DateTimeOffset.Now,
                Data = new List<DataModel>()
            };
            string[] tags = new string[] { "tag1", "tag2" };
            foreach (var item in tags)
            {
                mm.Data.Add(new DataModel()
                {
                    Tag = item,
                    Value = rnd.NextDouble() * 100
                });
            }

            p.Measurements.Add(mm);

            string xml = SerializeHelper.SerializeToStringWithDCS<MeasurementPackage>(p);
            Trace.WriteLine(xml);

            Assert.IsNotNull(xml);

            MeasurementPackage dps = SerializeHelper.DeserializeWithDCS<MeasurementPackage>(xml);

            Assert.IsNotNull(dps);
            Assert.AreEqual(dps.Key, p.Key);
            Assert.AreEqual(dps.Measurements.Count, p.Measurements.Count);

        }

    }
}
