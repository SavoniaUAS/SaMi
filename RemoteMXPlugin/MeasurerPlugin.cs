using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Savonia.Measurements.Providers.Models;
using Savonia.Measurements.Models;

namespace RemoteMXPlugin
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
        private RemoteMXPluginSettings settings;

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
                settings = SerializeHelper.DeserializeWithDCS<RemoteMXPluginSettings>(System.IO.File.ReadAllText(this.ConfigurationFile.GetPluginRootedPath()));
            }
        }
        /// <summary>
        /// The MeasurerService calls Process method periodically as defined in plugin's config ExecuteInterval element. The time is seconds.
        /// Note that the same instance of this class is alive the whole time as the service is alive! So make this process as sleek and lean as possible.
        /// </summary>
        public void Process()
        {
            var readConfigFile = this.settings.ReadConfigPath.GetPluginRootedPath();
            ReadConfig readConfig = null;
            if (System.IO.File.Exists(readConfigFile))
            {
                readConfig = SerializeHelper.DeserializeWithDCS<ReadConfig>(System.IO.File.ReadAllText(readConfigFile));
            }
            else
            {
                readConfig = new ReadConfig();
            }

            RemoteMXService.site response = null;
            DateTime readStartDate = readConfig.LatestValueRead.AddSeconds(1.0); // set startDate to latest value read + one second to not read the latest value again.
            // set endDate to start date + timespan of max time to read measurements or current time which ever is smaller.
            DateTime readEndDate = new DateTime(Math.Min(DateTime.Now.Ticks, readStartDate.Add(this.settings.MaxTimeToReadAtOnce).Ticks)); 
            using (RemoteMXService.RdmSitesClient client = new RemoteMXService.RdmSitesClient())
            {
                var sites = client.getSites(new RemoteMXService.user() { password = this.settings.Password, username = this.settings.UserName });
                if (null == sites || sites.Length == 0)
                {
                    Log.Append(string.Format("No sites found from RemoteMX for user {0}", this.settings.UserName));
                }
                else
                {
                    if (sites.Any(s => s.id == this.settings.SiteId))
                    {
                        RemoteMXService.datareq request = new RemoteMXService.datareq()
                        {
                            siteId = this.settings.SiteId,
                            username = this.settings.UserName,
                            password = this.settings.Password,
                            startDate = readStartDate,
                            endDate = readEndDate
                        };
                        response = client.getData(request);
                    }
                    else
                    {
                        string sitesString = "";
                        sites.ToList().ForEach(s =>
                        {
                             sitesString += string.Format("Site id: {0}, name: {1}{2}", s.id, s.name, Environment.NewLine);
                        });
                        throw new Exception(string.Format("Configured site id (SiteId: {1}) was not found from RemoteMX service with username {2}. {0}RemoteMX sites found:{0}{3}",
                                                            Environment.NewLine,
                                                            this.settings.SiteId,
                                                            this.settings.UserName,
                                                            sitesString));
                    }
                }
            }

            string serialized = string.Empty;
            if (null == response || null == response.data || response.data.Length == 0)
            {
                string messageTemplate = "No measurements found from RemoteMX for user {0} with site id {1} between {2} and {3}. Next reading will try with the same start time ({2}).";
                // No data --> update latest read value to readEndDate when the readEndDate is old enough.
                // This adds a little redundancy to data reading to allow service breaks etc.
                // Try to read the same time period for twice the duration of MaxTimeToReadAtOnce setting and after that move on.
                if (readEndDate.Add(this.settings.MaxTimeToReadAtOnce.Add(this.settings.MaxTimeToReadAtOnce)) < DateTime.Now)
                {
                    readConfig.LatestValueRead = readEndDate;
                    serialized = SerializeHelper.SerializeToStringWithDCS<ReadConfig>(readConfig);
                    System.IO.File.WriteAllText(readConfigFile, serialized);
                    messageTemplate = "No measurements found from RemoteMX for user {0} with site id {1} between {2} and {3}. Next reading will start with updated start time ({3}).";
                }
                Log.Append(string.Format(messageTemplate, 
                                this.settings.UserName,
                                this.settings.SiteId,
                                readStartDate,
                                readEndDate));
                return;
            }

            // we have data!

            // Do the measurement reading here... and then create a MeasurementPackage object with a proper key to enable saving measurements to SAMI system.
            MeasurementPackage p = new MeasurementPackage()
            {
                Key = this.settings.SamiKey,
                Measurements = new List<MeasurementModel>(response.data.Length)
            };

            MeasurementModel mm = null;
            DateTime newestReadValue = readConfig.LatestValueRead;
            foreach (var item in response.data)
            {
                mm = new MeasurementModel()
                {
                    Object = settings.MeasurementObject,
                    Tag = settings.MeasurementTag,
                    Timestamp = item.timestamp
                };
                if (item.timestamp > newestReadValue)
                {
                    newestReadValue = item.timestamp;
                }

                if (null != item.variables && item.variables.Length > 0)
                {
                    mm.Data = new List<DataModel>(item.variables.Length);
                    bool save = false;
                    foreach (var val in item.variables)
                    {
                        // when SaveMappedValuesOnly == true --> save = false. Data is saved only when ChannelMap contains a map for the channel.
                        // when SaveMappedValuesOnly == false --> save = true. Data is saved with or without ChannelMap info.
                        save = !this.settings.SaveMappedValuesOnly;
                        var tag = val.channel;
                        if (this.settings.ChannelMap.ContainsKey(tag))
                        {
                            tag = this.settings.ChannelMap[tag];
                            save = true;
                        }
                        if (save)
                        {
                            mm.Data.Add(new DataModel()
                            {
                                Tag = tag,
                                Value = val.value
                            });
                        }
                    }
                }
                // NOTE! a measurement might not contain any Data items.
                p.Measurements.Add(mm);
            }

            readConfig.LatestValueRead = newestReadValue;
            serialized = SerializeHelper.SerializeToStringWithDCS<ReadConfig>(readConfig);

            System.IO.File.WriteAllText(readConfigFile, serialized);

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
