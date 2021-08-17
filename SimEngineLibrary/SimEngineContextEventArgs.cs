using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SimEngineInterfaceHelpers;
using SimioAPI;

namespace SimEngineLibrary
{
    public class ExperimentReplicationEndedEventArgs
    {
        public string ModelName { get; set; }

        public ReplicationEndedEventArgs ReplicationEndedArgs { get; set; }

    }
    public class RequestEndedEventArgs
    {
        public SimEngineRequest Request { get; set; }

    }

    public class ExperimentScenarioEndedEventArgs
    {
        public string ModelName { get; set; }

        public ScenarioEndedEventArgs ScenarioEndedArgs { get; set; }

    }

    public class ExperimentRunCompletedEventArgs
    {
        public string ModelName { get; set; }

        public RunCompletedEventArgs RunCompletedArgs { get; set; }

        public ExperimentResults RunResults { get; set; }

    }




}
