using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Savonia.Measurements.Manager.Models
{
   
        [Flags]
        public enum AccessControls
        {
         None = 0x00,
         Read = 0x01,
        /// <summary>
        /// Access to Admin controller + all other access
        /// </summary>
        Admin = 0xFF
        }
    
}