using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.IO;

namespace AndrewTweddle.BattleCity.Experimental.CommandLine
{
    public static class PerformanceTestHelper
    {
        public static void TimeAction(string logFilePath, string title, int repetitions, Action action)
        {
            WriteTestTitle(logFilePath, title);
            Stopwatch swatch = Stopwatch.StartNew();
            try
            {
                for (int i = 0; i < repetitions; i++)
                {
                    action();
                }
                swatch.Stop();

                WriteDurationStats(logFilePath, repetitions, swatch);
            }
            catch (Exception exc)
            {
                WriteExceptionToLog(logFilePath, exc);
                throw;
            }
        }

        public static void TimeActionWithArgument<T>(string logFilePath,
            string title, int repetitions, T arg, Action<T> action)
        {
            WriteTestTitle(logFilePath, title);
            Stopwatch swatch = Stopwatch.StartNew();
            try
            {
                for (int i = 0; i < repetitions; i++)
                {
                    action(arg);
                }
                swatch.Stop();
                WriteDurationStats(logFilePath, repetitions, swatch);
            }
            catch (Exception exc)
            {
                WriteExceptionToLog(logFilePath, exc);
                throw;
            }
        }

        public static TResult TimeFunction<TResult>(string logFilePath, string title, int repetitions,
            Func<TResult> function)
        {
            TResult result = default(TResult);
            WriteTestTitle(logFilePath, title);
            Stopwatch swatch = Stopwatch.StartNew();
            try
            {
                for (int i = 0; i < repetitions; i++)
                {
                    result = function();
                }
                swatch.Stop();
                WriteDurationStats(logFilePath, repetitions, swatch);
                return result;
            }
            catch (Exception exc)
            {
                WriteExceptionToLog(logFilePath, exc);
                throw;
            }
        }

        public static TResult TimeFunctionWithArgument<T, TResult>(string logFilePath, string title, int repetitions,
            T arg, Func<T, TResult> function)
        {
            TResult result = default(TResult);
            WriteTestTitle(logFilePath, title);
            Stopwatch swatch = Stopwatch.StartNew();
            try
            {
                for (int i = 0; i < repetitions; i++)
                {
                    result = function(arg);
                }
                swatch.Stop();
                WriteDurationStats(logFilePath, repetitions, swatch);
            }
            catch (Exception exc)
            {
                WriteExceptionToLog(logFilePath, exc);
                throw;
            }
            return result;
        }

        public static void WriteCommentsToLog(string logFilePath, string comments)
        {
            if (!string.IsNullOrWhiteSpace(comments))
            {
                string message = String.Format(
                    "COMMENTS:\r\n{0}\r\n===============================================================================\r\n\r\n",
                    comments);
                File.AppendAllText(logFilePath, message);
            }
        }

        public static void WriteToLog(string logFilePath, string message)
        {
            File.AppendAllText(logFilePath, message);
            Console.Write(message);
        }

        public static void WriteExceptionToLog(string logFilePath, Exception exc)
        {
            string excMessage = String.Format("An error occurred: {0}\r\n", exc);
            File.AppendAllText(logFilePath, excMessage);
        }

        public static void WriteTestTitle(string logFilePath, string title)
        {
            string titleText = String.Format("Timing action: {0}\r\n\r\nStarting at: {1}\r\n", title, DateTime.Now);
            File.AppendAllText(logFilePath, titleText);
            Console.WriteLine(titleText);
        }

        public static void WriteDurationStats(string logFilePath, int repetitions, Stopwatch swatch)
        {
            StringBuilder sb = new StringBuilder();
            StringWriter sw = new StringWriter(sb);
            sw.WriteLine("Duration for {0} repetitions: {1}", repetitions, swatch.Elapsed);
            sw.WriteLine("Average duration: {0} microseconds", swatch.ElapsedMilliseconds * 1000.0 / repetitions);
            sw.WriteLine("-------------------------------------------------------------------------------");
            sw.WriteLine();
            sw.Flush();
            string durationText = sb.ToString();
            Console.Write(durationText);
            File.AppendAllText(logFilePath, durationText);
        }

    }
}
