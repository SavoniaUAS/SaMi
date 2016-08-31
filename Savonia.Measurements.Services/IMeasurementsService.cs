using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using Savonia.Measurements.Models;
using System.ServiceModel.Web;
using System.Threading.Tasks;

namespace Savonia.Measurements.Services
{
    [ServiceContract(Namespace = "Savonia.MeasurementService.V3")]
    public interface IMeasurementsService
    {
        [OperationContract]
        [WebInvoke(Method = "POST",
                    RequestFormat = WebMessageFormat.Json,
                    ResponseFormat = WebMessageFormat.Json,
                    UriTemplate = "measurements/save",
                    BodyStyle = WebMessageBodyStyle.Wrapped)]
        Task<SaveResult> SaveMeasurementsAsync(string key, MeasurementModel[] measurements);

        [OperationContract]
        [WebInvoke(Method = "POST",
                    RequestFormat = WebMessageFormat.Json,
                    ResponseFormat = WebMessageFormat.Json,
                    UriTemplate = "measurementpackage/save")]
        Task<SaveResult> SaveMeasurementPackageAsync(MeasurementPackage measurementPackage);

        [OperationContract]
        [WebInvoke(Method = "GET",
                    RequestFormat = WebMessageFormat.Json,
                    ResponseFormat = WebMessageFormat.Json,
                    UriTemplate = "measurements/{key}?data-tags={sensors}&obj={obj}&tag={tag}&take={take}&from={from}&to={to}&inclusiveFrom={inclusiveFrom}&inclusiveTo={inclusiveTo}",
                    BodyStyle = WebMessageBodyStyle.Bare)]
        Task<List<MeasurementModel>> RestGetMeasurementsAsync(string key, string sensors, string obj, string tag, string take, string from, string to, string inclusiveFrom, string inclusiveTo);

        [OperationContract]
        Task<List<MeasurementModel>> GetMeasurementsAsync(MeasurementQueryModel query);

        [OperationContract]
        [WebInvoke(Method = "GET",
                    RequestFormat = WebMessageFormat.Json,
                    ResponseFormat = WebMessageFormat.Json,
                    UriTemplate = "sensors/{key}",
                    BodyStyle = WebMessageBodyStyle.Bare)]
        Task<List<SensorModel>> GetSensorsAsync(string key);


        [OperationContract]
        [WebInvoke(Method = "GET",
                    RequestFormat = WebMessageFormat.Json,
                    ResponseFormat = WebMessageFormat.Json,
                    UriTemplate = "measurements/template",
                    BodyStyle = WebMessageBodyStyle.Bare)]
        MeasurementModel[] GetSaveMeasurementsTemplate();

        [OperationContract]
        [WebInvoke(Method = "GET",
                    RequestFormat = WebMessageFormat.Json,
                    ResponseFormat = WebMessageFormat.Json,
                    UriTemplate = "measurementpackage/template",
                    BodyStyle = WebMessageBodyStyle.Bare)]
        MeasurementPackage GetSaveMeasurementPackageTemplate();
    }
}
