using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimioApiHelper
{
    /// <summary>
    /// A watcher group is a collection of entries where all are in the same time interval (ID)
    /// The idea is to simplify the display so they can be grouped
    /// </summary>
    public class WatcherEntryGroup
    {
        /// <summary>
        /// The 'key' for the group
        /// </summary>
        public long Id { get; set; }

        public DateTime Timestamp { get; set;  }

        public List<WatcherEntry> EntryList { get; set; }

        public WatcherEntryGroup(long id, DateTime timestamp)
        {
            Id = id;
            this.Timestamp = timestamp;
            EntryList = new List<WatcherEntry>();
        }

        /// <summary>
        /// Add or Edit a watcher entry.
        /// </summary>
        /// <param name="entry"></param>
        public void AddOrEditWatcherEntry( WatcherChangeTypes changeType, string path)
        {
            WatcherEntry entry = EntryList.SingleOrDefault(rr => rr.FullPath == path);
            if (entry == null)
            {
                entry = new WatcherEntry(Timestamp, path);
                EntryList.Add(entry);
            }

            entry.AddChangeType(changeType);
        }

        /// <summary>
        /// Create a report of lines, where the first line of each group is the time, folder, count
        /// and each line within is the indented file and type of change
        /// </summary>
        /// <returns></returns>
        public string CreateLineReport()
        {
            if (!EntryList.Any())
                return string.Empty;

            StringBuilder sb = new StringBuilder();

            var xx = EntryList.GroupBy(ff => ff.Folder);

            foreach ( IGrouping<string,WatcherEntry> folderGrouping in EntryList.GroupBy(ff => ff.Folder) )
            {
                bool isFirst = true;
                int nn = 0;
                foreach ( WatcherEntry entry in folderGrouping )
                {
                    if ( isFirst )
                    {
                        sb.AppendLine($"{entry.Timestamp:HH:mm:ss} {entry.Folder}");
                        isFirst = false;
                    }

                    sb.AppendLine($"   {++nn:00} [{entry.BuildChangeString()}] {entry.FileName}");
                }
            }
            return sb.ToString();
        }

    }
}
