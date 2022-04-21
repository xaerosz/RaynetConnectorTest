using Microsoft.Extensions.Logging;
using System;

namespace RaynetConnectorTest
{
    public class Utilities
    {
        static ILoggerFactory _loggerFactory;


        public static void ConfigureLogger(ILoggerFactory loggerFactory)
        {
            _loggerFactory = loggerFactory;
        }


        public static ILogger CreateLogger<T>()
        {
            //Usage: Utilities.CreateLogger<SomeClass>().LogError(LoggingEvents.SomeEventId, ex, "An error occurred because of xyz");

            if (_loggerFactory == null)
            {
                throw new InvalidOperationException($"{nameof(ILogger)} is not configured. {nameof(ConfigureLogger)} must be called before use");
            }

            return _loggerFactory.CreateLogger<T>();
        }
    }
}
