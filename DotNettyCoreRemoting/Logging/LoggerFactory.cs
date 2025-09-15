using Microsoft.Extensions.Logging;

namespace DotNettyCoreRemoting.Logging
{
    /// <summary>
    /// 日志记录器工厂
    /// </summary>
    public class LoggerFactory : ILoggerFactory
    {
        private static LoggerFactory _instance;
        private static readonly object _lock = new object();
        private static Microsoft.Extensions.Logging.ILoggerFactory _microsoftLoggerFactory;
        
        private LoggerFactory() { }
        
        /// <summary>
        /// 获取日志记录器工厂实例
        /// </summary>
        public static LoggerFactory Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        if (_instance == null)
                        {
                            _instance = new LoggerFactory();
                            // 初始化微软日志工厂
                            _microsoftLoggerFactory = Microsoft.Extensions.Logging.LoggerFactory.Create(builder =>
                            {
                                builder.AddSimpleConsole(options => options.TimestampFormat = "[yyyy-MM-dd HH:mm:ss.fff] ");
                            });
                        }
                    }
                }
                return _instance;
            }
        }
        
        /// <summary>
        /// 创建日志记录器
        /// </summary>
        /// <param name="name">日志记录器名称</param>
        /// <returns>日志记录器实例</returns>
        public ILogger CreateLogger(string name)
        {
            if (_microsoftLoggerFactory != null)
            {
                var microsoftLogger = _microsoftLoggerFactory.CreateLogger(name);
                return new MicrosoftLoggerAdapter(microsoftLogger);
            }
            // 降级回原始实现
            return new ConsoleLogger(name);
        }
    }
}