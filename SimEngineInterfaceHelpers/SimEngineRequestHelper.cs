using SimEngineInterfaceHelpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SimEngineInterfaceHelpers
{
    /// <summary>
    /// Helper to place a request in a queue in persistent storage
    /// The top folder is the "queue location" and it has subfolders of
    /// (1) running (2) success, and (3) failure
    /// </summary>
    public static class SimEngineRequestHelper
    {

        /// <summary>
        /// The request repository is a folder, and the files within are in time-order, so the smallest is oldest,
        /// which is the one we want (FIFO). This will grabe this and deserialize it.
        /// </summary>
        /// <param name="queueLocation"></param>
        /// <param name="explanation"></param>
        /// <returns></returns>
        public static SimEngineRequest GetNextRequest(string queueLocation, out string explanation)
        {
            explanation = "";
            string marker = "Begin.";
            SimEngineRequest request = null;
            try
            {
                marker = $"Getting files from {queueLocation}";
                List<string> fileList = Directory.GetFiles(queueLocation, "*.json").ToList();
                if (!fileList.Any())
                    return null;

                string path = fileList.First();

                // try 5 times... to handle the rare conflicts between producer and consumer.
                int attempts = 0;

                while (attempts < 5)
                {
                    marker = $"Deserializing from={path}. Attempts={attempts}";
                    if (!JsonHelpers.DeserializeFromFile<SimEngineRequest>(path, out request, out explanation))
                    {
                        if (++attempts >= 5)
                        {
                            return null;
                        }
                        System.Threading.Thread.Sleep(100);
                    }
                    else
                    {
                        request.RequestPath = path;
                        marker = $"Set RequestPath to={request.RequestPath}";

                        goto DoneWithAttempts;
                    }
                } // while not successful

            DoneWithAttempts:
                return request;
            }
            catch (Exception ex)
            {
                explanation = $"Location={queueLocation}. Marker={marker}. Err={ex.Message}";
                return null;
            }
        }

        public static string GetRunningFolder(string queueLocation) => Path.Combine(queueLocation, "Running");
        public static string GetSuccessFolder(string queueLocation) => Path.Combine(queueLocation, "Success");
        public static string GetFailureFolder(string queueLocation) => Path.Combine(queueLocation, "Failure");

        /// <summary>
        /// Build a legitimate file path using the current time (DateTimeOffset)
        /// and the Project name, with a .json extension.
        /// This guarantess that the default ordering from GetFiles will have
        /// the oldest files first.
        /// Example: 2020-11-29 13-58-34-3922-DiscretPartsProduction.json
        /// </summary>
        /// <param name="queueLocation"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public static string BuildRequestPath(string queueLocation, SimEngineRequest request)
        {
            string timestamp = DateTimeOffset.Now.ToString("yyyy-MM-dd HH:mm.ss.ffff")
                .Replace(":", "-")
                .Replace(".", "-");
            string projectName = Path.GetFileNameWithoutExtension(request.ProjectFilename);

            string fullpath = Path.Combine(queueLocation, $"{timestamp}-{projectName}.json");

            return fullpath;
        }

        /// <summary>
        /// The repository is a folder, and these are in time-order, so the smallest is oldest,
        /// which is the one we want (FIFO). This will grabe this and deserialize it.
        /// </summary>
        /// <param name="queueLocation"></param>
        /// <param name="explanation"></param>
        /// <returns></returns>
        public static bool PutRequest(string queueLocation, SimEngineRequest request, out string explanation)
        {
            explanation = "";
            try
            {
                if (!Directory.Exists(queueLocation))
                {
                    explanation = $"Path={queueLocation} does not exist.";
                    return false;
                }
                if (request == null)
                {
                    explanation = $"request cannot be null.";
                    return false;
                }

                string path = BuildRequestPath(queueLocation, request);

                if (!JsonHelpers.SerializeToFile<SimEngineRequest>(path, request, false, out explanation))
                    return false;

                return true;

            }
            catch (Exception ex)
            {
                explanation = $"Location={queueLocation}. Err={ex.Message}";
                return false;
            }
        }

    }



}
