using Microsoft.VisualStudio.TestTools.UnitTesting;
using Savonia.Measurements.Providers.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestClients
{
    [TestClass]
    public class PluginTests
    {
        public static Logger Log = new Logger();

        [TestMethod]
        public void OPCUAPluginProcess()
        {
            OPCUAPlugin.MeasurerPlugin plugin = new OPCUAPlugin.MeasurerPlugin();
            plugin.ConfigurationFile = @"your configuration file";
            plugin.Name = "OPCUAlugin Test";
            plugin.Log = Log;
            plugin.LoadConfig();
            plugin.ProcessCompleted += plugin_ProcessCompleted;

            plugin.Process();
        }

        [TestMethod]
        public void CsvPluginProcess()
        {
            CsvReaderPlugin.MeasurerPlugin plugin = new CsvReaderPlugin.MeasurerPlugin();
            plugin.ConfigurationFile = @"your configuration file";
            plugin.Name = "Csv Plugin Test";
            plugin.Log = Log;
            plugin.LoadConfig();
            plugin.ProcessCompleted += plugin_ProcessCompleted;

            plugin.Process();
        }


        [TestMethod]
        public void PilotMeasurementPluginProcess()
        {
            PilotMeasurementPlugin.PilotMeasurementPlugin plugin = new PilotMeasurementPlugin.PilotMeasurementPlugin();
            plugin.ConfigurationFile = @"your configuration file";
            plugin.Name = "Pilot Plugin Test";
            plugin.Log = Log;
            plugin.LoadConfig();
            plugin.ProcessCompleted += plugin_ProcessCompleted;

            plugin.Process();
        }

        void plugin_ProcessCompleted(object sender, Savonia.Measurements.Providers.Models.ProcessCompletedEventArgs e)
        {
            var p = e.MeasurementPackage;
            Trace.WriteLine(p.Key);
            foreach (var item in p.Measurements)
            {
                Trace.WriteLine(string.Format("{0}: {1}/{2}", item.Timestamp, item.Object, item.Tag));
                foreach (var v in item.Data)
                {
                    Trace.WriteLine(string.Format("\t{0} = {1}", v.Tag, v.Value));
                }
            }
        }


    }
}
