using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace Savonia.Measurements.Providers.MeasurerService.Models
{
    [Serializable]
    public class PluginSettings 
    {
        /// <summary>
        /// Minimun plugin execution interval in seconds. This overrides all plugin execute interval settings 
        /// that are smaller.
        /// </summary>
        public int MinimumExecutionInterval { get; set; }
        [XmlElement(ElementName="Plugin")]
        public List<MeasurerProcessPlugin> Plugins { get; set; }

        public PluginSettings() {
            this.Plugins = new List<MeasurerProcessPlugin>();
        }
    }
    [Serializable]
    public class MeasurerProcessPlugin 
    {
        /// <summary>
        /// Plugin friendly name.
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Plugin assembly (dll) location = full path.
        /// </summary>
        public string PluginAssembly { get; set; }
        /// <summary>
        /// Is plugin enable or disabled.
        /// </summary>
        public bool Enabled { get; set; }
        /// <summary>
        /// Optional description about what plugin does.
        /// </summary>
        public string Description { get; set; }
        /// <summary>
        /// Plugin specific configuration file path.
        /// </summary>
        public string ConfigFile { get; set; }
        /// <summary>
        /// Plugin execution interval in seconds.
        /// </summary>
        public int ExecuteInterval { get; set; }
        /// <summary>
        /// Is plugin executed when Data Provider Service starts.
        /// </summary>
        public bool ExecuteOnServiceStart { get; set; }
    }

}
