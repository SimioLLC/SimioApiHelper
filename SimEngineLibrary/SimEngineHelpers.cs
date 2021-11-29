using LoggertonHelpers;
using SimEngineInterfaceHelpers;
using SimioAPI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimEngineLibrary
{
    /// <summary>
    /// Helpers for automating the bare SimioEngine (aka "headless" mode).
    /// </summary>
    public static class SimEngineHelpers
    {

        public static string BuildSimioDesktopExtensionsPath(string simioInstallPath, bool testForExistence)
        {
            string marker = $"Begin. InstallPath={simioInstallPath}";
            try
            {
                string userExtensionsPath = Path.Combine(simioInstallPath, "UserExtensions");

                marker = $"UserExtenationsPath={userExtensionsPath}";

                if ( testForExistence )
                {
                    marker = $"Test that Folder={userExtensionsPath} exists.";

                    if (Directory.Exists(userExtensionsPath) == false)
                        throw new ApplicationException($"Folder={userExtensionsPath} does not exist.");

                }

                return userExtensionsPath;
            }
            catch (Exception ex)
            {
                throw new ApplicationException($"Marker={marker} Err={ex.Message}");
            }
        }

        /// <summary>
        /// Find the 'normal' installation path, which depends on whether this is a 32 or 64 bit targetted app.
        /// If 32 or Any, the ProgramFiles location will both resolve to "program files (x86)" and Simio is under "Simio"
        /// If 64, then it will resolve to program files, and Simio is under "Simio LLC > Simio"
        /// </summary>
        /// <param name="explanation"></param>
        /// <returns></returns>
        public static string FindNormalSimioInstallPath()
        {
            string marker = "Begin.";
            try
            {
                string programFiles = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
                string programFiles86 = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);

                string installPath = "";
                if ( programFiles == programFiles86)
                {
                    installPath = Path.Combine(programFiles86, "simio");
                }
                else
                {
                    installPath = Path.Combine(programFiles, "simio llc", "simio");
                }

                if (Directory.Exists(installPath) == false)
                    throw new ApplicationException($"Simio InstallPath={installPath} cannot be found.");

                return installPath;
    
            }
            catch (Exception ex)
            {
                throw new ApplicationException($"Marker={marker} Err={ex.Message}");
            }
        }

        public static string BuildSimioDesktopExtensionsPath(bool testForExistence)
        {
            string marker = $"Begin.";
            try
            {
                string simioInstallPath = FindNormalSimioInstallPath();
                marker = $"InstallPath={simioInstallPath}";

                string userExtensionsPath = Path.Combine(simioInstallPath, "UserExtensions");

                marker = $"UserExtenationsPath={userExtensionsPath}";

                if (testForExistence)
                {
                    marker = $"Test that Folder={userExtensionsPath} exists.";

                    if (Directory.Exists(userExtensionsPath) == false)
                        throw new ApplicationException($"Folder={userExtensionsPath} does not exist.");
                }

                return userExtensionsPath;
            }
            catch (Exception ex)
            {
                throw new ApplicationException($"Marker={marker} Err={ex.Message}");
            }
        }

        /// <summary>
        /// Run an experiment. The experiment is Reset prior to run.
        /// </summary>
        /// <param name="extensionsPath"></param>
        /// <param name="sourceProjectPath"></param>
        /// <param name="saveProjectPath">Full path to where to save project</param>
        /// <param name="experimentName"></param>
        /// <param name="modelName"></param>
        /// <param name="explanation"></param>
        /// <returns></returns>
        public static bool RunModelExperiment(string extensionsPath, string sourceProjectPath, string saveProjectPath,
            string modelName, string experimentName, 
            List<string> warningList,
            out string explanation)
        {
            if ( warningList == null )
            {
                explanation = $"WarningList cannot be null.";
                return false;
            }

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
                warningList.Clear();
                ISimioProject project = LoadProject(extensionsPath, sourceProjectPath, warningList, out explanation);
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
        /// A loaded project is passed in that must contain the named Model and Experiment.
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
                if ( !RunModelExperiment(model, experimentName, out explanation))
                {
                    throw new ApplicationException($"Cannot Run Experiment={experimentName}. Err={explanation}");
                }

                // Successful run. Save the project?
                if (File.Exists(saveProjectPath))
                {
                    marker = $"Save Project After Experiment Run to={saveProjectPath}";
                    LogIt($"Info: {marker}");

                    SimioProjectFactory.SaveProject(project, saveProjectPath, out string[] warnings);
                    explanation = "";
                    if (warnings.Any())
                    {
                        LogIt($"Warning: 'SaveProject' had {warnings.Length} Warnings:");
                        int nn = 0;
                        foreach (string warning in warnings)
                        {
                            explanation += $"  Warning[{++nn}]:{warning}";
                            LogIt($"  Warning: {warning}");
                        }
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
        /// Run an experiment. The experiment is Reset prior to run.
        /// A loaded project is passed in that must contain the named Model and Experiment.
        /// If the saveProjectPath is present and exists, then the project will be
        /// saved to that location. If there are Save warnings, then true is still
        /// returned, but the warnings are in explanation.
        /// </summary>
        /// <param name="model"></param>
        /// <param name="experimentName"></param>
        /// <param name="explanation"></param>
        /// <returns></returns>
        public static bool RunModelExperiment(IModel model, 
            string experimentName,
            out string explanation)
        {
            explanation = "";
            string marker = "Begin";

            try
            {

                marker = $"Loading Experiment named={experimentName}";
                IExperiment experiment = LoadExperiment(model, experimentName, out explanation);
                if (experiment == null)
                    return false;

                // Create some methods to handle experiment events. Events are:
                // RunStarted, RunProgessChanged, RunCompleted
                // ScenarioStarted, ScenarioEnded
                // ReplicationStarted, ReplicationEnded
                // 


                // Here the 'e' is RunStartedEventArgs
                experiment.RunStarted += (s, e) =>
                {
                    marker = $"Event=> Experiment={experiment.Name} Run Started.";
                    LogIt($"Info: {marker}");
                };

                // Here the 'e' is ReplicationEndedEventArgs
                experiment.ReplicationEnded += (s, e) =>
                {
                    marker = $"Event=> Ended Replication={e.ReplicationNumber} of Scenario={e.Scenario.Name}.";
                    LogIt($"Info: {marker}");
                };

                // Here the 'e' is ScenarioStartedEventArgs
                experiment.ScenarioStarted += (s, e) =>
                {
                    marker = $"Event=> Scenario={e.Scenario.Name} Started";
                    LogIt($"Info: {marker}.");

                };
                experiment.ScenarioEnded += (s, e) =>
                {
                    marker = $"Event=> Scenario={e.Scenario.Name} Ended";
                    LogIt($"Info: {marker}");
                };


                // Here the 'e' is RunCompletedEventArgs
                experiment.RunCompleted += (s, e) =>
                {
                    marker = $"Event=> Experiment={experiment.Name} Run Complete.";
                    LogIt($"Info: {marker}");
                    foreach (ITable table in model.Tables)
                    {
                        table.ExportForInteractive();
                    }

                };

                // Now do the run.
                LogIt($"Info: Resetting Experiment={experiment.Name}");
                //experiment.Reset();
                LogIt($"Info: Running Experiment={experiment.Name}");
                experiment.Run();
                //experiment.RunAsync(); // Another option

                // Let's look at some results
                var response = experiment.Responses;


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
        public static bool SaveProject(SimEngineContext context, string savePath, List<string> warningList, out string explanation)
        {
            explanation = "";
            string marker = "Begin.";

            if ( warningList == null )
            {
                explanation = $"WarningList cannot be null.";
                return false;
            }


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

                string[] warnings;
                if (!SimioProjectFactory.SaveProject(context.CurrentProject, savePath, out warnings))
                    LogIt($"SaveProject failed.");

                marker = $"Saved Project={savePath} with {warnings.Count()} warnings.";
                int ii = 1;
                foreach (string warning in warnings)
                {
                    warningList.Add($"Warning[{++ii}]:OnSave: {warning}");
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
        /// Save the given project to the 'savePath'.
        /// </summary>
        /// <param name="project"></param>
        /// <param name="saveFolderPath"></param>
        /// <param name="warningList">warnings and errors from saving go here</param>
        public static bool SaveProject(ISimioProject project, string saveFolderPath, List<string> warningList, out string explanation)
        {
            explanation = "";
            string marker = "Begin.";

            if (warningList == null)
            {
                explanation = $"WarningList cannot be null.";
                return false;
            }


            // If project not loaded, return error
            if ( project == null)
            {
                explanation = $"Project is null.";
                return false;
            }

            string folderPath = Path.GetDirectoryName(saveFolderPath);

            if (Directory.Exists(folderPath) == false)
            {
                explanation = $"FolderPath={folderPath} not found.";
                return false;
            }

            try
            {

                // Open project file.
                marker = $"Saving Project={project.Name} to {saveFolderPath}.";
                LogIt($"Info: {marker}");

                bool canSave = SimioProjectFactory.SaveProject(project, saveFolderPath, out string[] warnings);

                marker = $"Saved Project={saveFolderPath} with {warnings.Count()} warnings.";
                int ii = 0;
                foreach (string warning in warnings)
                {
                    ++ii;
                    warningList.Add($"Warning[{ii}]:OnSave: {warning}");
                    LogIt($"Warning: {ii}{warning}");
                }

                if ( !canSave )
                {
                    explanation = $"Save for Project={project.Name} failed.";
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                explanation = $"Cannot Save Simio Project={project.Name} to {saveFolderPath} Err={ex.Message}";
                return false;
            }
        }



        /// <summary>
        /// Set the extensions path and then Load the project file and return a SimioProject.
        /// Warnings are added to the warningList
        /// </summary>
        /// <param name="projectFullPath"></param>
        public static ISimioProject LoadProject(string extensionsPath, string projectFullPath, List<string> warningList, out string explanation)
        {
            explanation = "";
            string marker = "Begin.";

            if ( warningList == null )
            {
                explanation = $"WarningList cannot be null.";
                return null;
            }

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
                string[] warnings;

                LogIt($"Info: {marker}");
                ISimioProject simioProject = SimioProjectFactory.LoadProject(projectFullPath, out warnings);

                marker = $"Loaded Project={projectFullPath} with {warnings.Count()} warnings.";
                int ii = 1;
                warningList.Clear();
                foreach (string warning in warnings)
                {
                    warningList.Add($"Warning[{ii++}]:OnLoad: {warning}");
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
        /// <param name="projectFullPath"></param>
        /// <param name="modelName"></param>
        /// <param name="runRiskAnalysis"></param>
        /// <param name="saveModelAfterRun"></param>
        /// <param name="publishPlanAfterRun"></param>
        /// <param name="explanation"></param>
        /// <returns></returns>
        public static bool RunModelPlan(string extensionsPath, string projectFullPath, 
                string modelName, 
                bool runRiskAnalysis, bool saveModelAfterRun, bool publishPlanAfterRun, 
                List<string> warningList,
                out string explanation)
        {
            explanation = "";
            string marker = "Begin";

            if ( warningList == null )
            {
                explanation = $"WarningList cannot be null.";
                return false;
            }

            warningList.Clear();
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
                    //Cursor.Current = Cursors.WaitCursor;
                    SimioProjectFactory.SetExtensionsPath(extensionsPath);

                    project = LoadProject(extensionsPath, projectFullPath, warningList, out explanation);
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
                    //Cursor.Current = Cursors.Default;
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
                    SaveProject(project, projectFullPath, warningList, out explanation);
                    
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

        /// <summary>
        /// Build a KVP dictionary from a comma-delimited string of key=value pairs.
        /// The key is always low-cased.
        /// </summary>
        /// <param name="arguments"></param>
        /// <param name="explanation"></param>
        /// <returns></returns>
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
        public static bool RunProjectPlan(string extensionsPath, string projectFullPath, List<RequestArgument> argList, List<string> warningList, 
            out string explanation)
        {
            explanation = "";
            string marker = "Begin";

            if ( warningList == null)
            {
                explanation = $"WarningList cannot be null.";
                return false;
            }


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
                    //Cursor.Current = Cursors.WaitCursor;
                    SimioProjectFactory.SetExtensionsPath(extensionsPath);

                    project = LoadProject(extensionsPath, projectFullPath, warningList,  out explanation);
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
                    //Cursor.Current = Cursors.Default;
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

                    bool canSave = SaveProject(project, saveAs, warningList, out explanation);
                    if ( !canSave )
                    {
                        explanation = $"Cannot Save Project";
                        return false;
                    }
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
        /// Run an experiment and save.
        /// </summary>
        /// <param name="extensionsPath"></param>
        /// <param name="projectPath"></param>
        /// <param name="modelName"></param>
        /// <param name="experimentName"></param>
        /// <param name="explanation"></param>
        /// <returns></returns>
        public static bool RunProjectExperiment(string extensionsPath, string projectPath, List<RequestArgument> argList, 
            List<string> warningList,
            out string explanation)
        {
            explanation = "";

            if ( warningList == null )
            {
                explanation = $"WarningList cannot be null.";
                return false;
            }

            string marker = "Begin";
            try
            {
                marker = $"Check ProjectPath={projectPath}";
                if ( !File.Exists(projectPath ))
                {
                    explanation = $"No such Project={projectPath}";
                    return false;
                }

                string projectFolder = Path.GetDirectoryName(projectPath);
                string projectFilename = Path.GetFileName(projectPath);


                string saveFolder = GetArgumentAsString(argList, "saveFolder", projectFolder, out explanation);
                marker = $"Check SaveFolder={saveFolder}";

                if (saveFolder != null && !Directory.Exists(saveFolder))
                {
                    explanation = $"SaveFolder={saveFolder} does not exist.";
                    return false;
                }

                string saveFilename = GetArgumentAsString(argList, "saveFilename", projectFilename, out explanation);
                string savePath = Path.Combine(saveFolder, saveFilename);

                string modelName = GetArgumentAsString(argList, "model", "model", out explanation);
                string experimentName = GetArgumentAsString(argList, "experiment", "experiment1", out explanation);

                string runContextNote = $"SavePath={savePath}. Model={modelName} Experiment={experimentName}";

                marker = $"Setting ExtensionsPath={extensionsPath}";
                SimioProjectFactory.SetExtensionsPath(extensionsPath);

                marker = $"Loading Project from={projectPath}. Run Context={runContextNote}";

                warningList.Clear();
                var _simioProject = LoadProject(extensionsPath, projectPath, warningList, out explanation);
                if (_simioProject == null)
                {
                    explanation = $"Cannot load Project={projectPath} as there were {warningList.Count}";
                    return false;
                }

                marker = $"Access the model={modelName} and Experiment={experimentName}";
                var model = _simioProject.Models[modelName];
                if (model == null)
                {
                    explanation = $"Model={modelName} Not Found In Project";
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
                            warningList,
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
            {
                Loggerton.Instance.LogIt(EnumLogFlags.Information, message);
            }
            else
            {
                Loggerton.Instance.LogIt(EnumLogFlags.Error, message);
                System.Diagnostics.Trace.WriteLine(message, "Error");
            }
        }

        public static void Alert(string message)
        {
            Loggerton.Instance.LogIt(EnumLogFlags.Error, message);
            System.Diagnostics.Trace.WriteLine(message, "Error");
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
