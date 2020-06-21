﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Anniversary.Options
{
    public class ServiceDiscoveryOptions
    {
        public string OuterClientServiceName { get; set; }
        public string ServiceName { get; set; }
        public ConsulOptions Consul { get; set; }
    }
}