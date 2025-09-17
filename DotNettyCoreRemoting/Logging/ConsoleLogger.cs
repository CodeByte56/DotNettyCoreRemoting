using System;

namespace DotNettyCoreRemoting.Logging
{
    /// <summary>
    /// 控制台日志记录器
    /// </summary>
    public class ConsoleLogger : ILogger
    {
        private readonly string _name;
        
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="name">日志记录器名称</param>
        public ConsoleLogger(string name = null)
        {
            _name = name ?? "Default";
        }
        
        /// <summary>
        /// 记录错误日志
        /// </summary>
        /// <param name="message">日志消息</param>
        public void Error(string message)
        {
            WriteErrorLog(message, null, null);
        }
        
        /// <summary>
        /// 记录错误日志
        /// </summary>
        /// <param name="message">日志消息</param>
        /// <param name="exception">异常对象</param>
        public void Error(string message, Exception exception)
        {
            WriteErrorLog(message, null, exception);
        }
        
        /// <summary>
        /// 记录错误日志
        /// </summary>
        /// <param name="sourceType">源类型</param>
        /// <param name="message">日志消息</param>
        public void Error(Type sourceType, string message)
        {
            WriteErrorLog(message, sourceType, null);
        }
        
        /// <summary>
        /// 记录错误日志
        /// </summary>
        /// <param name="sourceType">源类型</param>
        /// <param name="message">日志消息</param>
        /// <param name="exception">异常对象</param>
        public void Error(Type sourceType, string message, Exception exception)
        {
            WriteErrorLog(message, sourceType, exception);
        }
        
        /// <summary>
        /// 记录信息日志
        /// </summary>
        /// <param name="message">日志消息</param>
        public void Info(string message)
        {
            WriteInfoLog(message, null, null);
        }
        
        /// <summary>
        /// 记录信息日志
        /// </summary>
        /// <param name="message">日志消息</param>
        /// <param name="exception">异常对象</param>
        public void Info(string message, Exception exception)
        {
            WriteInfoLog(message, null, exception);
        }
        
        /// <summary>
        /// 记录信息日志
        /// </summary>
        /// <param name="sourceType">源类型</param>
        /// <param name="message">日志消息</param>
        public void Info(Type sourceType, string message)
        {
            WriteInfoLog(message, sourceType, null);
        }
        
        /// <summary>
        /// 记录信息日志
        /// </summary>
        /// <param name="sourceType">源类型</param>
        /// <param name="message">日志消息</param>
        /// <param name="exception">异常对象</param>
        public void Info(Type sourceType, string message, Exception exception)
        {
            WriteInfoLog(message, sourceType, exception);
        }
        
        private void WriteErrorLog(string message, Type sourceType, Exception exception)
        {
            var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
            var source = sourceType != null ? sourceType.Name : "Unknown";
            
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"[{timestamp}] [ERROR] [{_name}] [{source}] {message}");
            
            if (exception != null)
            {
                Console.WriteLine($"{exception.Message}");
                Console.WriteLine($"{exception.StackTrace}");
            }
            
            Console.ResetColor();
        }
        
        private void WriteInfoLog(string message, Type sourceType, Exception exception)
        {
            var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
            var source = sourceType != null ? sourceType.Name : "Unknown";
            
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"[{timestamp}] [INFO] [{_name}] [{source}] {message}");
            
            if (exception != null)
            {
                Console.WriteLine($"{exception.Message}");
                Console.WriteLine($"{exception.StackTrace}");
            }
            
            Console.ResetColor();
        }
    }
}