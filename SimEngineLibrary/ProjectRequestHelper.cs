using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace SimEngineLibrary
{
    /// <summary>
    /// Helper to place a request in a queue in persistent storage
    /// </summary>
    public static class ProjectRequestHelper
    {

        /// <summary>
        /// The repositry is a folder, and these are in time-order, so the smallest is oldest,
        /// which is the one we want (FIFO). This will grabe this and deserialize it.
        /// </summary>
        /// <param name="queueLocation"></param>
        /// <param name="explanation"></param>
        /// <returns></returns>
        public static ProjectRequest GetNextRequest(string queueLocation, out string explanation)
        {
            explanation = "";
            ProjectRequest request = null;
            try
            {
                List<string> fileList = Directory.GetFiles(queueLocation, "*.json").ToList();
                if (!fileList.Any())
                    return null;

                string path = fileList.First();

                // try 5 times... to handle the rare conflicts between producer and consumer.
                int attempts = 0;

                while (attempts < 5)
                {
                    if (!JsonHelpers.DeserializeFromFile<ProjectRequest>(path, out request, out explanation))
                    {
                        if ( ++attempts >= 5)
                        {
                            return null;
                        }
                        System.Threading.Thread.Sleep(100);
                    }
                    else
                    {
                        request.RequestPath = path;
                        goto DoneWithAttempts;
                    }
                } // while not successful

            DoneWithAttempts:
                return request;
            }
            catch (Exception ex)
            {
                explanation = $"Location={queueLocation}. Err={ex.Message}";
                return null;
            }
        }

        /// <summary>
        /// Build a legitimate file path using the current time (DateTimeOffset)
        /// and the Project name, with a .json extension.
        /// This guarantess that the default ordering from GetFiles will have
        /// the oldest files first.
        /// </summary>
        /// <param name="queueLocation"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public static string BuildRequestPath(string queueLocation, ProjectRequest request)
        {
            string timestamp = DateTimeOffset.Now.ToString("yyyy-MM-dd HH:mm.ss.ffff")
                .Replace(":", "-")
                .Replace(".", "-");
            string projectName = Path.GetFileNameWithoutExtension(request.ProjectPath);

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
        public static bool PutRequest(string queueLocation, ProjectRequest request,  out string explanation)
        {
            explanation = "";
            try
            {
                if ( !Directory.Exists(queueLocation) )
                {
                    explanation = $"Path={queueLocation} does not exist.";
                    return false;
                }
                if ( request == null )
                {
                    explanation = $"request cannot be null.";
                    return false;
                }

                string path = BuildRequestPath(queueLocation, request);

                if (!JsonHelpers.SerializeToFile<ProjectRequest>(path, request, false, out explanation))
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
