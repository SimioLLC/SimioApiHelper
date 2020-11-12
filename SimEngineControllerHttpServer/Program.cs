

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

namespace SimEngineControllerHttpServer
{
    class Program
    {

        public string Mode = "Null";

        static void Main(string[] args)
        {
            //log4net.Config.XmlConfigurator.Configure();

            StartHttpServer(8080);
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
                    Name = "SimEngine Controller",
                    UrlRegex = @"^/SimEngine/RunProject/.*$",
                    Method = "GET",
                    Callable = (HttpRequest request) => {

                        string explanation = "";
                        string success = "";

                        string[] tokens = request.Path.Split('/');
                        if ( tokens.Length != 5 )
                            explanation = $"Expected 5 tokens. Found={tokens.Length}";
                        else
                        {
                            string projectName = WebUtility.UrlDecode(tokens[3]);
                            string arguments = WebUtility.UrlDecode(tokens[4]);

                            string projectPath = Path.Combine(projectsFolder, projectName);
                            if ( !File.Exists(projectPath))
                            {
                                explanation = $"Cannot find Project={projectPath}";
                            }
                            else
                            {
                                string modelName = "Model"; // default
                                string experimentName = "Experiment1"; // default
                                if ( !String.IsNullOrEmpty(arguments))
                                {
                                    string[] argTokens = arguments.Split(',');
                                    if ( argTokens.Length > 0)
                                        modelName = argTokens[0];
                                    if ( argTokens.Length > 1)
                                        experimentName = argTokens[1];
                                }

                                ProjectRequest newRequest = new ProjectRequest(projectPath, "Experiment", "Model,Experiment1", "");
                                if ( !ProjectRequestHelper.PutRequest(queueFolder, newRequest, out explanation))
                                {
                                    explanation = $"Cannot Store Request. Err={explanation}";
                                }
                                else
                                {

                                }

                            }
                        }

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

        private static void Alert(string msg)
        {
            Console.WriteLine(msg);
        }
    }
}

