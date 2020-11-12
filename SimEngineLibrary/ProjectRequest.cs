using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimEngineLibrary
{
    public class ProjectRequest
    {
        public DateTimeOffset RequestTime { get; set; }

        /// <summary>
        /// Where the Simio project is located
        /// </summary>
        public string ProjectPath { get; set; }

        /// <summary>
        /// What to do with the model
        /// </summary>
        public string ProjectAction { get; set; }

        /// <summary>
        /// Arguments that accompany the action.
        /// </summary>
        public string ActionArguments { get; set; }

        /// <summary>
        /// Where to send the completion information
        /// </summary>
        public string NotifyAddress { get; set; }

        public string RunNotes { get; set; }

        /// <summary>
        /// Where the request is located.
        /// </summary>
        public string RequestPath { get; set; }

        public ProjectRequest( string projectPath, string projectAction, string actionArguments, string notifyAddress)
        {
            RequestTime = new DateTimeOffset();
            ProjectPath = projectPath;
            ProjectAction = projectAction;
            ActionArguments = actionArguments;
            NotifyAddress = notifyAddress;
            RequestPath = "";

            RunNotes = "";
        }

        /// <summary>
        /// Parameterless constructor for deserialization.
        /// </summary>
        public ProjectRequest()
        {

        }

    }
}
