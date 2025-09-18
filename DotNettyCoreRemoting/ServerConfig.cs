using DotNettyCoreRemoting.DependencyInjection;
using DotNettyCoreRemoting.Serialization;
using System;
using System.Collections.Generic;
using System.Text;

namespace DotNettyCoreRemoting
{
    public class ServerConfig
    {
        public string HostName { get; set; } = "127.0.0.1";

        public int NetworkPort { get; set; } = 9090;

        public Action<IDependencyInjectionContainer> RreistContainer { get; set; }

        public virtual IDependencyInjectionContainer DependencyInjectionContainer { get; set; }

        public ISerializerAdapter Serializer { get; set; }

        public Microsoft.Extensions.Logging.ILoggerFactory LoggerFactory { get; set; }

    }
}
