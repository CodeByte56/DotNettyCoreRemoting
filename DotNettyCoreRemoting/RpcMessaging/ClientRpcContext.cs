using System;
using System.Collections.Generic;
using System.Text;

namespace DotNettyCoreRemoting.RpcMessaging
{
    [Serializable]
    public class ClientRpcContext
    {
        /// <summary>
        /// Gets or sets the result message, that was received from server after the call was invoked on server side.
        /// </summary>
        public MethodCallResultMessage ResultMessage { get; set; }

        /// <summary>
        /// Gets or sets whether this RPC call is in error state.
        /// </summary>
        public bool Error { get; set; }

        /// <summary>
        /// Gets or sets an exception that describes an error that occurred on server side RPC invocation.
        /// </summary>
        public string ErrorMessage { get; set; }

    }
}
