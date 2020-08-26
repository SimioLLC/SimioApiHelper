using SimioAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HeadlessLibrary
{
    /// <summary>
    /// Store the Scenarios along with the Scenario Results.
    /// The result is an atomic result with many to each scenario.
    /// </summary>
    public class ExperimentResult
    {
        public List<IScenario> ScenarioList { get; set; }

        public List<IScenarioResult> ResultList { get; set; }

        public ExperimentResult()
        {
            this.ScenarioList = new List<IScenario>();
            this.ResultList = new List<IScenarioResult>();
        }

        public bool AddScenario(IScenario scenario)
        {
            try
            {

            }
            catch (Exception ex)
            {
                throw new ApplicationException($"Cannot add Scenario={scenario.Name}. Err={ex}");
            }
        }
    }
}
