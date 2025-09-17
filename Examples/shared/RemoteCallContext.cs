using System;
using System.Collections.Generic;
using System.Text;

namespace shared
{
    [Serializable]
    public class RemoteCallContext
    {
        public string AssemblyName { get; set; }
        public string TypeName { get; set; }
        public string MethodName { get; set; }
        public object[] Args { get; set; }

    }
}
