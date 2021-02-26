using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimioEngineHelpers
{
    /// <summary>
    /// A SimEngineRequest is a desired Action on a Simio Project
    /// </summary>
    public class SimEngineRequest
    {
        public DateTimeOffset RequestTime { get; set; }

        /// <summary>
        /// Where the Simio project is located
        /// </summary>
        public string ProjectFilename { get; set; }

        /// <summary>
        /// What is being requested
        /// </summary>
        public string Action { get; set; }

        /// <summary>
        /// Arguments that accompany the action.
        /// </summary>
        public List<RequestArgument> ActionArguments { get; set; }

        /// <summary>
        /// Where to send the completion information
        /// </summary>
        public string NotifyAddress { get; set; }

        /// <summary>
        /// Freeform information included with the request
        /// </summary>
        public string RunNotes { get; set; }

        /// <summary>
        /// Results of the run, added by the SimEngine Runner.
        /// </summary>
        public string RunResults { get; set; }

        /// <summary>
        /// Where the request is located.
        /// </summary>
        public string RequestPath { get; set; }

        public SimEngineRequest(string projectFilename, string projectAction, List<RequestArgument> argList)
        {
            this.RequestTime = DateTimeOffset.Now;
            this.ProjectFilename = projectFilename;
            this.Action = projectAction;
            this.ActionArguments = argList;
            this.RequestPath = "";

            this.RunNotes = "";
            this.RunResults = "";
        }

        /// <summary>
        /// Parameterless constructor for deserialization.
        /// </summary>
        public SimEngineRequest()
        {
            this.RequestTime = new DateTimeOffset();

        }

    }

    /// <summary>
    /// A keyvalue pair that holds a request argument.
    /// </summary>
    public class RequestArgument
    {
        public string Key { get; set; }
        public string Value { get; set; }

        public RequestArgument(string key, string value)
        {
            Key = key.Trim().ToLower();
            Value = value.Trim();
        }
    }
}
