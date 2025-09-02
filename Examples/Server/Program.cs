// See https://aka.ms/new-console-template for more information
using CoreServer;
using DotNettyCoreRemoting;
using DotNettyCoreRemoting.DependencyInjection;
using shared;
using System.ComponentModel;

var server = new DotNettyRPCServer(new ServerConfig
{
    HostName = "127.0.0.1",
    NetworkPort = 9095,
    RreistContainer = container =>
    {
        container.RegisterService<IMyFirstServer, MyFirstServer>(ServiceLifetime.SingleCall);
    }
});

server.Start();

Console.WriteLine("Server IS Run");
Console.ReadLine();

