using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using Savonia.Measurements.Models;
using Savonia.Measurements.Database;
using Savonia.Measurements.Models.Helpers;
using Savonia.WCF.ErrorReporter;
using System.ServiceModel.Web;
using System.Diagnostics;
using System.Threading.Tasks;
using System.ServiceModel.Channels;

namespace Savonia.Measurements.Services
{
    [ServiceBehavior(Namespace = "Savonia.MeasurementService.V3")]
    [ErrorBehavior(typeof(Savonia.WCF.ErrorReporter.ErrorReporterModule))]
    public class MeasurementsService : IMeasurementsService
    {
        private const string WebInvokeUriEnd = "/json";

        public async Task<SaveResult> SaveMeasurementsAsync(string key, MeasurementModel[] measurements)
        {
            return await this.SaveMeasurementsInternalAsync(key, measurements);
        }

        public async Task<SaveResult> SaveMeasurementPackageAsync(MeasurementPackage measurementPackage)
        {
            var key = null != measurementPackage ? measurementPackage.Key : null;
            MeasurementModel[] measurements = null;
            if (null != measurementPackage && null != measurementPackage.Measurements)
            {
                measurements = measurementPackage.Measurements.ToArray();
            }
            return await this.SaveMeasurementsInternalAsync(key, measurements);
        }

        private async Task<SaveResult> SaveMeasurementsInternalAsync(string key, params MeasurementModel[] measurements)
        {
            Repository r = new Repository(true);
            SaveResult result = null;
            try
            {
                result = await r.SaveMeasurementsAsync(key, measurements);
            }
            catch (UnauthorizedAccessException uex)
            {
                if (IsWebInvoked)
                {
                    Failure f = new Failure()
                    {
                        Code = uex.HResult,
                        Exception = uex,
                        Index = -1,
                        Reason = uex.FriendlyException()
                    };
                    throw new WebFaultException<Failure>(f, System.Net.HttpStatusCode.Forbidden);
                }
                throw new FaultException(uex.Message);
            }
            catch (Exception ex)
            {
                Savonia.WCF.ErrorReporter.ErrorReporterModule em = new ErrorReporterModule();
                var source = OperationContext.Current.IncomingMessageProperties[RemoteEndpointMessageProperty.Name] as RemoteEndpointMessageProperty;
                em.HandleError(ex, string.Format("SaveMeasurementsInternal the actual root error. Request from: {0}", null != source ? source.Address : "n/a"));
                if (IsWebInvoked)
                {
                    Failure f = new Failure()
                    {
                        Code = ex.HResult,
                        Exception = ex,
                        Index = -1,
                        Reason = ex.FriendlyException()
                    };
                    throw new WebFaultException<Failure>(f, System.Net.HttpStatusCode.BadRequest);
                }
                throw new FaultException(ex.FriendlyException());
            }

            return result;
        }

        private bool IsWebInvoked
        {
            get
            {
                return OperationContext.Current.Channel.LocalAddress.Uri.LocalPath.EndsWith(WebInvokeUriEnd, StringComparison.InvariantCultureIgnoreCase);
            }
        }

        public async Task<List<MeasurementModel>> RestGetMeasurementsAsync(string key, string sensors, string obj, string tag, string take, string from, string to, string inclusiveFrom, string inclusiveTo)
        {
            MeasurementQueryModel query = new MeasurementQueryModel()
            {
                Key = key,
                Obj = obj,
                Tag = tag,
                Sensors = sensors,
                Take = take.QueryParamToInt(),
                From = from.QueryParamToDateTimeOffset(),
                To = to.QueryParamToDateTimeOffset(),
                InclusiveFrom = inclusiveFrom.QueryParamToBoolean(true),
                InclusiveTo = inclusiveTo.QueryParamToBoolean(true)
            };
            return await this.GetMeasurementsAsync(query);
        }

        public async Task<List<MeasurementModel>> GetMeasurementsAsync(MeasurementQueryModel query)
        {
            Repository r = new Repository();

            try
            {
                var data = await r.GetMeasurementsAsync(query, true);
                return data;
            }
            catch (KeyNotFoundException kex)
            {
                throw new FaultException(kex.Message);
            }
            catch (UnauthorizedAccessException uex)
            {
                throw new FaultException(uex.Message);
            }
        }

        public async Task<List<SensorModel>> GetSensorsAsync(string key)
        {
            Repository r = new Repository();
            try
            {
                var sensors = await r.GetSensorsAsync(key);

                return sensors;
            }
            catch (KeyNotFoundException kex)
            {
                throw new FaultException(kex.Message);
            }
            catch (UnauthorizedAccessException uex)
            {
                throw new FaultException(uex.Message);
            }
        }

        public MeasurementPackage GetSaveMeasurementPackageTemplate()
        {
            MeasurementPackage model = new MeasurementPackage();
            model.Key = "your-key-goes-here";
            model.Measurements = new List<MeasurementModel>();
            MeasurementModel mm = new MeasurementModel();
            mm.Object = "your-measurement-object";
            mm.Tag = "your-measurement-tag";
            mm.Timestamp = DateTimeOffset.Now;
            mm.Note = "if this measurement has a note";
            
            mm.Data = new List<DataModel>();
            DataModel dm = new DataModel();
            dm.Tag = "This is your sensor identification. This is unique to you.";
            dm.Value = 3.14;
            dm.TextValue = "your sensor can have some textual values also.";

            mm.Data.Add(dm);

            model.Measurements.Add(mm);

            return model;
        }

        public MeasurementModel[] GetSaveMeasurementsTemplate()
        {
            MeasurementModel[] model = new MeasurementModel[1];
            MeasurementModel mm = new MeasurementModel();
            mm.Object = "your-measurement-object";
            mm.Tag = "your-measurement-tag";
            mm.Timestamp = DateTimeOffset.Now;
            mm.Note = "if this measurement has a note";
            mm.Data = new List<DataModel>();
            DataModel dm = new DataModel();
            dm.Tag = "This is your sensor identification. This is unique to you.";
            dm.Value = 3.14;
            dm.TextValue = "your sensor can have some textual values also.";

            mm.Data.Add(dm);

            model[0] = mm;

            return model;
        }
    }
}
