using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Savonia.Measurements.Providers.Models
{
    /// <summary>
    /// Sample implementation of IMeasurer. This can be used to implement own meaurers.
    /// Override methods LoadConfig, Process and Dispose.
    /// </summary>
    public abstract class MeasurerBase : IMeasurer
    {
        public event EventHandler<ProcessCompletedEventArgs> ProcessCompleted;

        public string ConfigurationFile { get; set; }
        public string Name { get; set; }
        public Logger Log { get; set; }

        /// <summary>
        /// This LoadConfig base implementation is empty (does nothing).
        /// </summary>
        public virtual void LoadConfig()
        {
            // load config...
        }
        /// <summary>
        /// This Process base implementation is empty (does nothing).
        /// </summary>
        public virtual void Process()
        {
            // do some work...

            // create measurement package
            //Savonia.Measurements.Models.MeasurementPackage p = new Measurements.Models.MeasurementPackage();

            // read the measurements...

            // use static Logger if needed.
            //Savonia.Measurements.Providers.Models.Logger.Append("some info to log...");

            // and when ready raise ProcessCompleted event
            //OnProcessCompleted(new ProcessCompletedEventArgs(this.Name, p));
        }
        /// <summary>
        /// This method can be used to raise ProcessCompleted event.
        /// This conforms to https://msdn.microsoft.com/en-us/library/w369ty8x.aspx pattern.
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnProcessCompleted(ProcessCompletedEventArgs e)
        {
            // event handler pattern, see: https://msdn.microsoft.com/en-us/library/w369ty8x.aspx
            var handler = ProcessCompleted;
            if (null != handler)
            {
                handler(this, e);
            }
        }
        /// <summary>
        /// This Dispose base implementation is empty (does nothing).
        /// </summary>
        public virtual void Dispose()
        {
            // release resources etc.
        }
    }
}
