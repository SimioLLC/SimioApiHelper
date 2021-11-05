using System;
using System.IO;
using SimioAPI;
using System.Threading.Tasks;
using SimEngineLibrary;
using System.Collections.Generic;

namespace RunSimioScheduleConsole
{
    class Program
    {
        static ISimioProject _simioProject;
        static void Main(string[] args)
        {
            // Open project
            string[] warnings;
            try
            {
                string extensionsPath = System.AppDomain.CurrentDomain.BaseDirectory;
                Logit($"Info: Starting. Default ExtensionsPath={extensionsPath}.");
                SimioProjectFactory.SetExtensionsPath(extensionsPath);
                Logit($"Info: ExtensionsPath Set successfully.");

                Logit("Read Command Line Settings");
                if (args.Length == 0)
                    throw new Exception("Project Path And File Must Be Specified In First Command Argument");

                // set parameters
                string projectPathAndFile = args[0];
                bool saveModelAfterRun = false;
                string modelName = "Model";
                string experimentName = "";

                if ( !File.Exists(projectPathAndFile))
                    throw new ApplicationException($"Cannot find SimioProject file at={projectPathAndFile}");

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
                    throw new Exception("Project Not Found : " + projectPathAndFile);

                string projectFolder = Path.GetDirectoryName(projectPathAndFile);

                Logit($"Info: Project Name={projectPathAndFile} Model={modelName} Experiment={experimentName} SaveAfterRun={saveModelAfterRun}");

                // Test if experiment can be done.
                string simpleTestProjectFullpath = Path.Combine(projectFolder, "LicenseExperimentTest.spfx");
                if ( File.Exists(simpleTestProjectFullpath))
                {
                    Logit($"Info: Testing license with Project=[{simpleTestProjectFullpath}]");

                    try
                    {
                        Logit($"Info: Loading License Project=[{simpleTestProjectFullpath}]");
                        ISimioProject simioProject = SimioProjectFactory.LoadProject(simpleTestProjectFullpath, out warnings);

                        Logit($"Info: Loaded Project={simpleTestProjectFullpath}");

                        if (!SimEngineHelpers.RunModelExperiment(simioProject, "", modelName, experimentName,
                            out string explanation))
                        {
                            Logit($"Error: Cannot Run Model={modelName} Experiment={experimentName}");
                        }
                        Logit($"Info: Run Ended for Project={simpleTestProjectFullpath}");

                    }
                    catch (Exception ex)
                    {
                        Logit($"LicenseTest: Cannot Run Simple Experiment. Err={ex.Message}");
                    }


                }

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
                    Logit("Starting Experiment");

                    string savePathAndFile = "";
                    if (saveModelAfterRun)
                        savePathAndFile = projectPathAndFile;

                    List<string> warningList = new List<string>();

                    if (!SimEngineHelpers.RunModelExperiment( model, experimentName, out string explanation))
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
                Logit($"RunError={ex.Message}");
            }
        }

        private static void Logit(string msg)
        {
            string fullMsg = $"{DateTime.Now.ToString("HH:mm:ss.ff")}: {msg}";
            Console.WriteLine(fullMsg);
            System.Diagnostics.Trace.WriteLine(fullMsg);
        }


    }
}
