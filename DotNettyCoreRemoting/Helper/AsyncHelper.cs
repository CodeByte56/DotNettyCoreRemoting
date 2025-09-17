using System;
using System.Threading;
using System.Threading.Tasks;
using DotNettyCoreRemoting.Logging;

namespace DotNettyCoreRemoting.Util
{
    /// <summary>
    /// 异步转同步,防止ASP.NET中死锁
    /// https://cpratt.co/async-tips-tricks/
    /// </summary>
    public static class AsyncHelper
    {
        private static readonly TaskFactory _myTaskFactory =
            new TaskFactory(CancellationToken.None, TaskCreationOptions.None, TaskContinuationOptions.None, TaskScheduler.Default);

        /// <summary>
        /// 同步执行
        /// </summary>
        /// <param name="func">任务</param>
        public static void RunSync(Func<Task> func)
        {
            try
            {
                _myTaskFactory.StartNew(func).Unwrap().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Logger.Error(typeof(AsyncHelper), "异步转同步执行失败", ex);
                throw;
            }
        }

        /// <summary>
        /// 同步执行
        /// </summary>
        /// <typeparam name="TResult">返回类型</typeparam>
        /// <param name="func">任务</param>
        /// <returns></returns>
        public static TResult RunSync<TResult>(Func<Task<TResult>> func)
        {
            try
            {
                return _myTaskFactory.StartNew(func).Unwrap().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Logger.Error(typeof(AsyncHelper), "异步转同步执行失败(带返回值)", ex);
                throw;
            }
        }
    }
}
