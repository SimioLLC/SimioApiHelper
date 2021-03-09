using SimEngineInterfaceHelpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace SimEngineInterfaceFileDrop
{
    /// <summary>
    /// This controller looks for *.txt files
    /// The lines of the file are formatted as follows:
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

            Logit($"Starting FileDrop SimEngine Interface. DropFolder={dropFolder} RequestsFolder={requestsFolder}");
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
                    Logit($"Info: processing file={filepath}");
                    try
                    {
                        List<SimEngineRequest> requestList = new List<SimEngineRequest>();
                        if (!ParseRawRequestFile(filepath, requestList, out explanation))
                        {
                            explanation = $"Bad Request. Err={explanation}";
                            goto DoneWithRequest;
                        }

                        Logit($"Info: Found {requestList.Count} Requests.");
                        foreach (SimEngineRequest request in requestList)
                        {
                            Logit($"Info: Putting Request={request}");
                            if (!SimEngineRequestHelper.PutRequest(requestsFolder, request, out explanation))
                            {
                                explanation = $"Cannot Store Request. Err={explanation}";
                                goto DoneWithRequest;
                            }
                        }

                    DoneWithRequest:;
                        string filename = Path.GetFileName(filepath);

                        if ( string.IsNullOrEmpty(explanation) )
                        {
                            Logit($"Info: Success with File={filepath}");
                            string targetFile = Path.Combine(successPath, filename);
                            if (File.Exists(targetFile))
                                File.Delete(targetFile);
                            File.Move(filepath, targetFile);
                        }
                        else
                        {
                            Logit($"Failure with File={filepath}. Err={explanation}");
                            string targetFile = Path.Combine(failurePath, filename);
                            if (File.Exists(targetFile))
                                File.Delete(targetFile);
                            File.Move(filepath, targetFile);
                        }

                    }
                    catch (Exception ex)
                    {
                        Alert($"Err={ex.Message}");
                    }
                    System.Threading.Thread.Sleep(10); // guarantees unique filenames
                } // foreach file


                System.Threading.Thread.Sleep(500);

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
        private static bool ParseRawRequestFile(string fileDropFolderPath, List<SimEngineRequest> requestsList, out string explanation)
        {
            explanation = "";

            if ( requestsList == null )
            {
                explanation = $"RequestsList cannot be null";
                return false;
            }

            requestsList.Clear();
            StringBuilder sbComments = new StringBuilder();

            try
            {
                string[] lines = File.ReadAllLines(fileDropFolderPath);
                foreach (string line in lines)
                {
                    string trimmedLine = line.Trim();
                    if (string.IsNullOrEmpty(trimmedLine))
                        goto GetNextLine;

                    if (trimmedLine.StartsWith("#"))
                    {
                        sbComments.AppendLine(trimmedLine);
                        goto GetNextLine;
                    }

                    string[] argTokens = line.Trim().Split(',');
                    int lineNbr = 0;
                    string action = "";
                    string project = "";
                    List<RequestArgument> argsList = new List<RequestArgument>();

                    foreach (string argToken in argTokens.ToList())
                    {
                        lineNbr++;
                        string[] pairTokens = argToken.Split('=');
                        if (pairTokens.Length != 2)
                        {
                            explanation = $"Line#={lineNbr} Pair={argToken} is missing '='";
                            return false;
                        }

                        string key = pairTokens[0].ToLower();

                        switch (key)
                        {
                            case "action":
                                {
                                    action = pairTokens[1];
                                }
                                break;
                            case "project":
                                {
                                    project = pairTokens[1];
                                }
                                break;

                            default:
                                {
                                    RequestArgument arg = new RequestArgument(pairTokens[0], pairTokens[1]);
                                    argsList.Add(arg);
                                }
                                break;
                        }


                    } // foreach argPair

                    if ( !string.IsNullOrEmpty(action) && !string.IsNullOrEmpty(project) )
                    {
                        SimEngineRequest request = new SimEngineRequest(action, project, argsList);
                        requestsList.Add(request);
                    }
                    else
                    {
                        explanation = $"Invalid Request at Line={lineNbr}. No Action or Project specified.";
                        return false;
                    }


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
            Console.WriteLine($"{DateTime.Now.ToString("HH:mm:ss.ff")}: {msg}");
        }
        private static void Logit(string msg)
        {
            Console.WriteLine($"{DateTime.Now.ToString("HH:mm:ss.ff")}: {msg}");
        }
    }
}

