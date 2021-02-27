using System;
using System.IO;
using SimioAPI;
using System.Threading.Tasks;
using SimEngineLibrary;

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
                Logit("Start");
                string extensionsPath = System.AppDomain.CurrentDomain.BaseDirectory;
                SimioProjectFactory.SetExtensionsPath(extensionsPath);

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

                Logit($"Project Name={projectPathAndFile} Model={modelName} Experiment={experimentName} SaveAfterRun={saveModelAfterRun}");

                // Open project file.
                Logit($"Loading Model=[{modelName}]");
                _simioProject = SimioProjectFactory.LoadProject(projectPathAndFile, out warnings);

                // Run schedule and save for existing events.
                var model = _simioProject.Models[modelName];
                if (model == null)
                {
                    Logit($"Model={modelName} Not Found In Project");
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

                    if (!SimEngineHelpers.RunModelExperiment(extensionsPath, projectPathAndFile, savePathAndFile, modelName, experimentName, 
                            out string explanation))
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
