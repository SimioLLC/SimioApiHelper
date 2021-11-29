using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using SimioAPI;

namespace SimEngineConsoleExperimentSaveTable
{
    /// <summary>
    /// Like the ScenarioResult, but with a few additional properties (like ScenarioName)
    /// </summary>
    public class ExperimentResult
    {
        private IScenario Scenario { get; set; }

        private IScenarioResult ScenarioResult { get; set; }

        public string ScenarioName => Scenario.Name;

        public string ObjectType => ScenarioResult.ObjectType;
        public string ObjectName => ScenarioResult.ObjectName;
        public string DataSource => ScenarioResult.DataSource;
        public string StatisticCategory => ScenarioResult.StatisticCategory;
        public string StatisticType => ScenarioResult.StatisticType;
        public string DataItem => ScenarioResult.DataItem;
        public double Average => ScenarioResult.Average;
        public double Minimum => ScenarioResult.Minimum;
        public double Maximum => ScenarioResult.Maximum;
        public double HalfWidth => ScenarioResult.HalfWidth;
        public double StandardDeviation => ScenarioResult.StandardDeviation;

        public ExperimentResult(IScenario scenario, IScenarioResult result)
        {
            Scenario = scenario;
            ScenarioResult = result;
        }
    }

    /// <summary>
    /// Store the Scenarios along with the Scenario Results.
    /// The result is an atomic result with many to each scenario.
    /// </summary>
    public class ExperimentResults
    {

        /// <summary>
        /// All the Result from all the scenarios
        /// </summary>
        public List<ExperimentResult> ResultList { get; set; }

        public ExperimentResults()
        {
            this.ResultList = new List<ExperimentResult>();
        }

        /// <summary>
        /// Add scenario results to our Experiment ResultList
        /// </summary>
        /// <param name="scenario"></param>
        /// <param name="scenarioResults"></param>
        /// <returns></returns>
        public bool AddScenarioResults(IScenario scenario, IScenarioResults scenarioResults)
        {
            if (scenario == null)
                throw new ApplicationException($"Null Scenario argument.");

            try
            {
                foreach (IScenarioResult result in scenarioResults)
                {
                    ExperimentResult er = new ExperimentResult(scenario, result);
                    ResultList.Add(er);
                }
                return true;
            }
            catch (Exception ex)
            {
                throw new ApplicationException($"Cannot add Scenario={scenario.Name}. Err={ex}");
            }
        }

        /// <summary>
        /// Fields are the IScenearioResult fields, plus ScenarioName
        /// Hierarchy: ObjectType, ObjectName, DataSource, Category, DataItem, Statistic
        /// E.g. #1: ModelEntity, Customer, [Population], Content,    NumberInSystem, Average
        /// E.g. #2: Path,        Path1,    [Travelers],  EntryQueue, NumberWaiting, Maximum (Hours)
        /// </summary>
        /// <param name="fieldList"></param>
        /// <param name="outPath"></param>
        /// <param name="explanation"></param>
        /// <returns></returns>
        public bool OutputCsv(string outPath, out string explanation)
        {
            explanation = "";
            try
            {
                string folder = Path.GetDirectoryName(outPath);
                if (!Directory.Exists(Path.GetDirectoryName(outPath)))
                {
                    explanation = $"No such folder={folder}";
                    return false;
                }

                if (ResultList == null || !ResultList.Any())
                {
                    explanation = $"No Experiment Results found.";
                    return false;
                }

                Type erType = typeof(ExperimentResult);
                Dictionary<string, PropertyInfo> piDict = new Dictionary<string, PropertyInfo>();
                foreach (var pi in erType.GetProperties())
                {
                    piDict.Add(pi.Name, pi);
                }

                StringBuilder sbFile = new StringBuilder();
                bool firstRecord = true;
                foreach (var er in ResultList)
                {
                    if (firstRecord)
                    {
                        StringBuilder sbNames = new StringBuilder();
                        foreach (var pi in piDict.Values)
                        {
                            sbNames.Append($"{pi.Name}\t");
                        }
                        sbFile.AppendLine(sbNames.ToString().Trim());
                        firstRecord = false;
                    }

                    // Now get values
                    StringBuilder sbValues = new StringBuilder();
                    foreach (var pi in piDict.Values)
                    {
                        var vv = pi.GetValue(er);
                        switch (pi.PropertyType.Name)
                        {
                            case "String":
                                sbValues.Append($"{vv}\t");
                                break;

                            case "Double":
                                sbValues.Append($"{vv}\t");
                                break;

                            default:
                                sbValues.Append($"{vv}\t");
                                break;
                        }
                    } // foreach property (to get value)
                    sbFile.AppendLine(sbValues.ToString().Trim());
                }

                File.WriteAllText(outPath, sbFile.ToString());

                return true;
            }
            catch (Exception ex)
            {
                throw new ApplicationException($"Path={outPath}. Err={ex}");
            }
        }


    }

}
