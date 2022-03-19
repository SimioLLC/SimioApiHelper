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
    /// Assumptions:
    /// Simio is installed under Program Files (not x86)
    /// Command Line Arguments:
    /// 1. Full path to Project file (e.g. spfx)
    /// 2. Optional boolean to Save to project file after run
    /// 3. Optional Model name (default Model)
    /// 4. Optional Experiment name (default Experiment)
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
                    Logit("Project File (full path) must Be Specified In first command argument");
                    throw new Exception(marker);
                }

                // set parameters
                string projectFilenameFullpath = args[0];
                bool saveProjectAfterRun= false;
                string modelName = "Model"; // default
                string experimentName = "";

                string modelsFolderName = Path.GetDirectoryName(projectFilenameFullpath);
                string[] files = Directory.GetFiles(modelsFolderName);

                if (!File.Exists(projectFilenameFullpath))
                {
                    Logit($"Cannot find SimioProject file at={projectFilenameFullpath}");
                    throw new ApplicationException(marker);
                }

                if (args.Length >= 2)
                {
                    saveProjectAfterRun = Convert.ToBoolean(args[1]);
                    Logit($"2nd arg set SaveProjectAfterRun={saveProjectAfterRun}");
                }

                if (args.Length >= 3)
                {
                    modelName = args[2];
                    Logit($"3rd arg set Model={modelName}");
                }

                if (args.Length >= 4)
                {
                    experimentName = args[3];
                    Logit($"4th arg set Experiment={experimentName}");
                }

                // If File Not Exist, Throw Exeption

                string projectFolder = Path.GetDirectoryName(projectFilenameFullpath);

                Logit($"Info: Project Name={projectFilenameFullpath} "
                    + $"Model={modelName} Experiment={experimentName} SaveAfterRun={saveProjectAfterRun}");


                // Open project file. This can take a few seconds
                Logit($"Loading Project=[{projectFilenameFullpath}]");
                _simioProject = SimioProjectFactory.LoadProject(projectFilenameFullpath, out warnings);

                // Run schedule and save for existing events.
                var model = _simioProject.Models[modelName];
                if (model == null)
                {
                    throw new ApplicationException($"Model={modelName} Not Found In Project={projectFilenameFullpath}");
                }
                else
                {
                    if (model.Experiments == null)
                        throw new ApplicationException($"Model's Experiments collection is null.");

                    // Start Experiment
                    Logit("Info: Starting Experiment");

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


                    if (saveProjectAfterRun)
                    {
                        string directory = Path.GetDirectoryName(projectFilenameFullpath);
                        string fileExtension = Path.GetExtension(projectFilenameFullpath);
                        string filename = Path.GetFileNameWithoutExtension(projectFilenameFullpath) + "-Saved";

                        string saveProjectFullpath = Path.Combine(directory, $"{filename}{fileExtension}");

                        Logit($"Save Project After Experiment Run to={saveProjectFullpath}");
                        SimioProjectFactory.SaveProject(_simioProject, saveProjectFullpath, out warnings);
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
