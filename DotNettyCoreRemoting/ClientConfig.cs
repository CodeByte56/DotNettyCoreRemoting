using DotNettyCoreRemoting.Serialization;
using System;
using System.Collections.Generic;
using System.Text;

namespace DotNettyCoreRemoting
{
    public class ClientConfig
    {
        /// <summary>
        /// Gets or sets the host name of the DotNettyCoreRemoting server to be connected to.
        /// </summary>
        public string ServerHostName { get; set; } = "127.0.0.1";

        /// <summary>
        /// Gets or sets the network port of the DotNettyCoreRemoting server to be connected to.
        /// </summary>
        public int ServerPort { get; set; } = 9090;

        public int timeout { get; set; } = 60;

        public ISerializerAdapter Serializer { get; set; }
    }
}
