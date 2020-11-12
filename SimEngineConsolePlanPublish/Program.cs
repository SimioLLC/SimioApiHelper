using System;
using System.IO;
using SimioAPI;
using SimioServerConnector;
using System.Threading.Tasks;

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
                // We'll assume our DLLs have been placed at the same location as this EXE
                string extensionsPath = System.AppDomain.CurrentDomain.BaseDirectory;
                SimioProjectFactory.SetExtensionsPath(extensionsPath);

                Console.WriteLine("Read Command Line Settings");
                if (args.Length == 0) 
                    throw new Exception("Project Path And File Must Be Specified In First Command Argument");

                // set parameters
                string projectPathAndFile = args[0];
                bool runRiskAnalysis = false;
                bool saveModelAfterRun = false;
                bool publishPlanAfterRun = false;
                string modelName = "Model";
                string publishName = "";

                // set project file
                if (args.Length >= 2)
                {
                    runRiskAnalysis = Convert.ToBoolean(args[1]);
                }
                if (args.Length >= 3)
                {
                    saveModelAfterRun = Convert.ToBoolean(args[2]);
                }
                if (args.Length >= 4)
                {
                    publishPlanAfterRun = Convert.ToBoolean(args[3]);
                }
                if (args.Length >= 5)
                {
                    modelName = args[4];
                }
                if (args.Length >= 6)
                {
                    publishName = args[5];
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
                    if ( model.Plan == null )
                        throw new ApplicationException($"Model's Plan is null. Do you have the correct Simio licensing?");

                    // Start Plan
                    Console.WriteLine("Starting Plan");
                    RunPlanOptions options = new RunPlanOptions();
                    options.AllowDesignErrors = false;

                    model.Plan.RunPlan(options);
                    if (runRiskAnalysis)
                    {
                        Console.WriteLine("Plan Finished...Starting Analyze Risk");
                        model.Plan.RunRiskAnalysis();
                    }

                    if (saveModelAfterRun)
                    {
                        Console.WriteLine("Save Project After Schedule Run");
                        SimioProjectFactory.SaveProject(_simioProject, projectPathAndFile, out warnings);
                    }

                    // Publish the plan to portal after Run. 
                    // This (of course) requires the URL of your Portal, plus an access token (PAT) for security.
                    if (publishPlanAfterRun)
                    {
                        Console.WriteLine("Info: PublishPlan");
                        string url = "https://test.internal.SimioPortal.com/";
                        string pat =    "eyJ1IjoiZGFuX2hvdWNrQGhvdG1haWwuY29tIiwidCI6Ik9zeEp1bmtqdnBPaHcxR2RlUk9INjBSTUcyVm51SFpXSFBQbmpYMVNHREo3cjFkT0pMWVZhQXpFeHdzM0RvVWlIWU41Tjd4YUFhZndVNmNFekVuN1FBPT0ifQ ==";
                        var pub = DoPublish(url, projectPathAndFile, pat, modelName, publishName, "Scheduling Discrete Part Production");
                        pub.Wait();
                    }


                    Console.WriteLine("End");
                    Console.ReadLine();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("RunError=" + ex.Message);
            }
        }

        static async Task DoPublish(string url, string file, string pat, string modelName, string publishName, string publishDescription)
        {
            var factory = new ConnectionFactory();
            using (var connection = factory.CreateConnection(new Uri(url), pat))
            {
                using (var readStream = System.IO.File.OpenRead(file))
                {
                    IPublishModelResult result = await connection.PublishModel(readStream, modelName, publishName, publishDescription);
                    if (result.Succeeded)
                    {
                        Console.WriteLine($"Model published to url: {result.PublishUrl}");
                    }
                    else
                    {
                        Console.WriteLine($"Error publishing: {result.Error ?? String.Empty}");
                    }
                }
            }
        }

    }
}
