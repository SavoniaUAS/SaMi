using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.IO;
using Newtonsoft.Json.Converters;

namespace PilotMeasurementPlugin
{
    /// <summary>
    /// Class used to for deserializing the JSON-config file
    /// </summary>
    class PilotConfig
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="configpath"></param>
        /// <returns></returns>
        public static PilotConfig GetPilotConfig(string configpath)
        {
            return JsonConvert.DeserializeObject<PilotConfig>(File.ReadAllText(configpath), new IsoDateTimeConverter { DateTimeFormat = "dd.MM.yyyy HH:mm:ss.fff" });
        }

        public static void SavePilotConfig(string configpath, PilotConfig pc)
        {
            File.WriteAllText(configpath, JsonConvert.SerializeObject(pc));
        }
        /// <summary>
        /// 
        /// </summary>
        public string SamiWriteKey { get; set; }
        public string MeasurementObject { get; set; }
        public string MeasurementTag { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string TempDirectory { get; set; }

        /// <summary>
        /// KeyValuePairs for the paths to the MeasurementFiles as key and the LastMeasurementTimes saved to database as value
        /// </summary>
        public Dictionary<String, DateTimeOffset> MeasurementFiles { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string MeasurementFileRoot { get; set; }

        public bool VerboseLogging { get; set; }

        internal void UpdateLastMeasurementTime(DateTimeOffset LastMeasurementTime)
        {
            Dictionary<String, DateTimeOffset> updatedMeasurementFiles = new Dictionary<string, DateTimeOffset>();

            foreach (KeyValuePair<string, DateTimeOffset> measurementfile in this.MeasurementFiles)
            {
                updatedMeasurementFiles.Add(measurementfile.Key, LastMeasurementTime);
            }
            this.MeasurementFiles = updatedMeasurementFiles;
        }
    }
}
