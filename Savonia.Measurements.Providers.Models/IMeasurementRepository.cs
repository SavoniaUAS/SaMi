using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Savonia.Measurements.Models;
using Savonia.Measurements.Providers.Models;

namespace Savonia.Measurements.Providers.Models
{
    public interface IMeasurementRepository
    {
        Logger Log { set; }
        string Name { get; }
        void Initialize(string config);
        MeasurementPersistResult Persist(MeasurementPackage package);
        void Dispose();
    }
}
