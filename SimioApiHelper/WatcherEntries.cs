using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimioApiHelper
{
    /// <summary>
    /// All the Filewatcher groups.
    /// A group is a list of files that have the same folder in the same time interval.
    /// </summary>
    public class WatcherEntries
    {
        /// <summary>
        /// Dictionary of groups, keyed by ID, which is a time interval number.
        /// </summary>
        public Dictionary<long, WatcherEntryGroup> GroupDict { get; set; }

        private DateTime BeginTime { get; set; }

        private int IntervalSeconds { get; set; }

        private WatcherEntryGroup CurrentGroup { get; set; }

        private DateTime LastChange { get; set; }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="intervalSeconds"></param>
        public WatcherEntries(int intervalSeconds)
        {
            IntervalSeconds = intervalSeconds;

            this.Clear();

            long id = ComputeGroupId(DateTime.Now);

        }

        /// <summary>
        /// Computes a unique id for each group.
        /// </summary>
        /// <param name="timestamp"></param>
        /// <returns></returns>
        public long ComputeGroupId(DateTime timestamp)
        {
            double secs = timestamp.Subtract(BeginTime).TotalSeconds;

            long id = (int) (secs / IntervalSeconds);
            return id;
        }
        
        public void AddOrEditWatcherEntry( DateTime timestamp, WatcherChangeTypes changeType, string path )
        {
            long id = ComputeGroupId( timestamp );

            if ( CurrentGroup == null || id != CurrentGroup.Id)
            {
                CurrentGroup = new WatcherEntryGroup(id, timestamp);
                if (!this.GroupDict.ContainsKey(id))
                    GroupDict.Add(id, CurrentGroup);
            }

            CurrentGroup.AddOrEditWatcherEntry( changeType, path );

        }

        /// <summary>
        /// Clear the entries.
        /// </summary>
        public void Clear()
        {
            GroupDict = new Dictionary<long, WatcherEntryGroup>();
            CurrentGroup = null;
            BeginTime = DateTime.Now;

        }

    }
}
