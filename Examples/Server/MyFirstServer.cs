using shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreServer
{
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

        public string SayHello_str(string msg)
        {
            return msg;
        }
    }
}
