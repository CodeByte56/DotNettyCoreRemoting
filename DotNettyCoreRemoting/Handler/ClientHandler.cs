using DotNetty.Buffers;
using DotNetty.Transport.Channels;
using DotNettyCoreRemoting;
using DotNettyCoreRemoting.Handler;
using DotNettyCoreRemoting.Logging;
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
                Logger.Error(typeof(ClientHandler), $"收到未知类型的消息: {message?.GetType().Name}");
            }
        }


        public override void ChannelInactive(IChannelHandlerContext context)
        {
            Logger.Error(typeof(ClientHandler), $"客户端连接断开: {context.Channel.RemoteAddress}");
            base.ChannelInactive(context);
        }

        public override void ExceptionCaught(IChannelHandlerContext context, Exception exception)
        {
            Logger.Error(typeof(ClientHandler), $"客户端异常 - 远程地址: {context.Channel.RemoteAddress}", exception);
            context.CloseAsync();
        }
    }
}