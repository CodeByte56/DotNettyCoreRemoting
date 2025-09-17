using CoreRemoting;
using DotNettyCoreRemoting;
using DotNettyCoreRemoting.Serialization.Binary;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace DotNettyCoreRemoting.Client
{

    public static class RemoteActivator
    {

        public static T GetObject<T>(string url)
        {
            // Step 1: 替换 localhost 为 127.0.0.1
            var fixedUrl = FixLocalhost(url);

            // Step 2: 解析 URI
            var uri = new Uri(fixedUrl);

            // Step 3: 获取 host 和 port
            string host = uri.Host;
            int port = uri.Port; // 默认端口处理

            // Step 4: 创建或获取客户端
            string clientKey = $"{host}:{port}";

            var client = new DotNettyRPCClient(new ClientConfig
            {
                ServerHostName = host,
                ServerPort = port,
                Serializer = new BinarySerializerAdapter()
            });

            // Step 5: 创建代理
            return client.CreateProxy<T>();
        }

        // 替换 localhost 为 127.0.0.1
        private static string FixLocalhost(string originalUrl)
        {
            var uriBuilder = new UriBuilder(originalUrl);
            if (string.Equals(uriBuilder.Host, "localhost", StringComparison.OrdinalIgnoreCase))
            {
                uriBuilder.Host = "127.0.0.1";
            }
            return uriBuilder.ToString();
        }
    }
}
