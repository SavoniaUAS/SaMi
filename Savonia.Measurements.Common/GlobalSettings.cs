using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Savonia.Measurements.Common
{
    public static class GlobalSettings
    {
        private static readonly object _lock = new Object();
        /// <summary>
        /// Get parallelization threshold from app settings "MeasurementSaveParallelizationThreshold". Return 100 if no setting value is present.
        /// </summary>
        public static int ParallelizationThreshold
        {
            get 
            {
                var value = ConfigurationManager.AppSettings[Constants.ParallelizationThresholdKey];
                int val = 100;
                if (int.TryParse(value, out val))
                {
                    return val;
                }
                return val;
            }
        }
        /// <summary>
        /// Get parallelization thread count from app settings "MeasurementSaveParallelizationThreadsCount". Return null if setting value cannot be parsed to int.
        /// </summary>
        public static int? ParallelizationThreadsCount
        {
            get
            {
                var value = ConfigurationManager.AppSettings[Constants.ParallelizationThreadsCountKey];
                int val = 0;
                if (int.TryParse(value, out val))
                {
                    return val;
                }
                return null;
            }
        }

        /// <summary>
        /// Gets the csv-path from app settings (Manager).
        /// </summary>
        public static string GetCsvPath
        {
            get
            {
                return ConfigurationManager.AppSettings[Constants.CsvPath];
            }
        }

        /// <summary>
        /// Gets the JSON-path from app settings (Manager).
        /// </summary>
        public static string GetJSONPath
        {
            get
            {
                return ConfigurationManager.AppSettings[Constants.JsonPath];
            }
        }

        /// <summary>
        /// Gets the main site Url. Returns ~/ if app setting for MainSiteUrl is not found.
        /// </summary>
        public static string GetMainSiteUrl
        {
            get
            {
                return ConfigurationManager.AppSettings[Constants.MainSiteUrl] ?? "~/";
            }
        }

        /// <summary>
        /// Gets the raw string of admin users from app settings (Manager).
        /// </summary>
        public static string GetAdminUsersRaw
        {
            get
            {
                return ConfigurationManager.AppSettings[Constants.Admins];
            }
        }

        /// <summary>
        /// Gets admins users from app settings (Manager). Returns null if admins cannot be found. [Might be slow if there is multiple simultaneous clients.] 
        /// </summary>
        public static List<string> GetAdminUsers
        {           
        get
            {
                lock (_lock)
                {
                    List<string> AdminUsers = new List<string>();
                    var raw = ConfigurationManager.AppSettings[Constants.Admins];
                    if (raw != null)
                    {
                        var admins = raw.Split(",;".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                        foreach (var a in admins)
                        {
                            AdminUsers.Add(a.Trim());
                        }
                        return AdminUsers;
                    }
                    return null;
                }
            }
        }

        /// <summary>
        /// Gets salt from app settings.
        /// </summary>
        public static string GetSalt
        {
            get
            {
                return ConfigurationManager.AppSettings[Constants.Salt];
            }
        }

        /// <summary>
        /// Gets encryption key from app settings.
        /// </summary>
        public static string GetEncryptionKey
        {
            get
            {
                return ConfigurationManager.AppSettings[Constants.EncryptionKey];
            }
        }
        

        /// <summary>
        /// Gets Service base url from app settings.
        /// </summary>
        public static string GetServiceUrl
        {
            get
            {
                return ConfigurationManager.AppSettings[Constants.ServiceBaseUrl];
            }
        }

    }
}
