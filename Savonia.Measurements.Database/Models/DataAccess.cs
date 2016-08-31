using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Savonia.Measurements.Models;

namespace Savonia.Measurements.Database.Models
{
    public class DataAccess
    {
        private AccessKeyModel key = null;

        public DataAccess(AccessKeyModel key)
        {
            this.key = key;
        }

        public int ProviderID
        {
            get
            {
                return null != key ? key.ProviderID : default(int);
            }
        }

        public short? KeyId
        {
            get 
            {
                return null != key ? key.KeyId : default(short?);
            }
        }

        /// <summary>
        /// Check if access control in key matches required access level.
        /// Returns true when this IsValid and AccessControl contains same bits as in requiredAccessLevel.
        /// When isMinLevel is set then return true then AccessControl is greater or equal than requiredAccessLevel.
        /// </summary>
        /// <param name="requiredAccessLevel"></param>
        /// <param name="isMinLevel"></param>
        /// <returns></returns>
        public bool AccessAllowed(AccessControl requiredAccessLevel, bool isMinLevel = false)
        {
            if (null == key || !key.IsValid || requiredAccessLevel == AccessControl.NoAccess)
            {
                return false;
            }
            int req = (int)requiredAccessLevel;
            if (isMinLevel)
            {
                return (int)key.AccessControl >= req;
            }
            return ((int)key.AccessControl & req) == req;
        }
    }
}
