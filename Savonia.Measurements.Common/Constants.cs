using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Savonia.Measurements.Common
{
    public static class Constants
    {
        public const string ParallelizationThresholdKey = "MeasurementSaveParallelizationThreshold";
        public const string ParallelizationThreadsCountKey = "MeasurementSaveParallelizationThreadsCount";
        /// <summary>
        /// Path of temp csc-files
        /// </summary>
        public const string CsvPath = "CsvFilePath";
        /// <summary>
        /// Path of JSON-files
        /// </summary>
        public const string JsonPath = "JsonFilePath";
        /// <summary>
        /// Key for main site url.
        /// </summary>
        public const string MainSiteUrl = "MainSiteUrl";

        /// <summary>
        /// Admins of web app
        /// </summary>
        public const string Admins = "AdminUsers";

        /// <summary>
        /// Default salt
        /// </summary>
        public const string Salt = "KeyDefaultSalt";
        /// <summary>
        /// Key used in encryption
        /// </summary>
        public const string EncryptionKey = "KeyEncryptionKey";
        /// <summary>
        /// Service base url
        /// </summary>
        public const string ServiceBaseUrl = "ServiceV3BaseUrl";
    }
}
