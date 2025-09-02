using DotNetty.Buffers;
using DotNetty.Transport.Channels;
using DotNettyCoreRemoting;
using DotNettyCoreRemoting.Handler;
using System;
using System.Text;
using System.Threading.Tasks;

namespace Coldairarrow.DotNettyCoreRemoting
{
    public class ClientHandler : ChannelHandlerAdapter
    {
        private readonly ClientWait _clientWait;

        public ClientHandler(ClientWait clientWait)
        {
            _clientWait = clientWait ?? throw new ArgumentNullException(nameof(clientWait));
        }

        public override void ChannelRead(IChannelHandlerContext context, object message)
        {
            if (message is IByteBuffer res)
            {
                var responseBytes = res.ByteBufferTobyte();
                res.Release(); // ✅ 释放 buffer

                // ✅ 异步处理，避免阻塞 EventLoop
                _ = Task.Run(() => { _clientWait.Set(context.Channel.Id.AsShortText(), responseBytes); });
            }
            else
            {
                Console.WriteLine($"[Client] 收到未知类型: {message?.GetType().Name}");
            }
        }


        public override void ChannelInactive(IChannelHandlerContext context)
        {
            Console.WriteLine($"[Client] 连接断开: {context.Channel.RemoteAddress}");
            base.ChannelInactive(context);
        }

        public override void ExceptionCaught(IChannelHandlerContext context, Exception exception)
        {
            Console.WriteLine($"[Client] 异常: {exception}");
            context.CloseAsync();
        }
    }
}