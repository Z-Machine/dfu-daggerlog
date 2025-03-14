using DaggerfallWorkshop.Game;
using DaggerfallWorkshop.Game.Utility.ModSupport;
using System;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.Localization.SmartFormat;
using Wenzil.Console;

namespace Game.Mods.DaggerLog
{
    public class DaggerLog : MonoBehaviour
    {
        protected static DaggerLog instance;

        public static bool HasInstance => instance != null;
        public static DaggerLog TryGetInstance() => HasInstance ? instance : null;
        public static DaggerLog Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindObjectOfType<DaggerLog>();
                    if (instance == null)
                    {
                        var go = new GameObject(nameof(DaggerLog));
                        instance = go.AddComponent<DaggerLog>();
                        DontDestroyOnLoad(go);
                    }
                }
                return instance;
            }
        }

        public static Mod Mod { get; private set; }
        public Mod GetMod() => Mod;

        protected Logger logger;
        protected ExceptionWindow exceptionWindow;
        protected ExceptionDatabase exceptionDatabase;


        [Invoke(StateManager.StateTypes.Start, 0)]
        public static void Init(InitParams initParams)
        {
            if (!Application.isPlaying) return;
            Mod = initParams.Mod;

            var uiManager = DaggerfallUI.Instance.UserInterfaceManager;
            Instance.exceptionWindow = new ExceptionWindow(uiManager);
            Instance.exceptionDatabase = new ExceptionDatabase();

            RegisterCommands();

            // Ensure log directory exists.
            if (!Directory.Exists(Utils.GetLogFolderPath()))
            {
                Directory.CreateDirectory(Utils.GetLogFolderPath());
            }

            // Setup logger
            Instance.logger = Logger.GetLogger(nameof(DaggerLog));
            var fileWriter = new FileSink(Utils.GetCurrentLogPath());
            var formatter = new LogMessageFormatter(Utils.GetTimestamp);
            Logger.Add((logger, logType, message) =>
            {
                message = formatter.FormatMessage(logger, logType, message);
                fileWriter.WriteLine(logger, logType, message);
            });

            // Register Events
            UnityLogHandler logHandler;
            if (!UnityLogHandler.TryOverrideLogger(out logHandler))
            {
                // Fallback to this event if we don't override.
                Application.logMessageReceived += HandleLogMessage;
            }

            AppDomain.CurrentDomain.UnhandledException += HandleException;

            // Clean-up
            Application.quitting += () =>
            {
                AppDomain.CurrentDomain.UnhandledException -= HandleException;
                Application.logMessageReceived -= HandleLogMessage;
                fileWriter?.Dispose();
            };

            Mod.IsReady = true;
        }

        internal static void HandleException(object sender, UnhandledExceptionEventArgs e)
        {
            var exception = e.ExceptionObject as Exception;

            if (e.IsTerminating)
            {
                Instance.logger.Warning($"!!FATAL EXCEPTION ENCOUNTERED!!");
            }

            //Debug.LogException(exception, sender as UnityEngine.Object);
            var stacktrace = exception.StackTrace ?? new System.Diagnostics.StackTrace(1).ToString();
            HandleLogMessage(exception.ToString(), stacktrace, LogType.Exception);
        }

        internal static void HandleLogMessage(string message, string stacktrace, LogType logType)
        {
            var logger = Instance.logger;
            if (logger == null) return;

            if (logType == LogType.Error || logType == LogType.Exception)
            {
                if (string.IsNullOrEmpty(stacktrace))
                {
                    stacktrace = new System.Diagnostics.StackTrace(1).ToString();
                }
                logger.WriteLog(logType, $"{message}{Environment.NewLine}{stacktrace}");
            }
            else
            {
                logger.WriteLog(logType, message);
            }

            if (logType == LogType.Exception)
            {
                ShowExceptionWindow(message);
            }
        }

        protected static void RegisterCommands()
        {
            ConsoleCommandsDatabase.RegisterCommand("exception", args =>
            {
                var exception = new Exception($"Requested Exception. Args={string.Join(", ", args)}");
                Debug.LogAssertion("Assertion");
                Debug.LogError("Error");
                Debug.LogWarning("Warning");
                throw exception;
            }, "Raises an exception.");

            ConsoleCommandsDatabase.RegisterCommand("force-crash", args =>
            {
                if (args.Length != 1) return "Invalid arguments";
                if (!bool.TryParse(args[0], out var shouldRun)) return "Must be true/false";
                if (shouldRun == false) return "Not crashing.";
                Utils.ForceCrash();
                return "Crashed.";
            }, "Crashes the game. Must pass true.");
        }

        protected static void ShowExceptionWindow(string exception)
        {
            // If the exception was already encountered then we have ignored it.
            if (!Instance.exceptionDatabase.AddException(exception)) return;

            // Close console cause it's annoying w/ overlap
            var console = GameObject.Find("Console");
            if (console && console.TryGetComponent<ConsoleUI>(out var ui) && ui.isConsoleOpen)
            {
                ui.ToggleConsole(true);
                ui.CloseConsole();
            }

            var sb = new StringBuilder();
            Utils.AppendModList(sb);
            //Utils.AppendPlayerInfo(sb);
            Utils.AppendLocationInfo(sb);
            sb.AppendLine(exception);

            Instance.exceptionWindow.CurrentMessage = exception;
            Instance.exceptionWindow.ClipboardContents = sb.ToString();

            var uiManager = DaggerfallUI.Instance.UserInterfaceManager;
            uiManager.PushWindow(Instance.exceptionWindow);
        }
    }
}