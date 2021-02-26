using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SimEngineLibrary;


namespace SimEngineInterfaceFileDrop
{
    /// <summary>
    /// This controller looks for *.txt files
    /// That are formatted as follows:
    /// 1. Lines beginning with '#' are comments
    /// 2. The data line is {projectName.extension},{argument,...}
    /// where arguments are comma-delimited and are key=value pairs
    /// Example:
    /// # This is a test experiment
    /// experiment,DiscretePartsProduction.spfx,model=model,experiment=experiment1,saveFolder=c:\test\savefolder
    /// </summary>
    class Program
    {
        static void Main(string[] args)
        {
            string dropFolder = Properties.Settings.Default.FileDropFolder;
            string requestsFolder = Properties.Settings.Default.RequestsFolder;

            if (!Directory.Exists(dropFolder))
            {
                Alert($"DropFolder={dropFolder} does not exist.");
                Environment.Exit(1);
            }

            if (!Directory.Exists(requestsFolder))
            {
                Alert($"RequestsFolder={requestsFolder} does not exist.");
                Environment.Exit(1);
            }

            StartFileDropServer(dropFolder, requestsFolder);
        }

        private static void StartFileDropServer(string fileDropFolder, string requestsFolder)
        {
            string explanation = "";

            string runningPath = SimEngineRequestHelper.GetRunningFolder(fileDropFolder);
            if (!Directory.Exists(runningPath))
                Directory.CreateDirectory(runningPath);

            string successPath = SimEngineRequestHelper.GetSuccessFolder(fileDropFolder);
            if (!Directory.Exists(successPath))
                Directory.CreateDirectory(successPath);

            string failurePath = SimEngineRequestHelper.GetFailureFolder(fileDropFolder);
            if (!Directory.Exists(failurePath))
                Directory.CreateDirectory(failurePath);

            while (true)
            {
                foreach (string filepath in Directory.GetFiles(fileDropFolder, "*.txt"))
                {

                    try
                    {
                        string action = "";
                        string projectFilename = "";
                        List<RequestArgument> argList = new List<RequestArgument>();
                        if (!ParseRawRequestFile(filepath, out action, argList, out explanation))
                        {
                            explanation = $"Bad Request. Err={explanation}";
                            goto DoneWithRequest;
                        }

                        SimEngineRequest newRequest = new SimEngineRequest(projectFilename, action, argList);
                        if (!SimEngineRequestHelper.PutRequest(requestsFolder, newRequest, out explanation))
                        {
                            explanation = $"Cannot Store Request. Err={explanation}";
                            goto DoneWithRequest;
                        }

                    DoneWithRequest:;
                        string filename = Path.GetFileName(filepath);

                        if ( string.IsNullOrEmpty(explanation) )
                        {
                            Logit($"Success with File={filepath}");
                            File.Move(filepath, Path.Combine(successPath, filename));
                        }
                        else
                        {
                            Logit($"Failure with File={filepath}. Err={explanation}");
                            File.Move(filepath, Path.Combine(failurePath, filename));
                        }

                    }
                    catch (Exception ex)
                    {
                        Alert($"Err={ex.Message}");
                    }
                    System.Threading.Thread.Sleep(10); // guarantees unique filenames
                } // foreach file


                System.Threading.Thread.Sleep(1000);

            };

        }

        /// <summary>
        /// A raw request file has been place in the requests area. A SimEngine request file is built.
        /// The format of the raw file is:
        /// * Lines beginning with # are comments
        /// * lines that are empty after trimming are ignored
        /// * lines beginning with "action:" are followed by action name and argument
        /// * actions are a comma list, with first token the action name (e.g. Experiment)
        ///   and other tokens in key=value pairs. E.g. Experiment,Project=MyProject,Experiment=MyExperiment
        ///   or "Status,ID=MyID"
        /// </summary>
        /// <param name="fileDropFolderPath"></param>
        /// <param name="action"></param>
        /// <param name="projectFilename"></param>
        /// <param name="argsList"></param>
        /// <param name="explanation"></param>
        /// <returns></returns>
        private static bool ParseRawRequestFile(string fileDropFolderPath, out string action, List<RequestArgument> argsList, out string explanation)
        {
            explanation = "";
            action = "";

            if ( argsList == null )
            {
                explanation = $"ArgumentList cannot be null";
                return false;
            }

            argsList.Clear();

            try
            {
                string[] lines = File.ReadAllLines(fileDropFolderPath);
                foreach (string line in lines)
                {
                    string trimmedLine = line.Trim();
                    if (string.IsNullOrEmpty(trimmedLine))
                        goto GetNextLine;

                    if (trimmedLine.StartsWith("#"))
                        goto GetNextLine;

                    string[] argTokens = line.Trim().Split(',');
                    int nn = 0;
                    foreach (string argToken in argTokens.ToList())
                    {
                        nn++;
                        if (nn == 1) // First one is an action
                        {
                            action = argToken;
                        }
                        else 
                        {
                            string[] pairTokens = argToken.Split('=');
                            if (pairTokens.Length != 2)
                            {
                                explanation = $"Pair={argToken} is missing '='";
                                return false;
                            }

                            RequestArgument arg = new RequestArgument(pairTokens[0], pairTokens[1]);
                            argsList.Add(arg);

                        }
                    } // foreach argPair


                    GetNextLine:;
                }

                return true;
            }
            catch (Exception ex)
            {
                explanation = $"File={fileDropFolderPath} Err={ex.Message}";
                return false;
            }
        }

        private static void Alert(string msg)
        {
            Console.WriteLine(msg);
        }
        private static void Logit(string msg)
        {
            Console.WriteLine(msg);
        }
    }
}

