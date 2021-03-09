using SimEngineInterfaceHelpers;
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
    /// An example of a SimEngine 'controller' or 'runner' that draws from a FIFO queue (implemented
    /// as a folder of time-ordered request files, and continually executes those requests
    /// until no more exist, and then waits upon that folder for new requests.
    /// The startup arguments must include a location (folder) for the DLLs to use (the extensions folder).
    /// and a location (folder) for project files (that the request files implicitly reference).
    /// The requests with added status information are moved to either a Success or Failure sub-folder that
    /// is beneath the Requests folder when done.
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
                Logit($"Info: The Request folder={requestsFolder} was found.");

                string extensionsPath = Properties.Settings.Default.ExtensionsPath;
                if (!Directory.Exists(extensionsPath))
                {
                    Logit($"ExtensionsPath for Simio processing={extensionsPath} does not exist.");
                    Environment.Exit(1005);
                }
                Logit($"Info: The Extensions folder={extensionsPath} was found.");

                string projectsFolder = Properties.Settings.Default.ProjectsPath;
                if (!Directory.Exists(projectsFolder))
                {
                    Logit($"Projects Storage folder={projectsFolder} does not exist.");
                    Environment.Exit(1007);
                }
                Logit($"Info: The Projects folder={requestsFolder} was found.");

                bool done = false;
                marker = $"Requests={requestsFolder}. ExtensionDLLs={extensionsPath} Projects={projectsFolder}";
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

                Logit($"Starting Loop.");

                while (!done)
                {
                    // Requests are queued by time and ID.
                    SimEngineRequest request = SimEngineRequestHelper.GetNextRequest(requestsFolder, out explanation);
                    if (request != null)
                    {
                        // Now we have the request. Figure out what to do.
                        Logit($"Processing request. Action={request.Action} Project={request.ProjectFilename}");


                        // Run the project
                        string projectFullPath = Path.Combine(projectsFolder, request.ProjectFilename);
                        if ( !File.Exists(projectFullPath))
                        {
                            // Move to failure
                            explanation = $"No ProjectFile found at={projectFullPath}";
                            goto FinishUp;
                        }

                        // Move to "running" folder.
                        string runningPath = Path.Combine(runningFolder, Path.GetFileName(request.RequestPath));
                        if (File.Exists(runningPath))
                            File.Delete(runningPath);
                        File.Move(request.RequestPath, runningPath);


                        switch (request.Action.ToUpper())
                        {
                            case "PLAN":
                                {
                                    // Run the project
                                    if (!SimEngineHelpers.RunProjectPlan(extensionsPath, projectFullPath, request.ActionArguments, out explanation))
                                    {
                                        request.RunResults = $"Error with Request={request.ID} Project={request.ProjectFilename} Error={explanation}.";
                                        // Move to Failure
                                    }
                                    else
                                    {
                                        request.RunResults = $"Success with Request={request.ID} Project={request.ProjectFilename}";
                                        // Move to Success
                                    }

                                }
                                break;

                            case "EXPERIMENT":
                                {

                                    if (!SimEngineHelpers.RunProjectExperiment(extensionsPath, projectFullPath, request.ActionArguments, out explanation))
                                    {
                                        request.RunResults = $"Error with Request={request.ID} Project={request.ProjectFilename} Error={explanation}.";
                                        // Move to Failure
                                    }
                                    else
                                    {
                                        request.RunResults = $"Success with Request={request.ID} Project={request.ProjectFilename}";
                                        // Move to Success
                                    }
                                }
                                break;

                                // Alter congifiguration, such as location of project
                            case "CONFIGURATION":
                                {
                                    // Todo: Alter configurations at run-time
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
                        Logit($"Error Getting Request File from={requestsFolder}. Err={explanation}");
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
