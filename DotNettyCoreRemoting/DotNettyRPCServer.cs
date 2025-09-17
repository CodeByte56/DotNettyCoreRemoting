using Castle.Facilities.TypedFactory.Internal;
using CoreRemoting.RemoteDelegates;
using DotNetty.Buffers;
using DotNetty.Codecs;
using DotNetty.Transport.Bootstrapping;
using DotNetty.Transport.Channels;
using DotNetty.Transport.Channels.Sockets;
using DotNettyCoreRemoting.DependencyInjection;
using DotNettyCoreRemoting.Handler;
using DotNettyCoreRemoting.Logging;
using DotNettyCoreRemoting.RemoteDelegates;
using DotNettyCoreRemoting.RpcMessaging;
using DotNettyCoreRemoting.Serialization;
using DotNettyCoreRemoting.Serialization.Binary;
using ServiceLifetime = DotNettyCoreRemoting.DependencyInjection.ServiceLifetime;
using Serialize.Linq.Nodes;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Reflection;
using DotNetty.Handlers.Timeout;

namespace DotNettyCoreRemoting
{
    public class DotNettyRPCServer : IDotNettyCoreServer
    {
        #region 私有成员

        private int _port { get; set; }

        private IPAddress _hostName { get; set; }
        ServerBootstrap _serverBootstrap { get; }
        IChannel _serverChannel { get; set; }

        /// <summary>
        /// Gets the configured serializer.
        /// </summary>
        internal ISerializerAdapter Serializer { get; }

        private readonly IDependencyInjectionContainer _container;

        public IDependencyInjectionContainer ServiceRegistry => _container;


        private readonly RemoteDelegateInvocationEventAggregator _remoteDelegateInvocationEventAggregator;


        /// <summary>
        /// Gets the remote delegate invocation event aggregator.
        /// </summary>
        [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
        internal RemoteDelegateInvocationEventAggregator RemoteDelegateInvocation =>
            _remoteDelegateInvocationEventAggregator;

        #endregion

        #region 构造函数

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="_serverConfig">服务端配置信息</param>
        public DotNettyRPCServer(ServerConfig _serverConfig)
        {
            _port = _serverConfig.NetworkPort;
            _hostName = IPAddress.Parse(_serverConfig.HostName);
            _container = _serverConfig.DependencyInjectionContainer ?? new CastleWindsorDependencyInjectionContainer();

            _container.RegisterService<IDelegateProxyFactory, RemoteDelegates.DelegateProxyFactory>(
               lifetime: ServiceLifetime.Singleton,
               asHiddenSystemService: true);

            _serverConfig.RreistContainer?.Invoke(_container);

            Serializer = _serverConfig.Serializer ?? new BinarySerializerAdapter();

            _serverBootstrap = new ServerBootstrap()
                .Group(new MultithreadEventLoopGroup(), new MultithreadEventLoopGroup())
                .Channel<TcpServerSocketChannel>()
                .Option(ChannelOption.SoBacklog, 100)
                .ChildHandler(new ActionChannelInitializer<IChannel>(channel =>
                {
                    IChannelPipeline pipeline = channel.Pipeline;

                    //pipeline.AddLast("timeout", new IdleStateHandler(0, 0, 60));
                    // 添加帧处理（保持原样）
                    pipeline.AddLast("framing-enc", new LengthFieldPrepender(8));
                    pipeline.AddLast("framing-dec", new LengthFieldBasedFrameDecoder(
                        maxFrameLength: int.MaxValue,    // 支持大消息
                        lengthFieldOffset: 0,
                        lengthFieldLength: 8,
                        lengthAdjustment: 0,
                        initialBytesToStrip: 8));

                    // ✅ 使用新的 ServerHandler
                    pipeline.AddLast(new ServerHandler(this));
                }));
        }

        #endregion


        public ClientRpcContext InvokeRemoteMethod(byte[] request)
        {
            var me = new RemoteMethod(_container, Serializer);

            var callMessage = Serializer.Deserialize<MethodCallMessage>(request);

            return me.InvokeRemoteMethod(callMessage);

        }

        #region 外部接口

        /// <summary>
        /// 开始运行服务
        /// </summary>
        public void Start()
        {
            try
            {
                _serverChannel = _serverBootstrap.BindAsync(_hostName, _port).Result;

                Logger.Info(typeof(DotNettyRPCServer), $"RPC服务器启动成功 - 主机: {_hostName}, 端口: {_port}");

            }
            catch (Exception ex)
            {
                Logger.Error(typeof(DotNettyRPCServer), $"RPC服务器启动失败 - 主机: {_hostName}, 端口: {_port}", ex);
                throw;
            }
        }

        /// <summary>
        /// 停止服务
        /// </summary>
        public void Stop()
        {
            try
            {
                _serverChannel.CloseAsync();
            }
            catch (Exception ex)
            {
                Logger.Error(typeof(DotNettyRPCServer), "RPC服务器停止失败", ex);
                throw;
            }
        }

        #endregion
    }
}
