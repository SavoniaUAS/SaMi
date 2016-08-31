using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Savonia.Measurements.Manager.Models;
using Savonia.Measurements.Database;
using Savonia.Measurements.Models;
using Savonia.Measurements.Models.Helpers;
using System.Threading.Tasks;

namespace Savonia.Measurements.Manager.Controllers
{
    public class ImportController : Controller
    {
        // GET: Data
        public ActionResult Index()
        {
            RawDataModel model = new RawDataModel();
            return View(model);
        }

        [HttpPost]
        public async Task<ActionResult> Parse(RawDataModel data)
        {
            ParsedDataModel model = new ParsedDataModel();
            if (null == data)
            {
                return View(model);
            }

            model.Key = data.Key;
            model.DataFieldType = DataField.Value;
            List<SensorModel> sensors = null;
            if (!string.IsNullOrEmpty(model.Key))
            {
                Repository r = new Repository();

                sensors = await r.GetSensorsAsync(model.Key);
            }

            model.Data = this.ParseRawData(data);
            this.PopulateFieldMap(model, sensors);

            return View(model);
        }


        private void PopulateFieldMap(ParsedDataModel model, List<SensorModel> sensors)
        {
            var dataRow = model.Data.FirstOrDefault();
            if (null != dataRow)
            {
                if (model.FieldMap.Count > dataRow.Count)
                {
                    model.FieldMap = model.FieldMap.Take(dataRow.Count).ToList();
                }
                else
                {
                    int sensorIndex = 0;
                    int sensorCount = null != sensors ? sensors.Count : 0;
                    for (int i = model.FieldMap.Count; i < dataRow.Count; i++)
                    {
                        if (sensorIndex < sensorCount)
                        {
                            model.FieldMap.Add(sensors[sensorIndex++].Tag);
                        }
                        else
                        {
                            model.FieldMap.Add("");
                        }
                    }
                }
            }

        }

        private List<List<string>> ParseRawData(RawDataModel data)
        {
            List<List<string>> model = null;
            if (!string.IsNullOrEmpty(data.RawData))
            {
                var rows = data.RawData.Split(Environment.NewLine.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                model = new List<List<string>>(rows.Length);
                string[] d = null;
                var separators = string.IsNullOrEmpty(data.Separators) ? RawDataModel.DefaultSeparators.ToCharArray() : data.Separators.ToCharArray();
                foreach (var r in rows)
                {
                    d = r.Split(separators);
                    model.Add(d.ToList());
                }
            }
            else
            {
                model = new List<List<string>>();
            }
            return model;
        }


        [HttpPost]
        public async Task<ActionResult> SaveParsed(ParsedDataModel data)
        {
            if (null == data || string.IsNullOrEmpty(data.Key))
            {
                ModelState.AddModelError("Key", "Key cannot be empty!");
                return View("Parse", data);
            }

            data.Data = this.ParseRawData(data);

            Repository r = new Repository();

            List<MeasurementModel> measurements = new List<MeasurementModel>();
            MeasurementModel mm;
            int timeIndex = data.FieldMap.IndexOf(ParsedDataModel.MeasurementTimeField);
            int objectIndex = data.FieldMap.IndexOf(ParsedDataModel.MeasurementObjectField);
            int tagIndex = data.FieldMap.IndexOf(ParsedDataModel.MeasurementTagField);

            foreach (var row in data.Data)
            {
                mm = new MeasurementModel()
                {
                    Timestamp = timeIndex > -1 ? row[timeIndex].ToDateTimeOffset() : DateTimeOffset.Now,
                    Object = objectIndex > -1 ? row[objectIndex] : null,
                    Tag = tagIndex > -1 ? row[tagIndex] : null,
                    Data = new List<DataModel>()
                };

                DataModel d;
                switch (data.DataFieldType)
                { 
                    case DataField.LongValue:
                        for (int i = 0; i < row.Count; i++)
                        {
                            if (i != timeIndex && i != objectIndex && i != tagIndex)
                            {
                                if (string.IsNullOrEmpty(data.FieldMap[i]))
                                {
                                    continue;
                                }
                                d = new DataModel()
                                {
                                    Tag = data.FieldMap[i],
                                    LongValue = row[i].ToLong()
                                };

                                mm.Data.Add(d);
                            }
                        }
                        break;
                    case DataField.Value:
                        for (int i = 0; i < row.Count; i++)
                        {
                            if (i != timeIndex && i != objectIndex && i != tagIndex)
                            {
                                if (string.IsNullOrEmpty(data.FieldMap[i]))
                                {
                                    continue;
                                }
                                d = new DataModel()
                                {
                                    Tag = data.FieldMap[i],
                                    Value = row[i].ToDouble()
                                };

                                mm.Data.Add(d);
                            }
                        }
                        break;
                    default:
                        for (int i = 0; i < row.Count; i++)
                        {
                            if (string.IsNullOrEmpty(data.FieldMap[i]))
                            {
                                continue;
                            }
                            if (i != timeIndex && i != objectIndex && i != tagIndex)
                            {
                                d = new DataModel()
                                {
                                    Tag = data.FieldMap[i],
                                    TextValue = row[i]
                                };

                                mm.Data.Add(d);
                            }
                        }
                        break;
                }
                measurements.Add(mm);
            }

            SaveResult result = null;
            try
            {
                result = await r.SaveMeasurementsAsync(data.Key, measurements.ToArray());
            }
            catch (KeyNotFoundException kex)
            {
                return HttpNotFound(kex.Message);
            }
            catch (UnauthorizedAccessException uex)
            {
                return HttpNotFound(uex.Message);
            }

            return View(result);
        }
    }
}