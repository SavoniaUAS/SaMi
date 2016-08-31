using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Savonia.Measurements.Models;

namespace Savonia.Measurements.Providers.Models
{
    public class MeasurementPersistResult
    {
        public SaveResult SaveResult { get; set; }
        public Exception Exception { get; set; }

        public MeasurementPersistResult()
        {
        }

        public MeasurementPersistResult(SaveResult result)
        {
            this.SaveResult = result;
        }

        public bool Persisted
        {
            get 
            {
                if (null != this.SaveResult)
                {
                    return this.SaveResult.Success;
                }
                else
                {
                    return false;
                }
            }
        }
    }
}
