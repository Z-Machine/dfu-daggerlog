using System;
using UnityEngine;

namespace Game.Mods.DaggerLog
{
    public sealed class UnityLogHandler : ILogHandler, IDisposable
    {
        private DaggerfallUnityApplication.LogHandler previousHandler;

        private UnityLogHandler(ILogHandler previousHandler)
        {
            if (previousHandler != null && previousHandler is DaggerfallUnityApplication.LogHandler logHandler)
            {
                this.previousHandler = logHandler;
            }
        }

        public void LogException(Exception exception, UnityEngine.Object context)
        {
            try
            {
                DaggerLog.HandleException(context, new UnhandledExceptionEventArgs(exception, false));
                previousHandler?.LogException(exception, context);
            }
            catch (Exception) { }
        }

        public void LogFormat(LogType logType, UnityEngine.Object context, string format, params object[] args)
        {
            try
            {
                var message = string.Format(format, args);
                if (logType != LogType.Exception)
                {
                    DaggerLog.HandleLogMessage(message, null, logType);
                }
                previousHandler?.LogFormat(logType, context, message);
            }
            catch (Exception) { }
        }

        public void Dispose()
        {
            previousHandler?.Dispose();
        }

        internal static bool TryOverrideLogger(out UnityLogHandler logHandler)
        {
            logHandler = null;
            if (!Application.isPlaying) return false;
            if (Application.installMode == ApplicationInstallMode.Editor) return false;

            try
            {
                var prevLogHandler = Debug.unityLogger.logHandler;
                logHandler = new UnityLogHandler(prevLogHandler);
                Debug.unityLogger.logHandler = logHandler;

                logHandler?.LogFormat(LogType.Log, DaggerLog.Instance, "Debug.unityLogger.logHandler is now controlled by {0}.", nameof(DaggerLog));
            }
            catch (Exception) { }

            return logHandler != null;
        }
    }
}