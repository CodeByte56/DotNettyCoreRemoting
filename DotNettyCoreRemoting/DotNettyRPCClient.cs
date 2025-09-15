using DotNettyCoreRemoting.Logging;
using DotNettyCoreRemoting.RemoteDelegates;
using DotNettyCoreRemoting.RpcMessaging;
using DotNettyCoreRemoting.Serialization;
using DotNettyCoreRemoting.Serialization.Binary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace DotNettyCoreRemoting
{
    public class DotNettyRPCClient
    {
        private readonly ClientDelegateRegistry _delegateRegistry;
        private ClientConfig _config { get; set; }

        /// <summary>
        /// Gets the configured serializer.
        /// </summary>
        internal ISerializerAdapter Serializer { get; }

        public DotNettyRPCClient()
        {
            _delegateRegistry = new ClientDelegateRegistry();

            MethodCallMessageBuilder = new MethodCallMessageBuilder();

        }

        public DotNettyRPCClient(ClientConfig config) : this()
        {
            _config = config;

            ProxyBuilder = new RemotingProxyBuilder();

            Serializer = config.Serializer ?? new BinarySerializerAdapter();
        }


        public IPEndPoint ipEndPoint()
        {
            return ToIPEndPoint($"{_config.ServerHostName}:{_config.ServerPort}");
        }

        /// <summary>
        /// 转为网络终结点IPEndPoint
        /// </summary>=
        /// <param name="str">字符串</param>
        /// <returns></returns>
        public IPEndPoint ToIPEndPoint(string str)
        {
            IPEndPoint iPEndPoint = null;
            try
            {
                string[] strArray = str.Split(':').ToArray();
                string addr = strArray[0];
                int port = Convert.ToInt32(strArray[1]);
                iPEndPoint = new IPEndPoint(IPAddress.Parse(addr), port);
            }
            catch
            {
                iPEndPoint = null;
            }

            return iPEndPoint;
        }

        /// <summary>
        /// Gets the local client delegate registry.
        /// </summary>
        internal ClientDelegateRegistry ClientDelegateRegistry => _delegateRegistry;

        /// <summary>
        /// Gets a utility object for building remoting messages.
        /// </summary>
        internal IMethodCallMessageBuilder MethodCallMessageBuilder { get; set; }

        internal RemotingProxyBuilder ProxyBuilder { get; set; }

        public T CreateProxy<T>(string serviceName = "")
        {
            try
            {
                return ProxyBuilder.CreateProxy<T>(this, serviceName, _config.timeout);
            }
            catch (Exception ex)
            {
                Logger.Error(typeof(DotNettyRPCClient), $"创建代理失败: {typeof(T).Name}, 服务名: {serviceName}", ex);
                throw;
            }
        }

    }
}
