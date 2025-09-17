using DotNettyCoreRemoting;
using shared;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FrameworkClient
{
    internal class Program
    {
        static void Main(string[] args)
        {

            var client = new DotNettyRPCClient(new ClientConfig
            {
                ServerHostName = "127.0.0.1",
                ServerPort = 31000
            });

            var first_ser = client.CreateProxy<IMyFirstServer>();

            first_ser.SayHello("111");

            Console.WriteLine("Hello, World!");

            Console.ReadLine();
        }
    }
}
