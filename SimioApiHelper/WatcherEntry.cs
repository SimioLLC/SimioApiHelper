using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimioApiHelper
{
    /// <summary>
    /// An entry is a file, with a list of changes that happened to that file.
    /// 
    /// </summary>
    public class WatcherEntry
    {

        public string Key { get; set; }

        public List<WatcherChangeTypes> ChangeTypeList { get; set; }

        public DateTime Timestamp { get; set; }

        /// <summary>
        /// The time of the last FileWatcher update. 
        /// Used to keep multiple updates from changing the content status.
        /// </summary>
        public DateTime LastUpdateTime { get; set; }

        public WatcherChangeTypes LastWatcherChangeType { get; set; }

        public string FullPath { get; set; }

        public string Folder { get { return Path.GetDirectoryName(FullPath); } }

        public string FileName { get { return Path.GetFileName(FullPath); } } 

        public long FileSize { get; set; }

        /// <summary>
        /// The contents of the file
        /// </summary>
        public string Contents { get; set;  }

        /// <summary>
        /// Set during the Update to indicate contents of the file changed.
        /// </summary>
        public bool HasContentsChanged { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="path"></param>
        public WatcherEntry(DateTime timestamp, string path)
        {
            Timestamp = timestamp;
            ChangeTypeList = new List<WatcherChangeTypes>();

            this.FullPath = path;

            HasContentsChanged = false;
            LastUpdateTime = DateTime.MinValue;
            LastWatcherChangeType = WatcherChangeTypes.Created; // default

        }

        /// <summary>
        /// See if the contents of a file have changed
        /// </summary>
        /// <param name="fullpath"></param>
        /// <returns></returns>
        public void UpdateFile(FileSystemEventArgs fsea)
        {
            try
            {
                if (FileName.StartsWith("b4578"))
                {
                    string xx = "";
                }

                // If it is too soon, then do not process this file.
                if ( this.HasContentsChanged && (LastUpdateTime != DateTime.MinValue) )
                {
                    double deltaSeconds = DateTime.Now.Subtract(this.LastUpdateTime).TotalSeconds;
                    string ss = $"File={FileName} Watch update deltaSeconds={deltaSeconds} ChangeType:this={fsea.ChangeType} last={LastWatcherChangeType}";
                    if (deltaSeconds < 10)
                        return;
                }

                FileInfo fi = new FileInfo(this.FullPath);
                this.HasContentsChanged = false;
                string newContents = "";

                int loopCount = 0;

                do
                {
                    try
                    {
                        newContents = File.ReadAllText(this.FullPath);
                        goto DoneWithReadLoop;
                    }
                    catch (Exception ex)
                    {
                        System.Threading.Thread.Sleep(50);
                    }
                    loopCount++;

                } while (loopCount < 5);

            DoneWithReadLoop:
                if (loopCount >= 5)
                {
                    LogIt($"Reading={FullPath} Could not read file.");
                    return;
                }

                // Initialize?
                if (FileSize == 0 && Contents == null)
                {
                    FileSize = fi.Length;
                    this.Contents = newContents;
                    this.HasContentsChanged = false;
                }
                else if (fi.Length != FileSize) // length changes, so there is definitely a change
                {
                    if (newContents != Contents)
                    {
                        Contents = newContents;
                        HasContentsChanged = true;
                    }
                }
                else // equal size
                {
                    if (newContents != Contents)
                    {
                        Contents = newContents;
                        HasContentsChanged = true;
                    }
                }

                LastWatcherChangeType = fsea.ChangeType;
                LastUpdateTime = DateTime.Now;
            }
            catch (Exception ex)
            {
                LogIt($"UpdateFile:Path={fsea.FullPath} Err={ex}");
                return;
            }

            return;
        }

        /// <summary>
        /// Add a change type.
        /// </summary>
        /// <param name="changeType"></param>
        public void AddChangeType(WatcherChangeTypes changeType)
        {
            if (!ChangeTypeList.Contains(changeType))
                ChangeTypeList.Add(changeType);
        }

        /// <summary>
        /// Look at all the changes in the list and create
        /// a 
        /// </summary>
        /// <returns></returns>
        public string BuildChangeString()
        {
            if (ChangeTypeList != null && ChangeTypeList.Any())
            {
                StringBuilder sb = new StringBuilder();
                foreach (WatcherChangeTypes change in this.ChangeTypeList)
                    sb.Append($"{change},");
                string ss = sb.ToString();
                if (ss.EndsWith(","))
                    ss = ss.Remove(ss.Length - 1, 1);

                return ss;
            }
            else
                return string.Empty;
        }

        public override string ToString()
        {
            return $"{this.FileName} (Size={this.FileSize}, Change={this.Timestamp.ToString("HH:mm:ss")}) Folder={this.Folder}";
        }

        private void LogIt(string msg)
        {
            LoggertonHelpers.Loggerton.Instance.LogIt( LoggertonHelpers.EnumLogFlags.All, $"WatcherEntry:{msg}");
        }
    }
}
