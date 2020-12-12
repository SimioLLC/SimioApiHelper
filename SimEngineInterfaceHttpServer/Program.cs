

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SimEngineLibrary;
using SimpleHttpServer;
using SimpleHttpServer.Models;
using SimpleHttpServer.RouteHandlers;

namespace SimEngineInterfaceHttp
{
    /// <summary>
    /// Run a SimEngine controller that uses HTTP to get the reques
    /// </summary>
    class Program
    {

        public string Mode = "Null";

        static void Main(string[] args)
        {
            //log4net.Config.XmlConfigurator.Configure();

            int port = Properties.Settings.Default.SitePort;
            StartHttpServer(port);
        }

        private static void StartHttpServer(int port)
        {
            try
            {
                string projectsFolder = Properties.Settings.Default.ProjectsFolder;
                string queueFolder = Properties.Settings.Default.RequestsFolder;

                var route_config = new List<SimpleHttpServer.Models.Route>() {
                new Route {
                    Name = "Hello Handler",
                    UrlRegex = @"^/$",
                    Method = "GET",
                    Callable = (HttpRequest request) => {
                        return new HttpResponse()
                        {
                            ContentAsUTF8 = $"Hello from Simio HttpServer. Time is {DateTime.Now}."
                             + " Options are Simio/SimEngine/yourModelName",
                            ReasonPhrase = "OK",
                            StatusCode = "200"
                        };
                     }
                },
                new Route {
                    Name = "SimEngine Controller for Experiment",
                    UrlRegex = @"^/SimEngine/Experiment.*$",
                    Method = "GET",
                    Callable = (HttpRequest request) => {

                        string explanation = "";
                        string success = "";

                        Uri myUri = new Uri(request.Path);

                        List<KeyValuePair<string,string>> parameterList = new List<KeyValuePair<string, string>>();
                        if ( !ParseUrl(request.Path, parameterList, out explanation) || parameterList.Count == 0 )
                        {
                            explanation = $"Bad Request. Err={explanation}";
                            goto DoneWithRequest;
                        }

                        //string projectName = WebUtility.UrlDecode(tokens[3]);
                        //string arguments = WebUtility.UrlDecode(tokens[4]);

                        string projectName = parameterList.SingleOrDefault(rr => rr.Key == "project").Value;
                        if ( projectName == null )
                        {
                            explanation = $"argument 'project' must be specified.";
                            goto DoneWithRequest;
                        }

                        string projectPath = Path.Combine(projectsFolder, projectName);
                        if ( !File.Exists(projectPath))
                        {
                            explanation = $"Cannot find Project={projectPath}";
                            goto DoneWithRequest;
                        }

                        string modelName = parameterList.SingleOrDefault(rr => rr.Key == "model").Value; // default
                        if ( modelName == null )
                        {
                            explanation = $"argument 'model' must be specified.";
                            goto DoneWithRequest;
                        }

                        string experimentName = parameterList.SingleOrDefault(rr => rr.Key == "experiment").Value; // default
                        if ( experimentName == null )
                        {
                            explanation = $"argument 'experiment' must be specified.";
                            goto DoneWithRequest;
                        }

                        List<RequestArgument> argList = new List<RequestArgument>();
                        SimEngineRequest newRequest = new SimEngineRequest(projectPath, "Experiment", argList);
                        if ( !SimEngineRequestHelper.PutRequest(queueFolder, newRequest, out explanation))
                        {
                            explanation = $"Cannot Store Request. Err={explanation}";
                            goto DoneWithRequest;
                        }

                        DoneWithRequest:;

                        if ( !string.IsNullOrEmpty(explanation))
                        {
                            return new HttpResponse()
                            {
                                ContentAsUTF8 = $"{DateTime.Now}: SimEngine Request=[{request.Path}]. Error={explanation}",
                                ReasonPhrase = "ERROR",
                                StatusCode = "501"
                            };
                        }
                        else
                        {
                            return new HttpResponse()
                            {
                                ContentAsUTF8 = $"{DateTime.Now}: SimEngine Request=[{request.Path}]. Success={success}",
                                ReasonPhrase = "OK",
                                StatusCode = "200"
                            };

                        }
                    }

                     
                },
                new Route {
                    Name = "FileSystem Static Handler",
                    UrlRegex = @"^/Static/(.*)$",
                    Method = "GET",
                    Callable = new FileSystemRouteHandler()
                    {
                        BasePath = @"C:\Temp",
                        ShowDirectories=true }.Handle
                }
            };

                HttpServer httpServer = new HttpServer(port, route_config);

                Thread thread = new Thread(new ThreadStart(httpServer.Listen));
                thread.Start();

            }
            catch (Exception ex)
            {
                Alert($"Err={ex.Message}");
            }
        }

        /// <summary>
        /// Parse the URL to identify the arguments, which are
        /// returned as keyvalue pairs.
        /// The URL arguments follow the ampersand, and are of the form key=value.
        /// Both the key and value are trimmed for whitespace, and the key is set lowercase.
        /// </summary>
        /// <param name="uri"></param>
        /// <param name="argsList"></param>
        /// <param name="explanation"></param>
        /// <returns></returns>
        private static bool ParseUrl(string uri, List<KeyValuePair<string,string>>argsList, out string explanation)
        {
            explanation = "";

            if ( argsList == null )
            {
                explanation = "argsList cannot be null";
                return false;
            }

            argsList.Clear();

            try
            {
                string[] tokens = uri.Split('?');
                if ( tokens.Length <= 1)
                {
                    return true;
                }

                string[] argTokens = tokens[1].Trim().Split('&');
                foreach (string argPair in argTokens.ToList())
                {
                    string[] pairTokens = argPair.Split('=');
                    if ( pairTokens.Length != 2 )
                    {
                        explanation = $"Pair={argPair} is missing '='";
                        return false;
                    }

                    KeyValuePair<string,string> pair = new KeyValuePair<string,string>(pairTokens[0].Trim().ToLower(), pairTokens[1].Trim());
                    argsList.Add(pair);
                }

                return true;
            }
            catch (Exception ex)
            {
                explanation = $"Uri={uri} Err={ex.Message}";
                return false;
            }
        }

        private static void Alert(string msg)
        {
            Console.WriteLine(msg);
        }
    }
}

