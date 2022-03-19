using System;
using System.Collections.Generic;
using System.IO;
using SimioAPI;
using System.Threading.Tasks;

namespace SimEngineConsoleOpenLoadSave
{
    /// <summary>
    /// This project demonstrates a very simple Load and Save projet workflow.
    /// Usage: Testing the saving and looking for differences.
    /// In this case, we will save to a different file (i.e. not overwriting)
    /// Assumptions:
    /// Simio is installed under Program Files (not x86)
    /// Command Line Arguments:
    /// 1. Full path to Project file (e.g. spfx)
    /// </summary>
    internal class Program
    {
        static ISimioProject _simioProject;
        static string marker = "Begin.";

        static void Main(string[] args)
        {
            // Open project
            string[] warnings;
            try
            {
                string projectFilenameLoadPath = @"..\..\..\models\ExportingDataFromExperimentsMultiple.spfx"; 
                string extensionsPath = "";
                string programFolder = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);

                extensionsPath = Path.Combine(programFolder, "Simio LLC", "Simio", "UserExtensions");
                Logit($"Starting. Default ExtensionsPath={extensionsPath}.");
                SimioProjectFactory.SetExtensionsPath(extensionsPath);
                Logit($"Info: ExtensionsPath Set successfully.");

                // See if we have command-line arguments (in VS, look under Properties > Debug tab)
                if (args.Length > 0)
                {
                    projectFilenameLoadPath = args[0];
                    marker = $"Info: Project found in CommandLine={projectFilenameLoadPath}";
                    Logit(marker);
                }

                if (!File.Exists(projectFilenameLoadPath))
                {
                    Logit($"Cannot find SimioProject file at={projectFilenameLoadPath}");
                    throw new ApplicationException(marker);
                }
                else
                    Logit($"Info: Located SimioProject file at={projectFilenameLoadPath}");

                string projectFolder = Path.GetDirectoryName(projectFilenameLoadPath);

                // Open/Load the project file. This can take a few seconds (e.g. ClaudesPizza = 8 sec)
                Logit($"Info: Loading Project=[{projectFilenameLoadPath}]");
                _simioProject = SimioProjectFactory.LoadProject(projectFilenameLoadPath, out warnings);

                if (warnings.Length > 0)
                    Logit($"Warning: Project was loaded, but with {warnings.Length} warnings.");
                else
                    Logit($"Info: Project was loaded with no warnings.");

                // Build the save file name (which we will make different than the original
                // by appending '-Saved' to the project name.
                string fileExtension = Path.GetExtension(projectFilenameLoadPath);
                string filename = Path.GetFileNameWithoutExtension(projectFilenameLoadPath) + "-Saved";

                string saveProjectFullpath = Path.Combine(projectFolder, $"{filename}{fileExtension}");

                // Note that if we don't have the 'save' privilege, this will throw exceptions below
                Logit($"Info: Saving Project to={saveProjectFullpath}");
                SimioProjectFactory.SaveProject(_simioProject, saveProjectFullpath, out warnings);

                Logit("End");
                Console.ReadLine();
            }
            catch (Exception ex)
            {
                Logit($"RunError. Marker={marker} Err={ex.Message}");
            }
        }

        private static void Logit(string msg)
        {
            marker = msg;
            string fullMsg = $"{DateTime.Now:HH:mm:ss.ff}: {msg}";
            Console.WriteLine(fullMsg);
            System.Diagnostics.Trace.WriteLine(fullMsg);
        }


    }
}
