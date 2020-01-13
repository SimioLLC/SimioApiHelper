using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.IO;

namespace SimioHelper
{
    public static class DLLHelpers
    {

        public static string GetDllInfo(string filepathForDll)
        {
            try
            {
                AssemblyName aName = AssemblyName.GetAssemblyName(filepathForDll);

                StringBuilder sb = new StringBuilder();
                sb.AppendLine($"FullName={aName.FullName}");
                sb.AppendLine($"Name={aName.Name}");
                sb.AppendLine($"ProcessorArchitecture={aName.ProcessorArchitecture}");
                sb.AppendLine($"Version={aName.Version}");
                sb.AppendLine($"VersionCompatiblity={aName.CodeBase}");
                sb.AppendLine($"Flags={aName.Flags}");

                return sb.ToString();

            }
            catch (Exception ex)
            {
                throw new ApplicationException($"File={filepathForDll} Err={ex}");
            }
        }

        /// <summary>
        /// Round up the usual folders for Simio DLLs,
        /// and return them as a list of paths to those folders.
        /// </summary>
        /// <returns></returns>
        public static List<string> GetSimioApiLocations()
        {
            try
            {
                List<string> locations = new List<string>();

                // User extensions
                {
                    string myDocs = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                    string fullPath = Path.Combine(myDocs, "SimioUserExtensions");

                    if (!Directory.Exists(fullPath))
                        throw new ApplicationException($"Cannot find Path={fullPath}. Check if Simio is installed.");

                    locations.Add(fullPath);
                }

                // public extensions
                {
                    string myDocs = Environment.GetFolderPath(Environment.SpecialFolder.CommonDocuments);
                    string fullPath = Path.Combine(myDocs, "Simio", "Examples", "UserExtensions");

                    if (!Directory.Exists(fullPath))
                        throw new ApplicationException($"Cannot find Path={fullPath}. Check if Simio is installed.");

                    locations.Add(fullPath);
                }

                // Simio installation (Program Files (x86))
                {
                    string simioProgramsx86 = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);
                    string fullPath = Path.Combine(simioProgramsx86, "Simio");
                    locations.Add(fullPath);
                    // Simio UserExtensions underneath Program Files (x86)
                    fullPath = Path.Combine(fullPath, "UserExtensions");
                    locations.Add(fullPath);
                }

                // Simio installation (Program Files)
                {
                    string simioPrograms = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
                    string fullPath = Path.Combine(simioPrograms, "Simio");
                    if ( Directory.Exists(fullPath) && !locations.Contains(fullPath))
                    {
                        locations.Add(fullPath);
                        // Simio UserExtensions underneath Program Files
                        fullPath = Path.Combine(fullPath, "UserExtensions");
                        locations.Add(fullPath);
                    }
                }


                return locations;
            }
            catch (Exception ex)
            {
                throw new ApplicationException($"Err={ex}");
            }
        }
        public static List<string> GetDllFiles(string folderPath)
        {
            try
            {
                if (!Directory.Exists(folderPath))
                    throw new ApplicationException($"Cannot find Path={folderPath}. Check if Simio is installed.");

                string[] files = Directory.GetFiles(folderPath, "*.DLL", SearchOption.AllDirectories);

                return files.ToList();
            }
            catch (Exception ex)
            {
                throw new ApplicationException($"Folder={folderPath} Err={ex}");
            }
        }

        /// <summary>
        /// Extension to fetch loadable types.
        /// Ref:https://stackoverflow.com/questions/7889228/how-to-prevent-reflectiontypeloadexception-when-calling-assembly-gettypes
        /// <param name="assembly"></param>
        /// <returns></returns>
        public static IEnumerable<Type> GetLoadableTypes(this Assembly assembly)
        {
            if (assembly == null) throw new ArgumentNullException(nameof(assembly));
            try
            {
                return assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException e)
            {
                return e.Types.Where(t => t != null);
            }
        }

    }
}
