using SimEngineLibrary;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimEngineController
{
    /// <summary>
    /// An example of a SimEngine 'runner' that draws from a FIFO queue (implemented
    /// as a folder of time-ordered request files, and executes
    /// based on those requests.
    /// It requires the location of the DLLs to use (the extensions folder)
    /// and a location (folder) for project files (that the request files implicitly reference).
    /// The requests are moved to either a Success or Failure folder when done.
    /// </summary>
    class Program
    {
        static void Main(string[] args)
        {
            string marker = "Begin.";

            try
            {

                string explanation = "";

                string requestsFolder = Properties.Settings.Default.RequestsPath;
                if (!Directory.Exists(requestsFolder))
                {
                    Logit($"Requests Storage folder={requestsFolder} does not exist.");
                    Environment.Exit(1003);
                }

                string extensionsPath = Properties.Settings.Default.ExtensionsPath;
                if (!Directory.Exists(extensionsPath))
                {
                    Logit($"ExtensionsPath for Simio processing={extensionsPath} does not exist.");
                    Environment.Exit(1005);
                }

                string projectsFolder = Properties.Settings.Default.ProjectsPath;
                if (!Directory.Exists(projectsFolder))
                {
                    Logit($"Projects Storage folder={projectsFolder} does not exist.");
                    Environment.Exit(1007);
                }

                bool done = false;
                marker = $"Requests={requestsFolder}. DLLs={extensionsPath} Projects={projectsFolder}";

                string successFolder = Path.Combine(requestsFolder, "Success");
                if (!Directory.Exists(successFolder))
                    Directory.CreateDirectory(successFolder);

                string failureFolder = Path.Combine(requestsFolder, "Failure");
                if (!Directory.Exists(failureFolder))
                    Directory.CreateDirectory(failureFolder);

                Logit($"Starting Loop.");

                while (!done)
                {
                    // Requests are queued by time and ID.
                    SimEngineRequest request = SimEngineRequestHelper.GetNextRequest(requestsFolder, out explanation);
                    if (request != null)
                    {
                        // Now we have the request. Figure out what to do.
                        Logit($"Processing request. Action={request.Action} Project={request.ProjectFilename}");

                        // Move to "running" folder.

                        // Run the project
                        string projectFullPath = Path.Combine(projectsFolder, request.ProjectFilename);
                        if ( !File.Exists(projectFullPath))
                        {
                            // Move to failure
                            explanation = $"No ProjectFile={projectFullPath}";
                            goto FinishUp;
                        }

                        switch (request.Action.ToUpper())
                        {
                            case "PLAN":
                                {
                                    // Run the project
                                    if (!SimEngineHelpers.RunProjectPlan(extensionsPath, projectFullPath, request.ActionArguments, out explanation))
                                    {
                                        request.RunResults = $"Error={explanation}.";
                                        // Move to Failure
                                    }
                                    else
                                    {
                                        request.RunResults = $"Ran Project={request.ProjectFilename} successfully.";
                                        // Move to Success
                                    }

                                }
                                break;

                            case "EXPERIMENT":
                                {

                                    if (!SimEngineHelpers.RunProjectExperiment(extensionsPath, projectFullPath, request.ActionArguments, out explanation))
                                    {
                                        request.RunResults = $"Error={explanation}.";
                                        // Move to Failure
                                    }
                                    else
                                    {
                                        request.RunResults = $"Ran Project={request.ProjectFilename} successfully.";
                                        // Move to Success
                                    }
                                }
                                break;

                                // Alter congifiguration, such as location of project
                            case "CONFIGURATION":
                                {
                                    // Todo:
                                }
                                break;

                            case "STATUS":
                                {
                                    //Todo: Given the ID, return a status
                                }
                                break;

                            default:
                                Logit($"Unknown ProjectAction={request.Action}");
                                break;

                        } // switch


                    FinishUp:

                        string source = request.RequestPath;
                        string fn = Path.GetFileName(source);

                        if (string.IsNullOrEmpty(explanation))
                        {
                            File.Move(source, Path.Combine(successFolder, fn));
                        }
                        else
                        {
                            File.Move(source, Path.Combine(failureFolder, fn));
                        }

                    }
                    else
                    {
                        Logit($"Error Getting Requeset File from={requestsFolder}. Err={explanation}");
                    }

                    // Take a break
                    System.Threading.Thread.Sleep(1000);
                }; // while

            }
            catch (Exception ex)
            {
                Logit($"Marker={marker}. Failure={ex.Message}");
                Environment.Exit(1013);
            }


        } // main


        private static void Logit(string msg)
        {
            Console.WriteLine(msg);
        }

    }
}
