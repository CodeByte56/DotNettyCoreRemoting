# DotNettyCoreRemoting

## 语言
[查看英文版本](README.md)

## 项目介绍

DotNettyCoreRemoting 是一个基于 [DotNetty](https://github.com/Azure/DotNetty) 网络框架实现的高性能、轻量级远程过程调用 (RPC) 库，支持 .NET Core 和 .NET Framework 平台。该项目是在 [CoreRemoting](https://github.com/theRainbird/CoreRemoting.git) 基础修改实现简单版本。

## 特性

- 🔄 **跨平台兼容** - 同时支持 .NET Core 和 .NET Framework
- ⚡ **高性能** - 基于 DotNetty 实现的高效网络通信
- 🛠️ **简单易用** - 提供简洁的 API，易于集成和使用
- 🧩 **依赖注入支持** - 内置多种依赖注入容器适配器
- 🔍 **多种序列化方式** - 支持二进制和 Bson 序列化
- 🔄 **泛型支持** - 完整支持泛型方法调用
- ⏱️ **超时控制** - 提供灵活的超时设置
- 📦 **复杂对象传输** - 支持 DataTable 等复杂对象的序列化和传输

## 快速开始

### 安装

使用 NuGet 包管理器安装 DotNettyCoreRemoting：

```powershell
Install-Package DotNettyCoreRemoting
```

或者使用 .NET CLI：

```powershell
dotnet add package DotNettyCoreRemoting
```

### 服务端示例

1. 定义服务接口:

```csharp
public interface IMyFirstServer
{
    void SayHello(string msg);
    T SayHelloT<T>(T msg);
}
```

2. 实现服务接口:

```csharp
public class MyFirstServer : IMyFirstServer
{
    public void SayHello(string msg)
    {
        Console.WriteLine(msg);
    }
    
    public T SayHelloT<T>(T msg)
    {
        return msg;
    }
}
```

3. 启动 RPC 服务器:

```csharp
// 创建服务器
var server = new DotNettyRPCServer(new ServerConfig
{
    HostName = "127.0.0.1",  // 在生产环境中，仅当您打算接受来自多个网络接口的连接时才绑定到0.0.0.0，并始终通过防火墙、网络策略或应用级控制来限制访问。
    NetworkPort = 9095,
    RreistContainer = container =>
    {
        // 注册服务，设置生命周期为每次调用创建新实例
        container.RegisterService<IMyFirstServer, MyFirstServer>(ServiceLifetime.SingleCall);
    },
});

// 启动服务器
server.Start();

Console.WriteLine("服务器已启动，按任意键停止...");
Console.ReadKey();

// 停止服务器
server.Stop();
```

### 客户端示例

```csharp
// 创建客户端连接到服务器
var client = new DotNettyRPCClient(new ClientConfig
{
    ServerHostName = "127.0.0.1",
    ServerPort = 9095,
    timeout = 120 // 超时设置（秒）
});

// 创建远程服务代理
var service = client.CreateProxy<IMyFirstServer>();

// 调用远程方法
string message = "Hello from client!";
service.SayHello(message);

// 调用泛型方法
var result = service.SayHelloT("Generic method test");
Console.WriteLine($"调用结果: {result}");
```

## 配置选项

### 服务端配置

```csharp
var serverConfig = new ServerConfig
{
    // 服务器主机名或IP地址
    HostName = "127.0.0.1",
    
    // 服务器监听端口
    NetworkPort = 9095,
    
    // 依赖注入容器注册回调
    RreistContainer = container => { /* 注册服务 */ },
    
    // 自定义依赖注入容器（可选）
    DependencyInjectionContainer = new CastleWindsorDependencyInjectionContainer(),
    
    // 自定义序列化器（可选）
    Serializer = new BinarySerializerAdapter(),
    
    // 自定义日志工厂（可选）
    LoggerFactory = new Microsoft.Extensions.Logging.LoggerFactory()
};
```

### 客户端配置

```csharp
var clientConfig = new ClientConfig
{
    // 服务器主机名或IP地址
    ServerHostName = "127.0.0.1",
    
    // 服务器端口
    ServerPort = 9095,
    
    // 调用超时时间（秒）
    timeout = 120,
    
    // 自定义序列化器（可选）
    Serializer = new BinarySerializerAdapter()
};
```

## 高级特性

### 自定义序列化器

您可以实现 `ISerializerAdapter` 接口来创建自定义序列化器：

```csharp
public class CustomSerializer : ISerializerAdapter
{
    // 实现序列化和反序列化方法
}
```

### 使用不同的依赖注入容器

DotNettyCoreRemoting 支持多种依赖注入容器：

```csharp
// Castle Windsor (默认)
var server = new DotNettyRPCServer(new ServerConfig
{
    DependencyInjectionContainer = new CastleWindsorDependencyInjectionContainer()
});

// Microsoft.Extensions.DependencyInjection
var server = new DotNettyRPCServer(new ServerConfig
{
    DependencyInjectionContainer = new MicrosoftDependencyInjectionContainer()
});
```

## 项目结构

```
DotNettyCoreRemoting/
├── DependencyInjection/  # 依赖注入相关实现
├── Handler/              # 网络处理器
├── RemoteDelegates/      # 远程委托功能
├── RpcMessaging/         # RPC消息相关
├── Serialization/        # 序列化实现
├── DotNettyRPCServer.cs  # RPC服务器实现
└── DotNettyRPCClient.cs  # RPC客户端实现
```

## 常见问题

### 连接问题

- 确保服务器已启动并监听正确的端口
- 检查防火墙设置是否阻止了连接
- 验证客户端配置的服务器地址和端口是否正确

### 序列化问题

- 确保传输的对象是可序列化的
- 对于复杂对象，可能需要自定义序列化逻辑
- 对于泛型类型，确保所有泛型参数都是可序列化的

## 开源协议

该项目使用 MIT 许可证。

## 贡献

欢迎提交问题和拉取请求来帮助改进这个项目。
