using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Savonia.Measurements.Providers.Models;
using Savonia.Measurements.Models;
using System.Diagnostics;
using Siemens.OpcUA;
using Opc.Ua;

namespace OPCUAPlugin
{
    /// <summary>
    /// Your plugin must be a public class and it must either directly implements Savonia.Measurements.Providers.Models.IMeasurer interface 
    /// or derive a class that implements the interface.
    /// </summary>
    public class MeasurerPlugin : IMeasurer
    {
        /// <summary>
        /// The MeasurerService listens to this event.
        /// </summary>
        public event EventHandler<ProcessCompletedEventArgs> ProcessCompleted;

        /// <summary>
        /// The MeasurerService sets ConfigurationFile value from plugin's config ConfigFile element when initializing this plugin.
        /// </summary>
        public string ConfigurationFile { get; set; }
        /// <summary>
        /// The MeasurerService sets Name value from plugin's config Name element when initializing this plugin.
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// The MeasurerService sets Log to a Logger object which can be used to log things.
        /// </summary>
        public Logger Log { get; set; }

        // Your plugin can have its own variables as needed.
        private OpcUAPluginSettings settings;


        private Server m_Server = null;
        private UInt16 m_NameSpaceIndex = 0;

        /// <summary>
        /// The MeasurerService calls LoadConfig method after the properties are set.
        /// </summary>
        public void LoadConfig()
        {
            // Do your plugin configs here if needed. The ConfigurationFile does not need to be a file! For the MeasurerService it is a string.
            if (!string.IsNullOrEmpty(this.ConfigurationFile))
            {
                // when accessing files note that the default working directory is the one where the service exe file is.
                // you can use GetPluginRootedPath() extension method to get rooted path for plugins folder.
                settings = SerializeHelper.DeserializeWithDCS<OpcUAPluginSettings>(System.IO.File.ReadAllText(this.ConfigurationFile.GetPluginRootedPath()));
            }
        }

        void opcUaServer_CertificateEvent(CertificateValidator validator, CertificateValidationEventArgs e)
        {
            // Accept all certificate -> better ask user
            e.Accept = true;
        }

        /// <summary>
        /// The MeasurerService calls Process method periodically as defined in plugin's config ExecuteInterval element. The time is seconds.
        /// Note that the same instance of this class is alive the whole time as the service is alive! So make this process as sleek and lean as possible.
        /// </summary>
        public void Process()
        {
            m_Server = new Server();
            m_Server.CertificateEvent += new certificateValidation(opcUaServer_CertificateEvent);
            // Connect with URL from Server URL
            m_Server.Connect(this.settings.OpcUAServiceUrl);
            ReadNamespaceFromServer();

            // read values

            NodeIdCollection nodesToRead = new NodeIdCollection();
            DataValueCollection results;
            int expectedResultsCount = this.settings.OPCUAItemMap.Count;
            Dictionary<string, OPCUAItem> itemMap = new Dictionary<string, OPCUAItem>(expectedResultsCount);
            this.settings.OPCUAItemMap.ForEach(m =>
            {
                itemMap.Add(m.Identifier, m);
                nodesToRead.Add(new NodeId(m.Identifier, m_NameSpaceIndex));
            });

            // Read the values
            m_Server.ReadValues(nodesToRead, out results);

            if (results.Count != expectedResultsCount)
            {
                throw new Exception("Reading value returned unexptected number of result");
            }

            //// we have data!

            // Do the measurement reading here... and then create a MeasurementPackage object with a proper key to enable saving measurements to SAMI system.
            MeasurementPackage p = new MeasurementPackage()
            {
                Key = this.settings.SamiKey,
                Measurements = new List<MeasurementModel>(results.Count)
            };

            MeasurementModel mm = null;
            mm = new MeasurementModel()
            {
                Object = settings.MeasurementObject,
                Tag = settings.MeasurementTag,
                Timestamp = results[0].SourceTimestamp.ToLocalTime(),
                Data = new List<DataModel>(results.Count)
            };
            ValueLinearScale scale = null;
            OPCUAItem opcUAItem = null;
            for (int i = 0; i < results.Count; i++)
            {
                scale = null;
                var tag = this.settings.OPCUAItemMap[i].Identifier;
                if (itemMap.ContainsKey(tag))
                {
                    opcUAItem = itemMap[tag];
                    tag = opcUAItem.SamiTag;
                    scale = opcUAItem.Scale;
                }
                var dm = new DataModel()
                {
                    Tag = tag
                };
                if (null != results[i].Value)
                {
                    if (null == scale)
                    {
                        dm.Value = Convert.ToDouble(results[i].Value);
                    }
                    else
                    {
                        dm.Value = scale.ScaleValue(Convert.ToDouble(results[i].Value));
                    }
                }
                mm.Data.Add(dm);
            }
            p.Measurements.Add(mm);
            // your plugin may throw exceptions and those will be logged in the service, but you can also handle your exceptions and log them if needed.

            // When your plugin has something to save raise ProcessCompleted event. The Name property can be used as ProcessCompletedEventArgs source parameter.
            // If at some point your plugin has nothing to save, then don't raise the event.
            this.OnProcessCompleted(new ProcessCompletedEventArgs(this.Name, p));

            m_Server.Disconnect();
            m_Server.CertificateEvent -= new certificateValidation(opcUaServer_CertificateEvent);
            m_Server = null;
        }

        /// <summary>
        /// Read namespace table from server and find index for namespace specified in settings.
        /// </summary>
        private void ReadNamespaceFromServer()
        {
            // Read Namespace Table
            NodeIdCollection nodesToRead = new NodeIdCollection();
            DataValueCollection results;

            nodesToRead.Add(Variables.Server_NamespaceArray);

            // Read the namespace array
            m_Server.ReadValues(nodesToRead, out results);

            if ((results.Count != 1) || (results[0].Value.GetType() != typeof(string[])))
            {
                throw new Exception("Reading namespace table returned unexptected result");
            }

            // Try to find the namespace URI entered by the user
            string[] nameSpaceArray = (string[])results[0].Value;
            ushort i;
            for (i = 0; i < nameSpaceArray.Length; i++)
            {
                if (nameSpaceArray[i] == this.settings.NamespaceUri)
                {
                    m_NameSpaceIndex = i;
                }
            }

            // Check if the namespace was found
            if (m_NameSpaceIndex == 0)
            {
                throw new Exception(string.Format("Namespace {0} not found in server namespace table", this.settings.NamespaceUri));
            }
        }

        // Default event handler pattern, see: https://msdn.microsoft.com/en-us/library/w369ty8x.aspx
        private void OnProcessCompleted(ProcessCompletedEventArgs e)
        {
            var handler = ProcessCompleted;
            if (null != handler)
            {
                handler(this, e);
            }
        }

        /// <summary>
        /// The MeasurerService calls Dispose method when the service is closing. 
        /// </summary>
        public void Dispose()
        {
            this.settings = null;
        }
    }
}
