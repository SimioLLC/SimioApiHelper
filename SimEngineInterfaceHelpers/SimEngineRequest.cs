using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace SimEngineInterfaceHelpers
{
    /// <summary>
    /// A SimEngineRequest is a desired Action on a Simio Project
    /// </summary>
    public class SimEngineRequest
    {
        /// <summary>
        /// A sortable key, where time is the time the request is created
        /// down to a millisecond.
        /// </summary>
        public Guid ID { get { return new Guid(); } }

        /// <summary>
        /// When the request was made. 
        /// </summary>
        public DateTimeOffset RequestTime { get; set; }

        /// <summary>
        /// What is being requested
        /// </summary>
        public string Action { get; set; }
        /// <summary>
        /// Where the Simio project is located
        /// </summary>
        public string ProjectFilename { get; set; }

        /// <summary>
        /// Case-insentive arguments that help define the Action.
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
        /// Results of the run, added by the SimEngine Controller/Runner.
        /// </summary>
        public string RunResults { get; set; }

        /// <summary>
        /// Where the request is located.
        /// </summary>
        public string RequestPath { get; set; }

        /// <summary>
        /// If the run fails, this string contains the reason(s)
        /// </summary>
        public string RunErrors { get; set; }

        public SimEngineRequest(string projectAction, string projectFilename, List<RequestArgument> argList)
        {
            RequestTime = DateTimeOffset.Now;
            Action = projectAction;
            ProjectFilename = projectFilename;
            ActionArguments = argList;
            RequestPath = "";

            RunNotes = "";
            RunResults = "";
            RunErrors = "";
        }

        /// <summary>
        /// Parameterless constructor for deserialization.
        /// </summary>
        public SimEngineRequest()
        {
            RequestTime = new DateTimeOffset();

        }

        public override string ToString()
        {
            return $"Action={Action} ProjectFile={ProjectFilename} Notes={this.RunNotes} Arguments(#={ActionArguments.Count()})";
        }

    }

}
