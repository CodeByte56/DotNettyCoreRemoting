// See https://aka.ms/new-console-template for more information
using CoreRemoting.Serialization.Bson;
using CoreServer;
using DotNettyCoreRemoting;
using DotNettyCoreRemoting.DependencyInjection;
using shared;
using System.ComponentModel;
using System.Threading;

// 创建服务器
var server = new DotNettyRPCServer(new ServerConfig
{
    HostName = "127.0.0.1",
    NetworkPort = 9095,
    RreistContainer = container =>
    {
        container.RegisterService<IMyFirstServer, MyFirstServer>(ServiceLifetime.SingleCall);
    },
});

// 创建一个事件用于保持服务器运行
var keepRunningEvent = new ManualResetEventSlim(false);

// 注册进程退出事件处理程序
AppDomain.CurrentDomain.ProcessExit += (sender, e) =>
{
    Console.WriteLine("正在停止服务器...");
    server.Stop();
    Console.WriteLine("服务器已停止");
};

// 启动服务器
server.Start();

Console.WriteLine("Server IS Run");
Console.WriteLine("服务器已启动，按任意键停止...");

// 等待用户按键或进程退出
Thread thread = new Thread(() =>
{
    Console.ReadKey();
    keepRunningEvent.Set();
});
thread.IsBackground = true;
thread.Start();

// 等待停止信号
keepRunningEvent.Wait();

// 停止服务器
server.Stop();
Console.WriteLine("服务器已停止");

