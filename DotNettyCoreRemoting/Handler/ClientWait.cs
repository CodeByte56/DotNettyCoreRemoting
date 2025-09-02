using DotNetty.Buffers;
using DotNettyCoreRemoting.RpcMessaging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DotNettyCoreRemoting.Handler
{
    public class ClientWait
    {
        private ConcurrentDictionary<string, ClientObj> _waits { get; set; } = new ConcurrentDictionary<string, ClientObj>();
        public void Start(string id)
        {
            _waits[id] = new ClientObj();
        }
        public void Set(string id, byte[] response)
        {
            //var theObj = _waits[id];
            //theObj.ReturnResponse = response;
            //theObj.WaitHandler.Set();


            if (_waits.TryGetValue(id, out var theObj))
            {
                theObj.ReturnResponse = response;
                theObj.WaitHandler.Set(); // ✅ 触发事件，唤醒 Wait
            }

        }
        public ClientObj Wait(string id, TimeSpan? timeout = null)
        {
            //var clientObj = _waits[id];
            //clientObj.WaitHandler.WaitOne();
            //Task.Run(() =>
            //{
            //    _waits.TryRemove(id, out ClientObj value);
            //});
            //return clientObj;


            if (!_waits.TryGetValue(id, out var clientObj))
            {
                throw new KeyNotFoundException($"等待键 '{id}' 不存在，可能已超时或被提前清理。");
            }

            // ✅ 使用 WaitOne(TimeSpan) 实现超时控制
            bool signaled = timeout.HasValue
                ? clientObj.WaitHandler.WaitOne(timeout.Value)
                : clientObj.WaitHandler.WaitOne(); // 不推荐无限等待

            if (!signaled)
            {
                // ⏳ 超时了，清理资源
                _waits.TryRemove(id, out _);
                throw new TimeoutException($"等待服务端响应超时。等待键: {id}，超时时间: {timeout?.TotalSeconds} 秒。");
            }

            // ✅ 等待成功，响应已填充，异步清理字典
            Task.Run(() =>
            {
                _waits.TryRemove(id, out _);
            });

            return clientObj;
        }
    }
}
