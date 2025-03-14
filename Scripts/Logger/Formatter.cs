using UnityEngine;

namespace Game.Mods.DaggerLog
{
    public delegate string TimeDelegate();
    public delegate string Formatter(Logger logger, LogType logType, string message);

    public sealed class LogMessageFormatter
    {
        readonly TimeDelegate timeDelegate;
        readonly string format;

        public LogMessageFormatter(TimeDelegate timeDelegate, string format = "{0} [{1}] [{2}] {3}")
        {
            this.format = format;
            this.timeDelegate = timeDelegate;
        }
        public string FormatMessage(Logger logger, LogType logType, string message) => format != null && !string.IsNullOrEmpty(message)
            ? string.Format(format, timeDelegate(), logger.GetName(), Utils.GetLogTypeString(logType), message)
            : string.Empty;
    }
}