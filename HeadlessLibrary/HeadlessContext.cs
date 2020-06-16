using SimioAPI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace HeadlessLibrary
{
    /// <summary>
    /// A context object that is used by some of the HeadlessHelpers methods
    /// </summary>
    public class HeadlessContext
    {

        public string ProjectPath { get; set; }

        /// <summary>
        /// Points to where all the supporting files (EXE, DLL, etc. live)
        /// </summary>
        public string ExtensionsPath { get; set; }

        /// <summary>
        /// The currently loaded project.
        /// </summary>
        public ISimioProject CurrentProject { get; set; }
        
        /// <summary>
        /// The currently loaded model
        /// </summary>
        public IModel CurrentModel { get; set; }

        /// <summary>
        /// The currently loaded experiment
        /// </summary>
        public IExperiment CurrentExperiment { get; set; }

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

        public HeadlessContext(string extensionsPath)
        {
            if (!Initialize(extensionsPath, out string explanation))
                throw new ApplicationException($"Initializing Headless Context. Err-{explanation}");

        }

        /// <summary>
        /// Initialize, which means setting the ExtensionsPath and loading the project.
        /// If there are warnings, they are placed in LoadWarningsList.
        /// </summary>
        /// <param name="extensionsPath"></param>
        /// <param name="projectFullPath"></param>
        /// <param name="explanation"></param>
        /// <returns></returns>
        public bool Initialize(string extensionsPath, out string explanation)
        {
            explanation = "";
            string marker = "Checking extension and project paths.";

            try
            {
                if (Directory.Exists(extensionsPath) == false)
                {
                    explanation = $"ExtensionsPath={extensionsPath} not found.";
                    return false;
                }

                ExtensionsPath = extensionsPath;

                marker = $"Setting extensions path={extensionsPath}";
                SimioProjectFactory.SetExtensionsPath(extensionsPath);

                return true;

            }
            catch (Exception ex)
            {
                explanation = $"Failed to initialize HeadlessContext. ExtensionsPath={extensionsPath} Err={ex.Message}";
                return false;
            }

        }

        /// <summary>
        /// Initialize, which means setting the ExtensionsPath and loading the project.
        /// If there are warnings, they are placed in LoadWarningsList.
        /// </summary>
        /// <param name="projectFullPath"></param>
        /// <param name="explanation"></param>
        /// <returns></returns>
        public bool LoadProject(string projectFullPath, out string explanation)
        {
            explanation = "";
            string marker = "Checking extension and project paths.";

            if ( ExtensionsPath == null )
            {
                explanation = $"Cannot LoadProject with null ExtensionsPath";
                return false;
            }

            try
            {
                // If File Not Exist, Throw Exception
                if (File.Exists(projectFullPath) == false)
                {
                    explanation = $"Project File={projectFullPath} not found.";
                    return false;
                }

                ProjectPath = projectFullPath;

                marker = $"Setting extensions path={ExtensionsPath}";
                SimioProjectFactory.SetExtensionsPath(ExtensionsPath); // No harm in doing it again.

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
        /// Load the model from the given project.
        /// Returns a Model object or a null if errors.
        /// </summary>
        /// <param name="modelName"></param>
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
                explanation = $"Cannot load Model={modelName} Err={ex.Message}";
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
                marker = "Starting Plan (model.Plan.RunPlan)";
                CurrentModel.Plan.RunPlan();

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
        public bool RunModelExperiment( out string explanation)
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

            // Check for Experiment
            if (CurrentExperiment == null)
            {
                explanation = $"Model={CurrentModel.Name} has no Experiment selected.";
                return false;
            }

            try
            {
                // Run the experiment. Events will be thrown when replications start and end,
                // so you would have to handle those elsewhere.
                marker = "Starting Experiment (Experiment.Run)";
                CurrentExperiment.Run();

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
            if ( CurrentProject == null)
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
