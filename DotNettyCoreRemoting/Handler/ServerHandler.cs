using DotNetty.Buffers;
using DotNetty.Handlers.Flow;
using DotNetty.Transport.Channels;
using DotNettyCoreRemoting.Logging;
using DotNettyCoreRemoting.RpcMessaging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace DotNettyCoreRemoting.Handler
{
    public class ServerHandler : ChannelHandlerAdapter
    {
        private readonly DotNettyRPCServer _rpcServer;

        public ServerHandler(DotNettyRPCServer rpcServer)
        {
            _rpcServer = rpcServer ?? throw new ArgumentNullException(nameof(rpcServer));
        }

        public override void ChannelRead(IChannelHandlerContext context, object message)
        {
            if (message is IByteBuffer requestBuffer)
            {

                var requestBytes = requestBuffer.ByteBufferTobyte();
                requestBuffer.Release(); // ✅ 必须释放
                _ = Task.Run(async () => await HandleRequestAsync(context, requestBytes));

                //HandleRequestAsync(context, requestBytes).Wait();

            }
            else
            {
                context.CloseAsync();
            }
        }

        private async Task HandleRequestAsync(IChannelHandlerContext context, byte[] requestBytes)
        {
            byte[] responseBytes = null;

            try
            {
                // 模拟你的业务逻辑：反序列化、调用方法、返回数据库数据
                var resultContext = _rpcServer.InvokeRemoteMethod(requestBytes);
                responseBytes = _rpcServer.Serializer.Serialize(resultContext);

            }
            catch (Exception ex)
            {
                Logger.Error(typeof(ServerHandler), ex, "处理客户端请求时发生错误");
                var resultContext = new ClientRpcContext()
                {
                    Error = true,
                    ErrorMessage = ex.Message
                };

                responseBytes = _rpcServer.Serializer.Serialize(resultContext);
            }

            if (responseBytes == null || responseBytes.Length == 0)
            {
                Logger.Error(typeof(ServerHandler), "服务器处理请求后没有生成响应数据");
                var resultContext = new ClientRpcContext()
                {
                    Error = true,
                    ErrorMessage = "No response from server"
                };

                responseBytes = _rpcServer.Serializer.Serialize(resultContext);
            }

            var responseBuffer = Unpooled.WrappedBuffer(responseBytes);

            try
            {
                await context.WriteAndFlushAsync(responseBuffer);

                await context.CloseAsync(); // 处理完请求后关闭连接
            }
            finally
            {
                responseBuffer?.Release(); // 只在这里释放
            }

        }

        public override void ExceptionCaught(IChannelHandlerContext context, Exception exception)
        {
            Logger.Error(typeof(ServerHandler), exception, $"服务器异常 - 远程地址: {context.Channel.RemoteAddress}");
            context.CloseAsync();
        }
    }
}
