using Microsoft.Extensions.Logging;
using System;

namespace DotNettyCoreRemoting.Logging
{


    public class DotNettyCoreRemotingLogger
    {

    }

    /// <summary>
    /// 日志记录器静态类
    /// </summary>
    internal static class Logger
    {
        private static ILoggerFactory? _loggerFactory;
        private static readonly object _lock = new();

        /// <summary>
        /// 由 Host 启动时注入（推荐方式）
        /// </summary>
        public static void SetLoggerFactory(ILoggerFactory factory)
        {
            _loggerFactory = factory ?? throw new ArgumentNullException(nameof(factory));
        }

        /// <summary>
        /// 获取指定类别的 ILogger（支持泛型）
        /// </summary>
        private static ILogger GetLogger(Type categoryType)
        {
            if (_loggerFactory == null)
            {
                lock (_lock)
                {
                    if (_loggerFactory == null)
                    {
                        // ❗ 兜底：临时创建 Console Logger（仅用于开发/调试）
                        _loggerFactory = Microsoft.Extensions.Logging.LoggerFactory.Create(builder =>
                        {
                            builder.AddSimpleConsole(options =>
                            {
                                options.TimestampFormat = "[yyyy-MM-dd HH:mm:ss.fff] ";
                                options.SingleLine = true;
                            });
                        });
                        System.Diagnostics.Debug.WriteLine("⚠️ AppLogger 在 Host 启动前被使用，日志未被 Serilog 接管。");
                    }
                }
            }
            return _loggerFactory.CreateLogger(categoryType);
        }

        // ========== 封装常用日志方法 ==========

        public static void Error(string message, params object?[] args)
            => GetLogger(typeof(DotNettyCoreRemotingLogger)).LogError(message, args);

        public static void Error(Type type, string message, params object?[] args)
             => GetLogger(type).LogError(message, args);

        public static void Error(Exception exception, string message, params object?[] args)
            => GetLogger(typeof(DotNettyCoreRemotingLogger)).LogError(exception, message, args);


        public static void Error(Type type, Exception exception, string message, params object?[] args)
             => GetLogger(type).LogError(exception, message, args);

        public static void Warn(string message, params object?[] args)
            => GetLogger(typeof(DotNettyCoreRemotingLogger)).LogWarning(message, args);

        public static void Info(string message, params object?[] args)
            => GetLogger(typeof(DotNettyCoreRemotingLogger)).LogInformation(message, args);

        public static void Info(Type type, string message, params object?[] args)
            => GetLogger(type).LogInformation(message, args);

        public static void Debug(string message, params object?[] args)
            => GetLogger(typeof(DotNettyCoreRemotingLogger)).LogDebug(message, args);

        public static void Trace(string message, params object?[] args)
            => GetLogger(typeof(DotNettyCoreRemotingLogger)).LogTrace(message, args);

        // ========== 支持指定分类的日志方法（推荐用于类内使用） ==========

        public static void Error<TCategory>(string message, params object?[] args)
            => GetLogger(typeof(TCategory)).LogError(message, args);

        public static void Error<TCategory>(Exception exception, string message, params object?[] args)
            => GetLogger(typeof(TCategory)).LogError(exception, message, args);

        public static void Warn<TCategory>(string message, params object?[] args)
            => GetLogger(typeof(TCategory)).LogWarning(message, args);

        public static void Info<TCategory>(string message, params object?[] args)
            => GetLogger(typeof(TCategory)).LogInformation(message, args);

        public static void Debug<TCategory>(string message, params object?[] args)
            => GetLogger(typeof(TCategory)).LogDebug(message, args);

        public static void Trace<TCategory>(string message, params object?[] args)
            => GetLogger(typeof(TCategory)).LogTrace(message, args);
    }
}