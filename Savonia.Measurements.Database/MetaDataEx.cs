using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity;

namespace Savonia.Measurements.Database
{
    public partial class SavoniaMeasurementsV2Entities
    {

        /// <summary>
        /// Get meta data. If tag is used to search meta objects, only those data rows (tags) that are present in found objects.
        /// To get the whole object use only context and object parameters.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="object"></param>
        /// <param name="tag"></param>
        /// <param name="version"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public async Task<List<Meta>> GetMetaDataAsync(string context, string @object = null, string tag = null, int version = MetaDataRepository.LatestVersion, byte[] data = null)
        {
            if (string.IsNullOrEmpty(context))
            {
                throw new ArgumentOutOfRangeException("context", "Parameter context cannot be null or empty.");
            }
            IQueryable<Meta> metaData;
            bool hasObject = !string.IsNullOrEmpty(@object);
            bool hasTag = !string.IsNullOrEmpty(tag);

            if (hasObject && hasTag)
            {
                // the most specific query
                metaData = this.Metas.Where(m => m.Context == context && m.Object == @object && m.Tag == tag);
            }
            else if (hasObject)
            {
                metaData = this.Metas.Where(m => m.Context == context && m.Object == @object);
            }
            else if (hasTag)
            {
                metaData = this.Metas.Where(m => m.Context == context && m.Tag == tag);
            }
            else
            {
                metaData = this.Metas.Where(m => m.Context == context);
            }

            if (null != data)
            {
                metaData = metaData.Where(m => m.Data == data);
            }

            if (version > MetaDataRepository.LatestVersion)
            {
                var specificVersion = await metaData.Where(m => m.Version == version).ToListAsync().ConfigureAwait(false);
                return specificVersion;
            }
            else if (version == MetaDataRepository.LatestVersion)
            {
                List<Meta> latestData = new List<Meta>();
                var objectGrouped = await metaData.GroupBy(m => m.Object).ToListAsync().ConfigureAwait(false);
                foreach (var obj in objectGrouped)
                {
                    foreach (var tagVersions in obj.GroupBy(m => m.Tag))
                    {
                        var max = tagVersions.Max(m => m.Version);
                        latestData.AddRange(tagVersions.Where(m => m.Version == max));
                    }
                }

                return latestData;
            }
            else
            {
                // get all versions
                return await metaData.ToListAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Synchronous. Get meta data. If tag is used to search meta objects, only those data rows (tags) that are present in found objects.
        /// To get the whole object use only context and object parameters.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="object"></param>
        /// <param name="tag"></param>
        /// <param name="version"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public List<Meta> GetMetaData(string context, string @object = null, string tag = null, int version = MetaDataRepository.LatestVersion, byte[] data = null)
        {
            if (string.IsNullOrEmpty(context))
            {
                throw new ArgumentOutOfRangeException("context", "Parameter context cannot be null or empty.");
            }
            IQueryable<Meta> metaData;
            bool hasObject = !string.IsNullOrEmpty(@object);
            bool hasTag = !string.IsNullOrEmpty(tag);

            if (hasObject && hasTag)
            {
                // the most specific query
                metaData = this.Metas.Where(m => m.Context == context && m.Object == @object && m.Tag == tag);
            }
            else if (hasObject)
            {
                metaData = this.Metas.Where(m => m.Context == context && m.Object == @object);
            }
            else if (hasTag)
            {
                metaData = this.Metas.Where(m => m.Context == context && m.Tag == tag);
            }
            else
            {
                metaData = this.Metas.Where(m => m.Context == context);
            }

            if (null != data)
            {
                metaData = metaData.Where(m => m.Data == data);
            }

            if (version > MetaDataRepository.LatestVersion)
            {
                var specificVersion = metaData.Where(m => m.Version == version).ToList();
                return specificVersion;
            }
            else if (version == MetaDataRepository.LatestVersion)
            {
                List<Meta> latestData = new List<Meta>();
                foreach (var obj in metaData.GroupBy(m => m.Object))
                {
                    foreach (var tagVersions in obj.GroupBy(m => m.Tag))
                    {
                        var max = tagVersions.Max(m => m.Version);
                        latestData.AddRange(tagVersions.Where(m => m.Version == max));
                    }
                }

                return latestData;
            }
            else
            {
                // get all versions
                return metaData.ToList();
            }
        }
    }
}
