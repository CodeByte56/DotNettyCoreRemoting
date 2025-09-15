namespace DotNettyCoreRemoting.Logging
{
    /// <summary>
    /// 日志记录器工厂接口
    /// </summary>
    public interface ILoggerFactory
    {
        /// <summary>
        /// 创建日志记录器
        /// </summary>
        /// <param name="name">日志记录器名称</param>
        /// <returns>日志记录器实例</returns>
        ILogger CreateLogger(string name);
    }
}