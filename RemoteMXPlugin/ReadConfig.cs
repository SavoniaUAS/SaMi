﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace RemoteMXPlugin
{
    [DataContract]
    public class ReadConfig
    {
        [DataMember]
        public DateTime LatestValueRead { get; set; }
    }
}
