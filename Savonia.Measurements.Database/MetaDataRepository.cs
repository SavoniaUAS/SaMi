using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Threading.Tasks;
using System.Data.Entity;
using System.Diagnostics;
using Savonia.Measurements.Database.Models;

namespace Savonia.Measurements.Database
{
    public class MetaDataRepository : IDisposable
    {
        public const int LatestVersion = 0x0;
        public const int AllVersions = -1; // 0xFFFFFFFF

        // meta constants

        private SavoniaMeasurementsV2Entities db;
        private bool doNotDisposeDb = false;

        public MetaDataRepository()
        {
            this.db = new SavoniaMeasurementsV2Entities();
        }

        public MetaDataRepository(SavoniaMeasurementsV2Entities db)
        {
            this.db = db;
            doNotDisposeDb = true;
        }


        /// <summary>
        /// Makes the modification to db. Remember to SaveChanges()!!!
        /// </summary>
        /// <param name="context"></param>
        /// <param name="object"></param>
        /// <param name="tag"></param>
        /// <param name="version"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        private async Task SetMetaDataAsync(string context, string @object, string tag, int version, byte[] data)
        {
            var exists = await this.db.GetMetaDataAsync(context, @object, tag, version);
            Meta mc = null;
            if (null != exists)
            {
                mc = exists.FirstOrDefault();
            }

            if (null == data)
            {
                if (null != mc)
                {
                    // delete existing data
                    this.db.Metas.Remove(mc);
                }
                return;
            }

            if (null == mc)
            {
                mc = new Meta()
                {
                    Context = context,
                    Object = @object,
                    Tag = tag,
                    Version = version,
                    Data = data
                };
                this.db.Metas.Add(mc);
            }
            else
            {
                mc.Data = data;
            }
        }
        /// <summary>
        /// Synchronous. Makes the modification to db. Remember to SaveChanges()!!!
        /// </summary>
        /// <param name="context"></param>
        /// <param name="object"></param>
        /// <param name="tag"></param>
        /// <param name="version"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        private void SetMetaData(string context, string @object, string tag, int version, byte[] data)
        {
            var exists = this.db.GetMetaData(context, @object, tag, version);
            Meta mc = null;
            if (null != exists)
            {
                mc = exists.FirstOrDefault();
            }

            if (null == data)
            {
                if (null != mc)
                {
                    // delete existing data
                    this.db.Metas.Remove(mc);
                }
                return;
            }

            if (null == mc)
            {
                mc = new Meta()
                {
                    Context = context,
                    Object = @object,
                    Tag = tag,
                    Version = version,
                    Data = data
                };
                this.db.Metas.Add(mc);
            }
            else
            {
                mc.Data = data;
            }
        }


        /// <summary>
        /// Delete meta object or data from the meta object. 
        /// Deletes the whole meta object when only metaObject is provided. 
        /// When tag or version is specified then those values from meta object are deleted.
        /// </summary>
        /// <param name="metaObject"></param>
        /// <param name="tag"></param>
        /// <param name="version"></param>
        /// <returns></returns>
        public async Task DeleteMetaObjectAsync(MetaObject metaObject, string tag = null, int version = MetaDataRepository.AllVersions)
        {
            if (null == metaObject)
            {
                return;
            }
            await this.DeleteMetaObjectAsync(metaObject.Context, metaObject.Object, tag, version);
        }
        /// <summary>
        /// Delete meta object or data from the meta object. 
        /// Deletes the whole meta object when only metaObject is provided. 
        /// When tag or version is specified then those values from meta object are deleted.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="object"></param>
        /// <param name="tag"></param>
        /// <param name="version"></param>
        /// <returns></returns>
        public async Task DeleteMetaObjectAsync(string context, string @object, string tag = null, int version = MetaDataRepository.AllVersions)
        {
            var obj = await this.db.GetMetaDataAsync(context, @object, tag, version);
            this.db.Metas.RemoveRange(obj);
            await this.db.SaveChangesAsync().ConfigureAwait(false);
        }

        /// <summary>
        /// Synchronous. Delete meta object or data from the meta object. 
        /// Deletes the whole meta object when only metaObject is provided. 
        /// When tag or version is specified then those values from meta object are deleted.
        /// </summary>
        /// <param name="metaObject"></param>
        /// <param name="tag"></param>
        /// <param name="version"></param>
        /// <returns></returns>
        public void DeleteMetaObject(MetaObject metaObject, string tag = null, int version = MetaDataRepository.AllVersions)
        {
            if (null == metaObject)
            {
                return;
            }
            this.DeleteMetaObject(metaObject.Context, metaObject.Object, tag, version);
        }
        /// <summary>
        /// Synchronous. Delete meta object or data from the meta object. 
        /// Deletes the whole meta object when only metaObject is provided. 
        /// When tag or version is specified then those values from meta object are deleted.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="object"></param>
        /// <param name="tag"></param>
        /// <param name="version"></param>
        /// <returns></returns>
        public void DeleteMetaObject(string context, string @object, string tag = null, int version = MetaDataRepository.AllVersions)
        {
            var obj = this.db.GetMetaData(context, @object, tag, version);
            this.db.Metas.RemoveRange(obj);
            this.db.SaveChanges();
        }

        /// <summary>
        /// Adds or updates meta datas. Saves all meta object properties.
        /// Deletes metaobject when it is not valid.
        /// </summary>
        /// <param name="metaObject"></param>
        /// <returns></returns>
        public async Task SaveMetaDataAsync(params MetaObject[] metaObject)
        {
            if (null == metaObject)
            {
                return;
            }
            foreach (var mo in metaObject)
            {
                if (!mo.IsValid)
                {
                    await this.DeleteMetaObjectAsync(mo);
                }
                else
                {
                    //mo.Properties.ForEach(async tag => await this.SetMetaDataAsync(mo.Context, mo.Object, tag.Tag, tag.Version, tag.Value));
                    foreach (var tag in mo.Properties)
                    {
                        await this.SetMetaDataAsync(mo.Context, mo.Object, tag.Tag, tag.Version, tag.Value);
                    }
                }
            }
            await this.db.SaveChangesAsync().ConfigureAwait(false);
        }

        /// <summary>
        /// Adds or updates meta object tags specified by <paramref name="tags"/>.
        /// Deletes the meta object when it is not valid.
        /// </summary>
        /// <param name="metaObject"></param>
        /// <param name="tags">Specify tags to update or add. Other meta object's tags are not changed or saved. Set to null or empty array to update all meta object tags.</param>
        /// <returns></returns>
        public async Task SaveMetaDataAsync(MetaObject metaObject, params string[] tags)
        {
            if (null == metaObject)
            {
                return;
            }
            if (!metaObject.IsValid)
            {
                await this.DeleteMetaObjectAsync(metaObject);
            }
            else
            {
                if (null == tags || tags.Length == 0)
                {
                    foreach (var tag in metaObject.Properties)
                    {
                        await this.SetMetaDataAsync(metaObject.Context, metaObject.Object, tag.Tag, tag.Version, tag.Value);
                    }
                }
                else
                {
                    foreach (var tag in metaObject.Properties.Where(t => tags.Contains(t.Tag)))
                    {
                        await this.SetMetaDataAsync(metaObject.Context, metaObject.Object, tag.Tag, tag.Version, tag.Value);
                    }
                }
            }
            await this.db.SaveChangesAsync().ConfigureAwait(false);
        }

        /// <summary>
        /// Adds or updates meta datas synchronously.
        /// </summary>
        /// <param name="metaObject"></param>
        /// <returns></returns>
        public void SaveMetaData(params MetaObject[] metaObject)
        {
            if (null == metaObject)
            {
                return;
            }
            foreach (var mo in metaObject)
            {
                if (!mo.IsValid)
                {
                    this.DeleteMetaObject(mo);
                }
                else
                {
                    mo.Properties.ForEach(tag => this.SetMetaData(mo.Context, mo.Object, tag.Tag, tag.Version, tag.Value));
                }
            }
            this.db.SaveChanges();
        }

        public async Task<List<MetaObject>> GetMetaObjectsAsync(MetaObject model, string tag = null, int version = MetaDataRepository.LatestVersion)
        {
            if (null == model)
            {
                throw new ArgumentNullException("model");
            }

            return await this.GetMetaObjectsAsync(model.Context, model.Object, tag: tag, version: version);
        }

        /// <summary>
        /// Get meta objects. Found meta objects with all tags are returned.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="object"></param>
        /// <param name="tag"></param>
        /// <param name="version"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public async Task<List<MetaObject>> GetMetaObjectsAsync(string context, string @object = null, string tag = null, int version = MetaDataRepository.LatestVersion, byte[] data = null)
        {
            var metaData = await this.db.GetMetaDataAsync(context, @object, tag, version, data);

            if (null == metaData)
            {
                return null;
            }

            List<MetaObject> metaObjects = new List<MetaObject>();
            MetaObject mo;
            // GetMetaDataAsync returns only partial objects when more specific data is queried (ie. with tag value).
            bool fetchWholeObject = null != tag || null != data; // when specific data is queried then re-query the actual meta object.
            foreach (var objects in metaData.GroupBy(m => m.Object))
            {
                var o = objects.First();
                if (fetchWholeObject)
                {
                    mo = await GetMetaObjectAsync(o.Context, o.Object);
                }
                else
                {
                    mo = new MetaObject(o.Context, o.Object);
                    mo.Properties = new List<MetaProperty>(objects.Count());
                    foreach (var ot in objects)
                    {
                        mo.Properties.Add(new MetaProperty(ot.Tag, ot.Data, ot.Version));
                    }
                }
                metaObjects.Add(mo);
            }

            return metaObjects;
        }

        /// <summary>
        /// Get a meta object. 
        /// </summary>
        /// <param name="context"></param>
        /// <param name="object"></param>
        /// <returns></returns>
        public async Task<MetaObject> GetMetaObjectAsync(string context, string @object)
        {
            var metaData = await this.db.GetMetaDataAsync(context, @object);

            if (null == metaData || metaData.Count() == 0)
            {
                return null;
            }
            return this.GetMetaObject(metaData);
        }

        /// <summary>
        /// Synchronous. Get a meta object. 
        /// </summary>
        /// <param name="context"></param>
        /// <param name="object"></param>
        /// <returns></returns>
        public MetaObject GetMetaObject(string context, string @object)
        {
            var metaData = this.db.GetMetaData(context, @object);

            if (null == metaData || metaData.Count() == 0)
            {
                return null;
            }
            return this.GetMetaObject(metaData);
        }

        private MetaObject GetMetaObject(List<Meta> metaData)
        {
            var o = metaData.First();
            MetaObject mo = new MetaObject(o.Context, o.Object);
            mo.Properties = new List<MetaProperty>(metaData.Count());

            metaData.ForEach(m => mo.Properties.Add(new MetaProperty(m.Tag, m.Data, m.Version)));

            return mo;
        }

        #region IDisposable Members
        private bool _disposed = false;

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }
            if (disposing)
            {
                if (!doNotDisposeDb && null != this.db)
                {
                    this.db.Dispose();
                    this.db = null;
                }
            }
            _disposed = true;
        }

        #endregion
    }
}