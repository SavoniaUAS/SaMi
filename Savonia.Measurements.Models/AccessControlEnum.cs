using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Savonia.Measurements.Models
{
    public enum AccessControl
    {
        NoAccess = 0x00,
        Read = 0x01,
        Write = 0x02,
        ReadWrite = Read | Write,
        Delete = 0x10,
        Modify = 0x20 | Delete | ReadWrite,
        Full = 0xFF
    }
}
