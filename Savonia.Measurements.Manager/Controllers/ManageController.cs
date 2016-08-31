using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Savonia.Measurements.Models;
using Savonia.Measurements.Database;
using System.Diagnostics;
using System.Threading.Tasks;
using Savonia.Measurements.Manager.Models;

namespace Savonia.Measurements.Manager.Controllers
{
    [SamiAuthorize(Permission = AccessControls.Admin)]
    public class ManageController : Controller
    {
        private const string SessionProviderIDKey = "providerID";
        // GET: Manage
        public ActionResult Index()
        {
            Repository r = new Repository();
            var model = r.GetProviders();
            return View(model);
        }
        public async Task<ActionResult> Details(int id)
        {
            Repository r = new Repository();
            var model = r.GetProvider(id);

            Helpers.ManageHelper mh = new Helpers.ManageHelper();
            model.Keys = mh.AddAccessKeysToSessionAndUpdate(this.HttpContext, model.Keys);
            mh.AddProviderKeyToSession(this.HttpContext, model);
            model.Key = "******";
            foreach (var k in model.Keys)
            {
                k.Key = "******";
            }
            var sensors = await r.GetSensorsAsync(id);
            ViewBag.Sensors = sensors;
            ViewBag.ProviderID = id;
            return View(model);
        }
        public async Task<ActionResult> GetKey(short? data)
        {
            Helpers.ManageHelper mh = new Helpers.ManageHelper();
            string key = "";
            try
            {
                if (data.HasValue)
                {
                    key = mh.GetAccessKeyFromSession(this.HttpContext, data.Value);
                }
                else
                {
                    key = mh.GetProviderKeyFromSession(this.HttpContext);
                }
                if (key == null || key == "")
                {
                    return HttpNotFound("Key not found go to the index page");
                }
            }
            catch (Exception ex)
            {
                Savonia.Web.ErrorReporter.ErrorReporterModule.HandleException(ex, System.Web.HttpContext.Current.Request);
                return HttpNotFound("Key not found go to the index page");
            }
            return Json(key);
        }


        private string GetKey()
        {
            int maxTries = 5;
            int count = 0;
            string key;
            Repository r = new Repository();

            do
            {
                key = Savonia.Measurements.Database.Helpers.DBHelper.GenerateKey(DateTime.Now.Year.ToString(), DateTime.Now.ToString("MM-dd"));
                if (r.IsNewProviderKeyValid(key))
                {
                    break;
                }
            } while (count++ < maxTries);

            return key;
        }

        // GET: Manage/Create
        public ActionResult Create()
        {
            ProviderModel model = new ProviderModel();
            model.Key = this.GetKey();
            model.CreatedBy = User.Identity.Name;
            model.ActiveFrom = DateTime.Now;
            model.ActiveTo = model.DataStorageUntil = DateTime.Now.AddYears(1);

            return View(model);
        }

        // POST: Manage/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(ProviderModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            try
            {
                Repository r = new Repository();
                if (!r.IsNewProviderKeyValid(model.Key))
                {
                    ModelState.AddModelError("Key", "Provided key is not valid.");
                    return View(model);
                }
                model.Created = DateTime.Now;
                r.SaveNewProvider(model);
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                Savonia.Web.ErrorReporter.ErrorReporterModule.HandleException(ex, System.Web.HttpContext.Current.Request);
                return View(model);
            }
        }

        // GET: Manage/Edit/5
        public ActionResult Edit(int id)
        {
            Repository r = new Repository();
            var model = r.GetProvider(id);
            Session[SessionProviderIDKey] = id;
            return View(model);
        }

        // POST: Manage/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(int id, ProviderModel model)
        {
            try
            {
                Repository r = new Repository();
                await r.UpdateProviderAsync(id, model);
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                Savonia.Web.ErrorReporter.ErrorReporterModule.HandleException(ex, System.Web.HttpContext.Current.Request);
                return View();
            }
        }

        public async Task<ActionResult> EditAccessKey(int providerID, short? id)
        {
            Repository r = new Repository();
            Helpers.ManageHelper mh = new Helpers.ManageHelper();
            AccessKeyModel model = new AccessKeyModel();
            try
            {
                var key = mh.GetAccessKeyFromSession(this.HttpContext, id.Value);
                if (key == null)
                {
                    return RedirectToAction("Index");
                }
                model = await r.GetAccessKeyAsync(key);
                if (model.ProviderID != providerID)
                {
                    return RedirectToAction("Details", new { id = providerID });
                }
            }
            catch (Exception ex)
            {
                Savonia.Web.ErrorReporter.ErrorReporterModule.HandleException(ex, System.Web.HttpContext.Current.Request);
                return RedirectToAction("Index");
            }
            model.Key = "*****";
            model.KeyId = id;
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> EditAccessKey(AccessKeyModel model)
        {
            if (model.KeyId.HasValue)
            {
                Helpers.ManageHelper mh = new Helpers.ManageHelper();
                model.Key = mh.GetAccessKeyFromSession(this.HttpContext, model.KeyId.Value, false);
                if (model.Key != null)
                {
                    ModelState.Remove("key");
                    ModelState.Add("Key", new ModelState());
                    ModelState.SetModelValue("Key", new ValueProviderResult(model.Key, model.Key, System.Globalization.CultureInfo.CurrentCulture));
                }
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }
            Repository r = new Repository();
            try
            {
                await r.UpdateAccessKeyAsync(model);
            }
            catch (Exception ex)
            {
                Savonia.Web.ErrorReporter.ErrorReporterModule.HandleException(ex, System.Web.HttpContext.Current.Request);
                ModelState.AddModelError("Key", "Unknown error occurred!");
                model.Key = "*****";
                return View(model);
            }

            return RedirectToAction("Details", new { id = model.ProviderID });

        }

        // GET: Manage/Delete/5
        public async Task<ActionResult> Delete(int id)
        {
            Repository r = new Repository();
            ProviderModel model = new ProviderModel();
            try
            {
                var m = r.GetProvider(id);
                if (m != null)
                {
                    model = m;
                    var counts = await r.GetProviderDataCounts(id);
                    if (counts.ContainsKey("AccessKeys"))
                    {
                        ViewBag.AccessKeyCount = counts["AccessKeys"];
                    }
                    if (counts.ContainsKey("Measurements"))
                    {
                        ViewBag.MeasurementCount = counts["Measurements"];
                    }
                    if (counts.ContainsKey("Sensors"))
                    {
                        ViewBag.SensorCount = counts["Sensors"];
                    }
                }

            }
            catch (Exception ex)
            {
                Savonia.Web.ErrorReporter.ErrorReporterModule.HandleException(ex, System.Web.HttpContext.Current.Request);
                return RedirectToAction("Index");
            }

            return View(model);
        }

        // POST: Manage/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Delete(int id, FormCollection collection)
        {
            try
            {
                Repository r = new Repository();
                await r.DeleteProviderAsync(id);
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                Savonia.Web.ErrorReporter.ErrorReporterModule.HandleException(ex, System.Web.HttpContext.Current.Request);
                return RedirectToAction("Index");
            }
        }

        public ActionResult CreateKey(int providerID)
        {
            AccessKeyModel model = new AccessKeyModel()
            {
                ProviderID = providerID,
                ValidFrom = DateTime.Now,
                Key = this.GetKey()
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> CreateKey(AccessKeyModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            Repository r = new Repository();
            try
            {
                if (!await r.TryCreateNewAccessKeyAsync(model))
                {
                    ModelState.AddModelError("Key", "Provided key is not valid!");
                    return View(model);
                }
            }
            catch (Exception ex)
            {
                Savonia.Web.ErrorReporter.ErrorReporterModule.HandleException(ex, System.Web.HttpContext.Current.Request);
                ModelState.AddModelError("Key", "Unknown error occurred!");
                return View(model);
            }
            return RedirectToAction("Details", new { id = model.ProviderID });
        }

        public async Task<ActionResult> DeleteKey(int providerID, short? id)
        {
            Repository r = new Repository();
            Helpers.ManageHelper mh = new Helpers.ManageHelper();
            AccessKeyModel model = new AccessKeyModel();
            try
            {
                var key = mh.GetAccessKeyFromSession(this.HttpContext, id.Value);
                if (key == null)
                {
                    return RedirectToAction("Index");
                }
                model = await r.GetAccessKeyAsync(key);
            }
            catch (Exception ex)
            {
                Savonia.Web.ErrorReporter.ErrorReporterModule.HandleException(ex, System.Web.HttpContext.Current.Request);
                return RedirectToAction("Index");
            }
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("DeleteKey")]
        public async Task<ActionResult> ConfirmDeleteKey(AccessKeyModel model)
        {
            Repository r = new Repository();
            try
            {
                await r.DeleteAccessKeyAsync(model);
            }
            catch (Exception ex)
            {
                Savonia.Web.ErrorReporter.ErrorReporterModule.HandleException(ex, System.Web.HttpContext.Current.Request);
                return View(model);
            }
            return RedirectToAction("Details", new { id = model.ProviderID });
        }

        public ActionResult ReHashKeys()
        {
            return View();
        }

        public ActionResult CreateSensor(int providerID)
        {
            ViewBag.ProviderID = providerID;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> CreateSensor(SensorModel model)
        {
            //create sensor 
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            Repository r = new Repository();
            try
            {
                if (!await r.TryCreateNewSensorAsync(model))
                {
                    ModelState.AddModelError("Tag", "Provided tag is not valid!");
                    return View(model);
                }
            }
            catch (Exception ex)
            {
                Savonia.Web.ErrorReporter.ErrorReporterModule.HandleException(ex, System.Web.HttpContext.Current.Request);
                return View(model);
            }


            return RedirectToAction("Details", new { id = model.ProviderID });
        }

        public ActionResult EditSensor(int providerID, string tag)
        {
            Repository r = new Repository();
            var model = r.GetSensor(providerID, tag);
            ViewBag.ProviderID = providerID;
            return View(model);
        }

        [HttpPost]
        public async Task<ActionResult> EditSensor(SensorModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            Repository r = new Repository();
            try
            {
                await r.UpdateSensorAsync(model);
            }
            catch (Exception ex)
            {
                Savonia.Web.ErrorReporter.ErrorReporterModule.HandleException(ex, System.Web.HttpContext.Current.Request);
                ModelState.AddModelError("Tag", "Do not change Tag or ID values!");
                return View(model);
            }
            return RedirectToAction("Details", new { id = model.ProviderID });
        }

        public ActionResult DeleteSensor(int providerID, string tag)
        {
            Repository r = new Repository();
            SensorModel model = new SensorModel();
            try
            {
                var m = r.GetSensor(providerID, tag);
                if (m == null)
                {
                    return RedirectToAction("Details", new { id = providerID });
                }
                model = m;
            }
            catch (Exception ex)
            {
                Savonia.Web.ErrorReporter.ErrorReporterModule.HandleException(ex, System.Web.HttpContext.Current.Request);
                return RedirectToAction("Details", new { id = providerID });
            }
            return View(model);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("DeleteSensor")]
        public async Task<ActionResult> DeleteSensor(SensorModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            Repository r = new Repository();
            try
            {
                await r.DeleteSensorAsync(model.ProviderID, model.Tag);
            }
            catch (Exception ex)
            {
                Savonia.Web.ErrorReporter.ErrorReporterModule.HandleException(ex, System.Web.HttpContext.Current.Request);
                ModelState.AddModelError("Tag", "Do not change Tag or ID values, if you wish to delete sensor!");
                return View(model);
            }
            return RedirectToAction("Details", new { id = model.ProviderID });
        }

        [HttpPost]
        public async Task<ActionResult> ReHashKeys(bool? makeHashs)
        {
            if (makeHashs.HasValue && makeHashs.Value)
            {
                Repository r = new Repository();
                await r.ReHashAllKeysAsync();
            }
            return RedirectToAction("Index");
        }
    }
}
