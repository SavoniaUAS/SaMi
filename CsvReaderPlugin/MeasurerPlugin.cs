using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Savonia.Measurements.Providers.Models;
using Savonia.Measurements.Models;
using System.Diagnostics;
using System.Net;
using System.IO;
using CsvHelper;
using CsvHelper.Configuration;

namespace CsvReaderPlugin
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
        private CsvReaderPluginSettings settings;

        private string GetCsvData(DateTimeOffset from, DateTimeOffset to)
        {
            string data = null;
            Encoding e = UTF8Encoding.UTF8;
            if (!string.IsNullOrEmpty(settings.ContentEncoding))
            {
                e = Encoding.GetEncoding(settings.ContentEncoding);
            }
            if (settings.IsWebResource)
            {
                // read from web
                using (System.Net.WebClient wc = new System.Net.WebClient())
                {
                    if (null != settings.AdditionalHttpHeaders)
                    {
                        foreach (var h in settings.AdditionalHttpHeaders)
                        {
                            wc.Headers.Add(h);
                        }
                    }
                    if (!string.IsNullOrEmpty(settings.UserName) && !string.IsNullOrEmpty(settings.Password))
                    {
                        wc.Credentials = new NetworkCredential(settings.UserName, settings.Password);
                    }
                    string uri = string.Format(settings.CsvUri, from, to);
                    wc.Encoding = e;
                    data = wc.DownloadString(uri);
                }
            }
            else
            {
                // read from file
                data = System.IO.File.ReadAllText(settings.CsvUri, e);
            }

            return data;
        }

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
                settings = SerializeHelper.DeserializeWithDCS<CsvReaderPluginSettings>(System.IO.File.ReadAllText(this.ConfigurationFile.GetPluginRootedPath()));
            }
        }

        /// <summary>
        /// The MeasurerService calls Process method periodically as defined in plugin's config ExecuteInterval element. The time is seconds.
        /// Note that the same instance of this class is alive the whole time as the service is alive! So make this process as sleek and lean as possible.
        /// </summary>
        public void Process()
        {
            // read values
            var readConfigFile = this.settings.ReadConfigPath.GetPluginRootedPath();
            ReadConfig readConfig = null;
            if (System.IO.File.Exists(readConfigFile))
            {
                readConfig = SerializeHelper.DeserializeWithDCS<ReadConfig>(System.IO.File.ReadAllText(readConfigFile));
            }
            else
            {
                readConfig = new ReadConfig();
                // substract one second from current time when ReadConfig is not read from file because readStartDate is readConfig.LatestValueRead plus one second
                // this way when the config file does not exist we start from current time.
                readConfig.LatestValueRead = DateTime.Now.AddSeconds(-1.0);
            }



            DateTime readStartDate = readConfig.LatestValueRead.AddSeconds(1.0); // set startDate to latest value read + one second to not read the latest value again.
            // set endDate to start date + timespan of max time to read measurements or current time which ever is smaller.
            DateTime readEndDate = new DateTime(Math.Min(DateTime.Now.Ticks, readStartDate.Add(this.settings.MaxTimeToReadAtOnce).Ticks));

            string csvDataString = this.GetCsvData(readStartDate, readEndDate);

            if (string.IsNullOrEmpty(csvDataString))
            {
                return;
            }
            // Do the measurement reading here... and then create a MeasurementPackage object with a proper key to enable saving measurements to SAMI system.
            MeasurementPackage p = new MeasurementPackage()
            {
                Key = this.settings.SamiKey,
                Measurements = new List<MeasurementModel>()
            };
            //MeasurementModel mm = null;
            DateTime newestReadValue = readConfig.LatestValueRead;

            // we have data! --> parse the csv
            using (var reader = new StringReader(csvDataString))
            {
                var csv = new CsvReader(reader, new CsvConfiguration() { HasHeaderRecord = settings.HasHeaders, Delimiter = settings.DelimeterChar, Quote = settings.QuotationChar });
                IFormatProvider doubleParser = System.Globalization.CultureInfo.InvariantCulture;
                while (csv.Read())
                {
                    MeasurementModel mm = new MeasurementModel()
                    {
                        Object = settings.MeasurementObject,
                        Tag = settings.MeasurementTag
                    };
                    if (settings.HasHeaders)
                    {
                        mm.Timestamp = csv.GetField<DateTime>(settings.MeasurementTimeFieldHeader);
                    }
                    else
                    {
                        mm.Timestamp = csv.GetField<DateTime>(settings.MeasurementTimeFieldIndex);
                    }
                    if (mm.Timestamp.LocalDateTime >= readStartDate && mm.Timestamp.LocalDateTime <= readEndDate)
                    {
                        if (mm.Timestamp > newestReadValue)
                        {
                            newestReadValue = mm.Timestamp.DateTime;
                        }
                        // append only those measurements that are in valid time range
                        mm.Data = new List<DataModel>();
                        foreach (var item in settings.ItemMap)
                        {
                            var scale = item.Scale;
                            double readValue;
                            string rawValue;
                            if (settings.HasHeaders)
                            {
                                rawValue = csv.GetField(item.Header);
                            }
                            else
                            {
                                rawValue = csv.GetField(item.Index);
                            }
                            readValue = double.Parse(rawValue.Replace(",", "."), doubleParser);
                            mm.Data.Add(new DataModel()
                            {
                                Tag = item.SamiTag,
                                Value = null != scale ? scale.ScaleValue(readValue) : readValue
                            });

                        }

                        p.Measurements.Add(mm);
                    }
                }
            }

            readConfig.LatestValueRead = newestReadValue;
            var serialized = SerializeHelper.SerializeToStringWithDCS<ReadConfig>(readConfig);

            System.IO.File.WriteAllText(readConfigFile, serialized);


            // When your plugin has something to save raise ProcessCompleted event. The Name property can be used as ProcessCompletedEventArgs source parameter.
            // If at some point your plugin has nothing to save, then don't raise the event.
            if (null != p.Measurements && p.Measurements.Count > 0)
            {
                this.OnProcessCompleted(new ProcessCompletedEventArgs(this.Name, p));
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
