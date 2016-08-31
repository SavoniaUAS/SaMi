using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Savonia.Measurements.Providers.Models;
using Savonia.Measurements.Models;

namespace MeasurementPluginSample
{
    /// <summary>
    /// Your plugin must be a public class and it must either directly implements Savonia.Measurements.Providers.Models.IMeasurer interface 
    /// or derive a class that implements the interface.
    /// </summary>
    public class DemoPlugin : IMeasurer
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
        private DemoPluginSettings settings;

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
                settings = SerializeHelper.Deserialize<DemoPluginSettings>(System.IO.File.ReadAllText(this.ConfigurationFile.GetPluginRootedPath()));
            }
        }
        /// <summary>
        /// The MeasurerService calls Process method periodically as defined in plugin's config ExecuteInterval element. The time is seconds.
        /// Note that the same instance of this class is alive the whole time as the service is alive! So make this process as sleek and lean as possible.
        /// </summary>
        public void Process()
        {
            // Do the measurement reading here... and then create a MeasurementPackage object with a proper key to enable saving measurements to SAMI system.
            Random rnd = new Random();
            MeasurementPackage p = new MeasurementPackage()
            {
                Key = this.settings.Key,
                Measurements = new List<MeasurementModel>()
            };

            MeasurementModel mm = new MeasurementModel()
            {
                Object = settings.Object,
                Tag = settings.Tag,
                Timestamp = DateTimeOffset.Now,
                Data = new List<DataModel>()
            };

            foreach (var item in this.settings.SensorTags)
            {
                mm.Data.Add(new DataModel()
                {
                    Tag = item,
                    Value = rnd.NextDouble() * 100
                });
            }

            p.Measurements.Add(mm);

            this.Log.Append("Test message to log...");

            // your plugin may throw exceptions and those will be logged in the service, but you can also handle your exceptions and log them if needed.
            // throw exception or do not handle possible exceptions...
            // throw new Exception("some info...");
            // or handle exceptions and log the info...
            // Log.Append("some info...");

            // When your plugin has something to save raise ProcessCompleted event. The Name property can be used as ProcessCompletedEventArgs source parameter.
            // If at some point your plugin has nothing to save, then don't raise the event.
            this.OnProcessCompleted(new ProcessCompletedEventArgs(this.Name, p));
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
