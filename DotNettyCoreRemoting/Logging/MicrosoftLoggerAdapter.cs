using System;
using Microsoft.Extensions.Logging;

namespace DotNettyCoreRemoting.Logging
{
    /// <summary>
    /// 微软日志适配器
    /// 用于将微软的ILogger接口适配到项目自定义的ILogger接口
    /// </summary>
    public class MicrosoftLoggerAdapter : ILogger
    {
        private readonly Microsoft.Extensions.Logging.ILogger _microsoftLogger;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="microsoftLogger">微软的日志记录器实例</param>
        public MicrosoftLoggerAdapter(Microsoft.Extensions.Logging.ILogger microsoftLogger)
        {
            _microsoftLogger = microsoftLogger ?? throw new ArgumentNullException(nameof(microsoftLogger));
        }

        /// <summary>
        /// 记录错误日志
        /// </summary>
        /// <param name="message">日志消息</param>
        public void Error(string message)
        {
            _microsoftLogger.LogError(message);
        }

        /// <summary>
        /// 记录错误日志
        /// </summary>
        /// <param name="message">日志消息</param>
        /// <param name="exception">异常对象</param>
        public void Error(string message, Exception exception)
        {
            _microsoftLogger.LogError(exception, message);
        }

        /// <summary>
        /// 记录错误日志
        /// </summary>
        /// <param name="sourceType">源类型</param>
        /// <param name="message">日志消息</param>
        public void Error(Type sourceType, string message)
        {
            if (sourceType == null)
            {
                _microsoftLogger.LogError(message);
            }
            else
            {
                using (_microsoftLogger.BeginScope(new { SourceType = sourceType.FullName }))
                {
                    _microsoftLogger.LogError(message);
                }
            }
        }

        /// <summary>
        /// 记录错误日志
        /// </summary>
        /// <param name="sourceType">源类型</param>
        /// <param name="message">日志消息</param>
        /// <param name="exception">异常对象</param>
        public void Error(Type sourceType, string message, Exception exception)
        {
            if (sourceType == null)
            {
                _microsoftLogger.LogError(exception, message);
            }
            else
            {
                using (_microsoftLogger.BeginScope(new { SourceType = sourceType.FullName }))
                {
                    _microsoftLogger.LogError(exception, message);
                }
            }
        }

        /// <summary>
        /// 记录信息日志
        /// </summary>
        /// <param name="message">日志消息</param>
        public void Info(string message)
        {
            _microsoftLogger.LogInformation(message);
        }

        /// <summary>
        /// 记录信息日志
        /// </summary>
        /// <param name="message">日志消息</param>
        /// <param name="exception">异常对象</param>
        public void Info(string message, Exception exception)
        {
            _microsoftLogger.LogInformation(exception, message);
        }

        /// <summary>
        /// 记录信息日志
        /// </summary>
        /// <param name="sourceType">源类型</param>
        /// <param name="message">日志消息</param>
        public void Info(Type sourceType, string message)
        {
            if (sourceType == null)
            {
                _microsoftLogger.LogInformation(message);
            }
            else
            {
                using (_microsoftLogger.BeginScope(new { SourceType = sourceType.FullName }))
                {
                    _microsoftLogger.LogInformation(message);
                }
            }
        }

        /// <summary>
        /// 记录信息日志
        /// </summary>
        /// <param name="sourceType">源类型</param>
        /// <param name="message">日志消息</param>
        /// <param name="exception">异常对象</param>
        public void Info(Type sourceType, string message, Exception exception)
        {
            if (sourceType == null)
            {
                _microsoftLogger.LogInformation(exception, message);
            }
            else
            {
                using (_microsoftLogger.BeginScope(new { SourceType = sourceType.FullName }))
                {
                    _microsoftLogger.LogInformation(exception, message);
                }
            }
        }
    }
}