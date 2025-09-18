# DotNettyCoreRemoting

## Language
[View Chinese Version](README.zh-CN.md)

## Project Introduction

DotNettyCoreRemoting is a high-performance, lightweight Remote Procedure Call (RPC) library based on the [DotNetty](https://github.com/Azure/DotNetty) networking framework, supporting both .NET Core and .NET Framework platforms. This project is a modified and simplified implementation based on [CoreRemoting](https://github.com/theRainbird/CoreRemoting.git).

## Features

- 🔄 **Cross-platform compatibility** - Supports both .NET Core and .NET Framework
- ⚡ **High performance** - Efficient network communication based on DotNetty
- 🛠️ **Simple and easy to use** - Provides a clean API that's easy to integrate and use
- 🧩 **Dependency injection support** - Built-in adapters for multiple DI containers
- 🔍 **Multiple serialization methods** - Supports binary and Bson serialization
- 🔄 **Generic support** - Full support for generic method calls
- ⏱️ **Timeout control** - Provides flexible timeout settings
- 📦 **Complex object transmission** - Supports serialization and transmission of complex objects like DataTable

## Quick Start

### Installation

Install DotNettyCoreRemoting using NuGet Package Manager:

```powershell
Install-Package DotNettyCoreRemoting
```

Or using .NET CLI:

```powershell
dotnet add package DotNettyCoreRemoting
```

### Server Example

1. Define service interface:

```csharp
public interface IMyFirstServer
{
    void SayHello(string msg);
    T SayHelloT<T>(T msg);
}
```

2. Implement service interface:

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

3. Start RPC server:

```csharp
// Create server
var server = new DotNettyRPCServer(new ServerConfig
{
    HostName = "127.0.0.1",  // In production, bind to 0.0.0.0 only if you intend to accept connections from multiple interfaces, and always restrict access via firewall, network policy, or application-level controls.
    NetworkPort = 9095,
    RreistContainer = container =>
    {
        // Register service with SingleCall lifetime
        container.RegisterService<IMyFirstServer, MyFirstServer>(ServiceLifetime.SingleCall);
    },
});

// Start server
server.Start();

Console.WriteLine("Server started, press any key to stop...");
Console.ReadKey();

// Stop server
server.Stop();
```

### Client Example

```csharp
// Create client and connect to server
var client = new DotNettyRPCClient(new ClientConfig
{
    ServerHostName = "127.0.0.1",
    ServerPort = 9095,
    timeout = 120 // Timeout setting (seconds)
});

// Create remote service proxy
var service = client.CreateProxy<IMyFirstServer>();

// Call remote method
string message = "Hello from client!";
service.SayHello(message);

// Call generic method
var result = service.SayHelloT("Generic method test");
Console.WriteLine($"Call result: {result}");
```

## Configuration Options

### Server Configuration

```csharp
var serverConfig = new ServerConfig
{
    // Server hostname or IP address
    HostName = "127.0.0.1",
    
    // Server listening port
    NetworkPort = 9095,
    
    // Dependency injection container registration callback
    RreistContainer = container => { /* Register services */ },
    
    // Custom dependency injection container (optional)
    DependencyInjectionContainer = new CastleWindsorDependencyInjectionContainer(),
    
    // Custom serializer (optional)
    Serializer = new BinarySerializerAdapter(),
    
    // Custom logger factory (optional)
    LoggerFactory = new Microsoft.Extensions.Logging.LoggerFactory()
};
```

### Client Configuration

```csharp
var clientConfig = new ClientConfig
{
    // Server hostname or IP address
    ServerHostName = "127.0.0.1",
    
    // Server port
    ServerPort = 9095,
    
    // Call timeout in seconds
    timeout = 120,
    
    // Custom serializer (optional)
    Serializer = new BinarySerializerAdapter()
};
```

## Advanced Features

### Custom Serializer

You can implement the `ISerializerAdapter` interface to create custom serializers:

```csharp
public class CustomSerializer : ISerializerAdapter
{
    // Implement serialization and deserialization methods
}
```

### Using Different DI Containers

DotNettyCoreRemoting supports multiple dependency injection containers:

```csharp
// Castle Windsor (default)
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

## Project Structure

```
DotNettyCoreRemoting/
├── DependencyInjection/  # Dependency injection implementations
├── Handler/              # Network handlers
├── RemoteDelegates/      # Remote delegate functionality
├── RpcMessaging/         # RPC messaging related
├── Serialization/        # Serialization implementations
├── DotNettyRPCServer.cs  # RPC server implementation
└── DotNettyRPCClient.cs  # RPC client implementation
```

## FAQ

### Connection Issues

- Make sure the server is running and listening on the correct port
- Check if firewall settings are blocking the connection
- Verify that the client is configured with the correct server address and port

### Serialization Issues

- Ensure that the objects being transferred are serializable
- For complex objects, custom serialization logic may be required
- For generic types, ensure all generic parameters are serializable

## License

This project is licensed under the MIT License.

## Contributing

Contributions are welcome! Please submit issues and pull requests to help improve this project.