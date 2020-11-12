using SimEngineLibrary;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimEngineRunner
{
    class Program
    {
        static void Main(string[] args)
        {

            try
            {

                string explanation = "";

                string requestsFolder = Properties.Settings.Default.RequestsFolder;
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

                bool done = false;

                string successFolder = Path.Combine(requestsFolder, "Success");
                if (!Directory.Exists(successFolder))
                    Directory.CreateDirectory(successFolder);

                string failureFolder = Path.Combine(requestsFolder, "Failure");
                if (!Directory.Exists(failureFolder))
                    Directory.CreateDirectory(failureFolder);

                while (!done)
                {

                    bool isSuccess = false;

                    ProjectRequest request = ProjectRequestHelper.GetNextRequest(requestsFolder, out explanation);
                    if (request != null)
                    {
                        // Now we have the request. Figure out what to do.
                        Logit($"Processing request={request.ProjectPath}");

                        switch (request.ProjectAction.ToUpper())
                        {

                            case "EXPERIMENT":
                                {

                                    // Run the project
                                    if (!SimEngineHelpers.RunAndSaveProjectExperiment(extensionsPath, request.ProjectPath, request.ActionArguments, out explanation))
                                    {
                                        Logit($"Error={explanation}.");
                                    }
                                    else
                                    {
                                        Logit($"Ran Project={request.ProjectPath} successfully.");
                                        isSuccess = true;
                                    }
                                }
                                break;

                            default:
                                Logit($"Unknown ProjectAction={request.ProjectAction}");
                                break;

                        } // switch

                        if (isSuccess)
                        {
                            string fn = Path.GetFileName(request.RequestPath);
                            string source = request.RequestPath;
                            File.Move(source, Path.Combine(successFolder, fn));
                        }
                        else
                        {
                            string fn = Path.GetFileName(request.RequestPath);
                            string source = request.RequestPath;
                            File.Move(source, Path.Combine(failureFolder, fn));
                        }


                    }
                    else // process error
                    {
                        Logit($"Process Error={explanation}");
                    }


                    // Take a break
                    System.Threading.Thread.Sleep(1000);
                }; // while

            }
            catch (Exception ex)
            {
                Logit($"Startup Failure={ex.Message}");
                Environment.Exit(1013);
            }


        } // main


        private static void Logit(string msg)
        {
            Console.WriteLine(msg);
        }

    }
}
