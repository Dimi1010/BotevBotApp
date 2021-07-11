using Discord;
using Microsoft.Extensions.Logging;
using System;

namespace BotevBotApp.Extensions
{
    public static class LogLevelConversions
    {
        /// <summary>
        /// Converts the <see cref="LogSeverity"/> value to corresponding <see cref="LogLevel"/>.
        /// </summary>
        /// <param name="severity">The value to convert.</param>
        /// <returns>The converted value.</returns>
        public static LogLevel ToLogLevel(this LogSeverity severity)
        {
            return severity switch
            {
                LogSeverity.Critical => LogLevel.Critical,
                LogSeverity.Error => LogLevel.Error,
                LogSeverity.Warning => LogLevel.Warning,
                LogSeverity.Info => LogLevel.Information,
                LogSeverity.Verbose => LogLevel.Debug,
                LogSeverity.Debug => LogLevel.Trace,
                _ => throw new NotSupportedException("The provided value does not currenty have a conversion."),
            };
        }

        /// <summary>
        /// Converts the <see cref="LogLevel"/> value to corresponding <see cref="LogSeverity"/>.
        /// </summary>
        /// <param name="logLevel">The value to convert.</param>
        /// <returns>The converted value.</returns>
        public static LogSeverity ToLogSeverity(this LogLevel logLevel)
        {
            return logLevel switch
            {
                LogLevel.Trace => LogSeverity.Debug,
                LogLevel.Debug => LogSeverity.Verbose,
                LogLevel.Information => LogSeverity.Info,
                LogLevel.Warning => LogSeverity.Warning,
                LogLevel.Error => LogSeverity.Error,
                LogLevel.Critical => LogSeverity.Critical,
                LogLevel.None => LogSeverity.Critical,
                _ => throw new NotSupportedException("The provided value does not currently have a conversion."),
            };
        }
    }
}
