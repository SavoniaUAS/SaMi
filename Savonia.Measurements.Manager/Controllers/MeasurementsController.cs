using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.ModelBinding;
//using System.Web.Http.OData;
//using System.Web.Http.OData.Query;
//using System.Web.Http.OData.Routing;
using Microsoft.Data.OData;
using Savonia.Measurements.Database;
using Savonia.Measurements.Models;
using System.Web.OData.Routing;
using System.Web.OData;
using System.Web.OData.Query;


namespace Savonia.Measurements.Manager.Controllers
{
    /*
    The WebApiConfig class may require additional changes to add a route for this controller. Merge these statements into the Register method of the WebApiConfig class as applicable. Note that OData URLs are case sensitive.

    using System.Web.Http.OData.Builder;
    using System.Web.Http.OData.Extensions;
    using Savonia.Measurements.Models;
    ODataConventionModelBuilder builder = new ODataConventionModelBuilder();
    builder.EntitySet<MeasurementModel>("Measurements");
    config.Routes.MapODataServiceRoute("odata", "odata", builder.GetEdmModel());
    */

    /// <summary>
    /// Measurements odata 
    /// </summary>
    //[ODataRoutePrefix("Measurements")]
    //public class MeasurementsODataController : ODataController
    public class MeasurementsController : ODataController
    {
        private Repository r = new Repository();
        private static ODataValidationSettings _validationSettings = new ODataValidationSettings();

        public IHttpActionResult Get()
        {
            return BadRequest("Please, provide access key to read measurement.");
        }


        // GET: odata/V4/Measurements(your-key)
        [EnableQuery]
        public async Task<IHttpActionResult> Get([FromODataUri] string key, ODataQueryOptions<MeasurementModel> queryOptions)
        {
            IQueryable<MeasurementModel> measurements;
            // validate the query.
            try
            {
                queryOptions.Validate(_validationSettings);

                measurements = await r.GetMeasurementsAsync(key);
            }
            catch (ODataException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (KeyNotFoundException kex)
            {
                return BadRequest(kex.Message);
            }
            catch (UnauthorizedAccessException uex)
            {
                return BadRequest(uex.Message);
            }
            return Ok<IQueryable<MeasurementModel>>(measurements);
        }

        // PUT: odata/Measurements(5)
        public async Task<IHttpActionResult> Put([FromODataUri] long key, Delta<MeasurementModel> delta)
        {
            Validate(delta.GetEntity());

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // TODO: Get the entity here.

            // delta.Put(measurementModel);

            // TODO: Save the patched entity.

            // return Updated(measurementModel);
            return StatusCode(HttpStatusCode.NotImplemented);
        }

        // POST: odata/Measurements
        public async Task<IHttpActionResult> Post(MeasurementModel measurementModel)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // TODO: Add create logic here.

            // return Created(measurementModel);
            return StatusCode(HttpStatusCode.NotImplemented);
        }

        // PATCH: odata/Measurements(5)
        [AcceptVerbs("PATCH", "MERGE")]
        public async Task<IHttpActionResult> Patch([FromODataUri] long key, Delta<MeasurementModel> delta)
        {
            Validate(delta.GetEntity());

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // TODO: Get the entity here.

            // delta.Patch(measurementModel);

            // TODO: Save the patched entity.

            // return Updated(measurementModel);
            return StatusCode(HttpStatusCode.NotImplemented);
        }

        // DELETE: odata/Measurements(5)
        public async Task<IHttpActionResult> Delete([FromODataUri] long key)
        {
            // TODO: Add delete logic here.

            // return StatusCode(HttpStatusCode.NoContent);
            return StatusCode(HttpStatusCode.NotImplemented);
        }

        protected override void Dispose(bool disposing)
        {
            r.Dispose();
            base.Dispose(disposing);
        }
    }
}
