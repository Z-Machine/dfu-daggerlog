using System;
using System.IO;
using System.Linq;
using System.Text;
using DaggerfallWorkshop;
using DaggerfallWorkshop.Game;
using DaggerfallWorkshop.Game.Utility.ModSupport;
using UnityEngine;
using UnityEngine.Localization.SmartFormat;

namespace Game.Mods.DaggerLog
{
    public static class Utils
    {
        public static string GetLogFolderPath() => Path.Combine(DaggerfallUnityApplication.PersistentDataPath, "DaggerLog");
        public static string GetCrashLogPath() => Path.Combine(GetLogFolderPath(), "crash.log");
        public static string GetCurrentLogPath() => Path.Combine(GetLogFolderPath(), "current.log");
        public static string GetTimestamp() => DateTimeOffset.Now.ToString("HH:mm:ss");
        public static string GetFileTimestamp() => DateTimeOffset.Now.ToString("yyyy-MM-dd_HH-mm-ss");
        public static string GetLogTypeString(LogType type) => Enum.GetName(typeof(LogType), type) ?? "Unknown";
        public static void ForceCrash() => UnityEngine.Diagnostics.Utils.ForceCrash(UnityEngine.Diagnostics.ForcedCrashCategory.FatalError);
        public static void AppendModList(StringBuilder sb)
        {
            var mods = ModManager.Instance.Mods.Where(mod => mod.Enabled);
            sb.AppendLineSmart("Modlist: {0} currently enabled mods.", mods.Count());
            foreach (var mod in mods)
            {
                sb.AppendLineSmart("[{0}] '{1}' GUID={2}", mod.LoadPriority, mod.Title, mod.GUID);
            }
            sb.AppendLine();
        }
        public static void AppendLocationInfo(StringBuilder sb)
        {
            var gps = GameManager.Instance.PlayerGPS;
            sb.AppendLineSmart("Location: {0}, {1}", gps.CurrentLocalizedLocationName, gps.CurrentLocalizedRegionName);
            sb.AppendLineSmart("World Position: {0}, {1}", gps.WorldX, gps.WorldZ);
            sb.AppendLine();
        }
    }
}