using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Savonia.Measurements.Providers.Models
{
    /// <summary>
    /// Measurement plugin interface.
    /// </summary>
    public interface IMeasurer
    {
        event EventHandler<ProcessCompletedEventArgs> ProcessCompleted;
        /// <summary>
        /// Public ConfigurationFile string property to set/get configuration file path.
        /// </summary>
        string ConfigurationFile { get; set; }
        /// <summary>
        /// Public Name string property to identify different plugins. The service uses this
        /// to report exceptions from different plugins.
        /// </summary>
        string Name { get; set; }
        /// <summary>
        /// Public Logger. The service provides a logger object for each plugin.
        /// </summary>
        Logger Log { get; set; }
        /// <summary>
        /// Public LoadConfig() method is called after property values are set. This should load configuration
        /// information from ConfigurationFile if needed.
        /// </summary>
        void LoadConfig();
        /// <summary>
        /// Public Process() method. This method does the actual processing. When processing is ready results can be notified by raising ProcessCompleted event.
        /// This is executed in it's own thread.
        /// </summary>
        void Process();
        /// <summary>
        /// Public Dispose() method. This method is called when the service closes. This should release all reserved
        /// resources (mainly Logger object by calling Log.Dispose()).
        /// </summary>
        void Dispose();
    }
}
