using Savonia.Measurements.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Savonia.Measurements.Manager.Helpers
{
    /// <summary>
    /// This class is used to store accesskeys temporary to session and get them if requested.
    /// </summary>
    public class ManageHelper
    {

        private readonly string accessKey = "accessKey";
        private readonly string providerKey = "providerKey";
        /// <summary>
        /// Adds access keys to session and generates keyIds if necessary.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="keys"></param>
        public List<AccessKeyModel> AddAccessKeysToSessionAndUpdate(HttpContextBase context, List<AccessKeyModel> keys)
        {
            keys = GenerateKeyIdsIfNotExist(keys);
            foreach (var k in keys)
            {
                var name = accessKey + k.KeyId.ToString();
                context.Session.Add(name, k.CombinedKey);
            }
            return keys;
        }

        /// <summary>
        /// Adds provider models key to session
        /// </summary>
        /// <param name="context"></param>
        /// <param name="provider"></param>
        public void AddProviderKeyToSession(HttpContextBase context, ProviderModel provider)
        {
            string name = providerKey;

            context.Session.Add(name, provider.Key);
        }



        /// <summary>
        /// Iterates through list of AccessKeyModel and generates KeyId:s for keys that has no KeyId.
        /// </summary>
        /// <param name="keys"></param>
        /// <returns></returns>
        public List<AccessKeyModel> GenerateKeyIdsIfNotExist(List<AccessKeyModel> keys)
        {
            short i = 1;
            foreach (var k in keys)
            {
                if (!k.KeyId.HasValue)
                {
                    var max = keys.Max(x => x.KeyId);
                    if (max != null)
                    {
                        max++;
                        k.KeyId = max;
                    }
                    else
                    {
                        k.KeyId = i;
                    }
                }
                i++;
            }

            return keys;
        }

        /// <summary>
        /// Tries to get AccessKey from session using keyId.
        /// </summary>
        /// <param name="context">specifies the context</param>
        /// <param name="keyId">id that determines key</param>
        /// <param name="combinedKey">Determines if the key returned is combined key or key</param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException">The exception that is thrown when a requested method or operation is not implemented.</exception>
        public string GetAccessKeyFromSession(HttpContextBase context, short keyId, bool combinedKey = true)
        {
            string name = accessKey + keyId;
            string key;
            try
            {
                key = context.Session[name].ToString();
                if (key != null && combinedKey == false)
                {
                    var k = key.Split('-');
                    key = "";
                    for (int i = 1; i < k.Length; i++)
                    {
                        key += k[i];
                        if (k.Length > 2 && i != k.Length - 1)
                        {
                            key += "-";
                        }
                    }
                }
            }
            catch (NotImplementedException)
            {
                throw;
            }
            return key;
        }

        /// <summary>
        /// Tries to get ProviderKey from session.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="keyId"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException">The exception that is thrown when a requested method or operation is not implemented.</exception>
        public string GetProviderKeyFromSession(HttpContextBase context)
        {
            string name = providerKey;
            string key;
            try
            {
                key = context.Session[name].ToString();
            }
            catch (NotImplementedException)
            {
                throw;
            }

            return key;
        }

    }
}