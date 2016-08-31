using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Savonia.Measurements.Models;
using Savonia.Measurements.Database;
using Savonia.Measurements.Models.Helpers;
using System.Threading.Tasks;

namespace Savonia.Measurements.Manager.Controllers
{
    /// <summary>
    /// Measurement web api. Note very experimental and currently not used!
    /// </summary>
    public class MeasurementController : ApiController
    {
        // GET: api/Measurement
        public IEnumerable<string> Get()
        {
            return new string[] { "Please provide a key and other parameters.",
                                    "Default values: obj = null, tag = null, top = 20",
                                    "Sample: api/Measurement/mykey-goes-here", 
                                    "Sample 2: api/Measurement/mykey-goes-here?obj=my-object",
                                    "Sample 3: api/Measurement/mykey-goes-here?obj=my-object&tag=my-tag",
                                    "Sample 4: api/Measurement/mykey-goes-here?obj=my-object&tag=my-tag&top=50"};
        }

        // GET: api/Measurement/my-key?obj=some-object&tag=some-tag
        //public IHttpActionResult Get(MeasurementQueryModel query)
        public async Task<IHttpActionResult> Get(string key, string sensors, string obj, string tag, string take, string from, string to)
        {
            MeasurementQueryModel query = new MeasurementQueryModel()
            {
                Key = key,
                Obj = obj,
                Tag = tag,
                Sensors = sensors,
                Take = take.QueryParamToInt(),
                From = from.QueryParamToDateTimeOffset(),
                To = to.QueryParamToDateTimeOffset()
            };
            Repository r = new Repository();

            try
            {
                
                var model = await r.GetMeasurementsAsync(query, true);
                if (null == model)
                {
                    return NotFound();
                }
                return Ok(model);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized();
            }
        }

        // POST: api/Measurement
        public void Post([FromBody]MeasurementPackage value)
        {
            //Repository r = new Repository();

            //r.SaveMeasurements(value);
        }

        // PUT: api/Measurement/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE: api/Measurement/5
        public void Delete(int id)
        {
        }
    }
}
