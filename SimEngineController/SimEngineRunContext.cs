using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimEngineController
{
    /// <summary>
    /// Everything the SimEngine controller needs to complete it tasks.
    /// </summary>
    public class SimEngineRunContext
    {
        /// <summary>
        /// Full path to where the simio projects live 
        /// </summary>
        public string ProjectFolderPath { get; set; }

        /// <summary>
        /// Full path to the extensions folder. 
        /// All the user-define DLLs must reside here
        /// </summary>
        public string ExtensionsFolderPath { get; set; }

        /// <summary>
        /// A context object that is used by some of the SimioEngine Helpers methods
        /// </summary>
        public class SimEngineRunContext
        {
            /// <summary>
            /// Full path to where the log files are stored.
            /// Defaults to {applicationData}\SimEngine\Logs
            /// </summary>
            private string LogsFolderPath { get; set; }

            /// <summary>
            /// Full path to the folder holding the Simio Project files (e.g. spfx, ...)
            /// Defaults to {applicationData}\SimEngine\Projects
            /// </summary>
            public string ProjectsFolderPath { get; set; }

            /// <summary>
            /// Full path to the folder holding the Simio Project files (e.g. spfx, ...)
            /// Defaults to {applicationData}\SimEngine\ProjectSaves
            /// </summary>
            public string ProjectSavesFolderPath { get; set; }

            /// <summary>
            /// Points to where all the supporting files (EXE, DLL, etc. live)
            /// Defaults to {applicationData}\SimEngine\Extensions
            /// </summary>
            public string ExtensionsFolderPath { get; set; }


            public event EventHandler<ExperimentReplicationEndedEventArgs> ExperimentReplicationEnded;

            protected virtual void OnExperimentReplicationCompleted(ExperimentReplicationEndedEventArgs e)
            {
                ExperimentReplicationEnded?.Invoke(this, e);
            }


            public event EventHandler<ExperimentScenarioEndedEventArgs> ExperimentScenarioEnded;
            protected virtual void OnExperimentScenarioCompleted(ExperimentScenarioEndedEventArgs e)
            {
                ExperimentScenarioEnded?.Invoke(this, e);
            }

            public event EventHandler<ExperimentRunCompletedEventArgs> ExperimentRunCompleted;
            protected virtual void OnExperimentRunCompleted(ExperimentRunCompletedEventArgs e)
            {
                ExperimentRunCompleted?.Invoke(this, e);
            }

            /// <summary>
            /// Constructor.
            /// </summary>
            /// <param name="extensionsPath"></param>
            public SimEngineContext(string projectsPath, string extensionsPath, string logsPath, List<RequestArgument> actionArguments)
            {

                if (!Initialize(projectsPath, extensionsPath, logsPath, out string explanation))
                    throw new ApplicationException($"Initializing SimioEngine Context. Err={explanation}");

                this.ProjectsFolderPath = projectsPath;
                this.ExtensionsFolderPath = extensionsPath;
                this.LogsFolderPath = logsPath;

                this.ActionArgumentList = actionArguments ?? throw new ApplicationException($"ActionArguments cannot be null");

            }

            /// <summary>
            /// Constructor.
            /// </summary>
            /// <param name="extensionsPath"></param>
            public SimEngineContext(string projectsPath, string extensionsPath, string logsPath)
            {

                if (!Initialize(projectsPath, extensionsPath, logsPath, out string explanation))
                    throw new ApplicationException($"Initializing SimioEngine Context. Err={explanation}");

            }

            private void LogIt(string msg)
            {

                if (MyLogger == null)
                    return;

                MyLogger.LogIt(EnumLogFlags.All, $"{msg}");
            }



            /// <summary>
            /// Constructor.
            /// </summary>
            public SimEngineRunContext()
        {
            try
            {
                string marker = "Begin.";

                try
                {

                    string explanation = "";

                    string requestsFolder = Properties.Settings.Default.RequestsPath;
                    requestsFolder = SimEngineRunHelpers.ResolvePath(requestsFolder);

                    if (!Directory.Exists(requestsFolder))
                    {
                        Logit($"Requests Storage folder={requestsFolder} does not exist.");
                        Environment.Exit(1003);
                    }
                    Logit($"Info: The Request folder={requestsFolder} was found.");

                    string extensionsPath = Properties.Settings.Default.ExtensionsPath;
                    extensionsPath = SimEngineRunHelpers.ResolvePath(extensionsPath);
                    if (!Directory.Exists(extensionsPath))
                    {
                        Logit($"ExtensionsPath for Simio processing={extensionsPath} does not exist.");
                        Environment.Exit(1005);
                    }
                    Logit($"Info: The Extensions folder={extensionsPath} was found.");

                    string projectsFolder = Properties.Settings.Default.ProjectsPath;
                    projectsFolder = SimEngineRunHelpers.ResolvePath(projectsFolder);
                    if (!Directory.Exists(projectsFolder))
                    {
                        Logit($"Projects Storage folder={projectsFolder} does not exist.");
                        Environment.Exit(1007);
                    }
                    Logit($"Info: The Projects folder={projectsFolder} was found.");

                    string logFolder = Properties.Settings.Default.ProjectsPath;
                    logFolder = SimEngineRunHelpers.ResolvePath(logFolder);
                    if (!Directory.Exists(logFolder))
                    {
                        Logit($"Projects Storage folder={logFolder} does not exist.");
                        Environment.Exit(1007);
                    }
                    Logit($"Info: The Projects folder={logFolder} was found.");

                    bool done = false;
                    marker = $"Requests={requestsFolder}. ExtensionDLLs={extensionsPath} Projects={projectsFolder} Logs={logFolder}";
                    Logit($"Info: marker");

                    string successFolder = Path.Combine(requestsFolder, "Success");
                    if (!Directory.Exists(successFolder))
                    {
                        Directory.CreateDirectory(successFolder);
                        Logit($"Info: The Success folder={successFolder} was created.");
                    }

                    string failureFolder = Path.Combine(requestsFolder, "Failure");
                    if (!Directory.Exists(failureFolder))
                    {
                        Directory.CreateDirectory(failureFolder);
                        Logit($"Info: The Failure folder={successFolder} was created.");
                    }

                    string runningFolder = Path.Combine(requestsFolder, "Running");
                    if (!Directory.Exists(runningFolder))
                    {
                        Directory.CreateDirectory(runningFolder);
                        Logit($"Info: The Running folder={runningFolder} was created.");
                    }



                }
                catch (Exception ex)
                {
                    throw new ApplicationException($"Cannot build SimEngine Context. Err={ex}");
                }


            }
            catch (Exception ex)
            {
                throw new ApplicationException($"Cannot build SimEngine Context. Err={ex}");

            }

        }
            /// <summary>
            /// Initialize, which means setting the ExtensionsPath and loading the project.
            /// If there are warnings, they are placed in LoadWarningsList.
            /// </summary>
            /// <param name="extensionsPath"></param>
            /// <param name="projectFullPath"></param>
            /// <param name="explanation"></param>
            /// <returns></returns>
            public bool Initialize(string projectsPath, string extensionsPath, string logsPath, out string explanation)
            {
                explanation = "";
                string marker = "Checking extension and project paths.";

                try
                {
                    if (Directory.Exists(projectsPath) == false)
                    {
                        explanation = $"ProjectsFolderPath={projectsPath} not found.";
                        return false;
                    }
                    this.ProjectsFolderPath = projectsPath;

                    if (Directory.Exists(extensionsPath) == false)
                    {
                        explanation = $"ExtensionsPath={extensionsPath} not found.";
                        return false;
                    }
                    this.ExtensionsFolderPath = extensionsPath;

                    if (Directory.Exists(logsPath) == false)
                    {
                        explanation = $"LogsFolderPath={logsPath} not found.";
                        return false;
                    }
                    this.LogsFolderPath = logsPath;

                    marker = $"Setting extensions path={extensionsPath}";
                    SimioProjectFactory.SetExtensionsPath(extensionsPath);

                    return true;

                }
                catch (Exception ex)
                {
                    explanation = $"Marker={marker}. Failed to initialize SimioEngine Context. ExtensionsPath={extensionsPath} Err={ex.Message}";
                    return false;
                }

            }



            private void Logit(string msg)
        {
            LoggertonHelpers.Loggerton.Instance.LogIt(LoggertonHelpers.EnumLogFlags.All, msg);
        }
    }
}
