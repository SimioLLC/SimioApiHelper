using LoggertonHelpers;
using SimioAPI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace HeadlessLibrary
{
    /// <summary>
    /// Helpers for running the Simio Model in a non-UI aka "headless" mode.
    /// </summary>
    public static class HeadlessHelpers
    {

        /// <summary>
        /// Run an experiment
        /// </summary>
        /// <param name="projectPathAndFile"></param>
        /// <param name="experimentName"></param>
        /// <param name="saveModelAfterRun"></param>
        /// <param name="explanation"></param>
        /// <returns></returns>
        public static bool RunExperiment(string extensionsPath, string projectFullPath, string modelName, string experimentName, bool saveModelAfterRun, out string explanation)
        {
            explanation = "";
            string marker = "Begin";

            try
            {
                // Set an extensions path to where we can locate User Extensions, etc.
                if ( string.IsNullOrEmpty(extensionsPath))
                    extensionsPath = System.AppDomain.CurrentDomain.BaseDirectory;

                marker = $"Setting Extensions Path to={extensionsPath}";
                LogIt($"Info: {marker}");
                SimioProjectFactory.SetExtensionsPath(extensionsPath);

                marker = $"Loading Project from={projectFullPath}";
                ISimioProject project = LoadProject(extensionsPath, projectFullPath, out explanation);
                if (project == null)
                    return false;

                marker = $"Loading Model named={modelName}";
                IModel model = LoadModel(project, modelName, out explanation);
                if (model == null)
                    return false;

                marker = $"Loading Experiment named={experimentName}";
                IExperiment experiment = LoadExperiment(model, experimentName, out explanation);
                if (experiment == null)
                    return false;

                // Create some methods to handle experiment events
                experiment.ReplicationEnded += (s, e) =>
                {
                    LogIt($"Info: Event=> Ended Replication={e.ReplicationNumber} of Scenario={e.Scenario.Name}.");
                };

                experiment.ScenarioEnded += (s, e) =>
                {
                    LogIt($"Info: Event=> Scenario={e.Scenario.Name} Ended.");
                };

                experiment.RunCompleted += (s, e) =>
                {
                    LogIt($"Info: Event=> Experiment={experiment.Name} Run Complete. ");
                };

                // Now do the run.
                experiment.Reset();
                experiment.Run();
                //experiment.RunAsync(); // Another option

                if (saveModelAfterRun)
                {
                    marker = $"Save Project After Experiment Run to= (SimioProjectFactory.SaveProject)";
                    LogIt($"Info: {marker}");

                    string[] warnings;
                    SimioProjectFactory.SaveProject(project, projectFullPath, out warnings);
                    foreach ( string warning in warnings)
                    {
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
        /// Load the project file and return a SimioProject
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
                if ( File.Exists(projectFullPath) == false)
                {
                    explanation = $"Project File={projectFullPath} not found.";
                    return null;
                }

                if ( Directory.Exists(extensionsPath) == false )
                {
                    explanation = $"ExtensionsPath={extensionsPath} not found.";
                    return null;
                }

                marker = $"Setting extensions path={extensionsPath}";
                SimioProjectFactory.SetExtensionsPath(extensionsPath);

                // Open project file.
                marker = $"Loading Project={projectFullPath}.";
                LogIt($"Info: {marker}");
                ISimioProject simioProject = SimioProjectFactory.LoadProject(projectFullPath, out warnings);
                foreach (string warning in warnings)
                {
                    LogIt($"Warning: {warning}");
                }
                LogIt($"Info: Project={projectFullPath} Loaded.");
                return simioProject;
            }
            catch (Exception ex)
            {
                throw new ApplicationException($"Cannot load={projectFullPath} Err={ex.Message}");
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
        public static HeadlessContext LoadProject(string extensionsPath, string projectFullPath, out List<string> warnings, out string explanation)
        {
            explanation = "";
            warnings = null;

            try
            {
                HeadlessContext context = new HeadlessContext();

                if (context.Initialize(explanation, projectFullPath, out explanation))
                    return null;

                return context;
            }
            catch (Exception ex)
            {
                explanation = $"Cannot load={projectFullPath} Err={ex.Message}";
                return null;
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
        /// Load the experiment from the given model.
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
        /// Set the extensionpath, load a project, and run a plan for the given model.
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

                // Check for Plan
                if (model.Plan == null)
                {
                    explanation = $"Model={model.Name} has no Plan.";
                    return false;
                }

                // Start Plan
                marker = "Starting Plan (model.Plan.RunPlan)";
                LogIt($"Info: {marker}");
                model.Plan.RunPlan();

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

                    // ADD PUBLISH PLAN CODE HERE
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


        public static void LogIt(string message)
        {
            if (message.ToLower().StartsWith("info:"))
                Loggerton.Instance.LogIt(EnumLogFlags.Information, message);
            else
                Loggerton.Instance.LogIt(EnumLogFlags.Error, message);
        }

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

    public class HeadlessContext
    {

        public string ProjectPath { get; set; }

        public string ExtensionsPath { get; set; }

        public ISimioProject CurrentProject { get; set; }
        
        public IModel CurrentModel { get; set; }
   
        public List<string> ProjectLoadErrorList { get; set; }

        public List<string> ModelLoadErrorList { get; set; }

        public HeadlessContext()
        {


        }

        /// <summary>
        /// Initialize, which means setting the ExtensionsPath and loading the project.
        /// If there are warnings, they are placed in LoadWarningsList.
        /// </summary>
        /// <param name="extensionsPath"></param>
        /// <param name="projectFullPath"></param>
        /// <param name="explanation"></param>
        /// <returns></returns>
        public bool Initialize(string extensionsPath, string projectFullPath, out string explanation)
        {
            explanation = "";
            string marker = "Checking extension and project paths.";

            try
            {
                // If File Not Exist, Throw Exeption
                if (File.Exists(projectFullPath) == false)
                {
                    explanation = $"Project File={projectFullPath} not found.";
                    return false;
                }

                if (Directory.Exists(extensionsPath) == false)
                {
                    explanation = $"ExtensionsPath={extensionsPath} not found.";
                    return false;
                }

                ProjectPath = projectFullPath;
                ExtensionsPath = extensionsPath;

                marker = $"Setting extensions path={extensionsPath}";
                SimioProjectFactory.SetExtensionsPath(extensionsPath);

                // Open project file.
                marker = $"Loading Project={projectFullPath}.";
                CurrentProject = SimioProjectFactory.LoadProject(projectFullPath, out string[] warnings);

                ProjectLoadErrorList = null;
                if ( warnings.Length > 0 )
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
                explanation = $"Failed to initialize HeadlessContext. ProjectPath={projectFullPath} Err={ex.Message}";
                return false;
            }

        }

        /// <summary>
        /// Load the model from the given project.
        /// Returns a Model object or a null if errors.
        /// </summary>
        /// <param name="projectFullPath"></param>
        public bool LoadModel(string modelName, out string explanation)
        {
            explanation = "";
            string marker = "Begin.";

            if ( CurrentProject == null  )
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
                    ModelLoadErrorList.Clear();
                    if (CurrentModel.Errors.Count > 0 )
                    {
                        int errorCount = 0;
                        // Log any model errors
                        foreach (IError err in CurrentModel.Errors)
                        {
                            ModelLoadErrorList.Add($"  {++errorCount}. Error={err.ErrorText} Object={err.ObjectName} Type={err.ObjectType} Property: Name={err.PropertyName} Value={err.PropertyValue}");
                        }
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
                explanation = $"Cannot load={modelName} Err={ex.Message}";
                return false;
            }
        }

        /// <summary>
        /// Assumes a Model is loaded, this initiates a RunPlan with the given options.
        /// </summary>
        /// <param name="options"></param>
        /// <param name="explanation"></param>
        /// <returns></returns>
        public bool RunModelPlan(EnumRunPlanOptions options, out string explanation)
        {
            explanation = "";
            string marker = "Begin";

            string[] warnings;

            if ( CurrentProject == null )
            {
                explanation = $"Cannot run plan. No Project is currently loaded";
                return false;
            }

            if ( CurrentModel == null  )
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

                // Start Plan
                marker = "Starting Plan (model.Plan.RunPlan)";
                CurrentModel.Plan.RunPlan();

                if ( (options & EnumRunPlanOptions.RunRiskAnalysis) != 0 )
                {
                    marker = "Plan Finished...Starting Analyze Risk (model.Plan.RunRiskAnalysis)";
                    CurrentModel.Plan.RunRiskAnalysis();
                }
                if ( (options & EnumRunPlanOptions.SaveProjectAfterRun) != 0)
                {
                    marker = "Save Project After Schedule Run (SimioProjectFactory.SaveProject)";
                    SimioProjectFactory.SaveProject(CurrentProject, this.ProjectPath, out warnings);
                }
                if ( (options & EnumRunPlanOptions.PublicPlanAfterRun) != 0)
                {
                    marker = "PublishPlan";

                    // ADD PUBLISH PLAN CODE HERE
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

    }
}
