using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Mods.DaggerLog
{
    public delegate void LogDelegate(Logger logger, LogType logType, string message);

    public sealed class Logger
    {
        private event LogDelegate onLog;
        private readonly string logName;

        public string GetName() => logName;
        public Logger(string logName) => this.logName = logName;

        public void Log(string message) => WriteLog(LogType.Log, message);
        public void Error(string message) => WriteLog(LogType.Error, message);
        public void Warning(string message) => WriteLog(LogType.Warning, message);
        public void Exception(string message) => WriteLog(LogType.Exception, message);
        public void Assert(string message) => WriteLog(LogType.Assert, message);

        public void WriteLog(LogType logType, string message) =>
            onLog?.Invoke(this, logType, message);

        public void Reset() => onLog = null;

        #region Static
        static readonly Dictionary<string, Logger> Loggers = new Dictionary<string, Logger>();

        static LogDelegate _appenders;

        public static void Add(LogDelegate appender)
        {
            _appenders += appender;
            foreach (var logger in Loggers.Values)
            {
                logger.onLog += appender;
            }
        }

        public static void Remove(LogDelegate appender)
        {
            _appenders -= appender;
            foreach (var logger in Loggers.Values)
            {
                logger.onLog -= appender;
            }
        }

        public static Logger GetLogger(Type type) => GetLogger(type.FullName);
        public static Logger GetLogger(string name)
        {
            if (!Loggers.TryGetValue(name, out var logger))
            {
                logger = new Logger(name)
                {
                    // TODO: Configure rolling logs, filesize limit?
                };
                logger.onLog += _appenders;
                Loggers.Add(name, logger);
            }
            return logger;
        }

        public static void ClearLoggers() => Loggers.Clear();
        public static void ClearAppenders()
        {
            _appenders = null;
            foreach (var logger in Loggers.Values)
            {
                logger.onLog = null;
            }
        }
        #endregion Static
    }
}