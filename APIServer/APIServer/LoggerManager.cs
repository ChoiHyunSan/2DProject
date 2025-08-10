using Microsoft.Extensions.Logging;
using ZLogger;

namespace APIServer
{
    public static class LoggerManager
    {
        private static ILogger _globalLogger = null!;
        private static ILoggerFactory _loggerFactory = null!;

        public static void SetLoggerFactory(ILoggerFactory factory, string categoryName)
        {
            _loggerFactory = factory;
            _globalLogger = factory.CreateLogger(categoryName);
        }

        public static ILogger Logger => _globalLogger;
        public static ILogger<T> GetLogger<T>() where T : class => _loggerFactory.CreateLogger<T>();

        // INFO: Timestamp + eventType + message + (payload)
        public static void LogInfo(ILogger logger, EventType eventType, string message, object? payload = null)
        {
            if (payload is null)
            {
                // 이름 붙여 캡처( :@ ) → JSON 키로 출력됨
                logger.ZLogInformation($"eventType:{eventType:@eventType} message:{message:@message}");
            }
            else
            {
                // 객체는 :json 으로 직렬화 + 키 이름 지정( :@payload )
                logger.ZLogInformation($"eventType:{eventType:@eventType} message:{message:@message} payload:{payload:json:@payload}");
            }
        }

        // ERROR: Timestamp + errorCode + eventType + message + (payload)
        public static void LogError(ILogger logger, ErrorCode errorCode, EventType eventType, string message, object? payload = null)
        {
            if (payload is null)
            {
                logger.ZLogError($"errorCode:{errorCode:@errorCode} eventType:{eventType:@eventType} message:{message:@message}");
            }
            else
            {
                logger.ZLogError($"errorCode:{errorCode:@errorCode} eventType:{eventType:@eventType} message:{message:@message} payload:{payload:json:@payload}");
            }
        }
    }
}