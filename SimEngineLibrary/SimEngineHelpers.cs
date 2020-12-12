using LoggertonHelpers;
using SimioAPI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SimEngineLibrary
{
    /// <summary>
    /// Helpers for running the Simio Model in a non-UI aka "headless" mode.
    /// </summary>
    public static class SimEngineHelpers
    {

        
        /// <summary>
        /// Run an experiment. The experiment is Reset prior to run.
        /// </summary>
        /// <param name="projectPathAndFile"></param>
        /// <param name="experimentName"></param>
        /// <param name="saveModelAfterRun"></param>
        /// <param name="explanation"></param>
        /// <returns></returns>
        public static bool RunModelExperiment(string extensionsPath, string sourceProjectPath, string saveProjectPath,
            string modelName, string experimentName, 
            out string explanation)
        {
            string contextInfo = $"ExtensionsPath={extensionsPath}, ProjectPath={sourceProjectPath}:";
            string marker = "Begin.";
            try
            {
                // Set an extensions path to where we can locate User Extensions, etc.
                if (string.IsNullOrEmpty(extensionsPath))
                    extensionsPath = System.AppDomain.CurrentDomain.BaseDirectory;

                marker = $"{contextInfo}. Setting Extensions Path...";
                LogIt($"Info: {marker}");
                SimioProjectFactory.SetExtensionsPath(extensionsPath);

                marker = $"{contextInfo}. Loading Project...";
                ISimioProject project = LoadProject(extensionsPath, sourceProjectPath, out explanation);
                if (project == null)
                    return false;

                marker = $"{contextInfo}. Running Experiment...";
                if ( RunModelExperiment(project, saveProjectPath, modelName, experimentName, out explanation))
                    return true;
                else 
                    return false;
            }
            catch (Exception ex)
            {
                explanation = $"Marker={marker} Err={ex.Message}";
                return false;
            }
        }

        /// <summary>
        /// Run an experiment. The experiment is Reset prior to run.
        /// If the saveProjectPath is present and exists, then the project will be
        /// saved to that location. If there are Save warnings, then true is still
        /// returned, but the warnings are in explanation.
        /// </summary>
        /// <param name="projectPathAndFile"></param>
        /// <param name="experimentName"></param>
        /// <param name="saveModelAfterRun"></param>
        /// <param name="explanation"></param>
        /// <returns></returns>
        public static bool RunModelExperiment(ISimioProject project, string saveProjectPath,
            string modelName, string experimentName,
            out string explanation)
        {
            explanation = "";
            string marker = "Begin";

            try
            {
                marker = $"Loading Model named={modelName}";
                IModel model = LoadModel(project, modelName, out explanation);
                if (model == null)
                    return false;

                
                marker = $"Loading Experiment named={experimentName}";
                IExperiment experiment = LoadExperiment(model, experimentName, out explanation);
                if (experiment == null)
                    return false;

                // Create some methods to handle experiment events

                // Here the 'e' is ReplicationEndedEventArgs
                experiment.ReplicationEnded += (s, e) =>
                {
                    LogIt($"Info: Event=> Ended Replication={e.ReplicationNumber} of Scenario={e.Scenario.Name}.");
                };

                // Here the 'e' is ScenarioEndedEventArgs
                experiment.ScenarioEnded += (s, e) =>
                {
                    LogIt($"Info: Event=> Scenario={e.Scenario.Name} Ended.");
                };

                // Here the 'e' is RunCompletedEventArgs
                experiment.RunCompleted += (s, e) =>
                {
                    LogIt($"Info: Event=> Experiment={experiment.Name} Run Complete. ");
                    foreach (ITable table in model.Tables)
                    {
                        table.ExportForInteractive();
                    }

                };

                // Now do the run.
                experiment.Reset();
                experiment.Run();
                //experiment.RunAsync(); // Another option

                // Let's look at some results
                var response = experiment.Responses;

                if (File.Exists(saveProjectPath))
                {
                    marker = $"Save Project After Experiment Run to={saveProjectPath}";
                    LogIt($"Info: {marker}");

                    string[] warnings;
                    SimioProjectFactory.SaveProject(project, saveProjectPath, out warnings);
                    explanation = "";
                    int nn = 0;
                    foreach (string warning in warnings)
                    {
                        explanation += $"Save Warnings({warnings.Length}): Warning{++nn}:{warning}";
                        LogIt($"Warning: {warning}");
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                throw new ApplicationException($"Marker={marker} Err={ex.Message}");
            }
        }

        /// <summary>
        /// Save the given project to the 'savePath'.
        /// </summary>
        /// <param name="project"></param>
        /// <param name="savePath"></param>
        public static bool SaveProject(SimEngineContext context, string savePath, out string explanation)
        {
            explanation = "";
            string marker = "Begin.";
            string[] warnings;

            // If project not loaded, return error
            if (context == null || context.CurrentProject == null)
            {
                explanation = $"Context or Project is null.";
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

                // Open project file.
                marker = $"Saving Project={context.CurrentProject.Name} to {savePath}.";
                LogIt($"Info: {marker}");
                if (!SimioProjectFactory.SaveProject(context.CurrentProject, savePath, out warnings))
                    LogIt($"SaveProject failed.");

                marker = $"Saved Project={savePath} with {warnings.Count()} warnings.";
                int ii = 1;
                foreach (string warning in warnings)
                {
                    LogIt($"Warning: {ii++}{warning}");
                }

                return true;
            }
            catch (Exception ex)
            {
                explanation = $"Cannot Save Simio Project={context.CurrentProject.Name} to {savePath} Err={ex.Message}";
                return false;
            }
        }

        /// <summary>
        /// Create and return HeadlessContext with the given extensions path and
        /// a Simio Project at projectFullPath.
        /// Returns null if there is errors are encountered, along with an explanation.
        /// Note that the project can load with a warnings list, which is null if there are no errors.
        /// Load the project file and return a SimioProject
        /// </summary>
        /// <param name="projectFullPath"></param>
        public static SimEngineContext CreateContext(string extensionsPath, out string explanation)
        {
            explanation = "";
            
            try
            {
                SimEngineContext context = new SimEngineContext(extensionsPath);

                return context;
            }
            catch (Exception ex)
            {
                explanation = $"Cannot Create Context with ExtensionsPath={extensionsPath} Err={ex.Message}";
                return null;
            }
        }


        /// <summary>
        /// Set the extensions path and then Load the project file and return a SimioProject.
        /// </summary>
        /// <param name="projectFullPath"></param>
        public static ISimioProject LoadProject(string extensionsPath, string projectFullPath, out string explanation)
        {
            explanation = "";
            string marker = "Begin.";
            string[] warnings;

            try
            {
                // If File Not Exist, Throw Exeption
                if (File.Exists(projectFullPath) == false)
                {
                    explanation = $"Project File={projectFullPath} not found.";
                    return null;
                }

                if (Directory.Exists(extensionsPath) == false)
                {
                    explanation = $"ExtensionsPath={extensionsPath} not found.";
                    return null;
                }

                marker = $"Setting extensions path={extensionsPath}";
                LogIt($"Info: {marker}");
                SimioProjectFactory.SetExtensionsPath(extensionsPath);

                // Open project file.
                marker = $"Loading Project={projectFullPath}.";
                LogIt($"Info: {marker}");
                ISimioProject simioProject = SimioProjectFactory.LoadProject(projectFullPath, out warnings);

                marker = $"Loaded Project={projectFullPath} with {warnings.Count()} warnings.";
                int ii = 1;
                foreach (string warning in warnings)
                {
                    LogIt($"Warning: {ii++}{warning}");
                }

                return simioProject;
            }
            catch (Exception ex)
            {
                throw new ApplicationException($"Cannot load={projectFullPath} Err={ex.Message}");
            }
        }



        /// <summary>
        /// Load the model from the given project.
        /// Returns a Model object or a null if errors.
        /// </summary>
        /// <param name="projectFullPath"></param>
        private static IModel LoadModel(ISimioProject project, string modelName, out string explanation)
        {
            explanation = "";
            string marker = "Begin.";

            try
            {
                marker = $"Loading Model={modelName}";
                LogIt($"Info: {marker}");

                // Get the model from within the project
                var model = project.Models[modelName];
                if (model != null)
                {
                    if ( model.Errors.Count == 0 )
                    {
                        LogIt($"Info: Model={modelName} Successfully loaded.");
                    }
                    else
                    {
                        LogIt($"Model={modelName} not loaded due to {model.Errors.Count} errors. Individual errors follow:");
                        int errorCount = 0;
                        // Log any model errors
                        foreach (IError err in model.Errors)
                        {
                            LogIt($"  {++errorCount}. Error={err.ErrorText} Object={err.ObjectName} Type={err.ObjectType} Property: Name={err.PropertyName} Value={err.PropertyValue}");
                        }
                        return null;
                    }
                }
                else // model is null
                {
                    explanation = $"Model={modelName} could not be loaded from Project={project.Name}";
                    return null;
                }

                return model;
            }
            catch (Exception ex)
            {
                explanation = $"Cannot load={modelName} Err={ex.Message}";
                return null;
            }
        }

        /// <summary>
        /// Load the experiment with name 'experimentName' from the given model.
        /// Returns an Experiment object or a null if errors.
        /// </summary>
        /// <param name="model">The model object; see LoadModel</param>
        /// <param name="experimentName">The name of an experiment with model</param>
        private static IExperiment LoadExperiment(IModel model, string experimentName, out string explanation)
        {
            explanation = "";
            string marker = "Begin.";

            if (model == null)
            {
                explanation = "Cannot load Experiment: Model is null.";
                return null;
            }

            try
            {
                marker = $"Loading Experiment={experimentName}";

                // Get the experiment from within the Model
                var experiment = model.Experiments[experimentName];
                if (experiment == null)
                {
                    explanation = $"Experiment={experimentName} Not Found within Model={model.Name}";
                    return null;
                }

                return experiment;
            }
            catch (Exception ex)
            {
                explanation = $"Cannot load={experimentName} Err={ex.Message}";
                return null;
            }
        }

        /// <summary>
        /// Set the extensionpath, load a project, then load the model, and run a plan for that model.
        /// </summary>
        /// <param name="extensionsPath">For DLL search. E.g. AppDomain.CurrentDomain.BaseDirectory</param>
        /// <param name="projectPathAndFile"></param>
        /// <param name="modelName"></param>
        /// <param name="runRiskAnalysis"></param>
        /// <param name="saveModelAfterRun"></param>
        /// <param name="publishPlanAfterRun"></param>
        /// <param name="explanation"></param>
        /// <returns></returns>
        public static bool RunModelPlan(string extensionsPath, string projectFullPath, string modelName, bool runRiskAnalysis, bool saveModelAfterRun, bool publishPlanAfterRun, out string explanation)
        {
            explanation = "";
            string marker = "Begin";

            string[] warnings;

            try
            {
                // Set an extensions path to where we can locate User Extensions, etc.
                if (string.IsNullOrEmpty(extensionsPath))
                {
                    extensionsPath = System.AppDomain.CurrentDomain.BaseDirectory;
                    LogIt($"Info: No ExtensionsPath supplied. Defaulting to={extensionsPath}");
                }

                marker = $"Setting Extensions Path to={extensionsPath}";
                LogIt($"Info: {marker}");

                // Load the Project
                ISimioProject project = null;
                try
                {
                    Cursor.Current = Cursors.WaitCursor;
                    SimioProjectFactory.SetExtensionsPath(extensionsPath);

                    project = LoadProject(extensionsPath, projectFullPath, out explanation);
                    if (project == null)
                        return false;
                }
                catch (Exception ex)
                {
                    explanation = $"Cannot load from {projectFullPath}. Err={ex.Message}";
                    return false;
                }
                finally
                {
                    Cursor.Current = Cursors.Default;
                }

                // Load the Model
                IModel model = LoadModel(project, modelName, out explanation);
                if (model == null)
                    return false;

                // Test to see if we can 'cheat'
                IPlan plan = (IPlan)model;
                plan.RunPlan(new RunPlanOptions() { AllowDesignErrors = true } );

                // Check for Plan
                if (model.Plan == null)
                {
                    explanation = $"Model={model.Name} has no Plan (you may not have a license for one)";
                    return false;
                }

                // Start Plan
                marker = "Starting Plan (model.Plan.RunPlan)";
                LogIt($"Info: {marker}");
                model.Plan.RunPlan( new RunPlanOptions() { AllowDesignErrors = true } );

                IPlan plan2 = (IPlan)model;
                plan2.RunPlan(new RunPlanOptions() { AllowDesignErrors = true });

                if (runRiskAnalysis)
                {
                    marker = "Plan Finished...Starting Analyze Risk (model.Plan.RunRiskAnalysis)";
                    LogIt($"Info: {marker}");
                    model.Plan.RunRiskAnalysis();
                }
                if (saveModelAfterRun)
                {
                    marker = "Save Project After Schedule Run (SimioProjectFactory.SaveProject)";
                    LogIt($"Info: {marker}");
                    SimioProjectFactory.SaveProject(project, projectFullPath, out warnings);
                }
                if (publishPlanAfterRun)
                {
                    marker = "PublishPlan";
                    LogIt($"Info: {marker}");

                    // Todo: Add publish plan code here
                }
                marker = "End";

                return true;
            }
            catch (Exception ex)
            {
                explanation = $"Project={projectFullPath} Model={modelName} Marker={marker} Err={ex.Message}";
                Alert(explanation);
                return false;
            }
        } // RunModel

        public static Dictionary<string, KeyValuePair<string,string>> BuildKvpDictionary(string arguments, out string explanation)
        {
            explanation = "";
            string marker = "Begin";
            try
            {
                Dictionary<string, KeyValuePair<string, string>> argsDict = new Dictionary<string, KeyValuePair<string, string>>();
                List<string> tokensList = arguments.Split(',').ToList();

                foreach (string token in tokensList)
                {
                    marker = $"Token={token}";
                    string[] pairs = token.Split('=');
                    if (pairs.Length == 2)
                    {
                        KeyValuePair<string, string> kvp = new KeyValuePair<string, string>(pairs[0], pairs[1]);
                        if ( !argsDict.ContainsKey(kvp.Key.ToLower()))
                            argsDict.Add(kvp.Key.ToLower(), kvp);
                    }
                }
                return argsDict;
            }
            catch (Exception ex)
            {
                explanation = $"Marker={marker} Err={ex}";
                return null;
            }

        }


        /// <summary>
        /// Return the argument associated with the key.
        /// If the key is not found, then return the default, 
        /// If the default is null, then the argument is required, so
        ///   if not found then null is returned and explanation is set.
        /// </summary>
        /// <param name="argList"></param>
        /// <param name="key"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        private static string GetArgumentAsString(List<RequestArgument> argList, string key, string defaultValue, out string explanation)
        {
            explanation = "";

            RequestArgument arg = argList.SingleOrDefault(rr => rr.Key == key);
            if ( arg != null)
            {
                return arg.Value;
            }    
            else if ( defaultValue == null)
            {
                explanation = $"Key={key}. Null default and key not found.";
                return null;
            }
            else
            {
                return defaultValue;
            }
        }

        /// <summary>
        /// Returns the argument or a default or null if invalid boolean decode.
        /// If default is null, then the argument is required.
        /// If explanation is not empty, then there is a problem.
        /// </summary>
        /// <param name="argDict"></param>
        /// <param name="key"></param>
        /// <param name="defaultBool"></param>
        /// <returns></returns>
        private static bool? GetArgumentAsBoolean( List<RequestArgument> argList, string key, 
            bool? defaultBool, out string explanation )
        {

            string defaultString = defaultBool.ToString();
            string vv = GetArgumentAsString(argList, key, defaultString, out explanation);
            if (vv == null)
                return null;
            else
            {
                if (!bool.TryParse(vv, out bool boolValue))
                {
                    explanation = $"Key={key}. Expected boolean. Found={vv}";
                    return null;
                }
                else
                    return boolValue;
            }

        }
        /// <summary>
        /// Returns the argument or a default or null if invalid boolean decode.
        /// </summary>
        /// <param name="argDict"></param>
        /// <param name="key"></param>
        /// <param name="defaultBool"></param>
        /// <returns></returns>
        private static int? GetArgumentAsInteger( List<RequestArgument> argList, string key, 
            int? defaultInt, out string explanation)
        {
            string defaultString = defaultInt.ToString();
            string vv = GetArgumentAsString(argList, key, defaultString, out explanation);
            if (vv == null)
                return null;
            else
            {
                if (!int.TryParse(vv, out int intValue))
                    return null;
                else
                    return intValue;
            }

        }

        /// <summary>
        /// Set the extensionpath, load a project, then load the model, and run a plan for that model.
        /// </summary>
        /// <param name="extensionsPath">For DLL search. E.g. AppDomain.CurrentDomain.BaseDirectory</param>
        /// <param name="projectPathAndFile"></param>
        /// <param name="arguments">Comma delimited string of arguments</param>
        /// <param name="runRiskAnalysis"></param>
        /// <param name="explanation"></param>
        /// <returns></returns>
        public static bool RunProjectPlan(string extensionsPath, string projectFullPath, List<RequestArgument> argList, out string explanation)
        {
            explanation = "";
            string marker = "Begin";

            string[] warnings;

            try
            {
                // Set an extensions path to where we can locate User Extensions, etc.
                if (string.IsNullOrEmpty(extensionsPath))
                {
                    extensionsPath = System.AppDomain.CurrentDomain.BaseDirectory;
                    LogIt($"Info: No ExtensionsPath supplied. Defaulting to={extensionsPath}");
                }

                marker = $"Setting Extensions Path to={extensionsPath}";
                LogIt($"Info: {marker}");

                string saveAs = GetArgumentAsString(argList, "saveAs", "", out explanation);
                if ( saveAs != null && !Directory.Exists(saveAs))
                {
                    explanation = $"SaveAs path={saveAs} does not exist.";
                    return false;
                }

                string modelName = GetArgumentAsString(argList, "model", "model", out explanation);
                bool? runRiskAnalysis = GetArgumentAsBoolean(argList, "riskAnalysis", false, out explanation);
                if ( runRiskAnalysis == null )
                {
                    explanation = $"{explanation}";
                    return false;
                }
                bool? publishPlan = GetArgumentAsBoolean(argList, "publishPlan", false, out explanation);

                // Load the Project
                ISimioProject project = null;
                try
                {
                    Cursor.Current = Cursors.WaitCursor;
                    SimioProjectFactory.SetExtensionsPath(extensionsPath);

                    project = LoadProject(extensionsPath, projectFullPath, out explanation);
                    if (project == null)
                        return false;
                }
                catch (Exception ex)
                {
                    explanation = $"Cannot load from {projectFullPath}. Err={ex.Message}";
                    return false;
                }
                finally
                {
                    Cursor.Current = Cursors.Default;
                }

                // Load the Model
                IModel model = LoadModel(project, modelName, out explanation);
                if (model == null)
                    return false;

                

                // Test to see if we can 'cheat'
                IPlan plan = (IPlan)model;
                plan.RunPlan(new RunPlanOptions() { AllowDesignErrors = true });

                // Check for Plan
                if (model.Plan == null)
                {
                    explanation = $"Model={model.Name} has no Plan (you may not have a license for one)";
                    return false;
                }

                // Start Plan
                marker = "Starting Plan (model.Plan.RunPlan)";
                LogIt($"Info: {marker}");
                model.Plan.RunPlan(new RunPlanOptions() { AllowDesignErrors = true });

                IPlan plan2 = (IPlan)model;
                plan2.RunPlan(new RunPlanOptions() { AllowDesignErrors = true });

                if ( runRiskAnalysis.Value )
                {
                    marker = "Plan Finished...Starting Analyze Risk (model.Plan.RunRiskAnalysis)";
                    LogIt($"Info: {marker}");
                    model.Plan.RunRiskAnalysis();
                }

                if ( saveAs != null )
                {
                    marker = $"Saving Project to={saveAs}";
                    LogIt($"Info: {marker}");
                    SimioProjectFactory.SaveProject(project, saveAs, out warnings);
                }

                if ( publishPlan.Value )
                {
                    marker = "PublishPlan";
                    LogIt($"Info: {marker}");

                    // Todo: Add publish plan code here
                }
                marker = "End";

                return true;
            }
            catch (Exception ex)
            {
                explanation = $"Project={projectFullPath} Marker={marker} Err={ex.Message}";
                Alert(explanation);
                return false;
            }
        } // RunModel


        /// <summary>
        /// Prompt the user for a Simio project file.
        /// </summary>
        /// <returns></returns>
        public static string GetProjectFile()
        {
            try
            {
                OpenFileDialog dialog = new OpenFileDialog();
                dialog.Multiselect = false;
                dialog.Filter = "Simio Project|*.spfx";

                DialogResult result = dialog.ShowDialog();

                if (result != DialogResult.OK)
                    return string.Empty;

                return dialog.FileName;
            }
            catch (Exception ex)
            {
                throw new ApplicationException($"Cannot get project file. Err={ex.Message}");
            }
        }
        /// <summary>
        /// Prompt the user for a Simio project file.
        /// </summary>
        /// <returns></returns>
        public static string GetExtensionsFolder()
        {
            try
            {
                FolderBrowserDialog dialog = new FolderBrowserDialog();
                dialog.SelectedPath = "";
                dialog.ShowNewFolderButton = false;

                DialogResult result = dialog.ShowDialog();

                if (result != DialogResult.OK)
                    return string.Empty;

                return dialog.SelectedPath;
            }
            catch (Exception ex)
            {
                throw new ApplicationException($"Cannot get project file. Err={ex.Message}");
            }
        }

        /// <summary>
        /// Run an experiment and save.
        /// </summary>
        /// <param name="extensionsPath"></param>
        /// <param name="projectPath"></param>
        /// <param name="modelName"></param>
        /// <param name="experimentName"></param>
        /// <param name="explanation"></param>
        /// <returns></returns>
        public static bool RunProjectExperiment(string extensionsPath, string projectPath, List<RequestArgument> argList,  
            out string explanation)
        {
            explanation = "";

            string marker = "Begin";
            try
            {
                if ( !File.Exists(projectPath ))
                {
                    explanation = $"No such Project={projectPath}";
                    return false;
                }

                string projectFolder = Path.GetDirectoryName(projectPath);
                string projectFilename = Path.GetFileName(projectPath);


                string saveFolder = GetArgumentAsString(argList, "saveFolder", projectFolder, out explanation);
                if (saveFolder != null && !Directory.Exists(saveFolder))
                {
                    explanation = $"SaveFolder={saveFolder} does not exist.";
                    return false;
                }

                string saveFilename = GetArgumentAsString(argList, "saveFilename", projectFilename, out explanation);
                string savePath = Path.Combine(saveFolder, saveFilename);

                string modelName = GetArgumentAsString(argList, "model", "model", out explanation);
                string experimentName = GetArgumentAsString(argList, "experiment", "experiment1", out explanation);

                marker = $"Loading Project from={projectPath}";
                string[] warnings;

                var _simioProject = SimioProjectFactory.LoadProject(projectPath, out warnings);
                if (warnings.Length > 0)
                {
                    explanation = $"Cannot load Project={projectPath} as there were {warnings.Length} warnings";
                    return false;
                }

                marker = $"Access the model={modelName} and Experiment={experimentName}";
                var model = _simioProject.Models[modelName];
                if (model == null)
                {
                    explanation = "Model Not Found In Project";
                    return false;
                }
                else
                {
                    if (model.Experiments == null)
                    {
                        explanation = $"Model's Experiments collection is null.";
                        return false;
                    }

                    // Start Experiment
                    Console.WriteLine("Starting Experiment");

                    if (!SimEngineHelpers.RunModelExperiment(extensionsPath, projectPath, savePath, 
                            modelName, experimentName, 
                            out explanation))
                    {
                        return false;
                    }
                    else
                    {
                        marker = $"Info: Model={modelName} Experiment={experimentName} performed the actions successfully. Check the logs for more information.";
                    }

                }
                return true;
            }
            catch (Exception ex)
            {
                explanation = $"Marker={marker} Project={projectPath}. Err={ex.Message}";
                return false;
            }
        }

        public static void LogIt(string message)
        {
            if (message.ToLower().StartsWith("info:"))
                Loggerton.Instance.LogIt(EnumLogFlags.Information, message);
            else
                Loggerton.Instance.LogIt(EnumLogFlags.Error, message);
        }

        public static void Alert(string message)
        {
            Loggerton.Instance.LogIt(EnumLogFlags.Error, message);
            MessageBox.Show(message);
        }
        public static void Alert(EnumLogFlags flags, string message)
        {
            Loggerton.Instance.LogIt(flags, message);
            MessageBox.Show(message);
        }
    }


    [Flags]
    public enum EnumRunPlanOptions
    {
        None = 0,
        RunRiskAnalysis = 1,
        SaveProjectAfterRun = 2,
        PublicPlanAfterRun = 4
    }
}
