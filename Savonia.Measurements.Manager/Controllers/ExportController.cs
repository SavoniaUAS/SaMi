using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Savonia.Measurements.Models;
using Savonia.Measurements.Database;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using System.IO;
using Savonia.Measurements.Manager.Models;
using System.Threading;
using Savonia.Measurements.Common;

namespace Savonia.Measurements.Manager.Controllers
{
    public class ExportController : Controller
    {
        private static readonly string querySessionKey = "query";
        public ActionResult Login()
        {
            MeasurementQueryModel model = Session[querySessionKey] as MeasurementQueryModel;
            ModelStateDictionary d= Session["modelstate"] as ModelStateDictionary;
            if (d != null)
            {
                foreach (var k in d)
                {
                    if (k.Value.Errors.Count>0)
                    {
                        ModelState.AddModelError(k.Key, k.Value.Errors[0]?.ErrorMessage);
                    } 
                }
            }
            if (null == model)
            {
                model = new MeasurementQueryModel();
            }
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Login(MeasurementQueryModel query)
        {
            if (!ModelState.IsValid)
            {
                return View("Login", query);
            }
            
            Repository r = new Repository();

            try
            {
                var queries =  r.GetQueries(query.Key);
                List<SavedQueryModel> savedQueries = new List<SavedQueryModel>();
                if (null != queries)
                {
                    foreach (var q in queries)
                    {
                        savedQueries.Add(new SavedQueryModel(q));
                    }
                }
                ViewBag.Queries = savedQueries;
                var objects =  await r.GetDistinctObjectsAsync(query.Key);
                ViewBag.Objects = objects;
                var sensorsTask = r.GetSensorsAsync(query.Key);
                ObservableCollection<SensorModel> sensors = new ObservableCollection<SensorModel>(await sensorsTask);
                ViewBag.Sensors = sensors;
                Helpers.LoginHelper lh = new Helpers.LoginHelper();
                lh.CreateLoginCredentials(this.HttpContext,query.Key);         
            }
            catch (KeyNotFoundException kex)
            {
                Savonia.Web.ErrorReporter.ErrorReporterModule.HandleException(kex, System.Web.HttpContext.Current.Request);
                ModelState.AddModelError("Key", kex.Message);
                return View();
            }
            catch (UnauthorizedAccessException uex)
            {
                Savonia.Web.ErrorReporter.ErrorReporterModule.HandleException(uex, System.Web.HttpContext.Current.Request);
                ModelState.AddModelError("Key", uex.Message);
                return View();
            }
            return View("Get");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Measurements(MeasurementQueryModel query, CancellationToken t)
        {
            CancellationToken disconnectedToken = Response.ClientDisconnectedToken;
            var timeOutToken = Request.TimedOutToken;
            var source = CancellationTokenSource.CreateLinkedTokenSource(t, disconnectedToken,timeOutToken);
            Helpers.LoginHelper lh = new Helpers.LoginHelper();
            query.Key = lh.GetAuthenticationKey(this.HttpContext);
            if (query.Key != null)
            {
               ModelState.Remove("key");
               ModelState.Add("Key", new ModelState());
               ModelState.SetModelValue("Key", new ValueProviderResult(query.Key, query.Key, System.Globalization.CultureInfo.CurrentCulture));
            }

            if (!ModelState.IsValid)
            {
               return RedirectToAction("RedirectToLogin");
            }
            Repository r = new Repository();

            try
            {                             
                var measurements = await r.GetMeasurementsAsync(query,source.Token, true);
                if (measurements?.Count > 0 && !source.IsCancellationRequested)
                {
                   
                    ViewBag.DataCount = measurements[0].Data.Count;
                    Helpers.DocumentHelper s = new Helpers.DocumentHelper();
                    string filename = Session.SessionID + ".json";
                    string directoryPath = Server.MapPath(Common.GlobalSettings.GetJSONPath);
                    s.SerializeJSONFile<List<MeasurementModel>>(measurements, directoryPath, filename);
                    ViewBag.DatetimeFormats = Helpers.DatetimeFormatHelper.GetPossibleDatetimeFormats();
                    ViewBag.ExportModel = new Models.ExportModel();
                    ViewBag.MeasurementCount = measurements.Count;
                }
                else
                {
                    source.Dispose();
                    return Content("<span class='text-danger'>0 measurements found with given parameters.</span>");
                }
                if (measurements.Count > 3000)
                {
                    var m = measurements.Take(3000);
                    measurements = m.ToList();
                }
                
                return PartialView("~/Views/Export/Export.cshtml", measurements);
            }
            catch (KeyNotFoundException kex)
            {
                Savonia.Web.ErrorReporter.ErrorReporterModule.HandleException(kex, System.Web.HttpContext.Current.Request);
                return RedirectToAction("RedirectToLogin");
            }
            catch (UnauthorizedAccessException uex)
            {
                Savonia.Web.ErrorReporter.ErrorReporterModule.HandleException(uex, System.Web.HttpContext.Current.Request);
                return RedirectToAction("RedirectToLogin");
            }
            catch (OperationCanceledException canceled)
            {
                Savonia.Web.ErrorReporter.ErrorReporterModule.HandleException(canceled, System.Web.HttpContext.Current.Request);
                source.Dispose();
                return Content("<span class='text-danger'>Request cancelled.</span>");
            }
            catch (Exception ex)
            {
                Savonia.Web.ErrorReporter.ErrorReporterModule.HandleException(ex, System.Web.HttpContext.Current.Request);
                return RedirectToAction("RedirectToLogin");
            }
        }

        public async Task<ActionResult> SaveQuery(MeasurementQueryModel query)
        {
            string c = "";
            if (string.IsNullOrEmpty(query?.Name))
            {
                ModelState.AddModelError("Name", "Name field is required!");
                c = " <div class='alert alert-warning fade in'><a class='close' data-dismiss='alert' aria-label='close'>&times;</a>Name field is required</div>";
                return Content(c);
            }
            Helpers.LoginHelper lh = new Helpers.LoginHelper();
            query.Key = lh.GetAuthenticationKey(this.HttpContext);
            if (query.Key != null)
            {
                ModelState.Remove("key");
                ModelState.Add("Key", new ModelState());
                ModelState.SetModelValue("Key", new ValueProviderResult(query.Key, query.Key, System.Globalization.CultureInfo.CurrentCulture));
            }
            c = " <div class='alert alert-danger fade in'><a class='close' data-dismiss='alert' aria-label='close'>&times;</a><span class='text-info'><span class='text-success'>Query: " + query.Name + " was <strong>NOT</strong> Saved successfully!</span></span></div>";
            if (!ModelState.IsValid)
            {
                return Content(c);
            }
            Repository r = new Repository();
            int result = 0;
            SavedQueryModel savedQuery = new SavedQueryModel();
            try
            {
                result = await r.SaveQueryAsync(query);
                if (result > 0)
                {
                    query.ID = result;
                    savedQuery = new SavedQueryModel(query);
                }
                else if (result == -1)
                {
                    c = " <div class='alert alert-danger fade in'><a class='close' data-dismiss='alert' aria-label='close'>&times;</a><span class='text-info'><span class='text-success'>Queries cannot be saved with Master key!</span></span></div>";
                    return Content(c);
                }
                else
                {
                    return Content(c);
                }
            }
            catch (Exception ex)
            {
                Savonia.Web.ErrorReporter.ErrorReporterModule.HandleException(ex, System.Web.HttpContext.Current.Request);
                return Content(c);
            }
            return Json(savedQuery);
        }

        public async Task<ActionResult> DeleteQuery([System.Web.Http.FromBody]MeasurementQueryModel query)
        {

            Helpers.LoginHelper lh = new Helpers.LoginHelper();
            query.Key = lh.GetAuthenticationKey(this.HttpContext);
            if (query.Key != null)
            {
                ModelState.Remove("key");
                ModelState.Add("Key", new ModelState());
                ModelState.SetModelValue("Key", new ValueProviderResult(query.Key, query.Key, System.Globalization.CultureInfo.CurrentCulture));
            }
            string c = " <div class='alert alert-danger fade in'><a class='close' data-dismiss='alert' aria-label='close'>&times;</a><span class='text-info'><span class='text-success'>Query: " + query.Name + " was <strong>NOT</strong> deleted successfully!</span></span></div>";
            if (!ModelState.IsValid)
            {
                return Content(c);
            }
            Repository r = new Repository();
            int result = 0;
            SavedQueryModel savedQuery = new SavedQueryModel();
            try
            {
                result = await r.DeleteQueryAsync(query);
                if (result > 0)
                {
                    savedQuery = new SavedQueryModel(query);
                }
                else
                {
                    return Content(c);
                }
            }
            catch (Exception ex)
            {
                Savonia.Web.ErrorReporter.ErrorReporterModule.HandleException(ex, System.Web.HttpContext.Current.Request);
                return Content(c);
            }
            return Json(savedQuery);
        }
        /// <summary>
        /// Returns error with url that can be used to redirection if ajaxScript.js is used
        /// </summary>
        /// <returns></returns>
        public ActionResult RedirectToLogin()
        {
            UrlHelper u = new UrlHelper(this.ControllerContext.RequestContext);
            string url = u.Action("Login", "Export", null, this.Request.Url.Scheme);
            url += "Export/Login";
            this.HttpContext.Session.Clear();
            ModelState.Clear();
            ModelState.AddModelError("Key", "Your session expired!");
            Session["modelstate"] = ModelState;
            return HttpNotFound(url);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ExportFile(ExportModel options)
        {
            try
            {
                List<DataModel> data = new List<DataModel>();
                string filename = Session.SessionID;
                string path = "";
                string fullPath = "";
                Helpers.DocumentHelper s = new Helpers.DocumentHelper();
                filename += ".json";
                path = Server.MapPath(Common.GlobalSettings.GetJSONPath);
                fullPath = path + filename;
                List<MeasurementModel> measurements = s.DezerializeJSONFile<List<MeasurementModel>>(fullPath);

                if (measurements == null || measurements?.Count < 1)
                {
                    return new HttpNotFoundResult("Data Cant be read");
                }
                string csvFilename = Session.SessionID + ".csv";
                string csvPath = Server.MapPath(Common.GlobalSettings.GetCsvPath);
                string csvFullPath = csvPath + csvFilename;
                Helpers.CsvExportHelper ceh = new Helpers.CsvExportHelper();
                ceh.CreateCsvFile(csvFilename, csvPath, options, measurements);
                //HACK: Possible out of memory if file exceeds maximum storage size of byte array! should this be configured on server IIS?
                byte[] filedata = System.IO.File.ReadAllBytes(csvFullPath);
                Response.AddHeader("Content-Disposition", ceh.CreateContentDisposition("Measurements.csv",filename,path).ToString());
                string contentType = MimeMapping.GetMimeMapping(csvFullPath);
                System.IO.File.Delete(csvFullPath);
                return File(filedata, contentType);
            }
            catch (IOException ioex)
            {
                Savonia.Web.ErrorReporter.ErrorReporterModule.HandleException(ioex, System.Web.HttpContext.Current.Request);
                return new HttpNotFoundResult("Data Cant be read");
            }
            catch (UnauthorizedAccessException uaex)
            {
                Savonia.Web.ErrorReporter.ErrorReporterModule.HandleException(uaex, System.Web.HttpContext.Current.Request);
                return new HttpUnauthorizedResult("Data Cant be read");

            }
            catch (ArgumentException arex)
            {
                Savonia.Web.ErrorReporter.ErrorReporterModule.HandleException(arex, System.Web.HttpContext.Current.Request);
                return new HttpNotFoundResult("Data Cant be read");

            }
            catch (NotSupportedException nsupex)
            {
                Savonia.Web.ErrorReporter.ErrorReporterModule.HandleException(nsupex, System.Web.HttpContext.Current.Request);
                return new HttpNotFoundResult("Data Cant be read");

            }
            catch (Exception ex)
            {
                Savonia.Web.ErrorReporter.ErrorReporterModule.HandleException(ex, System.Web.HttpContext.Current.Request);
                return new HttpNotFoundResult("Data Cant be read");
            }
        }
    }
}
