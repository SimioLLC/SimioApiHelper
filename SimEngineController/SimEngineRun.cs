using SimEngineInterfaceHelpers;
using SimEngineLibrary;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimEngineController
{
    public class SimEngineRun
    {

        /// <summary>
        /// The currently loaded project.
        /// </summary>
        public ISimioProject CurrentProject { get; set; }

        public Loggerton MyLogger { get; set; }

        /// <summary>
        /// The currently loaded model
        /// </summary>
        public IModel CurrentModel { get; set; }

        /// <summary>
        /// The currently loaded plan.
        /// </summary>
        public IPlan CurrentPlan { get; set; }

        /// <summary>
        /// The currently loaded experiment
        /// </summary>
        public IExperiment CurrentExperiment { get; set; }

        /// <summary>
        /// List of arguments associated with desired action.
        /// E.g. if doing an Experiment, there needs to be Model and Experiment Names
        /// </summary>
        public List<RequestArgument> ActionArgumentList { get; set; }


        /// <summary>
        /// Errors generated during the project load
        /// </summary>
        public List<string> ProjectLoadErrorList { get; set; }

        /// <summary>
        /// Errors generated during the Model load
        /// </summary>
        public List<string> ModelLoadErrorList { get; set; }

        /// <summary>
        /// Errors generated during the Project save
        /// </summary>
        public List<string> ProjectSaveErrorList { get; set; }


        public event EventHandler<ExperimentReplicationEndedEventArgs> ExperimentReplicationEnded;


        public event EventHandler<RequestEndedEventArgs> RequestCompleted;



        protected virtual void OnExperimentReplicationCompleted(ExperimentReplicationEndedEventArgs e)
        {
            ExperimentReplicationEnded?.Invoke(this, e);
        }

        /// <summary>
        /// Process a single request.
        /// Combintation of the context and request are used to get a single request. 
        /// For example, the context's ProjectsFolderPath and the request's ProjectFilename are used to get a full path to the project.
        /// </summary>
        /// <param name="explanation"></param>
        /// <returns></returns>
        public bool Start(SimEngineRunContext context, SimEngineRequest request, out string explanation)
        {
            explanation = "";

            if (request == null)
            {
                explanation = $"Null request. Nothing to do.";
                return false;
            }

            try
            {

                if (request != null)
                {
                    // Now we have the request. Figure out what to do.
                    Logit($"Processing request. Action={request.Action} Project={request.ProjectFilename}");


                    // Run the project
                    string projectFullPath = Path.Combine(context.ProjectFolderPath, request.ProjectFilename);
                    if (!File.Exists(projectFullPath))
                    {
                        // Move to failure
                        explanation = $"No ProjectFile found at={projectFullPath}";
                        goto FinishUp;
                    }

                    //// Move to "running" folder.
                    //string runningPath = Path.Combine(runningFolder, Path.GetFileName(request.RequestPath));
                    //if (File.Exists(runningPath))
                    //    File.Delete(runningPath);
                    //File.Move(request.RequestPath, runningPath);

                    List<string> warningList = new List<string>();

                    switch (request.Action.ToUpper())
                    {
                        case "PLAN":
                            {
                                // Run the project
                                if (!SimEngineHelpers.RunProjectPlan(extensionsPath, projectFullPath, request.ActionArguments, warningList, out explanation))
                                {
                                    request.RunResults = $"Error with Request={request.ID} Project={request.ProjectFilename} Error={explanation}.";
                                    // Move to Failure
                                }
                                else
                                {
                                    request.RunResults = $"Success with Request={request.ID} Project={request.ProjectFilename}";

                                    // Move to Success
                                }

                            }
                            break;

                        case "EXPERIMENT":
                            {

                                if (!SimEngineHelpers.RunProjectExperiment(context.ExtensionsFolderPath, context.p, warningList, out explanation))
                                {
                                    request.RunResults = $"Error with Request={request.ID} Project={request.ProjectFilename} Error={explanation}.";
                                    // Move to Failure
                                }
                                else
                                {
                                    request.RunResults = $"Success with Request={request.ID} Project={request.ProjectFilename}";
                                    // Move to Success
                                }
                            }
                            break;

                        // Alter congifiguration, such as location of project
                        case "CONFIGURATION":
                            {
                                // Todo: Alter configurations at run-time
                            }
                            break;

                        case "STATUS":
                            {
                                //Todo: Given the ID, return a status
                            }
                            break;

                        default:
                            Logit($"Unknown ProjectAction={request.Action}");
                            break;

                    } // switch


                FinishUp:

                    string source = request.RequestPath;
                    string fn = Path.GetFileName(source);

                    if (string.IsNullOrEmpty(explanation))
                    {
                        // throw 'success'
                        //File.Move(source, Path.Combine(successFolder, fn));
                    }
                    else
                    {
                        // throw 'failure'
                        //File.Move(source, Path.Combine(failureFolder, fn));
                    }

                }
                return true;
            }
            catch (Exception ex)
            {

                explanation = $"Request Failure. Err={ex}";
                return false;
            }

        }

        private void Logit(string msg)
        {
            SimEngineHelpers.LogIt($"SimEngineController::{msg}");
        }


        /// <summary>
        /// Process a single request
        /// </summary>
        /// <param name="explanation"></param>
        /// <returns></returns>
        public static void HandleOneRequest(out string explanation)
        {
            try
            {

                // Requests are queued by time and ID.
                SimEngineRequest request = SimEngineRequestHelper.GetNextRequest(requestsFolder, out explanation);
                if (request != null)
                {
                    // Now we have the request. Figure out what to do.
                    Logit($"Processing request. Action={request.Action} Project={request.ProjectFilename}");


                    // Run the project
                    string projectFullPath = Path.Combine(projectsFolder, request.ProjectFilename);
                    if (!File.Exists(projectFullPath))
                    {
                        // Move to failure
                        explanation = $"No ProjectFile found at={projectFullPath}";
                        goto FinishUp;
                    }

                    // Move to "running" folder.
                    string runningPath = Path.Combine(runningFolder, Path.GetFileName(request.RequestPath));
                    if (File.Exists(runningPath))
                        File.Delete(runningPath);
                    File.Move(request.RequestPath, runningPath);

                    List<string> warningList = new List<string>();

                    switch (request.Action.ToUpper())
                    {
                        case "PLAN":
                            {
                                // Run the project
                                if (!SimEngineHelpers.RunProjectPlan(context, out explanation))
                                {
                                    request.RunResults = $"Error with Request={request.ID} Project={request.ProjectFilename} Error={explanation}.";
                                    // Move to Failure
                                }
                                else
                                {
                                    request.RunResults = $"Success with Request={request.ID} Project={request.ProjectFilename}";

                                    // Move to Success
                                }

                            }
                            break;

                        case "EXPERIMENT":
                            {
                                SimEngineRunContext context = new SimEngineContext(extensionsPath, projectFullPath, request.ActionArguments);


                                if (!SimEngineHelpers.RunProjectExperiment(extensionsPath, projectFullPath, request.ActionArguments, warningList, out explanation))
                                {
                                    request.RunResults = $"Error with Request={request.ID} Project={request.ProjectFilename} Error={explanation}.";
                                    // Move to Failure
                                }
                                else
                                {
                                    request.RunResults = $"Success with Request={request.ID} Project={request.ProjectFilename}";
                                    // Move to Success
                                }
                            }
                            break;

                        // Alter congifiguration, such as location of project
                        case "CONFIGURATION":
                            {
                                // Todo: Alter configurations at run-time
                            }
                            break;

                        case "STATUS":
                            {
                                //Todo: Given the ID, return a status
                            }
                            break;

                        default:
                            Logit($"Unknown ProjectAction={request.Action}");
                            break;

                    } // switch


                FinishUp:

                    string source = request.RequestPath;
                    string fn = Path.GetFileName(source);

                    if (string.IsNullOrEmpty(explanation))
                    {
                        File.Move(source, Path.Combine(successFolder, fn));
                    }
                    else
                    {
                        File.Move(source, Path.Combine(failureFolder, fn));
                    }

                }
            }
            catch (Exception ex)
            {

                throw;
            }

        } // method

        /// <summary>
        /// Load the project. 
        /// Load all models, experiments, ...
        /// If there are warnings, they are placed in LoadWarningsList.
        /// </summary>
        /// <param name="projectFullPath"></param>
        /// <param name="explanation"></param>
        /// <returns></returns>
        public bool LoadProject(string projectName, out string explanation)
        {
            explanation = "";
            string marker = "Checking extension and project paths.";
            string projectFullPath = "";

            CurrentProject = null;

            try
            {

                projectFullPath = Path.Combine(ProjectsFolderPath, projectName);



                marker = $"Setting extensions path={ExtensionsFolderPath}";
                SimioProjectFactory.SetExtensionsPath(ExtensionsFolderPath); // No harm in doing it again.

                // Open project file.
                marker = $"Loading Project={projectFullPath}.";
                CurrentProject = SimioProjectFactory.LoadProject(projectFullPath, out string[] warnings);

                ProjectLoadErrorList = null;
                if (warnings.Length > 0)
                {
                    ProjectLoadErrorList = new List<string>();
                    foreach (string warning in warnings)
                    {
                        ProjectLoadErrorList.Add(warning);
                    }
                }
                return true;

            }
            catch (Exception ex)
            {
                explanation = $"Failed to LoadProject={projectFullPath} Err={ex.Message}";
                return false;
            }

        }

        /// <summary>
        /// Load the named model from the given project and assigns it to our CurrentModel.
        /// Returns true of the model loads, or false and an error list if it doesn't
        /// /// </summary>
        /// <param name="modelName"></param>
        public bool LoadModel(string modelName, out string explanation)
        {
            explanation = "";
            string marker = "Begin.";

            if (CurrentProject == null)
            {
                explanation = $"No Current Project to load model from";
                return false;
            }

            try
            {
                marker = $"Loading Model={modelName}";

                // Get the model from within the project
                CurrentModel = CurrentProject.Models[modelName];
                if (CurrentModel != null)
                {
                    if (CurrentModel.Errors.Count > 0)
                    {
                        int errorCount = 0;
                        // Create a string from model errors
                        StringBuilder sb = new StringBuilder();
                        foreach (IError err in CurrentModel.Errors)
                        {
                            sb.AppendLine($" {++errorCount}. Error={err.ErrorText} Object={err.ObjectName} Type={err.ObjectType} Property: Name={err.PropertyName} Value={err.PropertyValue}");
                        }
                        explanation = $"Model={modelName} LoadErrors:{sb}";
                        return false;
                    }
                }
                else // model is null
                {
                    explanation = $"Model={modelName} could not be loaded from Project={CurrentProject.Name}";
                    return false;
                }


                return true; ;
            }
            catch (Exception ex)
            {
                explanation = $"Cannot load Model={modelName} Marker={marker} Err={ex.Message}";
                return false;
            }
        }

        /// <summary>
        /// Load the experiment from our CurrentModel.
        /// Returns true of the model loads, or false and an error list if it doesn't
        /// /// </summary>
        /// <param name="modelName"></param>
        public bool LoadExperiment(string experimentName, out string explanation)
        {
            explanation = "";
            string marker = "Begin.";

            if (CurrentModel == null)
            {
                explanation = $"No CurrentModel to load Experiment from";
                return false;
            }

            try
            {
                marker = $"Loading Experiment={experimentName}";

                // Get the model from within the project
                CurrentExperiment = CurrentModel.Experiments[experimentName];
                if (CurrentExperiment != null)
                {
                    if (CurrentExperiment.Scenarios.Count == 0)
                    {
                        explanation = $"Experiment={experimentName} has no Scenarios.";
                        return false;
                    }

                    // Create a string from scenarios (debug)
                    StringBuilder sb = new StringBuilder();
                    foreach (IScenario scenario in CurrentExperiment.Scenarios)
                    {
                        sb.AppendLine($" Scenario={scenario.Name}. ControlValues={scenario.ControlValues} ");
                    }
                    string xx = sb.ToString();
                    return true;
                }
                else // experiment is null
                {
                    explanation = $"Experiment={experimentName} could not be loaded from Model={CurrentModel.Name}";
                    return false;
                }

            }
            catch (Exception ex)
            {
                explanation = $"Cannot load Model={experimentName} Err={ex.Message}";
                return false;
            }
        }

        /// <summary>
        /// Assumes a Model is loaded, this initiates a RunPlan.
        /// The running of a Plan creates about dozen Simio logs, such as ResourceCapacityLog.
        /// </summary>
        /// <param name="options"></param>
        /// <param name="explanation"></param>
        /// <returns></returns>
        public bool LoadPlan(out string explanation)
        {
            explanation = "";
            string marker = "Begin";

            try
            {

                if (CurrentModel == null)
                {
                    explanation = $"Cannot run plan. No Model is currently loaded";
                    return false;
                }

                // Check for Plan
                if (CurrentModel.Plan == null)
                {
                    explanation = $"Model={CurrentModel.Name} has no Plan.";
                    return false;
                }

                CurrentPlan = CurrentModel.Plan;
                return true;
            }
            catch (Exception ex)
            {
                explanation = $"Project={CurrentProject.Name} Model={CurrentModel.Name} Marker={marker} Err={ex.Message}";
                return false;
            }

        }


        /// <summary>
        /// Assumes a Model is loaded, this initiates a RunPlan.
        /// The running of a Plan creates about dozen Simio logs, such as ResourceCapacityLog.
        /// </summary>
        /// <param name="options"></param>
        /// <param name="explanation"></param>
        /// <returns></returns>
        public bool RunModelPlan(out string explanation)
        {
            explanation = "";
            string marker = "Begin";

            if (CurrentPlan == null)
            {
                explanation = $"Cannot run plan, as none are loaded from Project={CurrentProject?.Name} Model={CurrentModel?.Name}";
                return false;
            }


            try
            {
                marker = "Starting Plan (model.Plan.RunPlan)";
                CurrentModel.Plan.RunPlan(null);

                bool canRunPlan = SimEngineHelpers.RunProjectPlan(this.ExtensionsFolderPath, this.ProjectsFolderPath, ActionArgumentList, this.ProjectLoadErrorList, out string explantion);

                RequestArgument saveArg = ActionArgumentList.SingleOrDefault(rr => rr.Key == "Save");
                if (saveArg != null)
                {
                    if (!SimEngineHelpers.SaveProject(CurrentProject, saveArg.Value, this.ProjectLoadErrorList, out explanation))
                        return false;
                }


                marker = "End";

                return true;
            }
            catch (Exception ex)
            {
                explanation = $"Project={CurrentProject.Name} Model={CurrentModel.Name} Marker={marker} Err={ex.Message}";
                return false;
            }

        }
        /// <summary>
        /// Assumes a Model is loaded, this initiates a RiskAnalysis run.
        /// The running of a RiskAnalysis creates about a dozen Simio logs, such as ResourceCapacityLog.
        /// </summary>
        /// <param name="explanation"></param>
        /// <returns></returns>
        public bool RunModelRiskAnalysis(out string explanation)
        {
            explanation = "";
            string marker = "Begin";

            if (CurrentProject == null)
            {
                explanation = $"Cannot run plan. No Project is currently loaded";
                return false;
            }

            if (CurrentModel == null)
            {
                explanation = $"Cannot run plan. No Model is currently loaded";
                return false;
            }

            // Check for Plan
            if (CurrentModel.Plan == null)
            {
                explanation = $"Model={CurrentModel.Name} has no Plan.";
                return false;
            }

            try
            {
                marker = "Starting RiskAnalysis (model.Plan.RunRiskAnalysis)";
                CurrentModel.Plan.RunRiskAnalysis();

                marker = "End";

                return true;
            }
            catch (Exception ex)
            {
                explanation = $"Project={CurrentProject.Name} Model={CurrentModel.Name} Marker={marker} Err={ex.Message}";
                return false;
            }

        }

        /// <summary>
        /// Assumes an Experiment is loaded, this initiates an Experiment run.
        /// This can be very long-running.
        /// </summary>
        /// <param name="explanation"></param>
        /// <returns></returns>
        public bool RunModelExperiment(string resultFilepath, out string explanation)
        {
            explanation = "";
            string marker = "Begin";

            ExperimentResults experimentResults = new ExperimentResults();

            if (CurrentProject == null)
            {
                explanation = $"Cannot run plan. No Project is currently loaded";
                return false;
            }

            if (CurrentModel == null)
            {
                explanation = $"Cannot run plan. No Model is currently loaded";
                return false;
            }

            // Check for Experiment
            if (CurrentExperiment == null)
            {
                explanation = $"Model={CurrentModel.Name} has no Experiment selected.";
                return false;
            }

            try
            {
                // ReplicationEndedEventArgs
                CurrentExperiment.ReplicationEnded += (s, e) =>
                {
                    int repNumber = e.ReplicationNumber;
                    double runTime = e.ActualRuntimeInSeconds;

                    LogIt($"ReplicationEnded: Scenario={e.Scenario.Name}, Rep#={e.ReplicationNumber}, EndTime={e.EndingSimulationTime}, "
                        + $" State={e.ReplicationEndedState}, RunSeconds={e.ActualRuntimeInSeconds} Error={e.ErrorMessage}, "
                        );

                    ExperimentReplicationEndedEventArgs args = new ExperimentReplicationEndedEventArgs();
                    args.ModelName = this.CurrentModel.Name;
                    args.ReplicationEndedArgs = e;

                    OnExperimentReplicationCompleted(args);

                };

                // ScenarioEndedEventArgs
                CurrentExperiment.ScenarioEnded += (s, e) =>
                {
                    LogIt($"ScenarioEnded: Results:");

                    foreach (var result in e.Results)
                    {
                            // Log the results to get a feel for what is being returned.
                            LogIt($"ScenarioEnded: ObjType='{result.ObjectType}', ObjName='{result.ObjectName}', DS='{result.DataSource}', "
                            + $"Cat='{result.StatisticCategory}', DataItem='{result.DataItem}', Stat='{result.StatisticCategory}', "
                            + $"Avg='{result.Average:0.00}', Min='{result.Minimum:f0.00}', Max='{result.Maximum:f0.00}"
                            );

                        experimentResults.AddScenarioResults(e.Scenario, e.Results);
                    }

                    ExperimentScenarioEndedEventArgs args = new ExperimentScenarioEndedEventArgs();
                    args.ModelName = this.CurrentModel.Name;
                    args.ScenarioEndedArgs = e;

                    OnExperimentScenarioCompleted(args);

                };

                // RunCompletedEventArgs
                CurrentExperiment.RunCompleted += (s, e) =>
                {

                    LogIt($"RunCompleted: TotalRun={e.TotalRunTime.TotalSeconds}, Error={e.Error}, Cancelled?={e.Cancelled}, "
                        + $" Message={e.Message} UserState={e.UserState} ");

                    ExperimentRunCompletedEventArgs args = new ExperimentRunCompletedEventArgs();
                    args.ModelName = this.CurrentModel.Name;
                    args.RunCompletedArgs = e;
                    args.RunResults = experimentResults;

                    OnExperimentRunCompleted(args);

                };

                // RunProgressChangedEventArgs
                CurrentExperiment.RunProgressChanged += (s, e) =>
                {
                    LogIt($"ProgressChanged:");
                    string ss = e.ToString();


                };


                //--------------- Run the Experiment -----------------------------
                // Events will be thrown when replications start and end,
                // so you would have to handle those elsewhere.
                marker = "Starting Experiment (Experiment.Run)";
                CurrentExperiment.Reset();

                CurrentExperiment.Run();



                if (string.IsNullOrEmpty(resultFilepath))
                {
                    LogIt($"Info: No result Filepath specified. Results not written.");
                }
                else
                {
                    string folder = Path.GetDirectoryName(resultFilepath);
                    if (Directory.Exists(folder))
                    {

                        StringBuilder sb = new StringBuilder();
                        // Build a tab-delimited file

                        if (!experimentResults.OutputCsv(resultFilepath, out explanation))
                        {
                            explanation = $"Cannot Write CSV results to={resultFilepath}";
                            return false;
                        }
                    }
                }

                marker = "End";
                return true;
            }
            catch (Exception ex)
            {
                explanation = $"Project={CurrentProject.Name} Model={CurrentModel.Name} Marker={marker} Err={ex.Message} InnerEx={ex.InnerException}";
                return false;
            }

        }




        /// <summary>
        /// Save the given project to the 'savePath'.
        /// </summary>
        /// <param name="project"></param>
        /// <param name="savePath"></param>
        public bool SaveProject(string savePath, out string explanation)
        {
            explanation = "";
            string marker = "Begin.";
            string[] warnings;

            // If project not loaded, return error
            if (CurrentProject == null)
            {
                explanation = $"No project is loaded (Project is null).";
                return false;
            }

            string folderPath = Path.GetDirectoryName(savePath);

            if (Directory.Exists(folderPath) == false)
            {
                explanation = $"FolderPath={folderPath} not found.";
                return false;
            }

            try
            {

                // Save project file.
                marker = $"Saving Project={CurrentProject.Name} to {savePath}.";
                if (!SimioProjectFactory.SaveProject(CurrentProject, savePath, out warnings))
                    explanation = $"SaveProject failed.";

                marker = $"Saved Project={savePath} with {warnings.Count()} warnings.";
                ProjectSaveErrorList = new List<string>();
                int ii = 1;
                foreach (string warning in warnings)
                {
                    ProjectSaveErrorList.Add($"Warning: {ii++}{warning}");
                }

                return true;
            }
            catch (Exception ex)
            {
                explanation = $"Cannot Save Simio Project={CurrentProject.Name} to {savePath} Err={ex.Message}";
                return false;
            }
        }

    }

}

