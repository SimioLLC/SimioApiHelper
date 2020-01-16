using LoggertonHelpers;
using SimioAPI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SimioApiHelper
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
        public static bool RunExperiment(string projectFullPath, string modelName, string experimentName, bool saveModelAfterRun, out string explanation)
        {
            explanation = "";
            string marker = "Begin";

            try
            {
                // Set an extensions path to where we can locate User Extensions, etc.
                string extensionsPath = System.AppDomain.CurrentDomain.BaseDirectory;
                marker = $"Setting Extensions Path to={extensionsPath}";
                LogIt($"Info: {marker}");
                SimioProjectFactory.SetExtensionsPath(extensionsPath);

                marker = $"Loading Project from={projectFullPath}";
                ISimioProject project = LoadProject(projectFullPath, out explanation);
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
                throw new ApplicationException($"Marker={marker} Err={ex}");
            }
        }


        /// <summary>
        /// Load the project file and return a SimioProject
        /// </summary>
        /// <param name="projectFullPath"></param>
        private static ISimioProject LoadProject(string projectFullPath, out string explanation)
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

                // Open project file.
                marker = "Loading Model";
                LogIt($"Info: {marker}");
                ISimioProject simioProject = SimioProjectFactory.LoadProject(projectFullPath, out warnings);
                foreach (string warning in warnings)
                {
                    LogIt($"Warning: {warning}");
                }
                return simioProject;
            }
            catch (Exception ex)
            {
                throw new ApplicationException($"Cannot load={projectFullPath} Err={ex}");
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

                // Get the model from within the project
                var model = project.Models[modelName];
                if (model.Errors.Count > 0)
                {
                    LogIt($"Model not loaded due to {model.Errors.Count} errors. Individual errors follow:");
                    int errorCount = 0;
                    // Log any model errors
                    foreach (IError err in model.Errors)
                    {
                        LogIt($"  {++errorCount}. Error={err.ErrorText} Object={err.ObjectName} Type={err.ObjectType} Property: Name={err.PropertyName} Value={err.PropertyValue}");
                    }
                }
                else if (model == null)
                {
                    explanation = $"Model={modelName} could not be loaded from Project={project.Name}";
                    return null;
                }

                return model;
            }
            catch (Exception ex)
            {
                explanation = $"Cannot load={modelName} Err={ex}";
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
                explanation = $"Cannot load={experimentName} Err={ex}";
                return null;
            }
        }

        /// <summary>
        /// Run a model plan
        /// </summary>
        /// <param name="projectPathAndFile"></param>
        /// <param name="modelName"></param>
        /// <param name="runRiskAnalysis"></param>
        /// <param name="saveModelAfterRun"></param>
        /// <param name="publishPlanAfterRun"></param>
        /// <param name="explanation"></param>
        /// <returns></returns>
        public static bool RunModel(string projectFullPath, string modelName, bool runRiskAnalysis, bool saveModelAfterRun, bool publishPlanAfterRun, out string explanation)
        {
            explanation = "";
            string marker = "Begin";

            string[] warnings;

            try
            {
                // Set an extensions path to where we can locate User Extensions, etc.
                string extensionsPath = System.AppDomain.CurrentDomain.BaseDirectory;
                marker = $"Setting Extensions Path to={extensionsPath}";
                LogIt($"Info: {marker}");

                ISimioProject project = null;
                try
                {
                    Cursor.Current = Cursors.WaitCursor;
                    SimioProjectFactory.SetExtensionsPath(extensionsPath);

                    project = LoadProject(projectFullPath, out explanation);
                    if (project == null)
                        return false;
                }
                catch (Exception ex)
                {
                    explanation = $"Cannot load from {projectFullPath}. Err={ex}";
                    return false;
                }
                finally
                {
                    Cursor.Current = Cursors.Default;
                }

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
}
