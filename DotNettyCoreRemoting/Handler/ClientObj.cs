using DotNetty.Buffers;
using DotNettyCoreRemoting.RpcMessaging;
using System.Threading;

namespace DotNettyCoreRemoting.Handler
{
    public class ClientObj
    {
        public AutoResetEvent WaitHandler { get; set; } = new AutoResetEvent(false);
        public byte[] ReturnResponse { get; set; }
    }
}
