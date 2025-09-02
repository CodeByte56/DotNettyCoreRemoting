using DotNetty.Buffers;
using DotNetty.Handlers.Flow;
using DotNetty.Transport.Channels;
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
                var resultContext = new ClientRpcContext();
                resultContext.Error = true;
                resultContext.ErrorMessage = ex.Message;

                responseBytes = _rpcServer.Serializer.Serialize(resultContext);
            }

            if (responseBytes == null || responseBytes.Length == 0)
            {
                return;
            }

            var responseBuffer = Unpooled.WrappedBuffer(responseBytes);

            try
            {
                await context.WriteAndFlushAsync(responseBuffer);
            }
            catch (Exception ex)
            {
                responseBuffer?.Release(); // 只在这里释放
            }
        }

        public override void ExceptionCaught(IChannelHandlerContext context, Exception exception)
        {
            context.CloseAsync();
        }


        [Serializable] // 如果你用 BinaryFormatter，需要这个
        public class RpcErrorResponse
        {
            public bool Error { get; set; }
            public string Message { get; set; }

            public RpcErrorResponse(string message)
            {
                Error = true;
                Message = message;
            }
        }
    }
}
