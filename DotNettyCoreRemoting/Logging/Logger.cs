using System;

namespace DotNettyCoreRemoting.Logging
{
    /// <summary>
    /// 日志记录器静态类
    /// </summary>
    public static class Logger
    {
        private static ILogger _logger;
        private static readonly object _lock = new object();
        
        private static ILogger CurrentLogger
        {
            get
            {
                if (_logger == null)
                {
                    lock (_lock)
                    {
                        if (_logger == null)
                        {
                            _logger = LoggerFactory.Instance.CreateLogger("DotNettyCoreRemoting");
                        }
                    }
                }
                return _logger;
            }
        }
        

        /// <summary>
        /// 记录信息日志
        /// </summary>
        /// <param name="message">日志消息</param>
        public static void Info(string message)
        {
            CurrentLogger.Info(message);
        }
        
        /// <summary>
        /// 记录信息日志
        /// </summary>
        /// <param name="message">日志消息</param>
        /// <param name="exception">异常对象</param>
        public static void Info(string message, Exception exception)
        {
            CurrentLogger.Info(message, exception);
        }
        
        /// <summary>
        /// 记录信息日志
        /// </summary>
        /// <param name="sourceType">源类型</param>
        /// <param name="message">日志消息</param>
        public static void Info(Type sourceType, string message)
        {
            CurrentLogger.Info(sourceType, message);
        }
        
        /// <summary>
        /// 记录信息日志
        /// </summary>
        /// <param name="sourceType">源类型</param>
        /// <param name="message">日志消息</param>
        /// <param name="exception">异常对象</param>
        public static void Info(Type sourceType, string message, Exception exception)
        {
            CurrentLogger.Info(sourceType, message, exception);
        }

        /// <summary>
        /// 记录错误日志
        /// </summary>
        /// <param name="message">日志消息</param>
        public static void Error(string message)
        {
            CurrentLogger.Error(message);
        }
        
        /// <summary>
        /// 记录错误日志
        /// </summary>
        /// <param name="message">日志消息</param>
        /// <param name="exception">异常对象</param>
        public static void Error(string message, Exception exception)
        {
            CurrentLogger.Error(message, exception);
        }
        
        /// <summary>
        /// 记录错误日志
        /// </summary>
        /// <param name="sourceType">源类型</param>
        /// <param name="message">日志消息</param>
        public static void Error(Type sourceType, string message)
        {
            CurrentLogger.Error(sourceType, message);
        }
        
        /// <summary>
        /// 记录错误日志
        /// </summary>
        /// <param name="sourceType">源类型</param>
        /// <param name="message">日志消息</param>
        /// <param name="exception">异常对象</param>
        public static void Error(Type sourceType, string message, Exception exception)
        {
            CurrentLogger.Error(sourceType, message, exception);
        }
    }
}