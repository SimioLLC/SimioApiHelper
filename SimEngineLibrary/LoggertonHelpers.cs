using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace LoggertonHelpers
{

    /// <summary>
    /// Bitmask to determine what type of log is being generated.
    /// </summary>
    [Flags]
    public enum EnumLogFlags
    {
        None = 0,
        Information = 1,
        Event = 2,
        Warning = 4,
        Error = 8,
        All = 0xffff
    }

    public interface ILogEntry
    {
        DateTime TimeStamp { get; set; }
        string Message { get; set; }
        EnumLogFlags Flags { get; set; }

        bool IsExcluded { get; set; }
    }

    public interface ILogger
    {
        string GetLogs(EnumLogFlags flags);

        List<ILogEntry> GetLogsAsList(EnumLogFlags flags);

        void LogIt(EnumLogFlags logType, string message);

        /// <summary>
        /// Clear all cached logs.
        /// </summary>
        void ClearLogs();


    }

    public class LogEntry : ILogEntry
    {
        public EnumLogFlags Flags { get; set; }

        public DateTime TimeStamp { get; set; }

        public string Message { get; set; }

        /// <summary>
        /// Has this message been excluded by regex excludes?
        /// </summary>
        public bool IsExcluded { get; set; }

        public LogEntry(EnumLogFlags flags, String msg)
        {
            Flags = flags;
            TimeStamp = DateTime.Now;
            Message = msg;
        }

        public override string ToString()
        {
            return $"{Flags}:{TimeStamp.ToString("HH:mm:ss.ff")} {Message}";
        }
    }

    public sealed class Loggerton : ILogger
    {
        private static readonly Loggerton instance = new Loggerton();
        public static Loggerton Instance { get { return instance; } }

        static Loggerton()
        {
        }
        private Loggerton()
        {

        }

        public bool IsEnabled = true;

        private List<ILogEntry> Logs = new List<ILogEntry>();

        private List<string> excludesList = new List<string>();

        /// <summary>
        /// Given a comma list of enumLogFlags, set all cached
        /// logs with those flags as excluded (case-insensitive)
        /// </summary>
        /// <param name="commalist"></param>
        public void SetExcludes(string commalist)
        {
            excludesList.Clear();
            excludesList = commalist.Split(',').ToList();

            // Reevaluate all the logs
            foreach (LogEntry le in Logs)
            {
                le.IsExcluded = false;
                foreach (string expr in excludesList)
                {
                    if (Regex.IsMatch(le.Message, expr, RegexOptions.IgnoreCase))
                    {
                        le.IsExcluded = true;
                        goto GetNextLogEntry;
                    }
                }
                GetNextLogEntry:;
            }
        }

        /// <summary>
        /// Get all logs that should be displayed.
        /// </summary>
        /// <param name="flags"></param>
        /// <param name="excludes"></param>
        /// <returns></returns>
        public string GetLogs(EnumLogFlags flags)
        {
            List<ILogEntry> filteredLogs = GetLogsAsList(flags);

            StringBuilder sb = new StringBuilder();
            foreach (LogEntry le in filteredLogs)
            {
                sb.AppendLine($"{le}");
            }

            return sb.ToString();
        }

        /// <summary>
        /// Get all logs that should be displayed.
        /// </summary>
        /// <param name="flags"></param>
        /// <param name="excludes"></param>
        /// <returns></returns>
        public List<ILogEntry> GetLogsAsList(EnumLogFlags flags)
        {
            List<ILogEntry> filteredLogs = Logs
                .Where(rr => (rr.Flags & flags) != 0)
                .Where(rr => !rr.IsExcluded)
                .OrderByDescending(rr => rr.TimeStamp)
                .ToList();

            return filteredLogs;
        }

        /// <summary>
        /// Clear all internal logs
        /// </summary>
        public void ClearLogs()
        {
            Logs.Clear();
        }

        /// <summary>
        /// Return all logs as a string with cr/lf
        /// </summary>
        /// <returns></returns>
        public string ShowLogs()
        {
            string filteredLogs = GetLogs(EnumLogFlags.All);
            return filteredLogs;
        }

        /// <summary>
        /// Write the logs to a file
        /// </summary>
        /// <param name="path"></param>
        public void WriteLogs(string path)
        {
            File.WriteAllText(path, Logs.ToString());
        }

        /// <summary>
        /// Log and store one entry
        /// </summary>
        /// <param name="logType"></param>
        /// <param name="message"></param>

        public void LogIt(EnumLogFlags logType, string message)
        {
            if (!IsEnabled)
                return;

            bool isExcluded = false;
            foreach (string expr in excludesList)
            {
                if (Regex.IsMatch(message, expr, RegexOptions.IgnoreCase))
                {
                    isExcluded = true;
                    break;
                }
            }

            LogEntry entry = new LogEntry(logType, message);
            entry.IsExcluded = isExcluded;
            Logs.Add(entry);
        }

    }
}

