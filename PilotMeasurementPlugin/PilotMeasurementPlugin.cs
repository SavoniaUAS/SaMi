using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Savonia.Measurements.Providers.Models;
using Savonia.Measurements.Models;

namespace PilotMeasurementPlugin
{
    /// <summary>
    /// Your plugin must be a public class and it must either directly implements Savonia.Measurements.Providers.Models.IMeasurer interface 
    /// or derive a class that implements the interface.
    /// </summary>
    public class PilotMeasurementPlugin : IMeasurer
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
        /// <summary>
        /// JSON deserialized object of configuration variables
        /// </summary>
        private PilotConfig pc;

        /// <summary>
        /// The MeasurerService calls LoadConfig method after the properties are set.
        /// </summary>
        public void LoadConfig()
        {
            //pilotconfig returns itself
            pc = PilotConfig.GetPilotConfig(ConfigurationFile.GetPluginRootedPath());
        }

        /// <summary>
        /// The MeasurerService calls Process method periodically as defined in plugin's config ExecuteInterval element. The time is seconds.
        /// Note that the same instance of this class is alive the whole time as the service is alive! So make this process as sleek and lean as possible.
        /// </summary>
        public void Process()
        {
            //library takes the config and a Logger instance
            PilotLib pl = new PilotLib(pc, Log);

            //librarys only public facing method return a MeasurementPackage
            MeasurementPackage mp = pl.GetMeasurementPackage();
            // When your plugin has something to save raise ProcessCompleted event. The Name property can be used as ProcessCompletedEventArgs source parameter.
            // If at some point your plugin has nothing to save, then don't raise the event.
            if(mp != null && mp.Measurements != null && mp.Measurements.Count > 0)
            {
                //Update and save the configuration files LastMeasurementTime for each file
                pc.UpdateLastMeasurementTime(pl.NewLastMeasurementTime);
                PilotConfig.SavePilotConfig(ConfigurationFile.GetPluginRootedPath(), pc);

                this.OnProcessCompleted(new ProcessCompletedEventArgs(this.Name, mp));
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
            this.pc = null;
        }
    }
}
