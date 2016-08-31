using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Savonia.Measurements.Models;

namespace Savonia.Measurements.Providers.Models
{
    public class ProcessCompletedEventArgs : EventArgs
    {
        public string Source { get; set; }
        public MeasurementPackage MeasurementPackage { get; set; }

        public ProcessCompletedEventArgs()
        { }

        public ProcessCompletedEventArgs(string source, MeasurementPackage measurements)
        {
            this.Source = source;
            this.MeasurementPackage = measurements;
        }
    }
}
