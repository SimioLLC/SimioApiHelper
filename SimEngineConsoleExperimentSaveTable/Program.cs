using System;
using System.IO;
using SimioAPI;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace SimEngineConsoleExperimentSaveTable
{
    /// <summary>
    /// This project demonstrates how the Experiment API can be interacted with to 
    /// create a tab-delimited CSV file of the results.
    /// Look particularly at the end-of-scenario events to see how the response results are treated.
    /// It also does a Save project at the end, assuming the user has the license to do so.
    /// 
    /// </summary>
    class Program
    {
        static ISimioProject _simioProject;
        static string marker = "Begin.";

        static void Main(string[] args)
        {
            // Open project
            string[] warnings;
            try
            {
                string extensionsPath = System.AppDomain.CurrentDomain.BaseDirectory;
                string programFolder = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);

                extensionsPath = Path.Combine(programFolder, "Simio LLC", "Simio", "UserExtensions");
                Logit($"Starting. Default ExtensionsPath={extensionsPath}.");
                SimioProjectFactory.SetExtensionsPath(extensionsPath);
                Logit($"Info: ExtensionsPath Set successfully.");

                Logit("Read Command Line Settings");
                if (args.Length == 0)
                {
                    Logit("Project Path And File Must Be Specified In First Command Argument");
                    throw new Exception(marker);
                }

                // set parameters
                string projectPathAndFile = args[0];
                bool saveModelAfterRun = false;
                string modelName = "Model";
                string experimentName = "";

                if (!File.Exists(projectPathAndFile))
                {
                    Logit($"Cannot find SimioProject file at={projectPathAndFile}");
                    throw new ApplicationException(marker);
                }

                if (args.Length >= 2)
                {
                    saveModelAfterRun = Convert.ToBoolean(args[1]);
                }
                if (args.Length >= 3)
                {
                    modelName = args[2];
                }
                if (args.Length >= 4)
                {
                    experimentName = args[3];
                }

                // If File Not Exist, Throw Exeption
                if (File.Exists(projectPathAndFile) == false)
                {
                    Logit("Project Not Found : " + projectPathAndFile);
                    throw new Exception("Looking for Simio Project");
                }

                string projectFolder = Path.GetDirectoryName(projectPathAndFile);

                Logit($"Info: Project Name={projectPathAndFile} Model={modelName} Experiment={experimentName} SaveAfterRun={saveModelAfterRun}");


                // Open project file.
                Logit($"Loading Project=[{projectPathAndFile}]");
                _simioProject = SimioProjectFactory.LoadProject(projectPathAndFile, out warnings);

                // Run schedule and save for existing events.
                var model = _simioProject.Models[modelName];
                if (model == null)
                {
                    throw new ApplicationException($"Model={modelName} Not Found In Project={projectPathAndFile}");

                }
                else
                {
                    if (model.Experiments == null)
                        throw new ApplicationException($"Model's Experiments collection is null.");

                    // Start Experiment
                    Logit("Info: Starting Experiment");

                    string savePathAndFile = "";
                    if (saveModelAfterRun)
                        savePathAndFile = projectPathAndFile;

                    List<string> warningList = new List<string>();

                    string resultsPath = Path.Combine(projectFolder, "results.csv");
                    if (!SimEngineHelpers.RunModelExperiment(model, experimentName, resultsPath, out string explanation))
                    {
                        throw new ApplicationException(explanation);
                    }
                    else
                    {
                        Logit( $"Info: Model={modelName} Experiment={experimentName} performed the actions successfully. Check the logs for more information.");
                    }


                    if (saveModelAfterRun)
                    {
                        Logit("Save Project After Experiment Run");
                        SimioProjectFactory.SaveProject(_simioProject, projectPathAndFile, out warnings);
                    }

                    Logit("End");
                    Console.ReadLine();
                }
            }
            catch (Exception ex)
            {
                Logit($"RunError. Marker={marker} Err={ex.Message}");
            }
        }

        private static void Logit(string msg)
        {
            marker = msg;
            string fullMsg = $"{DateTime.Now:HH:mm:ss.ff}: {msg}";
            Console.WriteLine(fullMsg);
            System.Diagnostics.Trace.WriteLine(fullMsg);
        }


    }
}
