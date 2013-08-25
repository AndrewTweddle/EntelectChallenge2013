using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Reflection;
using AndrewTweddle.BattleCity.Core.Elements;
using AndrewTweddle.BattleCity.Core.Calculations.Distances;
using AndrewTweddle.BattleCity.Core.States;

namespace AndrewTweddle.BattleCity.Core.Helpers
{
    public static class DebugHelper
    {
        public static string LogFolder { get; private set; }
        public static string AppName { get; private set; }
        public static DateTime TimeStarted { get; private set; }

        [Conditional("DEBUG")]
        public static void InitializeLogFolder(DateTime timeStarted, 
            string rootFolder = null, string appName = null)
        {
            if (string.IsNullOrEmpty(appName))
            {
                appName = Assembly.GetEntryAssembly().GetName().Name;
            }
            
            string timeString = timeStarted.ToString("yyyy-MM-dd_HHmm");
            string logFolder = System.IO.Path.Combine(rootFolder ?? String.Empty, timeString, appName);
            if (System.IO.Directory.Exists(logFolder))
            {
                // There is already such a folder, so append the seconds to get uniqueness:
                timeString = timeStarted.ToString("yyyy-MM-dd_HHmmss");
                logFolder = System.IO.Path.Combine(rootFolder ?? String.Empty, timeString, appName);
            }

            // Set various parameters:
            AppName = appName;
            TimeStarted = timeStarted;
            System.IO.Directory.CreateDirectory(logFolder);
            LogFolder = logFolder;
        }

        /// <summary>
        /// Generate a full file path under the log folder.
        /// </summary>
        /// <param name="fileFormat">The default file format to use. The following substitutions are performed:
        /// {0} = AppName
        /// {1} = TimeStarted
        /// {2} = Current time
        /// {3} = Current game tick
        /// </param>
        /// <param name="longFileFormat">The long file format is used if the LogFolder property is empty</param>
        /// <returns></returns>
        public static string GenerateFilePath(string fileFormat, string longFileFormat = null)
        {
            int tick = Game.Current.CurrentTurn == null ? 0 : Game.Current.CurrentTurn.Tick;
            string filePath;
            if (string.IsNullOrWhiteSpace(LogFolder))
            {
                if (string.IsNullOrEmpty(longFileFormat))
                {
                    filePath = String.Format(fileFormat, AppName, TimeStarted, DateTime.Now, tick);
                }
                else
                {
                    filePath = String.Format(longFileFormat, AppName, TimeStarted, DateTime.Now, tick);
                }
            }
            else
            {
                filePath = System.IO.Path.Combine(
                    LogFolder,
                    String.Format(fileFormat, AppName, TimeStarted, DateTime.Now, tick));
                string subDirectory = System.IO.Path.GetDirectoryName(filePath);
                if (!System.IO.Directory.Exists(subDirectory))
                {
                    System.IO.Directory.CreateDirectory(subDirectory);
                }
            }
            return filePath;
        }

        [Conditional("DEBUG")]
        public static void WireUpDebugListeners(bool includeConsoleListener = false)
        {
            Debug.AutoFlush = true;

            // Add console listener:
            if (includeConsoleListener)
            {
                Debug.Listeners.Add(new TextWriterTraceListener(Console.Out));
            }

            // Add log file listener:
            string appName = Assembly.GetEntryAssembly().GetName().Name;
            string logFileName = GenerateFilePath("{0}.log",
                String.Format("{0}_{1}.log", appName, DateTime.Now.ToString("yyyy-MM-dd_HHmmss")));
            Trace.Listeners.Add(new TextWriterTraceListener(logFileName));
        }

        [System.Diagnostics.Conditional("DEBUG")]
        public static void LogDebugMessage(string caller, string format, params object[] args)
        {
            string message = args.Length == 0 ? format : String.Format(format, args);
            string stage = 
                Game.Current.CurrentTurn != null 
                ? string.Format("Tick {0}", Game.Current.CurrentTurn.Tick)
                : "BEFORE GAME";
            System.Diagnostics.Debug.WriteLine("{0} @ {1} | {2} | {3}",
                stage, DateTime.Now.ToString("HH:mm:ss"), caller, message);
        }

        [System.Diagnostics.Conditional("DEBUG")]
        public static void LogDebugError(string caller, Exception exc, string message = "")
        {
            LogDebugMessage(caller, "ERROR! {0}", message);
            System.Diagnostics.Debug.WriteLine("");
            System.Diagnostics.Debug.WriteLine(
                "{0}.{1}{1}STACK TRACE:{1}{2}", 
                exc, 
                Environment.NewLine, 
                exc.StackTrace);
        }

        [System.Diagnostics.Conditional("DEBUG")]
        public static void WriteLine(char repeatChar = ' ')
        {
            string line = "";
            if (repeatChar != ' ')
            {
                line = new string(repeatChar, 60);
            }
            System.Diagnostics.Debug.WriteLine(line);
        }
    }
}
