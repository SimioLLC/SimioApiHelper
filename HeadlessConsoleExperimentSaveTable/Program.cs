using System;
using System.IO;
using SimioAPI;
using System.Threading.Tasks;
using HeadlessLibrary;

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
                Console.WriteLine("Start");
                string extensionsPath = System.AppDomain.CurrentDomain.BaseDirectory;
                SimioProjectFactory.SetExtensionsPath(extensionsPath);

                Console.WriteLine("Read Command Line Settings");
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

                Console.WriteLine("Project Name = " + projectPathAndFile);

                // Open project file.
                Console.WriteLine($"Loading Model=[{modelName}]");
                _simioProject = SimioProjectFactory.LoadProject(projectPathAndFile, out warnings);

                // Run schedule and save for existing events.
                var model = _simioProject.Models[modelName];
                if (model == null)
                {
                    Console.WriteLine("Model Not Found In Project");
                }
                else
                {
                    if (model.Experiments == null)
                        throw new ApplicationException($"Model's Experiments collection is null.");

                    // Start Experiment
                    Console.WriteLine("Starting Experiment");

                    if (!SimEngineHelpers.RunExperiment(extensionsPath, projectPathAndFile, modelName, experimentName, saveModelAfterRun,
                            out string explanation))
                    {
                        throw new ApplicationException(explanation);
                    }
                    else
                    {
                        Console.WriteLine( $"Info: Model={modelName} Experiment={experimentName} performed the actions successfully. Check the logs for more information.");
                    }


                    if (saveModelAfterRun)
                    {
                        Console.WriteLine("Save Project After Experiment Run");
                        SimioProjectFactory.SaveProject(_simioProject, projectPathAndFile, out warnings);
                    }

                    Console.WriteLine("End");
                    Console.ReadLine();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"RunError={ex.Message}");
            }
        }


    }
}
