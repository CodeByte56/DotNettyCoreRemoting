using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace shared
{
    public interface IRemoteCommandService
    {
        // 同步调用
        object Execute(RemoteCallContext context);

        // 可选：支持异步
        Task<object> ExecuteAsync(RemoteCallContext context);
    }
}
