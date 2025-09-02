using Castle.MicroKernel.Lifestyle.Pool;
using Coldairarrow.DotNettyCoreRemoting;
using DotNetty.Codecs;
using DotNetty.Transport.Bootstrapping;
using DotNetty.Transport.Channels;
using DotNetty.Transport.Channels.Sockets;
using DotNettyCoreRemoting.Handler;
using DotNettyCoreRemoting.Toolbox;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace DotNettyCoreRemoting
{
    public static class DotNettyClientManager
    {
        static DotNettyClientManager()
        {
            _bootstrap = new Bootstrap()
                .Group(new MultithreadEventLoopGroup())
                .Channel<TcpSocketChannel>()
                .Option(ChannelOption.TcpNodelay, true)
                .Handler(new ActionChannelInitializer<ISocketChannel>(channel =>
                {
                    IChannelPipeline pipeline = channel.Pipeline;

                    // ✅ 添加帧编解码器（必须与服务端一致）
                    pipeline.AddLast("framing-enc", new LengthFieldPrepender(8));
                    pipeline.AddLast("framing-dec", new LengthFieldBasedFrameDecoder(
                        maxFrameLength: int.MaxValue,
                        lengthFieldOffset: 0,
                        lengthFieldLength: 8,
                        lengthAdjustment: 0,
                        initialBytesToStrip: 8));

                    // ✅ 使用优化后的 ClientHandler
                    pipeline.AddLast(new ClientHandler(_clientWait));
                }));
        }

        public static Bootstrap _bootstrap { get; }

        public static ClientWait _clientWait { get; } = new ClientWait();
    }
}
