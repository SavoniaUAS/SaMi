using Microsoft.VisualStudio.TestTools.UnitTesting;
using Savonia.Measurements.Database;
using Savonia.Measurements.Database.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Savonia.Measurements.Models;

namespace TestClients
{
    [TestClass]
    public class MetaTests
    {

        [TestMethod]
        public async Task AddMetaIdeas()
        {
            Stopwatch sw = Stopwatch.StartNew();
            using (MetaDataRepository db = new MetaDataRepository())
            {
                MetaObject mo;
                
               
               
                    mo = new MetaObject("P-X Idea", Guid.NewGuid().ToString());
                    mo.AddProperty<string>("Name", string.Format("name-{0}",1));
                    mo.AddProperty<string>("Description", string.Format("description-{0}",1));
                    mo.AddProperty<string>("Submitter", string.Format("submitter-{0}",1));

                    await db.SaveMetaDataAsync(mo);
              
            }
            sw.Stop();
            Trace.TraceInformation("AddMetaIdeas took {0} ms.", sw.Elapsed.TotalMilliseconds);
        }

        [TestMethod]
        public async Task AddProviderDataSensors()
        {
            List<SensorModel> sensors = new List<SensorModel> { new SensorModel { Name = "test", Description = "sad", ProviderID = 1, Tag = "tag", Unit = "" ,ValueDecimalCount=0}, new SensorModel { Name = "test1", Description = "sad", ProviderID = 1, Tag = "tag", Unit = "" , ValueDecimalCount = 0 } };
           await AddProviderDataList(1, "MeasurementSensors", sensors);
        }

        [TestMethod]
        public async Task AddProviderDataObjects()
        {
            List<string> objects = new List<string> { "object", "object1", "object2", "object3" };
            await AddProviderDataString(1, "MeasurementObjects", objects);
        }

        [TestMethod]
        public async Task AddProviderDataTags()
        {
            List<string> tags = new List<string> { "tag1", "tag2", "tag3", "tag4" };
            await AddProviderDataString(1, "MeasurementTags", tags);
        }

        /// <summary>
        /// Adds data to meta table using properties of data as tag value.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="providerID"></param>
        /// <param name="tag"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        [Obsolete]
        public async Task AddProviderDataSingle<T>(int providerID,List<T> data)
        {
            Stopwatch sw = Stopwatch.StartNew();

            var type = data[0].GetType();
            var properties = type.GetProperties();
            MetaObject mo = new MetaObject("ProviderData",providerID.ToString());
            for (int i = 0; i < data.Count; i++)
            {
                foreach (var prop in properties)
                {
                    try
                    {
                        if (prop.GetIndexParameters().Length == 0)
                        {
                            Trace.TraceInformation("   {0} ({1}): {2}", prop.Name,
                                                                          prop.PropertyType.Name,
                                                                          prop.GetValue(data[0]));
                            var propType = prop.PropertyType;
                            var value = prop.GetValue(data[i]);

                            System.Reflection.MethodInfo returnResult =
                           typeof(MetaObject)
                           .GetMethods().Single(m => m.Name == "AddProperty" && m.IsGenericMethod && m.ContainsGenericParameters
                           )
                           .MakeGenericMethod(propType);
                            returnResult.Invoke(mo, new object[] { prop.Name, value });
                        }
                        else
                        {
                            Trace.TraceInformation("   {0} ({1}): <Indexed>", prop.Name,
                                             prop.PropertyType.Name);
                        }

                    }
                    catch (Exception e)
                    {

                        Trace.TraceInformation(e.Message);
                    }
                }
            }
           

            using (MetaDataRepository db = new MetaDataRepository())
            {
               await db.SaveMetaDataAsync(mo);
            }
            sw.Stop();
            Trace.TraceInformation("AddMetaIdeas took {0} ms.", sw.Elapsed.TotalMilliseconds);
        }

        public async Task AddProviderDataList<T>(int providerID, string tag, List<T> data)
        {
            Stopwatch sw = Stopwatch.StartNew();

            var type = data[0].GetType();
            var properties = type.GetProperties();
            MetaObject mo = new MetaObject("ProviderData", providerID.ToString());
           
            mo.AddProperty<List<T>>(tag, (List<T>)data); 
            


            using (MetaDataRepository db = new MetaDataRepository())
            {

                await db.SaveMetaDataAsync(mo);
            }
            sw.Stop();
            Trace.TraceInformation("AddMetaIdeas took {0} ms.", sw.Elapsed.TotalMilliseconds);
        }

        public async Task AddProviderDataString(int providerID, string tag, List<string> data)
        {
            Stopwatch sw = Stopwatch.StartNew();

            var type = data[0].GetType();
            var properties = type.GetProperties();
            MetaObject mo = new MetaObject("ProviderData", providerID.ToString());
        
            mo.AddProperty<List<string>>(tag, (List<string>)data);
            
            using (MetaDataRepository db = new MetaDataRepository())
            {

                await db.SaveMetaDataAsync(mo);
            }
            sw.Stop();
            Trace.TraceInformation("AddMetaIdeas took {0} ms.", sw.Elapsed.TotalMilliseconds);
        }

        [TestMethod]
        public async Task GetIdeas()
        {
            
            Stopwatch sw = Stopwatch.StartNew();
            int count = 0;
            using (MetaDataRepository db = new MetaDataRepository())
            {
               
                var data = await db.GetMetaObjectsAsync("P-X Idea");
                count = data.Count;
                foreach (var i in data)
                {
                    //Trace.TraceInformation(i.ToString());
                    //Trace.TraceInformation("Decrypted: {0}", i.GetPropertyValue<string>("Name", decryptionKey: decryptionKey));
                    //Trace.TraceInformation("Decrypted: {0}", i.GetPropertyValue<string>("Description", decryptionKey: decryptionKey));
                    var name = i.GetPropertyValue<string>("Name");
                    var description = i.GetPropertyValue<string>("Description");
                    var submitter = i.GetPropertyValue<string>("Submitter");
                    Trace.TraceInformation($"Object on {i.Object} Name on {name}, Desc on {description}, submitter on {submitter}");
                }
            }
            sw.Stop();
            Trace.TraceInformation("{0} with P-{1} took {2} ms. Found {3} objects.", "random", "P-X Idea", sw.Elapsed.TotalMilliseconds, count);
            

        }

        [TestMethod]
        public async Task GetMeasurementTags()
        {
          await  GetProviderData(1, "MeasurementObjects");

        }


        [TestMethod]
        public async Task GetMeasurementSensors()
        {
            await GetProviderData<SensorModel>(1, "MeasurementSensors");

        }




        public async Task GetProviderData(int providerID,string tag)
        {

            Stopwatch sw = Stopwatch.StartNew();
            int count = 0;
            using (MetaDataRepository db = new MetaDataRepository())
            {

                var data = await db.GetMetaObjectsAsync("ProviderData",providerID.ToString(),tag);
                count = data.Count;
                foreach (var i in data)
                {
                    var objects = i.GetPropertyValue<List<string>>(tag);
                    string d = "";
                }
            }
            sw.Stop();
            Trace.TraceInformation("{0} with P-{1} took {2} ms. Found {3} objects.", "random", "P-X Idea", sw.Elapsed.TotalMilliseconds, count);


        }

        public async Task<List<T>> GetProviderData<T>(int providerID, string tag)
        {

            Stopwatch sw = Stopwatch.StartNew();
            List<T> metaData = new List<T>();
            int count = 0;
            using (MetaDataRepository db = new MetaDataRepository())
            {

                var data = await db.GetMetaObjectsAsync("ProviderData", providerID.ToString(), tag);
                count = data.Count;
                foreach (var i in data)
                {
                    var objects = i.GetPropertyValue<List<T>>(tag);
                    string d = "";
                }
            }
            sw.Stop();
            Trace.TraceInformation("{0} with P-{1} took {2} ms. Found {3} objects.", "random", "P-X Idea", sw.Elapsed.TotalMilliseconds, count);
            return metaData;

        }
        [TestMethod]
        public async Task GetDisctinctObjects()
        {
          await  GetDistinctObjects(2);
        }

        private async Task GetDistinctObjects(int providerID)
        {
            List<string> data = new List<string>();
            using (SavoniaMeasurementsV2Entities db = new SavoniaMeasurementsV2Entities())
            {
                try
                {
                     data = (from u in db.Measurements
                                where u.ProviderID.Equals(providerID)
                                select u.Object).Distinct().ToList();
                }
                catch (ArgumentNullException)
                {

                    throw;
                }
               
            }
        }

        [TestMethod]
        public async Task GetDisctinctTags()
        {
            await GetDistinctTags(2);
        }

        private async Task GetDistinctTags(int providerID)
        {
            List<string> data = new List<string>();
            using (SavoniaMeasurementsV2Entities db = new SavoniaMeasurementsV2Entities())
            {
                try
                {
                     data = (from u in db.Measurements
                                where u.ProviderID.Equals(providerID)
                                select u.Tag).Distinct().ToList();
                }
                catch (ArgumentNullException e)
                {

                    throw;
                }
                
            }
        }

        [TestMethod]
        public async Task UpdateSensors()
        {
         await   GetAndSetSensors();
        }
        
        public async Task GetAndSetSensors()
        {
            try
            {
                MeasurementQueryModel q = new MeasurementQueryModel();
                q.Key = "SK104-savoniatest";
                
               
                Repository r = new Repository();
               

                    var sensors = r.GetSensorsAsync(q.Key);
                    await AddProviderDataList(1, "MeasurementSensors", await sensors);
            
                
            }
            catch (Exception)
            {
              
            }
           

        }
        /*
       public static MetaObject ToIdeaMeta(this IdeaModel model, ICryptoTransform cryptor)
       {
           MetaObject mo = new MetaObject(IdeaModel.IdeaContext, model.Id.ToString());
           mo.AddProperty<string>("Name", model.Name, cryptor);
           mo.AddProperty<string>("Description", model.Description, cryptor);
           mo.AddProperty<string>("Submitter", model.Submitter, cryptor);

           // Note: model.AttachmentReferences are not saved here! 
           if (null != model.Properties)
           {
               foreach (var item in model.Properties)
               {
                   mo.AddProperty<string>(item.Key, item.Value, cryptor);
               }
           }
           return mo;
       }

      var model = idea.ToModel(await r.GetMetaObjectAsync(IdeaModel.IdeaContext, idea.ID.ToString()), decryptor);

       public static IdeaModel ToModel(this Idea idea, MetaObject meta, ICryptoTransform decryptor)
       {
           IdeaModel model = new IdeaModel()
           {
               Id = idea.ID,
               Timestamp = idea.Timestamp,
               IdeaCategoryId = idea.IdeaCategoryID,
               ProcessId = idea.ProcessID,
               SortIndex = idea.SortIndex
           };
           if (null != meta)
           {
               model.Name = meta.GetPropertyValue<string>("Name", decryptor: decryptor);
               model.Description = meta.GetPropertyValue<string>("Description", decryptor: decryptor);
               model.Submitter = meta.GetPropertyValue<string>("Submitter", decryptor: decryptor);
               model.AttachmentReferences = meta.GetPropertyValue<List<MetaReference>>(IdeaModel.IdeaAttachmentReferenceTagName);

               //Jos tulee useampi mitä ei ole luettu
               model.Properties = new Dictionary<string, string>();
               foreach (var p in meta.UnreadProperties)
               {
                   model.Properties.Add(p.Tag, meta.GetPropertyValue<string>(p, decryptor: decryptor));
               }
           }

           return model;
       }
       */


    }
}
