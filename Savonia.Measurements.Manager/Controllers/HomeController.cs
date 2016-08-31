using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Savonia.Measurements.Models;
using Savonia.Measurements.Database;
using System.Threading.Tasks;

namespace Savonia.Measurements.Manager.Controllers
{
    public class HomeController : Controller
    {
        private static readonly string querySessionKey = "query";

        public ActionResult Index()
        {
            MeasurementQueryModel model = Session[querySessionKey] as MeasurementQueryModel;
            if (null == model)
            {
                model = new MeasurementQueryModel();
            }
            return View(model);
        }

        public async Task<ActionResult> Measurements(MeasurementQueryModel query)
        {
            var sessionQuery = Session[querySessionKey] as MeasurementQueryModel;

            if (!ModelState.IsValid)
            {
                if (null == sessionQuery)
                {
                    return View("Index", query);
                }
                query = sessionQuery;
            }
            Repository r = new Repository();

            try
            {
                var modelTask = r.GetMeasurementsAsync(query);
                var sensorsTask = r.GetSensorsAsync(query.Key);

                ViewBag.Sensors = await sensorsTask;
                Session[querySessionKey] = query;
                return View(await modelTask);
            }
            catch (KeyNotFoundException kex)
            {
                return HttpNotFound(kex.Message);
            }
            catch (UnauthorizedAccessException uex)
            {
                return new HttpUnauthorizedResult(uex.Message);
            }
        }

        public async Task<ActionResult> Details(long id)
        {
            Repository r = new Repository();
            MeasurementQueryModel queryModel = Session[querySessionKey] as MeasurementQueryModel;
            try
            {
                string key = null;
                List<string> sensors = null;
                if (null != queryModel)
                {
                    key = queryModel.Key;
                    sensors = queryModel.DataTags();
                }
                var model = await r.GetMeasurementAsync(key, id, sensors);

                return View(model);
            }
            catch (KeyNotFoundException kex)
            {
                return HttpNotFound(kex.Message);
            }
            catch (UnauthorizedAccessException uex)
            {
                return new HttpUnauthorizedResult(uex.Message);
            }
        }

        public async Task<ActionResult> Delete(long id)
        {
            Repository r = new Repository();
            MeasurementQueryModel queryModel = Session[querySessionKey] as MeasurementQueryModel;
            try
            {
                var model = await r.GetMeasurementAsync(null != queryModel ? queryModel.Key : null, id, null);

                return View(model);
            }
            catch (KeyNotFoundException kex)
            {
                return HttpNotFound(kex.Message);
            }
            catch (UnauthorizedAccessException uex)
            {
                return new HttpUnauthorizedResult(uex.Message);
            }
        }

        [HttpPost]
        [ActionName("Delete")]
        public async Task<ActionResult> ConfirmDelete(long id)
        {
            Repository r = new Repository();
            MeasurementQueryModel queryModel = Session[querySessionKey] as MeasurementQueryModel;
            try
            {
                await r.DeleteMeasurementAsync(null != queryModel ? queryModel.Key : null, id);

                return RedirectToAction("Measurements");
            }
            catch (KeyNotFoundException kex)
            {
                return HttpNotFound(kex.Message);
            }
            catch (UnauthorizedAccessException uex)
            {
                return new HttpUnauthorizedResult(uex.Message);
            }
        }

        public ActionResult Help()
        {
            return View();
        }

    }
}