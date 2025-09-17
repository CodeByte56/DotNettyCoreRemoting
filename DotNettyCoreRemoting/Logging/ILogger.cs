using System;

namespace DotNettyCoreRemoting.Logging
{
    /// <summary>
    /// 日志记录器接口
    /// </summary>
    public interface ILogger
    {
        /// <summary>
        /// 记录错误日志
        /// </summary>
        /// <param name="message">日志消息</param>
        void Error(string message);
        
        /// <summary>
        /// 记录错误日志
        /// </summary>
        /// <param name="message">日志消息</param>
        /// <param name="exception">异常对象</param>
        void Error(string message, Exception exception);
        
        /// <summary>
        /// 记录错误日志
        /// </summary>
        /// <param name="sourceType">源类型</param>
        /// <param name="message">日志消息</param>
        void Error(Type sourceType, string message);
        
        /// <summary>
        /// 记录错误日志
        /// </summary>
        /// <param name="sourceType">源类型</param>
        /// <param name="message">日志消息</param>
        /// <param name="exception">异常对象</param>
        void Error(Type sourceType, string message, Exception exception);
        
        /// <summary>
        /// 记录信息日志
        /// </summary>
        /// <param name="message">日志消息</param>
        void Info(string message);
        
        /// <summary>
        /// 记录信息日志
        /// </summary>
        /// <param name="message">日志消息</param>
        /// <param name="exception">异常对象</param>
        void Info(string message, Exception exception);
        
        /// <summary>
        /// 记录信息日志
        /// </summary>
        /// <param name="sourceType">源类型</param>
        /// <param name="message">日志消息</param>
        void Info(Type sourceType, string message);
        
        /// <summary>
        /// 记录信息日志
        /// </summary>
        /// <param name="sourceType">源类型</param>
        /// <param name="message">日志消息</param>
        /// <param name="exception">异常对象</param>
        void Info(Type sourceType, string message, Exception exception);
    }
}