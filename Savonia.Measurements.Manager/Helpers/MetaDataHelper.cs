using Savonia.Measurements.Database;
using Savonia.Measurements.Database.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace Savonia.Measurements.Manager.Helpers
{
    /// <summary>
    /// This class was used to access providers data from meta table, but it was deprecated until there is need to store providers data to meta.
    /// If distinct queries in repository are too slow there might be need to start using this class 
    /// </summary>
    [Obsolete("Was used for testing and currently not used.")]
    public class MetaDataHelper
    {
        

      /*  public async Task<List<string>> GetProviderData(int providerID, string tag)
        {
            List<string> data = new List<string>();
            int count = 0;

            Repository r = new Repository();

            using (MetaDataRepository db = new MetaDataRepository())
            {
                //basically this returns always 1 metaobject since the tag is defined 
                var metaObjects = await db.GetMetaObjectsAsync("ProviderData", providerID.ToString(), tag);
                count = metaObjects.Count;
                foreach (var i in metaObjects)
                {
                    var objects = i.GetPropertyValue<List<string>>(tag);
                    data.AddRange(objects.Except(data));
                }
            }
            return data;
        }

            /// <summary>
            /// Returns list of ProviderData from meta table with given tag and provider id.
            /// </summary>
            /// <typeparam name="T"></typeparam>
            /// <param name="providerID"></param>
            /// <param name="tag"></param>
            /// <returns></returns>
        public async Task<List<T>> GetProviderData<T>(int providerID, string tag)
        {

            List<T> data = new List<T>();
            int count = 0;
            using (MetaDataRepository db = new MetaDataRepository())
            {

                var metaObject = await db.GetMetaObjectsAsync("ProviderData", providerID.ToString(), tag);
                count = metaObject.Count;
                foreach (var i in metaObject)
                {
                    var objects = i.GetPropertyValue<List<T>>(tag);
                    data.AddRange(objects.Except(data));
                }
            }

            return data;
        }

        /// <summary>
        /// Adds data to meta table
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="providerID"></param>
        /// <param name="tag"></param>
        /// <param name="data"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        private async Task AddProviderDataList<T>(int providerID, string tag, List<T> data, string context = "ProviderData")
        {
            MetaObject mo = new MetaObject(context, providerID.ToString());

            mo.AddProperty<List<T>>(tag, (List<T>)data);

            using (MetaDataRepository db = new MetaDataRepository())
            {
                await db.SaveMetaDataAsync(mo);
            }
        }


            private async Task AddProviderDataString(int providerID, string tag, List<string> data, string context = "ProviderData")
            {
                MetaObject mo = new MetaObject(context, providerID.ToString());

                mo.AddProperty<List<string>>(tag, (List<string>)data);

                using (MetaDataRepository db = new MetaDataRepository())
                {
                    await db.SaveMetaDataAsync(mo);
                }
            }
     /// <summary>
     /// Updates measurement object in meta-table with given provider id.
     /// </summary>
     /// <param name="providerID"></param>
     /// <returns></returns>
        public async Task UpdateObjectInMeta(int providerID)
        {
            try
            {
                Repository r = new Repository();
                var objects = await r.GetDistinctObjectsAsync(providerID);
                //var objects = await r.GetDistinctObjectsAsync("SK104-savoniatest");
                var objectKey = Savonia.Measurements.Common.Constants.MetaObjectKey;
                await AddProviderDataList<string>(providerID, objectKey, objects);
            }
            catch (Exception)
            {

                throw;
            }

        }

        /// <summary>
        /// Updates measurement tags in meta-table with given provider id.
        /// </summary>
        /// <param name="providerID"></param>
        /// <returns></returns>
        public async Task UpdateTagsInMeta(int providerID)
        {
            try
            {
                Repository r = new Repository();
                var tags = await r.GetDistinctTagsAsync(providerID);
                //var tags = await r.GetDistinctTagsAsync("SK104-savoniatest");
                var tagKey = Savonia.Measurements.Common.Constants.MetaTagKey;
                await AddProviderDataList<string>(providerID, tagKey, tags);
            }
            catch (Exception)
            {

                throw;
            }

        }

        /// <summary>
        /// Updates sensor list in meta-table with given provider id.
        /// </summary>
        /// <param name="providerID"></param>
        /// <returns></returns>
        [Obsolete]
        public async Task UpdateSensorsInMeta(int providerID)
        {
            try
            {
                Repository r = new Repository();
                var sensors = r.GetSensorsAsync(providerID);
                var sensorKey = System.Configuration.ConfigurationManager.AppSettings[Helpers.WebConfigHelper.MetaSensorKey];
                await AddProviderDataList(providerID, sensorKey, await sensors);
            }
            catch (Exception)
            {
                throw;
            }

    
        }*/
    }
}