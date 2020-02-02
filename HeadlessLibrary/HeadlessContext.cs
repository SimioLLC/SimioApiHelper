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
