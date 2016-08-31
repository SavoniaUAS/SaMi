using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace Savonia.Measurements.Models
{
    [DataContract]
    public class SaveResult
    {
        [DataMember]
        public bool Success { get; set; }

        [Obsolete]
        [DataMember]
        public List<MeasurementStatus> Statuses { get; set; }

        /// <summary>
        /// Failures contains a list of failed meurement indexes.
        /// </summary>
        [DataMember]
        public List<Failure> Failures { get; set; }

        public void AddFailure(Failure f)
        {
            if (null == Failures)
            {
                Failures = new List<Failure>();
            }
            Failures.Add(f);
        }

        /// <summary>
        /// Sets Success to true when Failures list is null or empty.
        /// </summary>
        public void PopulateOverallSuccessStatus()
        {
            // Sets Success to true when all Statuses are in Saved state (IsSaved is true).
            Success = null == Failures || Failures.Count == 0;
        }
    }

    [DataContract]
    public class Failure
    {
        /// <summary>
        /// Index of failed measurement.
        /// </summary>
        [DataMember]
        public int Index { get; set; }
        [DataMember]
        public int Code { get; set; }
        [DataMember]
        public string Reason { get; set; }

        [IgnoreDataMember]
        public Exception Exception { get; set; }
    }

    [DataContract]
    public class MeasurementStatus
    {
        [DataMember]
        public DateTimeOffset MeasurementTimeStamp { get; set; }
        [DataMember]
        public bool IsSaved { get; set; }
    }
}