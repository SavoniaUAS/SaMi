using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.IO;
using Savonia.Measurements.Models;
using Savonia.Measurements.Providers.Models;
using Newtonsoft.Json.Converters;
using SocialExplorer.IO.FastDBF;
using System.Globalization;

namespace PilotMeasurementPlugin
{
    
    class PilotLib
    {
        /// <summary>
        /// JSON deserialized configuration parameters
        /// </summary>
        private PilotConfig Config;

        /// <summary>
        /// First files last measurement time set in to the future (arbitrary) for the first file
        /// All of first files timestamps will be earlier than this date
        /// After first file has been parsed, this datetime will be the last timestamp from the first file
        /// </summary>
        public DateTimeOffset NewLastMeasurementTime = DateTimeOffset.Now+TimeSpan.FromHours(2);

        /// <summary>
        /// Savonia Measurements Logger object for easy logging
        /// </summary>
        private Logger Log;

        /// <summary>
        /// Constructor which sets PilotLib's configuration parameters to be used in the process
        /// Also set the logger instance
        /// </summary>
        /// <param name="_config"></param>
        public PilotLib(PilotConfig _config, Logger _log)
        {
            Config = _config;
            Log = _log;
        }

        /// <summary>
        /// Main public method for getting the measurementpackage
        /// </summary>
        /// <returns>Final (hopefully) fully formed MeasurementPackage</returns>
        public MeasurementPackage GetMeasurementPackage()
        {
            MeasurementPackage mp = new MeasurementPackage();

            mp.Key = Config.SamiWriteKey;
            mp.Measurements = GetMeasurements();
            return mp;
        }

        /// <summary>
        /// Main private method that copies the measurement files to parse and
        /// return them as a list of measurements
        /// </summary>
        /// <param name="mp"></param>
        /// <returns>List of measurements for MeasurementPackage.Measurements</returns>
        private List<MeasurementModel> GetMeasurements()
        {
            //Measurements to be returned
            List<MeasurementModel> measurements = new List<MeasurementModel>();
            //Flag for the first file to be parsed through. Set to false after first file has been parsed
            bool isFirst = true;

            

            //Copy all files listed in config to a temporary location
            foreach (KeyValuePair<string, DateTimeOffset> measurementfile in Config.MeasurementFiles)
            {
                //Note * look down
                try
                {
                    CopyMeasurementFile(System.IO.Path.Combine(Config.MeasurementFileRoot, measurementfile.Key));
                }
                catch (Exception ex)
                {
                    Log.Append(string.Format("CopyMeasurementFile() Exception: {0}", ex.ToString()), true);
                }
            }

            //Loop through all the copied files
            string file;
            foreach (KeyValuePair<string, DateTimeOffset> measurementfile in Config.MeasurementFiles)
            {
                //Note * look down
                try
                {
                    file = System.IO.Path.Combine(Config.MeasurementFileRoot, measurementfile.Key);
                    LoadMeasurementsFromFile(file, measurementfile.Value, measurements);

                    //After first file has been looped through, get newest timestamp from measurements and treat it
                    //as the LastMeasurementTime - this cuts off some measurements from the end of other files
                    //which will be picked up come next sync
                    if (isFirst && measurements.Count > 0)
                    {
                        if (Config.VerboseLogging)
                        {
                            Log.Append(String.Format("Was first file, set new last measurement time {0}", file), true);
                        }
                        isFirst = false;
                        NewLastMeasurementTime = measurements.Max(m => m.Timestamp);
                        if (Config.VerboseLogging)
                        {
                            Log.Append(String.Format("New last measurement time was set at {0}", NewLastMeasurementTime), true);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Log.Append(string.Format("LoadMeasurementsFromFile() Exception: {0}", ex.ToString()), true);
                }
            }

            //Note * --- Note that exception handling can be sketchy at best and it is hard to predict
            //how the program responds if all the files are not readable at a particular time
            //or something else goes wrong during read/parse

            return measurements;
        }

        /// <summary>
        /// Copy measurement file to gain latest state of the file
        /// </summary>
        /// <param name="filepath"></param>
        private void CopyMeasurementFile(string filepath)
        {
            //All files copied to Temp-directory set in the .cfg-file
            //don't leave empty in config please. Probably explodes if left empty
            string tempfilepath = Path.Combine(Config.TempDirectory.GetPluginRootedPath(), Path.GetFileName(filepath));

            //create directory if it doesn't exist
            var tempDir = Config.TempDirectory.GetPluginRootedPath();
            if (!Directory.Exists(tempDir))
            {
                Directory.CreateDirectory(tempDir);
                if (Config.VerboseLogging)
                {
                    Log.Append(String.Format("Created temp directory {0}", tempDir), true);
                }
            }

            File.Copy(filepath, tempfilepath, true);
            if (Config.VerboseLogging)
            {
                Log.Append(String.Format("Copied file {0} to {1}", filepath, tempfilepath), true);
            }

        }

        private void LoadMeasurementsFromFile(string filepath, DateTimeOffset lastmeasurementtimestamp, List<MeasurementModel> measurements)
        {
            //get previously copied file from the same temporary location it was copied to
            string tempfilepath = Path.Combine(Config.TempDirectory.GetPluginRootedPath(), Path.GetFileName(filepath));

            //Instantiate and open the file into a DBFFile object
            DbfFile dbf = new DbfFile();
            dbf.Open(tempfilepath, FileMode.Open);
            if (Config.VerboseLogging)
            {
                Log.Append(String.Format("Opened dbf file {0}", tempfilepath), true);
            }
            //Add new measurements from dbf since last measurement into list of measurements
            AddNewMeasurements(dbf, lastmeasurementtimestamp, measurements);

            //Close file and delete
            dbf.Close();
            File.Delete(tempfilepath);
            if (Config.VerboseLogging)
            {
                Log.Append(String.Format("Deleted dbf file {0}", tempfilepath), true);
            }
        }

        /// <summary>
        /// Main dbf looper
        /// Get sensor data from file and add it to the list of measurements
        /// </summary>
        /// <param name="dbf"></param>
        /// <param name="lastmeasurementtimestamp"></param>
        /// <param name="measurements"></param>
        private void AddNewMeasurements(DbfFile dbf, DateTimeOffset lastmeasurementtimestamp, List<MeasurementModel> measurements)
        {
            //raw string 'date' output from file row as yyyyMMddHHmmssfff
            string date;
            //parsed date of row
            DateTime rowtimestamp;
            //raw string 'value' output from file
            string value;
            //parse value of row
            double rowvalue;

            //loop increment (for DBF file rows)
            int i = 0;
            if (Config.VerboseLogging)
            {
                Log.Append(String.Format("Begin dbf file read {0}. File has {1} rows.", dbf.FileName, dbf.Header.RecordCount), true);
            }
            //Loop through all rows in file
            while (i < dbf.Header.RecordCount)
            {
                //ReadValue from row i and column 0 to get date, parse the datetime and round it down to closest 500ms increment
                dbf.ReadValue(i, 0, out date);
                rowtimestamp = DateTime.ParseExact(date.Trim(), "yyyyMMddHHmmssfff", CultureInfo.InvariantCulture);
                rowtimestamp = rowtimestamp.AddMilliseconds(GetMillisecondsToSubstract(rowtimestamp.Millisecond));
                if (Config.VerboseLogging)
                {
                    Log.Append(String.Format("Read row {0} on dbf file {1}. Row time is {2}.", i, dbf.FileName, rowtimestamp), true);
                }
                //check if row is newer than last saved measurement AND older than the LastMeasurementTime from the first parsed file
                if (rowtimestamp > lastmeasurementtimestamp && rowtimestamp <= NewLastMeasurementTime)
                {
                    //read row value (column 1) and parse it
                    dbf.ReadValue(i, 1, out value);
                    rowvalue = value.Trim().ToDouble();
                    if (Config.VerboseLogging)
                    {
                        Log.Append(String.Format("Read row {0} on dbf file {1}. Row value is {2}.", i, dbf.FileName, rowvalue), true);
                    }

                    //check if a measurement with corresponding timestamp has already been added

                    MeasurementModel oldmeasurement = null;
                    if(measurements.Count > 0)
                    { 
                        oldmeasurement = measurements.SingleOrDefault(m => m.Timestamp == rowtimestamp);
                    }
                    //if yes - append data to old measurement
                    if (oldmeasurement != null)
                    {
                        oldmeasurement.Data.Add(new DataModel()
                        {
                            Tag = Path.GetFileName(dbf.FileName).Replace(".dbf", "_PILOT"),
                            Value = rowvalue
                        });
                        if (Config.VerboseLogging)
                        {
                            Log.Append(String.Format("Added values to old measurement {0}.", oldmeasurement.Timestamp), true);
                        }
                    }
                    else //if no - add a new measurement
                    {
                        MeasurementModel newmeasurement = new MeasurementModel();
                        newmeasurement.Timestamp = rowtimestamp;
                        newmeasurement.Object = this.Config.MeasurementObject;
                        newmeasurement.Tag = this.Config.MeasurementTag;
                        newmeasurement.Data = new List<DataModel>();
                        newmeasurement.Data.Add(new DataModel()
                        {
                            Tag = Path.GetFileName(dbf.FileName).Replace(".dbf", "_PILOT"),
                            Value = rowvalue
                        });
                        measurements.Add(newmeasurement);
                        if (Config.VerboseLogging)
                        {
                            Log.Append(String.Format("Created new measurement {0} and added it to measurements list which now contains {1} items.", newmeasurement.Timestamp, measurements.Count), true);
                        }
                    }
                }
                else
                {
                    if (Config.VerboseLogging)
                    {
                        Log.Append(String.Format("Row {0} time {1} was not between {2} and {3}, value was not added to measurements.", i, rowtimestamp, lastmeasurementtimestamp, NewLastMeasurementTime), true);
                    }
                }
                i++;
            }
            if (Config.VerboseLogging)
            {
                Log.Append(String.Format("End dbf file read {0}. Read {1} rows.", dbf.FileName, i), true);
            }
        }

        /// <summary>
        /// Simple rounding down to closest 0,5 seconds (>500ms to 500ms and >0ms to 0ms)
        /// </summary>
        /// <param name="ms">milliseconds to round</param>
        /// <returns>rouded down milliseconds</returns>
        private int GetMillisecondsToSubstract(int ms)
        {
            return -((ms>= 500) ? ms - 500 : ms);
        }
    }
}
